using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BmFont;
using FontSettings.Framework.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace FontSettings.Framework.Exporting
{
    internal class FontExporter : IFontExporter
    {
        public async Task<IResultWithoutData> Export(FontConfig config, FontExportSettings settings)
        {
            try
            {
                // 检查文件夹。
                Directory.CreateDirectory(settings.OutputDirectory);

                // 检查文件名。
                {
                    char[] invalidNameChars = Path.GetInvalidFileNameChars();
                    var invalidChars = invalidNameChars.Where(c => settings.OutputFileName.Contains(c));
                    if (invalidChars.Any())
                        throw new ArgumentException($"Filename '{settings.OutputFileName}' cannot contain the following char(s): {string.Join(", ", invalidChars)}");
                }

                if (settings.Format == FontFormat.SpriteFont)
                {
                    SpriteFont spriteFont = await SpriteFontGenerator.GenerateAsync(config);

                    if (settings.InXnb)
                    {
                        string filepath = Path.Combine(settings.OutputDirectory, $"{settings.OutputFileName}.xnb");
                        using Stream stream = File.Open(filepath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
                        using var writer = new XnbWriter(stream, settings.XnbPlatform, settings.GameFramework, settings.GraphicsProfile, settings.IsCompressed);

                        writer.WritePrimaryObject(spriteFont);
                    }
                    else
                    {
                        // png for texture
                        {
                            string pngPath = Path.Combine(settings.OutputDirectory, $"{settings.OutputFileName}.png");
                            using Stream stream = File.Open(pngPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
                            Texture2D texture = spriteFont.Texture;
                            FontHelpers.BlockOnUIThread(
                                () => texture.SaveAsPng(stream, texture.Width, texture.Height));
                        }

                        // json for font info
                        {
                            var fontInfo = new
                            {
                                spriteFont.LineSpacing,
                                spriteFont.Spacing,
                                spriteFont.DefaultCharacter,
                                spriteFont.Characters,
                                Glyphs = spriteFont.GetGlyphs()
                            };
                            string json = JsonConvert.SerializeObject(fontInfo, Formatting.Indented);

                            string jsonPath = Path.Combine(settings.OutputDirectory, $"{settings.OutputFileName}.json");
                            File.WriteAllText(jsonPath, json);
                        }
                    }
                }

                else if (settings.Format == FontFormat.BmFont)
                {
                    var bmFont = await BmFontGenerator.GenerateAsync(config);
                    FontFile fontFile = bmFont.FontFile;
                    Texture2D[] pages = bmFont.Pages;

                    if (settings.InXnb)
                    {
                        string filepath = Path.Combine(settings.OutputDirectory, $"{settings.OutputFileName}.xnb");
                        using Stream stream = File.Open(filepath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
                        using var writer = new XnbWriter(stream, settings.XnbPlatform, settings.GameFramework, settings.GraphicsProfile, settings.IsCompressed);

                        XmlSource xmlSource = FontHelpers.ParseFontFile(fontFile);
                        writer.WritePrimaryObject(xmlSource);

                        int index = 0;
                        foreach (FontPage fontPage in fontFile.Pages)
                        {
                            string pageName = fontPage.File;
                            string pagePath = Path.Combine(settings.OutputDirectory, $"{pageName}.xnb");
                            using Stream pageStream = File.Open(pagePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
                            using var pageWriter = new XnbWriter(pageStream, settings.XnbPlatform, settings.GameFramework, settings.GraphicsProfile, settings.IsCompressed);

                            Texture2D page = pages[index++];
                            pageWriter.WritePrimaryObject(page);
                        }
                    }
                    else
                    {
                        // .fnt (font file)
                        {
                            string xml = FontHelpers.ParseFontFile(fontFile).Source;
                            string fntPath = Path.Combine(settings.OutputDirectory, $"{settings.OutputFileName}.fnt");
                            File.WriteAllText(fntPath, xml);
                        }

                        // .png (pages)
                        {
                            int index = 0;
                            foreach (FontPage fontPage in fontFile.Pages)
                            {
                                string pngPath = Path.Combine(settings.OutputDirectory, $"{fontPage.File}.png");
                                using Stream pageStream = File.Open(pngPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
                                Texture2D page = pages[index++];
                                FontHelpers.BlockOnUIThread(() =>
                                    page.SaveAsPng(pageStream, page.Width, page.Height));
                            }
                        }
                    }
                }

                return ResultFactory.SuccessResultWithoutData();
            }
            catch (Exception ex)
            {
                return ResultFactory.ErrorResultWithoutData(ex);
            }
        }
    }
}
