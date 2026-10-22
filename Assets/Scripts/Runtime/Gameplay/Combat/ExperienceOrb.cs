using System.Collections.Generic;
using UnityEngine;

namespace VampireLike
{
    public sealed class ExperienceOrb : MonoBehaviour
    {
        GameManager game;
        int amount;
        float created;
        bool collecting;
        public int Amount=>amount;
        public Vector2Int Cell{get;private set;}
        public void UpdateCell(){Cell=CombatWorld.ExperienceCell(transform.position);}
        public bool CanMerge(Vector3 point)=>isActiveAndEnabled&&!collecting&&(point-transform.position).sqrMagnitude<CombatWorld.ExperienceMergeRadius*CombatWorld.ExperienceMergeRadius;
        public void Add(int value){amount+=value;created=Time.time;transform.localScale=Vector3.one*Mathf.Clamp(1+Mathf.Log(Mathf.Max(1,amount),2)*.07f,1,1.7f);}
        public void Initialize(GameManager manager, int value) { game = manager; amount = value; created = Time.time;collecting=false;UpdateCell();transform.localScale=Vector3.one; }
        void OnDisable(){if(game!=null&&game.Combat!=null)game.Combat.ForgetExperience(this);}
        void Update()
        {
            if (game == null || !game.Ready || game.IsPaused) return;
            Vector3 target = game.Player.transform.position + Vector3.up * .6f;
            float distance = Vector3.Distance(transform.position, target);
            if (collecting || distance < game.Player.PickupRadius){collecting=true;transform.position = Vector3.MoveTowards(transform.position, target, (9 + (Time.time - created) * .2f) * Time.deltaTime);}
            transform.Rotate(0, 90 * Time.deltaTime, 0);
            if (distance < .65f) { game.Progression.Experience(amount); game.Audio.Play(SoundCue.Pickup); game.Pool.Return(gameObject); }
            else if (Time.time - created > 180 || distance > 85) game.Pool.Return(gameObject);
        }
    }
}
