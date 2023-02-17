using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.FontPatching.Editors;
using FontSettings.Framework.FontPatching.Loaders;

namespace FontSettings.Framework.FontPatching.Replacers
{
    internal class FontReplacer : IFontReplacer
    {
        public object Replacement { get; }

        public FontReplacer(object replacement)
        {
            this.Replacement = replacement;
        }

        public FontReplacer(IFontLoader loader)
            : this(loader.Load())
        {
        }

        void IFontEditor.Edit(object data) => throw new NotSupportedException();
    }
}
