using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        public IEnumerable<FontPresetModel> Parse(FontContentPackItem settings, ISemanticVersion format, IContentPack sContentPack)
        {
            float size = ParseSize(settings.Size);
            IEnumerable<LanguageInfo> languages = this.ParseLanguage(settings.Language);
            IEnumerable<GameFontType> fontTypes = this.ParseFontType(settings.Type);

            CharacterPatchMode characterPatchMode = CharacterPatchMode.BasedOnOriginal;
            IEnumerable<CharacterRange> @override = null,
                                        add = null,
                                        remove = null;
            if (languages.Count() == 1)
            {
                if (settings.Character != null)
                {
                    @override = this.ParseCharacterRanges(settings.Character, sContentPack, nameof(settings.Character));
                    characterPatchMode = CharacterPatchMode.Override;
                }
                else
                {
                    if (settings.CharacterAdd != null)
                        add = this.ParseCharacterRanges(settings.CharacterAdd, sContentPack, nameof(settings.CharacterAdd));

                    if (settings.CharacterRemove != null)
                        remove = this.ParseCharacterRanges(settings.CharacterRemove, sContentPack, nameof(settings.CharacterRemove));
                }
            }

            // 多个语言
            else
            {
                // 仅允许重写，不允许修改。
                if (settings.Character != null)
                {
                    @override = this.ParseCharacterRanges(settings.Character, sContentPack, nameof(settings.Character));
                    characterPatchMode = CharacterPatchMode.Override;
                }
            }

            Func<string> name = this.ParseLocalizableField(settings.Name, sContentPack.Translation);
            Func<string> notes = this.ParseLocalizableField(settings.Notes, sContentPack.Translation);

            foreach (LanguageInfo language in languages)
            {
                foreach (GameFontType fontType in fontTypes)
                {
                    var context = new FontContext(language, fontType);

                    var basePreset = new FontPresetModel(
                        context: context,
                        settings: new FontConfigModel(
                            Enabled: true,
                            FontFile: settings.FontFile,
                            FontIndex: settings.Index,
                            FontSize: size,
                            Spacing: settings.Spacing,
                            LineSpacing: settings.LineSpacing,
                            CharOffsetX: settings.OffsetX,
                            CharOffsetY: settings.OffsetY,
                            PixelZoom: settings.PixelZoom,
                            CharacterPatchMode: characterPatchMode,
                            CharacterOverride: @override,
                            CharacterAdd: add,
                            CharacterRemove: remove,
                            DefaultCharacter: settings.DefaultCharacter,
                            Mask: settings.Mask));

                    yield return new FontPresetModelForContentPack(basePreset, sContentPack, name, notes);
                }
            }
        }

        private float ParseSize(float sizeField)
        {
            if (sizeField <= 0)
            {
                throw this.ParseFieldException(nameof(FontContentPackItem.Size),
                    $"Font size must be bigger than 0. Current: {sizeField}");
            }

            return sizeField;
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

            if (parsedLanguages.Count == 0)
            {
                throw this.ParseFieldException(nameof(FontContentPackItem.Language),
                    $"Cannot get any language from the given value. Value: '{languageField}'");
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

            if (parsedTypes.Count == 0)
            {
                throw this.ParseFieldException(nameof(FontContentPackItem.Type),
                    $"Cannot get any font type from the given value. Value: '{typeField}'");
            }

            return parsedTypes;
        }

        private IEnumerable<CharacterRange> ParseCharacterRanges(string characterFile, IContentPack sContentPack, string fieldName)
        {
            if (sContentPack.HasFile(characterFile))
            {
                IEnumerable<CharacterRange> ranges;

                if (this._characterFileHelper.TryParseJson(characterFile, sContentPack, out ranges, out Exception jsonEx))
                    return ranges;

                if (this._characterFileHelper.TryParseUnicode(characterFile, sContentPack, out ranges, out Exception uniEx))
                    return ranges;

                throw this.ParseFieldException(fieldName,
                    $"Failed to read the character file ('{characterFile}')"
                    + $"\nWhen attempts to read in json, the error is: {jsonEx}"
                    + $"\nWhen attempts to read in unicode, the error is: {uniEx}");
            }
            else
            {
                throw this.ParseFieldException(fieldName,
                    $"Cannot find character file in content pack's folder. File: '{characterFile}'");
            }
        }

        private Func<string> ParseLocalizableField(string field, ITranslationHelper translation)
        {
            field ??= string.Empty;

            Match match = Regex.Match(field, @"^{{[Ii]18[Nn]:(.*)}}$");
            if (match.Success)
            {
                string key = match.Groups[1].Value.Trim();
                return () => translation.Get(key);
            }
            else
            {
                return () => field;
            }
        }

        private Exception ParseFieldException(string fieldName, string message)
        {
            return new Exception($"Error parsing '{fieldName}' field: {message}");
        }
    }
}
