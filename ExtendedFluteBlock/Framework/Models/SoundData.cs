using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;
using StardewModdingAPI.Utilities;

namespace FluteBlockExtension.Framework.Models
{
    /// <summary>Model for a sound.</summary>
    internal class SoundData : IEquatable<SoundData>
    {
        private string _name;

        private string _description;

        public static readonly SoundData Empty = new SoundData() { IsEmpty = true };

        public static SoundData Flute = new SoundData
        {
            NameFunc = I18n.Sound_Flute_Name,
            DescriptionFunc = I18n.Sound_Flute_Desc,
            CueName = "flute",
            RawPitch = 12,
            IsEmpty = false
        };

        public static SoundData GameSound(
            Func<string> name,
            string cueName,
            int rawPitch = 0,
            Func<string> description = null)
        {
            SoundData sound = new SoundData { NameFunc = name, CueName = cueName, RawPitch = rawPitch, DescriptionFunc = description };
            return sound;
        }

        public static bool IsEmptySound(SoundData sound)
        {
            return sound.IsEmptySound();
        }

        /// <summary>The display sound name.</summary>
        public string Name
        {
            get
            {
                var func = this.NameFunc;
                if (func != null) return func();
                return this._name;
            }
            set { this._name = value; }
        }

        /// <summary>The unique cue name in game.</summary>
        public string CueName { get; set; }

        /// <summary>The original pitch in the sound file.</summary>
        public int RawPitch { get; set; }

        /// <summary>Remarks for this sound.</summary>
        public string Description
        {
            get
            {
                var func = this.DescriptionFunc;
                if (func != null) return func();
                return this._description;
            }
            set { this._description = value; }
        }

        public bool IsEnabled { get; set; } = true;

        public bool IsEmpty { get; set; }

        internal Func<string> NameFunc { get; set; }

        internal Func<string> DescriptionFunc { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (obj is not SoundData other) return false;

            return this.Equals(other);
        }

        public override int GetHashCode()
        {
            return this.CueName.GetHashCode();
        }

        public bool Equals(SoundData other)
        {
            return this.CueName == other?.CueName;
        }

        public bool IsEmptySound()
        {
            return this.IsEmpty;
        }
    }
}