using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.FontScanning.Scanners
{
    internal abstract class InstalledFontScannerBase : IFontFileScanner
    {
        public ScanSettings? ScanSettings { get; set; }

        protected abstract string[] FontInstallationDirectories { get; }

        protected InstalledFontScannerBase(ScanSettings? settings)
        {
            this.ScanSettings = settings;
        }

        public virtual IEnumerable<string> ScanForFontFiles()
        {
            return this.SafeGetInstallationDirectories()
                .Select(dir => new BasicFontFileScanner(dir, this.ScanSettings))
                .SelectMany(scanner => scanner.ScanForFontFiles());
        }

        private string[] SafeGetInstallationDirectories()
        {
            return this.FontInstallationDirectories
                ?.Where(dir => Directory.Exists(dir))
                .Distinct()
                .ToArray()
                ?? Array.Empty<string>();
        }
    }
}
