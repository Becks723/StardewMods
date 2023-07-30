using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FontSettings.Framework.Fonts
{
    internal class SpriteTextFont : SpriteFontBase
    {
        private readonly SpriteTextObject _spriteText;

        public SpriteTextFont(BmFontData? bmFont, float pixelZoom, LanguageInfo language, Func<bool> bmFontInLatinLanguages)
        {
            this._spriteText = new SpriteTextObject(bmFont, pixelZoom, language, bmFontInLatinLanguages);
        }

        public override void Draw(SpriteBatch b, string text, Vector2 position, Color color)
        {
            this._spriteText.drawString(b, text, (int)position.X, (int)position.Y, customColor: color);
        }

        public override Vector2 MeasureString(string text)
        {
            return new Vector2(
                this._spriteText.getWidthOfString(text),
                this._spriteText.getHeightOfString(text));
        }

        public override void DrawBounds(SpriteBatch b, string text, Vector2 position, Color color)
        {
        }
    }
}
