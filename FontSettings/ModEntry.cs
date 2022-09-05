using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BmFont;
using FontSettings.Framework;
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

        /// <summary>记录不同语言下字体信息是否已经缓存了。</summary>
        private readonly Dictionary<LanguageInfo, bool> _cacheTable = new();

        private readonly MigrateTo_0_2_0 _0_2_0_Migration = new();

        private ModConfig _config;

        private RuntimeFontManager _fontManager;

        private GameFontChanger _fontChanger;

        private FontPresetManager _presetManager;

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
            this._fontManager = new(helper.ModContent);
            this._fontChanger = new(this._fontManager);
            this._presetManager = new(Path.Combine(Constants.DataPath, ".smapi", "mod-data", this.ModManifest.UniqueID.ToLower(), "Presets"), "System");

            foreach (LocalizedContentManager.LanguageCode code in Enum.GetValues<LocalizedContentManager.LanguageCode>())
            {
                if (code is LocalizedContentManager.LanguageCode.mod)
                    continue;

                LanguageInfo langInfo = new LanguageInfo(code, FontHelpers.GetLocale(code));
                this._cacheTable[langInfo] = false;
            }

            Harmony = new Harmony(this.ModManifest.UniqueID);
            {
                new Game1Patcher(this._config, this._fontManager, this._fontChanger)
                    .Patch(Harmony, this.Monitor);

                new SpriteTextPatcher(this._config, this._fontManager, this._fontChanger)
                    .Patch(Harmony, this.Monitor);

                new GameMenuAdder()
                    .AddFontSettingsPage(helper, Harmony, this._config, this._fontManager, this._fontChanger, this._presetManager, config =>
                    {
                        this.SaveConfig(config);
                        this.SaveFontSettings(config.Fonts);
                    });

                new FontShadowPatcher(this._config)
                    .Patch(Harmony, this.Monitor);
            }

            helper.Events.Content.AssetRequested += this.OnAssetRequested;
            helper.Events.Content.LocaleChanged += this.OnLocaleChanged;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;

            this.RecordFontData(LocalizedContentManager.LanguageCode.en, null);
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
                int width = 800 + IClickableMenu.borderWidth * 2;
                int height = 600 + IClickableMenu.borderWidth * 2 + 64;
                Game1.activeClickableMenu = new FontSettingsPage(this._config, this._fontManager, this._fontChanger, this._presetManager, config =>
                {
                    this.SaveConfig(config);
                    this.SaveFontSettings(config.Fonts);
                },
                Game1.uiViewport.Width / 2 - width / 2,
                Game1.uiViewport.Height / 2 - height / 2,
                width,
                height,
                isStandalone: true);
            }
        }

        private void OnLocaleChanged(object sender, LocaleChangedEventArgs e)
        {
            // 记录字体数据。
            this.RecordFontData(e.NewLanguage, e.NewLocale);
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            void ReplaceSpriteFont(GameFontType fontType)
            {
                FontConfig config = this._config.Fonts.GetOrCreateFontConfig(LocalizedContentManager.CurrentLanguageCode,
                    FontHelpers.GetCurrentLocale(), fontType);

                if (!config.Enabled)
                    return;
                else if (config.FontFilePath is null)
                    e.Edit(asset =>
                    {
                        SpriteFontGenerator.EditExisting(
                            existingFont: asset.GetData<SpriteFont>(),
                            overridePixelHeight: config.FontSize,
                            overrideCharRange: config.CharacterRanges ?? CharRangeSource.GetBuiltInCharRange(config.GetLanguage()),
                            overrideSpacing: config.Spacing,
                            overrideLineSpacing: config.LineSpacing,
                            extraCharOffsetX: config.CharOffsetX,
                            extraCharOffsetY: config.CharOffsetY
                        );
                    });
                else
                    e.LoadFrom(() =>
                    {
                        return SpriteFontGenerator.FromTtf(
                            ttfPath: InstalledFonts.GetFullPath(config.FontFilePath),
                            fontIndex: config.FontIndex,
                            fontPixelHeight: config.FontSize,
                            characterRanges: config.CharacterRanges ?? CharRangeSource.GetBuiltInCharRange(config.GetLanguage()),
                            spacing: config.Spacing,
                            lineSpacing: config.LineSpacing,
                            charOffsetX: config.CharOffsetX,
                            charOffsetY: config.CharOffsetY
                        );
                    }, AssetLoadPriority.High);
            }

            if (e.NameWithoutLocale.IsEquivalentTo("Fonts/SmallFont"))
            {
                ReplaceSpriteFont(GameFontType.SmallFont);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Fonts/SpriteFont1"))
            {
                ReplaceSpriteFont(GameFontType.DialogueFont);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this._fontManager.Dispose();
        }

        private void RecordFontData(LocalizedContentManager.LanguageCode languageCode, string locale)
        {
            if (languageCode is LocalizedContentManager.LanguageCode.en && string.IsNullOrEmpty(locale))
                locale = "en";
            LanguageInfo langInfo = new LanguageInfo(languageCode, locale);

            // 记录mod语言。
            bool isCached = false;
            if (languageCode is LocalizedContentManager.LanguageCode.mod &&
                !this._cacheTable.TryGetValue(langInfo, out isCached))
            {
                this._cacheTable[langInfo] = false;
            }

            if (isCached)
            {
                this.Monitor.Log($"无需记录{locale}语言下的游戏字体数据！");
                return;
            }

            this.Monitor.Log($"正在记录{locale}语言下的游戏字体数据……", LogLevel.Debug);

            this.Helper.Events.Content.AssetRequested -= this.OnAssetRequested;

            SpriteFont smallFont = this.Helper.GameContent.Load<SpriteFont>("Fonts/SmallFont");
            SpriteFont dialogueFont = this.Helper.GameContent.Load<SpriteFont>("Fonts/SpriteFont1");
            GameBitmapSpriteFont spriteText = this.LoadGameBmFont(this.Helper, languageCode);

            this.Helper.Events.Content.AssetRequested += this.OnAssetRequested;  // 这条必须在InvalidateCache之前，我也不知道为什么。TODO

            // 记录内置字体。
            this._fontManager.RecordBuiltInSpriteFont(GameFontType.SmallFont, smallFont);
            this._fontManager.RecordBuiltInSpriteFont(GameFontType.DialogueFont, dialogueFont);
            this._fontManager.RecordBuiltInBmFont(spriteText);

            // 记录字符范围，加载字体要用。
            CharRangeSource.RecordBuiltInCharRange(smallFont);

            //if (locale != "en")  // 如果是英文原版，InvalidateCache后会自动重新加载，导致this.OnAssetRequested误触发，所以不需要InvalidateCache。 TODO: 为啥有时候不是英文也会触发this.OnAssetRequested？
            //{
            string LocalizedAssetName(string assetName) => locale != "en" ? $"{assetName}.{locale}" : assetName;
            this.Helper.GameContent.InvalidateCache(LocalizedAssetName("Fonts/SmallFont"));
            this.Helper.GameContent.InvalidateCache(LocalizedAssetName("Fonts/SpriteFont1"));
            //}

            this.Monitor.Log($"已完成记录{locale}语言下的游戏字体数据！", LogLevel.Debug);
            this._cacheTable[langInfo] = true;
        }

        private GameBitmapSpriteFont LoadGameBmFont(IModHelper helper, LocalizedContentManager.LanguageCode languageCode)
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
                FontFile fontFile = FontLoader.Parse(helper.GameContent.Load<XmlSource>(fntPath).Source);
                helper.GameContent.InvalidateCache(fntPath);
                List<Texture2D> pages = new List<Texture2D>(fontFile.Pages.Count);
                foreach (FontPage fontPage in fontFile.Pages)
                {
                    string assetName = $"Fonts/{fontPage.File}";
                    pages.Add(helper.GameContent.Load<Texture2D>(assetName));
                    helper.GameContent.InvalidateCache(assetName);
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
        }
    }
}
