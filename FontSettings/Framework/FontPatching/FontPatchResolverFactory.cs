using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.FontPatching.Resolving;

namespace FontSettings.Framework.FontPatching
{
    internal class FontPatchResolverFactory
    {
        public IFontPatchResolver CreateResolver(GameFontType fontType)
        {
            return fontType switch
            {
                GameFontType.SmallFont => new SpriteFontPatchResolver(),
                GameFontType.DialogueFont => new SpriteFontPatchResolver(),
                GameFontType.SpriteText => new BmFontPatchResolver(),
                _ => throw new NotSupportedException(),
            };
        }
    }
}
