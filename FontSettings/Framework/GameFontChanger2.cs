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
    internal class GameFontChanger2
    {
        /// <summary>每次成功替换后将配置储存在这里，以便下一次替换时与新配置比较，来优化替换方案。</summary>
        private readonly Dictionary<LanguageCode, Dictionary<GameFontType, FontConfig>> _lastFontConfigs = new();

        private readonly RuntimeFontManager _fontManager;

        public GameFontChanger2(RuntimeFontManager fontManager)
        {
            this._fontManager = fontManager;

            foreach (LanguageCode code in Enum.GetValues<LanguageCode>())
            {
                var dic = new Dictionary<GameFontType, FontConfig>();
                foreach (GameFontType fontType in Enum.GetValues<GameFontType>())
                    dic[fontType] = null;

                this._lastFontConfigs[code] = dic;
            }
        }

        // 替换当前游戏字体，如果失败，沿用当前字体。
        public async Task<bool> ReplaceOriginalOrRemainAsync(FontConfig font)
        {
            return await this.ReplaceOriginalAndCatchExceptionAsync(font,
                ex => $"替换{font.InGameType.LocalizedName()}失败，将沿用当前字体。错误：{ex.Message}\n堆栈信息：\n{ex.StackTrace}");
        }

        public async Task<bool> ReplaceOriginalAndCatchExceptionAsync(FontConfig font, Func<Exception, string> errorLog, Action errorCallback = null)
        {
            bool success = false;
            try
            {
                bool replaced = await this.ReplaceOriginalAsync(font);
                return success = replaced;
            }
            catch (Exception ex)
            {
                ILog.Error(errorLog(ex));
                errorCallback?.Invoke();
                return success = false;
            }
            finally
            {
                if (success)
                    this.UpdateLastFontConfig(font);
            }
        }

        /// <summary>纯异步，无异常捕获。</summary>
        public Task<bool> ReplaceOriginalAsync(FontConfig font)
        {
            return Task.Run(() => this.ReplaceOriginal(font));
        }

        /// <returns>是否确确实实替换了。</returns>
        private bool ReplaceOriginal(FontConfig font)
        {
            if ((int)font.Lang != (int)LocalizedContentManager.CurrentLanguageCode)
                return false;

            switch (font.InGameType)
            {
                case GameFontType.SmallFont:
                case GameFontType.DialogueFont:
                    return this.ReplaceOriginalSpriteFont(font);

                case GameFontType.SpriteText:
                    return this.ReplaceOriginalBmFont(font);

                default:
                    throw new NotSupportedException();
            }
        }

        private bool ReplaceOriginalSpriteFont(FontConfig font)
        {
            FontConfig lastFont = this.GetLastFontConfig(font.Lang, font.InGameType);
            bool enabledChanged = lastFont.Enabled != font.Enabled;
            bool existingFontPathChanged = lastFont.ExistingFontPath != font.ExistingFontPath;
            bool fontFilePathChanged = lastFont.FontFilePath != font.FontFilePath;
            bool fontIndexChanged = lastFont.FontIndex != font.FontIndex;
            bool fontSizeChanged = lastFont.FontSize != font.FontSize;
            bool spacingChanged = lastFont.Spacing != font.Spacing;
            bool lineSpacingChanged = lastFont.LineSpacing != font.LineSpacing;
            // TODO: 优化

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
                    font.CharacterRanges ?? CharRangeSource.GetBuiltInCharRange(font.Lang),
                    font.Spacing,
                    font.LineSpacing
                );
            }

            else
            {
                newFont = SpriteFontGenerator.FromTtf(
                    InstalledFonts.GetFullPath(font.FontFilePath),
                    font.FontIndex,
                    font.FontSize,
                    font.CharacterRanges ?? CharRangeSource.GetBuiltInCharRange(font.Lang),
                    font.TextureWidth,
                    font.TextureHeight,
                    spacing: font.Spacing,
                    lineSpacing: font.LineSpacing);
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

            return true;
        }

        private bool ReplaceOriginalBmFont(FontConfig font)
        {
            // 目前仅支持非拉丁字符的修改，因为拉丁字符使用的是LooseSprites\font_bold, font_colored素材，
            // 而非拉丁字符（中文、日文、韩文、俄文、泰文）使用的才是BmFont。
            if (LocalizedContentManager.CurrentLanguageLatin) return false;

            FontConfig lastFont = this.GetLastFontConfig(font.Lang, font.InGameType);
            bool enabledChanged = lastFont.Enabled != font.Enabled;
            bool existingFontPathChanged = lastFont.ExistingFontPath != font.ExistingFontPath;
            bool fontFilePathChanged = lastFont.FontFilePath != font.FontFilePath;
            bool fontIndexChanged = lastFont.FontIndex != font.FontIndex;
            bool fontSizeChanged = lastFont.FontSize != font.FontSize;
            bool spacingChanged = lastFont.Spacing != font.Spacing;
            bool lineSpacingChanged = lastFont.LineSpacing != font.LineSpacing;

            bool needGenerateNewFiles = false;

            if (!font.Enabled)
            {
                GameBitmapSpriteFont builtInBmFont = this._fontManager.GetBuiltInBmFont(font.Lang);

                SpriteTextFields.FontFile = builtInBmFont.FontFile;
                SpriteTextFields._characterMap = builtInBmFont.CharacterMap;
                SpriteTextFields.fontPages = builtInBmFont.Pages;
                SpriteText.fontPixelZoom = builtInBmFont.FontPixelZoom;
                return true;
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
                    return true;
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
                // 这里不用记录，因为图片是使用内置的。

                return true;
            }

            // 剩下的情况。
            {
                BmFontGenerator.GenerateFile(
                    font.Lang,
                    font.FontFilePath,
                    out string outputDir,
                    out string outputName,
                    font.FontIndex,
                    (int)font.FontSize,
                    font.CharacterRanges
                );
                BmFontGenerator.LoadBmFont(Path.Combine(outputDir, outputName),
                    out FontFile fontFile, out Texture2D[] pages);

                var charMap = new Dictionary<char, FontChar>();
                foreach (FontChar fontChar in fontFile.Chars)
                    charMap.Add((char)fontChar.ID, fontChar);

                // 记录。
                this._fontManager.RecordBmFont(new GameBitmapSpriteFont()
                {
                    FontFile = fontFile,
                    Pages = pages.ToList(),
                    LanguageCode = (LocalizedContentManager.LanguageCode)(int)font.Lang,
                    FontPixelZoom = 1f
                });
                font.ExistingFontPath = Path.Combine(outputDir, outputName);

                // 替换。
                SpriteTextFields.FontFile = fontFile;
                SpriteTextFields._characterMap = charMap;
                SpriteTextFields.fontPages = pages.ToList();
                SpriteText.fontPixelZoom = 1f;  // 忽略放大倍数。

                return true;
            }
        }

        private FontConfig GetLastFontConfig(LanguageCode code, GameFontType fontType)
        {
            return this._lastFontConfigs[code][fontType];
        }

        private void UpdateLastFontConfig(FontConfig config)
        {
            this._lastFontConfigs[config.Lang][config.InGameType] = config;
        }
    }
}
