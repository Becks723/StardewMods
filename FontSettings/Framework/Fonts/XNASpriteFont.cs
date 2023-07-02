using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using static Microsoft.Xna.Framework.Graphics.SpriteFont;

namespace FontSettings.Framework.Fonts
{
    internal class XNASpriteFont : SpriteFontBase
    {
        public SpriteFont InnerFont { get; }

        public XNASpriteFont(SpriteFont innerFont)
        {
            this.InnerFont = innerFont ?? throw new ArgumentNullException(nameof(innerFont));
        }

        public override void Draw(SpriteBatch b, string text, Vector2 position, Color color)
        {
            b.DrawString(this.InnerFont, text, position, color);
        }

        public override unsafe void DrawBounds(SpriteBatch b, string text, Vector2 position, Color color)
        {
            if (string.IsNullOrEmpty(text)) return;

            var glyphData = this.InnerFont.GetGlyphs();
            Vector2 offset = Vector2.Zero;
            bool firstGlyphOfLine = false;
            foreach (char c in text)
            {
                Glyph glyph;
                if (!glyphData.TryGetValue(c, out glyph))
                    if (this.InnerFont.DefaultCharacter.HasValue)
                        if (!glyphData.TryGetValue(this.InnerFont.DefaultCharacter.Value, out glyph))
                            continue;
                    else
                        continue;

                switch (c)
                {
                    case '\r':
                        continue;

                    case '\n':
                        firstGlyphOfLine = true;
                        offset.X = 0;
                        offset.Y += this.InnerFont.LineSpacing;
                        continue;
                }

                if (firstGlyphOfLine)
                {
                    offset.X = Math.Max(glyph.LeftSideBearing, 0);
                    firstGlyphOfLine = false;
                }
                else
                    offset.X += this.InnerFont.Spacing + glyph.LeftSideBearing;

                var p = offset;
                p += position;
                p.X += glyph.Cropping.X;
                p.Y += glyph.Cropping.Y;

                b.Draw(Game1.staminaRect, new Rectangle((int)p.X, (int)p.Y, glyph.BoundsInTexture.Width, glyph.BoundsInTexture.Height), color);

                offset.X += glyph.Width + glyph.RightSideBearing;
            }
        }

        public override Vector2 MeasureString(string text)
        {
            return this.InnerFont.MeasureString(text);
        }
    }
}
