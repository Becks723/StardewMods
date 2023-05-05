using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BmFont;
using FontSettings.Framework.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;

namespace FontSettings.Framework.Fonts
{
    internal class SpriteTextFont2 : SpriteFontBase
    {
        private FontFile _cachedFontFile;
        private List<Texture2D> _cachedFontPages;
        private Dictionary<char, FontChar> _cachedCharacterMap;

        private readonly FontFile _instanceFontFile;
        private readonly List<Texture2D> _instanceFontPages;
        private readonly Dictionary<char, FontChar> _instanceCharacterMap;

        public SpriteTextFont2(BmFontData bmFont)
        {
            if (bmFont is null)
                throw new ArgumentNullException(nameof(bmFont));

            this._instanceFontFile = bmFont.FontFile;
            this._instanceFontPages = new(bmFont.Pages);
            this._instanceCharacterMap = bmFont.FontFile.Chars.ToDictionary(fontChar => (char)fontChar.ID);
        }

        public override void Draw(SpriteBatch b, string text, Vector2 position, Color color)
        {
            try
            {
                this.BeforeSpriteTextdrawString();
                SpriteText.drawString(b, text, (int)position.X, (int)position.Y);
            }
            finally
            {
                this.AfterSpriteTextdrawString();
            }
        }

        public override Vector2 MeasureString(string text)
        {
            try
            {
                this.BeforeSpriteTextdrawString();
                return new Vector2(
                    SpriteText.getWidthOfString(text),
                    SpriteText.getHeightOfString(text));
            }
            finally
            {
                this.AfterSpriteTextdrawString();
            }
        }

        public override void DrawBounds(SpriteBatch b, string text, Vector2 position, Color color)
        {
        }

        private void BeforeSpriteTextdrawString()
        {
            this._cachedFontFile = SpriteTextFields.FontFile;
            this._cachedFontPages = SpriteTextFields.fontPages;
            this._cachedCharacterMap = SpriteTextFields._characterMap;

            SpriteTextFields.FontFile = this._instanceFontFile;
            SpriteTextFields.fontPages = this._instanceFontPages;
            SpriteTextFields._characterMap = this._instanceCharacterMap;
        }

        private void AfterSpriteTextdrawString()
        {
            SpriteTextFields.FontFile = this._cachedFontFile;
            SpriteTextFields.fontPages = this._cachedFontPages;
            SpriteTextFields._characterMap = this._cachedCharacterMap;
        }
    }
}
