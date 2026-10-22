using System.Collections.Generic;
using UnityEngine;

namespace VampireLike
{
    // Owns one pending offer for this run. UI callbacks must present its exact token.
    public sealed class ShrineBlessings
    {
        readonly GameManager game;
        readonly RunState run;
        WorldSite pending;
        int token;
        List<BlessingDefinition> choices;
        public ShrineBlessings(GameManager game) { this.game = game; run = game.Run; }
        public static List<BlessingDefinition> Offers(GameCatalog catalog, int seed, Vector2Int chunk)
        {
            var options = new List<BlessingDefinition>(catalog.blessings);
            var random = new System.Random(WorldService.SeedHash(seed ^ 0x37ab, chunk.x, chunk.y));
            for (int i = options.Count - 1; i > 0; i--)
            { int j = random.Next(i + 1); var swap = options[i]; options[i] = options[j]; options[j] = swap; }
            if (options.Count > 3) options.RemoveRange(3, options.Count - 3);
            return options;
        }
        bool Valid(WorldSite site) => game.Ready && game.Run == run && !run.settled && site != null &&
            site.type == 2 && !site.complete && game.World.IsActiveSite(site) &&
            Vector3.Distance(game.Player.transform.position, site.position) < 4.8f;
        public bool Open(WorldSite site)
        {
            if (game.IsPaused || !Valid(site)) return false;
            pending = site; choices = Offers(game.Catalog, run.seed, site.chunk);
            game.UI.ShowBlessings(choices, ++token); return true;
        }
        public void Cancel() { pending = null; choices = null; token++; }
        public bool Select(int offerToken, string id)
        {
            if (offerToken != token || game.UI.ActiveModal != "blessing" || !Valid(pending) ||
                choices == null || !choices.Exists(b => b.id == id)) return false;
            var site = pending;
            var blessing = game.Catalog.Blessing(id);
            // Consume before any effects, XP, save, or UI callbacks can re-enter.
            Cancel();
            if (blessing == null || !game.World.CompleteEvent(site.id)) return false;
            run.blessings[id] = run.BlessingLevel(id) + 1;
            run.rerolls += blessing.rerolls;
            game.Player.RefreshStats(true); game.Abilities.Refresh();
            game.Player.Heal(game.Player.MaxHealth * .45f);
            game.Progression.Reward(3, 4); game.Progression.Experience(18);
            game.Save.Flush(); game.UI.CloseUpgrade();
            game.Audio.Play(SoundCue.Unlock); game.Combat.Effect("vfx/rune", site.position, .8f, 4);
            game.UI.Toast("祭坛 · " + blessing.name + " ×" + run.BlessingLevel(id) + "\n生命恢复 · 定向点 +3 · 晶石 +4");
            return true;
        }
    }
}
