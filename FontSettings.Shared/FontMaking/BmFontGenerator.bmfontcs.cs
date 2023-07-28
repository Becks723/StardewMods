using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BmFont;
using BmFontCS;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FontSettings.Framework
{
    internal partial class BmFontGenerator
    {
        private static void InternalGenerateIntoMemory(string fontFilePath,
            out FontFile fontFile, out Texture2D[] pages,
            int fontIndex, int fontSize,
            CharacterRange[] chars, string[] charsFiles,
            int paddingUp, int paddingRight, int paddingDown, int paddingLeft,
            int spacingHoriz, int spacingVert,
            float charOffsetX, float charOffsetY,
            string pageName, 
            Color mask)
        {
            var bmfont = new BmFontCS.BmFont();
            bmfont.GenerateIntoMemory(fontFilePath, out fontFile, out pages, new BmFontSettings
            {
                FontSize = fontSize,
                FontIndex = fontIndex,
                Chars = chars.Select(range => new UnicodeRange { Start = range.Start, End = range.End }).ToArray(),
                CharsFiles = charsFiles,
                Padding = new Padding(paddingUp, paddingRight, paddingDown, paddingLeft),
                Spacing = new Spacing(spacingHoriz, spacingVert),
                Name = pageName,
                Mask = mask
            });

            // offset
            foreach (FontChar fontChar in fontFile.Chars)
            {
                fontChar.XOffset += (int)Math.Round(charOffsetX);
                fontChar.YOffset += (int)Math.Round(charOffsetY);
            }
        }

        public static BmFontMetadata GenerateMetadata(
            string name,
            string fontFilePath,
            int fontIndex,
            float fontSize,
            float spacing,
            int? lineSpacing,
            float charOffsetX,
            float charOffsetY,
            IEnumerable<CharacterRange> charRanges,
            int? pageWidth = null,
            int? pageHeight = null)
        {
            if ((pageWidth == null && pageHeight != null)
             || (pageWidth != null && pageHeight == null))
                throw new ArgumentException($"{nameof(pageWidth)} and {nameof(pageHeight)} must be both null or non-null.");

            int bitmapWidth = pageWidth ?? 512;
            int bitmapHeight = pageHeight ?? 512;

            var bmfont = new BmFontCS.BmFont();
            bmfont.GenerateIntoMemory(fontFilePath, out FontFile fontFile, out byte[][] pages, new BmFontSettings
            {
                FontSize = (int)Math.Round(fontSize),
                FontIndex = fontIndex,
                Chars = charRanges.Select(range => new UnicodeRange { Start = range.Start, End = range.End }).ToArray(),
                Spacing = new Spacing((int)Math.Round(spacing), 0),
                TextureSize = new Size(bitmapWidth, bitmapHeight),
                Name = name
            });

            // offset
            foreach (FontChar fontChar in fontFile.Chars)
            {
                fontChar.XOffset += (int)Math.Round(charOffsetX);
                fontChar.YOffset += (int)Math.Round(charOffsetY);
            }

            return new BmFontMetadata(
                FontFile: fontFile,
                Pages: pages.Select(p => new BmFontPageMetadata(p)).ToArray());
        }
    }
}
