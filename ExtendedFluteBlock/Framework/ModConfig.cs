using System;
using static FluteBlockExtension.Framework.Constants;

namespace FluteBlockExtension.Framework
{
    internal class ModConfig
    {
        private bool _enableMod = true;

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

        public int MinAccessiblePitch { get; set; } = MIN_PATCHED_PRESERVEDPARENTSHEETINDEX_VALUE;

        public int MaxAccessiblePitch { get; set; } = MAX_PATCHED_PRESERVEDPARENTSHEETINDEX_VALUE;

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
