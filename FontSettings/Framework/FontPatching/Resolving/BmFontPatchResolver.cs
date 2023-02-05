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
        public override IResult<IFontPatch, Exception> Resolve(FontConfig config)
        {
            try
            {
                IBmFontPatch patch;

                if (!config.Enabled)
                {
                    patch = this.PatchFactory.ForBypassBmFont();
                }

                else if (config.FontFilePath == null)  // TODO: 等集齐所有原版字体后弃用
                {
                    patch = this.PatchFactory.ForEditBmFont(config);
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
                    patch = this.PatchFactory.ForLoadBmFont(bmFont, pixelZoom);
                }

                return this.SuccessResult(patch);
            }

            catch (Exception ex)
            {
                return this.ErrorResult(ex);
            }
        }

        public override async Task<IResult<IFontPatch, Exception>> ResolveAsync(FontConfig config)
        {
            return await Task.Run(() => this.Resolve(config));
        }
    }
}
