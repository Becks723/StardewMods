using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BmFont;
using Microsoft.Xna.Framework.Graphics;

namespace FontSettings.Framework.FontGenerators
{
    internal class SampleFontGenerator : BaseFontGenerator<SampleFontGeneratorParameter>
    {
        private readonly char _defaultChar = '*';

        private readonly FontManager _fontManager;

        public SampleFontGenerator(FontManager fontManager)
        {
            this._fontManager = fontManager;
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

        private ISpriteFont SpriteFont(SampleFontGeneratorParameter param)
        {
            SpriteFont spriteFont;

            if (!param.Enabled)
                spriteFont = this._fontManager.GetBuiltInSpriteFont(param.Language, param.FontType);

            else if (param.FontFilePath == null)
                spriteFont = SpriteFontGenerator.FromExisting(
                    existingFont: this._fontManager.GetBuiltInSpriteFont(param.Language, param.FontType),
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
                return this._fontManager.GetBuiltInBmFont(param.Language);

            else if (param.FontFilePath is null)
            {
                GameBitmapSpriteFont builtIn = this._fontManager.GetBuiltInBmFont(param.Language);

                FontFile fontFile = builtIn.FontFile.DeepClone();
                fontFile.Common.LineHeight = (int)param.LineSpacing;
                // TODO: 搞懂其他属性，如Base，Spacing，Padding与SpriteFont的关系。

                return new GameBitmapSpriteFont
                {
                    FontFile = fontFile,
                    Pages = new List<Texture2D>(builtIn.Pages),
                    CharacterMap = builtIn.CharacterMap,
                    LanguageCode = builtIn.LanguageCode,
                    FontPixelZoom = param.PixelZoom,
                };
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

                return new GameBitmapSpriteFont
                {
                    FontFile = fontFile,
                    Pages = new List<Texture2D>(pages),
                    LanguageCode = param.Language.Code,
                    FontPixelZoom = param.PixelZoom
                };
            }
        }
    }

}
