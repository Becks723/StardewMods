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
            bool log = this._scanSettings.LogDetails;
            bool recursiveScan = this._scanSettings.RecursiveScan;
            string[] extensions = this._scanSettings.Extensions.ToArray();
            var ignoredFiles = this._scanSettings.IgnoredFiles;

            if (log)
            {
                ILog.Debug($"Scanning fonts in {this._baseDirectory}...");
                ILog.Trace($"Scan settings: {this.GetScanSettingsForLog(this._scanSettings)}");
            }

            var allFiles = Directory.EnumerateFiles(
                path: this._baseDirectory,
                searchPattern: "*.*",
                searchOption: recursiveScan ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            foreach (string file in allFiles)
            {
                // wrong extension
                if (!extensions.Any(ext => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                {
                    if (log)
                        ILog.Trace($"Skipped {file} (unknown format)");
                    continue;
                }

                // ignored file
                if (ignoredFiles.Any(ignore => file.EndsWith(ignore)))  // TODO: 大小写
                {
                    if (log)
                        ILog.Trace($"Skipped {file} (ignored)");
                    continue;
                }

                // ok
                if (log)
                    ILog.Debug($"Loaded {file}");
                yield return file;
            }
        }

        private string GetScanSettingsForLog(ScanSettings scanSettings)
        {
            return $"Recursive={scanSettings.RecursiveScan}, FileExtensions={string.Join(',', scanSettings.Extensions)}, IgnoredFiles={string.Join(',', scanSettings.IgnoredFiles)}";
        }
    }
}
