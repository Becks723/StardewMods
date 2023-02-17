using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.DataAccess.Models;
using FontSettings.Framework.Models;
using FontSettings.Framework.Preset;

namespace FontSettings.Framework.DataAccess.Parsing
{
    internal class FontPresetParser
    {
        private readonly IFontFileProvider _fontFileProvider;
        private readonly IVanillaFontConfigProvider _vanillaFontConfigProvider;
        private readonly IVanillaFontProvider _vanillaFontProvider;

        private readonly FontFilePathParseHelper _fontFilePathParseHelper = new();

        public FontPresetParser(IFontFileProvider fontFileProvider, IVanillaFontConfigProvider vanillaFontConfigProvider, IVanillaFontProvider vanillaFontProvider)
        {
            this._fontFileProvider = fontFileProvider;
            this._vanillaFontConfigProvider = vanillaFontConfigProvider;
            this._vanillaFontProvider = vanillaFontProvider;
        }

        public IEnumerable<Preset.FontPreset> Parse(Models.FontPresetData preset)
        {
            var language = new LanguageInfo(preset.Lang, preset.Locale);
            var fontType = this.ParseFontType(preset.FontType);

            var settings = new FontConfig(
                Enabled: true,
                FontFilePath: this.ParseFontFilePath(preset.Requires.FontFileName, language, fontType),
                FontIndex: preset.FontIndex,
                FontSize: preset.FontSize,
                Spacing: preset.Spacing,
                LineSpacing: preset.LineSpacing,
                CharOffsetX: preset.CharOffsetX,
                CharOffsetY: preset.CharOffsetY,
                CharacterRanges: this._vanillaFontProvider.GetVanillaCharacterRanges(language, fontType));

            if (preset.PixelZoom != 0)
                settings = new BmFontConfig(
                    original: settings,
                    pixelZoom: preset.PixelZoom);

            var fontPreset = new Preset.FontPreset(
                language: language,
                fontType: fontType,
                settings: settings);

            // 目前的预设全是withKey的，而key就是name本身。
            fontPreset = new FontPresetWithKey(
                copy: new FontPresetWithName(fontPreset, preset.Name),
                key: preset.Name);

            yield return fontPreset;
        }

        public Models.FontPresetData ParseBack(Preset.FontPreset preset)
        {
            var font = preset.Settings;

            return new Models.FontPresetData
            {
                Requires = new() { FontFileName = this.ParseBackFontFilePath(font.FontFilePath, preset.Language, preset.FontType) },
                FontType = this.ParseBackFontType(preset.FontType),
                FontIndex = font.FontIndex,
                Lang = preset.Language.Code,
                Locale = preset.Language.Locale,
                FontSize = font.FontSize,
                Spacing = font.Spacing,
                LineSpacing = (int)font.LineSpacing,
                CharOffsetX = font.CharOffsetX,
                CharOffsetY = font.CharOffsetY,
                PixelZoom = font.Supports<IWithPixelZoom>()
                    ? font.GetInstance<IWithPixelZoom>().PixelZoom
                    : 0,
            };
        }

        private GameFontType ParseFontType(FontPresetFontType presetFontType)
        {
            if (presetFontType == FontPresetFontType.Any)
                presetFontType = FontPresetFontType.Small;

            return (GameFontType)(int)presetFontType;
        }

        private FontPresetFontType ParseBackFontType(GameFontType fontType)
        {
            return (FontPresetFontType)(int)fontType;
        }

        private string? ParseFontFilePath(string? path, LanguageInfo language, GameFontType fontType)
            => this._fontFilePathParseHelper.ParseFontFilePath(path, this._fontFileProvider.FontFiles, this._vanillaFontConfigProvider, language, fontType);

        private string? ParseBackFontFilePath(string? path, LanguageInfo language, GameFontType fontType)
            => this._fontFilePathParseHelper.ParseBackFontFilePath(path, this._fontFileProvider.FontFiles, this._vanillaFontConfigProvider, language, fontType);
    }
}
