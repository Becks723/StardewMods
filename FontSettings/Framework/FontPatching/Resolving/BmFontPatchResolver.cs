using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BmFont;
using FontSettings.Framework.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FontSettings.Framework.FontPatching.Resolving
{
    internal class BmFontPatchResolver : BaseFontPatchResolver
    {
        public BmFontPatchResolver(Func<PatchModeInfo> patchModeInfo) 
            : base(patchModeInfo)
        {
        }

        // must return IBmFontPatch.
        public override IResult<IFontPatch, Exception> Resolve(FontConfig config, FontContext context)
        {
            try
            {
                IBmFontPatch patch;

                var info = this.GetPatchModeInfo(context);

                if (!config.Enabled)
                {
                    patch = this.PatchFactory.ForBypassBmFont(
                        FontHelpers.GetDefaultFontPixelZoom(context.Language));
                }

                else if (config.FontFilePath == null)  // TODO: 等集齐所有原版字体后弃用
                {
                    patch = this.PatchFactory.ForEditBmFont(config, info.EditPriority);
                }

                else
                {
                    var bmFont = BmFontGenerator.Generate(config);
                    float pixelZoom = config.Supports<IWithPixelZoom>()
                        ? config.GetInstance<IWithPixelZoom>().PixelZoom
                        : 1f;

                    patch = info.LoadOrReplace
                        ? this.PatchFactory.ForLoadBmFont(bmFont, pixelZoom, info.LoadPriority)
                        : this.PatchFactory.ForReplaceBmFont(bmFont, pixelZoom, info.EditPriority);
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
