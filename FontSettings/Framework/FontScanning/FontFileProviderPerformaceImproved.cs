using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FontSettings.Framework.FontScanning
{
    /// <summary>Limition: Don't modify `Scanners` collection.</summary>
    internal class FontFileProviderPerformaceImproved : FontFileProvider
    {
        private readonly object _lock = new();

        public FontFileProviderPerformaceImproved(IEnumerable<IFontFileScanner> scanners)
        {
            foreach (IFontFileScanner scanner in scanners)
                this.Scanners.Add(scanner);

            _ = this.RescanForFontFilesAsync();
        }

        public override sealed void RescanForFontFiles()
        {
            lock (this._lock)
                base.RescanForFontFiles();
        }

        private async Task RescanForFontFilesAsync()
        {
            await Task.Run(() => this.RescanForFontFiles());
        }
    }
}
