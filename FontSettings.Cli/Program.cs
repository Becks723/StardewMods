using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using Assimp;
using BmFont;
using CommandLine;
using CommandLine.Text;
using FontSettings.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SkiaSharp;

namespace FontSettings.CommandLine
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var parser = new Parser(settings => settings.CaseInsensitiveEnumValues = true);

            var parseResult = parser.ParseArguments<MakeFontOption>(args);

            parseResult.WithParsed<MakeFontOption>(option =>
                  {
                      Directory.CreateDirectory(option.OutputDirectory);
                      Debug.Assert(File.Exists(option.FontFilePath));

                      string successPath = null;

                      switch (option.OutputFormat)
                      {
                          case FontFormat.SpriteFont:
                              SpriteFontMetadata font = SpriteFontGenerator.GenerateMetadata(
                                  ttfPath: option.FontFilePath,
                                  fontIndex: option.FontIndex,
                                  fontPixelHeight: option.FontSize.InPixels(),
                                  characterRanges: TestCharacterRanges(),
                                  spacing: option.Spacing,
                                  lineSpacing: option.LineSpacing,
                                  charOffsetX: option.CharOffsetX,
                                  charOffsetY: option.CharOffsetY);

                              if (option.InXnb)
                                  successPath = WriteXnbSpriteFont(font, option);
                              else
                                  successPath = WritePlainSpriteFont(font, option);
                              break;

                          case FontFormat.BmFont:
                              BmFontMetadata bmFont = BmFontGenerator.GenerateMetadata(
                                  name: option.OutputName,
                                  fontFilePath: option.FontFilePath,
                                  fontIndex: option.FontIndex,
                                  fontSize: option.FontSize.InPixels(),
                                  charRanges: TestCharacterRanges(),
                                  spacing: option.Spacing,
                                  lineSpacing: option.LineSpacing,
                                  charOffsetX: option.CharOffsetX,
                                  charOffsetY: option.CharOffsetY,
                                  pageWidth: option.PageSize?.Width,
                                  pageHeight: option.PageSize?.Height);

                              if (option.InXnb)
                                  successPath = WriteXnbBmFont(bmFont, option);
                              else
                                  successPath = WritePlainBmFont(bmFont, option);
                              break;
                      }

                      Console.WriteLine($"Successfully saved to {successPath}");
                  })
                .WithNotParsed(errors =>
                {
                    var helpText = HelpText.AutoBuild(parseResult);
                    Console.WriteLine(helpText);
                });

#if DEBUG
            Console.WriteLine();
            Console.WriteLine("Program has ended. Press any key to exit.");
            Console.ReadKey();
