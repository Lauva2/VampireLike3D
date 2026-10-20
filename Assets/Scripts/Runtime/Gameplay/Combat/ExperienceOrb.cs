using System.Collections.Generic;
using UnityEngine;

namespace VampireLike
{
    public sealed class ExperienceOrb : MonoBehaviour
    {
        GameManager game;
        int amount;
        float created;
        public void Initialize(GameManager manager, int value) { game = manager; amount = value; created = Time.time; }
        void Update()
        {
            if (game == null || !game.Ready || game.IsPaused) return;
            Vector3 target = game.Player.transform.position + Vector3.up * .6f;
            float distance = Vector3.Distance(transform.position, target);
            if (distance < game.Player.PickupRadius) transform.position = Vector3.MoveTowards(transform.position, target, (9 + (Time.time - created) * .2f) * Time.deltaTime);
            transform.Rotate(0, 90 * Time.deltaTime, 0);
            if (distance < .65f) { game.Progression.Experience(amount); game.Audio.Play(SoundCue.Pickup); game.Pool.Return(gameObject); }
            else if (Time.time - created > 180 || distance > 85) game.Pool.Return(gameObject);
        }
    }
}
