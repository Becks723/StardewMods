using System;
using System.Collections.Generic;
using System.Linq;
using BmFont;
using FontSettings.Framework;
using FontSettings.Framework.FontInfomation;
using FontSettings.Framework.Menus;
using FontSettings.Framework.Patchers;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData;

namespace FontSettings
{
    internal class ModEntry : Mod
    {
        /// <summary>记录不同语言下字体信息是否已经缓存了。</summary>
        private readonly Dictionary<string, bool> _cacheTable = new();

        private ModConfig _config;

        private RuntimeFontManager _fontManager;

        private GameFontChanger _fontChanger;

        internal static Harmony Harmony { get; private set; }

        public override void Entry(IModHelper helper)
        {
            I18n.Init(helper.Translation);
            Log.Init(this.Monitor);
            this._config = helper.ReadConfig<ModConfig>();
            this._fontManager = new(helper.ModContent);
            this._fontChanger = new(this._fontManager);
            foreach (LocalizedContentManager.LanguageCode code in Enum.GetValues<LocalizedContentManager.LanguageCode>())
            {
                if (code is LocalizedContentManager.LanguageCode.mod)
                    continue;

                string locale = FontHelpers.GetLocale(code);
                this._cacheTable[locale] = false;  // TODO: 支持mod语言。
            }

            BmFontGenerator.Initialize(helper);
            CharsFileManager.Initialize(System.IO.Path.Combine(helper.DirectoryPath, "Chars"));

            Harmony = new Harmony(this.ModManifest.UniqueID);
            {
                new Game1Patcher(this._config, this._fontManager, this._fontChanger)
                    .Patch(Harmony, this.Monitor);

                new SpriteTextPatcher(this._config, this._fontManager, this._fontChanger)
                    .Patch(Harmony, this.Monitor);

                OptionsPageWithFont.Initialize(this._config, this._fontManager, this._fontChanger, (cfg) => helper.WriteConfig(cfg));
                OptionsPageWithFont.Patch(Harmony, this.Monitor);
            }

            helper.Events.Content.AssetRequested += this.OnAssetRequested;
            helper.Events.Content.LocaleChanged += this.OnLocaleChanged;

            this.RecordFontData(LocalizedContentManager.LanguageCode.en, null);
        }

        private void OnLocaleChanged(object sender, LocaleChangedEventArgs e)
        {
            // 创建配置项。
            foreach (GameFontType type in Enum.GetValues<GameFontType>())
            {
                if (!this._config.Fonts.Any(f =>
                    f.Locale == e.NewLocale
                    && f.InGameType == type
                    && f.Lang == e.NewLanguage))
                {
                    this._config.Fonts.Add(new FontConfig()
                    {
                        Enabled = false,
                        Lang = e.NewLanguage,
                        Locale = !string.IsNullOrEmpty(e.NewLocale) ? e.NewLocale : "en",
                        InGameType = type,
                        FontSize = 24,
                        Spacing = 0,
                        LineSpacing = 24
                    });
                }
            }
            this.Helper.WriteConfig(this._config);

            // 记录字体数据。
            this.RecordFontData(e.NewLanguage, e.NewLocale);
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            void ReplaceSpriteFont(GameFontType fontType)
            {
                FontConfig config = this._config.GetOrCreateFontConfig(LocalizedContentManager.CurrentLanguageCode,
                    FontHelpers.GetCurrentLocale(), fontType);

                if (!config.Enabled)
                    return;
                else if (config.FontFilePath is null)
                    e.Edit(asset =>
                    {
                        SpriteFont spriteFont = asset.GetData<SpriteFont>();
                        spriteFont.Spacing = config.Spacing;
                        spriteFont.LineSpacing = config.LineSpacing;
                    });
                else
                    e.LoadFrom(() =>
                    {
                        return SpriteFontGenerator.FromTtf(
                            InstalledFonts.GetFullPath(config.FontFilePath),
                            config.FontIndex,
                            config.FontSize,
                            config.CharacterRanges ?? CharRangeSource.GetBuiltInCharRange(config.GetLanguage()),
                            spacing: config.Spacing,
                            lineSpacing: config.LineSpacing
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
            if (string.IsNullOrEmpty(locale))
                locale = "en";

            // 记录mod语言。
            bool isCached = false;
            if (languageCode is LocalizedContentManager.LanguageCode.mod &&
                !this._cacheTable.TryGetValue(locale, out isCached))
            {
                this._cacheTable[locale] = false;
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
            this._cacheTable[locale] = true;
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

        private T LoadWithoutCache<T>(string key, string locale)
        {
            T result = this.Helper.GameContent.Load<T>(key);
            this.Helper.GameContent.InvalidateCache($"{key}.{locale}");
            return result;
        }
    }
}
