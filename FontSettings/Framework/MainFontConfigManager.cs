using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.DataAccess.Parsing;
using FontSettings.Framework.Models;
using FontSettings.Framework.Preset;

namespace FontSettings.Framework
{
    internal class MainFontConfigManager : IFontConfigManager, IVanillaFontConfigProvider, IFontPresetManager
    {
        private readonly IDictionary<FontConfigKey, FontConfigModel> _fontConfigs = new Dictionary<FontConfigKey, FontConfigModel>();
        private readonly IDictionary<FontConfigKey, FontConfigModel> _vanillaConfigs = new Dictionary<FontConfigKey, FontConfigModel>();
        private readonly IDictionary<string, FontPresetModel> _keyedPresets = new Dictionary<string, FontPresetModel>();
        private readonly IList<FontPresetModel> _cpPresets = new List<FontPresetModel>();

        private readonly FontFilePathParseHelper _pathHelper = new();
        private readonly IFontFileProvider _fontFileProvider;
        private readonly IVanillaFontProvider _vanillaFontProvider;

        public event EventHandler<FontConfigUpdatedEventArgs> ConfigUpdated;
        public event EventHandler<PresetUpdatedEventArgs>? PresetUpdated;

        public MainFontConfigManager(IFontFileProvider fontFileProvider, IVanillaFontProvider vanillaFontProvider)
        {
            this._fontFileProvider = fontFileProvider;
            this._vanillaFontProvider = vanillaFontProvider;
        }

        /// <summary>Won't raise <see cref="ConfigUpdated"/>.</summary>
        public void AddFontConfig(KeyValuePair<FontConfigKey, FontConfigModel> config)
        {
            lock (this._fontConfigs)
            {
                this._fontConfigs.Add(config);
            }
        }

        /// <summary>Won't raise <see cref="ConfigUpdated"/>.</summary>
        public void AddFontConfig(FontConfigKey key, FontConfigModel config)
        {
            lock (this._fontConfigs)
            {
                this._fontConfigs[key] = config;
            }
        }

        public void UpdateFontConfig(LanguageInfo language, GameFontType fontType, FontConfig? config)
        {
            this.UpdateFontConfig(language, fontType, config, raiseConfigUpdated: true);
        }

        public bool TryGetFontConfig(LanguageInfo language, GameFontType fontType, out FontConfig? fontConfig)
        {
            lock (this._fontConfigs)
            {
                if (this._fontConfigs.TryGetValue(new FontConfigKey(language, fontType), out FontConfigModel model))
                {
                    fontConfig = this.MakeConfigObject(model, language, fontType);
                    return true;
                }
                else
                {
                    fontConfig = null;
                    return false;
                }
            }
        }

        public IDictionary<FontConfigKey, FontConfig> GetAllFontConfigs()
        {
            lock (this._fontConfigs)
            {
                return this._fontConfigs.ToDictionary(
                    pair => pair.Key,
                    pair => this.MakeConfigObject(pair.Value, pair.Key.Language, pair.Key.FontType));
            }
        }

        public void AddVanillaConfig(FontConfigKey key, FontConfigModel config)
        {
            lock (this._vanillaConfigs)
            {
                this._vanillaConfigs[key] = config;
            }
        }

        public FontConfig GetVanillaFontConfig(LanguageInfo language, GameFontType fontType)
        {
            lock (this._vanillaConfigs)
            {
                if (this._vanillaConfigs.TryGetValue(new FontConfigKey(language, fontType), out FontConfigModel model))
                {
                    return this.MakeConfigObject(model, language, fontType);
                }
            }

            return this.CreateFallbackFontConfig(language, fontType);
        }

        public void UpdatePreset(string name, FontPreset? preset)
        {
            this.UpdatePreset(name, preset, raisePresetUpdated: true);
        }

        public IEnumerable<FontPreset> GetPresets(LanguageInfo language, GameFontType fontType)
        {
            /* 确保SpriteText已经初始化完 */
            StardewValley.BellsAndWhistles.SpriteText.getWidthOfString(string.Empty);

            return from model in this.GetAllPresets()
                   where model.Context.Language == language && model.Context.FontType == fontType
                   select this.MakePresetObject(model);
        }

        public bool IsReadOnlyPreset(FontPreset preset)
        {
            if (preset is IPresetWithKey<string> withKey)
                return !this._keyedPresets.ContainsKey(withKey.Key);

            return true;
        }

