using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.FontPatching
{
    internal interface IFontPatchInvalidator
    {
        void InvalidateAndPropagate(FontContext context);
    }
}
