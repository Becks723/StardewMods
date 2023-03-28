using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace FontSettings.Framework.FontPatching.Invalidators
{
    internal class SmallFontInvalidator : BaseFontPatchInvalidator
    {
        private readonly IGameContentHelper _contentHelper;

        public SmallFontInvalidator(IModHelper modHelper)
        {
            this._contentHelper = modHelper.GameContent;
        }

        protected override void InvalidateCore(FontPatchContext context)
        {
            Debug.Assert(context.FontType == GameFontType.SmallFont);

            this._contentHelper.InvalidateCache(
                FontHelpers.LocalizeAssetName("Fonts/SmallFont", context.Language));
        }
    }
}
