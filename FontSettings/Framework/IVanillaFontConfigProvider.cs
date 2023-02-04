using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Models;

namespace FontSettings.Framework
{
    internal interface IVanillaFontConfigProvider
    {
        FontConfig_ GetVanillaFontConfig(LanguageInfo language, GameFontType fontType);
    }

    internal class VanillaFontConfigProvider : IVanillaFontConfigProvider
    {
        private readonly IDictionary<FontConfigKey, FontConfig_> _vanillaFontsLookup;

        public VanillaFontConfigProvider(IDictionary<FontConfigKey, FontConfig_> vanillaFonts)
        {
            this._vanillaFontsLookup = vanillaFonts;
        }

        public FontConfig_ GetVanillaFontConfig(LanguageInfo language, GameFontType fontType)
        {
            if (this._vanillaFontsLookup.TryGetValue(new FontConfigKey(language, fontType), out FontConfig_ value))
                return value;

            return this.CreateFallbackFontConfig(language, fontType);
        }

        private FontConfig_ CreateFallbackFontConfig(LanguageInfo language, GameFontType fontType)
        {
            if (fontType != GameFontType.SpriteText)
                return new FontConfig_(
                    Enabled: true,
                    FontFilePath: null,
                    FontIndex: 0,
                    FontSize: 26,
                    Spacing: 0,
                    LineSpacing: 26,
                    CharOffsetX: 0,
                    CharOffsetY: 0,
                    CharacterRanges: CharRangeSource.GetBuiltInCharRange(language));

            else
                return new BmFontConfig(
                    Enabled: true,
                    FontFilePath: null,
                    FontIndex: 0,
                    FontSize: 26,
                    Spacing: 0,
                    LineSpacing: 26,
                    CharOffsetX: 0,
                    CharOffsetY: 0,
                    CharacterRanges: CharRangeSource.GetBuiltInCharRange(language),
                    PixelZoom: FontHelpers.GetDefaultFontPixelZoom());
        }
    }
}
