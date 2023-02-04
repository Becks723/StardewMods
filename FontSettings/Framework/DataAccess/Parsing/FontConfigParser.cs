using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Models;

namespace FontSettings.Framework.DataAccess.Parsing
{
    internal class FontConfigParser
    {
        protected readonly IFontFileProvider _fontFileProvider;

        protected readonly FontFilePathParseHelper _fontFilePathParseHelper = new();

        public FontConfigParser(IFontFileProvider fontFileProvider)
        {
            this._fontFileProvider = fontFileProvider;
        }

        public KeyValuePair<FontConfigKey, FontConfig_> Parse(FontConfig config)
        {
            var key = new FontConfigKey(
                Language: new LanguageInfo(config.Lang, config.Locale),
                FontType: config.InGameType);

            var parsed = new FontConfig_(
                Enabled: config.Enabled,
                FontFilePath: this.ParseFontFilePath(config.FontFilePath, key.Language, key.FontType),
                FontIndex: config.FontIndex,
                FontSize: config.FontSize,
                Spacing: config.Spacing,
                LineSpacing: config.LineSpacing,
                CharOffsetX: config.CharOffsetX,
                CharOffsetY: config.CharOffsetY,
                CharacterRanges: config.CharacterRanges ?? CharRangeSource.GetBuiltInCharRange(key.Language)  // if null, use game built-in ranges.
            );

            // TODO: PixelZoom < 0 时 validate。
            if (config.PixelZoom != 0)
                parsed = new BmFontConfig(parsed, config.PixelZoom);

            return new(key, parsed);
        }

        public FontConfig ParseBack(KeyValuePair<FontConfigKey, FontConfig_> config)
        {
            var language = config.Key.Language;
            var fontType = config.Key.FontType;
            var configValue = config.Value;

            var parsedBack = new FontConfig();

            // common fields.
            parsedBack.Lang = language.Code;
            parsedBack.Locale = language.Locale;
            parsedBack.InGameType = fontType;
            parsedBack.Enabled = configValue.Enabled;
            parsedBack.FontIndex = configValue.FontIndex;
            parsedBack.FontSize = configValue.FontSize;
            parsedBack.Spacing = configValue.Spacing;
            parsedBack.LineSpacing = (int)configValue.LineSpacing;
            parsedBack.CharOffsetX = configValue.CharOffsetX;
            parsedBack.CharOffsetY = configValue.CharOffsetY;
            parsedBack.PixelZoom = configValue.Supports<IWithPixelZoom>()
                ? configValue.GetInstance<IWithPixelZoom>().PixelZoom
                : 0f;

            // FontFilePath
            string fontFilePath;
            {
                fontFilePath = this.ParseBackFontFilePath(configValue.FontFilePath, language, fontType);
            }
            parsedBack.FontFilePath = fontFilePath;

            // CharacterRanges
            IEnumerable<CharacterRange> ranges;
            {
                var vanillaRanges = CharRangeSource.GetBuiltInCharRange(language);
                var currentRanges = configValue.CharacterRanges;

                if (vanillaRanges == currentRanges)
                    ranges = null;
                else
                    ranges = currentRanges;
            }
            parsedBack.CharacterRanges = ranges;

            return parsedBack;
        }

        protected virtual string ParseFontFilePath(string path, LanguageInfo language, GameFontType fontType)
        {
            return this._fontFilePathParseHelper.ParseFontFilePath(path, this._fontFileProvider.FontFiles);
        }

        protected virtual string ParseBackFontFilePath(string path, LanguageInfo language, GameFontType fontType)
        {
            return this._fontFilePathParseHelper.ParseBackFontFilePath(path, this._fontFileProvider.FontFiles);
        }
    }
}
