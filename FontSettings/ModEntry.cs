using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BmFont;
using FontSettings.Framework;
using FontSettings.Framework.DataAccess;
using FontSettings.Framework.DataAccess.Models;
using FontSettings.Framework.DataAccess.Parsing;
using FontSettings.Framework.FontGenerators;
using FontSettings.Framework.FontPatching;
using FontSettings.Framework.FontScanning;
using FontSettings.Framework.FontScanning.Scanners;
using FontSettings.Framework.Menus;
using FontSettings.Framework.Menus.Views;
using FontSettings.Framework.Migrations;
using FontSettings.Framework.Models;
using FontSettings.Framework.Patchers;
using FontSettings.Framework.Preset;
using HarmonyLib;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData;
using StardewValley.Menus;

namespace FontSettings
{
    internal class ModEntry : Mod
    {
        private readonly string _globalFontDataKey = "font-data";
        private readonly string _const_fontPath_ja = "assets/fonts/sp-setofont/sp-setofont.ttf";
        private readonly string _const_fontPath_ko = "assets/fonts/SDMiSaeng/SDMiSaeng.ttf";
        private readonly string _const_fontPath_zh = "assets/fonts/NotoSansCJKsc-Bold/NotoSansCJKsc-Bold.otf";

        private MigrateTo_0_6_0 _0_6_0_Migration;

        private ModConfig _config;

        private FontManager _fontManager;

        private readonly Lazy<ContentManager> _contentManager = new(
            () => new ContentManager(
                GameRunner.instance.Content.ServiceProvider,
                GameRunner.instance.Content.RootDirectory));

        private FontConfigRepository _fontConfigRepository;
        private FontPresetRepository _fontPresetRepository;
        private VanillaFontDataRepository _vanillaFontDataRepository;
        private SampleDataRepository _sampleDataRepository;

        private FontConfigParserForUser _userFontConfigParser;
        private FontPresetParser _fontPresetParser;

        private FontConfigManager _fontConfigManager;
        private IVanillaFontConfigProvider _vanillaFontConfigProvider;
        private IFontFileProvider _fontFileProvider;
        private Framework.Preset.FontPresetManager _fontPresetManager;
        private IGameFontChangerFactory _fontChangerFactory;
        private FontPatchInvalidatorManager _invalidatorManager;

        private readonly FontSettingsMenuContextModel _menuContextModel = new();

        private MainFontPatcher _mainFontPatcher;

        internal static IModHelper ModHelper { get; private set; }

        internal static Harmony Harmony { get; private set; }

