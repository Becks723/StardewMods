using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework
{
    internal interface IVanillaFontProvider
    {
        ISpriteFont GetVanillaFont(LanguageInfo language, GameFontType fontType);

        IEnumerable<CharacterRange> GetVanillaCharacterRanges(LanguageInfo language, GameFontType fontType);
    }
}
