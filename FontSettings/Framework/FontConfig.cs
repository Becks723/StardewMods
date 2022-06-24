using System;
using System.Linq;

namespace FontSettings.Framework
{
    internal class FontConfig : Legacy.FontConfig_0_1_0
    {
        public string Locale { get; set; }

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
