using System;
using Newtonsoft.Json;
using static FluteBlockExtension.Framework.Constants;

namespace FluteBlockExtension.Framework
{
    internal class ModConfig
    {
        private bool _enableMod;

        /// <summary>Whether to extend pitch.</summary>
        [JsonProperty("Enable Extended Pitch")]
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

        /// <summary>Different sounds on different floors.</summary>
        [JsonProperty("Enable More Sounds")]
        public bool EnableSounds { get; set; } = true;

        /// <summary>Min pitch when tuning a flute block.</summary>
        /// <remarks>Differ from <see cref="MIN_PATCHED_PRESERVEDPARENTSHEETINDEX_VALUE"/>.</remarks>
        [JsonProperty("MinPitch")]
        public int MinAccessiblePitch { get; set; } = MIN_PATCHED_PRESERVEDPARENTSHEETINDEX_VALUE;

        /// <summary>Max pitch when tuning a flute block.</summary>
        /// <remarks>Differ from <see cref="MAX_PATCHED_PRESERVEDPARENTSHEETINDEX_VALUE"/>.</remarks>
        [JsonProperty("MaxPitch")]
        public int MaxAccessiblePitch { get; set; } = MAX_PATCHED_PRESERVEDPARENTSHEETINDEX_VALUE;

        public ModConfig()
        {
            this.EnableMod = true;  // init property to run setter logic.
        }

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
