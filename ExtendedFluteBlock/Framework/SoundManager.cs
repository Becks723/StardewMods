using System;
using System.Collections.Generic;
using System.Linq;
using FluteBlockExtension.Framework.Models;
using Microsoft.Xna.Framework.Audio;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace FluteBlockExtension.Framework
{
    /// <summary>Manages runtime sound resources.</summary>
    internal static class SoundManager
    {
        /// <summary>Stored cueName-sounds pairs.</summary>
        private static readonly Dictionary<string, List<RuntimeSound>> _sounds = new();

        public static IMonitor Monitor;

        /// <summary>Gets one or more soundEffects by a cue name.</summary>
        public static SoundEffect[] GetSoundEffects(string cueName)
        {
            // 由于自定义音色在加载时便保存了，所以可以直接拿到。这里TryGetValue拿不到的都是原版音色。
            if (!_sounds.TryGetValue(cueName, out var sounds))
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

                // 保存。
                sounds = new(soundEffects.Count);
                foreach (SoundEffect effect in soundEffects)
                    sounds.Add(new(null, effect));
                _sounds[cueName] = sounds;
            }

            return sounds.Select(s => s.Effect).ToArray();
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

        /// <summary>Load or update sounds according to data in the config.</summary>
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
                        bool edit = _sounds.TryGetValue(cueName, out var oldSounds);
                        List<RuntimeSound> newSounds = new(sound.FilePaths.Count);
                        foreach (FilePath path in sound.FilePaths)
                        {
                            string fullPath = PathUtilities.NormalizePath(path.GetFullPath(soundsFolder));
                            if (edit)
                            {
                                var exist = oldSounds.Find(s => s.FullPath == fullPath);
                                if (exist != null)
                                {
                                    oldSounds.Remove(exist);
                                    if (exist.Effect != null)
                                    {
                                        newSounds.Add(exist);
                                        continue;
                                    }
                                }
                            }
                            newSounds.Add(
                                new(fullPath, LoadSoundEffect(fullPath, cueName))
                            );
                        }

                        // remove old sounds.
                        if (edit)
                        {
                            foreach (var s in oldSounds)
                            {
                                s.Effect?.Dispose();
                            }
                        }

                        _sounds[cueName] = newSounds;

                        // set cue.
                        SoundEffect[] effects = newSounds.Select(s => s.Effect).Where(e => e != null).ToArray();
                        try
                        {
                            // existing cue.
                            var cueDef = Game1.soundBank.GetCueDefinition(cueName);
                            cueDef.SetSound(effects, 3);
                        }
                        catch
                        {
                            // new cue.
                            CueDefinition cue = new(cueName, effects, 3);
                            Game1.soundBank.AddCue(cue);
                        }
                        break;
                }
            }
        }

        private static SoundEffect? LoadSoundEffect(string path, string cueName)
        {
            try
            {
                return SoundEffect.FromFile(
                    path = PathUtilities.NormalizePath(path)
                );
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed when attempting to load sound file in a cue.\n"
                          + $"Cue name: {cueName}\n"
                          + $"Sound file: {path}\n"
                          + $"Message: {ex.Message}\n" 
                          + $"Stack Trace:\n{ex.StackTrace}", LogLevel.Error);
                return null;
            }
        }

        /// <summary>Queries raw data of a cue. Must be an original game cue.</summary>
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

        private class RuntimeSound
        {
            public string? FullPath { get; }

            public SoundEffect? Effect { get; }

            public RuntimeSound(string? fullPath, SoundEffect? effect)
            {
                this.FullPath = fullPath;
                this.Effect = effect;
            }
        }
    }
}
