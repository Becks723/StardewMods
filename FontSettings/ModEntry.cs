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
using StardewValley;
using StardewValley.GameData;
using StardewValley.Menus;

namespace FontSettings
{
    internal class ModEntry : Mod
    {
        private readonly string _globalFontDataKey = "font-data";

        private readonly MigrateTo_0_2_0 _0_2_0_Migration = new();
        private readonly MigrateTo_0_6_0 _0_6_0_Migration = new();

        private ModConfig _config;

        private FontManager _fontManager;

        private GameFontChanger _fontChanger;

        private GameFontChangerImpl _fontChangerImpl;

        private FontPresetManager _presetManager;

        private LocalizedContentManager _vanillaContentManager;

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

            this._fontManager = new(helper.ModContent);
            this._fontChanger = new(this._fontManager);
            this._fontChangerImpl = new GameFontChangerImpl(helper, this._config);
            this._presetManager = new(Path.Combine(Constants.DataPath, ".smapi", "mod-data", this.ModManifest.UniqueID.ToLower(), "Presets"), "System");
            this._vanillaContentManager = new LocalizedContentManager(GameRunner.instance.Content.ServiceProvider, GameRunner.instance.Content.RootDirectory);

            this._0_6_0_Migration.Apply(helper, this.ModManifest, this._config.Fonts, this.SaveFontSettings, this._presetManager);

            Harmony = new Harmony(this.ModManifest.UniqueID);
            {
                new Game1Patcher(this._config, this._fontManager, this._fontChanger)
                    .Patch(Harmony, this.Monitor);

                //new SpriteTextPatcher(this._config, this._fontManager, this._fontChanger)
                //    .Patch(Harmony, this.Monitor);

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
            this.RecordFontData(newLangugage, newLocale);

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

        private void RecordFontData(LocalizedContentManager.LanguageCode languageCode, string locale)
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

                SpriteFont smallFont = this._vanillaContentManager.Load<SpriteFont>("Fonts/SmallFont");
                SpriteFont dialogueFont = this._vanillaContentManager.Load<SpriteFont>("Fonts/SpriteFont1");
                GameBitmapSpriteFont spriteText = this.LoadGameBmFont(languageCode);

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
                LocalizedContentManager.LanguageCode.mod when !LocalizedContentManager.CurrentModLanguage.UseLatinFont => LocalizedContentManager.CurrentModLanguage.FontFile,
                _ => null
            };

            if (fntPath != null)
            {
                var contentManager = this._vanillaContentManager;

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
            const string path = "assets/sample.json";
            var sample = this.Helper.Data.ReadJsonFile<SampleData>(path);
            if (sample == null)
            {
                this.Monitor.Log($"Missing file: {path}. Please download the mod again to restore the file.");
                sample = new SampleData();
            }
            return sample;
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

            Game1.activeClickableMenu = new FontSettingsPage(this._config, this._fontManager, sampleFontGenerator, sampleAsyncFontGenerator, this._fontChangerImpl, this._presetManager, this.SaveFontSettings, this.Helper.ModRegistry);
        }
    }
}