        public override void Entry(IModHelper helper)
        {
            ModHelper = this.Helper;
            I18n.Init(helper.Translation);
            Log.Init(this.Monitor);
            Textures.Init(helper.ModContent);

            this._config = helper.ReadConfig<ModConfig>();
            this.CheckConfigValid(this._config);

            foreach (var code in Enum.GetValues<LocalizedContentManager.LanguageCode>())
            {
                if (code == LocalizedContentManager.LanguageCode.th)
                    continue;

                if (code != LocalizedContentManager.LanguageCode.mod)
                    this.RecordVanillaFontData(code, FontHelpers.GetLocale(code));
            }

            // init migrations.
            this._0_6_0_Migration = new(helper, this.ModManifest);

            // init repositories.
            this._fontConfigRepository = new FontConfigRepository(helper);
            this._fontPresetRepository = new FontPresetRepository(Path.Combine(Constants.DataPath, ".smapi", "mod-data", this.ModManifest.UniqueID.ToLower(), "Presets"));
            this._vanillaFontDataRepository = new VanillaFontDataRepository(helper, this.Monitor);
            this._sampleDataRepository = new SampleDataRepository(helper, this.Monitor);

            // do changes to database.
            this._0_6_0_Migration.ApplyDatabaseChanges(
                fontConfigRepository: this._fontConfigRepository,
                fontPresetRepository: this._fontPresetRepository,
                modConfig: this._config,
                writeModConfig: this.SaveConfig);

            // init service objects.
            this._config.Sample = this._sampleDataRepository.ReadSampleData();
            this._vanillaFontConfigProvider = this.GetVanillaFontConfigProvider();
            this._fontFileProvider = this.GetFontFileProvider();

            this._userFontConfigParser = new FontConfigParserForUser(this._fontFileProvider, this._vanillaFontConfigProvider);
            this._fontPresetParser = new FontPresetParser(this._fontFileProvider, this._vanillaFontConfigProvider);

            this._fontConfigManager = this.GetFontConfigManager(this._userFontConfigParser);
            this._fontPresetManager = this.GetFontPresetManager(this._fontPresetParser);
            this._invalidatorManager = new FontPatchInvalidatorManager(helper);
            this._fontChangerFactory = new FontPatchChangerFactory(new FontPatchResolverFactory(), this._invalidatorManager);

            // connect manager and repository.
            this._fontConfigManager.ConfigUpdated += this.OnFontConfigUpdated;
            this._fontPresetManager.PresetUpdated += this.OnFontPresetUpdated;

            // init font patching.
            this._mainFontPatcher = new MainFontPatcher(this._fontConfigManager, new FontPatchResolverFactory(), this._invalidatorManager);

            this.AssertModFileExists(this._const_fontPath_ja, out _);
            this.AssertModFileExists(this._const_fontPath_ko, out _);
            this.AssertModFileExists(this._const_fontPath_zh, out _);

            this._fontManager ??= new(helper.ModContent);

            Harmony = new Harmony(this.ModManifest.UniqueID);
            {
                new GameMenuPatcher()
                    .AddFontSettingsPage(helper, Harmony, this._config, this.SaveConfig);

                new FontShadowPatcher(this._config)
                    .Patch(Harmony, this.Monitor);
            }

            helper.Events.Content.AssetRequested += this.OnAssetRequested;
            helper.Events.Content.AssetReady += this.OnAssetReady;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
        }

        IVanillaFontConfigProvider GetVanillaFontConfigProvider()
        {
            var modFontFileProvider = new FontFileProvider();
            modFontFileProvider.Scanners.Add(new BasicFontFileScanner(this.Helper.DirectoryPath, new ScanSettings()));

            var parser = new FontConfigParser(modFontFileProvider);
            var vanillaFonts = this._vanillaFontDataRepository
                .ReadVanillaFontData().Fonts
                .Select(font => parser.Parse(font))
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            return new VanillaFontConfigProvider(vanillaFonts);
        }
        FontConfigManager GetFontConfigManager(FontConfigParserForUser parser)
        {
            var vanillaFonts = this._fontConfigRepository.ReadAllConfigs()
                .Select(font => parser.Parse(font))
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            return new FontConfigManager(vanillaFonts);
        }

        IFontFileProvider GetFontFileProvider()
        {
            var fontFileProvider = new FontFileProvider();
            {
                var scanSettings = new ScanSettings();
                fontFileProvider.Scanners.Add(new InstalledFontScannerForWindows(scanSettings));
                fontFileProvider.Scanners.Add(new InstalledFontScannerForMacOS(scanSettings));
                fontFileProvider.Scanners.Add(new InstalledFontScannerForLinux(scanSettings));
            }
            return fontFileProvider;
        }
        Framework.Preset.FontPresetManager GetFontPresetManager(FontPresetParser parser)
        {
            var presets = this._fontPresetRepository.ReadAllPresets()
                .SelectMany(pair => parser.Parse(pair.Value));

            return new Framework.Preset.FontPresetManager(presets);
        }
        private void OnFontConfigUpdated(object sender, EventArgs e)
        {
            var configs = this._fontConfigManager.GetAllFontConfigs()
                .Select(pair => this._userFontConfigParser.ParseBack(pair));

            var configObject = new FontConfigs();
            foreach (var config in configs)
                configObject.Add(config);

            this._fontConfigRepository.WriteAllConfigs(configObject);
        }
        private void OnFontPresetUpdated(object sender, PresetUpdatedEventArgs e)
        {
            string name = e.Name;
            var preset = e.Preset;

            var presetObject = preset == null
                ? null
                : this._fontPresetParser.ParseBack(preset);

            this._fontPresetRepository.WritePreset(name, presetObject);
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            new GMCMIntegration(
                config: this._config,
                reset: this.ResetConfig,
                save: () => this.SaveConfig(this._config),
                modRegistry: this.Helper.ModRegistry,
                monitor: this.Monitor,
                manifest: this.ModManifest
            ).Integrate();
        }

