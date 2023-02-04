using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.FontScanning.Scanners
{
    internal class ScanSettings
    {
        public bool RecursiveScan { get; set; } = true;

        public ISet<string> Extensions { get; } = new HashSet<string>(new[] { ".ttf", ".ttc", ".otf" });
    }
}
