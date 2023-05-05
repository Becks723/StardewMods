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

        public const int maxCharacter = 999999;

        public const int maxHeight = 999999;

        public const int characterWidth = 8;

        public const int characterHeight = 16;

        public const int horizontalSpaceBetweenCharacters = 0;

        public const int verticalSpaceBetweenCharacters = 2;

        public const char newLine = '^';

        private float fontPixelZoom = 3f;

        public float shadowAlpha = 0.15f;

        private readonly Dictionary<char, FontChar> _characterMap;

        private readonly FontFile FontFile;

        private readonly List<Texture2D> fontPages;

        private readonly Texture2D spriteTexture;

        private readonly Texture2D coloredTexture;

        public const int color_Black = 0;

        public const int color_Blue = 1;

        public const int color_Red = 2;

        public const int color_Purple = 3;

        public const int color_White = 4;

        public const int color_Orange = 5;

        public const int color_Green = 6;

        public const int color_Cyan = 7;

        public const int color_Gray = 8;

        public bool forceEnglishFont = false;

        private readonly BmFontData _bmFont;
        private readonly LanguageInfo _language;
        private readonly bool _bmFontInLatinLanguages;
        public SpriteTextObject(BmFontData? bmFont, float pixelZoom, LanguageInfo language, bool bmFontInLatinLanguages)
        {
            this._bmFont = bmFont;
            this._language = language;
            this._bmFontInLatinLanguages = bmFontInLatinLanguages;

            if (bmFont != null)
            {
                this.FontFile = bmFont.FontFile;
                this.fontPages = new(bmFont.Pages);
                this._characterMap = bmFont.FontFile.Chars.ToDictionary(fontChar => (char)fontChar.ID);
            }
            this.spriteTexture = Game1.content.Load<Texture2D>("LooseSprites/font_bold");
            this.coloredTexture = Game1.content.Load<Texture2D>("LooseSprites/font_colored");
            this.fontPixelZoom = pixelZoom;
        }

        public void drawStringHorizontallyCenteredAt(SpriteBatch b, string s, int x, int y, int characterPosition = 999999, int width = -1, int height = 999999, float alpha = 1f, float layerDepth = 0.88f, bool junimoText = false, int color = -1, Color? customColor = null, int maxWidth = 99999)
        {
            this.drawString(b, s, x - this.getWidthOfString(s, maxWidth) / 2, y, characterPosition, width, height, alpha, layerDepth, junimoText, -1, "", color, customColor, ScrollTextAlignment.Left);
        }

        public int getWidthOfString(string s, int widthConstraint = 999999)
        {
            int num = 0;
            int num2 = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (!this.CurrentLanguageLatinPatched() && !this.forceEnglishFont)
                {
                    FontChar fontChar;
                    if (this._characterMap.TryGetValue(s[i], out fontChar))
                    {
                        num += fontChar.XAdvance;
                    }
                    num2 = Math.Max(num, num2);
                    if (s[i] == '^' || (float)num * this.fontPixelZoom > (float)widthConstraint)
                    {
                        num = 0;
                    }
                }
                else
                {
                    num += 8 + this.getWidthOffsetForChar(s[i]);
                    if (i > 0)
                    {
                        num += this.getWidthOffsetForChar(s[Math.Max(0, i - 1)]);
                    }
                    num2 = Math.Max(num, num2);
                    float num3 = (float)this.positionOfNextSpace(s, i, (int)((float)num * this.fontPixelZoom), 0);
                    if (s[i] == '^' || (float)num * this.fontPixelZoom >= (float)widthConstraint || num3 >= (float)widthConstraint)
                    {
                        num = 0;
                    }
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
                    if (!this._characterMap.ContainsKey(text[i]))
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

            if (!this.CurrentLanguageLatinPatched() && !this.forceEnglishFont)
            {
                for (int i = 0; i < s.Length; i++)
                {
                    if (s[i] == '^')
                    {
                        vector.Y += (float)(this.FontFile.Common.LineHeight + 2) * this.fontPixelZoom;
                        vector.X = 0f;
                    }
                    else
                    {
                        if (this.positionOfNextSpace(s, i, (int)vector.X, num) >= widthConstraint)
                        {
                            vector.Y += (float)(this.FontFile.Common.LineHeight + 2) * this.fontPixelZoom;
                            num = 0;
                            vector.X = 0f;
                        }
                        FontChar fontChar;
                        if (this._characterMap.TryGetValue(s[i], out fontChar))
                        {
                            vector.X += (float)fontChar.XAdvance * this.fontPixelZoom;
                        }
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
                }
                else
                {
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
            }
            return (int)(vector.Y + 16f * this.fontPixelZoom);
        }

        public Color getColorFromIndex(int index)
        {
            switch (index)
            {
                case -1:
                    if (this.CurrentLanguageLatinPatched())
                    {
                        return Color.White;
                    }
                    return new Color(86, 22, 12);
                case 1:
                    return Color.SkyBlue;
                case 2:
                    return Color.Red;
                case 3:
                    return new Color(110, 43, 255);
                case 4:
                    return Color.White;
                case 5:
                    return Color.OrangeRed;
                case 6:
                    return Color.LimeGreen;
                case 7:
                    return Color.Cyan;
                case 8:
                    return new Color(60, 60, 60);
            }
            return Color.Black;
        }

        public string getSubstringBeyondHeight(string s, int width, int height)
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
                    }
                    else
                    {
                        FontChar fontChar;
                        if (this._characterMap.TryGetValue(s[i], out fontChar))
                        {
                            if (i > 0)
                            {
                                vector.X += (float)fontChar.XAdvance * this.fontPixelZoom;
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
                }
                else
                {
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
                    }
                    else
                    {
                        FontChar fontChar;
                        if (this._characterMap.TryGetValue(s[i], out fontChar))
                        {
                            if (i > 0)
                            {
                                vector.X += (float)fontChar.XAdvance * this.fontPixelZoom;
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
                }
                else
                {
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
                s = s.Substring(list.Last<string>().Length);
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
            for (int i = startIndex; i >= 0; i--)
            {
                if (s[i] == ' ')
                {
                    return i;
                }
            }
            return startIndex;
        }

        public int getWidthOffsetForChar(char c)
        {
            if (c > '.')
            {
                if (c <= 'l')
                {
                    if (c == '^')
                    {
                        return -8;
                    }
                    switch (c)
                    {
                        case 'i':
                            break;
                        case 'j':
                        case 'l':
                            return -1;
                        case 'k':
                            return 0;
                        default:
                            return 0;
                    }
                }
                else
                {
                    if (c == '¡')
                    {
                        return -1;
                    }
                    switch (c)
                    {
                        case 'ì':
                        case 'í':
                        case 'î':
                        case 'ï':
                            break;
                        default:
                            if (c != 'ı')
                            {
                                return 0;
                            }
                            break;
                    }
                }
                return -1;
            }
            if (c <= '$')
            {
                if (c != '!')
                {
                    if (c != '$')
                    {
                        return 0;
                    }
                    return 1;
                }
            }
            else
            {
                if (c != ',' && c != '.')
                {
                    return 0;
                }
                return -2;
            }
            return -1;
        }

        public void drawStringWithScrollCenteredAt(SpriteBatch b, string s, int x, int y, int width, float alpha = 1f, int color = -1, Color? customColor = null, int scrollType = 0, float layerDepth = 0.88f, bool junimoText = false)
        {
            this.drawString(b, s, x - width / 2, y, 999999, width, 999999, alpha, layerDepth, junimoText, scrollType, "", color, customColor, ScrollTextAlignment.Center);
        }

        public void drawStringWithScrollCenteredAt(SpriteBatch b, string s, int x, int y, string placeHolderWidthText = "", float alpha = 1f, int color = -1, Color? customColor = null, int scrollType = 0, float layerDepth = 0.88f, bool junimoText = false)
        {
            this.drawString(b, s, x - this.getWidthOfString((placeHolderWidthText.Length > 0) ? placeHolderWidthText : s, 999999) / 2, y, 999999, -1, 999999, alpha, layerDepth, junimoText, scrollType, placeHolderWidthText, color, customColor, ScrollTextAlignment.Center);
        }

        public void drawStringWithScrollBackground(SpriteBatch b, string s, int x, int y, string placeHolderWidthText = "", float alpha = 1f, int color = -1, Color? customColor = null, ScrollTextAlignment scroll_text_alignment = ScrollTextAlignment.Left)
        {
            this.drawString(b, s, x, y, 999999, -1, 999999, alpha, 0.88f, false, 0, placeHolderWidthText, color, customColor, scroll_text_alignment);
        }

        public void drawString(SpriteBatch b, string s, int x, int y, int characterPosition = 999999, int width = -1, int height = 999999, float alpha = 1f, float layerDepth = 0.88f, bool junimoText = false, int drawBGScroll = -1, string placeHolderScrollWidthText = "", int color = -1, Color? customColor = null, ScrollTextAlignment scroll_text_alignment = ScrollTextAlignment.Left)
        {
            bool flag = true;
            if (width == -1)
            {
                flag = false;
                width = Game1.graphics.GraphicsDevice.Viewport.Width - x;
                if (drawBGScroll == 1)
                {
                    width = this.getWidthOfString(s, 999999) * 2;
                }
            }
            if (this.fontPixelZoom < 4f && this._language.Code != LocalizedContentManager.LanguageCode.ko)
            {
                y += (int)((4f - this.fontPixelZoom) * 4f);
            }
            Vector2 vector = new Vector2((float)x, (float)y);
            int num = 0;
            if (drawBGScroll != 1)
            {
                if (vector.X + (float)width > (float)(Game1.graphics.GraphicsDevice.Viewport.Width - 4))
                {
                    vector.X = (float)(Game1.graphics.GraphicsDevice.Viewport.Width - width - 4);
                }
                if (vector.X < 0f)
                {
                    vector.X = 0f;
                }
            }
            if (drawBGScroll == 0 || drawBGScroll == 2)
            {
                int num2 = this.getWidthOfString((placeHolderScrollWidthText.Length > 0) ? placeHolderScrollWidthText : s, 999999);
                if (flag)
                {
                    num2 = width;
                }
                if (drawBGScroll == 0)
                {
                    b.Draw(Game1.mouseCursors, vector + new Vector2(-12f, -3f) * 4f, new Rectangle?(new Rectangle(325, 318, 12, 18)), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
                    b.Draw(Game1.mouseCursors, vector + new Vector2(0f, -3f) * 4f, new Rectangle?(new Rectangle(337, 318, 1, 18)), Color.White * alpha, 0f, Vector2.Zero, new Vector2((float)num2, 4f), SpriteEffects.None, layerDepth - 0.001f);
                    b.Draw(Game1.mouseCursors, vector + new Vector2((float)num2, -12f), new Rectangle?(new Rectangle(338, 318, 12, 18)), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
                }
                else if (drawBGScroll == 2)
                {
                    b.Draw(Game1.mouseCursors, vector + new Vector2(-3f, -3f) * 4f, new Rectangle?(new Rectangle(327, 281, 3, 17)), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
                    b.Draw(Game1.mouseCursors, vector + new Vector2(0f, -3f) * 4f, new Rectangle?(new Rectangle(330, 281, 1, 17)), Color.White * alpha, 0f, Vector2.Zero, new Vector2((float)(num2 + 4), 4f), SpriteEffects.None, layerDepth - 0.001f);
                    b.Draw(Game1.mouseCursors, vector + new Vector2((float)(num2 + 4), -12f), new Rectangle?(new Rectangle(333, 281, 3, 17)), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
                }
                if (scroll_text_alignment == ScrollTextAlignment.Center)
                {
                    x += (num2 - this.getWidthOfString(s, 999999)) / 2;
                    vector.X = (float)x;
                }
                else if (scroll_text_alignment == ScrollTextAlignment.Right)
                {
                    x += num2 - this.getWidthOfString(s, 999999);
                    vector.X = (float)x;
                }
                vector.Y += (4f - this.fontPixelZoom) * 4f;
            }
            else if (drawBGScroll == 1)
            {
                int widthOfString = this.getWidthOfString((placeHolderScrollWidthText.Length > 0) ? placeHolderScrollWidthText : s, 999999);
                Vector2 vector2 = vector;
                if (Game1.currentLocation != null && Game1.currentLocation.map != null && Game1.currentLocation.map.Layers[0] != null)
                {
                    int num3 = 0 - Game1.viewport.X + 28;
                    int num4 = 0 - Game1.viewport.X + Game1.currentLocation.map.Layers[0].LayerWidth * 64 - 28;
                    if (vector.X < (float)num3)
                    {
                        vector.X = (float)num3;
                    }
                    if (vector.X + (float)widthOfString > (float)num4)
                    {
                        vector.X = (float)(num4 - widthOfString);
                    }
                    vector2.X += (float)(widthOfString / 2);
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
                b.Draw(Game1.mouseCursors, vector + new Vector2(-7f, -3f) * 4f, new Rectangle?(new Rectangle(324, 299, 7, 17)), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
                b.Draw(Game1.mouseCursors, vector + new Vector2(0f, -3f) * 4f, new Rectangle?(new Rectangle(331, 299, 1, 17)), Color.White * alpha, 0f, Vector2.Zero, new Vector2((float)this.getWidthOfString((placeHolderScrollWidthText.Length > 0) ? placeHolderScrollWidthText : s, 999999), 4f), SpriteEffects.None, layerDepth - 0.001f);
                b.Draw(Game1.mouseCursors, vector + new Vector2((float)widthOfString, -12f), new Rectangle?(new Rectangle(332, 299, 7, 17)), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
                b.Draw(Game1.mouseCursors, vector2 + new Vector2(0f, 52f), new Rectangle?(new Rectangle(341, 308, 6, 5)), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.0001f);
                x = (int)vector.X;
                if (placeHolderScrollWidthText.Length > 0)
                {
                    x += this.getWidthOfString(placeHolderScrollWidthText, 999999) / 2 - this.getWidthOfString(s, 999999) / 2;
                    vector.X = (float)x;
                }
                vector.Y += (4f - this.fontPixelZoom) * 4f;
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
                if (((this.CurrentLanguageLatinPatched() || this.IsSpecialCharacter(s[i])) | junimoText) || this.forceEnglishFont)
                {
                    float num5 = this.fontPixelZoom;
                    if ((this.IsSpecialCharacter(s[i]) | junimoText) || this.forceEnglishFont)
                    {
                        this.fontPixelZoom = 3f;
                    }
                    if (s[i] == '^')
                    {
                        vector.Y += 18f * this.fontPixelZoom;
                        vector.X = (float)x;
                        num = 0;
                        this.fontPixelZoom = num5;
                    }
                    else
                    {
                        num = (int)(0f * this.fontPixelZoom);
                        bool flag2 = char.IsUpper(s[i]) || s[i] == 'ß';
                        Vector2 value = new Vector2(0f, (float)(-1 + ((!junimoText & flag2) ? -3 : 0)));
                        if (s[i] == 'Ç')
                        {
                            value.Y += 2f;
                        }
                        if (this.positionOfNextSpace(s, i, (int)vector.X - x, num) >= width)
                        {
                            vector.Y += 18f * this.fontPixelZoom;
                            num = 0;
                            vector.X = (float)x;
                            if (s[i] == ' ')
                            {
                                this.fontPixelZoom = num5;
                                goto IL_C96;
                            }
                        }
                        b.Draw((color != -1) ? this.coloredTexture : this.spriteTexture, vector + value * this.fontPixelZoom, new Rectangle?(this.getSourceRectForChar(s[i], junimoText)), ((this.IsSpecialCharacter(s[i]) | junimoText) ? Color.White : customColor ?? this.getColorFromIndex(color)) * alpha, 0f, Vector2.Zero, this.fontPixelZoom, SpriteEffects.None, layerDepth);
                        if (i < s.Length - 1)
                        {
                            vector.X += 8f * this.fontPixelZoom + (float)num + (float)this.getWidthOffsetForChar(s[i + 1]) * this.fontPixelZoom;
                        }
                        if (s[i] != '^')
                        {
                            vector.X += (float)this.getWidthOffsetForChar(s[i]) * this.fontPixelZoom;
                        }
                        this.fontPixelZoom = num5;
                    }
                }
                else if (s[i] == '^')
                {
                    vector.Y += (float)(this.FontFile.Common.LineHeight + 2) * this.fontPixelZoom;
                    vector.X = (float)x;
                    num = 0;
                }
                else
                {
                    if (i > 0 && this.IsSpecialCharacter(s[i - 1]))
                    {
                        vector.X += 24f;
                    }
                    FontChar fontChar;
                    if (this._characterMap.TryGetValue(s[i], out fontChar))
                    {
                        Rectangle value2 = new Rectangle(fontChar.X, fontChar.Y, fontChar.Width, fontChar.Height);
                        Texture2D texture = this.fontPages[fontChar.Page];
                        if (this.positionOfNextSpace(s, i, (int)vector.X, num) >= x + width - 4)
                        {
                            vector.Y += (float)(this.FontFile.Common.LineHeight + 2) * this.fontPixelZoom;
                            num = 0;
                            vector.X = (float)x;
                        }
                        Vector2 vector3 = new Vector2(vector.X + (float)fontChar.XOffset * this.fontPixelZoom, vector.Y + (float)fontChar.YOffset * this.fontPixelZoom);
                        if (drawBGScroll != -1 && this._language.Code == LocalizedContentManager.LanguageCode.ko)
                        {
                            vector3.Y -= 8f;
                        }
                        if (this._language.Code == LocalizedContentManager.LanguageCode.ru)
                        {
                            Vector2 vector4 = new Vector2(-1f, 1f) * this.fontPixelZoom;
                            b.Draw(texture, vector3 + vector4, new Rectangle?(value2), (customColor ?? this.getColorFromIndex(color)) * alpha * this.shadowAlpha, 0f, Vector2.Zero, this.fontPixelZoom, SpriteEffects.None, layerDepth);
                            b.Draw(texture, vector3 + new Vector2(0f, vector4.Y), new Rectangle?(value2), (customColor ?? this.getColorFromIndex(color)) * alpha * this.shadowAlpha, 0f, Vector2.Zero, this.fontPixelZoom, SpriteEffects.None, layerDepth);
                            b.Draw(texture, vector3 + new Vector2(vector4.X, 0f), new Rectangle?(value2), (customColor ?? this.getColorFromIndex(color)) * alpha * this.shadowAlpha, 0f, Vector2.Zero, this.fontPixelZoom, SpriteEffects.None, layerDepth);
                        }
                        b.Draw(texture, vector3, new Rectangle?(value2), (customColor ?? this.getColorFromIndex(color)) * alpha, 0f, Vector2.Zero, this.fontPixelZoom, SpriteEffects.None, layerDepth);
                        vector.X += (float)fontChar.XAdvance * this.fontPixelZoom;
                    }
                }
            IL_C96:;
            }
        }

        private bool IsSpecialCharacter(char c)
        {
            return c.Equals('<') || c.Equals('=') || c.Equals('>') || c.Equals('@') || c.Equals('$') || c.Equals('`') || c.Equals('+');
        }

        public int positionOfNextSpace(string s, int index, int currentXPosition, int accumulatedHorizontalSpaceBetweenCharacters)
        {
            if (this._language.Code == LocalizedContentManager.LanguageCode.zh || this._language.Code == LocalizedContentManager.LanguageCode.th)
            {
                FontChar fontChar;
                if (this._characterMap.TryGetValue(s[index], out fontChar))
                {
                    return currentXPosition + (int)((float)fontChar.XAdvance * this.fontPixelZoom);
                }
                return currentXPosition + (int)((float)this.FontFile.Common.LineHeight * this.fontPixelZoom);
            }
            else
            {
                if (this._language.Code != LocalizedContentManager.LanguageCode.ja)
                {
                    for (int i = index; i < s.Length; i++)
                    {
                        if (!this.CurrentLanguageLatinPatched())
                        {
                            if (s[i] == ' ' || s[i] == '^')
                            {
                                return currentXPosition;
                            }
                            FontChar fontChar2;
                            if (this._characterMap.TryGetValue(s[i], out fontChar2))
                            {
                                currentXPosition += (int)((float)fontChar2.XAdvance * this.fontPixelZoom);
                            }
                            else
                            {
                                currentXPosition += (int)((float)this.FontFile.Common.LineHeight * this.fontPixelZoom);
                            }
                        }
                        else
                        {
                            if (s[i] == ' ' || s[i] == '^')
                            {
                                return currentXPosition;
                            }
                            currentXPosition += (int)(8f * this.fontPixelZoom + (float)accumulatedHorizontalSpaceBetweenCharacters + (float)(this.getWidthOffsetForChar(s[i]) + this.getWidthOffsetForChar(s[Math.Max(0, i - 1)])) * this.fontPixelZoom);
                            accumulatedHorizontalSpaceBetweenCharacters = (int)(0f * this.fontPixelZoom);
                        }
                    }
                    return currentXPosition;
                }
                FontChar fontChar3;
                if (this._characterMap.TryGetValue(s[index], out fontChar3))
                {
                    return currentXPosition + (int)((float)fontChar3.XAdvance * this.fontPixelZoom);
                }
                return currentXPosition + (int)((float)this.FontFile.Common.LineHeight * this.fontPixelZoom);
            }
        }

        private Rectangle getSourceRectForChar(char c, bool junimoText)
        {
            int num = (int)(c - ' ');
            if (c <= 'œ')
            {
                if (c <= 'ğ')
                {
                    if (c != 'Ğ')
                    {
                        if (c == 'ğ')
                        {
                            num = 103;
                        }
                    }
                    else
                    {
                        num = 102;
                    }
                }
                else if (c != 'İ')
                {
                    if (c != 'ı')
                    {
                        switch (c)
                        {
                            case 'Ő':
                                num = 105;
                                break;
                            case 'ő':
                                num = 106;
                                break;
                            case 'Œ':
                                num = 96;
                                break;
                            case 'œ':
                                num = 97;
                                break;
                        }
                    }
                    else
                    {
                        num = 99;
                    }
                }
                else
                {
                    num = 98;
                }
            }
            else if (c <= 'ş')
            {
                if (c != 'Ş')
                {
                    if (c == 'ş')
                    {
                        num = 101;
                    }
                }
                else
                {
                    num = 100;
                }
            }
            else if (c != 'Ű')
            {
                if (c != 'ű')
                {
                    if (c == '’')
                    {
                        num = 104;
                    }
                }
                else
                {
                    num = 108;
                }
            }
            else
            {
                num = 107;
            }
            return new Rectangle(num * 8 % this.spriteTexture.Width, num * 8 / this.spriteTexture.Width * 16 + (junimoText ? 224 : 0), 8, 16);
        }

        private bool CurrentLanguageLatinPatched()
        {
            if (this._bmFontInLatinLanguages)
                return false;
            else
                return FontHelpers.IsLatinLanguage(this._language);
        }
    }
}