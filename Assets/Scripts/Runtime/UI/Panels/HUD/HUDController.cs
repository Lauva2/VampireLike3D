using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class HUDController : UIController<HUDView, HUDModel>
    {
        public HUDController(UIManager manager, GameManager game, HUDModel model, HUDView view) : base(manager, game, model, view) { }
        public override void Bind()
        {
            On("HUDSkills", manager.ToggleSkills); On("HUDMap", manager.ToggleMap); On("HUDPause", manager.TogglePause);
        }
        public override void Render()
        {
            Model.nextRefresh = 0; Tick();
        }
        readonly Dictionary<AbilityKind, UIWidget> abilityWidgets = new Dictionary<AbilityKind, UIWidget>();
        public override void Tick()
        {
            if (!game.Ready || Time.unscaledTime < Model.nextRefresh) return;
            Model.nextRefresh = Time.unscaledTime + .1f; Model.Refresh(game); View.Present(Model);
            var run = game.Run;
            foreach (var pair in abilityWidgets) pair.Value.gameObject.SetActive(run.abilities.Contains(pair.Key));
            foreach (var kind in run.abilities)
            {
                if (!abilityWidgets.TryGetValue(kind, out var badge))
                {
                    badge = UnityEngine.Object.Instantiate(View.abilityTemplate, View.abilityList, false); abilityWidgets.Add(kind, badge);
                    badge.title.text = game.Catalog.Ability(kind).name; badge.symbol.Set(Icon(kind));
                    badge.accent.color = game.Catalog.Ability(kind).color;
                }
                badge.gameObject.SetActive(true);
            }
        }
    }
}
