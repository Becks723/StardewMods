using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace FontSettings.Framework.FontPatching.Invalidators
{
    internal class DialogueFontInvalidator : BaseFontPatchInvalidator
    {
        private readonly IGameContentHelper _contentHelper;

        public DialogueFontInvalidator(IModHelper modHelper)
        {
            this._contentHelper = modHelper.GameContent;
        }

        protected override void InvalidateCore()
        {
            this._contentHelper.InvalidateCache(
                this.LocalizeBaseAssetName("Fonts/SpriteFont1"));
        }
    }
}
