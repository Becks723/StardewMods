using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.DataAccess.Models;
using FontSettings.Framework.Models;
using FontSettings.Framework.Preset;
using StardewModdingAPI;

namespace FontSettings.Framework.DataAccess.Parsing
{
    internal class ContentPackParser
    {
        private readonly ContentPackCharacterFileHelper _characterFileHelper = new();

        public IEnumerable<FontPresetModel> Parse(FontContentPack contentPack, IContentPack sContentPack)
        {
            IEnumerable<LanguageInfo> languages = this.ParseLanguage(contentPack.Language);
            IEnumerable<GameFontType> fontTypes = this.ParseFontType(contentPack.Type);

            CharacterPatchMode characterPatchMode = CharacterPatchMode.BasedOnOriginal;
            IEnumerable<CharacterRange> @override = null,
                                        add = null,
                                        remove = null;
            if (languages.Count() == 1)
            {
                if (contentPack.Character != null && sContentPack.HasFile(contentPack.Character))
                {
                    @override = this.ParseCharacterRanges(contentPack.Character, sContentPack);
                    characterPatchMode = CharacterPatchMode.Override;
                }
                else
                {
                    if (contentPack.CharacterAppend != null && sContentPack.HasFile(contentPack.CharacterAppend))
                        add = this.ParseCharacterRanges(contentPack.CharacterAppend, sContentPack);

                    if (contentPack.CharacterRemove != null && sContentPack.HasFile(contentPack.CharacterRemove))
                        remove = this.ParseCharacterRanges(contentPack.CharacterRemove, sContentPack);
                }
            }

            foreach (LanguageInfo language in languages)
            {
                foreach (GameFontType fontType in fontTypes)
                {
                    var context = new FontContext(language, fontType);

                    var basePreset = new FontPresetModel(
                        context: context,
                        settings: new FontConfigModel(
                            Enabled: true,
                            FontFile: contentPack.FontFile,
                            FontIndex: contentPack.Index,
                            FontSize: contentPack.Size,
                            Spacing: contentPack.Spacing,
                            LineSpacing: contentPack.LineSpacing,
                            CharOffsetX: contentPack.OffsetX,
                            CharOffsetY: contentPack.OffsetY,
                            PixelZoom: contentPack.PixelZoom,
                            CharacterPatchMode: characterPatchMode,
                            CharacterOverride: @override,
                            CharacterAdd: add,
                            CharacterRemove: remove));

                    yield return new FontPresetModelExtensible(basePreset, FontPresetModelExtensible.ExtendType.FromContentPack) { SContentPack = sContentPack };
                }
            }
        }

        private IEnumerable<LanguageInfo> ParseLanguage(string languageField)
        {
            if (string.IsNullOrWhiteSpace(languageField))
                return Array.Empty<LanguageInfo>();

            var parsedLanguages = new List<LanguageInfo>();
            foreach (string seg in languageField.Split(','))
            {
                string lang = seg.Trim();

                foreach (LanguageInfo language in FontHelpers.GetAllAvailableLanguages())
                {
                    if (parsedLanguages.Contains(language))
                        continue;

                    bool matched = lang.Equals(language.Locale, StringComparison.OrdinalIgnoreCase);
                    if (!language.IsModLanguage())
                        matched |= lang.Equals(language.Code.ToString(), StringComparison.OrdinalIgnoreCase);

                    if (matched)
                    {
                        parsedLanguages.Add(language);
                        break;
                    }
                }
            }
            return parsedLanguages;
        }

        private IEnumerable<GameFontType> ParseFontType(string typeField)
        {
            if (string.IsNullOrWhiteSpace(typeField))
                return Array.Empty<GameFontType>();

            var parsedTypes = new HashSet<GameFontType>();
            foreach (string seg in typeField.Split(','))
            {
                string type = seg.Trim();

                switch (type.ToLower())
                {
                    case "small":
                        parsedTypes.Add(GameFontType.SmallFont);
                        break;
                    case "medium":
                        parsedTypes.Add(GameFontType.DialogueFont);
                        break;

                    case "dialogue":
                        parsedTypes.Add(GameFontType.SpriteText);
                        break;
                }
            }
            return parsedTypes;
        }

        private IEnumerable<CharacterRange> ParseCharacterRanges(string characterFile, IContentPack sContentPack)
        {
            if (this._characterFileHelper.TryParseJson(characterFile, sContentPack, out IEnumerable<CharacterRange> ranges))
                return ranges;

            string fullPath = Path.Combine(sContentPack.DirectoryPath, characterFile);

            if (this._characterFileHelper.TryParsePlainText(fullPath, out ranges))
                return ranges;

            return null;
        }
    }
}
