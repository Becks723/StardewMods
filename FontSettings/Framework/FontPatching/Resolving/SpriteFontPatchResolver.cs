using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FontSettings.Framework.FontPatching.Resolving
{
    internal class SpriteFontPatchResolver : BaseFontPatchResolver
    {
        public SpriteFontPatchResolver(Func<PatchModeInfo> patchModeInfo) 
            : base(patchModeInfo)
        {
        }

        public override IResult<IFontPatch, Exception> Resolve(FontConfig config, FontContext context)
        {
            try
            {
                IFontPatch patch;

                var info = this.GetPatchModeInfo(context);

                if (!config.Enabled)
                {
                    patch = this.PatchFactory.ForBypassSpriteFont();
                }

                else if (config.FontFilePath == null)  // TODO: 等集齐所有原版字体后弃用
                {
                    patch = this.PatchFactory.ForEditSpriteFont(config, info.EditPriority);
                }

                else
                {
                    SpriteFont spriteFont = SpriteFontGenerator.Generate(config);

                    patch = info.LoadOrReplace
                        ? this.PatchFactory.ForLoadSpriteFont(spriteFont, info.LoadPriority)
                        : this.PatchFactory.ForReplaceSpriteFont(spriteFont, info.EditPriority);
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
