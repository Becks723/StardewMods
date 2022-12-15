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

        /// <summary>缩放倍数。仅当<see cref="InGameType"/>为<see cref="GameFontType.SpriteText"/>时用到。对应<see cref="StardewValley.BellsAndWhistles.SpriteText.fontPixelZoom"/>字段。</summary>
        public float PixelZoom { get; set; }

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
            other.PixelZoom = this.PixelZoom;
        }
    }
}
