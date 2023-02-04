using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.FontPatching.Editors;
using FontSettings.Framework.FontPatching.Loaders;

namespace FontSettings.Framework.FontPatching.Resolving
{
    internal class FontPatch : IFontPatch
    {
        public IFontLoader? Loader { get; }

        public IFontEditor? Editor { get; }

        public FontPatch(IFontLoader? loader, IFontEditor? editor)
        {
            this.Loader = loader;
            this.Editor = editor;
        }
    }
}
