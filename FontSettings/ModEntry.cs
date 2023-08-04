using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BmFont;
using FontSettings.Framework;
using FontSettings.Framework.DataAccess;
using FontSettings.Framework.DataAccess.Models;
using FontSettings.Framework.DataAccess.Parsing;
using FontSettings.Framework.Exporting;
using FontSettings.Framework.FontGenerators;
using FontSettings.Framework.FontInfo;
using FontSettings.Framework.FontPatching;
using FontSettings.Framework.FontPatching.Invalidators;
using FontSettings.Framework.FontScanning;
using FontSettings.Framework.FontScanning.Scanners;
using FontSettings.Framework.Integrations;
using FontSettings.Framework.Menus;
using FontSettings.Framework.Menus.ViewModels;
using FontSettings.Framework.Menus.Views;
using FontSettings.Framework.Migrations;
using FontSettings.Framework.Models;
using FontSettings.Framework.Patchers;
using FontSettings.Framework.Preset;
using HarmonyLib;
using Microsoft.Xna.Framework;
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
        private ModConfig _config;
        private ModConfigWatcher _configWatcher;

        private FontConfigRepository _fontConfigRepository;
        private FontPresetRepository _fontPresetRepository;
        private VanillaFontDataRepository _vanillaFontDataRepository;
        private SampleDataRepository _sampleDataRepository;
        private ContentPackRepository _contentPackRepository;

        private MainFontConfigManager _fontConfigManager;
        private IFontFileProvider _fontFileProvider;
        private readonly IDictionary<IContentPack, IFontFileProvider> _cpFontFileProviders = new Dictionary<IContentPack, IFontFileProvider>();

        private readonly ISet<LanguageInfo> _languagesWhoseDataIsLoaded = new HashSet<LanguageInfo>();

        private readonly FontSettingsMenuContextModel _menuContextModel = new();

        private MainFontPatcher _mainFontPatcher;

        private FontPatchChanger _fontChanger;

        private VanillaFontProvider _vanillaFontProvider;

        private readonly FontExporter _exporter = new();

        private readonly DataAdditionalLanguagesWatcher _dataAdditionalLanguagesWatcher = new();

        private TitleFontButton _titleFontButton;

        internal static IModHelper ModHelper { get; private set; }

        internal static Harmony Harmony { get; private set; }

        public override void Entry(IModHelper helper)
        {
            ModHelper = this.Helper;
            I18n.Init(helper.Translation);
            Log.Init(this.Monitor);
            Textures.Init(this.ModManifest);
            StardewValleyUI.EntryPoint.Main();

            this._config = helper.ReadConfig<ModConfig>();
            this._config.ValidateValues(this.Monitor);
            this._configWatcher = new ModConfigWatcher(this._config);

            // init vanilla font provider.
            this._vanillaFontProvider = new VanillaFontProvider(helper, this.Monitor, this._config);
            this._vanillaFontProvider.RecordStarted += this.OnFontRecordStarted;
            this._vanillaFontProvider.RecordFinished += this.OnFontRecordFinished;

            string presetDirectory = Path.Combine(Constants.DataPath, ".smapi", "mod-data", this.ModManifest.UniqueID.ToLower(), "Presets");

            // do migrations
            {
                FontConfigRepository configRepo = new FontConfigRepository(helper);
                FontPresetRepository presetRepo = new FontPresetRepository(presetDirectory);

                new MigrateTo_0_6_0(helper, this.ModManifest)
                    .ApplyDatabaseChanges(
                        fontConfigRepository: configRepo,
                        fontPresetRepository: presetRepo,
                        modConfig: this._config,
                        writeModConfig: this.SaveConfig);
                new MigrateTo_0_12_0(helper)
                    .ApplyDatabaseChanges(
                        fontConfigRepository: configRepo,
                        presetRepository: presetRepo);
            }

            // init service objects.
            this._fontFileProvider = new FontFileProvider(this.YieldFontScanners());
            this.ReloadCpFontFileProviders();

            // init repositories.
            this._vanillaFontDataRepository = new VanillaFontDataRepository(helper, this.Monitor);
            this._fontConfigRepository = new FontConfigRepository(helper, this.Monitor);
            this._fontPresetRepository = new FontPresetRepository(presetDirectory, this.Monitor);
            this._contentPackRepository = new ContentPackRepository(helper.ContentPacks, this.Monitor, this.EnumerateTestContentPacks());
            this._sampleDataRepository = new SampleDataRepository(helper, this.Monitor);
            this._config.Sample = this._sampleDataRepository.ReadSampleData();

            // init managers.
            this._fontConfigManager = new MainFontConfigManager(this._fontFileProvider, this._vanillaFontProvider, this._cpFontFileProviders);

            // connect manager and repository.
            this._fontConfigManager.ConfigUpdated += (s, e) => this._fontConfigRepository.WriteConfig(e.Context, e.Config);
            this._fontConfigManager.PresetUpdated += (s, e) => this._fontPresetRepository.WritePreset(e.Name, e.Preset);

            // init font patching.
            this._mainFontPatcher = new MainFontPatcher(this._fontConfigManager, new FontPatchResolverFactory(this._config), new FontPatchInvalidatorComposition(helper), this.Monitor);
            this._vanillaFontProvider.SetInvalidateHelper(this._mainFontPatcher);
            this._fontChanger = new FontPatchChanger(this._mainFontPatcher);

            // watch `Data/AdditonalLanguages` asset.
            this._dataAdditionalLanguagesWatcher.Updated += this.OnDataAdditionalLanguagesUpdated;

            // TODO: 当前使用'export'文件夹作为唯一输出路径。
            string exportDir = Path.Combine(this.Helper.DirectoryPath, "export");
            foreach (var exporting in this._menuContextModel.Exporting.Values)
                exporting.OutputDirectory = exportDir;

            Harmony = new Harmony(this.ModManifest.UniqueID);
            {
                new FontShadowPatcher(this._config, this._configWatcher)
                    .Patch(Harmony, this.Monitor);

                new TextColorPatcher(this._config, this._configWatcher)
                    .Patch(Harmony, this.Monitor);

                var spriteTextPatcher = new SpriteTextPatcher(this._config);
                spriteTextPatcher.Patch(Harmony, this.Monitor);
                this._mainFontPatcher.FontPixelZoomOverride += (s, e) =>
                    spriteTextPatcher.SetOverridePixelZoom(e.PixelZoom);

                new SpriteTextLatinPatcher(this._config, this.ModManifest, helper)
                    .Patch(Harmony, this.Monitor);
            }

            helper.Events.Content.AssetRequested += this.OnAssetRequestedEarly;
            helper.Events.Content.AssetReady += this.OnAssetReadyEarly;
            helper.Events.GameLoop.UpdateTicking += this.OnUpdateTicking;
            helper.Events.Content.AssetRequested += this.OnAssetRequested;
            helper.Events.Content.AssetReady += this.OnAssetReady;
            helper.Events.Content.LocaleChanged += this.OnLocaleChanged;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
            helper.Events.Display.WindowResized += this.OnWindowResized;
            helper.Events.Display.Rendered += this.OnRendered;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;

#if DEBUG
            helper.ConsoleCommands.Add("export", "export font", (_, args) => this.ExportCommand(args));
#endif
            helper.ConsoleCommands.Add("local", this.LocalCommandDocs(), (_, args) => this.LocalCommand(args));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._fontChanger.Dispose();
            }

            base.Dispose(disposing);
        }

        private void OnUpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            this._mainFontPatcher.OnUpdateTicking(e);
            this._configWatcher.Update();
        }

        private void OnAssetRequestedEarly(object sender, AssetRequestedEventArgs e)
        {
            this._vanillaFontProvider.OnAssetRequested(e);
        }

        private void OnAssetReadyEarly(object sender, AssetReadyEventArgs e)
        {
            this._vanillaFontProvider.OnAssetReady(e);
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            new GMCMIntegration(
                config: this._config,
                reset: this.ResetConfig,
                save: () => this.SaveConfig(this._config),
                modRegistry: this.Helper.ModRegistry,
                monitor: this.Monitor,
                manifest: this.ModManifest,
                isGMCMOptionsRequired: false)
                .Integrate();

            new ToolbarIconsIntegration(
                modRegistry: this.Helper.ModRegistry,
                monitor: this.Monitor,
                uniqueId: this.ModManifest.UniqueID,
                openFontSettingsMenu: this.OpenFontSettingsMenu)
                .Integrate();

            // init title font button. (must be after `Textures.OnAssetRequested` subscription)
            this._titleFontButton = new TitleFontButton(
                position: this.GetTitleFontButtonPosition(),
                onClicked: () => this.OpenFontSettingsMenu());

            // load data for english.
            this.LoadDataForLanguage(FontHelpers.LanguageEn);
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
            Textures.OnAssetRequested(e);
            this._mainFontPatcher.OnAssetRequested(e);
        }

        private void OnAssetReady(object sender, AssetReadyEventArgs e)
        {
            this._mainFontPatcher.OnAssetReady(e);
            this._dataAdditionalLanguagesWatcher.OnAssetReady(e);
        }

        private void OnLocaleChanged(object sender, LocaleChangedEventArgs e)
        {
            this.LoadDataForLanguage(FontHelpers.GetCurrentLanguage());
        }

        private void OnWindowResized(object sender, WindowResizedEventArgs e)
        {
            if (this._titleFontButton != null)
                this._titleFontButton.Position = this.GetTitleFontButtonPosition();
        }

        private void OnRendered(object sender, RenderedEventArgs e)
        {
            if (this.IsTitleMenuInteractable())
                this._titleFontButton?.Draw(e.SpriteBatch);
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (this.IsTitleMenuInteractable())
                this._titleFontButton?.Update();
        }

        private void ReloadContentPacks()
        {
            // reload presets
            this._fontConfigManager.RemoveAllContentPacks();
            IEnumerable<FontPresetModel> cpPresets = this._contentPackRepository.ReadContentPacks(this._languagesWhoseDataIsLoaded);
            this._fontConfigManager.AddPresets(cpPresets);

            // reload fontFileProviders
            this.ReloadCpFontFileProviders();
        }

        private async void ExportCommand(string[] args)
        {
            var spriteFontConfig = new FontConfigBuilder()
                .BasicConfig(new FontConfig(
                    Enabled: true,
                    FontFilePath: @"C:\Windows\Fonts\micross.ttf",
                    FontIndex: 0,
                    FontSize: 26f,
                    Spacing: 0f,
                    LineSpacing: 26,
                    CharOffsetX: 0f,
                    CharOffsetY: 0f,
                    CharacterRanges: this._vanillaFontProvider.GetVanillaCharacterRanges(FontHelpers.LanguageEn, GameFontType.SmallFont)))
                .WithDefaultCharacter('*')
                .Build();
            var settings = new FontExportSettings(
                format: Framework.FontFormat.BmFont,
                inXnb: true,
                outputDirectory: Path.Combine(this.Helper.DirectoryPath, "export"),
                outputFileName: Path.GetRandomFileName(),
                xnbPlatform: XnbPlatform.Windows,
                gameFramework: Framework.GameFramework.Monogame,
                graphicsProfile: GraphicsProfile.HiDef,
                isCompressed: true,
                pageWidth: 0,
                pageHeight: 0);

            IResultWithoutData result = await new FontExporter().Export(spriteFontConfig, settings);

            if (result.IsSuccess)
            {
                Game1.playSound("money");
            }
            else
            {
                this.Monitor.Log($"{result.GetError()}", LogLevel.Error);
            }
        }

        private string LocalCommandDocs()
        {
            return "Command for local data (saved settings & presets)."
                + "\n"
                + "\nlocal data"
                + "\nUsage: local data <options>"
                + "\nOptions:"
                + "\n--clear                Clears all saved settings.";
        }

        private void LocalCommand(string[] args)
        {
            if (args.Length == 2
                && args[0] == "data"
                && args[1] == "--clear")
            {
                var configRepo = new FontConfigRepository(this.Helper);
                configRepo.ClearAllConfigs();
            }
        }

        private void LoadDataForLanguage(LanguageInfo language)
        {
            if (!this._languagesWhoseDataIsLoaded.Contains(language))
            {
                this._languagesWhoseDataIsLoaded.Add(language);

                foreach (GameFontType fontType in Enum.GetValues<GameFontType>())
                {
                    var context = new FontContext(language, fontType);

                    FontConfigModel? vanillaConfig = this._vanillaFontDataRepository.ReadVanillaFontConfig(context);
                    FontConfigModel? currentConfig = this._fontConfigRepository.ReadConfig(context);

                    this._fontConfigManager.AddVanillaConfig(context, vanillaConfig);
                    this._fontConfigManager.AddFontConfig(context, currentConfig);
                }

                IEnumerable<FontPresetModel> presets = this._fontPresetRepository.ReadPresets(language);
                IEnumerable<FontPresetModel> cpPresets = this._contentPackRepository.ReadContentPacks(language);

                this._fontConfigManager.AddPresets(presets);
                this._fontConfigManager.AddPresets(cpPresets);
            }
        }

        private void ReloadCpFontFileProviders()
        {
            this._cpFontFileProviders.Clear();
            foreach (IContentPack pack in this.Helper.ContentPacks.GetOwned())
            {
                FontFileProvider fontFileProvider;
                {
                    fontFileProvider = new FontFileProvider();
                    fontFileProvider.Scanners.Add(
                        new BasicFontFileScanner(pack.DirectoryPath, new ScanSettings()));
                }
                this._cpFontFileProviders[pack] = fontFileProvider;
            }
        }

        private void SaveConfig(ModConfig config)
        {
            this.Helper.WriteConfig(config);
        }

        private void ResetConfig()
        {
            // 重置
            this._config.ResetToDefault();

            // 保存
            this.Helper.WriteConfig(this._config);
        }

        private IEnumerable<IFontFileScanner> YieldFontScanners()
        {
            var scanSettings = new ScanSettings();

            // installation folder for each platform.
            switch (Constants.TargetPlatform)
            {
                case GamePlatform.Windows:
                    yield return new InstalledFontScannerForWindows(scanSettings);
                    break;
                case GamePlatform.Mac:
                    yield return new InstalledFontScannerForMacOS(scanSettings);
                    break;
                case GamePlatform.Linux:
                    yield return new InstalledFontScannerForLinux(scanSettings);
                    break;
                case GamePlatform.Android:
                    yield return new IntalledFontScannerForAndroid(scanSettings);
                    break;
            }

            // mod internal 'assets/fonts' folder.
            string vanillaFontFolder = Path.Combine(this.Helper.DirectoryPath, "assets/fonts");
            Directory.CreateDirectory(vanillaFontFolder);
            yield return new BasicFontFileScanner(vanillaFontFolder, scanSettings);

            // custom folders specified by user.
            var customFolders = this._config.CustomFontFolders.Distinct().ToArray();
            foreach (string folder in customFolders)
            {
                if (!Directory.Exists(folder))
                    this.Monitor.Log($"Skipped invalid custom font folder: {folder}");

                yield return new BasicFontFileScanner(folder, scanSettings);
            }
        }

        private void OnFontRecordStarted(object sender, RecordEventArgs e)
        {
            this.Monitor.Log($"记录{e.Language}的{e.FontType}，中断font patch。");
            this._mainFontPatcher.PauseFontPatch();
        }

        private void OnFontRecordFinished(object sender, RecordEventArgs e)
        {
            this.Monitor.Log($"完成记录{e.Language}的{e.FontType}。");

            // 准备好保存的字体设置，马上要修改了。
            {
                var context = new FontContext(e.Language, e.FontType);

                FontConfigModel? vanillaConfig = this._vanillaFontDataRepository.ReadVanillaFontConfig(context);
                FontConfigModel? currentConfig = this._fontConfigRepository.ReadConfig(context);

                this._fontConfigManager.AddVanillaConfig(context, vanillaConfig);
                this._fontConfigManager.AddFontConfig(context, currentConfig);
            }

            this.Monitor.Log($"恢复font patch。");
            this._mainFontPatcher.ResumeFontPatch();
        }

        private Lazy<IEnumerable<IContentPack>> _testContentPacks;
        private IEnumerable<IContentPack> EnumerateTestContentPacks()
        {
            this._testContentPacks ??= new(this.CreateTestContentPacks);
            return this._testContentPacks.Value;
        }

        private IEnumerable<IContentPack> CreateTestContentPacks()
        {
#if DEBUG
            IContentPack FakeContentPack(string relativeDir) => this.Helper.ContentPacks.CreateFake(Path.Combine(this.Helper.DirectoryPath, relativeDir));

            yield return FakeContentPack("content-packs/single");
#else
            return Array.Empty<IContentPack>();
#endif
        }

        private void OpenFontSettingsMenu()
        {
            var menu = this.CreateFontSettingsMenu();

            if (Game1.activeClickableMenu is TitleMenu)
                TitleMenu.subMenu = menu;
            else
                Game1.activeClickableMenu = menu;
        }

        private FontSettingsMenu CreateFontSettingsMenu()
        {
            Stopwatch stopwatch = new Stopwatch();
            try
            {
                stopwatch.Start();

                ISampleFontGenerator sampleFontGenerator = new SampleFontGenerator2(this._vanillaFontProvider, () => this._config.EnableLatinDialogueFont);

                FontSettingsMenuModel viewModel;
                {
                    bool async = true;
                    if (async)
                        viewModel = new FontSettingsMenuModelAsync(
                            config: this._config,
                            vanillaFontProvider: this._vanillaFontProvider,
                            sampleFontGenerator: sampleFontGenerator,
                            presetManager: this._fontConfigManager,
                            fontConfigManager: this._fontConfigManager,
                            vanillaFontConfigProvider: this._fontConfigManager,
                            gameFontChanger: this._fontChanger,
                            fontFileProvider: this._fontFileProvider,
                            cpFontFileProviders: this._cpFontFileProviders,
                            fontInfoRetriever: new FontInfoRetriever(),
                            asyncFontInfoRetriever: new FontInfoRetriever(),
                            exporter: this._exporter,
                            stagedValues: this._menuContextModel);
                    else
                        viewModel = new FontSettingsMenuModel(
                            config: this._config,
                            vanillaFontProvider: this._vanillaFontProvider,
                            sampleFontGenerator: sampleFontGenerator,
                            presetManager: this._fontConfigManager,
                            fontConfigManager: this._fontConfigManager,
                            vanillaFontConfigProvider: this._fontConfigManager,
                            gameFontChanger: this._fontChanger,
                            fontFileProvider: this._fontFileProvider,
                            cpFontFileProviders: this._cpFontFileProviders,
                            fontInfoRetriever: new FontInfoRetriever(),
                            exporter: this._exporter,
                            stagedValues: this._menuContextModel);
                }

                return new FontSettingsMenu(this._fontConfigManager, this.Helper.ModRegistry, this._config.EnableLatinDialogueFont, viewModel);
            }
            finally
            {
                stopwatch.Stop();
                this.Monitor.Log($"{nameof(FontSettingsMenu)} creation completed in '{stopwatch.ElapsedMilliseconds}ms'");
            }
        }

        private void OnDataAdditionalLanguagesUpdated(object sender, List<ModLanguage> value)
        {
            FontHelpers.SetModLanguages(value.ToArray());
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

        private Microsoft.Xna.Framework.Point GetTitleFontButtonPosition()
        {
            var registry = this.Helper.ModRegistry;
            bool gmcm = registry.IsLoaded("spacechase0.GenericModConfigMenu");  // (36, Game1.viewport.Height - 100)
            bool mum = registry.IsLoaded("cat.modupdatemenu");                  // (36, Game1.viewport.Height - 150 - 48)
                                                                                // ()
            int interval = 75 + 24;

            switch (0)
            {
                case { } when !gmcm:
                    return new(36, Game1.viewport.Height - interval);

                case { } when gmcm:
                    return mum
                        ? new(36, Game1.viewport.Height - interval * 3)
                        : new(36, Game1.viewport.Height - interval * 2);

                default:
                    return new(36, Game1.viewport.Height - interval);
            }
        }

        /// <summary>Copied from GMCM source code :D</summary>
        private bool IsTitleMenuInteractable()
        {
            if (Game1.activeClickableMenu is not TitleMenu titleMenu || TitleMenu.subMenu != null)
                return false;

            var method = this.Helper.Reflection.GetMethod(titleMenu, "ShouldAllowInteraction", false);
            if (method != null)
                return method.Invoke<bool>();
            else // method isn't available on Android
                return this.Helper.Reflection.GetField<bool>(titleMenu, "titleInPosition").GetValue();
        }
    }
}
