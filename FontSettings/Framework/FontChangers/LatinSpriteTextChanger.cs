using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using BmFont;
using FontSettings.Framework.FontInfomation;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.BellsAndWhistles;
using StardewValley;

namespace FontSettings.Framework.FontChangers
{
    internal class LatinSpriteTextChanger : BaseGameFontChanger
    {
        private readonly IGameContentHelper _gameContent;

        FontEditData _data;

        public LatinSpriteTextChanger(IModHelper helper)
        {
            helper.Events.Content.AssetRequested += this.OnAssetRequested;

            this._gameContent = helper.GameContent;
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            var data = this._data;
            if (data != null)
            {
                switch (data.EditMode)
                {
                    case EditMode.DoNothing:
                        break;

                    case EditMode.Replace:
                        SpriteFont font = data.Font;
                        if (e.NameWithoutLocale.IsEquivalentTo("LooseSprites/font_bold"))
                        {
                            e.Edit(asset =>
                            {
                                font.Texture
                                asset.AsImage().ExtendImage
                            })
                        }
                        break;
                }
            }
        }

        public override bool ChangeGameFont(FontConfig font)
        {
            try
            {
                if (TryResolveFontConfig(font, out this._data, out Exception ex))
                {
                    var content = this._gameContent;
                    content.InvalidateCache(GetFontFileAssetName());
                    content.InvalidateCache(this.LocalizeBaseAssetName(GetFontFileAssetName()));

                    var fontFile = SpriteText.FontFile;
                    foreach (FontPage page in fontFile?.Pages ?? Enumerable.Empty<FontPage>())
                    {
                        string pageName = $"Fonts/{page.File}";

                        content.InvalidateCache(pageName);
                        content.InvalidateCache(this.LocalizeBaseAssetName(pageName));
                    }

                    this.PropagateBmFont(this._data.Font?.PixelZoom);

                    return true;
                }
                else
                {
                    return false;
                }
            }
            finally
            {
                this._data = null;
            }
        }

        public override async Task<bool> ChangeGameFontAsync(FontConfig font)
        {
            try
            {
                bool success = await Task.Run(() => TryResolveFontConfig(font, out this._data, out Exception ex));
                if (success)
                {
                    var content = this._gameContent;
                    content.InvalidateCache(GetFontFileAssetName());
                    content.InvalidateCache(this.LocalizeBaseAssetName(GetFontFileAssetName()));

                    var fontFile = SpriteText.FontFile;
                    foreach (FontPage page in fontFile?.Pages ?? Enumerable.Empty<FontPage>())
                    {
                        string pageName = $"Fonts/{page.File}";

                        content.InvalidateCache(pageName);
                        content.InvalidateCache(this.LocalizeBaseAssetName(pageName));
                    }

                    this.PropagateBmFont(this._data.Font?.PixelZoom);
                }

                return success;
            }
            finally
            {
                this._data = null;
            }
        }

        void PropagateBmFont(float? customPixelZoom)
        {
            // 此方法必须在 !LocalizedContentManager.CurrentLanguageLatin 下运行。
            FontFile fontFile;
            float fontPixelZoom;
            var characterMap = new Dictionary<char, FontChar>();
            var fontPages = new List<Texture2D>();

            FontFile loadFont(string assetName)
                => FontLoader.Parse(Game1.content.Load<XmlSource>(assetName).Source);

            string fontFileName = GetFontFileAssetName();
            fontFile = loadFont(fontFileName);
            fontPixelZoom = customPixelZoom != null ? customPixelZoom.Value : GetFontPixelZoom();
            foreach (FontChar current in fontFile.Chars)
            {
                char key = (char)current.ID;
                characterMap.Add(key, current);
            }
            foreach (FontPage current2 in fontFile.Pages)
            {
                fontPages.Add(Game1.content.Load<Texture2D>("Fonts\\" + current2.File));
            }

            SpriteText.FontFile = fontFile;
            SpriteText.characterMap = characterMap;
            SpriteText.fontPages = fontPages;
            SpriteText.fontPixelZoom = fontPixelZoom;
        }

        static bool TryResolveFontConfig(FontConfig config, out FontEditData data, out Exception ex)
        {
            data = null;
            ex = null;

            if (!config.Enabled)
            {
                goto doNothing;
            }

            if (config.FontFilePath is null)
            {
                goto doNothing;
            }

            SpriteFont font;
            try
            {
                font = CreateSpriteFont(config);
            }
            catch (Exception exception)
            {
                font = null;
                ex = exception;
            }

            if (font != null)
            {
                data = new FontEditData(config, EditMode.Replace, font);
                return true;
            }
            else
            {
                return false;
            }

        doNothing:
            data = new FontEditData(config, EditMode.DoNothing, null);
            return true;
        }

        static SpriteFont CreateSpriteFont(FontConfig config)
        {
            return SpriteFontGenerator.FromTtf(
                ttfPath: InstalledFonts.GetFullPath(config.FontFilePath),
                fontIndex: config.FontIndex,
                fontPixelHeight: config.FontSize,
                characterRanges: config.CharacterRanges ?? CharRangeSource.GetBuiltInCharRange(config.GetLanguage()),
                spacing: config.Spacing,
                lineSpacing: config.LineSpacing,
                charOffsetX: config.CharOffsetX,
                charOffsetY: config.CharOffsetY);
        }

        private static string GetFontFileAssetName()
        {
            return LocalizedContentManager.CurrentLanguageCode switch
            {
                LocalizedContentManager.LanguageCode.ja => "Fonts/Japanese",
                LocalizedContentManager.LanguageCode.ru => "Fonts/Russian",
                LocalizedContentManager.LanguageCode.zh => "Fonts/Chinese",
                LocalizedContentManager.LanguageCode.th => "Fonts/Thai",
                LocalizedContentManager.LanguageCode.ko => "Fonts/Korean",
                LocalizedContentManager.LanguageCode.mod when !LocalizedContentManager.CurrentModLanguage.UseLatinFont => LocalizedContentManager.CurrentModLanguage.FontFile,
                _ => null
            };
        }

        private static float GetFontPixelZoom()
        {
            return LocalizedContentManager.CurrentLanguageCode switch
            {
                LocalizedContentManager.LanguageCode.ja => 1.75f,
                LocalizedContentManager.LanguageCode.ru => 3f,
                LocalizedContentManager.LanguageCode.zh => 1.5f,
                LocalizedContentManager.LanguageCode.th => 1.5f,
                LocalizedContentManager.LanguageCode.ko => 1.5f,
                LocalizedContentManager.LanguageCode.mod => LocalizedContentManager.CurrentModLanguage.FontPixelZoom,
                _ => throw new NotSupportedException()
            };
        }

        private static XmlSource ParseBack(FontFile fontFile)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(FontFile));
            using var writer = new StringWriter();
            xmlSerializer.Serialize(writer, fontFile);

            string xml = writer.ToString();
            return new XmlSource(xml);
        }

        private record FontEditData(FontConfig Config, EditMode EditMode, SpriteFont Font) : IDisposable
        {
            public void Dispose()
            {
                this.Font?.Texture?.Dispose();
            }
        }

        private enum EditMode
        {
            DoNothing,
            Replace
        }
    }
}
