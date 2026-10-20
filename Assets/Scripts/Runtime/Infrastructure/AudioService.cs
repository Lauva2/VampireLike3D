using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VampireLike
{
    public enum SoundCue { Gun, Magic, Sword, Pickup, Hurt, Dodge, Upgrade, Unlock, Boss, Click }
    public sealed class AudioService : MonoBehaviour
    {
        GameManager game;
        AudioSource music, effects;
        readonly Dictionary<SoundCue, AudioClip> sounds = new Dictionary<SoundCue, AudioClip>();
        readonly Dictionary<BiomeId, AudioClip> tracks = new Dictionary<BiomeId, AudioClip>();
        readonly Dictionary<SoundCue, float> lastSounds = new Dictionary<SoundCue, float>();
        public IEnumerator Initialize(GameManager manager)
        {
            game = manager;
            music = gameObject.AddComponent<AudioSource>(); music.loop = true; music.spatialBlend = 0;
            effects = gameObject.AddComponent<AudioSource>(); effects.spatialBlend = 0;
            foreach (SoundCue cue in System.Enum.GetValues(typeof(SoundCue)))
                yield return game.Assets.LoadAudio("sound/" + cue.ToString().ToLowerInvariant(), clip => sounds[cue] = clip);
            for (int i = 0; i < 4; i++)
            {
                BiomeId biome = (BiomeId)i;
                yield return game.Assets.LoadAudio("music/" + i, clip => tracks[biome] = clip);
            }
            RefreshVolumes(); SetBiome(BiomeId.Forest);
        }
        public void RefreshVolumes()
        {
            if (music == null) return;
            music.volume = game.Save.Profile.musicVolume * .5f;
            effects.volume = game.Save.Profile.soundVolume * .5f;
        }
        public void SetBiome(BiomeId biome)
        {
            if (music == null || !tracks.TryGetValue(biome, out var clip) || clip == null || music.clip == clip) return;
            music.clip = clip; music.Play();
        }
        public void Play(SoundCue cue)
        {
            if (effects == null || !sounds.TryGetValue(cue, out var clip) || clip == null) return;
            if (lastSounds.TryGetValue(cue, out float last) && Time.unscaledTime - last < (cue == SoundCue.Pickup ? .09f : .055f)) return;
            lastSounds[cue] = Time.unscaledTime;
            effects.pitch = Random.Range(.94f, 1.06f); effects.PlayOneShot(clip);
        }
    }
}