        public bool IsValidPresetName(string name, out InvalidPresetNameTypes? invalidType)
        {
            invalidType = null;
            if (string.IsNullOrWhiteSpace(name))
                invalidType = InvalidPresetNameTypes.EmptyName;

            else if (this.ContainsInvalidChar(name))
                invalidType = InvalidPresetNameTypes.ContainsInvalidChar;

            else if (this.DuplicatePresetName(name))
                invalidType = InvalidPresetNameTypes.DuplicatedName;

            return invalidType == null;
        }

        /// <summary>Won't raise <see cref="PresetUpdated"/>.</summary>
        public void AddPresets(IEnumerable<FontPresetModel> presets)
        {
            foreach (var model in presets)
            {
                if (model.Supports<IPresetWithKey<string>>())
                {
                    var modelWithKey = model.GetInstance<IPresetWithKey<string>>();
                    this._keyedPresets[modelWithKey.Key] = model;
                }

                if (model.Supports<IPresetFromContentPack>())
                {
                    this._cpPresets.Add(model);
                }
            }
        }

        public void RemoveAllContentPacks()
        {
            this._cpPresets.Clear();
        }

        public void RemoveContentPacks(LanguageInfo language)
        {
            var toRemove = this._cpPresets.Where(preset => preset.Context.Language == language);
            foreach (FontPresetModel preset in toRemove)
            {
                this._cpPresets.Remove(preset);
            }
        }

        private void UpdateFontConfig(LanguageInfo language, GameFontType fontType, FontConfig? config, bool raiseConfigUpdated)
        {
            var key = new FontConfigKey(language, fontType);

            lock (this._fontConfigs)
            {
                if (!this._fontConfigs.ContainsKey(key))
                {
                    if (config != null)
                    {
                        var model = this.MakeConfigModel(config, language, fontType);
                        this._fontConfigs.Add(key, model);
                        if (raiseConfigUpdated)
                            this.RaiseConfigUpdated(key, model);
                    }
                }
                else
                {
                    FontConfigModel model;
                    if (config != null)
                        this._fontConfigs[key] = (model = this.MakeConfigModel(config, language, fontType));
                    else
                    {
                        this._fontConfigs.Remove(key);
                        model = null;
                    }

                    if (raiseConfigUpdated)
                        this.RaiseConfigUpdated(key, model);
                }
            }
        }

        private FontConfig MakeConfigObject(FontConfigModel model, LanguageInfo language, GameFontType fontType)
        {
            string fontFile = model.FontFile ?? this.GetVanillaFontFile(language, fontType);

            var config = new FontConfig(
                Enabled: model.Enabled,
                FontFilePath: this._pathHelper.ParseFontFilePath(fontFile, this._fontFileProvider.FontFiles),
                FontIndex: model.FontIndex,
                FontSize: model.FontSize,
                Spacing: model.Spacing,
                LineSpacing: model.LineSpacing,
                CharOffsetX: model.CharOffsetX,
                CharOffsetY: model.CharOffsetY,
                CharacterRanges: model.CharacterPatchMode switch
                {
                    CharacterPatchMode.BasedOnOriginal => this.PatchCharacterRanges(
                        original: this._vanillaFontProvider.GetVanillaCharacterRanges(language, fontType),
                        add: model.CharacterAdd,
                        remove: model.CharacterRemove),
                    CharacterPatchMode.Override => model.CharacterOverride ?? Array.Empty<CharacterRange>(),
                    _ => throw new NotSupportedException(),
                });

            if (fontType == GameFontType.SpriteText)
                config = new BmFontConfig(config, model.PixelZoom);

            return config;
        }

        private FontConfigModel MakeConfigModel(FontConfig config, LanguageInfo language, GameFontType fontType)
        {
            string fontFile = this._pathHelper.ParseBackFontFilePath(config.FontFilePath, this._fontFileProvider.FontFiles);
            string vFontFile = this.GetVanillaFontFile(language, fontType);
            if (fontFile == vFontFile)
                fontFile = null;

            var vanillaRanges = this._vanillaFontProvider.GetVanillaCharacterRanges(language, fontType);
            bool isOriginalRanges = FontHelpers.AreSameCharacterRanges(vanillaRanges, config.CharacterRanges);

            return new FontConfigModel(
                Enabled: config.Enabled,
                FontFile: fontFile,
                FontIndex: config.FontIndex,
                FontSize: config.FontSize,
                Spacing: config.Spacing,
                LineSpacing: config.LineSpacing,
                CharOffsetX: config.CharOffsetX,
                CharOffsetY: config.CharOffsetY,
                PixelZoom: config is BmFontConfig bmFont ? bmFont.PixelZoom : 0f,
                CharacterPatchMode: isOriginalRanges ? CharacterPatchMode.BasedOnOriginal : CharacterPatchMode.Override,
                CharacterOverride: isOriginalRanges ? null : config.CharacterRanges,
                CharacterAdd: null,
                CharacterRemove: null);
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

        private bool ContainsInvalidChar(string name)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            foreach (char c in name)
            {
                if (invalidChars.Contains(c))
                    return true;
            }

            return false;
        }

