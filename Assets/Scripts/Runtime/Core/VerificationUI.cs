#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed partial class VerificationRunner
    {
        static readonly Vector2Int[] uiSizes={new Vector2Int(1280,720),new Vector2Int(1920,1080),new Vector2Int(1920,1200),new Vector2Int(2560,1080),new Vector2Int(2560,1440)};
        bool auditLayout;
        [System.Serializable] sealed class LayoutAudit {public int width,height,textCount;public List<string> clipped=new List<string>();}
        [System.Serializable] sealed class SelectionCycleAudit {public int cycle;public string openedModal,closedModal;public bool characterRefreshed,menuVisible,renderersValid;}
        void AuditLayout(string path,int width,int height)
        {
            var result=new LayoutAudit{width=width,height=height};
            foreach(var text in game.UI.View.GetComponentsInChildren<Text>())
            {
                if(!text.isActiveAndEnabled||string.IsNullOrEmpty(text.text)||text.rectTransform.rect.width<1)continue;
                result.textCount++;float expected=text.preferredHeight,available=text.rectTransform.rect.height;
                if(expected>available+2)result.clipped.Add(text.name+": "+text.text.Replace('\n',' ') +" (needs "+expected.ToString("F1")+", has "+available.ToString("F1")+")");
            }
            File.WriteAllText(Path.ChangeExtension(path,"layout.json"),JsonUtility.ToJson(result,true));
            Check(result.clipped.Count==0,"UI text fits "+Path.GetFileNameWithoutExtension(path)+(result.clipped.Count>0?": "+result.clipped[0]:""));
        }
        IEnumerator UIMatrix()
        {
            yield return SelectionRendererRegression();
            auditLayout=true;
            foreach(var size in uiSizes)
            {
                Screen.SetResolution(size.x,size.y,false);yield return Wait(.25f);
                game.UI.ShowMenu();yield return Wait(.2f);yield return Capture("ui-menu-"+size.x+"x"+size.y,size.x,size.y);
                game.UI.ShowSelection();yield return Wait(.2f);yield return Capture("ui-selection-locked-"+size.x+"x"+size.y,size.x,size.y);game.UI.CloseModal();
                game.UI.ShowSelection();yield return Wait(.2f);
                game.UI.View.biomeList.GetComponentInParent<ScrollRect>().verticalNormalizedPosition=0;yield return Wait(.2f);
                yield return Capture("ui-selection-expansion-locked-"+size.x+"x"+size.y,size.x,size.y);game.UI.CloseModal();
                game.UI.ShowProgress();yield return Wait(.2f);yield return Capture("ui-progress-"+size.x+"x"+size.y,size.x,size.y);
                yield return ExpansionTaskPages(size);game.UI.CloseModal();
                game.UI.ShowSettings();yield return Wait(.2f);yield return Capture("ui-settings-"+size.x+"x"+size.y,size.x,size.y);game.UI.CloseModal();
                game.UI.ShowLoading("正在展开新的世界…");game.UI.LoadingProgress(.55f);yield return Wait(.2f);yield return Capture("ui-loading-"+size.x+"x"+size.y,size.x,size.y);game.UI.ShowMenu();
            }
            foreach(var task in game.Catalog.tasks)game.Save.Profile.completedTasks.Add(task.id);
            for(int i=0;i<4;i++)game.Save.Profile.bossWins[i]=true;
            game.UI.ShowSelection();
            foreach(var size in uiSizes){Screen.SetResolution(size.x,size.y,false);yield return Wait(.25f);yield return Capture("ui-selection-unlocked-"+size.x+"x"+size.y,size.x,size.y);}
            foreach(var size in uiSizes)
            {
                Screen.SetResolution(size.x,size.y,false);yield return Wait(.25f);
                game.UI.View.biomeList.GetComponentInParent<ScrollRect>().verticalNormalizedPosition=0;yield return Wait(.2f);
                yield return Capture("ui-selection-expansion-unlocked-"+size.x+"x"+size.y,size.x,size.y);
            }
            game.BeginRun("adventurer",BiomeId.Forest);float deadline=Time.realtimeSinceStartup+90;
            while(!game.Ready&&Time.realtimeSinceStartup<deadline)yield return null;
            Check(game.Ready,"UI matrix enters real gameplay");if(!game.Ready)yield break;
            yield return VerifyExploration();
            foreach(var u in game.Catalog.upgrades)if(u.effect==UpgradeEffect.NewAbility&&game.Progression.Eligible(u))game.Progression.GrantForDiagnostic(u.id);
            Check(game.Run.abilities.Count==9,"HUD and skill tree retain all nine acquired weapons");
            game.Run.skillPoints=15;
            foreach(var size in uiSizes)
            {
                Screen.SetResolution(size.x,size.y,false);yield return Wait(.25f);ClearEnemies();game.Player.Heal(1000);
                game.UI.ShowHUD();yield return Capture("ui-hud-"+size.x+"x"+size.y,size.x,size.y);
                game.UI.View.abilityList.GetComponentInParent<ScrollRect>().horizontalNormalizedPosition=1;yield return Wait(.2f);
                yield return Capture("ui-hud-expansion-"+size.x+"x"+size.y,size.x,size.y);
                game.UI.ToggleSkills();yield return Wait(.2f);yield return Capture("ui-skills-"+size.x+"x"+size.y,size.x,size.y);game.UI.CloseModal();
                yield return ExpansionSkillPages(size,false);
                foreach(var task in game.Catalog.tasks)if(task.id=="boomerang"||task.id=="froststaff"||task.id.Contains("."))game.Save.Profile.completedTasks.Remove(task.id);
                yield return ExpansionSkillPages(size,true);
                foreach(var task in game.Catalog.tasks)if(!game.Save.Profile.completedTasks.Contains(task.id))game.Save.Profile.completedTasks.Add(task.id);
                game.UI.ToggleMap();yield return Wait(.2f);yield return Capture("ui-map-"+size.x+"x"+size.y,size.x,size.y);game.UI.CloseModal();
                game.UI.TogglePause();yield return Wait(.2f);yield return Capture("ui-pause-"+size.x+"x"+size.y,size.x,size.y);
                game.UI.ShowSettings();yield return Wait(.2f);yield return Capture("ui-pause-settings-"+size.x+"x"+size.y,size.x,size.y);game.UI.CloseModal();game.UI.CloseModal();
                game.Progression.Experience(game.Run.nextExperience);game.UI.ShowUpgrade(game.Progression.Choices());yield return Wait(.2f);yield return Capture("ui-upgrade-"+size.x+"x"+size.y,size.x,size.y);
                var options=game.Progression.Choices();if(options.Count>0)game.Progression.Apply(options[0].id,false);
            }
            game.Combat.ActivateBoss("ui-matrix-victory",game.Player.transform.position+Vector3.forward*8,BiomeId.Forest,game.Catalog.biomes[0].bossId);
            game.Combat.Boss?.Hit(new DamagePacket{damage=10000000,source=AbilityKind.Gun});yield return Wait(1.25f);
            foreach(var size in uiSizes){Screen.SetResolution(size.x,size.y,false);yield return Wait(.25f);yield return Capture("ui-victory-"+size.x+"x"+size.y,size.x,size.y);}
            game.UI.CloseModal();game.FinishRun(true);
            foreach(var size in uiSizes){Screen.SetResolution(size.x,size.y,false);yield return Wait(.25f);yield return Capture("ui-results-"+size.x+"x"+size.y,size.x,size.y);}
            game.ReturnToMenu();yield return Wait(2);Check(game.CanBeginRun,"UI matrix returns to menu without a stuck modal pause");
            game.BeginRun("adventurer", BiomeId.Forest); deadline = Time.realtimeSinceStartup + 90;
            while (!game.Ready && Time.realtimeSinceStartup < deadline) yield return null;
            Check(game.Ready && game.Run.blessings.Count == 0, "New gameplay run clears all previous shrine blessings");
            if (game.Ready)
            {
                int crystals = game.Save.Profile.crystals; staleBlessingClick?.Invoke();
                Check(game.Run.blessings.Count == 0 && game.Save.Profile.crystals == crystals, "Old run shrine UI callback cannot grant into a new run");
                game.Progression.GrantForDiagnostic("regen");
                Check(game.Run.Effect(game.Catalog, UpgradeEffect.Regeneration) > 0, "Lethal damage regression runs with automatic regeneration active");
                game.Player.Hurt(1000000);
                Check(game.Flow == GameFlow.Settling && game.Run.settled && game.Player.Health == 0, "Lethal damage immediately requests the unique GameManager death settlement");
                game.Player.SendMessage("Update");
                Check(game.Player.Health == 0 && game.Run.settled, "An automatic-regeneration update cannot revive lethal damage before settlement");
                int settledCount = game.Save.Profile.settledRuns.Count;
                game.Player.Hurt(1000000); game.FinishRun(false);
                Check(game.Save.Profile.settledRuns.Count == settledCount, "Repeated lethal damage and death requests do not duplicate settlement");
                yield return Wait(.7f); game.ReturnToMenu(); yield return Wait(2);
            }
        }
        static bool GraphicsHaveRenderers(Transform root,bool checkCachedReference)
        {
            var graphics=root.GetComponentsInChildren<Graphic>(true);
            if(graphics.Length==0)return false;
            foreach(var graphic in graphics)
            {
                var renderer=graphic.GetComponent<CanvasRenderer>();
                if(renderer==null||(checkCachedReference&&graphic.canvasRenderer!=renderer))return false;
            }
            return true;
        }
        static Button FindUIProbeButton(Transform root,string name)
        {
            foreach(var button in root.GetComponentsInChildren<Button>(true))if(button.name==name)return button;
            return null;
        }
        IEnumerator SelectionRendererRegression()
        {
            game.UI.ShowMenu();
            Check(GraphicsHaveRenderers(game.UI.View.transform,false),"Inactive UI templates and maps already contain CanvasRenderer without lazy Graphic repair");
            var play=FindUIProbeButton(game.UI.View.menu.transform,"PlayButton");
            var close=FindUIProbeButton(game.UI.View.selection.transform,"SelectionClose");
            Check(play!=null&&close!=null,"Selection regression uses the bound PlayButton and SelectionClose buttons");
            if(play==null||close==null)yield break;
            bool transitions=true,renderers=true;int refreshed=0;
            for(int cycle=0;cycle<8;cycle++)
            {
                var observation=new SelectionCycleAudit{cycle=cycle+1};
                play.onClick.Invoke();transitions&=game.UI.ActiveModal=="selection";
                observation.openedModal=game.UI.ActiveModal;
                // Selecting the current unlocked character rebuilds both lists and
                // disables old masked graphics before their deferred destruction.
                foreach(var widget in game.UI.View.characterList.GetComponentsInChildren<UIWidget>())
                    if(widget.button.interactable){widget.button.onClick.Invoke();refreshed++;observation.characterRefreshed=true;break;}
                foreach(var scroll in game.UI.View.selection.GetComponentsInChildren<ScrollRect>())scroll.verticalNormalizedPosition=cycle%2;
                Canvas.ForceUpdateCanvases();
                foreach(var mask in game.UI.View.selection.GetComponentsInChildren<RectMask2D>())mask.PerformClipping();
                observation.renderersValid=GraphicsHaveRenderers(game.UI.View.transform,true);
                yield return null;
                close.onClick.Invoke();Canvas.ForceUpdateCanvases();
                observation.closedModal=game.UI.ActiveModal;observation.menuVisible=game.UI.View.menu.activeSelf;
                transitions&=observation.closedModal=="menu"&&observation.menuVisible;
                yield return null;
                observation.renderersValid&=GraphicsHaveRenderers(game.UI.View.transform,true);renderers&=observation.renderersValid;
                File.AppendAllText(Path.Combine(output,"SelectionRendererCycles.jsonl"),JsonUtility.ToJson(observation)+"\n");
            }
            Check(transitions&&refreshed==8,"Eight real Play/select/close cycles rebuild locked lists and return to menu");
            Check(renderers,"Clipping, disabling and destroying selection graphics retain valid CanvasRenderer references");
        }
        IEnumerator ExpansionTaskPages(Vector2Int size)
        {
            foreach(var task in game.Catalog.tasks)
            {
                if(task.id!="boomerang"&&task.id!="froststaff"&&!task.id.Contains("."))continue;
                foreach(var widget in game.UI.View.taskList.GetComponentsInChildren<UIWidget>())
                {
                    if(!widget.title.text.Contains(task.name))continue;
                    Canvas.ForceUpdateCanvases();var list=(RectTransform)game.UI.View.taskList;var item=(RectTransform)widget.transform;
                    var scroll=list.GetComponentInParent<ScrollRect>();float top=-item.anchoredPosition.y-item.rect.height*.5f;
                    list.anchoredPosition=new Vector2(list.anchoredPosition.x,Mathf.Clamp(top,0,Mathf.Max(0,list.rect.height-scroll.viewport.rect.height)));
                    yield return Wait(.15f);yield return Capture("ui-task-expansion-"+task.id+"-"+size.x+"x"+size.y,size.x,size.y);break;
                }
            }
        }
        IEnumerator ExpansionSkillPages(Vector2Int size,bool locked)
        {
            game.UI.ToggleSkills();yield return Wait(.15f);
            foreach(string branch in new[]{"弹道","剑修","元素","联动"})
            {
                foreach(var button in game.UI.View.skills.GetComponentsInChildren<Button>(true))if(button.name=="Filter"+branch){button.onClick.Invoke();break;}
                yield return Wait(.15f);Canvas.ForceUpdateCanvases();
                game.UI.View.skillList.GetComponentInParent<ScrollRect>().verticalNormalizedPosition=0;
                yield return Wait(.15f);yield return Capture("ui-skills-expansion-"+branch+"-"+(locked?"locked":"unlocked")+"-"+size.x+"x"+size.y,size.x,size.y);
            }
            foreach(var button in game.UI.View.skills.GetComponentsInChildren<Button>(true))if(button.name=="Filter全部"){button.onClick.Invoke();break;}
            game.UI.CloseModal();
        }
        IEnumerator EffectGallery()
        {
            ClearEnemies();
            foreach(string id in new[]{"flame","explosion","frost","lightning","rune","slash","arc","leafburst","sand","iceburst","embers","spore","crystalburst"})
            {
                Vector3 point=game.Player.transform.position+Vector3.forward*3;
                if(id=="arc")game.Combat.Chain(point+Vector3.left*2,point+Vector3.right*2);
                else game.Combat.Effect("vfx/"+id,point,1.0f,2);
                yield return RealtimeSequence("effect-"+id,1.15f);
            }
        }
    }
}

#endif
