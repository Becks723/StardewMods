using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;

namespace FontSettings.Framework
{
    internal class FontConfig
    {
        public bool Enabled { get; set; } = false;

        public LocalizedContentManager.LanguageCode Lang { get; set; }

        public string Locale { get; set; }

        public GameFontType InGameType { get; set; }

        public string ExistingFontPath { get; set; }

        public string FontFilePath { get; set; }

        /// <summary>字体在合集文件（.ttc、.otc）中的索引。</summary>
        public int FontIndex { get; set; } = 0;

        /// <summary>字体大小，单位为像素px。</summary>
        public float FontSize { get; set; }

        public float Spacing { get; set; }

        /// <summary>行间距，即两个相邻基准线的距离。（ascent - descent + lineGap）</summary>
        public int LineSpacing { get; set; }

        public int? TextureWidth { get; set; }

        public int? TextureHeight { get; set; }

        public IEnumerable<CharacterRange> CharacterRanges { get; set; }

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
        }

        internal LanguageInfo GetLanguage()
        {
            return new LanguageInfo(this.Lang, this.Locale);
        }
    }
}
