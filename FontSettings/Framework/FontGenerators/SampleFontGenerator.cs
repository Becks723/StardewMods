using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BmFont;
using FontSettings.Framework.Fonts;
using Microsoft.Xna.Framework.Graphics;

namespace FontSettings.Framework.FontGenerators
{
    internal class SampleFontGenerator : BaseFontGenerator<SampleFontGeneratorParameter>
    {
        private readonly char _defaultChar = '*';

        private readonly IVanillaFontProvider _vanillaFontProvider;
        private readonly Func<bool> _enableLatinDialogueFont;

        public SampleFontGenerator(IVanillaFontProvider vanillaFontProvider, Func<bool> enableLatinDialogueFont)
        {
            this._vanillaFontProvider = vanillaFontProvider;
            this._enableLatinDialogueFont = enableLatinDialogueFont;
        }

        protected override ISpriteFont GenerateFontCore(SampleFontGeneratorParameter param)
        {
            if (param.FontType != GameFontType.SpriteText)
                return this.SpriteFont(param);
            else
                return this.BmFont(param);
        }

        protected override async Task<ISpriteFont> GenerateFontAsyncCore(SampleFontGeneratorParameter param)
        {
            return await Task.Run(() => this.GenerateFontCore(param));
        }

        protected override async Task<ISpriteFont> GenerateFontAsyncCore(SampleFontGeneratorParameter param, CancellationToken token)
        {
            // 开始异步，让它先跑着。
            var task = Task.Run(() => this.GenerateFontCore(param));

            // 在跑异步的期间，每隔100毫秒检查是否传入了取消请求，如果有，调ThrowIfCancellationRequested。
            while (!task.IsCompleted)
            {
                token.ThrowIfCancellationRequested();
                await Task.Delay(100, token);
            }

            // 到这里异步已经跑完了，也过了可取消的界限，返回异步的值。
            return await task;
        }

        private ISpriteFont SpriteFont(SampleFontGeneratorParameter param)
        {
            SpriteFont spriteFont;

            if (!param.Enabled)
                spriteFont = this.GetVanillaSpriteFont(param.Language, param.FontType);

            else if (param.FontFilePath == null)
                spriteFont = SpriteFontGenerator.FromExisting(
                    existingFont: this.GetVanillaSpriteFont(param.Language, param.FontType),
                    overrideSpacing: param.Spacing,
                    overrideLineSpacing: (int)param.LineSpacing,
                    extraCharOffsetX: param.CharOffsetX,
                    extraCharOffsetY: param.CharOffsetY);

            else
                spriteFont = SpriteFontGenerator.FromTtf(
                    ttfPath: param.FontFilePath,
                    fontIndex: param.FontIndex,
                    fontPixelHeight: param.FontSize,
                    characterRanges: FontHelpers.GetCharRange(param.SampleText, this._defaultChar),
                    spacing: param.Spacing,
                    lineSpacing: (int)param.LineSpacing,
                    defaultCharacter: this._defaultChar,
                    charOffsetX: param.CharOffsetX,
                    charOffsetY: param.CharOffsetY);

            return new XNASpriteFont(spriteFont);
        }

        private ISpriteFont BmFont(SampleFontGeneratorParameter param)
        {
            if (FontHelpers.IsLatinLanguage(param.Language) || !param.Enabled)
                return this.GetVanillaBmFont(param.Language, GameFontType.SpriteText);

            else if (param.FontFilePath is null)
            {
                GameBitmapSpriteFont builtIn = this.GetVanillaBmFont(param.Language, GameFontType.SpriteText);

                FontFile fontFile = builtIn.FontFile.DeepClone();
                fontFile.Common.LineHeight = (int)param.LineSpacing;
                // TODO: 搞懂其他属性，如Base，Spacing，Padding与SpriteFont的关系。

                return new GameBitmapSpriteFont(
                    bmFont: new Models.BmFontData
                    {
                        FontFile = fontFile,
                        Pages = builtIn.Pages.ToArray()
                    },
                    pixelZoom: param.PixelZoom,
                    language: param.Language,
                    bmFontInLatinLanguages: this._enableLatinDialogueFont());
            }
            else
            {
                BmFontGenerator.GenerateIntoMemory(
                    fontFilePath: param.FontFilePath,
                    fontFile: out FontFile fontFile,
                    pages: out Texture2D[] pages,
                    fontIndex: param.FontIndex,
                    fontSize: (int)param.FontSize,
                    charRanges: FontHelpers.GetCharRange(param.SampleText, this._defaultChar),
                    spacingHoriz: (int)param.Spacing,
                    charOffsetX: param.CharOffsetX,
                    charOffsetY: param.CharOffsetY
                );
                fontFile.Common.LineHeight = (int)param.LineSpacing;

                return new GameBitmapSpriteFont(
                    bmFont: new Models.BmFontData
                    {
                        FontFile = fontFile,
                        Pages = pages,
                    },
                    pixelZoom: param.PixelZoom,
                    language: param.Language,
                    bmFontInLatinLanguages: this._enableLatinDialogueFont());
            }
        }

        private SpriteFont? GetVanillaSpriteFont(LanguageInfo language, GameFontType fontType)
        {
            var font = this._vanillaFontProvider.GetVanillaFont(language, fontType);
            return font is XNASpriteFont xnaFont
                ? xnaFont.InnerFont
                : null;
        }

        private GameBitmapSpriteFont? GetVanillaBmFont(LanguageInfo language, GameFontType fontType)
        {
            var font = this._vanillaFontProvider.GetVanillaFont(language, fontType);
            return font as GameBitmapSpriteFont;
        }
    }
}
