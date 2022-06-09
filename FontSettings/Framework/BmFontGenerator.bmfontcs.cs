using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BmFont;
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
            int spacingHoriz, int spacingVert)
        {
            BmFontCS.BmFont.GenerateIntoMemory(fontFilePath, out fontFile, out pages, new BmFontCS.BmFontSettings
            {
                FontSize = fontSize,
                FontIndex = fontIndex,
                Chars = chars.Select(range => new BmFontCS.UnicodeRange { Start = range.Start, End = range.End }).ToArray(),
                CharsFiles = charsFiles,
                Padding = new BmFontCS.Padding(paddingUp, paddingRight, paddingDown, paddingLeft),
                Spacing = new BmFontCS.Spacing(spacingHoriz, spacingVert)
            });
        }
    }
}
