using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.Models
{
    internal record FontConfigKey(LanguageInfo Language, GameFontType FontType)
    {
        public static implicit operator FontContext(FontConfigKey key) => new(key.Language, key.FontType);
        public static implicit operator FontConfigKey(FontContext key) => new(key.Language, key.FontType);
    }
}
