using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.DataAccess.Models;
using FontSettings.Framework.Models;

namespace FontSettings.Framework.DataAccess.Parsing
{
    internal class FontPresetParser
    {
        private readonly FontConfigParser _settingsParser = new();

        public IEnumerable<FontPresetModel> Parse(FontPresetData preset)
        {
            var language = new LanguageInfo(preset.Lang, preset.Locale);
            var fontType = this.ParseFontType(preset.FontType);

            var settings = this._settingsParser.Parse(new FontConfigData
            {
                Enabled = true,
                Lang = preset.Lang,
                Locale = preset.Locale,
                InGameType = fontType,
                FontFilePath = preset.Requires.FontFileName,
                FontIndex = preset.FontIndex,
                FontSize = preset.FontSize,
                Spacing = preset.Spacing,
                LineSpacing = preset.LineSpacing,
                CharOffsetX = preset.CharOffsetX,
                CharOffsetY = preset.CharOffsetY,
                CharacterRanges = null,
                PixelZoom = preset.PixelZoom,
            }).Value;

            var basePreset = new FontPresetModel(new FontContext(language, fontType), settings);

            yield return new FontPresetModelLocal(basePreset, preset.Name);
        }

        public FontPresetData ParseBack(FontPresetModel preset)
        {
            var key = new FontConfigKey(preset.Context.Language, preset.Context.FontType);
            FontConfigData configData = this._settingsParser.ParseBack(new(key, preset.Settings));

            return new FontPresetData
            {
                Requires = new() { FontFileName = configData.FontFilePath },
                FontType = this.ParseBackFontType(configData.InGameType),
                FontIndex = configData.FontIndex,
                Lang = configData.Lang,
                Locale = configData.Locale,
                FontSize = configData.FontSize,
                Spacing = configData.Spacing,
                LineSpacing = configData.LineSpacing,
                CharOffsetX = configData.CharOffsetX,
                CharOffsetY = configData.CharOffsetY,
                PixelZoom = configData.PixelZoom,
            };
        }

        public IEnumerable<FontPresetModel> ParseCollection(IEnumerable<FontPresetData> presets)
        {
            return this.ParseCollection(presets,
                predicate: _ => true);
        }

        public IEnumerable<FontPresetModel> ParseCollection(IEnumerable<FontPresetData> presets, LanguageInfo language, GameFontType fontType)
        {
            return this.ParseCollection(presets,
                predicate: preset => preset.Lang == language.Code
                                    && preset.Locale == language.Locale
                                    && this.ParseFontType(preset.FontType) == fontType);
        }

        public IEnumerable<FontPresetModel> ParseCollection(IEnumerable<FontPresetData> presets, LanguageInfo language)
        {
            return this.ParseCollection(presets,
                predicate: preset => preset.Lang == language.Code
                                    && preset.Locale == language.Locale);
        }

        public IEnumerable<FontPresetModel> ParseCollection(IEnumerable<FontPresetData> presets, Func<FontPresetData, bool> predicate)
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            return presets
                .Where(preset => predicate(preset))
                .SelectMany(preset => this.Parse(preset));
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
    }
}