        private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (this._config.OpenFontSettingsMenu.JustPressed())
            {
                this.OpenFontSettingsMenu();
            }
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            this._mainFontPatcher.OnAssetRequested(e);
        }

        private void OnAssetReady(object sender, AssetReadyEventArgs e)
        {
            this._mainFontPatcher.OnAssetReady(e);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this._fontManager.Dispose();
        }

        private void RecordVanillaFontData(LocalizedContentManager.LanguageCode languageCode, string locale)
        {
            string DisplayLanguage()
            {
                if (languageCode is LocalizedContentManager.LanguageCode.en && locale == string.Empty)
                    return "en";
                return locale;
            }

            LanguageInfo langInfo = new LanguageInfo(languageCode, locale);
            string langStr = DisplayLanguage();

            if (this.HasCached(langInfo))
            {
                this.Monitor.Log($"无需记录{langStr}语言下的游戏字体数据！");
            }
            else
            {
                this.Monitor.Log($"正在记录{langStr}语言下的游戏字体数据……");

                var contentManager = this._contentManager.Value;
                SpriteFont smallFont;
                SpriteFont dialogueFont;
                try
                {
                    smallFont = contentManager.Load<SpriteFont>(FontHelpers.LocalizeAssetName("Fonts/SmallFont", languageCode, locale));
                }
                catch (ContentLoadException)
                {
                    smallFont = contentManager.Load<SpriteFont>("Fonts/SmallFont");
                }

                try
                {
                    dialogueFont = contentManager.Load<SpriteFont>(FontHelpers.LocalizeAssetName("Fonts/SpriteFont1", languageCode, locale));
                }
                catch (ContentLoadException)
                {
                    dialogueFont = contentManager.Load<SpriteFont>("Fonts/SpriteFont1");
                }
                GameBitmapSpriteFont spriteText = this.LoadGameBmFont(languageCode);

                // 记录内置字体。
                this._fontManager ??= new(this.Helper.ModContent);
                this._fontManager.RecordBuiltInSpriteFont(langInfo, GameFontType.SmallFont, smallFont);
                this._fontManager.RecordBuiltInSpriteFont(langInfo, GameFontType.DialogueFont, dialogueFont);
                this._fontManager.RecordBuiltInBmFont(langInfo, spriteText);

                // 记录字符范围，加载字体要用。
                CharRangeSource.RecordBuiltInCharRange(langInfo, smallFont);

                this.Monitor.Log($"已完成记录{langStr}语言下的游戏字体数据！");
                this.SetHasCached(langInfo);
            }
        }

        private void RecordVanillaModFontData(string locale, ModLanguage modLanguage)
        {
            LanguageInfo langInfo = new LanguageInfo(LocalizedContentManager.LanguageCode.mod, locale);
            string langStr = locale;

            if (this.HasCached(langInfo))
            {
                this.Monitor.Log($"无需记录{langStr}语言下的游戏字体数据！");
            }
            else
            {
                this.Monitor.Log($"正在记录{langStr}语言下的游戏字体数据……");

                SpriteFont smallFont = this.Helper.GameContent.Load<SpriteFont>($"Fonts/SmallFont.{locale}");
                SpriteFont dialogueFont = this.Helper.GameContent.Load<SpriteFont>($"Fonts/SpriteFont1.{locale}");
                GameBitmapSpriteFont spriteText = this.LoadModBmFont(modLanguage);

                // 记录内置字体。
                this._fontManager.RecordBuiltInSpriteFont(langInfo, GameFontType.SmallFont, smallFont);
                this._fontManager.RecordBuiltInSpriteFont(langInfo, GameFontType.DialogueFont, dialogueFont);
                this._fontManager.RecordBuiltInBmFont(langInfo, spriteText);

                // 记录字符范围，加载字体要用。
                CharRangeSource.RecordBuiltInCharRange(langInfo, smallFont);

                this.Monitor.Log($"已完成记录{langStr}语言下的游戏字体数据！");
                this.SetHasCached(langInfo);
            }
        }

