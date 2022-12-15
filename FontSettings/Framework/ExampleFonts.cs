using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BmFont;
using FontSettings.Framework.FontInfomation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FontSettings.Framework
{
    internal class ExampleFonts
    {
        private ISpriteFont _smallFont;

        private ISpriteFont _dialogueFont;

        private ISpriteFont _spriteTextFont;

        private readonly FontManager _fontManager;

        public ExampleFonts(FontManager fontManager)
        {
            this._fontManager = fontManager;
        }

        public ISpriteFont Get(GameFontType fontType)
        {
            return fontType switch
            {
                GameFontType.SmallFont => this._smallFont,
                GameFontType.DialogueFont => this._dialogueFont,
                GameFontType.SpriteText => this._spriteTextFont,
                _ => throw new NotSupportedException()
            };
        }

        /// <summary>如果值存在，直接返回现有值；否则重新赋值。</summary>
        public ISpriteFont Get(GameFontType fontType, bool enabled, string fontFilePath, int fontIndex, float fontSize, int spacing, int lineSpacing, Vector2 charOffset, float pixelZoom, string chars)
        {
            ISpriteFont oldValue = this.Get(fontType);
            if (oldValue != null)
                return oldValue;

            return this.ResetThenGet(fontType, enabled, fontFilePath, fontIndex, fontSize, spacing, lineSpacing, charOffset, pixelZoom, chars);
        }

        public ISpriteFont ResetThenGet(GameFontType fontType, bool enabled, string fontFilePath, int fontIndex, float fontSize, int spacing, int lineSpacing, Vector2 charOffset, float pixelZoom, string chars)
        {
            this.Dispose(fontType);

            ISpriteFont newValue;
            switch (fontType)
            {
                case GameFontType.SmallFont:
                case GameFontType.DialogueFont:
                    newValue = this.InternalCreateSpriteFont(fontType, enabled, fontFilePath, fontIndex, fontSize, spacing, lineSpacing, charOffset, chars);
                    break;
                case GameFontType.SpriteText:
                    newValue = this.InternalCreateBmFont(enabled, fontFilePath, fontIndex, fontSize, spacing, lineSpacing, charOffset, pixelZoom, chars);
                    break;
                default:
                    throw new NotSupportedException();
            }

            this.Assign(fontType, newValue);
            return newValue;
        }

        public void Dispose(GameFontType fontType)
        {
            ISpriteFont font = this.Map(fontType);
            switch (font)
            {
                case XNASpriteFont spriteFont:
                    if (!this._fontManager.IsBuiltInSpriteFont(spriteFont.InnerFont))
                        spriteFont.Dispose();
                    break;

                case BitmapSpriteFont bmFont:
                    if (!this._fontManager.IsBuiltInBmFont(bmFont))
                        bmFont.Dispose();
                    break;
            }
        }

        private ISpriteFont InternalCreateSpriteFont(GameFontType fontType, bool enabled, string fontFilePath, int fontIndex, float fontSize, int spacing, int lineSpacing, Vector2 charOffset, string text)
        {
            SpriteFont spriteFont;
            if (!enabled)
                spriteFont = this._fontManager.GetBuiltInSpriteFont(fontType);
            else
                try
                {
                    if (fontFilePath == null)
                        spriteFont = SpriteFontGenerator.FromExisting(
                            existingFont: this._fontManager.GetBuiltInSpriteFont(fontType),
                            overrideSpacing: spacing,
                            overrideLineSpacing: lineSpacing,
                            extraCharOffsetX: charOffset.X,
                            extraCharOffsetY: charOffset.Y
                        );  // TODO: 支持charRange、size.
                    else
                    {
                        char? defaultChar = '*';
                        spriteFont = SpriteFontGenerator.FromTtf(
                            InstalledFonts.GetFullPath(fontFilePath),
                            fontIndex,
                            fontSize,
                            FontHelpers.GetCharRange(text, defaultChar),
                            spacing: spacing,
                            lineSpacing: lineSpacing,
                            defaultCharacter: defaultChar,
                            charOffsetX: charOffset.X,
                            charOffsetY: charOffset.Y
                        );
                    }
                }
                catch (Exception ex)
                {
                    ILog.Error($"在生成SpriteFont时遇到了错误：{ex.Message}\n堆栈信息：\n{ex.StackTrace}");
                    return null;
                }

            return new XNASpriteFont(spriteFont);
        }

        private ISpriteFont InternalCreateBmFont(bool enabled, string fontFilePath, int fontIndex, float fontSize, int spacing, int lineSpacing, Vector2 charOffset, float pixelZoom, string text)
        {
            if (StardewValley.LocalizedContentManager.CurrentLanguageLatin || !enabled)
                return this._fontManager.GetBuiltInBmFont();
            else if (fontFilePath is null)
            {
                GameBitmapSpriteFont builtIn = this._fontManager.GetBuiltInBmFont();

                FontFile fontFile = builtIn.FontFile.DeepClone();
                fontFile.Common.LineHeight = lineSpacing;
                // TODO: 搞懂其他属性，如Base，Spacing，Padding与SpriteFont的关系。

                return new GameBitmapSpriteFont
                {
                    FontFile = fontFile,
                    Pages = new List<Texture2D>(builtIn.Pages),
                    CharacterMap = builtIn.CharacterMap,
                    LanguageCode = builtIn.LanguageCode,
                    FontPixelZoom = pixelZoom,
                };
            }
            else
                try
                {
                    FontFile fontFile;
                    Texture2D[] pages;
                    string fontFullPath = InstalledFonts.GetFullPath(fontFilePath);
                    BmFontGenerator.GenerateIntoMemory(
                        fontFilePath: fontFullPath,
                        fontFile: out fontFile,
                        pages: out pages,
                        fontIndex: fontIndex,
                        fontSize: (int)fontSize,
                        charRanges: FontHelpers.GetCharRange(text, '*'),
                        spacingHoriz: spacing,
                        charOffsetX: charOffset.X,
                        charOffsetY: charOffset.Y
                    );
                    fontFile.Common.LineHeight = lineSpacing;

                    GameBitmapSpriteFont bmFont = new()
                    {
                        FontFile = fontFile,
                        Pages = new List<Texture2D>(pages),
                        LanguageCode = StardewValley.LocalizedContentManager.CurrentLanguageCode,
                        FontPixelZoom = pixelZoom
                    };
                    return bmFont;
                }
                catch (Exception ex)
                {
                    ILog.Error($"在生成bmfont时遇到了错误：{ex.Message}\n堆栈信息：\n{ex.StackTrace}");
                    return null;
                }
        }

        private ISpriteFont Map(GameFontType fontType)
        {
            return fontType switch
            {
                GameFontType.SmallFont => this._smallFont,
                GameFontType.DialogueFont => this._dialogueFont,
                GameFontType.SpriteText => this._spriteTextFont,
                _ => throw new NotSupportedException()
            };
        }

        private void Assign(GameFontType fontType, ISpriteFont value)
        {
            switch (fontType)
            {
                case GameFontType.SmallFont: this._smallFont = value; break;
                case GameFontType.DialogueFont: this._dialogueFont = value; break;
                case GameFontType.SpriteText: this._spriteTextFont = value; break;
            }
        }
    }
}
