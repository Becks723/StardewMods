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

        private readonly RuntimeFontManager _fontManager;

        private string _bmFontFilePath;

        public ExampleFonts(RuntimeFontManager fontManager)
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
        public ISpriteFont Get(GameFontType fontType, bool enabled, string fontFilePath, int fontIndex, float fontSize, int spacing, int lineSpacing, Vector2 charOffset, string chars)
        {
            ISpriteFont oldValue = this.Get(fontType);
            if (oldValue != null)
                return oldValue;

            return this.ResetThenGet(fontType, enabled, fontFilePath, fontIndex, fontSize, spacing, lineSpacing, charOffset, chars);
        }

        public ISpriteFont ResetThenGet(GameFontType fontType, bool enabled, string fontFilePath, int fontIndex, float fontSize, int spacing, int lineSpacing, Vector2 charOffset, string chars)
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
                    newValue = this.InternalCreateBmFont(enabled, fontFilePath, fontIndex, fontSize, spacing, lineSpacing, charOffset, chars);
                    //newValue = this.InternalCreateSpriteFontAlternative(fontType, enabled, fontFilePath, fontIndex, fontSize, spacing, lineSpacing, chars);
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
                    FontHelpers.DeleteBmFont(this._bmFontFilePath);
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

        private ISpriteFont InternalCreateBmFont(bool enabled, string fontFilePath, int fontIndex, float fontSize, int spacing, int lineSpacing, Vector2 charOffset, string text)
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
                    FontPixelZoom = builtIn.FontPixelZoom,
                };
            }
            else
                try
                {
                    FontFile fontFile;
                    Texture2D[] pages;
                    string fontFullPath = InstalledFonts.GetFullPath(fontFilePath);
                    try
                    {
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
                    }
                    catch
                    {
                        BmFontGenerator.GenerateFile(
                            fontFilePath,
                            out string outputDir,
                            out string outputName,
                            fontIndex,
                            (int)fontSize,
                            FontHelpers.GetCharRange(text, '*')
                        );
                        this._bmFontFilePath = System.IO.Path.Combine(outputDir, outputName);
                        BmFontGenerator.LoadBmFont(this._bmFontFilePath,
                            out fontFile, out pages);

                        // 为所有字符增加偏移量。
                        foreach (FontChar fontChar in fontFile.Chars)
                        {
                            fontChar.XOffset += (int)Math.Round(charOffset.X);
                            fontChar.YOffset += (int)Math.Round(charOffset.Y);
                        }
                    }

                    fontFile.Common.LineHeight = lineSpacing;

                    GameBitmapSpriteFont bmFont = new()
                    {
                        FontFile = fontFile,
                        Pages = new List<Texture2D>(pages),
                        LanguageCode = StardewValley.LocalizedContentManager.CurrentLanguageCode,
                        FontPixelZoom = 1f
                    };
                    return bmFont;
                }
                catch (Exception ex)
                {
                    ILog.Error($"在生成bmfont时遇到了错误：{ex.Message}\n堆栈信息：\n{ex.StackTrace}");
                    return null;
                }
        }

        // 暂时的办法，用于SpriteText的示例。生成spritefont，内置的bmfont作为备用。
        private ISpriteFont InternalCreateSpriteFontAlternative(GameFontType fontType, bool enabled, string fontFilePath, int fontIndex, float fontSize, int spacing, int lineSpacing, string text)
        {
            SpriteFont spriteFont;
            if (!enabled)
                return this._fontManager.GetBuiltInBmFont();
            else
                try
                {
                    if (fontFilePath == null)
                        spriteFont = SpriteFontGenerator.FromExisting(
                            this._fontManager.GetBuiltInSpriteFont(fontType),
                            overrideSpacing: spacing,
                            overrideLineSpacing: lineSpacing
                        );  // TODO: 支持charRange、size.
                    else
                    {
                        if (!InstalledFonts.TryGetFullPath(fontFilePath, out string filePath))
                            throw new System.IO.FileNotFoundException();
                        char? defaultChar = '*';
                        var charRanges = FontHelpers.GetCharRange(text, defaultChar);
                        spriteFont = SpriteFontGenerator.FromTtf(filePath, fontIndex, fontSize, charRanges, spacing: spacing, lineSpacing: lineSpacing, defaultCharacter: defaultChar);
                    }
                }
                catch
                {
                    return null;
                }

            FontFile fontFile = new FontFile()
            {
                Common = new FontCommon()
                {
                    LineHeight = lineSpacing
                },
                Chars = new List<FontChar>(),
                Info = new FontInfo(),
                Kernings = new List<FontKerning>(),
                Pages = new List<FontPage>()
            };
            Dictionary<char, FontChar> charMap = new(spriteFont.Glyphs.Length);
            foreach (SpriteFont.Glyph glyph in spriteFont.Glyphs)
            {
                charMap.Add(glyph.Character, new FontChar()
                {
                    ID = glyph.Character,
                    X = glyph.BoundsInTexture.X,
                    Y = glyph.BoundsInTexture.Y,
                    Width = glyph.BoundsInTexture.Width,
                    Height = glyph.BoundsInTexture.Height,
                    XOffset = glyph.Cropping.X,
                    YOffset = glyph.Cropping.Y,
                    XAdvance = (int)glyph.Width,
                    Page = 0,
                    Channel = 15
                });
            }

            return new GameBitmapSpriteFont()
            {
                FontFile = fontFile,
                Pages = new(1) { spriteFont.Texture },
                CharacterMap = charMap,
                LanguageCode = StardewValley.LocalizedContentManager.CurrentLanguageCode,
                FontPixelZoom = 1f
            };
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
