using System;
using System.Collections.Generic;
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

        protected override void InvalidateCore()
        {
            this._contentHelper.InvalidateCache(
                this.LocalizeBaseAssetName("Fonts/SmallFont"));
        }
    }
}
