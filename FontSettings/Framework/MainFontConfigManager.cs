using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.DataAccess.Parsing;
using FontSettings.Framework.FontScanning;
using FontSettings.Framework.FontScanning.Scanners;
using FontSettings.Framework.Models;
using FontSettings.Framework.Preset;
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace FontSettings.Framework
{
    internal class MainFontConfigManager : IFontConfigManager, IVanillaFontConfigProvider, IFontPresetManager
    {
        private readonly IDictionary<FontContext, FontConfigModel?> _fontConfigs = new Dictionary<FontContext, FontConfigModel?>();
        private readonly IDictionary<FontContext, FontConfigModel?> _vanillaConfigs = new Dictionary<FontContext, FontConfigModel?>();
        private readonly IDictionary<string, FontPresetModel> _keyedPresets = new Dictionary<string, FontPresetModel>();
        private readonly IList<FontPresetModel> _cpPresets = new List<FontPresetModel>();

        private readonly FontFilePathParseHelper _pathHelper = new();
        private readonly IFontFileProvider _fontFileProvider;
        private readonly IVanillaFontProvider _vanillaFontProvider;
        private readonly IDictionary<IContentPack, IFontFileProvider> _cpFontFileProviders;

        public event EventHandler<FontConfigUpdatedEventArgs> ConfigUpdated;
        public event EventHandler<PresetUpdatedEventArgs>? PresetUpdated;

        public MainFontConfigManager(IFontFileProvider fontFileProvider, IVanillaFontProvider vanillaFontProvider, IDictionary<IContentPack, IFontFileProvider> cpFontFileProviders)
        {
            this._fontFileProvider = fontFileProvider;
            this._vanillaFontProvider = vanillaFontProvider;
            this._cpFontFileProviders = cpFontFileProviders;
        }

        /// <summary>Won't raise <see cref="ConfigUpdated"/>.</summary>
        public void AddFontConfig(FontContext context, FontConfigModel? config)
        {
            lock (this._fontConfigs)
            {
                this._fontConfigs[context] = config;
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
                if (this._fontConfigs.TryGetValue(new(language, fontType), out FontConfigModel? model))
                {
                    if (model != null)
                    {
                        fontConfig = this.MakeConfigObject(model, language, fontType);
                        return true;
                    }
                }

                fontConfig = null;
                return false;
            }
        }

        public IDictionary<FontContext, FontConfig> GetAllFontConfigs()
        {
            lock (this._fontConfigs)
            {
                return this._fontConfigs.Where(pair => pair.Value != null)
                    .ToDictionary(
                        pair => pair.Key,
                        pair => this.MakeConfigObject(pair.Value, pair.Key.Language, pair.Key.FontType));
            }
        }

        public void AddVanillaConfig(FontContext context, FontConfigModel? config)
        {
            lock (this._vanillaConfigs)
            {
                this._vanillaConfigs[context] = config;
            }
        }

        public FontConfig GetVanillaFontConfig(LanguageInfo language, GameFontType fontType)
        {
            lock (this._vanillaConfigs)
            {
                if (this._vanillaConfigs.TryGetValue(new(language, fontType), out FontConfigModel? model))
                {
                    if (model != null)
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
            if (preset.TryGetInstance(out IPresetWithKey<string> presetWithKey))
            {
                return !this._keyedPresets.ContainsKey(presetWithKey.Key);
            }

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
                if (model.TryGetInstance(out IPresetWithKey<string> modelWithKey))
                {
                    this._keyedPresets[modelWithKey.Key] = model;
                }

                if (model.TryGetInstance(out IPresetFromContentPack _))
                {
                    this._cpPresets.Add(model);
                }
            }
        }

        public void RemoveAllContentPacks()
        {
            this._cpPresets.Clear();
        }

        private void UpdateFontConfig(LanguageInfo language, GameFontType fontType, FontConfig? config, bool raiseConfigUpdated)
        {
            var context = new FontContext(language, fontType);

            lock (this._fontConfigs)
            {
                if (!this._fontConfigs.ContainsKey(context))
                {
                    if (config != null)
                    {
                        var model = this.MakeConfigModel(config, language, fontType);
                        this._fontConfigs.Add(context, model);
                        if (raiseConfigUpdated)
                            this.RaiseConfigUpdated(context, model);
                    }
                }
                else
                {
                    FontConfigModel model;
                    if (config != null)
                        this._fontConfigs[context] = (model = this.MakeConfigModel(config, language, fontType));
                    else
                    {
                        this._fontConfigs.Remove(context);
                        model = null;
                    }

                    if (raiseConfigUpdated)
                        this.RaiseConfigUpdated(context, model);
                }
            }
        }

        private FontConfig MakeConfigObject(FontConfigModel model, LanguageInfo language, GameFontType fontType)
        {
            var builder = new FontConfigBuilder();

            string fontFile = model.FontFile ?? this.GetVanillaFontFile(language, fontType);

            var config = new FontConfig(
                Enabled: model.Enabled,
                FontFilePath: this._pathHelper.ParseFontFilePath(fontFile, this.YieldAllPossibleFontFiles()),
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

            builder.BasicConfig(config);

            if (fontType != GameFontType.SpriteText)
                builder.WithDefaultCharacter(model.DefaultCharacter);

            if (fontType == GameFontType.SpriteText)
                builder.WithPixelZoom(model.PixelZoom);

            builder.WithSolidColorMask(model.Mask);

            return builder.Build();
        }

        private FontConfigModel MakeConfigModel(FontConfig config, LanguageInfo language, GameFontType fontType)
        {
            string fontFile = this._pathHelper.ParseBackFontFilePath(config.FontFilePath, this.YieldAllPossibleFontFiles());
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
                PixelZoom: config.TryGetInstance(out IWithPixelZoom withPixelZoom) ? withPixelZoom.PixelZoom : 0f,
                CharacterPatchMode: isOriginalRanges ? CharacterPatchMode.BasedOnOriginal : CharacterPatchMode.Override,
                CharacterOverride: isOriginalRanges ? null : config.CharacterRanges,
                CharacterAdd: null,
                CharacterRemove: null,
                DefaultCharacter: config.TryGetInstance(out IWithDefaultCharacter withDefaultCharacter)
                    ? withDefaultCharacter.DefaultCharacter : '*',
                Mask: config.TryGetInstance(out IWithSolidColor withSolidColor)
                    ? withSolidColor.SolidColor : Color.White);
        }

        private FontConfig CreateFallbackFontConfig(LanguageInfo language, GameFontType fontType)
        {
            if (language.IsLatinLanguage() && fontType == GameFontType.SpriteText)
                return this.FallbackLatinBmFontConfig(language, fontType);

            var builder = new FontConfigBuilder();

            builder.BasicConfig(new FontConfig(
                Enabled: true,
                FontFilePath: null,
                FontIndex: 0,
                FontSize: 26,
                Spacing: 0,
                LineSpacing: 26,
                CharOffsetX: 0,
                CharOffsetY: 0,
                CharacterRanges: this._vanillaFontProvider.GetVanillaCharacterRanges(language, fontType)));

            if (fontType != GameFontType.SpriteText)
                builder.WithDefaultCharacter('*');

            if (fontType == GameFontType.SpriteText)
                builder.WithPixelZoom(FontHelpers.GetDefaultFontPixelZoom());

            return builder.Build();
        }

        private FontConfig FallbackLatinBmFontConfig(LanguageInfo language, GameFontType fontType)
        {
            return new FontConfigBuilder()
                .BasicConfig(new FontConfig(
                    Enabled: true,
                    FontFilePath: null,
                    FontIndex: 0,
                    FontSize: 16,
                    Spacing: 0,
                    LineSpacing: 16,
                    CharOffsetX: 0,
                    CharOffsetY: 0,
                    CharacterRanges: this._vanillaFontProvider.GetVanillaCharacterRanges(language, fontType)))
                .WithPixelZoom(3f)
                .Build();
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
                if (preset != null)
                    yield return preset;

            foreach (var preset in this._cpPresets)
                if (preset != null)
                    yield return preset;
        }

        private FontPreset MakePresetObject(FontPresetModel model)
        {
            var settings = this.MakeConfigObject(model.Settings, model.Context.Language, model.Context.FontType);
            var basePreset = new FontPreset(model.Context, settings);

            var builder = new FontPresetBuilder()
                .BasicPreset(basePreset);
            if (model.TryGetInstance(out IPresetWithName withName))
                builder.WithName(withName.Name);
            if (model.TryGetInstance(out IPresetWithDescription withDesc))
                builder.WithDescription(withDesc.Description);
            if (model.TryGetInstance(out IPresetWithKey<string> withKey))
                builder.WithKey(withKey.Key);
            if (model.TryGetInstance(out IPresetFromContentPack fcp))
                builder.FromContentPack(fcp.SContentPack);
            return builder.Build();
        }

        private FontPresetModel MakePresetModel(FontPreset preset)
        {
            var settings = this.MakeConfigModel(preset.Settings,
                preset.Context.Language, preset.Context.FontType);
            var basicModel = new FontPresetModel(preset.Context, settings);

            if (preset.TryGetInstance(out IPresetFromContentPack fcp))
            {
                return new FontPresetModelForContentPack(basicModel, fcp.SContentPack,
                    preset.TryGetInstance(out IPresetWithName withName)
                        ? () => withName.Name
                        : () => string.Empty,
                    preset.TryGetInstance(out IPresetWithDescription withDesc)
                        ? () => withDesc.Description
                        : () => string.Empty);
            }
            else if (preset.TryGetInstance(out IPresetWithKey<string> withKey))
            {
                return new FontPresetModelLocal(basicModel, withKey.Key);
            }
            else
                return basicModel;
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

        private IEnumerable<string> YieldAllPossibleFontFiles()
        {
            return this._fontFileProvider.FontFiles
                .Concat(
                    this._cpFontFileProviders.Values.SelectMany(provider => provider.FontFiles)
                );
        }

        private void RaiseConfigUpdated(FontContext context, FontConfigModel config)
        {
            this.RaiseConfigUpdated(
                new FontConfigUpdatedEventArgs(context, config));
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
        public FontContext Context { get; }
        public FontConfigModel Config { get; }
        public FontConfigUpdatedEventArgs(FontContext context, FontConfigModel config)
        {
            this.Context = context;
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