        private bool DuplicatePresetName(string name)
        {
            return this._keyedPresets.ContainsKey(name);
        }

        private void UpdatePreset(string name, FontPreset? preset, bool raisePresetUpdated)
        {
            if (!this._keyedPresets.ContainsKey(name))
            {
                if (preset != null)
                {
                    var model = this.MakePresetModel(preset);
                    this._keyedPresets.Add(name, model);

                    if (raisePresetUpdated)
                        this.RaisePresetUpdated(name, model);
                }
            }
            else
            {
                if (preset != null)
                {
                    var model = this.MakePresetModel(preset);
                    this._keyedPresets[name] = model;

                    if (raisePresetUpdated)
                        this.RaisePresetUpdated(name, model);
                }
                else
                {
                    this._keyedPresets.Remove(name);

                    if (raisePresetUpdated)
                        this.RaisePresetUpdated(name, null);
                }
            }
        }

        private IEnumerable<FontPresetModel> GetAllPresets()
        {
            foreach (var preset in this._keyedPresets.Values)
                yield return preset;
        }

        private FontPreset MakePresetObject(FontPresetModel model)
        {
            var settings = this.MakeConfigObject(model.Settings, model.Context.Language, model.Context.FontType);
            var basePreset = new FontPreset(model.Context, settings);

            switch (model)
            {
                case IPresetWithKey<string> modelWithKey:
                    return new FontPresetExtensible(basePreset) { Key = modelWithKey.Key };

                default:
                    return basePreset;
            }
        }

        private FontPresetModel MakePresetModel(FontPreset preset)
        {
            var settings = this.MakeConfigModel(preset.Settings, preset.Context.Language, preset.Context.FontType);
            return new FontPresetModel(preset.Context, settings);
        }

        private IEnumerable<CharacterRange> PatchCharacterRanges(
        IEnumerable<CharacterRange> original,
        IEnumerable<CharacterRange> add,
            IEnumerable<CharacterRange> remove)
        {
            var origCh = FontHelpers.GetCharacters(original);
            var addCh = FontHelpers.GetCharacters(add);
            var remCh = FontHelpers.GetCharacters(remove);

            var patchCh = origCh
                .Concat(addCh)
                .SkipWhile(c => remCh.Contains(c));

            return FontHelpers.GetCharacterRanges(patchCh);
        }

        private string? GetVanillaFontFile(LanguageInfo language, GameFontType fontType)
        {
            lock (this._vanillaConfigs)
            {
                var context = new FontContext(language, fontType);
                return this._vanillaConfigs[context]?.FontFile;
            }
        }

        private void RaiseConfigUpdated(FontConfigKey key, FontConfigModel config)
        {
            this.RaiseConfigUpdated(
                new FontConfigUpdatedEventArgs(key, config));
        }

        protected virtual void RaiseConfigUpdated(FontConfigUpdatedEventArgs e)
        {
            ConfigUpdated?.Invoke(this, e);
        }

        private void RaisePresetUpdated(string name, FontPresetModel? preset)
        {
            this.RaisePresetUpdated(
                new PresetUpdatedEventArgs(name, preset));
        }

        protected virtual void RaisePresetUpdated(PresetUpdatedEventArgs e)
        {
            PresetUpdated?.Invoke(this, e);
        }
    }

    internal class FontConfigUpdatedEventArgs : EventArgs
    {
        public FontConfigKey Key { get; }
        public FontConfigModel Config { get; }
        public FontConfigUpdatedEventArgs(FontConfigKey key, FontConfigModel config)
        {
            this.Key = key;
            this.Config = config;
        }
    }

    internal class PresetUpdatedEventArgs : EventArgs
    {
        public string Name { get; }
        public FontPresetModel? Preset { get; }
        public PresetUpdatedEventArgs(string name, FontPresetModel? preset)
        {
            this.Name = name;
            this.Preset = preset;
        }
    }
}
