using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BmFont;
using FontSettings.Framework;
using FontSettings.Framework.FontGenerators;
using FontSettings.Framework.FontInfomation;
using FontSettings.Framework.Menus;
using FontSettings.Framework.Migrations;
using FontSettings.Framework.Patchers;
using HarmonyLib;
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

        private readonly MigrateTo_0_2_0 _0_2_0_Migration = new();
        private readonly MigrateTo_0_6_0 _0_6_0_Migration = new();

        private ModConfig _config;

        private FontManager _fontManager;

        private GameFontChangerImpl _fontChangerImpl;

        private FontPresetManager _presetManager;

        private LocalizedContentManager _vanillaContentManager;

        private string? _fontFullPath_ja;
        private string? _fontFullPath_ko;
        private string? _fontFullPath_zh;

        internal static IModHelper ModHelper { get; private set; }

        internal static Harmony Harmony { get; private set; }

        public override void Entry(IModHelper helper)
        {
            ModHelper = this.Helper;
            I18n.Init(helper.Translation);
            Log.Init(this.Monitor);
            Textures.Init(helper.ModContent);
            if (this._0_2_0_Migration.NeedMigrate(helper))
            {
                this._0_2_0_Migration.Apply(helper, out this._config);
                this.SaveConfig(this._config);
                this.SaveFontSettings(this._config.Fonts);
            }
            else
            {
                this._config = helper.ReadConfig<ModConfig>();
                this._config.Fonts = this.ReadFontSettings();
            }
            this.CheckConfigValid(this._config);
            this._config.Sample = this.ReadSampleData();
            this._config.VanillaFont = this.ReadVanillaFontData();

            this.AssertModFileExists(this._const_fontPath_ja, out this._fontFullPath_ja);
            this.AssertModFileExists(this._const_fontPath_ko, out this._fontFullPath_ko);
            this.AssertModFileExists(this._const_fontPath_zh, out this._fontFullPath_zh);

            this._fontManager = new(helper.ModContent);
            this._fontChangerImpl = new GameFontChangerImpl(helper, this._config, this.GetVanillaFontFile);
            this._presetManager = new(Path.Combine(Constants.DataPath, ".smapi", "mod-data", this.ModManifest.UniqueID.ToLower(), "Presets"), "System");
            this._vanillaContentManager = new LocalizedContentManager(GameRunner.instance.Content.ServiceProvider, GameRunner.instance.Content.RootDirectory);

            this._0_6_0_Migration.Apply(helper, this.ModManifest, this._config.Fonts, this.SaveFontSettings, this._config, this.SaveConfig, this._presetManager);

            Harmony = new Harmony(this.ModManifest.UniqueID);
            {
                new GameMenuPatcher()
                    .AddFontSettingsPage(helper, Harmony, this._config, this.SaveConfig);

                new FontShadowPatcher(this._config)
                    .Patch(Harmony, this.Monitor);
            }

            helper.Events.Content.LocaleChanged += this.OnLocaleChanged;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;

            this.OnLocaleChanged(LocalizedContentManager.LanguageCode.en, string.Empty);
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

        private void OnLocaleChanged(object sender, LocaleChangedEventArgs e)
        {
            this.OnLocaleChanged(e.NewLanguage, e.NewLocale);
        }

        private void OnLocaleChanged(LocalizedContentManager.LanguageCode newLangugage, string newLocale)
        {
            // 记录字体数据。
            this.RecordFontData(newLangugage, newLocale, LocalizedContentManager.CurrentModLanguage);

            foreach (GameFontType fontType in Enum.GetValues<GameFontType>())
            {
                if (this._config.Fonts.TryGetFontConfig(LocalizedContentManager.CurrentLanguageCode,
                    FontHelpers.GetCurrentLocale(), fontType, out FontConfig config))
                {
                    this._fontChangerImpl.ChangeGameFont(config);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this._fontManager.Dispose();
        }

        private void RecordFontData(LocalizedContentManager.LanguageCode languageCode, string locale, ModLanguage modLanguage)
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

                SpriteFont smallFont = this.Helper.GameContent.Load<SpriteFont>("Fonts/SmallFont");
                SpriteFont dialogueFont = this.Helper.GameContent.Load<SpriteFont>("Fonts/SpriteFont1");
                GameBitmapSpriteFont spriteText = languageCode != LocalizedContentManager.LanguageCode.mod
                    ? this.LoadGameBmFont(languageCode)
                    : this.LoadModBmFont(modLanguage);

                // 记录内置字体。
                this._fontManager.RecordBuiltInSpriteFont(GameFontType.SmallFont, smallFont);
                this._fontManager.RecordBuiltInSpriteFont(GameFontType.DialogueFont, dialogueFont);
                this._fontManager.RecordBuiltInBmFont(spriteText);

                // 记录字符范围，加载字体要用。
                CharRangeSource.RecordBuiltInCharRange(smallFont);

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
                LocalizedContentManager.LanguageCode.th => "Fonts/Thai",
                LocalizedContentManager.LanguageCode.ko => "Fonts/Korean",
                _ => null
            };

            if (fntPath != null)
            {
                var contentManager = this.Helper.GameContent;

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

        private SampleData ReadSampleData()
        {
            return this.ReadModJsonFile<SampleData>("assets/sample.json");
        }

        private VanillaFontData ReadVanillaFontData()
        {
            return this.ReadModJsonFile<VanillaFontData>("assets/vanilla-fonts.json");
        }

        private TModel ReadModJsonFile<TModel>(string path, bool logIfNotFound = true)
            where TModel : class, new()
        {
            var model = this.Helper.Data.ReadJsonFile<TModel>(path);
            if (model == null)
            {
                if (logIfNotFound)
                    this.Monitor.Log(I18n.Misc_ModFileNotFound(path), LogLevel.Error);
                model = new();
            }
            return model;
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

            Game1.activeClickableMenu = new FontSettingsPage(this._config, this._fontManager, sampleFontGenerator, sampleAsyncFontGenerator, this._fontChangerImpl, this._presetManager, this.SaveFontSettings, this.Helper.ModRegistry, this.GetVanillaFontFile);
        }

        // returns null if not supported.
        private string? GetVanillaFontFile(LanguageInfo lang, GameFontType fontType)
        {
            return (lang, fontType) switch
            {
                _ when lang == FontHelpers.LanguageJa => this._fontFullPath_ja,
                _ when lang == FontHelpers.LanguageKo => this._fontFullPath_ko,
                _ when lang == FontHelpers.LanguageZh => this._fontFullPath_zh,
                _ => null,
            };
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
