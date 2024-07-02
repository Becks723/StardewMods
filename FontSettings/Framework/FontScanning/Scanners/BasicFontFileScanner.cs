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
        public string BaseDirectory { get; set; }

        public ScanSettings? ScanSettings { get; set; }

        public BasicFontFileScanner(string baseDirectory, ScanSettings? scanSettings)
        {
            this.BaseDirectory = baseDirectory;
            this.ScanSettings = scanSettings;
        }

        public IEnumerable<string> ScanForFontFiles()
        {
            var settings = this.ScanSettings ?? new ScanSettings();

            bool log = settings.LogDetails;
            bool recursiveScan = settings.RecursiveScan;
            string[] extensions = settings.Extensions.ToArray();
            var ignoredFiles = settings.IgnoredFiles;

            this.DebugIfLog($"Scanning fonts in '{this.BaseDirectory}'...", log);
            this.Trace($"Scan settings: {this.GetScanSettingsForLog(settings)}");

            if (!Directory.Exists(this.BaseDirectory))
            {
                this.DebugIfLog($"Could not find path '{this.BaseDirectory}'! Skipping...", log);
                yield break;
            }

            var allFiles = Directory.EnumerateFiles(
                path: this.BaseDirectory,
                searchPattern: "*.*",
                searchOption: recursiveScan ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            foreach (string file in allFiles)
            {
                // wrong extension
                if (!extensions.Any(ext => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                {
                    this.DebugIfLog($"Skipped '{file}' (unknown format)", log);
                    continue;
                }

                // ignored file
                if (ignoredFiles.Any(ignore => file.EndsWith(ignore)))  // TODO: 大小写
                {
                    this.DebugIfLog($"Skipped '{file}' (ignored)", log);
                    continue;
                }

                // ok
                this.DebugIfLog($"Loaded '{file}'", log);
                yield return file;
            }
        }

        private void DebugIfLog(string message, bool log)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (log)
                ILog.Debug(message);
            else
                ILog.Trace(message);
        }

        private void Trace(string message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            ILog.Trace(message);
        }

        private string GetScanSettingsForLog(ScanSettings scanSettings)
        {
            return $"Recursive={scanSettings.RecursiveScan}, FileExtensions={string.Join(',', scanSettings.Extensions)}, IgnoredFiles={string.Join(',', scanSettings.IgnoredFiles)}";
        }
    }
}