        private GameBitmapSpriteFont LoadGameBmFont(LocalizedContentManager.LanguageCode languageCode)
        {
            string fntPath = languageCode switch
            {
                LocalizedContentManager.LanguageCode.ja => "Fonts/Japanese",
                LocalizedContentManager.LanguageCode.ru => "Fonts/Russian",
                LocalizedContentManager.LanguageCode.zh => "Fonts/Chinese",
                LocalizedContentManager.LanguageCode.ko => "Fonts/Korean",
                _ => null
            };

            if (fntPath != null)
            {
                var contentManager = this._contentManager.Value;

                FontFile fontFile = FontLoader.Parse(contentManager.Load<XmlSource>(fntPath).Source);
                List<Texture2D> pages = new List<Texture2D>(fontFile.Pages.Count);
                foreach (FontPage fontPage in fontFile.Pages)
                {
                    string assetName = $"Fonts/{fontPage.File}";
                    pages.Add(contentManager.Load<Texture2D>(assetName));
                }

                return new GameBitmapSpriteFont()
                {
                    FontFile = fontFile,
                    Pages = pages,
                    LanguageCode = languageCode
                };
            }

            return null;
        }

        private GameBitmapSpriteFont LoadModBmFont(ModLanguage modLanguage)
        {
            string fontFile;
            {
                if (!modLanguage.UseLatinFont)
                    fontFile = modLanguage.FontFile;
                else
                    fontFile = null;
            }

            if (fontFile != null)
            {
                var contentManager = this.Helper.GameContent;

                FontFile font = FontLoader.Parse(contentManager.Load<XmlSource>(fontFile).Source);
                List<Texture2D> pages = new List<Texture2D>(font.Pages.Count);
                foreach (FontPage fontPage in font.Pages)
                {
                    string assetName = $"Fonts/{fontPage.File}";
                    pages.Add(contentManager.Load<Texture2D>(assetName));
                }

                return new GameBitmapSpriteFont()
                {
                    FontFile = font,
                    Pages = pages,
                    LanguageCode = LocalizedContentManager.LanguageCode.mod
                };
            }

            return null;
        }

        /// <summary>记录不同语言下字体信息是否已经缓存了。</summary>
        private readonly IDictionary<LanguageInfo, bool> _cacheTable = new Dictionary<LanguageInfo, bool>();
        private bool HasCached(LanguageInfo lang)
        {
            // 如果不存在此键，创建并返回false。
            if (this._cacheTable.TryGetValue(lang, out bool cached))
                return cached;

            this._cacheTable.Add(lang, false);
            return false;
        }
        private void SetHasCached(LanguageInfo lang)
        {
            if (this._cacheTable.ContainsKey(lang))
                this._cacheTable[lang] = true;
        }

        private FontConfigs ReadFontSettings()
        {
            FontConfigs fonts = this.Helper.Data.ReadGlobalData<FontConfigs>(this._globalFontDataKey);
            if (fonts == null)
            {
                fonts = new FontConfigs();
                this.Helper.Data.WriteGlobalData(this._globalFontDataKey, fonts);
            }

            return fonts;
        }

        private void SaveConfig(ModConfig config)
        {
            this.Helper.WriteConfig(config);
        }

