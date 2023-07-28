using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.DataAccess.Models;
using FontSettings.Framework.Models;

namespace FontSettings.Framework.DataAccess.Parsing
{
    internal class FontConfigParser
    {
        public KeyValuePair<FontContext, FontConfigModel> Parse(FontConfigData config)
        {
            var context = new FontContext(
                Language: new LanguageInfo(config.Lang, config.Locale),
                FontType: config.InGameType);

            var parsed = new FontConfigModel(
                Enabled: config.Enabled,
                FontFile: config.FontFilePath,
                FontIndex: config.FontIndex,
                FontSize: config.FontSize,
                Spacing: config.Spacing,
                LineSpacing: config.LineSpacing,
                CharOffsetX: config.CharOffsetX,
                CharOffsetY: config.CharOffsetY,
                PixelZoom: config.PixelZoom,
                CharacterPatchMode: config.CharacterRanges != null ? CharacterPatchMode.Override : CharacterPatchMode.BasedOnOriginal,
                CharacterOverride: config.CharacterRanges,
                CharacterAdd: null,
                CharacterRemove: null,
                DefaultCharacter: config.DefaultCharacter,
                Mask: config.Mask
            );

            return new(context, parsed);
        }

        public FontConfigData ParseBack(KeyValuePair<FontContext, FontConfigModel> config)
        {
            var language = config.Key.Language;
            var fontType = config.Key.FontType;
            var configValue = config.Value;

            var parsedBack = new FontConfigData();

            // common fields.
            parsedBack.Lang = language.Code;
            parsedBack.Locale = language.Locale;
            parsedBack.InGameType = fontType;
            parsedBack.Enabled = configValue.Enabled;
            parsedBack.FontFilePath = configValue.FontFile;
            parsedBack.FontIndex = configValue.FontIndex;
            parsedBack.FontSize = configValue.FontSize;
            parsedBack.Spacing = configValue.Spacing;
            parsedBack.LineSpacing = (int)configValue.LineSpacing;
            parsedBack.CharOffsetX = configValue.CharOffsetX;
            parsedBack.CharOffsetY = configValue.CharOffsetY;
            parsedBack.PixelZoom = configValue.PixelZoom;
            parsedBack.CharacterRanges = configValue.CharacterPatchMode switch
            {
                CharacterPatchMode.BasedOnOriginal => null,  // TODO: 有可能需要考虑，不过目前全是override的
                CharacterPatchMode.Override => configValue.CharacterOverride,
                _ => throw new NotSupportedException(),
            };
            parsedBack.DefaultCharacter = configValue.DefaultCharacter;
            parsedBack.Mask = configValue.Mask;

            return parsedBack;
        }

        public IDictionary<FontContext, FontConfigModel> ParseCollection(FontConfigs configs, LanguageInfo language, GameFontType fontType)
        {
            return this.ParseCollection(configs,
                 predicate: config => config.Lang == language.Code
                                     && config.Locale == language.Locale
                                     && config.InGameType == fontType);
        }

        public IDictionary<FontContext, FontConfigModel> ParseCollection(FontConfigs configs, Func<FontConfigData, bool> predicate)
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            return configs
                .Where(config => predicate(config))
                .Select(config => this.Parse(config))
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }
}
