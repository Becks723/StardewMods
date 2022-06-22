using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BmFont;
using FontSettings.Framework.FontInfomation;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace FontSettings.Framework
{
    internal class GameFontChanger
    {
        private readonly RuntimeFontManager _fontManager;

        public GameFontChanger(RuntimeFontManager fontManager)
        {
            this._fontManager = fontManager;
        }

        // 替换当前游戏字体，如果失败，沿用当前字体。
        public bool ReplaceOriginalOrRamain(FontConfig font)
        {
            return this.ReplaceOriginalAndCatchException(font,
                ex => $"替换{font.InGameType.LocalizedName()}失败，将沿用当前字体。{ex.Message}\n堆栈信息：\n{ex.StackTrace}");
        }

        public bool ReplaceOriginalAndCatchException(FontConfig font, Func<Exception, string> errorLog, Action errorCallback = null)
        {
            try
            {
                this.ReplaceOriginal(font);
                return true;
            }
            catch (Exception ex)
            {
                ILog.Error(errorLog(ex));
                errorCallback?.Invoke();
                return false;
            }
        }

        public void ReplaceOriginal(FontConfig font)
        {
            if ((int)font.Lang != (int)LocalizedContentManager.CurrentLanguageCode) return;

            switch (font.InGameType)
            {
                case GameFontType.SmallFont:
                case GameFontType.DialogueFont:
                    this.ReplaceOriginalSpriteFont(font);
                    break;

                case GameFontType.SpriteText:
                    this.ReplaceOriginalBmFont(font);
                    break;
            }
        }

        // 替换当前游戏字体，如果失败，沿用当前字体。
        public async Task<bool> ReplaceOriginalOrRemainAsync(FontConfig font)
        {
            return await this.ReplaceOriginalAndCatchExceptionAsync(font,
                ex => $"替换{font.InGameType.LocalizedName()}失败，将沿用当前字体。{ex.Message}\n堆栈信息：\n{ex.StackTrace}");
        }

        public async Task<bool> ReplaceOriginalAndCatchExceptionAsync(FontConfig font, Func<Exception, string> errorLog, Action errorCallback = null)
        {
            try
            {
                await this.ReplaceOriginalAsync(font);
                return true;
            }
            catch (Exception ex)
            {
                ILog.Error(errorLog(ex));
                errorCallback?.Invoke();
                return false;
            }
        }

        public Task ReplaceOriginalAsync(FontConfig font)
        {
            return Task.Run(() => this.ReplaceOriginal(font));
        }

        private void ReplaceOriginalSpriteFont(FontConfig font)
        {
            SpriteFont? newFont;
            if (!font.Enabled)
                newFont = this._fontManager.GetBuiltInSpriteFont(font.InGameType);

            else if (font.ExistingFontPath != null)
                newFont = this._fontManager.GetLoadedFont(font);

            else if (font.FontFilePath is null)  // 保持原版字体，但可能改变大小、间距。
            {
                SpriteFont builtIn = this._fontManager.GetBuiltInSpriteFont(font.InGameType);
                newFont = SpriteFontGenerator.FromExisting(
                    builtIn,
                    font.FontSize,
                    font.CharacterRanges ?? CharRangeSource.GetBuiltInCharRange(font.GetLanguage()),
                    font.Spacing,
                    font.LineSpacing
                );
            }

            else
            {
                try
                {
                    if (!InstalledFonts.TryGetFullPath(font.FontFilePath, out string filePath))
                        throw new FileNotFoundException();
                    int index = font.FontIndex;
                    float size = font.FontSize;
                    int? texWidth = font.TextureWidth;
                    int? texHeight = font.TextureHeight;
                    int lineSpacing = font.LineSpacing;
                    int spacing = (int)font.Spacing;
                    var charRanges = font.CharacterRanges ?? CharRangeSource.GetBuiltInCharRange(font.GetLanguage());
                    newFont = SpriteFontGenerator.FromTtf(filePath, index, size, charRanges, texWidth, texHeight, spacing: spacing, lineSpacing: lineSpacing);
                }
                catch
                {
                    throw;
                }
            }

            void Replace(ref SpriteFont oldValue, SpriteFont newValue)
            {
                if (newFont != null
                    && !object.ReferenceEquals(oldValue, newFont))
                {
                    // 释放旧值。
                    if (!this._fontManager.IsBuiltInSpriteFont(oldValue))
                        oldValue.Texture.Dispose();

                    // 记录新值。
                    this._fontManager.RecordSpriteFont(font, newValue);

                    oldValue = newValue;
                }
            }

            switch (font.InGameType)
            {
                case GameFontType.SmallFont:
                    Replace(ref Game1.smallFont, newFont);
                    break;
                case GameFontType.DialogueFont:
                    Replace(ref Game1.dialogueFont, newFont);
                    break;
            }
        }

        private void ReplaceOriginalBmFont(FontConfig font)
        {
            // 目前仅支持非拉丁字符的修改，因为拉丁字符使用的是LooseSprites\font_bold, font_colored素材，
            // 而非拉丁字符（中文、日文、韩文、俄文、泰文）使用的才是BmFont。
            if (LocalizedContentManager.CurrentLanguageLatin) return;

            if (!font.Enabled)
            {
                GameBitmapSpriteFont builtInBmFont = this._fontManager.GetBuiltInBmFont(font.GetLanguage());

                SpriteTextFields.FontFile = builtInBmFont.FontFile;
                SpriteTextFields._characterMap = builtInBmFont.CharacterMap;
                SpriteTextFields.fontPages = builtInBmFont.Pages;
                SpriteText.fontPixelZoom = builtInBmFont.FontPixelZoom;
                return;
            }

            // 如果有现成的字体文件（可能是上一次生成后保存的，也可能是玩家自己填写的），直接加载。
            if (font.ExistingFontPath != null)
            {
                // bmfont的文件路径应为：不带扩展名的绝对路径。
                if (File.Exists(font.ExistingFontPath + ".fnt"))
                {
                    BmFontGenerator.LoadBmFont(font.ExistingFontPath,
                        out FontFile fontFile, out Texture2D[] pages);

                    var charMap = new Dictionary<char, FontChar>();
                    foreach (FontChar fontChar in fontFile.Chars)
                        charMap.Add((char)fontChar.ID, fontChar);

                    SpriteTextFields.FontFile = fontFile;
                    SpriteTextFields._characterMap = charMap;
                    SpriteTextFields.fontPages = pages.ToList();
                    SpriteText.fontPixelZoom = 1f;  // 忽略放大倍数。

                    this._fontManager.RecordBmFont(new GameBitmapSpriteFont()
                    {
                        FontFile = fontFile,
                        Pages = pages.ToList(),
                        LanguageCode = (LocalizedContentManager.LanguageCode)(int)font.Lang,
                        FontPixelZoom = 1f
                    });
                    return;
                }
            }

            // 没有给源字体文件，则使用原版字体，但可能更改其他属性。
            if (font.FontFilePath is null)
            {
                var builtIn = this._fontManager.GetBuiltInBmFont();

                FontFile fontFile = builtIn.FontFile.DeepClone();
                fontFile.Common.LineHeight = font.LineSpacing;
                // TODO: 搞懂其他属性，如Base，Spacing，Padding与SpriteFont的关系。

                SpriteTextFields.FontFile = fontFile;
                SpriteTextFields._characterMap = builtIn.CharacterMap;
                SpriteTextFields.fontPages = builtIn.Pages.ToList();
                SpriteText.fontPixelZoom = builtIn.FontPixelZoom;

                // 这里不用记录，因为是使用内置的。

                return;
            }

            // 剩下的情况。
            {
                FontFile fontFile;
                Texture2D[] pages;
                // 先试直接生成进内存。
                try
                {
                    BmFontGenerator.GenerateIntoMemory(
                        font.FontFilePath,
                        out fontFile, out pages,
                        font.FontIndex,
                        (int)font.FontSize,
                        Array.Empty<CharacterRange>(),
                        new[] { CharsFileManager.Get(font.GetLanguage()) }
                    );

                    // 清空生成的fnt路径。
                    font.ExistingFontPath = null;
                }
                catch
                {
                    // 再试生成文件，再读文件。
                    BmFontGenerator.GenerateFile(
                        font.FontFilePath,
                        out string outputDir,
                        out string outputName,
                        font.FontIndex,
                        (int)font.FontSize,
                        Array.Empty<CharacterRange>(),
                        new[] { CharsFileManager.Get(font.GetLanguage()) }
                    );
                    BmFontGenerator.LoadBmFont(Path.Combine(outputDir, outputName),
                        out fontFile, out pages);

                    // 保存生成的fnt路径。
                    font.ExistingFontPath = Path.Combine(outputDir, outputName);
                }

                fontFile.Common.LineHeight = font.LineSpacing;

                // 记录。
                this._fontManager.RecordBmFont(new GameBitmapSpriteFont()
                {
                    FontFile = fontFile,
                    Pages = pages.ToList(),
                    LanguageCode = (LocalizedContentManager.LanguageCode)(int)font.Lang,
                    FontPixelZoom = 1f
                });

                // 替换。
                SpriteTextFields.FontFile = fontFile;
                SpriteTextFields._characterMap = fontFile.Chars.ToDictionary(c => (char)c.ID, c => c);
                SpriteTextFields.fontPages = pages.ToList();
                SpriteText.fontPixelZoom = 1f;  // 忽略放大倍数。
            }
        }
    }
}