        private void SaveFontSettings(FontConfigs fonts)
        {
            this.Helper.Data.WriteGlobalData(this._globalFontDataKey, fonts);
        }

        private void ResetConfig()
        {
            // 重置
            this._config.ResetToDefault();

            // 保存
            this.Helper.WriteConfig(this._config);
        }

        private void CheckConfigValid(ModConfig config)
        {
            string WarnMessage<T>(string name, T max, T min) => $"{name}：最大值（{max}）小于最小值（{min}）。已重置。";

            // x offset
            if (config.MaxCharOffsetX < config.MinCharOffsetX)
            {
                ILog.Warn(WarnMessage("横轴偏移量", config.MaxCharOffsetX, config.MinCharOffsetX));
                config.MaxCharOffsetX = 10;
                config.MinCharOffsetX = -10;
            }

            // y offset
            if (config.MaxCharOffsetY < config.MinCharOffsetY)
            {
                ILog.Warn(WarnMessage("纵轴偏移量", config.MaxCharOffsetY, config.MinCharOffsetY));
                config.MaxCharOffsetY = 10;
                config.MinCharOffsetY = -10;
            }

            // font size
            if (config.MaxFontSize < config.MinFontSize)
            {
                ILog.Warn(WarnMessage("字体大小", config.MaxFontSize, config.MinFontSize));
                config.MaxFontSize = 100;
            }

            // spacing
            if (config.MaxSpacing < config.MinSpacing)
            {
                ILog.Warn(WarnMessage("字间距", config.MaxSpacing, config.MinSpacing));
                config.MaxSpacing = 10;
                config.MinSpacing = -10;
            }

            // line spacing
            if (config.MaxLineSpacing < config.MinLineSpacing)
            {
                ILog.Warn(WarnMessage("行间距", config.MaxLineSpacing, config.MinLineSpacing));
                config.MaxLineSpacing = 100;
            }

            // pixel zoom
            if (config.MaxPixelZoom < config.MinPixelZoom)
            {
                ILog.Warn(WarnMessage("缩放比例", config.MaxPixelZoom, config.MinPixelZoom));
                config.MaxPixelZoom = 5f;
                config.MinPixelZoom = 0.1f;
            }
        }

        private void OpenFontSettingsMenu()
        {
            var gen = new SampleFontGenerator(this._fontManager);
            IFontGenerator sampleFontGenerator = gen;
            IAsyncFontGenerator sampleAsyncFontGenerator = gen;

            Game1.activeClickableMenu = new FontSettingsPage(
                config: this._config,
                fontManager: this._fontManager,
                sampleFontGenerator: sampleFontGenerator,
                sampleAsyncFontGenerator: sampleAsyncFontGenerator,
                presetManager: this._fontPresetManager,
                registry: this.Helper.ModRegistry,
                fontConfigManager: this._fontConfigManager,
                vanillaFontConfigProvider: this._vanillaFontConfigProvider,
                fontChangerFactory: this._fontChangerFactory,
                fontFileProvider: this._fontFileProvider,
                stagedValues: this._menuContextModel);
        }

        private bool AssertModFileExists(string relativePath, out string? fullPath) // fullPath = null when returns false
        {
            fullPath = Path.Combine(this.Helper.DirectoryPath, relativePath);
            fullPath = PathUtilities.NormalizePath(fullPath);

            if (this.AssertModFileExists(fullPath))
                return true;
            else
            {
                fullPath = null;
                return false;
            }
        }

        private bool AssertModFileExists(string relativePath)
        {
            string fullPath = Path.Combine(this.Helper.DirectoryPath, relativePath);
            fullPath = PathUtilities.NormalizePath(fullPath);

            return this.AssertFileExists(fullPath, I18n.Misc_ModFileNotFound(relativePath));
        }

        private bool AssertFileExists(string filePath, string message)
        {
            if (!File.Exists(filePath))
            {
                this.Monitor.Log(message, LogLevel.Error);
                return false;
            }
            return true;
        }
    }
}
