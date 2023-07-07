using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.FontPatching.Editors
{
    internal interface IFontEditor
    {
        void Edit(object data);
        int Priority { get; }
    }
}
