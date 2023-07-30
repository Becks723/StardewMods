using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BmFont;
using FontSettings.Framework.Fonts;
using FontSettings.Framework.Models;
using Microsoft.Xna.Framework.Graphics;

namespace FontSettings.Framework.FontGenerators
{
    internal class SampleFontGenerator2 : ISampleFontGenerator
    {
        private readonly object _vanillaProviderLock = new object();

        private readonly IVanillaFontProvider _vanillaFontProvider;
        private readonly Func<bool> _enableLatinDialogueFont;

        public SampleFontGenerator2(IVanillaFontProvider vanillaFontProvider, Func<bool> enableLatinDialogueFont)
        {
            this._vanillaFontProvider = vanillaFontProvider;
            this._enableLatinDialogueFont = enableLatinDialogueFont;
        }

        public ISpriteFont Generate(FontConfig config, FontContext context)
        {
            if (context.FontType != GameFontType.SpriteText)
            {
                SpriteFont spriteFont;

                if (!config.Enabled)
                    spriteFont = this.GetVanillaSpriteFont(context);

                else if (config.FontFilePath == null)
                    spriteFont = SpriteFontGenerator.FromExisting(
                        existingFont: this.GetVanillaSpriteFont(context),
                        overrideSpacing: config.Spacing,
                        overrideLineSpacing: (int)config.LineSpacing,
                        extraCharOffsetX: config.CharOffsetX,
                        extraCharOffsetY: config.CharOffsetY);

                else
                    spriteFont =  SpriteFontGenerator.Generate(config);

                return new XNASpriteFont(spriteFont);
            }
            else
            {
                BmFontData bmFont;

                if ((!this._enableLatinDialogueFont() && context.Language.IsLatinLanguage()) || !config.Enabled)
                    return this.GetVanillaBmFont(context);

                else if (config.FontFilePath is null)
                {
                    GameBitmapSpriteFont builtIn = this.GetVanillaBmFont(context);

                    FontFile fontFile = builtIn.FontFile.DeepClone();
                    fontFile.Common.LineHeight = (int)config.LineSpacing;
                    // TODO: 搞懂其他属性，如Base，Spacing，Padding与SpriteFont的关系。

                    bmFont = new BmFontData(fontFile, builtIn.Pages.ToArray());
                }
                else
                {
                    bmFont = BmFontGenerator.Generate(config);
                    bmFont.FontFile.Common.LineHeight = (int)config.LineSpacing;
                }

                return new GameBitmapSpriteFont(
                    bmFont: bmFont,
                    pixelZoom: config.TryGetInstance(out IWithPixelZoom withPixelZoom)
                        ? withPixelZoom.PixelZoom
                        : 1f,
                    language: context.Language,
                    bmFontInLatinLanguages: this._enableLatinDialogueFont());
            }
        }

        public Task<ISpriteFont> GenerateAsync(FontConfig config, FontContext context, CancellationToken cancellationToken)
        {
            if (context.FontType != GameFontType.SpriteText)
                return this.SpriteFontAsync(config, context, cancellationToken);
            else
                return this.BmFontAsync(config, context, cancellationToken);
        }

        private async Task<ISpriteFont> SpriteFontAsync(FontConfig config, FontContext context, CancellationToken cancellationToken)
        {
            SpriteFont spriteFont;

            if (!config.Enabled)
                spriteFont = this.GetVanillaSpriteFont(context);

            else if (config.FontFilePath == null)
                spriteFont = SpriteFontGenerator.FromExisting(
                    existingFont: this.GetVanillaSpriteFont(context),
                    overrideSpacing: config.Spacing,
                    overrideLineSpacing: (int)config.LineSpacing,
                    extraCharOffsetX: config.CharOffsetX,
                    extraCharOffsetY: config.CharOffsetY);

            else
                spriteFont = await SpriteFontGenerator.GenerateAsync(config, cancellationToken);

            return new XNASpriteFont(spriteFont);
        }

        private async Task<ISpriteFont> BmFontAsync(FontConfig config, FontContext context, CancellationToken cancellationToken)
        {
            BmFontData bmFont;

            if ((!this._enableLatinDialogueFont() && context.Language.IsLatinLanguage()) || !config.Enabled)
                return this.GetVanillaBmFont(context);

            else if (config.FontFilePath is null)
            {
                GameBitmapSpriteFont builtIn = this.GetVanillaBmFont(context);

                FontFile fontFile = builtIn.FontFile.DeepClone();
                fontFile.Common.LineHeight = (int)config.LineSpacing;
                // TODO: 搞懂其他属性，如Base，Spacing，Padding与SpriteFont的关系。

                bmFont = new BmFontData(fontFile, builtIn.Pages.ToArray());
            }
            else
            {
                bmFont = await BmFontGenerator.GenerateAsync(config, cancellationToken);
                bmFont.FontFile.Common.LineHeight = (int)config.LineSpacing;
            }

            return new GameBitmapSpriteFont(
                bmFont: bmFont,
                pixelZoom: config.TryGetInstance(out IWithPixelZoom withPixelZoom)
                    ? withPixelZoom.PixelZoom
                    : 1f,
                language: context.Language,
                bmFontInLatinLanguages: this._enableLatinDialogueFont());
        }

        private SpriteFont? GetVanillaSpriteFont(FontContext context)
        {
            lock (this._vanillaProviderLock)
            {
                var font = this._vanillaFontProvider.GetVanillaFont(context.Language, context.FontType);
                return font is XNASpriteFont xnaFont
                    ? xnaFont.InnerFont
                    : null;
            }
        }

        private GameBitmapSpriteFont? GetVanillaBmFont(FontContext context)
        {
            lock (this._vanillaProviderLock)
            {
                var font = this._vanillaFontProvider.GetVanillaFont(context.Language, context.FontType);
                return font as GameBitmapSpriteFont;
            }
        }
    }
}
