using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.FontScanning.Scanners
{
    internal class MultiFontFileScanner : IFontFileScanner
    {
        private readonly IEnumerable<IFontFileScanner> _scanners;

        public MultiFontFileScanner(IEnumerable<IFontFileScanner> scanners)
        {
            this._scanners = scanners.Where(scanner => scanner != null);
        }

        public IEnumerable<string> ScanForFontFiles()
        {
            return this._scanners
                .SelectMany(scanner => scanner.ScanForFontFiles());
        }
    }
}
