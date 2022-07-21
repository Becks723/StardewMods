using System;
using System.Linq;

namespace FontSettings.Framework
{
    internal class FontConfig : Legacy.FontConfig_0_2_0
    {
        /// <summary>字符在横轴上的偏移量。</summary>
        public float CharOffsetX { get; set; }

        /// <summary>字符在纵轴上的偏移量。</summary>
        public float CharOffsetY { get; set; }

        internal void CopyTo(FontConfig other)
        {
            if (other is null) throw new ArgumentNullException(nameof(other));

            other.Enabled = this.Enabled;
            other.Lang = this.Lang;
            other.Locale = this.Locale;
            other.InGameType = this.InGameType;
            other.ExistingFontPath = this.ExistingFontPath;
            other.FontFilePath = this.FontFilePath;
            other.FontIndex = this.FontIndex;
            other.FontSize = this.FontSize;
            other.Spacing = this.Spacing;
            other.LineSpacing = this.LineSpacing;
            other.TextureWidth = this.TextureWidth;
            other.TextureHeight = this.TextureHeight;
            other.CharacterRanges = this.CharacterRanges?.AsEnumerable();
            other.CharOffsetX = this.CharOffsetX;
            other.CharOffsetY = this.CharOffsetY;
        }
    }
}
