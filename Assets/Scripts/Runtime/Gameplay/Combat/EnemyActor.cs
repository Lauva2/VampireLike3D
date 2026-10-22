using UnityEngine;

namespace VampireLike
{
    public sealed partial class EnemyActor : MonoBehaviour
    {
        public Transform visual, barFill, barRoot, leftArm, rightArm, eliteHalo,heart;
        public Transform eyeRig,leftFoot,rightFoot;
        public Transform[] bodySegments;
        Vector3[] segmentPositions;
        uint life;
        public long CollisionId => ((long)GetInstanceID() << 32) | life;
        public int Type { get; private set; }
        public bool Elite { get; private set; }
        public BiomeId Biome { get; private set; }
        public float Health { get; private set; }
        public float MaxHealth { get; private set; }
        public bool IsAlive => gameObject.activeInHierarchy && Health > 0;
        public bool Frozen => frostUntil > Time.time;
        public bool DiagnosticImmortal,DiagnosticStationary;
        GameManager game;
        float nextAttack, nextBurn, burnUntil, frostUntil, burnDamage, slowUntil, flashUntil;
        Vector3 chargeDirection;
        float chargeAt, chargeEnd;
        float speed, damage;
        DamagePacket lastBurn;
        int attacks;
        Vector3 initialScale;
        Renderer[] renderers;
        MaterialPropertyBlock block;
        float attackAnimation;
        Color cachedTint, biomeTint;float cachedFlash;
        void Awake(){block=new MaterialPropertyBlock();}

