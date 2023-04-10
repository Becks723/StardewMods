using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework
{
    internal partial record FontContext
    {
        public static FontContext For(LanguageInfo language, GameFontType fontType)
            => new FontContext(language, fontType);
    }
}
