using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BmFont;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace FontSettings.Framework
{
    internal class GameBitmapSpriteFont : BitmapSpriteFont
    {
        private Dictionary<char, FontChar> _characterMap;
        private float _fontPixelZoom = 3f;
        private LocalizedContentManager.LanguageCode _languageCode;

        public LocalizedContentManager.LanguageCode LanguageCode
        {
            get => this._languageCode;
            init
            {
                this._languageCode = value;
                this._fontPixelZoom = this.GetDefaultFontPixelZoom();
            }
        }

        public override FontFile FontFile
        {
            get => base.FontFile;
            init
            {
                base.FontFile = value;
                this._characterMap = new Dictionary<char, FontChar>(this.FontFile.Chars.Count);
                foreach (FontChar fontChar in this.FontFile.Chars)
                    this._characterMap.Add((char)fontChar.ID, fontChar);
            }
        }

        public float FontPixelZoom
        {
            get { return this._fontPixelZoom; }
            set { this._fontPixelZoom = value; }
        }

        public Dictionary<char, FontChar> CharacterMap
        {
            get { return this._characterMap; }
            set { this._characterMap = value; }
        }

        public override void Draw(SpriteBatch b, string text, Vector2 position, Color color)
        {
            this.drawString(b, text, (int)position.X, (int)position.Y, color);
        }

        public void drawString(SpriteBatch b, string s, int x, int y, Color color, int characterPosition = 999999, int width = -1, int height = 999999, float alpha = 1f, float layerDepth = 0.88f, bool junimoText = false, int drawBGScroll = -1, string placeHolderScrollWidthText = "")
        {
            bool width_specified = true;
            if (width == -1)
            {
                width_specified = false;
                width = Game1.graphics.GraphicsDevice.Viewport.Width - x;
                if (drawBGScroll == 1)
                {
                    width = this.GetWidthOfString(s) * 2;
                }
            }
            if (this.FontPixelZoom < 4f && this.LanguageCode != LocalizedContentManager.LanguageCode.ko)
            {
                y += (int)((4f - this.FontPixelZoom) * 4f);
            }
            Vector2 position = new Vector2(x, y);
            int accumulatedHorizontalSpaceBetweenCharacters = 0;
            if (drawBGScroll != 1)
            {
                if (position.X + width > Game1.graphics.GraphicsDevice.Viewport.Width - 4)
                {
                    position.X = Game1.graphics.GraphicsDevice.Viewport.Width - width - 4;
                }
                if (position.X < 0f)
                {
                    position.X = 0f;
                }
            }
            if (drawBGScroll == 0 || drawBGScroll == 2)
            {
                int scroll_width = this.GetWidthOfString((placeHolderScrollWidthText.Length > 0) ? placeHolderScrollWidthText : s);
                if (width_specified)
                {
                    scroll_width = width;
                }
                if (drawBGScroll == 0)
                {
                    b.Draw(Game1.mouseCursors, position + new Vector2(-12f, -3f) * 4f, new Rectangle(325, 318, 12, 18), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
                    b.Draw(Game1.mouseCursors, position + new Vector2(0f, -3f) * 4f, new Rectangle(337, 318, 1, 18), Color.White * alpha, 0f, Vector2.Zero, new Vector2(scroll_width, 4f), SpriteEffects.None, layerDepth - 0.001f);
                    b.Draw(Game1.mouseCursors, position + new Vector2(scroll_width, -12f), new Rectangle(338, 318, 12, 18), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
                }
                else if (drawBGScroll == 2)
                {
                    b.Draw(Game1.mouseCursors, position + new Vector2(-3f, -3f) * 4f, new Rectangle(327, 281, 3, 17), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
                    b.Draw(Game1.mouseCursors, position + new Vector2(0f, -3f) * 4f, new Rectangle(330, 281, 1, 17), Color.White * alpha, 0f, Vector2.Zero, new Vector2(scroll_width + 4, 4f), SpriteEffects.None, layerDepth - 0.001f);
                    b.Draw(Game1.mouseCursors, position + new Vector2(scroll_width + 4, -12f), new Rectangle(333, 281, 3, 17), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
                }

                position.Y += (4f - this.FontPixelZoom) * 4f;
            }
            else if (drawBGScroll == 1)
            {
                int text_width = this.GetWidthOfString((placeHolderScrollWidthText.Length > 0) ? placeHolderScrollWidthText : s);
                Vector2 speech_position = position;
                if (Game1.currentLocation != null && Game1.currentLocation.map != null && Game1.currentLocation.map.Layers[0] != null)
                {
                    int left_edge = -Game1.viewport.X + 28;
                    int right_edge = -Game1.viewport.X + Game1.currentLocation.map.Layers[0].LayerWidth * 64 - 28;
                    if (position.X < left_edge)
                    {
                        position.X = left_edge;
                    }
                    if (position.X + text_width > right_edge)
                    {
                        position.X = right_edge - text_width;
                    }
                    speech_position.X += text_width / 2;
                    if (speech_position.X < position.X)
                    {
                        position.X += speech_position.X - position.X;
                    }
                    if (speech_position.X > position.X + text_width - 24f)
                    {
                        position.X += speech_position.X - (position.X + text_width - 24f);
                    }
                    speech_position.X = Utility.Clamp(speech_position.X, position.X, position.X + text_width - 24f);
                }
                b.Draw(Game1.mouseCursors, position + new Vector2(-7f, -3f) * 4f, new Rectangle(324, 299, 7, 17), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
                b.Draw(Game1.mouseCursors, position + new Vector2(0f, -3f) * 4f, new Rectangle(331, 299, 1, 17), Color.White * alpha, 0f, Vector2.Zero, new Vector2(this.GetWidthOfString((placeHolderScrollWidthText.Length > 0) ? placeHolderScrollWidthText : s), 4f), SpriteEffects.None, layerDepth - 0.001f);
                b.Draw(Game1.mouseCursors, position + new Vector2(text_width, -12f), new Rectangle(332, 299, 7, 17), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
                b.Draw(Game1.mouseCursors, speech_position + new Vector2(0f, 52f), new Rectangle(341, 308, 6, 5), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.0001f);
                x = (int)position.X;
                if (placeHolderScrollWidthText.Length > 0)
                {
                    x += this.GetWidthOfString(placeHolderScrollWidthText) / 2 - this.GetWidthOfString(s) / 2;
                    position.X = x;
                }
                position.Y += (4f - this.FontPixelZoom) * 4f;
            }
            if (this.LanguageCode is LocalizedContentManager.LanguageCode.ko)
            {
                position.Y -= 8f;
            }
            s = s.Replace(Environment.NewLine, "");
            if (!junimoText && (this.LanguageCode is LocalizedContentManager.LanguageCode.ja or LocalizedContentManager.LanguageCode.zh or LocalizedContentManager.LanguageCode.th /*|| (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.mod && LocalizedContentManager.CurrentModLanguage.FontApplyYOffset)*/))
            {
                position.Y -= (4f - this.FontPixelZoom) * 4f;
            }
            s = s.Replace('♡', '<');
            for (int i = 0; i < Math.Min(s.Length, characterPosition); i++)
            {
                /*if ((IsLatin(this.LanguageCode) || IsSpecialCharacter(s[i])) | junimoText)
                {
                    float tempzoom = this.FontPixelZoom;
                    if (IsSpecialCharacter(s[i]) | junimoText)
                    {
                        this.FontPixelZoom = 3f;
                    }
                    if (s[i] is '^')
                    {
                        position.Y += 18f * this.FontPixelZoom;
                        position.X = x;
                        accumulatedHorizontalSpaceBetweenCharacters = 0;
                        this.FontPixelZoom = tempzoom;
                        continue;
                    }
                    accumulatedHorizontalSpaceBetweenCharacters = (int)(0f * this.FontPixelZoom);
                    bool upper = char.IsUpper(s[i]) || s[i] == 'ß';
                    Vector2 spriteFontOffset = new Vector2(0f, -1 + ((!junimoText & upper) ? -3 : 0));
                    if (s[i] == 'Ç')
                    {
                        spriteFontOffset.Y += 2f;
                    }
                    if (SpriteText.positionOfNextSpace(s, i, (int)position.X - x, accumulatedHorizontalSpaceBetweenCharacters) >= width)
                    {
                        position.Y += 18f * this.FontPixelZoom;
                        accumulatedHorizontalSpaceBetweenCharacters = 0;
                        position.X = x;
                        if (s[i] == ' ')
                        {
                            this.FontPixelZoom = tempzoom;
                            continue;
                        }
                    }
                    b.Draw((color != -1) ? SpriteText.coloredTexture : SpriteText.spriteTexture, position + spriteFontOffset * this.FontPixelZoom, SpriteText.getSourceRectForChar(s[i], junimoText), ((IsSpecialCharacter(s[i]) | junimoText) ? Color.White : SpriteText.getColorFromIndex(color)) * alpha, 0f, Vector2.Zero, this.FontPixelZoom, SpriteEffects.None, layerDepth);
                    if (i < s.Length - 1)
                    {
                        position.X += 8f * this.FontPixelZoom + accumulatedHorizontalSpaceBetweenCharacters + (float)SpriteText.getWidthOffsetForChar(s[i + 1]) * FontPixelZoom;
                    }
                    if (s[i] != '^')
                    {
                        position.X += (float)SpriteText.getWidthOffsetForChar(s[i]) * this.FontPixelZoom;
                    }
                    this.FontPixelZoom = tempzoom;
                }
                else*/
                if (s[i] is '^')
                {
                    position.Y += (this.FontFile.Common.LineHeight + 2) * this.FontPixelZoom;
                    position.X = x;
                    accumulatedHorizontalSpaceBetweenCharacters = 0;
                }
                else
                {
                    if (i > 0 && IsSpecialCharacter(s[i - 1]))
                    {
                        position.X += 24f;
                    }
                    if (this._characterMap.TryGetValue(s[i], out FontChar fontChar))
                    {
                        Rectangle sourceRect = new(fontChar.X, fontChar.Y, fontChar.Width, fontChar.Height);
                        Texture2D texture = this.Pages[fontChar.Page];
                        Vector2 position2 = position + new Vector2(fontChar.XOffset, fontChar.YOffset) * this.FontPixelZoom;
                        if (drawBGScroll != -1 && this.LanguageCode is LocalizedContentManager.LanguageCode.ko)
                        {
                            position2.Y -= 8f;
                        }
                        if (this.LanguageCode is LocalizedContentManager.LanguageCode.ru)
                        {
                            Vector2 offset = new Vector2(-1f, 1f) * this.FontPixelZoom;
                            b.Draw(texture, position2 + offset, sourceRect, /*SpriteText.getColorFromIndex(color)*/color * alpha * SpriteText.shadowAlpha, 0f, Vector2.Zero, this.FontPixelZoom, SpriteEffects.None, layerDepth);
                            b.Draw(texture, position2 + new Vector2(0f, offset.Y), sourceRect, /*SpriteText.getColorFromIndex(color)*/color * alpha * SpriteText.shadowAlpha, 0f, Vector2.Zero, this.FontPixelZoom, SpriteEffects.None, layerDepth);
                            b.Draw(texture, position2 + new Vector2(offset.X, 0f), sourceRect, /*SpriteText.getColorFromIndex(color)*/ color * alpha * SpriteText.shadowAlpha, 0f, Vector2.Zero, this.FontPixelZoom, SpriteEffects.None, layerDepth);
                        }
                        b.Draw(texture, position2, sourceRect, /*SpriteText.getColorFromIndex(color)*/color * alpha, 0f, Vector2.Zero, this.FontPixelZoom, SpriteEffects.None, layerDepth);
                        position.X += fontChar.XAdvance * this.FontPixelZoom;
                    }
                }
            }
        }

        public override Vector2 MeasureString(string text)
        {
            return new Vector2(this.GetWidthOfString(text), this.GetHeightOfString(text));
        }

        // 源自SpriteText.getWidthOfString，有改动，但代码逻辑不变。
        private int GetWidthOfString(string text)
        {
            int width = 0;
            int maxWidth = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (this._characterMap.TryGetValue(text[i], out FontChar fontChar))
                {
                    width += fontChar.XAdvance;
                }
                maxWidth = Math.Max(width, maxWidth);
                if (text[i] is '^')  // 游戏的换行符
                {
                    width = 0;
                }
            }
            return (int)(maxWidth * this.FontPixelZoom);
        }

        // 源自SpriteText.getHeightOfString，有改动，但代码逻辑不变。
        private int GetHeightOfString(string text)
        {
            if (text.Length == 0) return 0;

            text = text.Replace(Environment.NewLine, "");
            Vector2 position = Vector2.Zero;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] is '^')
                {
                    position.Y += (this.FontFile.Common.LineHeight + 2) * this.FontPixelZoom;
                    position.X = 0f;
                    continue;
                }

                if (this._characterMap.TryGetValue(text[i], out FontChar fontChar))
                {
                    position.X += fontChar.XAdvance * this.FontPixelZoom;
                }
            }
            return (int)(position.Y + (this.FontFile.Common.LineHeight + 2) * this.FontPixelZoom);
        }

        /// <summary>游戏内置的字体放大倍数。</summary>
        public static float GetDefaultFontPixelZoom(LocalizedContentManager.LanguageCode code)
        {
            switch (code)
            {
                case LocalizedContentManager.LanguageCode.ja: return 1.75f;
                case LocalizedContentManager.LanguageCode.ru: return 3f;
                case LocalizedContentManager.LanguageCode.zh: return 1.5f;
                case LocalizedContentManager.LanguageCode.th: return 1.5f;
                case LocalizedContentManager.LanguageCode.ko: return 1.5f;
                case LocalizedContentManager.LanguageCode.mod: return LocalizedContentManager.CurrentModLanguage.FontPixelZoom;
                default: return 3f;
            }
        }

        private float GetDefaultFontPixelZoom()
        {
            return GetDefaultFontPixelZoom(this.LanguageCode);
        }

        private static bool IsSpecialCharacter(char c)
        {
            return c is '<' or '>' or '=' or '@' or '$' or '`' or '+';
        }
    }
}
