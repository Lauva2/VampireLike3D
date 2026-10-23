using System.Collections.Generic;
using UnityEngine;

namespace VampireLike
{
    internal sealed class SpiritAttack : WeaponAttack
    {
        enum Phase { Follow, Windup, Dash, Return }
        sealed class Familiar
        {
            public GameObject view;
            public Phase phase;
            public EnemyActor target;
            public long life;
            public float queuedWeight, activeWeight, until;
            public Vector3 began;
            public DamagePacket packet;
        }
        readonly List<Familiar> familiars = new List<Familiar>();
        readonly List<EnemyActor> chainTargets = new List<EnemyActor>();
        public int ModelCount => familiars.Count;
        public int LogicalCount => Count;
        public float PendingWeight { get { float value = 0; foreach (var f in familiars) value += f.queuedWeight + f.activeWeight; return value; } }
        internal float EmittedStrikePower { get; private set; }
        internal Vector3 FirstPosition => familiars.Count > 0 ? familiars[0].view.transform.position : Position;
        float SpeedFactor => Mathf.Max(.1f, 1 + Effect(UpgradeEffect.AttackSpeed) + Effect(UpgradeEffect.SummonSpeed));
        public SpiritAttack(GameManager game, AbilityController owner) : base(game, owner, AbilityKind.Spirit) { }
        public override void Refresh()
        {
            if (Disposed) return;
            int visible = Mathf.Min(8, Count);
            while (familiars.Count < visible)
            {
                var view = familiars.Count == 0 ? Visual : game.Pool.Rent(Definition.prefabId, Position, Quaternion.identity);
                if (view == null) break;
                // A familiar flies in world space; player turning must not rotate an ongoing dash around the player.
                view.transform.SetParent(owner.transform.parent, true);
                var familiar = new Familiar { view = view }; familiars.Add(familiar);
                view.transform.position = Home(familiars.Count - 1);
            }
            // Acquired counts only grow during normal play; dispose surplus diagnostic views safely.
            while (familiars.Count > visible)
            {
                var last = familiars[familiars.Count - 1];
                familiars[0].queuedWeight += last.queuedWeight + last.activeWeight;
                game.Pool.Return(last.view); familiars.RemoveAt(familiars.Count - 1);
            }
        }
        Vector3 Home(int index)
        {
            float angle = Time.time * .65f + index * Mathf.PI * 2 / Mathf.Max(1, familiars.Count);
            return Position + new Vector3(Mathf.Sin(angle) * 1.6f, 1.45f + Mathf.Sin(Time.time * 3 + index) * .12f, Mathf.Cos(angle) * 1.6f);
        }
        public override void Cast(EnemyActor target)
        {
            if (Disposed || target == null) return;
            Refresh();
            foreach (var f in familiars)
            {
                f.queuedWeight += (float)Count / familiars.Count;
                if (f.phase == Phase.Follow || f.phase == Phase.Return) { f.target = target; f.life = target.CollisionId; }
            }
        }
        protected override void Fire(EnemyActor target, float weight) { }
        EnemyActor Resolve(Familiar f)
        {
            float range = Definition.range + Effect(UpgradeEffect.Range);
            if (f.target != null && f.target.CollisionId == f.life && f.target.CanBeTargeted &&
                (f.target.transform.position - Position).sqrMagnitude <= range * range && game.Combat.Visible(Position, f.target)) return f.target;
            f.target = game.Combat.Target(Position, range); f.life = f.target != null ? f.target.CollisionId : 0;
            return f.target;
        }
        public override void Tick()
        {
            if (Disposed || !game.Ready || game.IsPaused) return;
            Refresh();
            for (int i = 0; i < familiars.Count; i++)
            {
                var f = familiars[i]; var transform = f.view.transform; Vector3 home = Home(i);
                if (f.phase == Phase.Follow)
                {
                    transform.position = Vector3.MoveTowards(transform.position, home, (game.Player.Speed + 8) * Time.deltaTime);
                    transform.rotation = Quaternion.Euler(0, Time.time * 25 + i * 45, Mathf.Sin(Time.time * 4 + i) * 7);
                    transform.localScale = Vector3.one * .9f;
                    if (f.queuedWeight > 0)
                    {
                        if (Resolve(f) == null) { f.queuedWeight = 0; continue; }
                        f.activeWeight = f.queuedWeight; f.queuedWeight = 0;
                        f.packet = owner.Packet(kind, f.activeWeight); f.began = transform.position;
                        f.until = Time.time + .14f / Mathf.Sqrt(SpeedFactor); f.phase = Phase.Windup;
                    }
                }
                else if (f.phase == Phase.Windup)
                {
                    transform.position = f.began + Vector3.up * Mathf.Sin(Time.time * 18) * .08f;
                    transform.localScale = Vector3.one * 1.12f;
                    if (Time.time >= f.until) f.phase = Phase.Dash;
                }
                else if (f.phase == Phase.Dash)
                {
                    var target = Resolve(f);
                    if (target == null) { f.activeWeight = 0; f.phase = Phase.Return; continue; }
                    Vector3 destination = target.transform.position + Vector3.up * .8f;
                    Vector3 delta = destination - transform.position;
                    Vector3 next = Vector3.MoveTowards(transform.position, destination, Definition.speed * Mathf.Sqrt(SpeedFactor) * Time.deltaTime);
                    transform.rotation = Quaternion.LookRotation(delta.sqrMagnitude > .001f ? delta : Vector3.forward);
                    transform.localScale = new Vector3(.85f, .8f, 1.25f);
                    if (Physics.Linecast(transform.position, next, (1 << 10) | 1)) { f.activeWeight = 0; f.phase = Phase.Return; continue; }
                    transform.position = next;
                    if ((destination - next).sqrMagnitude < .4f)
                    {
                        target.Hit(f.packet); EmittedStrikePower += f.packet.damage;
                        game.Combat.Effect("vfx/impact", destination, .2f, .8f);
                        if (f.packet.lightning > 0)
                        {
                            game.Combat.Query(destination, 5, chainTargets); int left = f.packet.lightning;
                            var chained = f.packet; chained.damage *= .45f; chained.lightning = 0;
                            foreach (var other in chainTargets) if (other != target && left-- > 0)
                            { other.Hit(chained); game.Combat.Chain(destination, other.transform.position + Vector3.up); }
                        }
                        f.activeWeight = 0; f.phase = Phase.Return; game.Audio.Play(SoundCue.Magic);
                    }
                }
                else
                {
                    Vector3 homeDirection = home - transform.position;
                    if (homeDirection.sqrMagnitude > .001f) transform.rotation = Quaternion.LookRotation(homeDirection);
                    transform.position = Vector3.MoveTowards(transform.position, home, Mathf.Max(Definition.speed, game.Player.Speed + 8) * 1.3f * Time.deltaTime);
                    transform.localScale = Vector3.one * .9f;
                    if ((transform.position - home).sqrMagnitude < .1f) f.phase = Phase.Follow;
                }
            }
        }
        public override void Present() { }
        public override void ShiftOrigin(Vector3 shift)
        {
            foreach (var familiar in familiars)
            { familiar.began -= shift; familiar.view.transform.position -= shift; }
        }
        public override void Dispose()
        {
            for (int i = 1; i < familiars.Count; i++) game.Pool.Return(familiars[i].view);
            familiars.Clear(); chainTargets.Clear(); base.Dispose();
        }
    }
}
