using System.Collections.Generic;
using UnityEngine;

namespace VampireLike
{
    internal abstract class WeaponAttack
    {
        protected readonly GameManager game;
        protected readonly AbilityController owner;
        protected readonly AbilityKind kind;
        sealed class Volley
        {
            public EnemyActor target;
            public long life;
            public float at, spacing, weight;
            public int remaining;
        }
        readonly List<Volley> pending = new List<Volley>();
        protected Vector3 facing = Vector3.forward;
        protected float lastShot = -100;
        public int Pending => pending.Count;
        protected bool Disposed { get; private set; }
        protected WeaponAttack(GameManager game, AbilityController owner, AbilityKind kind)
        { this.game = game; this.owner = owner; this.kind = kind; }
        protected float Effect(UpgradeEffect effect) => owner.Modifier(effect, kind);
        protected int Count => Mathf.Max(1, 1 + Mathf.FloorToInt(Effect(UpgradeEffect.Count) + Effect(UpgradeEffect.Spread)));
        protected AbilityDefinition Definition => game.Catalog.Ability(kind);
        protected Vector3 Position => owner.transform.position;
        protected GameObject Visual => owner.WeaponVisual(kind);
        protected void Queue(EnemyActor target, float delay, int stages = 1, float spacing = 0, float weight = 1)
        { if (!Disposed) pending.Add(new Volley { target = target, life = target.CollisionId, at = Time.time + delay, remaining = stages, spacing = spacing, weight = weight }); }
        public abstract void Cast(EnemyActor target);
        protected abstract void Fire(EnemyActor target, float weight);
        public virtual void Present() { }
        public virtual void Refresh() { }
        public virtual void ShiftOrigin(Vector3 shift) { }
        public virtual void Tick()
        {
            if (Disposed || !game.Ready || game.IsPaused) return;
            for (int i = pending.Count - 1; i >= 0; i--)
            {
                var volley = pending[i];
                while (volley.remaining > 0 && Time.time >= volley.at)
                {
                    float range = Definition.range + Effect(UpgradeEffect.Range);
                    var target = volley.target;
                    if (target == null || target.CollisionId != volley.life || !target.CanBeTargeted ||
                        (target.transform.position - Position).sqrMagnitude > range * range || !game.Combat.Visible(Position, target))
                        target = game.Combat.Target(Position, range);
                    if (target != null) Fire(target, volley.weight);
                    volley.remaining--; volley.at += volley.spacing;
                }
                if (volley.remaining == 0) pending.RemoveAt(i);
            }
        }
        protected Vector3 Aim(EnemyActor target, Vector3 from)
        {
            Vector3 direction = (target.transform.position + Vector3.up * .8f - from).normalized;
            facing = direction; lastShot = Time.time;
            game.Player.FaceAttack(direction);
            owner.GetComponent<CharacterAnimator>()?.Attack(kind, direction);
            return direction;
        }
        protected ProjectileActor Spawn(Vector3 point, Vector3 direction)
        {
            var go = game.Pool.Rent(Definition.projectileId, point, Quaternion.LookRotation(direction));
            return go != null ? go.GetComponent<ProjectileActor>() : null;
        }
        public virtual void Dispose() { Disposed = true; pending.Clear(); }
    }
}
