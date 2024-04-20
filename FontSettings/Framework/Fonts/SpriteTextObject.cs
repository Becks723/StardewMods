using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BmFont;
using FontSettings.Framework.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace FontSettings.Framework.Fonts
{
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    [SuppressMessage("Style", "IDE0004")]
    internal class SpriteTextObject
    {
        public enum ScrollTextAlignment
        {
            Left,
            Center,
            Right
        }

        public const int scrollStyle_scroll = 0;

        public const int scrollStyle_speechBubble = 1;

        public const int scrollStyle_darkMetal = 2;

        public const int scrollStyle_blueMetal = 3;

        public const int maxCharacter = 999999;

        public const int maxHeight = 999999;

        public const int characterWidth = 8;

        public const int characterHeight = 16;

        public const int horizontalSpaceBetweenCharacters = 0;

        public const int verticalSpaceBetweenCharacters = 2;

        public const char newLine = '^';

        private float fontPixelZoom = 3f;

        public float shadowAlpha = 0.15f;

        private readonly Dictionary<char, FontChar> characterMap;

        private readonly FontFile FontFile;

        private readonly List<Texture2D> fontPages;

        private readonly Texture2D spriteTexture;

        private readonly Texture2D coloredTexture;

        public const int color_index_Default = -1;

        public const int color_index_Black = 0;

        public const int color_index_Blue = 1;

        public const int color_index_Red = 2;

        public const int color_index_Purple = 3;

        public const int color_index_White = 4;

        public const int color_index_Orange = 5;

        public const int color_index_Green = 6;

        public const int color_index_Cyan = 7;

        public const int color_index_Gray = 8;

        public const int color_index_JojaBlue = 9;

        public bool forceEnglishFont = false;

        public Color color_Default
        {
            get
            {
                if (!this.CurrentLanguageLatinPatched() && this._language.Code != LocalizedContentManager.LanguageCode.ru)
                {
                    return new Color(86, 22, 12);
                }

                return Color.White;
            }
        }

        public Color color_Black { get; } = Color.Black;


        public Color color_Blue { get; } = Color.SkyBlue;


        public Color color_Red { get; } = Color.Red;

        public Color color_Purple { get; } = new Color(110, 43, 255);

        public Color color_White { get; } = Color.White;

        public Color color_Orange { get; } = Color.OrangeRed;

        public Color color_Green { get; } = Color.LimeGreen;

        public Color color_Cyan { get; } = Color.Cyan;

        public Color color_Gray { get; } = new Color(60, 60, 60);

        public Color color_JojaBlue { get; } = new Color(52, 50, 122);

        private readonly BmFontData _bmFont;
        private readonly LanguageInfo _language;
        private readonly Func<bool> _bmFontInLatinLanguages;
        public SpriteTextObject(BmFontData? bmFont, float pixelZoom, LanguageInfo language, Func<bool> bmFontInLatinLanguages)
        {
            this._bmFont = bmFont;
            this._language = language;
            this._bmFontInLatinLanguages = bmFontInLatinLanguages;

            if (bmFont != null)
            {
                this.FontFile = bmFont.FontFile;
                this.fontPages = new(bmFont.Pages);
                this.characterMap = bmFont.FontFile.Chars.ToDictionary(fontChar => (char)fontChar.ID);
            }
            this.spriteTexture = Game1.content.Load<Texture2D>("LooseSprites/font_bold");
            this.coloredTexture = Game1.content.Load<Texture2D>("LooseSprites/font_colored");
            this.fontPixelZoom = pixelZoom;
        }


        public void drawStringHorizontallyCenteredAt(SpriteBatch b, string s, int x, int y, int characterPosition = 999999, int width = -1, int height = 999999, float alpha = 1f, float layerDepth = 0.88f, bool junimoText = false, Color? color = null, int maxWidth = 99999)
        {
            this.drawString(b, s, x - this.getWidthOfString(s, maxWidth) / 2, y, characterPosition, width, height, alpha, layerDepth, junimoText, -1, "", color);
        }

        public int getWidthOfString(string s, int widthConstraint = 999999)
        {
            int num = 0;
            int num2 = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (!this.CurrentLanguageLatinPatched() && this._language.Code != LocalizedContentManager.LanguageCode.ru && !this.forceEnglishFont)
                {
                    if (this.characterMap.TryGetValue(s[i], out var value))
                    {
                        num += value.XAdvance;
                    }

                    num2 = Math.Max(num, num2);
                    if (s[i] == '^' || (float)num * this.fontPixelZoom > (float)widthConstraint)
                    {
                        num = 0;
                    }

                    continue;
                }

                num += 8 + this.getWidthOffsetForChar(s[i]);
                if (i > 0)
                {
                    num += this.getWidthOffsetForChar(s[Math.Max(0, i - 1)]);
                }

                num2 = Math.Max(num, num2);
                float num3 = this.positionOfNextSpace(s, i, (int)((float)num * this.fontPixelZoom), 0);
                if (s[i] == '^' || (float)num * this.fontPixelZoom >= (float)widthConstraint || num3 >= (float)widthConstraint)
                {
                    num = 0;
                }
            }

            return (int)((float)num2 * this.fontPixelZoom);
        }

        public bool IsMissingCharacters(string text)
        {
            if (!this.CurrentLanguageLatinPatched() && !this.forceEnglishFont)
            {
                for (int i = 0; i < text.Length; i++)
                {
                    if (!this.characterMap.ContainsKey(text[i]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public int getHeightOfString(string s, int widthConstraint = 999999)
        {
            if (s.Length == 0)
            {
                return 0;
            }

            Vector2 vector = default(Vector2);
            int num = 0;
            s = s.Replace(Environment.NewLine, "");
            if (!this.CurrentLanguageLatinPatched() && this._language.Code != LocalizedContentManager.LanguageCode.ru && !this.forceEnglishFont)
            {
                for (int i = 0; i < s.Length; i++)
                {
                    if (s[i] == '^')
                    {
                        vector.Y += (float)(this.FontFile.Common.LineHeight + 2) * this.fontPixelZoom;
                        vector.X = 0f;
                        continue;
                    }

                    if (this.positionOfNextSpace(s, i, (int)vector.X, num) >= widthConstraint)
                    {
                        vector.Y += (float)(this.FontFile.Common.LineHeight + 2) * this.fontPixelZoom;
                        num = 0;
                        vector.X = 0f;
                    }

                    if (this.characterMap.TryGetValue(s[i], out var value))
                    {
                        vector.X += (float)value.XAdvance * this.fontPixelZoom;
                    }
                }

                return (int)(vector.Y + (float)(this.FontFile.Common.LineHeight + 2) * this.fontPixelZoom);
            }

            for (int j = 0; j < s.Length; j++)
            {
                if (s[j] == '^')
                {
                    vector.Y += 18f * this.fontPixelZoom;
                    vector.X = 0f;
                    num = 0;
                    continue;
                }

                if (this.positionOfNextSpace(s, j, (int)vector.X, num) >= widthConstraint)
                {
                    vector.Y += 18f * this.fontPixelZoom;
                    num = 0;
                    vector.X = 0f;
                }

                vector.X += 8f * this.fontPixelZoom + (float)num + (float)this.getWidthOffsetForChar(s[j]) * this.fontPixelZoom;
                if (j > 0)
                {
                    vector.X += (float)this.getWidthOffsetForChar(s[j - 1]) * this.fontPixelZoom;
                }

                num = (int)(0f * this.fontPixelZoom);
            }

            return (int)(vector.Y + 16f * this.fontPixelZoom);
        }

        public Color getColorFromIndex(int index)
        {
            return index switch
            {
                1 => this.color_Blue,
                2 => this.color_Red,
                3 => this.color_Purple,
                -1 => this.color_Default,
                4 => this.color_White,
                5 => this.color_Orange,
                6 => this.color_Green,
                7 => this.color_Cyan,
                8 => this.color_Gray,
                9 => this.color_JojaBlue,
                _ => Color.Black,
            };
        }

        public string getSubstringBeyondHeight(string s, int width, int height)
        {
            Vector2 vector = default(Vector2);
            int num = 0;
            s = s.Replace(Environment.NewLine, "");
            if (!this.CurrentLanguageLatinPatched() && this._language.Code != LocalizedContentManager.LanguageCode.ru)
            {
                for (int i = 0; i < s.Length; i++)
                {
                    if (s[i] == '^')
                    {
                        vector.Y += (float)(this.FontFile.Common.LineHeight + 2) * this.fontPixelZoom;
                        vector.X = 0f;
                        num = 0;
                        continue;
                    }

                    if (this.characterMap.TryGetValue(s[i], out var value))
                    {
                        if (i > 0)
                        {
                            vector.X += (float)value.XAdvance * this.fontPixelZoom;
                        }

                        if (this.positionOfNextSpace(s, i, (int)vector.X, num) >= width)
                        {
                            vector.Y += (float)(this.FontFile.Common.LineHeight + 2) * this.fontPixelZoom;
                            num = 0;
                            vector.X = 0f;
                        }
                    }

                    if (vector.Y >= (float)height - (float)this.FontFile.Common.LineHeight * this.fontPixelZoom * 2f)
                    {
                        return s.Substring(this.getLastSpace(s, i));
                    }
                }

                return "";
            }

            for (int j = 0; j < s.Length; j++)
            {
                if (s[j] == '^')
                {
                    vector.Y += 18f * this.fontPixelZoom;
                    vector.X = 0f;
                    num = 0;
                    continue;
                }

                if (j > 0)
                {
                    vector.X += 8f * this.fontPixelZoom + (float)num + (float)(this.getWidthOffsetForChar(s[j]) + this.getWidthOffsetForChar(s[j - 1])) * this.fontPixelZoom;
                }

                num = (int)(0f * this.fontPixelZoom);
                if (this.positionOfNextSpace(s, j, (int)vector.X, num) >= width)
                {
                    vector.Y += 18f * this.fontPixelZoom;
                    num = 0;
                    vector.X = 0f;
                }

                if (vector.Y >= (float)height - 16f * this.fontPixelZoom * 2f)
                {
                    return s.Substring(this.getLastSpace(s, j));
                }
            }

            return "";
        }

        public int getIndexOfSubstringBeyondHeight(string s, int width, int height)
        {
            Vector2 vector = default(Vector2);
            int num = 0;
            s = s.Replace(Environment.NewLine, "");
            if (!this.CurrentLanguageLatinPatched())
            {
                for (int i = 0; i < s.Length; i++)
                {
                    if (s[i] == '^')
                    {
                        vector.Y += (float)(this.FontFile.Common.LineHeight + 2) * this.fontPixelZoom;
                        vector.X = 0f;
                        num = 0;
                        continue;
                    }

                    if (this.characterMap.TryGetValue(s[i], out var value))
                    {
                        if (i > 0)
                        {
                            vector.X += (float)value.XAdvance * this.fontPixelZoom;
                        }

                        if (this.positionOfNextSpace(s, i, (int)vector.X, num) >= width)
                        {
                            vector.Y += (float)(this.FontFile.Common.LineHeight + 2) * this.fontPixelZoom;
                            num = 0;
                            vector.X = 0f;
                        }
                    }

                    if (vector.Y >= (float)height - (float)this.FontFile.Common.LineHeight * this.fontPixelZoom * 2f)
                    {
                        return i - 1;
                    }
                }

                return s.Length - 1;
            }

            for (int j = 0; j < s.Length; j++)
            {
                if (s[j] == '^')
                {
                    vector.Y += 18f * this.fontPixelZoom;
                    vector.X = 0f;
                    num = 0;
                    continue;
                }

                if (j > 0)
                {
                    vector.X += 8f * this.fontPixelZoom + (float)num + (float)(this.getWidthOffsetForChar(s[j]) + this.getWidthOffsetForChar(s[j - 1])) * this.fontPixelZoom;
                }

                num = (int)(0f * this.fontPixelZoom);
                if (this.positionOfNextSpace(s, j, (int)vector.X, num) >= width)
                {
                    vector.Y += 18f * this.fontPixelZoom;
                    num = 0;
                    vector.X = 0f;
                }

                if (vector.Y >= (float)height - 16f * this.fontPixelZoom)
                {
                    return j - 1;
                }
            }

            return s.Length - 1;
        }

        public List<string> getStringBrokenIntoSectionsOfHeight(string s, int width, int height)
        {
            List<string> list = new List<string>();
            while (s.Length > 0)
            {
                string stringPreviousToThisHeightCutoff = this.getStringPreviousToThisHeightCutoff(s, width, height);
                if (stringPreviousToThisHeightCutoff.Length <= 0)
                {
                    break;
                }

                list.Add(stringPreviousToThisHeightCutoff);
                s = s.Substring(list.Last().Length);
            }

            return list;
        }

        public string getStringPreviousToThisHeightCutoff(string s, int width, int height)
        {
            return s.Substring(0, this.getIndexOfSubstringBeyondHeight(s, width, height) + 1);
        }

        private int getLastSpace(string s, int startIndex)
        {
            if (this._language.Code == LocalizedContentManager.LanguageCode.ja || this._language.Code == LocalizedContentManager.LanguageCode.zh || this._language.Code == LocalizedContentManager.LanguageCode.th)
            {
                return startIndex;
            }

            for (int num = startIndex; num >= 0; num--)
            {
                if (s[num] == ' ')
                {
                    return num;
                }
            }

            return startIndex;
        }

        public int getWidthOffsetForChar(char c)
        {
            switch (c)
            {
                case ',':
                case '.':
                    return -2;
                case '!':
                case 'j':
                case 'l':
                case '¡':
                    return -1;
                case 'i':
                case 'ì':
                case 'í':
                case 'î':
                case 'ï':
                case 'ı':
                    return -1;
                case '^':
                    return -8;
                case '$':
                    return 1;
                case 'ş':
                    return -1;
                default:
                    return 0;
            }
        }

        public void drawStringWithScrollCenteredAt(SpriteBatch b, string s, int x, int y, int width, float alpha = 1f, Color? color = null, int scrollType = 0, float layerDepth = 0.88f, bool junimoText = false)
        {
            this.drawString(b, s, x - width / 2, y, 999999, width, 999999, alpha, layerDepth, junimoText, scrollType, "", color, ScrollTextAlignment.Center);
        }

        public void drawStringWithScrollCenteredAt(SpriteBatch b, string s, int x, int y, string placeHolderWidthText = "", float alpha = 1f, Color? color = null, int scrollType = 0, float layerDepth = 0.88f, bool junimoText = false)
        {
            this.drawString(b, s, x - this.getWidthOfString((placeHolderWidthText.Length > 0) ? placeHolderWidthText : s) / 2, y, 999999, -1, 999999, alpha, layerDepth, junimoText, scrollType, placeHolderWidthText, color, ScrollTextAlignment.Center);
        }

        public void drawStringWithScrollBackground(SpriteBatch b, string s, int x, int y, string placeHolderWidthText = "", float alpha = 1f, Color? color = null, ScrollTextAlignment scroll_text_alignment = ScrollTextAlignment.Left)
        {
            this.drawString(b, s, x, y, 999999, -1, 999999, alpha, 0.88f, junimoText: false, 0, placeHolderWidthText, color, scroll_text_alignment);
        }

        public void drawString(SpriteBatch b, string s, int x, int y, int characterPosition = 999999, int width = -1, int height = 999999, float alpha = 1f, float layerDepth = 0.88f, bool junimoText = false, int drawBGScroll = -1, string placeHolderScrollWidthText = "", Color? color = null, ScrollTextAlignment scroll_text_alignment = ScrollTextAlignment.Left)
        {
            bool hasValue = color.HasValue;
            color ??= this.color_Default;
            bool flag = true;
            if (width == -1)
            {
                flag = false;
                width = Game1.graphics.GraphicsDevice.Viewport.Width - x;
                if (drawBGScroll == 1)
                {
                    width = this.getWidthOfString(s) * 2;
                }
            }

            if (this.fontPixelZoom < 4f && this._language.Code != LocalizedContentManager.LanguageCode.ko)
            {
                y += (int)((4f - this.fontPixelZoom) * 4f);
            }

            Vector2 vector = new Vector2(x, y);
            int num = 0;
            if (drawBGScroll != 1)
            {
                if (vector.X + (float)width > (float)(Game1.graphics.GraphicsDevice.Viewport.Width - 4))
                {
                    vector.X = Game1.graphics.GraphicsDevice.Viewport.Width - width - 4;
                }

                if (vector.X < 0f)
                {
                    vector.X = 0f;
                }
            }

            switch (drawBGScroll)
            {
                case 0:
                case 2:
                case 3:
                    {
                        int num4 = this.getWidthOfString((placeHolderScrollWidthText.Length > 0) ? placeHolderScrollWidthText : s);
                        if (flag)
                        {
                            num4 = width;
                        }

                        switch (drawBGScroll)
                        {
                            case 0:
                                b.Draw(Game1.mouseCursors, vector + new Vector2(-12f, -3f) * 4f, new Rectangle(325, 318, 12, 18), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
                                b.Draw(Game1.mouseCursors, vector + new Vector2(0f, -3f) * 4f, new Rectangle(337, 318, 1, 18), Color.White * alpha, 0f, Vector2.Zero, new Vector2(num4, 4f), SpriteEffects.None, layerDepth - 0.001f);
                                b.Draw(Game1.mouseCursors, vector + new Vector2(num4, -12f), new Rectangle(338, 318, 12, 18), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
                                break;
                            case 2:
                                b.Draw(Game1.mouseCursors, vector + new Vector2(-3f, -3f) * 4f, new Rectangle(327, 281, 3, 17), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
                                b.Draw(Game1.mouseCursors, vector + new Vector2(0f, -3f) * 4f, new Rectangle(330, 281, 1, 17), Color.White * alpha, 0f, Vector2.Zero, new Vector2(num4 + 4, 4f), SpriteEffects.None, layerDepth - 0.001f);
                                b.Draw(Game1.mouseCursors, vector + new Vector2(num4 + 4, -12f), new Rectangle(333, 281, 3, 17), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
                                break;
                            case 3:
                                b.Draw(Game1.mouseCursors_1_6, vector + new Vector2(-3f, -3f) * 4f, new Rectangle(86, 145, 3, 17), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
                                b.Draw(Game1.mouseCursors_1_6, vector + new Vector2(0f, -3f) * 4f, new Rectangle(89, 145, 1, 17), Color.White * alpha, 0f, Vector2.Zero, new Vector2(num4 + 4, 4f), SpriteEffects.None, layerDepth - 0.001f);
                                b.Draw(Game1.mouseCursors_1_6, vector + new Vector2(num4 + 4, -12f), new Rectangle(92, 145, 3, 17), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
                                break;
                        }

                        switch (scroll_text_alignment)
                        {
                            case ScrollTextAlignment.Center:
                                x += (num4 - this.getWidthOfString(s)) / 2;
                                vector.X = x;
                                break;
                            case ScrollTextAlignment.Right:
                                x += num4 - this.getWidthOfString(s);
                                vector.X = x;
                                break;
                        }

                        vector.Y += (4f - this.fontPixelZoom) * 4f;
                        break;
                    }
                case 1:
                    {
                        int widthOfString = this.getWidthOfString((placeHolderScrollWidthText.Length > 0) ? placeHolderScrollWidthText : s);
                        Vector2 vector2 = vector;
                        if (Game1.currentLocation?.map?.Layers[0] != null)
                        {
                            int num2 = -Game1.viewport.X + 28;
                            int num3 = -Game1.viewport.X + Game1.currentLocation.map.Layers[0].LayerWidth * 64 - 28;
                            if (vector.X < (float)num2)
                            {
                                vector.X = num2;
                            }

                            if (vector.X + (float)widthOfString > (float)num3)
                            {
                                vector.X = num3 - widthOfString;
                            }

                            vector2.X += widthOfString / 2;
                            if (vector2.X < vector.X)
                            {
                                vector.X += vector2.X - vector.X;
                            }

                            if (vector2.X > vector.X + (float)widthOfString - 24f)
                            {
                                vector.X += vector2.X - (vector.X + (float)widthOfString - 24f);
                            }

                            vector2.X = Utility.Clamp(vector2.X, vector.X, vector.X + (float)widthOfString - 24f);
                        }

                        b.Draw(Game1.mouseCursors, vector + new Vector2(-7f, -3f) * 4f, new Rectangle(324, 299, 7, 17), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
                        b.Draw(Game1.mouseCursors, vector + new Vector2(0f, -3f) * 4f, new Rectangle(331, 299, 1, 17), Color.White * alpha, 0f, Vector2.Zero, new Vector2(this.getWidthOfString((placeHolderScrollWidthText.Length > 0) ? placeHolderScrollWidthText : s), 4f), SpriteEffects.None, layerDepth - 0.001f);
                        b.Draw(Game1.mouseCursors, vector + new Vector2(widthOfString, -12f), new Rectangle(332, 299, 7, 17), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
                        b.Draw(Game1.mouseCursors, vector2 + new Vector2(0f, 52f), new Rectangle(341, 308, 6, 5), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.0001f);
                        x = (int)vector.X;
                        if (placeHolderScrollWidthText.Length > 0)
                        {
                            x += this.getWidthOfString(placeHolderScrollWidthText) / 2 - this.getWidthOfString(s) / 2;
                            vector.X = x;
                        }

                        vector.Y += (4f - this.fontPixelZoom) * 4f;
                        break;
                    }
            }

            if (this._language.Code == LocalizedContentManager.LanguageCode.ko)
            {
                vector.Y -= 8f;
            }

            s = s.Replace(Environment.NewLine, "");
            if (!junimoText && (this._language.Code == LocalizedContentManager.LanguageCode.ja || this._language.Code == LocalizedContentManager.LanguageCode.zh || this._language.Code == LocalizedContentManager.LanguageCode.th || (this._language.Code == LocalizedContentManager.LanguageCode.mod && LocalizedContentManager.CurrentModLanguage.FontApplyYOffset)))
            {
                vector.Y -= (4f - this.fontPixelZoom) * 4f;
            }

            s = s.Replace('♡', '<');
            for (int i = 0; i < Math.Min(s.Length, characterPosition); i++)
            {
                if (this.CurrentLanguageLatinPatched() || this._language.Code == LocalizedContentManager.LanguageCode.ru || this.IsSpecialCharacter(s[i]) || junimoText || this.forceEnglishFont)
                {
                    float num5 = this.fontPixelZoom;
                    if (this.IsSpecialCharacter(s[i]) || junimoText || this.forceEnglishFont)
                    {
                        this.fontPixelZoom = 3f;
                    }

                    if (s[i] == '^')
                    {
                        vector.Y += 18f * this.fontPixelZoom;
                        vector.X = x;
                        num = 0;
                        this.fontPixelZoom = num5;
                        continue;
                    }

                    num = (int)(0f * this.fontPixelZoom);
                    bool flag2 = char.IsUpper(s[i]) || s[i] == 'ß';
                    Vector2 vector3 = new Vector2(0f, -1 + ((!junimoText && flag2) ? (-3) : 0));
                    if (s[i] == 'Ç')
                    {
                        vector3.Y += 2f;
                    }

                    if (this.positionOfNextSpace(s, i, (int)vector.X - x, num) >= width)
                    {
                        vector.Y += 18f * this.fontPixelZoom;
                        num = 0;
                        vector.X = x;
                        if (s[i] == ' ')
                        {
                            this.fontPixelZoom = num5;
                            continue;
                        }
                    }

                    Rectangle sourceRectForChar = this.getSourceRectForChar(s[i], junimoText);
                    b.Draw(hasValue ? this.coloredTexture : this.spriteTexture, vector + vector3 * this.fontPixelZoom, sourceRectForChar, ((this.IsSpecialCharacter(s[i]) || junimoText) ? Color.White : color.Value) * alpha, 0f, Vector2.Zero, this.fontPixelZoom, SpriteEffects.None, layerDepth);
                    if (i < s.Length - 1)
                    {
                        vector.X += 8f * this.fontPixelZoom + (float)num + (float)this.getWidthOffsetForChar(s[i + 1]) * this.fontPixelZoom;
                    }

                    if (s[i] != '^')
                    {
                        vector.X += (float)this.getWidthOffsetForChar(s[i]) * this.fontPixelZoom;
                    }

                    this.fontPixelZoom = num5;
                    continue;
                }

                if (s[i] == '^')
                {
                    vector.Y += (float)(this.FontFile.Common.LineHeight + 2) * this.fontPixelZoom;
                    vector.X = x;
                    num = 0;
                    continue;
                }

                if (i > 0 && this.IsSpecialCharacter(s[i - 1]))
                {
                    vector.X += 24f;
                }

                if (this.characterMap.TryGetValue(s[i], out var value))
                {
                    Rectangle value2 = new Rectangle(value.X, value.Y, value.Width, value.Height);
                    Texture2D texture = this.fontPages[value.Page];
                    if (this.positionOfNextSpace(s, i, (int)vector.X, num) >= x + width - 4)
                    {
                        vector.Y += (float)(this.FontFile.Common.LineHeight + 2) * this.fontPixelZoom;
                        num = 0;
                        vector.X = x;
                    }

                    Vector2 vector4 = new Vector2(vector.X + (float)value.XOffset * this.fontPixelZoom, vector.Y + (float)value.YOffset * this.fontPixelZoom);
                    if (drawBGScroll != -1 && this._language.Code == LocalizedContentManager.LanguageCode.ko)
                    {
                        vector4.Y -= 8f;
                    }

                    if (this._language.Code == LocalizedContentManager.LanguageCode.ru)
                    {
                        Vector2 vector5 = new Vector2(-1f, 1f) * this.fontPixelZoom;
                        b.Draw(texture, vector4 + vector5, value2, color.Value * alpha * this.shadowAlpha, 0f, Vector2.Zero, this.fontPixelZoom, SpriteEffects.None, layerDepth);
                        b.Draw(texture, vector4 + new Vector2(0f, vector5.Y), value2, color.Value * alpha * this.shadowAlpha, 0f, Vector2.Zero, this.fontPixelZoom, SpriteEffects.None, layerDepth);
                        b.Draw(texture, vector4 + new Vector2(vector5.X, 0f), value2, color.Value * alpha * this.shadowAlpha, 0f, Vector2.Zero, this.fontPixelZoom, SpriteEffects.None, layerDepth);
                    }

                    b.Draw(texture, vector4, value2, color.Value * alpha, 0f, Vector2.Zero, this.fontPixelZoom, SpriteEffects.None, layerDepth);
                    vector.X += (float)value.XAdvance * this.fontPixelZoom;
                }
            }
        }

        private bool IsSpecialCharacter(char c)
        {
            if (!c.Equals('<') && !c.Equals('=') && !c.Equals('>') && !c.Equals('@') && !c.Equals('$') && !c.Equals('`'))
            {
                return c.Equals('+');
            }

            return true;
        }

        public int positionOfNextSpace(string s, int index, int currentXPosition, int accumulatedHorizontalSpaceBetweenCharacters)
        {
            if (this._language.Code == LocalizedContentManager.LanguageCode.ja || this._language.Code == LocalizedContentManager.LanguageCode.zh || this._language.Code == LocalizedContentManager.LanguageCode.th)
            {
                float num = currentXPosition;
                string value = Game1.asianSpacingRegex.Match(s, index).Value;
                foreach (char key in value)
                {
                    if (this.characterMap.TryGetValue(key, out var value2))
                    {
                        num += (float)value2.XAdvance * this.fontPixelZoom;
                    }
                }

                return (int)num;
            }

            for (int j = index; j < s.Length; j++)
            {
                if (!this.CurrentLanguageLatinPatched() && this._language.Code != LocalizedContentManager.LanguageCode.ru)
                {
                    if (s[j] == ' ' || s[j] == '^')
                    {
                        return currentXPosition;
                    }

                    currentXPosition = ((!this.characterMap.TryGetValue(s[j], out var value3)) ? (currentXPosition + (int)((float)this.FontFile.Common.LineHeight * this.fontPixelZoom)) : (currentXPosition + (int)((float)value3.XAdvance * this.fontPixelZoom)));
                    continue;
                }

                if (s[j] == ' ' || s[j] == '^')
                {
                    return currentXPosition;
                }

                currentXPosition += (int)(8f * this.fontPixelZoom + (float)accumulatedHorizontalSpaceBetweenCharacters + (float)(this.getWidthOffsetForChar(s[j]) + this.getWidthOffsetForChar(s[Math.Max(0, j - 1)])) * this.fontPixelZoom);
                accumulatedHorizontalSpaceBetweenCharacters = (int)(0f * this.fontPixelZoom);
            }

            return currentXPosition;
        }

        private Rectangle getSourceRectForChar(char c, bool junimoText)
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
                case 'ё':
                    num = 560;
                    break;
                case 'ґ':
                    num = 561;
                    break;
                case 'є':
                    num = 562;
                    break;
                case 'і':
                    num = 563;
                    break;
                case 'ї':
                    num = 564;
                    break;
                case 'Ё':
                    num = 512;
                    break;
                case '–':
                    num = 464;
                    break;
                case '—':
                    num = 465;
                    break;
                case '№':
                    num = 466;
                    break;
                case 'Ґ':
                    num = 513;
                    break;
                case 'Є':
                    num = 514;
                    break;
                case 'І':
                    num = 515;
                    break;
                case 'Ї':
                    num = 516;
                    break;
                case 'Ą':
                    num = 576;
                    break;
                case 'ą':
                    num = 578;
                    break;
                case 'Ć':
                    num = 579;
                    break;
                case 'ć':
                    num = 580;
                    break;
                case 'Ę':
                    num = 581;
                    break;
                case 'ę':
                    num = 582;
                    break;
                case 'Ł':
                    num = 583;
                    break;
                case 'ł':
                    num = 584;
                    break;
                case 'Ń':
                    num = 585;
                    break;
                case 'ń':
                    num = 586;
                    break;
                case 'Ź':
                    num = 587;
                    break;
                case 'ź':
                    num = 588;
                    break;
                case 'Ż':
                    num = 589;
                    break;
                case 'ż':
                    num = 590;
                    break;
                case 'Ś':
                    num = 574;
                    break;
                case 'ś':
                    num = 575;
                    break;
                default:
                    if (num >= 1008 && num < 1040)
                    {
                        num -= 528;
                    }
                    else if (num >= 1040 && num < 1072)
                    {
                        num -= 512;
                    }

                    break;
            }

            return new Rectangle(num * 8 % this.spriteTexture.Width, num * 8 / this.spriteTexture.Width * 16 + (junimoText ? 224 : 0), 8, 16);
        }

        private bool CurrentLanguageLatinPatched()
        {
            if (this._bmFontInLatinLanguages())
                return false;
            else
                return FontHelpers.IsLatinLanguage(this._language);
        }
    }
}