using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.FontPatching.Editors;

namespace FontSettings.Framework.FontPatching.Replacers
{
    internal interface IFontReplacer : IFontEditor
    {
        object Replacement { get; }
    }
}
