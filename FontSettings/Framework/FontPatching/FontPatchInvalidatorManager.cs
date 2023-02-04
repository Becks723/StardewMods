using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.FontPatching.Invalidators;
using StardewModdingAPI;

namespace FontSettings.Framework.FontPatching
{
    internal class FontPatchInvalidatorManager
    {
        private readonly IFontPatchInvalidator _smallFontInvalidator;
        private readonly IFontPatchInvalidator _dialogueFontInvalidator;
        private readonly ISpriteTextPatchInvalidator _spriteTextInvalidator;

        public FontPatchInvalidatorManager(IModHelper helper)
        {
            this._smallFontInvalidator = new SmallFontInvalidator(helper);
            this._dialogueFontInvalidator = new DialogueFontInvalidator(helper);
            this._spriteTextInvalidator = new SpriteTextInvalidator(helper);
        }

        public IFontPatchInvalidator GetInvalidator(GameFontType fontType)
        {
            return fontType switch
            {
                GameFontType.SmallFont => this._smallFontInvalidator,
                GameFontType.DialogueFont => this._dialogueFontInvalidator,
                GameFontType.SpriteText => this._spriteTextInvalidator,
                _ => throw new NotSupportedException(),
            };
        }
    }
}
