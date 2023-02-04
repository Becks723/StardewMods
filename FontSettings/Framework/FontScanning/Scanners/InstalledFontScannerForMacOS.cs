using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.FontScanning.Scanners
{
    internal class InstalledFontScannerForMacOS : InstalledFontScannerBase
    {
        protected override string[] FontInstallationDirectories
        {
            get
            {
                // As documented on "Mac OS X: Font locations and their purposes"
                // https://web.archive.org/web/20191015122508/https://support.apple.com/en-us/HT201722
                return new[]
                {
                    "%HOME%/Library/Fonts/",
                    "/Library/Fonts/",
                    "/System/Library/Fonts/",
                    "/Network/Library/Fonts/"
                }.Select(path => Environment.ExpandEnvironmentVariables(path))
                .ToArray();
            }
        }

        public InstalledFontScannerForMacOS(ScanSettings settings)
            : base(settings)
        {
        }
    }
}
