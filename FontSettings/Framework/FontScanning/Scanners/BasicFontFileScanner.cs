using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.FontScanning.Scanners
{
    internal class BasicFontFileScanner : IFontFileScanner
    {
        private readonly string _baseDirectory;
        private readonly ScanSettings _scanSettings;

        public BasicFontFileScanner(string baseDirectory, ScanSettings scanSettings)
        {
            this._baseDirectory = baseDirectory;
            this._scanSettings = scanSettings;
        }

        public IEnumerable<string> ScanForFontFiles()
        {
            bool recursiveScan = this._scanSettings.RecursiveScan;
            string[] extensions = this._scanSettings.Extensions.ToArray();

            var allFiles = Directory.EnumerateFiles(
                path: this._baseDirectory,
                searchPattern: "*.*",
                searchOption: recursiveScan ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            return from file in allFiles
                   where extensions.Any(ext => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                   select file;
        }
    }
}
