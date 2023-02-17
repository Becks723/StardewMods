using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.FontPatching
{
    internal class FontPatchContext
    {
        public LanguageInfo Language { get; }

        public GameFontType FontType { get; }

        public FontPatchContext(LanguageInfo language, GameFontType fontType)
        {
            this.Language = language;
            this.FontType = fontType;
        }
    }
}
