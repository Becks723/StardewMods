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

        public ISet<string> IgnoredFiles { get; } = GetIgnoredFiles();

        public bool LogDetails { get; set; } = true;

        private static ISet<string> GetIgnoredFiles()
        {
            return new HashSet<string>
            {
                "mstmc.ttf"
            };
        }
    }
}
