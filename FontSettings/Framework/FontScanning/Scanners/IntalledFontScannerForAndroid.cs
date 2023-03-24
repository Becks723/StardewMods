using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.FontScanning.Scanners
{
    internal class IntalledFontScannerForAndroid : InstalledFontScannerBase
    {
        protected override string[] FontInstallationDirectories
        {
            get
            {
                return new[] 
                { 
                    "/system/fonts" 
                };
            }
        }

        public IntalledFontScannerForAndroid(ScanSettings settings)
            : base(settings)
        {
        }
    }
}
