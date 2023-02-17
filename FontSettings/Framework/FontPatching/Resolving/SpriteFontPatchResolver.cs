using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Models;
using Microsoft.Xna.Framework.Graphics;

namespace FontSettings.Framework.FontPatching.Resolving
{
    internal class SpriteFontPatchResolver : BaseFontPatchResolver
    {
        public override IResult<IFontPatch, Exception> Resolve(FontConfig config, FontPatchContext context)
        {
            try
            {
                IFontPatch patch;

                if (!config.Enabled)
                {
                    patch = this.PatchFactory.ForBypassSpriteFont();
                }

                else if (config.FontFilePath == null)  // TODO: 等集齐所有原版字体后弃用
                {
                    patch = this.PatchFactory.ForEditSpriteFont(config);
                }

                else
                {
                    SpriteFont spriteFont = SpriteFontGenerator.FromTtf(  // TODO: processing
                        ttfPath: config.FontFilePath,
                        fontIndex: config.FontIndex,
                        fontPixelHeight: config.FontSize,
                        characterRanges: config.CharacterRanges,
                        spacing: config.Spacing,
                        lineSpacing: (int)config.LineSpacing,
                        charOffsetX: config.CharOffsetX,
                        charOffsetY: config.CharOffsetY);
                    patch = context.Language.Code != StardewValley.LocalizedContentManager.LanguageCode.mod
                        ? this.PatchFactory.ForLoadSpriteFont(spriteFont)
                        : this.PatchFactory.ForReplaceSpriteFont(spriteFont);
                }

                return this.SuccessResult(patch);
            }

            catch (Exception ex)
            {
                return this.ErrorResult(ex);
            }
        }
    }
}
