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

namespace FontSettings.Framework.Exporting
{
    internal class FontExporter : IFontExporter
    {
        public async Task<IResultWithoutData> Export(FontConfig config, FontExportSettings settings)
        {
            try
            {
                Directory.CreateDirectory(settings.OutputDirectory);

                if (settings.Format == FontFormat.SpriteFont)
                {
                    SpriteFont spriteFont = await Task.Run(() => SpriteFontGenerator.FromTtf(
                        ttfPath: config.FontFilePath,
                        fontIndex: config.FontIndex,
                        fontPixelHeight: config.FontSize,
                        characterRanges: config.CharacterRanges,
                        spacing: config.Spacing,
                        lineSpacing: (int)config.LineSpacing,
                        charOffsetX: config.CharOffsetX,
                        charOffsetY: config.CharOffsetY,
                        defaultCharacter: config.TryGetInstance(out IWithDefaultCharacter withDefaultCharacter)
                            ? withDefaultCharacter.DefaultCharacter
                            : '*'));

                    if (settings.InXnb)
                    {
                        string filepath = Path.Combine(settings.OutputDirectory, $"{settings.OutputFileName}.xnb");
                        using Stream stream = File.Open(filepath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
                        using var writer = new XnbWriter(stream, settings.XnbPlatform, settings.GameFramework, settings.GraphicsProfile, settings.IsCompressed);

                        writer.WritePrimaryObject(spriteFont);

                        return ResultFactory.SuccessResultWithoutData();
                    }
                }

                else if (settings.Format == FontFormat.BmFont)
                {
                    FontFile fontFile = null;
                    Texture2D[] pages = null;
                    await Task.Run(() => BmFontGenerator.GenerateIntoMemory(
                        fontFilePath: config.FontFilePath,
                        fontFile: out fontFile,
                        pages: out pages,
                        fontIndex: config.FontIndex,
                        fontSize: (int)config.FontSize,
                        charRanges: config.CharacterRanges,
                        spacingHoriz: (int)config.Spacing,
                        charOffsetX: config.CharOffsetX,
                        charOffsetY: config.CharOffsetY));

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

                        return ResultFactory.SuccessResultWithoutData();
                    }
                }

                return ResultFactory.ErrorResultWithoutData(
                    exception: new NotSupportedException());

            }
            catch (Exception ex)
            {
                return ResultFactory.ErrorResultWithoutData(ex);
            }
        }
    }
}
