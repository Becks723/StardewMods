using BmFont;
using Microsoft.Xna.Framework.Graphics;

namespace FontSettings.Framework.Models
{
    internal class BmFontData
    {
        public FontFile FontFile { get; }
        public Texture2D[] Pages { get; }

        public BmFontData(FontFile fontFile, Texture2D[] pages)
        {
            this.FontFile = fontFile;
            this.Pages = pages;
        }
    }
}
