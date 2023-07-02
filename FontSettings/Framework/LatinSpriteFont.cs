using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using static StardewValley.BellsAndWhistles.SpriteText;

namespace FontSettings.Framework
{
    internal class LatinSpriteFont : ISpriteFont
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            if (!this.IsDisposed)
                this.IsDisposed = true;
        }

        public void Draw(SpriteBatch b, string text, Vector2 position, Color color)
        {
            drawString(b, text, (int)position.X, (int)position.Y);
        }

        public Vector2 MeasureString(string text)
        {
            return new Vector2(
                this.GetWidthOfString(text),
                this.GetHeightOfString(text));
        }

        private static void drawString(SpriteBatch b, string s, int x, int y, int characterPosition = 999999, int width = -1, int height = 999999, float alpha = 1f, float layerDepth = 0.88f, bool junimoText = false, int color = -1)
        {
            float fontZoom = 3f;
            width = Game1.graphics.GraphicsDevice.Viewport.Width - x;

            if (fontZoom < 4f && LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.ko)
            {
                y += (int)((4f - fontZoom) * 4f);
            }

            Vector2 position = new Vector2(x, y);
            int num = 0;
            if (position.X + width > Game1.graphics.GraphicsDevice.Viewport.Width - 4)
            {
                position.X = Game1.graphics.GraphicsDevice.Viewport.Width - width - 4;
            }

            if (position.X < 0f)
                position.X = 0f;

            s = s.Replace(Environment.NewLine, "");
            if (!junimoText && (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.th || (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.mod && LocalizedContentManager.CurrentModLanguage.FontApplyYOffset)))
            {
                position.Y -= (4f - fontZoom) * 4f;
            }

            s = s.Replace('♡', '<');
            for (int i = 0; i < Math.Min(s.Length, characterPosition); i++)
            {
                float tempZoom = fontZoom;
                if (IsSpecialCharacter(s[i]) || junimoText)
                {
                    fontZoom = 3f;
                }

                if (s[i] == '^')
                {
                    position.Y += 18f * fontZoom;
                    position.X = x;
                    num = 0;
                    fontZoom = tempZoom;
                    continue;
                }

                num = (int)(0f * fontZoom);
                bool flag2 = char.IsUpper(s[i]) || s[i] == 'ß';
                Vector2 value2 = new Vector2(0f, -1 + ((!junimoText && flag2) ? (-3) : 0));
                if (s[i] == 'Ç')
                {
                    value2.Y += 2f;
                }

                if (positionOfNextSpace(s, i, (int)position.X - x, num) >= width)
                {
                    position.Y += 18f * fontZoom;
                    num = 0;
                    position.X = x;
                    if (s[i] == ' ')
                    {
                        fontZoom = tempZoom;
                        continue;
                    }
                }

                b.Draw(
                    (color != -1) ? coloredTexture : spriteTexture,
                    position + value2 * fontZoom,
                    getSourceRectForChar(s[i], junimoText),
                    ((IsSpecialCharacter(s[i]) || junimoText) ? Color.White : getColorFromIndex(color)) * alpha,
                    0f,
                    Vector2.Zero,
                    fontZoom,
                    SpriteEffects.None,
                    layerDepth);
                if (i < s.Length - 1)
                {
                    position.X += 8f * fontZoom + num + getWidthOffsetForChar(s[i + 1]) * fontZoom;
                }

                if (s[i] != '^')
                {
                    position.X += getWidthOffsetForChar(s[i]) * fontZoom;
                }

                fontZoom = tempZoom;
            }
        }

        private int GetWidthOfString(string s)
        {
            int num = 0;
            int num2 = 0;
            for (int i = 0; i < s.Length; i++)
            {
                num += 8 + getWidthOffsetForChar(s[i]);
                if (i > 0)
                {
                    num += getWidthOffsetForChar(s[Math.Max(0, i - 1)]);
                }

                num2 = Math.Max(num, num2);
                if (s[i] == '^')
                    num = 0;
            }

            return (int)(num2 * fontPixelZoom);
        }

        private int GetHeightOfString(string s)
        {
            if (s.Length == 0) return 0;

            Vector2 vector = default;
            int num = 0;
            s = s.Replace(Environment.NewLine, "");

            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '^')
                {
                    vector.Y += 18f * fontPixelZoom;
                    vector.X = 0f;
                    num = 0;
                    continue;
                }

                vector.X += 8f * fontPixelZoom + num + getWidthOffsetForChar(s[i]) * fontPixelZoom;
                if (i > 0)
                {
                    vector.X += getWidthOffsetForChar(s[i - 1]) * fontPixelZoom;
                }

                num = (int)(0f * fontPixelZoom);
            }

            return (int)(vector.Y + 16f * fontPixelZoom);
        }

        private static bool IsSpecialCharacter(char c)
        {
            return c is '<' or '>' or '=' or '@' or '$' or '`' or '+';
        }

        private static Rectangle getSourceRectForChar(char c, bool junimoText)
        {
            int num = c - 32;
            switch (c)
            {
                case 'Œ':
                    num = 96;
                    break;
                case 'œ':
                    num = 97;
                    break;
                case 'Ğ':
                    num = 102;
                    break;
                case 'ğ':
                    num = 103;
                    break;
                case 'İ':
                    num = 98;
                    break;
                case 'ı':
                    num = 99;
                    break;
                case 'Ş':
                    num = 100;
                    break;
                case 'ş':
                    num = 101;
                    break;
                case '’':
                    num = 104;
                    break;
                case 'Ő':
                    num = 105;
                    break;
                case 'ő':
                    num = 106;
                    break;
                case 'Ű':
                    num = 107;
                    break;
                case 'ű':
                    num = 108;
                    break;
            }

            return new Rectangle(num * 8 % spriteTexture.Width, num * 8 / spriteTexture.Width * 16 + (junimoText ? 224 : 0), 8, 16);
        }
    }
}
