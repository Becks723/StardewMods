using BmFont;
using Microsoft.Xna.Framework.Graphics;

namespace FontSettings.Framework.Models
{
    internal class BmFontData
    {
        public FontFile FontFile { get; set; }
        public Texture2D[] Pages { get; set; }
    }
}
