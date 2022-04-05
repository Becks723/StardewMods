using System;
using System.Linq;
using FluteBlockExtension.Framework.Models;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace FluteBlockExtension.Framework
{
    internal class SoundFloorMapper
    {
        private readonly Func<SoundFloorMap> _map;

        public SoundFloorMapper(Func<SoundFloorMap> map)
        {
            this._map = map;
        }

        /// <summary>Map direct data required by client.</summary>
        /// <param name="floor">Given floor value. May be null.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>cue: The cue to pitch and play.</item>
        /// <item>duration: The original duration (in milliseconds) of the sound.</item>
        /// <item>rawPitch: The original pitch (in milliseconds) of the sound. 0 for middle C, 1 for C#, 2 for D...</item>
        /// </list></returns>
        public (ICue cue, double duration, int rawPitch) Map(Flooring? floor)
        {
            SoundData sound = this.MapForSound(floor);

            ICue cue;
            switch (sound.SoundType)
            {
                case SoundType.GameCue:
                    cue = Game1.soundBank.GetCue(sound.CueName);
                    if (SoundManager.IsAffectedByPitchVariable(sound.CueName))
                        cue.SetVariable("Pitch", 1200);
                    break;

                case SoundType.CustomCue:
                    cue = new CustomCue(Game1.soundBank.GetCue(sound.CueName));
                    break;

                default:
                    throw new NotSupportedException();
            }

            var effects = SoundManager.GetSoundEffects(sound.CueName);
            double averageDuration = effects.Average(effect => effect.Duration.TotalMilliseconds);
            return (cue, averageDuration, sound.RawPitch);
        }

        /// <summary>Map sound data.</summary>
        /// <param name="floor">Given floor value. May be null.</param>
        public SoundData MapForSound(Flooring? floor)
        {
            FloorData floorData = FloorData.From(floor?.whichFloor?.Value);
            SoundData? sound = this._map().FindSound(floorData);
            if (sound is null)
                return this.MapForSound(null);

            if (sound == null && floorData == FloorData.NonFloor)
                throw new InvalidOperationException("Must have mapping for no-floor.");

            return sound;
        }

        public (ICue cue, double duration, int rawPitch) MapForFlute()
        {
            ICue flute = Game1.soundBank.GetCue("flute");
            flute.SetVariable("Pitch", 1200);
            double duration = Game1.waveBank.GetSoundEffect(Constants.FluteTrackIndex).Duration.TotalMilliseconds;

            return (flute, duration, 12);
        }
    }
}