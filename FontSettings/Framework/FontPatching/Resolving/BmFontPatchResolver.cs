using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BmFont;
using FontSettings.Framework.Models;
using Microsoft.Xna.Framework.Graphics;

namespace FontSettings.Framework.FontPatching.Resolving
{
    internal class BmFontPatchResolver : BaseFontPatchResolver
    {
        // must return IBmFontPatch.
        public override IResult<IFontPatch, Exception> Resolve(FontConfig config, FontContext context)
        {
            try
            {
                IBmFontPatch patch;

                var (loadOrReplace, loadPriority, editPriority) = this.GetPatchDetailsForCompat(context);

                if (!config.Enabled)
                {
                    patch = this.PatchFactory.ForBypassBmFont(
                        FontHelpers.GetDefaultFontPixelZoom(context.Language));
                }

                else if (config.FontFilePath == null)  // TODO: 等集齐所有原版字体后弃用
                {
                    patch = this.PatchFactory.ForEditBmFont(config, editPriority);
                }

                else
                {
                    BmFontGenerator.GenerateIntoMemory(  // TODO: processing
                        fontFilePath: config.FontFilePath,
                        fontFile: out FontFile fontFile,
                        pages: out Texture2D[] pages,
                        fontIndex: config.FontIndex,
                        fontSize: (int)config.FontSize,
                        charRanges: config.CharacterRanges,
                        spacingHoriz: (int)config.Spacing,
                        charOffsetX: config.CharOffsetX,
                        charOffsetY: config.CharOffsetY);

                    var bmFont = new BmFontData()
                    {
                        FontFile = fontFile,
                        Pages = pages
                    };
                    float pixelZoom = config.Supports<IWithPixelZoom>()
                        ? config.GetInstance<IWithPixelZoom>().PixelZoom
                        : 1f;

                    patch = loadOrReplace
                        ? this.PatchFactory.ForLoadBmFont(bmFont, pixelZoom, loadPriority)
                        : this.PatchFactory.ForReplaceBmFont(bmFont, pixelZoom, editPriority);
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
