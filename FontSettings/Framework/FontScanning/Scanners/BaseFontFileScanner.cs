using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.FontScanning.Scanners
{
    internal abstract class BaseFontFileScanner : IFontFileScanner
    {
        public abstract IEnumerable<string> ScanForFontFiles();
    }
}
