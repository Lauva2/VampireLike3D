using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace VampireLike.Editor
{
    public static class AudioFactory
    {
        public static List<AudioEntry> Create()
        {
            Directory.CreateDirectory("Assets/Art/Audio/Generated"); var result = new List<AudioEntry>();
            foreach (SoundCue cue in Enum.GetValues(typeof(SoundCue)))
            {
                float length = cue == SoundCue.Unlock ? 1.1f : cue == SoundCue.Upgrade ? .6f : .18f;
                int cueIndex = (int)cue;
                string path = "Assets/Art/Audio/Generated/" + cue.ToString().ToLowerInvariant() + ".wav";
                Write(path, length, (t, i) =>
                {
                    float env = Mathf.Exp(-t * (cue == SoundCue.Unlock ? 4 : 19));
                    float frequency = cue == SoundCue.Gun ? 130 : cue == SoundCue.Hurt ? 110 : cue == SoundCue.Pickup ? 1100 : 380 + cueIndex * 57;
                    if (cue == SoundCue.Unlock || cue == SoundCue.Upgrade) frequency *= Mathf.Pow(2, new[] { 0,4,7,12 }[Mathf.Min(3,(int)(t*7))]/12f);
                    if (cue == SoundCue.Dodge) return Mathf.Sin(t * (1300 - t * 2000)) * env * .23f;
                    return (Mathf.Sin(t * frequency * Mathf.PI * 2) + .25f * Mathf.Sin(t * frequency * Mathf.PI * 4)) * env * .22f;
                });
                result.Add(Entry("sound/" + cue.ToString().ToLowerInvariant(), path));
            }
            for (int biome = 0; biome < 6; biome++)
            {
                int b = biome; string path = "Assets/Art/Audio/Generated/music-" + b + ".wav";
                Write(path, 24, (t, i) =>
                {
                    int[] scale = { 0,2,4,7,9,12,9,7,4,2,7,4,2,0,4,7 };
                    int[] chords = { 0, -5, -3, -7 };
                    float beat = .375f; int note = Mathf.FloorToInt(t / beat); float phase = t % beat;
                    int chord = chords[Mathf.FloorToInt(t / 6) % 4];
                    float f = 261.6256f * Mathf.Pow(2, (scale[(note + b * 3) % scale.Length] + chord) / 12f);
                    float melody = Mathf.Sin(phase * f * 6.28318f) * Mathf.Exp(-phase * 9) * .13f;
                    melody += Mathf.Sin(phase * f * 12.56636f) * Mathf.Exp(-phase * 18) * .025f;
                    float bass = 65.4064f * Mathf.Pow(2, chord / 12f);
                    float pad = Mathf.Sin(t * bass * 6.28318f) * .045f + Mathf.Sin(t * bass * 1.5f * 6.28318f) * .025f;
                    float pulse = Mathf.Sin(t * 7.5f) * .005f;
                    float edge = Mathf.Min(1, Mathf.Min(t * 10, (24-t)*10));
                    return (melody + pad + pulse) * edge;
                });
                result.Add(Entry("music/" + b, path));
            }
            return result;
        }
        static AudioEntry Entry(string id,string path)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(path) as AudioImporter;
            if (importer != null) { importer.forceToMono = true; var settings = importer.defaultSampleSettings; settings.compressionFormat = AudioCompressionFormat.Vorbis; settings.quality = .65f; importer.defaultSampleSettings = settings; importer.SaveAndReimport(); }
            return new AudioEntry { id=id, reference=new AssetReferenceT<AudioClip>(AssetDatabase.AssetPathToGUID(path)) };
        }
        static void Write(string path,float duration,Func<float,int,float> sample)
        {
            if(File.Exists(path))return;
            const int rate=22050; int count=(int)(duration*rate);
            using(var writer=new BinaryWriter(File.Create(path)))
            {
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); writer.Write(36+count*2); writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
                writer.Write(16); writer.Write((short)1);writer.Write((short)1);writer.Write(rate);writer.Write(rate*2);writer.Write((short)2);writer.Write((short)16);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(count*2);
                for(int i=0;i<count;i++) writer.Write((short)(Mathf.Clamp(sample(i/(float)rate,i),-1,1)*short.MaxValue));
            }
        }
    }
}
