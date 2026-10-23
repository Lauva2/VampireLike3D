#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed partial class VerificationRunner
    {
        [Serializable] sealed class PopupFrame
        {
            public string popup;
            public int frame;
            public float wallSeconds, alpha, scale, y, timeScale;
            public bool interactable, blocksRaycasts;
        }
        IEnumerator ObservePopupOpening(string name, GameObject panel, Action open)
        {
            open();
            var group = panel.GetComponent<CanvasGroup>();
            var surface = (RectTransform)panel.transform.GetChild(0);
            bool paused = game.Ready;
            float gameSeconds = paused ? game.Run.seconds : 0;
            Check(panel.activeSelf && group != null && group.alpha == 0 && !group.interactable && group.blocksRaycasts,
                "Popup opening starts transparent with guarded controls and backdrop: " + name);
            if (group == null) yield break;
            Vector2 startPosition = surface.anchoredPosition;
            Vector3 startScale = surface.localScale;
            string folder = Path.Combine(output, "Sequences", "popup-" + name);
            Directory.CreateDirectory(folder);
            float started = Time.realtimeSinceStartup, previousAlpha = 0;
            bool monotonic = true; int frame = 0;
            while (Time.realtimeSinceStartup - started < .55f)
            {
                monotonic &= group.alpha >= previousAlpha && group.alpha <= 1;
                previousAlpha = group.alpha;
                var sample = new PopupFrame { popup = name, frame = frame, wallSeconds = Time.realtimeSinceStartup - started,
                    alpha = group.alpha, scale = surface.localScale.x, y = surface.anchoredPosition.y, timeScale = Time.timeScale,
                    interactable = group.interactable, blocksRaycasts = group.blocksRaycasts };
                File.AppendAllText(Path.Combine(folder, "motion.jsonl"), JsonUtility.ToJson(sample) + "\n");
                RenderCapture(Path.Combine(folder, (frame++).ToString("D4") + ".jpg"), 1280, 720, true);
                yield return new WaitForSecondsRealtime(1f / 30);
            }
            Check(monotonic && group.alpha == 1 && group.interactable && group.blocksRaycasts,
                "Popup opening completes monotonically and enables controls: " + name);
            Check(surface.localScale.x > startScale.x && surface.anchoredPosition != startPosition,
                "Popup content arrives with labels and controls, not only the sheet: " + name);
            if (paused) Check(game.IsPaused && Time.timeScale == 0 && Mathf.Approximately(game.Run.seconds, gameSeconds),
                "Popup animates in unscaled time without advancing gameplay: " + name);
            File.WriteAllText(Path.Combine(folder, "metadata.json"), "{\"source\":\"actual background Player Camera.Render\",\"frames\":" + frame
                + ",\"requestedIntervalSeconds\":0.03333,\"fixedSimulationFps\":null,\"wallClockTimestamps\":true}");
        }
        IEnumerator PopupMenuRegression()
        {
            var ui = game.UI; var view = ui.View;
            GameObject[] panels = { view.menu, view.selection, view.progress, view.settings, view.loading };
            Action[] open = { ui.ShowMenu, ui.ShowSelection, ui.ShowProgress, ui.ShowSettings, () => ui.ShowLoading("正在展开新的世界…") };
            string[] names = { "menu", "selection", "progress", "settings", "loading" };
            for (int i = 0; i < panels.Length; i++)
            {
                ui.ShowHUD(); yield return ObservePopupOpening(names[i], panels[i], open[i]);
            }
            ui.ShowMenu(); yield return Wait(.32f);
            ui.ShowSelection();
            var close = FindUIProbeButton(view.selection.transform, "SelectionClose");
            Canvas.ForceUpdateCanvases();
            Check(!close.IsInteractable(), "Opening selection blocks pointer activation until its content is ready");
            ExecuteEvents.Execute(close.gameObject, new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
            Check(ui.ActiveModal == "selection", "An opening-frame pointer click cannot dismiss a fresh popup or click through it");
            yield return Wait(.06f);
            var group = view.selection.GetComponent<CanvasGroup>();
            float alpha = group.alpha; var title = view.selection.transform.Find("SelectionTitle"); var scale = title.localScale;
            ui.ShowSelection();
            Check(Mathf.Approximately(alpha, group.alpha) && title.localScale == scale,
                "Selection content refresh preserves the current opening progress without a flash");
            yield return Wait(.32f);
            ExecuteEvents.Execute(close.gameObject, new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
            Check(ui.ActiveModal == "menu", "A settled popup accepts the same pointer click normally");
            ui.ShowSettings(); yield return null; ui.CloseModal(); ui.ShowSelection(); yield return Wait(.32f);
            Check(!view.settings.activeSelf && view.settings.GetComponent<CanvasGroup>().alpha == 1
                && view.settings.transform.GetChild(0).localScale == Vector3.one && group.alpha == 1 && group.interactable,
                "Interrupted popup restores its pose and cannot later overwrite a newer popup");
            ui.ShowSettings(); yield return null; ui.enabled = false;
            Check(view.settings.GetComponent<CanvasGroup>().alpha == 1 && view.settings.transform.GetChild(0).localScale == Vector3.one,
                "Disabling UI motion restores a visible interactive resting pose");
            ui.enabled = true; ui.CloseModal(); yield return Wait(.32f);
        }
        IEnumerator PopupGameplayRegression()
        {
            var ui = game.UI; var view = ui.View;
            ui.ShowHUD(); yield return ObservePopupOpening("skills", view.skills, ui.ToggleSkills); ui.CloseModal();
            yield return ObservePopupOpening("map", view.map, ui.ToggleMap); ui.CloseModal();
            yield return ObservePopupOpening("pause", view.pause, ui.TogglePause);
            yield return ObservePopupOpening("pause-settings", view.settings, ui.ShowSettings);
            yield return ObservePopupOpening("settings-return-pause", view.pause, ui.CloseModal); ui.CloseModal();
            Check(!game.IsPaused && ui.ActiveModal == null, "Closing nested animated pause settings releases only the UI pause");
            game.Progression.Experience(game.Run.nextExperience);
            yield return ObservePopupOpening("upgrade", view.upgrade, () => ui.ShowUpgrade(game.Progression.Choices()));
            game.Progression.SkipLevel();
            yield return ObservePopupOpening("victory", view.victory, () => ui.ShowBossVictory(BiomeId.Forest)); ui.CloseModal();
            yield return ObservePopupOpening("results", view.results, () => ui.ShowResults(true)); ui.ShowHUD();
            ui.Toast("新旅途 · 九种力量自由交织", 2);
            var toast = view.transform.Find("Toast"); var group = toast.GetComponent<CanvasGroup>();
            Check(group.alpha == 0 && !group.interactable && !group.blocksRaycasts, "Toast opens softly without intercepting gameplay input");
            yield return RealtimeSequence("popup-toast", .55f);
            Check(group.alpha == 1 && !group.blocksRaycasts, "Toast opening completes while retaining input transparency");
            ui.Toast("连续提示保持当前动效", .1f);
            Check(group.alpha == 1, "Refreshing a visible toast does not flash back to transparent");
            yield return Wait(.2f);
            Check(!toast.gameObject.activeSelf && toast.localScale == Vector3.one, "Expired toast restores its pose for the next opening");
            ui.ShowHUD();
        }
    }
}
#endif
