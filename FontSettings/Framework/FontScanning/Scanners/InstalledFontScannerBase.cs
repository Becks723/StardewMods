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
        private readonly ScanSettings _settings;

        protected abstract string[] FontInstallationDirectories { get; }

        protected InstalledFontScannerBase(ScanSettings settings)
        {
            this._settings = settings;
        }

        public virtual IEnumerable<string> ScanForFontFiles()
        {
            return this.SafeGetInstallationDirectories()
                .Select(dir => new BasicFontFileScanner(dir, this._settings))
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
