using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace FontSettings.Framework.FontPatching.Invalidators
{
    internal class FontPatchInvalidatorComposition : IFontPatchInvalidator
    {
        private readonly IModHelper _helper;

        public FontPatchInvalidatorComposition(IModHelper helper)
        {
            this._helper = helper;
        }

        public void InvalidateAndPropagate(FontPatchContext context)
        {
            IFontPatchInvalidator invalidator = context.FontType switch
            {
                GameFontType.SmallFont => new SmallFontInvalidator(this._helper),
                GameFontType.DialogueFont => new DialogueFontInvalidator(this._helper),
                GameFontType.SpriteText => new SpriteTextInvalidator(this._helper),
                _ => throw new NotSupportedException()
            };

            invalidator.InvalidateAndPropagate(context);
        }
    }
}
