#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VampireLike
{
    public sealed partial class VerificationRunner
    {
        Action staleBlessingClick;
        WorldSite ShrineAt(Vector2Int key)
        { foreach (var site in game.World.Sites) if (site.type == 2 && site.chunk == key) return site; return null; }
        UIWidget FirstChoice()
        { foreach (Transform child in game.UI.View.upgradeList) if (child.gameObject.activeSelf) return child.GetComponent<UIWidget>(); return null; }
        IEnumerator VerifyBlessingVariety()
        {
            var shown = new HashSet<string>(); int region = 0, offers = 0; bool stacked = false;
            for (int attempt = 0; attempt < 64; attempt++)
            {
                game.UI.ShowHUD(); ClearEnemies();
                WorldSite target = null;
                foreach (var site in game.World.Sites) if (site.type == 2 && !site.complete) { target = site; break; }
                if (target == null)
                {
                    if (region >= 16) break;
                    Place(new Vector3(-240 + region % 4 * 160, 0, -240 + region / 4 * 160)); region++;
                    yield return AutoBattle(1.8f); continue;
                }
                Place(target.position); yield return AutoBattle(.15f); ClearEnemies();
                if (!game.Progression.Shrines.Open(target)) continue;
                var options = ShrineBlessings.Offers(game.Catalog, game.Run.seed, target.chunk);
                bool newDisplay = false; BlessingDefinition selected = options[0];
                foreach (var option in options) { if (shown.Add(option.id)) newDisplay = true; }
                foreach (var option in options) if (game.Run.BlessingLevel(option.id) == 0) { selected = option; break; }
                if (newDisplay)
                    foreach (var size in uiSizes)
                    {
                        Screen.SetResolution(size.x, size.y, false); yield return Wait(.25f);
                        yield return Capture("ui-blessing-variety-" + offers + "-" + size.x + "x" + size.y, size.x, size.y);
                    }
                int before = game.Run.BlessingLevel(selected.id);
                foreach (Transform child in game.UI.View.upgradeList)
                {
                    var card = child.GetComponent<UIWidget>();
                    if (card != null && child.gameObject.activeSelf && card.title.text == selected.name) { card.button.onClick.Invoke(); break; }
                }
                Check(game.Run.BlessingLevel(selected.id) == before + 1, "Discovered shrine grants real repeated or new blessing " + selected.id + " " + (before + 1));
                stacked |= game.Run.BlessingLevel(selected.id) >= 2; offers++;
                yield return AutoBattle(.25f);
                if (shown.Count == 6 && game.Run.blessings.Count == 6 && stacked) break;
            }
            Check(shown.Count == 6 && game.Run.blessings.Count == 6, "Real streamed shrine offers display and grant all six blessing kinds");
            Check(stacked, "Different real shrine sites can stack the same blessing beyond one layer");
            game.UI.ShowHUD(); game.UI.ToggleSkills(); yield return Wait(.25f);
            foreach (var size in uiSizes)
            {
                Screen.SetResolution(size.x, size.y, false); yield return Wait(.25f);
                yield return Capture("ui-blessing-owned-" + size.x + "x" + size.y, size.x, size.y);
            }
            game.UI.CloseModal();
            Place(new Vector3(0, 0, 32)); yield return AutoBattle(2.5f);
        }
        IEnumerator VerifyExploration()
        {
            ClearEnemies(); game.Inputs.ForVerification(Vector2.zero); game.UI.ShowHUD();
            var key = new Vector2Int(0, 1); var site = ShrineAt(key);
            Check(site != null, "Initial world provides an authored shrine event"); if (site == null) yield break;
            Place(site.position); yield return null;
            int crystals = game.Save.Profile.crystals, events = game.Save.Profile.events;
            game.Inputs.InteractForVerification(); yield return null; yield return null;
            Check(game.UI.ActiveModal == "blessing" && game.IsPaused, "Programmatic E opens a paused shrine choice modal");
            if (game.UI.ActiveModal != "blessing") yield break;
            var initialCard = FirstChoice(); string firstName = initialCard.title.text;
            Action cancelledClick = initialCard.button.onClick.Invoke;
            game.UI.CloseModal(); cancelledClick();
            Check(!site.complete && game.Run.blessings.Count == 0 && game.Save.Profile.crystals == crystals && game.Save.Profile.events == events, "Cancel and cancelled card callback grant no blessing currency or event completion");
            Check(game.Progression.Shrines.Open(site) && FirstChoice().title.text == firstName, "Reopening a shrine retains its deterministic offer");
            Place(site.position + Vector3.right * 10); FirstChoice().button.onClick.Invoke();
            Check(!site.complete && game.Run.blessings.Count == 0, "Shrine selection revalidates player interaction distance");
            game.UI.CloseModal(); Place(new Vector3(160, 0, 32)); yield return AutoBattle(2.5f);
            Check(!game.World.IsActiveSite(site) && !game.Progression.Shrines.Open(site), "Unloaded shrine references cannot open a new offer");
            Place(new Vector3(0, 0, 32)); yield return AutoBattle(2.5f); ClearEnemies();
            site = ShrineAt(key);
            Check(site != null && !site.complete && game.Progression.Shrines.Open(site), "Unclaimed shrine survives chunk unload and revisit");
            if (site == null || game.UI.ActiveModal != "blessing") yield break;
            Check(FirstChoice().title.text == firstName, "Rebuilt shrine preserves the same first choice");
            foreach (var size in uiSizes)
            {
                Screen.SetResolution(size.x, size.y, false); yield return Wait(.25f);
                yield return Capture("ui-blessing-" + size.x + "x" + size.y, size.x, size.y);
            }
            var chosen = FirstChoice();
            var definition = Array.Find(game.Catalog.blessings, b => b.name == chosen.title.text);
            int rerolls = game.Run.rerolls;
            float beforeEffect = game.Run.Effect(game.Catalog, definition.effect, AbilityKind.Gun);
            crystals = game.Save.Profile.crystals; events = game.Save.Profile.events;
            staleBlessingClick = chosen.button.onClick.Invoke;
            // A level earned while the event is open must remain queued behind it.
            game.Progression.Experience(game.Run.nextExperience);
            Check(game.UI.ActiveModal == "blessing", "Queued XP does not replace an active shrine offer");
            staleBlessingClick();
            Check(site.complete && game.Run.BlessingLevel(definition.id) == 1 && game.Save.Profile.crystals == crystals + 4 && game.Save.Profile.events == events + 1, "Choosing a shrine grants its blessing and base reward exactly once");
            Check(Mathf.Approximately(game.Run.Effect(game.Catalog, definition.effect, AbilityKind.Gun), beforeEffect + definition.value) && game.Run.rerolls == rerolls + definition.rerolls, "Selected blessing immediately contributes its real effect or reroll");
            int points = game.Run.skillPoints, experience = game.Run.experience, level = game.Run.level;
            staleBlessingClick();
            Check(game.Run.BlessingLevel(definition.id) == 1 && game.Save.Profile.crystals == crystals + 4 && game.Run.skillPoints == points && game.Run.experience == experience && game.Run.level == level && game.Save.Profile.events == events + 1, "Repeated shrine card invocation cannot duplicate rewards XP or completion");
            yield return null; yield return null;
            Check(game.UI.ActiveModal == "upgrade" && game.UI.View.upgrade.transform.Find("RerollButton").gameObject.activeSelf, "Pending level resumes with the regular upgrade controls after blessing selection");
            for (int i = 0; i < 8 && game.UI.ActiveModal == "upgrade"; i++)
            { var options = game.Progression.Choices(); if (options.Count > 0) game.Progression.Apply(options[0].id, false); yield return null; yield return null; }
            yield return VerifyBlessingVariety();
            Place(new Vector3(160, 0, 32)); yield return AutoBattle(2.5f);
            Place(new Vector3(0, 0, 32)); yield return AutoBattle(2.5f);
            var rebuilt = ShrineAt(key);
            Check(rebuilt != null && rebuilt.complete && !game.Progression.Shrines.Open(rebuilt) && !game.World.CompleteEvent(rebuilt.id), "Claimed shrine remains completed and rejects a second claim after chunk reload");
            Place(Vector3.zero); ClearEnemies();
        }
    }
}
#endif
