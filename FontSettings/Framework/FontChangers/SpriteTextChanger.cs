using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using BmFont;
using FontSettings.Framework.FontInfomation;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace FontSettings.Framework.FontChangers
{
    internal class SpriteTextChanger : BaseGameFontChanger
    {
        private readonly object _lock = new object();

        private readonly ModConfig _config;
        private readonly IGameContentHelper _gameContent;

        BmFontEditData _data;

        public SpriteTextChanger(IModHelper helper, ModConfig config)
        {
            helper.Events.Content.AssetRequested += this.OnAssetRequested;

            this._config = config;
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

                    case EditMode.Edit:
                        if (e.NameWithoutLocale.IsEquivalentTo(GetFontFileAssetName()))
                        {
                            e.Edit(asset =>
                            {
                                FontFile fontFile = FontLoader.Parse(asset.GetData<XmlSource>().Source);
                                EditBmFont(fontFile, data.Config);
                            });
                        }
                        break;

                    case EditMode.Replace:
                        FontFile fontFile = data.Font.FontFile;
                        Texture2D[] pages = data.Font.Pages;
                        if (e.NameWithoutLocale.IsEquivalentTo(GetFontFileAssetName()))
                        {
                            XmlSource xmlSource = ParseBack(fontFile);
                            e.LoadFrom(() => xmlSource, AssetLoadPriority.High);
                        }
                        else
                        {
                            foreach (FontPage page in fontFile.Pages)
                            {
                                if (e.NameWithoutLocale.IsEquivalentTo($"Fonts/{page.File}"))
                                {
                                    Texture2D texture = pages[page.ID];
                                    e.LoadFrom(() => texture, AssetLoadPriority.High);
                                }
                            }
                        }
                        break;
                }
            }
        }

        //public override bool ChangeGameFont(FontConfig font)
        //{
        //    lock (this._lock)
        //    {
        //        var lastData = this._data;

        //        if (TryResolveFontConfig(font, out this._data, out Exception ex))
        //        {
        //            var content = this._gameContent;
        //            content.InvalidateCache(GetFontFileAssetName());
        //            content.InvalidateCache(this.LocalizeBaseAssetName(GetFontFileAssetName()));
        //            if (lastData != null)
        //            {
        //                foreach (FontPage page in lastData.Font.FontFile.Pages)
        //                {
        //                    content.InvalidateCache($"Fonts/{page.File}");
        //                    content.InvalidateCache(this.LocalizeBaseAssetName($"Fonts/{page.File}"));
        //                }
        //            }

        //            this.PropagateBmFont();

        //            //lastData?.Dispose();

        //            return true;
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    }
        //}

        public override bool ChangeGameFont(FontConfig font)
        {
            try
            {
                if (TryResolveFontConfig(font, out this._data, out Exception ex))
                {
                    var content = this._gameContent;
                    content.InvalidateCache(GetFontFileAssetName());
                    content.InvalidateCache(this.LocalizeBaseAssetName(GetFontFileAssetName()));

                    var fontFile = SpriteTextFields.FontFile;
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

        //public override async Task<bool> ChangeGameFontAsync(FontConfig font)
        //{
        //    var lastData = this._data;

        //    bool success = await Task.Run(() =>
        //    {
        //        lock (this._lock)
        //        {
        //            return TryResolveFontConfig(font, out this._data, out Exception ex);
        //        }
        //    });

        //    if (success)
        //    {
        //        var content = this._gameContent;
        //        content.InvalidateCache(GetFontFileAssetName());
        //        content.InvalidateCache(this.LocalizeBaseAssetName(GetFontFileAssetName()));
        //        if (lastData?.Font != null)
        //        {
        //            foreach (FontPage page in lastData.Font.FontFile.Pages)
        //            {
        //                content.InvalidateCache($"Fonts/{page.File}");
        //                content.InvalidateCache(this.LocalizeBaseAssetName($"Fonts/{page.File}"));
        //            }
        //        }

        //        this.PropagateBmFont();

        //        lastData?.Dispose();
        //    }

        //    return success;
        //}

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

                    var fontFile = SpriteTextFields.FontFile;
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

            SpriteTextFields.FontFile = fontFile;
            SpriteTextFields._characterMap = characterMap;
            SpriteTextFields.fontPages = fontPages;
            SpriteText.fontPixelZoom = fontPixelZoom;
        }

        static bool TryResolveFontConfig(FontConfig config, out BmFontEditData data, out Exception ex)
        {
            data = null;
            ex = null;

            if (LocalizedContentManager.CurrentLanguageLatin)
            {
                ex = new NotSupportedException(); // TODO: 不支持拉丁。
                return false;
            }

            if (!config.Enabled)
            {
                data = new BmFontEditData(config, EditMode.DoNothing, null);
                return true;
            }

            if (config.FontFilePath is null)
            {
                data = new BmFontEditData(config, EditMode.Edit, new BmFontData(null, null, GetFontPixelZoom()));
                return true;
            }

            BmFontData font;
            try
            {
                font = GenerateBmFont(config);
            }
            catch (Exception exception)
            {
                font = null;
                ex = exception;
            }

            if (font != null)
            {
                data = new BmFontEditData(config, EditMode.Replace, font);
                return true;
            }
            else
            {
                return false;
            }

        doNothing:
            data = new BmFontEditData(config, EditMode.DoNothing, null);
            return true;
        }

        private static BmFontData GenerateBmFont(FontConfig config)
        {
            string fontFullPath = InstalledFonts.GetFullPath(config.FontFilePath);
            BmFontGenerator.GenerateIntoMemory(
                fontFilePath: fontFullPath,
                fontFile: out FontFile fontFile,
                pages: out Texture2D[] pages,
                fontIndex: config.FontIndex,
                fontSize: (int)config.FontSize,
                charRanges: config.CharacterRanges ?? CharRangeSource.GetBuiltInCharRange(config.GetLanguage()),
                spacingHoriz: (int)config.Spacing,
                charOffsetX: config.CharOffsetX,
                charOffsetY: config.CharOffsetY);

            return new BmFontData(fontFile, pages, config.PixelZoom);
        }

        private static void EditBmFont(FontFile fontFile, FontConfig config)
        {
            BmFontGenerator.EditExisting(
                existingFont: fontFile,
                overrideSpacing: config.Spacing,
                overrideLineSpacing: config.LineSpacing,
                extraCharOffsetX: config.CharOffsetX,
                extraCharOffsetY: config.CharOffsetY);
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

        private record BmFontEditData(FontConfig Config, EditMode EditMode, BmFontData Font) : IDisposable
        {
            public void Dispose()
            {
                this.Font?.Dispose();
            }
        }

        private record BmFontData(FontFile FontFile, Texture2D[] Pages, float PixelZoom) : IDisposable
        {
            public void Dispose()
            {
                foreach (Texture2D page in this.Pages)
                    page.Dispose();
            }
        }

        private enum EditMode
        {
            DoNothing,
            Edit,
            Replace
        }
    }
}
