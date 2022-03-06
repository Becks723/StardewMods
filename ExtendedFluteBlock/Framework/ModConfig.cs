using System;
using static FluteBlockExtension.Framework.Constants;

namespace FluteBlockExtension.Framework
{
    internal class ModConfig
    {
        private bool _enableMod = true;

        /// <summary>Whether to extend pitch.</summary>
        public bool EnableMod
        {
            get => this._enableMod;
            set
            {
                this._enableMod = value;

                if (this._enableMod)
                    MainPatcher.Patch();
                else
                    MainPatcher.Unpatch();
            }
        }

        /// <summary>Min pitch when tuning a flute block.</summary>
        /// <remarks>Differ from <see cref="MIN_PATCHED_PRESERVEDPARENTSHEETINDEX_VALUE"/>.</remarks>
        public int MinAccessiblePitch { get; set; } = MIN_PATCHED_PRESERVEDPARENTSHEETINDEX_VALUE;

        /// <summary>Max pitch when tuning a flute block.</summary>
        /// <remarks>Differ from <see cref="MAX_PATCHED_PRESERVEDPARENTSHEETINDEX_VALUE"/>.</remarks>
        public int MaxAccessiblePitch { get; set; } = MAX_PATCHED_PRESERVEDPARENTSHEETINDEX_VALUE;

        /// <summary>Verify new pitches value, then update the ones in patcher.</summary>
        /// <remarks>Call this when either <see cref="MinAccessiblePitch"/> or <see cref="MaxAccessiblePitch"/> is changed.</remarks>
        public void UpdatePitches()
        {
            int minPitch = this.MinAccessiblePitch;
            int maxPitch = this.MaxAccessiblePitch;
            minPitch = Math.Clamp(minPitch, MIN_PATCHED_PRESERVEDPARENTSHEETINDEX_VALUE, maxPitch);
            maxPitch = Math.Clamp(maxPitch, minPitch, MAX_PATCHED_PRESERVEDPARENTSHEETINDEX_VALUE);
            this.MinAccessiblePitch = MainPatcher.MinPitch = minPitch;
            this.MaxAccessiblePitch = MainPatcher.MaxPitch = maxPitch;
        }
    }
}
