using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Models;

namespace FontSettings.Framework.FontPatching
{
    internal interface IFontPatchResolver
    {
        IResult<IFontPatch, Exception> Resolve(FontConfig_ config);

        Task<IResult<IFontPatch, Exception>> ResolveAsync(FontConfig_ config);
    }
}
