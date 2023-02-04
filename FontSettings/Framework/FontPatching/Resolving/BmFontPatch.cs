using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.FontPatching.Editors;
using FontSettings.Framework.FontPatching.Loaders;

namespace FontSettings.Framework.FontPatching.Resolving
{
    internal class BmFontPatch : FontPatch, IBmFontPatch
    {
        public IDictionary<string, IFontLoader>? PageLoaders { get; }

        public float FontPixelZoom { get; } = 1f;

        public BmFontPatch(IFontLoader? loader, IFontEditor? editor, IDictionary<string, IFontLoader>? pageLoaders, float fontPixelZoom = 1f)
            : base(loader, editor)
        {
            this.PageLoaders = pageLoaders;
            this.FontPixelZoom = fontPixelZoom;
        }
    }
}
