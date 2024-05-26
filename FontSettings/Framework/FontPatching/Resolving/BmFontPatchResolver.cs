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

                        // when replace bmfont file, we are expecting there's an existing one, so we can use `e.Edit(...)`.
                        // but it does not always exist.
                        // for example, font file assetname for Chinese is "Fonts/Chinese",
                        // then the game/smapi will look for the following (Chinese for intance) in turn inside game folder:
                        //  1. Fonts/Chinese.zh-CN, 
                        //  2. Fonts/Cinese-international (only if loading Fonts/Chinese.zh-CN throws a ContentLoadException, mostly not found)
                        //  3. Fonts/Chinese              (only if loading Fonts/Cinese-international throws a ContentLoadException, mostly not found)

                        // only number 3 exists in game folder.
                        // so initally we check the flag `if (e.Name.IsEquivalentTo("Fonts/Chinese"))`,
                        // then call `e.Edit(asset => asset.ReplaceWith(...))` to replace.

                        // however, this strategy may conflict with content patcher.
                        // if a content pack writes
                        //  {
                        //    "Action": "Load",
                        //    "Target": "Fonts/Chinese",
                        //    "FromFile": "some/specific/file.fnt"
                        //  }
                        // then content patcher will load the specific file at "Fonts/Chinese.zh-CN" point (cp use `if (e.NameWithoutLocale.IsEquivalentTo("Fonts/Chinese"))` to check),
                        // thus our "Font/Chinese" point would never be hit.

                        // And here comes strategy II. We can also use `e.NameWithoutLocale` to check,
                        // and when it comes to a non-existing font file, we can provide a fake one, as a placeholder.
                        // so the game will be happy to load&replace at early "Fonts/Chinese.zh-CN" point, parallel with content patcher.
                        
                        // in latin languages, we already have "asset/fonts/latin.fnt" to load,
                        // so a fake .fnt file is only required in non-latin languages.
                        : this.PatchFactory.ForReplaceBmFont(bmFont, pixelZoom,
                            withFakeLoader: !FontHelpers.IsLatinLanguage(context.Language.Code), info.EditPriority);
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
