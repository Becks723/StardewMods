using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework
{
    internal partial record FontContext : IExtensible
    {
        public static FontContext For(LanguageInfo language, GameFontType fontType)
            => new FontContext(language, fontType);

        public virtual bool Supports<T>() where T : class
        {
            return this is T;
        }

        public T? GetInstance<T>() where T : class
        {
            return this as T;
        }
    }
}
