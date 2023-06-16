using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.FontScanning.Scanners
{
    internal class ExactFontFileScanner : BaseFontFileScanner
    {
        private readonly IEnumerable<string> _fontFiles;
        private readonly Action? _logAction;

        public ExactFontFileScanner(IEnumerable<string> fontFiles, Action? logAction = null)
        {
            this._fontFiles = fontFiles ?? Array.Empty<string>();
            this._logAction = logAction;
        }

        public override IEnumerable<string> ScanForFontFiles()
        {
            this._logAction?.Invoke();
            return this._fontFiles;
        }
    }
}
