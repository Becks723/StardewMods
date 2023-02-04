using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.FontPatching.Editors;
using FontSettings.Framework.FontPatching.Loaders;

namespace FontSettings.Framework.FontPatching
{
    internal interface IFontPatch
    {
        IFontLoader? Loader { get; }

        IFontEditor? Editor { get; }
    }
}
