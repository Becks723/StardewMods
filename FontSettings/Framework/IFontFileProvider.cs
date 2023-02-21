using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Models;

namespace FontSettings.Framework
{
    internal interface IFontFileProvider
    {
        IEnumerable<string> FontFiles { get; }

        void RescanForFontFiles();

        IResult<FontModel[]> GetFontData(string fontFile);
    }
}
