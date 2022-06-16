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

            BmFontGenerator.Initialize(helper);
            CharsFileManager.Initialize(System.IO.Path.Combine(helper.DirectoryPath, "Chars"));

            //this.RecordData(LocalizedContentManager.LanguageCode.en);

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
        }

        private void OnLocaleChanged(object sender, LocaleChangedEventArgs e)
        {
            this.RecordFontData(e.NewLanguage, e.NewLocale);

            // 创建配置项。
            foreach (GameFontType type in Enum.GetValues<GameFontType>())
            {
                if (!this._config.Fonts.Any(f => (int)f.Lang == (int)e.NewLanguage && f.InGameType == type))
                    this._config.Fonts.Add(new FontConfig()
                    {
                        Lang = FontHelpers.ConvertLanguageCode(e.NewLanguage),
                        InGameType = type,
                        FontSize = 24,
                        Spacing = 0,
                        LineSpacing = 24
                    });
            }
            this.Helper.WriteConfig(this._config);
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            void ReplaceSpriteFont(GameFontType fontType)
            {
                FontConfig config = this._config.GetFontConfig(FontHelpers.CurrentLanguageCode, fontType);

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
                            config.CharacterRanges ?? CharRangeSource.GetBuiltInCharRange(config.Lang),
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
            // 记录字符范围，加载字体要用。
            this.Helper.Events.Content.AssetRequested -= this.OnAssetRequested;

            SpriteFont smallFont = this.Helper.GameContent.Load<SpriteFont>("Fonts/SmallFont");
            SpriteFont dialogueFont = this.Helper.GameContent.Load<SpriteFont>("Fonts/SpriteFont1");
            GameBitmapSpriteFont spriteText = this.LoadGameBmFont(this.Helper, languageCode);

            this.Helper.Events.Content.AssetRequested += this.OnAssetRequested;  // 这条必须在InvalidateCache之前，我也不知道为什么。TODO

            this._fontManager.RecordBuiltInSpriteFont(GameFontType.SmallFont, smallFont);
            this._fontManager.RecordBuiltInSpriteFont(GameFontType.DialogueFont, dialogueFont);
            this._fontManager.RecordBuiltInBmFont(spriteText);
            CharRangeSource.RecordBuiltInCharRange(smallFont);

            this.Helper.GameContent.InvalidateCache($"Fonts/SmallFont.{locale}");
            this.Helper.GameContent.InvalidateCache($"Fonts/SpriteFont1.{locale}");
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
