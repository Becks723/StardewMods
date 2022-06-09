using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.FontInfomation
{
    internal interface IFontInfoSource
    {
        FontModel[] GetFontInfo(string fontFile);
    }
}
