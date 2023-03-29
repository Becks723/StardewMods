using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Models;
using FontSettings.Framework;

namespace FontSettings
{
    internal interface IAsyncFontInfoRetriever
    {
        Task<IResult<FontModel[]>> GetFontInfoAsync(string fontFile);
    }
}