#endif
        }

        static IEnumerable<CharacterRange> TestCharacterRanges()
        {
            yield return new CharacterRange(32, 128);
        }

        static string WriteXnbSpriteFont(SpriteFontMetadata font, MakeFontOption option)
        {
            Texture2DContent texture2DContent = Texture2DContent(font.Width, font.Height, font.Pixels);

            var fontContent = new SpriteFontContent()
            {
                Texture = texture2DContent,
                Glyphs = font.Bounds,
                Cropping = font.Cropping,
                CharacterMap = font.Characters,
                VerticalLineSpacing = font.LineSpacing,
                HorizontalSpacing = font.Spacing,
                Kerning = font.Kerning,
                DefaultCharacter = font.DefaultCharacter
            };

            string outputPath = Path.Combine(option.OutputDirectory, $"{option.OutputName}.xnb");
            SaveXnb(
                xnbPath: outputPath,
                content: fontContent);

            return outputPath;
        }

        private static Texture2DContent Texture2DContent(Texture2D texture)
        {
            /* Save to file, then import. */
            {
                /*
                string tempPath = Path.Combine(Path.GetTempPath(), $"makefont-{DateTime.Now:yyyy:MM:dd-HH:mm:ss}.png");
                using (Stream stream = File.OpenWrite(tempPath))
                    texture.SaveAsPng(stream, texture.Width, texture.Height);

                Texture2DContent texture2DContent;
                try
                {
                    var importer = new TextureImporter();
                    texture2DContent = importer.Import(tempPath, new FakeImporterContext()) as Texture2DContent;
                }
                finally
                {
                    File.Delete(tempPath);
                }
                return texture2DContent;
                */
            }

            /* Manually convert. */
            {
                Texture2DContent content = new Texture2DContent();
                BitmapContent face = new PixelBitmapContent<Color>(texture.Width, texture.Height);
                byte[] pixels = new byte[texture.Width * texture.Height];
                texture.GetData(pixels);
                face.SetPixelData(pixels);
                content.Faces[0].Add(face);
                return content;
            }
        }

        private static Texture2DContent Texture2DContent(int width, int height, byte[] pixels)
        {
            Texture2DContent content = new Texture2DContent();
            BitmapContent face = new PixelBitmapContent<Color>(width, height);

            var colors = new byte[width * height * 4];
            for (int i = 0; i < pixels.Length; i++)
            {
                byte b = pixels[i];
                colors[i * 4] = b;
                colors[i * 4 + 1] = b;
                colors[i * 4 + 2] = b;
                colors[i * 4 + 3] = b;
            }

            face.SetPixelData(colors);
            content.Faces[0].Add(face);
            return content;
        }

        static string WritePlainSpriteFont(SpriteFontMetadata font, MakeFontOption option)
        {
            var successPath = option.OutputDirectory;

            // png for texture
            {
                SaveTexture(
                    outputDirectory: option.OutputDirectory,
                    outputName: option.OutputName,
                    pixels: font.Pixels,
                    width: font.Width,
                    height: font.Height);
            }

            // json for font info
            {
                var fontInfo = GetFontInfo(font);
                string json = JsonConvert.SerializeObject(fontInfo, Formatting.Indented);

                string jsonPath = Path.Combine(option.OutputDirectory, $"{option.OutputName}.json");
                File.WriteAllText(jsonPath, json);
            }

            return successPath;
        }

        /// <summary>Get a human readable font info for json.</summary>
        private static object GetFontInfo(SpriteFontMetadata font)
        {
            return new
            {
                Spacing = font.Spacing,
                LineSpacing = font.LineSpacing,
                DefaultCharacter = font.DefaultCharacter,
                Characters = font.Characters,
                Glyphs = GetGlyphs(font)
            };
        }

        private static object GetGlyphs(SpriteFontMetadata font)
        {
            var dictionary = new Dictionary<char, SpriteFont.Glyph>();
            for (int i = 0; i < font.Characters.Count; i++)
            {
                dictionary.Add(font.Characters[i], new SpriteFont.Glyph
                {
                    Character = font.Characters[i],
                    BoundsInTexture = font.Bounds[i],
                    Cropping = font.Cropping[i],
                    LeftSideBearing = font.Kerning[i].X,
                    Width = font.Kerning[i].Y,
                    RightSideBearing = font.Kerning[i].Z,
                    WidthIncludingBearings = font.Kerning[i].X + font.Kerning[i].Y + font.Kerning[i].Z
                });
            }

            return dictionary;
        }

        private static string WriteXnbBmFont(BmFontMetadata font, MakeFontOption option)
        {
            string successPath = option.OutputDirectory;

            var contentCompiler = new ContentCompiler();

            // pages
            for (int i = 0; i < font.Pages.Length; i++)
            {
                var page = font.Pages[i];
                int width = font.FontFile.Common.ScaleW;
                int height = font.FontFile.Common.ScaleH;

                var texture2DContent = Texture2DContent(width, height, page.Pixels);

                string fileName = font.FontFile.Pages[i].File;
                string outputPath = Path.Combine(option.OutputDirectory, $"{fileName}.xnb");
                SaveXnb(
                    xnbPath: outputPath,
                    content: texture2DContent);
            }

            // fnt
            {
                XmlSource xmlSource = MakeFontUtils.SerializeFontFile(font.FontFile);

                string outputPath = Path.Combine(option.OutputDirectory, $"{option.OutputName}.xnb");
                SaveXnb(
                    xnbPath: outputPath,
                    content: xmlSource);
            }

            return successPath;
        }

        private static string WritePlainBmFont(BmFontMetadata font, MakeFontOption option)
        {
            string successPath = option.OutputDirectory;

            // pages
            for (int i = 0; i < font.Pages.Length; i++)
            {
                var page = font.Pages[i];
                int width = font.FontFile.Common.ScaleW;
                int height = font.FontFile.Common.ScaleH;

                string fileName = font.FontFile.Pages[i].File;
                SaveTexture(
                    outputDirectory: option.OutputDirectory,
                    outputName: fileName,
                    pixels: page.Pixels,
                    width: width,
                    height: height);
            }

            // fnt
            {
                XmlSource xmlSource = MakeFontUtils.SerializeFontFile(font.FontFile);

                string fntPath = Path.Combine(option.OutputDirectory, $"{option.OutputName}.fnt");
                File.WriteAllText(fntPath, xmlSource.Source);
            }

            return successPath;
        }

        private static void SaveTexture(string outputDirectory, string outputName, byte[] pixels, int width, int height)
        {
            var colors = ColorPixels(pixels, width, height);

            var bitmap = new SKBitmap();

            var gcHandle = GCHandle.Alloc(colors, GCHandleType.Pinned);

            var info = new SKImageInfo(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Unpremul);
            bitmap.InstallPixels(info, gcHandle.AddrOfPinnedObject());

            gcHandle.Free();

            string pngPath = Path.Combine(outputDirectory, $"{outputName}.png");
            using (Stream stream = File.OpenWrite(pngPath))
            {
                var image = SKImage.FromBitmap(bitmap);
                var data = image.Encode(SKEncodedImageFormat.Png, 100);
                data.SaveTo(stream);
            }
        }

        private static byte[] ColorPixels(byte[] pixels, int width, int height)
        {
            var colors = new byte[width * height * 4];
            for (int i = 0; i < pixels.Length; i++)
            {
                byte b = pixels[i];
                colors[i * 4] = b;
                colors[i * 4 + 1] = b;
                colors[i * 4 + 2] = b;
                colors[i * 4 + 3] = b;
            }

            return colors;
        }

        private static readonly Lazy<ContentCompiler> _contentCompiler = new(() => new ContentCompiler());
        private static void SaveXnb(
            string xnbPath, // fullpath, including .xnb extension.
            object content,
            TargetPlatform targetPlatform = TargetPlatform.DesktopGL,
            GraphicsProfile targetProfile = GraphicsProfile.HiDef,
            bool compressContent = true,
            string? rootDirectory = null,
            string? referenceRelocationPath = null)
        {
            rootDirectory ??= Path.GetDirectoryName(xnbPath);
            referenceRelocationPath ??= Path.GetDirectoryName(xnbPath);

            var contentCompiler = _contentCompiler.Value;

            using (Stream stream = File.OpenWrite(xnbPath))
                contentCompiler.Compile(
                    stream: stream,
                    content: content,
                    targetPlatform: targetPlatform,
                    targetProfile: targetProfile,
                    compressContent: compressContent,
                    rootDirectory: rootDirectory,
                    referenceRelocationPath: referenceRelocationPath);
        }
    }
}