        public void Initialize(GameManager manager, int type, bool elite, BiomeId biome)
        {
            game = manager; Type = type; Elite = elite; Biome = biome; unchecked { life++; }
            StopAllCoroutines(); leapActive = false; leapHeight = 0;
            float growth = 1 + game.Run.seconds / 200;
            MaxHealth = (type == 4 ? 1050 + game.Run.seconds * 2.5f : (type == 1 ? 8 : type == 2 ? 30 : 15) * (elite ? 5 : 1) * growth);
            var definition = game.Catalog.Enemy(type);
            if (type != 4 && definition != null) MaxHealth = definition.health * (elite ? 5 : 1) * growth;
            if (type == 4 && (biome == BiomeId.Swamp || biome == BiomeId.Crystal)) MaxHealth *= biome == BiomeId.Swamp ? 1.25f : 1.4f;
            Health = MaxHealth;
            speed = type == 4 ? 1.65f : type == 1 ? 4.5f : type == 2 ? 2.15f : type == 3 ? 2 : 2.45f;
            damage = (type == 4 ? 24 : type == 2 ? 13 : 7) * (1 + game.Run.seconds / 1500);
            if (type != 4 && definition != null) { speed = definition.speed * (elite && type >= 5 ? 1.12f : 1); damage = definition.damage * (1 + game.Run.seconds / 1500); }
            nextAttack = Time.time + Random.Range(1f, 2.5f); nextBurn = 0; burnUntil = 0; frostUntil = 0; slowUntil = 0;
            chargeAt = chargeEnd = 0; attacks = 0; burnDamage = 0; flashUntil = 0;
            initialScale = Vector3.one * (type == 4 ? 1 : elite ? 1.45f : 1);
            transform.localScale = initialScale;
            if (barRoot != null) barRoot.gameObject.SetActive(elite);
            if(eliteHalo!=null)eliteHalo.gameObject.SetActive(elite&&type!=4);
            renderers=visual.GetComponentsInChildren<Renderer>();
            visual.localPosition=Vector3.zero;visual.localRotation=Quaternion.identity;visual.localScale=Vector3.one;
            attackAnimation=-10;
            DiagnosticImmortal=false;DiagnosticStationary=false;
            if(eyeRig!=null)eyeRig.localScale=Vector3.one;
            if(leftFoot!=null)leftFoot.localRotation=Quaternion.identity;if(rightFoot!=null)rightFoot.localRotation=Quaternion.identity;
            windup=Type==4?(biome==BiomeId.Snow?1.15f:biome==BiomeId.Volcano||biome==BiomeId.Crystal?1.1f:.95f):Type==3?.4f:Type==5?.9f:Type==6?.95f:Type==7?1:Type==8?.65f:.8f;
            if(heart!=null)heartRenderer=heart.GetComponent<Renderer>();
            if(bodySegments!=null&&bodySegments.Length>0){if(segmentPositions==null){segmentPositions=new Vector3[bodySegments.Length];for(int i=0;i<bodySegments.Length;i++)segmentPositions[i]=bodySegments[i].localPosition;}for(int i=0;i<bodySegments.Length;i++)bodySegments[i].localPosition=segmentPositions[i];}
            SetTint();
        }
        void SetTint()
        {
            if (visual == null) return;
            Color tint = game.Catalog.Biome(Biome).accent;
            cachedTint=biomeTint=Color.Lerp(Color.white,tint,.12f);cachedFlash=0;
            foreach (var renderer in renderers)
            {
                renderer.GetPropertyBlock(block);
                block.SetColor("_Tint", Color.Lerp(Color.white, tint, .12f));block.SetFloat("_Flash",0);block.SetFloat("_Visibility",1);block.SetFloat("_Emission",0);renderer.SetPropertyBlock(block);
            }
        }
        void Update()
        {
            if (game == null || !game.Ready || game.IsPaused || Health <= 0) return;
            if (burnUntil > Time.time && nextBurn <= Time.time)
            {
                nextBurn = Time.time + .5f;
                var packet = lastBurn; packet.damage = burnDamage * .5f; packet.burn = 0; packet.burning = true;
                Hit(packet); if (!IsAlive) return;
            }
            Vector3 delta = game.Player.transform.position - transform.position; delta.y = 0;
            float distance = delta.magnitude;
            Vector3 direction = distance > .01f ? delta / distance : Vector3.forward;
            float movement = Frozen ? 0 : slowUntil > Time.time ? .45f : 1;
            if (Type == 4) BossAttacks(distance);
            if (Type >= 5) ExpandedAttacks(distance, direction, ref movement);
            if (leapActive) movement = 0;
            if(Type==4&&Time.time-attackAnimation<windup)movement=0;
            else if (Type == 3 && distance < 17 && Time.time > nextAttack)
            { nextAttack = Time.time + 2.6f;attackAnimation=Time.time;StartCoroutine(FireAfterWindup(direction)); }
            else if (Type == 2 && distance < 14 && Time.time > nextAttack)
            {
                nextAttack = Time.time + 4.5f; chargeDirection = direction; chargeAt = Time.time + .8f; chargeEnd = chargeAt + .6f;
                attackAnimation=Time.time;game.Combat.HazardLine(transform.position,transform.position+direction*8,.72f,.75f,.6f,damage,false);
            }
            if (chargeAt > 0 && Time.time < chargeAt) movement = 0;
            if (chargeAt > 0 && Time.time >= chargeAt && Time.time < chargeEnd) { direction = chargeDirection; movement *= Type==4?8:5; }
            if (Time.time >= chargeEnd) chargeAt = 0;
            if (Type == 3 && distance < 10) movement = .2f;
            if (!leapActive && distance < (Type == 4 ? 2 : .85f)) { game.Player.Hurt(damage); movement = .1f; }
            if(DiagnosticStationary&&game.DiagnosticMode)movement=0;
            Vector3 step = direction * speed * movement * Time.deltaTime;
            if (Physics.CheckSphere(transform.position + step + Vector3.up * .6f, .4f, 1 << 10)) step = Vector3.Cross(direction, Vector3.up) * speed * movement * Time.deltaTime;
            transform.position += step;
            if(Physics.Raycast(transform.position+Vector3.up*4,Vector3.down,out var ground,8,1<<0))transform.position=new Vector3(transform.position.x,Mathf.MoveTowards(transform.position.y,ground.point.y,Time.deltaTime*10),transform.position.z);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 6);
            if (visual != null)
            {
                float phase = Time.time * (Type == 4 ? 3 : 8) + GetInstanceID();
                visual.localPosition = Vector3.up * Mathf.Abs(Mathf.Sin(phase)) * (Type==1?.16f:.07f) * Mathf.Min(movement,1);
                visual.localRotation = Quaternion.Euler(chargeAt>Time.time?-15:chargeAt>0?18:0, 0, Mathf.Sin(phase) * (Frozen ? 0 : 4));
                float squash=Type==0?Mathf.Sin(phase)*.07f:0;visual.localScale =new Vector3(1-squash*.5f,1+squash,1-squash*.5f)*(Time.time < flashUntil ? 1.04f : 1);
                float attackT=Time.time-attackAnimation;
                float swing=attackT<.8f?-Mathf.Sin(attackT/.8f*Mathf.PI*.5f)*90:attackT<1.15f?Mathf.Lerp(-90,35,(attackT-.8f)/.35f):Mathf.Lerp(35,0,Mathf.Clamp01((attackT-1.15f)/.4f));
                if(attackT>1.55f)swing=0;
                if(leftArm!=null)leftArm.localRotation=Quaternion.Euler(swing+Mathf.Sin(phase)*8*movement,0,-5);
                if(rightArm!=null)rightArm.localRotation=Quaternion.Euler(swing-Mathf.Sin(phase)*8*movement,0,5);
                AnimateIntent(phase);
                AnimateExpansion();
                if(leftFoot!=null)leftFoot.localRotation=Quaternion.Euler(Mathf.Sin(phase)*32*movement,0,0);
                if(rightFoot!=null)rightFoot.localRotation=Quaternion.Euler(-Mathf.Sin(phase)*32*movement,0,0);
                if(eyeRig!=null)
                {
                    float cycle=(Time.time+GetInstanceID()*.173f)%5.1f,blink=cycle<.14f?Mathf.Sin(cycle/.14f*Mathf.PI):0;
                    float openness=Frozen?1.12f:Time.time<flashUntil?.38f:Mathf.Lerp(1,.06f,blink);
                    eyeRig.localScale=new Vector3(Time.time<flashUntil?1.08f:1,openness,1);
                }
                if(bodySegments!=null&&segmentPositions!=null)for(int i=0;i<bodySegments.Length;i++){bodySegments[i].localPosition=segmentPositions[i]+new Vector3(Mathf.Sin(phase-i*.65f)*.18f,Mathf.Sin(phase-i*.65f)*.06f,0);bodySegments[i].localRotation=Quaternion.Euler(0,Mathf.Cos(phase-i*.65f)*9,0);}
                float flash=Time.time<flashUntil?(Type==4?.10f:.22f):0;Color tint=Frozen?new Color(.63f,.87f,1):burnUntil>Time.time?new Color(1,.90f,.76f):biomeTint;
                if(tint!=cachedTint||!Mathf.Approximately(flash,cachedFlash)){cachedTint=tint;cachedFlash=flash;foreach(var renderer in renderers){renderer.GetPropertyBlock(block);block.SetFloat("_Flash",flash);block.SetColor("_Tint",tint);renderer.SetPropertyBlock(block);}}
            }
            if (barFill != null) barFill.localScale = new Vector3(Mathf.Max(.001f, Health / MaxHealth), 1, 1);
            if(barRoot!=null)barRoot.gameObject.SetActive(Elite||game.Combat.Marked==this||Health<MaxHealth&&distance<9);
            if (barRoot != null && Camera.main != null) barRoot.rotation = Camera.main.transform.rotation;
        }
        void BossAttacks(float distance)
        {
            if (Time.time < nextAttack || distance > 32) return;
            nextAttack = Time.time + (Health < MaxHealth * .5f ? 2.4f : 3.4f); attacks++;
            attackAnimation=Time.time;
            Vector3 target = game.Player.transform.position;
            if (Biome == BiomeId.Swamp || Biome == BiomeId.Crystal) { ExpandedBossAttack(target); return; }
            if (Biome == BiomeId.Forest)
            {
                for (int i = 0; i < 3; i++) game.Combat.Hazard(target + new Vector3((i - 1) * 3, 0, 0), 1.6f, .95f + i * .12f, .35f, damage, true);
                if (attacks % 3 == 0) for (int i = 0; i < 4; i++) game.Combat.Spawn(i % 2, transform.position + Quaternion.Euler(0, i * 90, 0) * Vector3.forward * 4);
            }
            else if (Biome == BiomeId.Desert)
            {
                Vector3 d = (target - transform.position).normalized;
                d.y=0;d.Normalize();chargeDirection=d;chargeAt=Time.time+.95f;chargeEnd=chargeAt+1.15f;game.Combat.HazardLine(transform.position,transform.position+d*18,1.5f,.95f,1.1f,damage,true);
            }
            else if (Biome == BiomeId.Snow)
            {
                game.Combat.Hazard(target, 3.2f, 1.15f, .6f, damage, true);
                for (int i = 0; i < 8; i++) game.Combat.Hazard(transform.position + Quaternion.Euler(0, i * 45, 0) * Vector3.forward * 6, 1.3f, 1, .6f, damage, true);
            }
            else
            {
                for (int i = 0; i < 5; i++) game.Combat.Hazard(target + new Vector3(Mathf.Sin(i * 2.4f) * 4, 0, Mathf.Cos(i * 2.4f) * 4), 2.1f, 1.1f + i * .15f, 4, damage, true);
            }
        }
        public void Hit(DamagePacket packet)
        {
            if (!IsAlive) return;
            float amount = packet.damage;
            if (Type == 7 && Time.time >= chargeEnd + 1.2f && Vector3.Dot(transform.forward, (game.Player.transform.position - transform.position).normalized) > .25f) amount *= .65f;
            if (Frozen && packet.shatter) { amount *= 1.6f; game.Combat.Effect("vfx/frost", transform.position, .35f, 2); }
            Health = DiagnosticImmortal&&game.DiagnosticMode?Mathf.Max(1,Health-amount):Health-amount; if(Time.time>flashUntil+.14f)flashUntil = Time.time + .055f;
            if (barRoot != null) barRoot.gameObject.SetActive(Elite||game.Combat.Marked==this||Vector3.Distance(transform.position,game.Player.transform.position)<9);
            if (packet.burn > 0) { burnUntil = Time.time + 3; burnDamage = Mathf.Max(burnDamage, packet.burn); lastBurn = packet; }
            if (packet.frost > 0)
            {
                slowUntil = Time.time + 2;
                if (Random.value < packet.frost && !Frozen) { frostUntil = Time.time + (Type == 4 ? .3f : 1.4f); game.Save.Profile.freezes++; game.Save.MarkDirty(); }
            }
            if (Health <= 0) game.Combat.Kill(this, packet);
        }
        public void PlayDeath(){StartCoroutine(Death());}
        System.Collections.IEnumerator FireAfterWindup(Vector3 direction)
        {
            yield return new WaitForSeconds(.4f);if(game!=null&&game.Ready&&IsAlive)game.Combat.EnemyShot(transform.position+Vector3.up*.9f,direction,damage);
        }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public void DiagnosticAttackAfter(float delay){if(game!=null&&game.DiagnosticMode)nextAttack=Time.time+delay;}
#endif
        public void DiagnosticHealth(float fraction){if(game!=null&&game.DiagnosticMode)Health=MaxHealth*Mathf.Clamp01(fraction);}
    }
}
