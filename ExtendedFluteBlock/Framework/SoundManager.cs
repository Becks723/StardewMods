using System;
using System.Collections.Generic;
using System.Linq;
using FluteBlockExtension.Framework.Models;
using Microsoft.Xna.Framework.Audio;
using StardewValley;

namespace FluteBlockExtension.Framework
{
    /// <summary>Manages runtime sound resources.</summary>
    internal static class SoundManager
    {
        private static readonly Dictionary<string, SoundEffect[]> _sounds = new();

        /// <summary>Gets one or more soundEffects by a cue name.</summary>
        public static SoundEffect[] GetSoundEffects(string cueName)
        {
            if (!_sounds.TryGetValue(cueName, out var effect))
            {
                List<SoundEffect> soundEffects = new();
                foreach ((int waveBankIndex, int trackIndex) in GetIndexes(cueName))
                {
                    WaveBank waveBank = waveBankIndex switch
                    {
                        0 => Game1.waveBank,
                        1 => Game1.waveBank1_4,
                        _ => throw new ArgumentException($"Unknown waveBank index: {waveBankIndex}.")
                    };
                    soundEffects.Add(waveBank.GetSoundEffect(trackIndex));
                }
                effect = soundEffects.ToArray();
                _sounds[cueName] = effect;
            }

            return effect;
        }

        /// <summary>Gets whether a given cue is controlled by the cue variable named "Pitch".</summary>
        public static bool IsAffectedByPitchVariable(string cueName)
        {
            // only game cue can have cue variables.
            CueDefinition cue = Game1.soundBank.GetCueDefinition(cueName);
            foreach (XactSoundBankSound sound in cue.sounds)
            {
                if (sound.rpcCurves.Contains(3))    // rpc curve index 3 points to the Pitch variable.
                {
                    return true;
                }
            }

            return false;
        }

        public static void LoadSounds(SoundsConfig config)
        {
            string soundsFolder = config.SoundsFolderPath;
            var soundFloorPairs = config.SoundFloorPairs;
            foreach (var pair in soundFloorPairs)
            {
                SoundData sound = pair.Sound;
                AssertMissingFields(sound);
                switch (sound.SoundType)
                {
                    case SoundType.CustomCue:
                        string cueName = sound.CueName;
                        if (!IsCueLoaded(cueName))
                        {
                            SoundEffect[] effects = sound.LoadSoundEffects(soundsFolder);
                            _sounds[cueName] = effects;
                            CueDefinition cue = new(
                                cueName,
                                effects,
                                3
                            );
                            Game1.soundBank.AddCue(cue);
                        }
                        break;
                }
            }
        }

        private static bool IsCueLoaded(string cueName)
        {
            try
            {
                Game1.soundBank.GetCue(cueName);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static IEnumerable<(int waveBankIndex, int trackIndex)> GetIndexes(string cueName)
        {
            // cueName here always refers to an original game cue.
            // Because custom cues don't need to call this method, and custom ones don't use these two fields.
            CueDefinition cue = Game1.soundBank.GetCueDefinition(cueName);
            foreach (XactSoundBankSound sound in cue.sounds)
            {
                if (!sound.complexSound)
                {
                    yield return (sound.waveBankIndex, sound.trackIndex);
                }
                else
                {
                    if (sound.soundClips != null)
                    {
                        /*foreach (XactClip clip in sound.soundClips)
                        {
                            foreach (ClipEvent ev in clip.clipEvents)
                            {
                                if (ev is PlayWaveEvent wave)
                                {
                                    foreach (PlayWaveVariant variant in wave.GetVariants())
                                    {
                                        yield return (variant.waveBank, variant.track);
                                    }
                                }
                            }
                        }*/

                        var items = from clip in sound.soundClips
                                    from ev in clip.clipEvents
                                    where ev is PlayWaveEvent
                                    from variant in (ev as PlayWaveEvent).GetVariants()
                                    select (variant.waveBank, variant.track);
                        foreach (var item in items)
                            yield return item;
                    }
                }
            }
        }

        private static void AssertMissingFields(SoundData sound)
        {
            AssertNull(nameof(sound.Name), sound.Name);
            AssertNull(nameof(sound.CueName), sound.CueName);

            switch (sound.SoundType)
            {
                case SoundType.GameCue:
                    break;

                case SoundType.CustomCue:
                    AssertEmpty(nameof(sound.FilePaths), sound.FilePaths);
                    break;
            }
        }

        private static void AssertNull<T>(string name, T field)
            where T : class
        {
            if (field is null)
                throw new ArgumentNullException(name);
        }

        private static void AssertEmpty<TList>(string name, TList list)
            where TList : System.Collections.IList
        {
            if (list.Count == 0)
                throw new ArgumentException(name);
        }

        //public void AddSound(string name, SoundEffect soundEffect)
        //{
        //    CueDefinition definition = new CueDefinition(name, soundEffect, 3);
        //    Game1.soundBank.AddCue(definition);
        //}

        //public void SetPitch(string name, float octaves)
        //{
        //    CueDefinition definition = Game1.soundBank.GetCueDefinition(name);
        //    definition.sounds[0].pitch = octaves;
        //}
    }
}
