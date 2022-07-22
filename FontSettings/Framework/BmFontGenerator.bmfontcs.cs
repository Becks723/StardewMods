using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BmFont;
using BmFontCS;
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
            float charOffsetX, float charOffsetY)
        {
            var bmfont = new BmFontCS.BmFont();
            bmfont.GenerateIntoMemory(fontFilePath, out fontFile, out pages, new BmFontSettings
            {
                FontSize = fontSize,
                FontIndex = fontIndex,
                Chars = chars.Select(range => new UnicodeRange { Start = range.Start, End = range.End }).ToArray(),
                CharsFiles = charsFiles,
                Padding = new Padding(paddingUp, paddingRight, paddingDown, paddingLeft),
                Spacing = new Spacing(spacingHoriz, spacingVert)
            });

            // offset
            foreach (FontChar fontChar in fontFile.Chars)
            {
                fontChar.XOffset += (int)Math.Round(charOffsetX);
                fontChar.YOffset += (int)Math.Round(charOffsetY);
            }
        }
    }
}
