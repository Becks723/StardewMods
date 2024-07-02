using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.FontScanning.Scanners
{
    internal class InstalledFontScannerForWindows : InstalledFontScannerBase
    {
        protected override string[] FontInstallationDirectories
        {
            get => new[]
                {
                    @"%SYSTEMROOT%\Fonts",
                    @"%APPDATA%\Microsoft\Windows\Fonts",
                    @"%LOCALAPPDATA%\Microsoft\Windows\Fonts"
                }.Select(path => Environment.ExpandEnvironmentVariables(path))
                .ToArray();
        }

        public InstalledFontScannerForWindows(ScanSettings? settings)
            : base(settings)
        {
        }
    }
}
