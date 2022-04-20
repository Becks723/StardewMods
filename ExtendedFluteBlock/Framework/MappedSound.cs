using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;

namespace FluteBlockExtension.Framework
{
    internal class MappedSound
    {
        public static MappedSound Mute { get; } = new(new DummyCue(), 0, 0);

        public static MappedSound Flute { get; } = new(Game1.soundBank.GetCue("flute"), 12, 500);

        /// <summary>The cue to play.</summary>
        public ICue Cue { get; }

        /// <summary>The original pitch of the sound. 0 for middle C, 1 for C#, 2 for D...</summary>
        public int RawPitch { get; }

        /// <summary>The original duration of the sound, in milliseconds.</summary>
        public double Duration { get; }

        public MappedSound(ICue cue!!, int rawPitch, double duration)
        {
            this.Cue = cue;
            this.RawPitch = rawPitch;
            this.Duration = duration;
        }
    }
}