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

        public int Priority { get; }

        public FontReplacer(object replacement, int priority)
        {
            this.Replacement = replacement;
            this.Priority = priority;
        }

        void IFontEditor.Edit(object data) => throw new NotSupportedException();
    }
}
