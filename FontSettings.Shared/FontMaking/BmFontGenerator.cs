using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BmFont;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace FontSettings.Framework
{
    internal partial class BmFontGenerator
    {
        private const int DefaultFontIndex = 0;
        private const int DefaultFontSize = 24;
        private const int DefaultPaddingUp = 0;
        private const int DefaultPaddingRight = 0;
        private const int DefaultPaddingDown = 0;
        private const int DefaultPaddingLeft = 0;
        private const int DefaultSpacingHoriz = 1;
        private const int DefaultSpacingVert = 1;
        private static readonly IEnumerable<CharacterRange> DefaultCharRanges = new[] { new CharacterRange(32, 126) };
        private static readonly string[] DefaultCharsFiles = Array.Empty<string>();

        private static string _baseDir;
        private static string _tmpDir;

        public static void GenerateIntoMemory(string fontFilePath,
            out FontFile fontFile, out Texture2D[] pages,
            int? fontIndex = null, int? fontSize = null,
            IEnumerable<CharacterRange>? charRanges = null, string[]? charsFiles = null,
            int? paddingUp = null, int? paddingRight = null, int? paddingDown = null, int? paddingLeft = null,
            int? spacingHoriz = null, int? spacingVert = null,
            float charOffsetX = 0, float charOffsetY = 0)
        {
            string finalFontFile = fontFilePath ?? throw new ArgumentNullException(nameof(fontFilePath));
            int finalFontIndex = fontIndex ?? DefaultFontIndex;
            int finalFontSize = fontSize ?? DefaultFontSize;
            int finalPaddingUp = paddingUp ?? DefaultPaddingUp;
            int finalPaddingRight = paddingRight ?? DefaultPaddingRight;
            int finalPaddingDown = paddingDown ?? DefaultPaddingDown;
            int finalPaddingLeft = paddingLeft ?? DefaultPaddingLeft;
            int finalSpacingHoriz = spacingHoriz ?? DefaultSpacingHoriz;
            int finalSpacingVert = spacingVert ?? DefaultSpacingVert;
            var finalChars = charRanges ?? DefaultCharRanges;
            string[] finalCharsFiles = charsFiles ?? DefaultCharsFiles;
            float finalOffsetX = charOffsetX;
            float finalOffsetY = charOffsetY;

            InternalGenerateIntoMemory(finalFontFile,
                out fontFile, out pages,
                finalFontIndex, finalFontSize,
                finalChars.ToArray(), finalCharsFiles,
                finalPaddingUp, finalPaddingRight, finalPaddingDown, finalPaddingLeft,
                finalSpacingHoriz, finalSpacingVert,
                finalOffsetX, finalOffsetY);
        }

        public static void LoadBmFont(string fntPathWithoutExtension, out FontFile fontFile, out Texture2D[] pages)
        {
            string fntPath = fntPathWithoutExtension + ".fnt";
            fontFile = FontLoader.Parse(File.ReadAllText(fntPath));

            // 加载图片手动查找路径，而不用fnt文件中的现成路径，是因为路径可能带中文，而fnt编码不一定是utf-8。
            List<string> pagePaths = new();
            string fntDir = Path.GetDirectoryName(fntPath);
            string fntName = Path.GetFileNameWithoutExtension(fntPath);
            string[] pngs = Directory.EnumerateFiles(fntDir, $"{fntName}_*.png",
                SearchOption.TopDirectoryOnly).ToArray();
            bool d2 = fontFile.Pages.Count >= 10;
            foreach (FontPage page in fontFile.Pages)
            {
                int id = page.ID;
                string idStr = d2 ? id.ToString("D2") : id.ToString();
                foreach (string png in pngs)
                {
                    string pngName = Path.GetFileNameWithoutExtension(png);
                    string numSuffix = pngName.Substring(pngName.LastIndexOf('_') + 1);
                    if (!int.TryParse(numSuffix, out int num))
                        continue;

                    if (id == num && idStr == numSuffix)
                        pagePaths.Add(png);
                }
            }

            if (pagePaths.Count != fontFile.Pages.Count)
                throw new FileNotFoundException($"名为'{fntName}'的位图字体需要{fontFile.Pages.Count}张png图片，但只找到{pagePaths.Count}张，重新生成位图字体可能会解决问题。");

            pages = pagePaths.Select(path => Texture2D.FromFile(Game1.graphics.GraphicsDevice, path)).ToArray();
        }

        // outputDir: 输出文件夹的完整路径。
        // outputName: 输出文件的名称（不带扩展名）。
        [Obsolete("现在可以直接生成进内存。就用不上先生成文件，再读进内存了。")]
        public static void GenerateFile(string fontFilePath, out string outputDir, out string outputName,
            int? fontIndex = null, int? fontSize = null,
            IEnumerable<CharacterRange>? charRanges = null, string[]? charsFiles = null,
            int? paddingUp = null, int? paddingLeft = null, int? paddingDown = null, int? paddingRight = null,
            int? spacingHoriz = null, int? spacingVert = null)
        {
            GenerateFile(fontFilePath, _baseDir, Path.GetRelativePath(_baseDir, _tmpDir), null, out string outputPath,
                fontIndex, fontSize, charRanges, charsFiles, paddingUp, paddingLeft, paddingDown, paddingRight, spacingHoriz, spacingVert);

            string outputPathWithExtension = outputPath + ".fnt";
            outputDir = Path.GetDirectoryName(outputPathWithExtension);
            outputName = Path.GetFileNameWithoutExtension(outputPathWithExtension);
        }

        [Obsolete("现在可以直接生成进内存。就用不上先生成文件，再读进内存了。")]
        public static void GenerateFile(string fontFilePath, string baseDir, string outputDir, string? outputName, out string outputPath,
            int? fontIndex = null, int? fontSize = null,
            IEnumerable<CharacterRange>? charRanges = null, string[]? charsFiles = null,
            int? paddingUp = null, int? paddingLeft = null, int? paddingDown = null, int? paddingRight = null,
            int? spacingHoriz = null, int? spacingVert = null)
        {
            string finalFontFile = fontFilePath ?? throw new ArgumentNullException(nameof(fontFilePath));
            int finalFontIndex = fontIndex ?? DefaultFontIndex;
            int finalFontSize = fontSize ?? DefaultFontSize;
            string finalOutputName = outputName ?? $"{Path.GetFileNameWithoutExtension(fontFilePath)}-{finalFontSize}px-{DateTime.Now:HH-mm-ss}";
            var finalCharRanges = charRanges ?? DefaultCharRanges;
            string[] finalCharsFiles = charsFiles ?? DefaultCharsFiles;
            int finalPaddingUp = paddingUp ?? DefaultPaddingUp;
            int finalPaddingRight = paddingRight ?? DefaultPaddingRight;
            int finalPaddingDown = paddingDown ?? DefaultPaddingDown;
            int finalPaddingLeft = paddingLeft ?? DefaultPaddingLeft;
            int finalSpacingHoriz = spacingHoriz ?? DefaultSpacingHoriz;
            int finalSpacingVert = spacingVert ?? DefaultSpacingVert;

            InternalGenerateFile(finalFontFile, baseDir, outputDir, finalOutputName, out outputPath,
                finalFontIndex, finalFontSize,
                finalCharRanges, finalCharsFiles,
                finalPaddingUp, finalPaddingRight, finalPaddingDown, finalPaddingLeft,
                finalSpacingHoriz, finalSpacingVert);
        }

        // TODO: 支持size和charRange: 这两项需要重新生成图片，不像下面的只需简单地修改属性。
        public static void EditExisting(FontFile existingFont,
            float? overrideSpacing = null,
            float? overrideLineSpacing = null,
            float extraCharOffsetX = 0,
            float extraCharOffsetY = 0)
        {
            // line spacing.
            if (overrideLineSpacing != null)
                existingFont.Common.LineHeight = (int)overrideLineSpacing.Value;
        }
    }
}
