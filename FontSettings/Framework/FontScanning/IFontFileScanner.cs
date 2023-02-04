using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.FontScanning
{
    internal interface IFontFileScanner
    {
        IEnumerable<string> ScanForFontFiles();
    }
}
