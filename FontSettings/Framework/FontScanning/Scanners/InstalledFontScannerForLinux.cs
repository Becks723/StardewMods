using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.FontScanning.Scanners
{
    internal class InstalledFontScannerForLinux : InstalledFontScannerBase
    {
        protected override string[] FontInstallationDirectories
        {
            get
            {
                return new[]
                {
                    "%HOME%/.fonts/",
                    "/usr/local/share/fonts",
                    "/usr/share/fonts"
                }.Select(path => Environment.ExpandEnvironmentVariables(path))
                .ToArray();
            }
        }

        public InstalledFontScannerForLinux(ScanSettings settings)
            : base(settings)
        {
        }
    }
}
