using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluteBlockExtension.Framework.Models;
using StardewValley;

namespace FluteBlockExtension.Framework
{
    internal class SoundResolver
    {
        public MappedSound ResolveSoundData(SoundData? sound)
        {
            // check sound valid.
            if (!CueUtilites.IsCueValid(sound?.CueName, out _))
            {
                return MappedSound.Mute;
            }

            ICue cue = Game1.soundBank.GetCue(sound.CueName);
            if (CueUtilites.IsNativeCue(sound.CueName))
            {
                // reset pitch to 0.
                if (CueUtilites.IsAffectedByPitchVariable(sound.CueName))
                    cue.SetVariable("Pitch", 1200);
            }
            else
            {
                cue = new CustomCue(cue);
            }

            return new MappedSound(cue, sound.RawPitch, this.GetDuration(sound.CueName));
        }

        private double GetDuration(string cueName)
        {
            var effects = CueUtilites.GetSoundEffects(cueName);

            if (effects.All(effect => effect is null))
            {
                return 0;
            }

            return effects.Where(effect => effect != null)
                          .Average(effect => effect.Duration.TotalMilliseconds);
        }
    }
}