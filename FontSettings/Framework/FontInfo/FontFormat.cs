using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.FontInfo
{
    internal enum FontFormat
    {
        Unknown,
        OpenType,           // .ttf .otf
        OpenTypeCollection  // .ttc .otc
    }
}
