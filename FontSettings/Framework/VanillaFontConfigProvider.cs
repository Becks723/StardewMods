using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Models;

namespace FontSettings.Framework
{
    internal class VanillaFontConfigProvider : IVanillaFontConfigProvider
    {
        private readonly IDictionary<FontConfigKey, FontConfig> _vanillaFontsLookup = new Dictionary<FontConfigKey, FontConfig>();
        private readonly IVanillaFontProvider _vanillaFontProvider;

        public VanillaFontConfigProvider(IVanillaFontProvider vanillaFontProvider)
        {
            this._vanillaFontProvider = vanillaFontProvider;
        }

        public VanillaFontConfigProvider(IDictionary<FontConfigKey, FontConfig> vanillaFonts, IVanillaFontProvider vanillaFontProvider)
            : this(vanillaFontProvider)
        {
            foreach (var pair in vanillaFonts)
                this._vanillaFontsLookup.Add(pair);
        }

        public void AddVanillaFontConfigs(IDictionary<FontConfigKey, FontConfig> vanillaFonts)
        {
            foreach (var pair in vanillaFonts)
                this._vanillaFontsLookup[pair.Key] = pair.Value;
        }

        public FontConfig GetVanillaFontConfig(LanguageInfo language, GameFontType fontType)
        {
            if (this._vanillaFontsLookup.TryGetValue(new FontConfigKey(language, fontType), out FontConfig value))
                return value;

            return this.CreateFallbackFontConfig(language, fontType);
        }

        private FontConfig CreateFallbackFontConfig(LanguageInfo language, GameFontType fontType)
        {
            if (language.IsLatinLanguage() && fontType == GameFontType.SpriteText)
                return this.FallbackLatinBmFontConfig(language, fontType);

            if (fontType != GameFontType.SpriteText)
                return new FontConfig(
                    Enabled: true,
                    FontFilePath: null,
                    FontIndex: 0,
                    FontSize: 26,
                    Spacing: 0,
                    LineSpacing: 26,
                    CharOffsetX: 0,
                    CharOffsetY: 0,
                    CharacterRanges: this._vanillaFontProvider.GetVanillaCharacterRanges(language, fontType));

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
                    CharacterRanges: this._vanillaFontProvider.GetVanillaCharacterRanges(language, fontType),
                    PixelZoom: FontHelpers.GetDefaultFontPixelZoom());
        }

        private FontConfig FallbackLatinBmFontConfig(LanguageInfo language, GameFontType fontType)
        {
            return new BmFontConfig(
                Enabled: true,
                FontFilePath: null,
                FontIndex: 0,
                FontSize: 16,
                Spacing: 0,
                LineSpacing: 16,
                CharOffsetX: 0,
                CharOffsetY: 0,
                CharacterRanges: this._vanillaFontProvider.GetVanillaCharacterRanges(language, fontType),
                PixelZoom: 3f);
        }
    }
}
