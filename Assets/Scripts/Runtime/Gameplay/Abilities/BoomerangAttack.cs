using UnityEngine;

namespace VampireLike
{
    internal sealed class BoomerangAttack : WeaponAttack
    {
        public BoomerangAttack(GameManager game, AbilityController owner) : base(game, owner, AbilityKind.Boomerang) { }
        public override void Cast(EnemyActor target) { if (!Disposed && target != null) Queue(target, .12f); }
        protected override void Fire(EnemyActor target, float weight)
        {
            Vector3 origin = Position + Vector3.up * 1.05f;
            Vector3 direction = Aim(target, origin);
            int count = Count, visible = Mathf.Min(16, count);
            for (int i = 0; i < visible; i++)
            {
                var shot = Spawn(origin, direction); if (shot == null) continue;
                Vector3 through = target.transform.position + Vector3.up * .8f;
                through += Vector3.Cross(Vector3.up, direction).normalized * (i - (visible - 1) * .5f) * .2f;
                shot.InitializeCurvedReturn(game, through, Definition.speed, owner.Packet(kind, weight * count / visible),
                    Definition.range + Effect(UpgradeEffect.Range), i % 2 == 0 ? 1 : -1);
            }
            game.Audio.Play(SoundCue.Sword);
        }
        public override void Present()
        {
            var visual = Visual; if (visual == null) return;
            float swing = Mathf.Clamp01(1 - (Time.time - lastShot) / .35f);
            visual.transform.position = Position + new Vector3(-1.05f, 1.2f, .15f) + facing * swing * .4f;
            visual.transform.rotation = Quaternion.Euler(0, Time.time * 70 + swing * 150, 15);
        }
    }
}
