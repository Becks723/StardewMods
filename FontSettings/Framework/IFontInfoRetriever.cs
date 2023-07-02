using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Models;

namespace FontSettings.Framework
{
    internal interface IFontInfoRetriever
    {
        IResult<FontModel[]> GetFontInfo(string fontFile);
    }
}
