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

            this.RecordData(LocalizedContentManager.LanguageCode.en);

            Harmony = new Harmony(this.ModManifest.UniqueID);
            {
                new Game1Patcher(this._config, this._fontManager, this._fontChanger)
                    .Patch(Harmony, this.Monitor);

                new SpriteTextPatcher(this._config, this._fontManager, this._fontChanger)
                    .Patch(Harmony, this.Monitor);

                OptionsPageWithFont.Initialize(this._config, this._fontManager, this._fontChanger, (cfg) => helper.WriteConfig(cfg));
                OptionsPageWithFont.Patch(Harmony, this.Monitor);
            }
            LocalizedContentManager.OnLanguageChange += this.OnLanguageChanged;

            helper.Events.Content.AssetsInvalidated += this.OnAssetsInvalidated;
            helper.Events.Content.AssetRequested += this.OnAssetRequested;
            helper.Events.Content.AssetReady += this.OnAssetReady;
            helper.Events.Content.LocaleChanged += this.OnLocaleChanged;
        }

        private void OnLocaleChanged(object sender, LocaleChangedEventArgs e)
        {
            this.RecordData(e.NewLanguage);
        }

        private void OnAssetReady(object sender, AssetReadyEventArgs e)
        {
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Fonts/SmallFont"))
            {
                e.LoadFrom(() =>
                {
                    FontConfig config = this._config.GetFontConfig(FontHelpers.CurrentLanguageCode, GameFontType.SmallFont);
                    return this.GetCustomSpriteFont(config);
                }, AssetLoadPriority.High);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Fonts/SpriteFont1"))
            {
                e.LoadFrom(() =>
                {
                    FontConfig config = this._config.GetFontConfig(FontHelpers.CurrentLanguageCode, GameFontType.DialogueFont);
                    return this.GetCustomSpriteFont(config);
                }, AssetLoadPriority.High);
            }
        }

        private void OnAssetsInvalidated(object sender, AssetsInvalidatedEventArgs e)
        {

        }

        private void OnLanguageChanged(LocalizedContentManager.LanguageCode code)
        {
            foreach (GameFontType type in Enum.GetValues<GameFontType>())
            {
                if (!this._config.Fonts.Any(f => (int)f.Lang == (int)code && f.InGameType == type))
                    this._config.Fonts.Add(new FontConfig()
                    {
                        Lang = (LanguageCode)(int)code,
                        InGameType = type,
                        FontSize = 12f,
                        Spacing = 0,
                        LineSpacing = 12
                    });
            }
            this.Helper.WriteConfig(this._config);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this._fontManager.Dispose();
        }

        private SpriteFont GetCustomSpriteFont(FontConfig font)
        {
            SpriteFont? newFont;

            if (!font.Enabled)
                newFont = this._fontManager.GetBuiltInSpriteFont(font.InGameType);

            else if (font.FontFilePath is null)  // 保持原版字体，但可能改变大小、间距。
            {
                SpriteFont builtIn = this._fontManager.GetBuiltInSpriteFont(font.InGameType);
                newFont = SpriteFontGenerator.FromExisting(
                    builtIn,
                    font.FontSize,
                    font.CharacterRanges ?? CharRangeSource.GetBuiltInCharRange(font.Lang),
                    font.Spacing,
                    font.LineSpacing
                );
            }

            else
            {
                newFont = SpriteFontGenerator.FromTtf(
                    InstalledFonts.GetFullPath(font.FontFilePath),
                    font.FontIndex,
                    font.FontSize,
                    font.CharacterRanges ?? CharRangeSource.GetBuiltInCharRange(font.Lang),
                    spacing: font.Spacing,
                    lineSpacing: font.LineSpacing
                );
            }

            return newFont;
        }

        private void RecordData(LocalizedContentManager.LanguageCode languageCode)
        {
            this._fontManager.RecordBuiltInSpriteFont(FontHelpers.ConvertLanguageCode(languageCode), GameFontType.SmallFont, this.Helper.GameContent.Load<SpriteFont>("Fonts/SmallFont"));
            this._fontManager.RecordBuiltInSpriteFont(FontHelpers.ConvertLanguageCode(languageCode), GameFontType.DialogueFont, this.Helper.GameContent.Load<SpriteFont>("Fonts/SpriteFont1"));
            this._fontManager.RecordBuiltInBmFont(this.LoadGameBmFont(this.Helper, languageCode));
            CharRangeSource.RecordBuiltInCharRange(FontHelpers.ConvertLanguageCode(languageCode), this._fontManager.GetBuiltInSpriteFont(GameFontType.SmallFont));
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
                List<Texture2D> pages = new List<Texture2D>(fontFile.Pages.Count);
                foreach (FontPage fontPage in fontFile.Pages)
                {
                    pages.Add(helper.GameContent.Load<Texture2D>($"Fonts/{fontPage.File}"));
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
    }
}
