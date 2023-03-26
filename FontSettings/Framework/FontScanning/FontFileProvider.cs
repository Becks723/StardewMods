using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.FontInfo;
using FontSettings.Framework.Models;

namespace FontSettings.Framework.FontScanning
{
    internal class FontFileProvider : IFontFileProvider
    {
        private IEnumerable<string> _fontFiles;

        public IEnumerable<string> FontFiles
        {
            get
            {
                if (this._fontFiles == null)
                    this.RescanForFontFiles();

                return this._fontFiles;
            }
        }

        public virtual ICollection<IFontFileScanner> Scanners { get; } = new List<IFontFileScanner>();

        public virtual void RescanForFontFiles()
        {
            var stopWatch = new Stopwatch();
            try
            {
                stopWatch.Start();
                this.RescanForFontFilesCore();
            }
            finally
            {
                stopWatch.Stop();
                ILog.Trace($"Scan fonts completed in '{stopWatch.ElapsedMilliseconds}ms'");
            }
        }

        protected virtual void RescanForFontFilesCore()
        {
            this._fontFiles = this.Scanners
                .Where(scanner => scanner != null)
                .SelectMany(scanner => scanner.ScanForFontFiles())
                .Distinct();
        }

        private readonly IFontInfoRetriever _fontSource = new FontInfoRetriever();
        public IResult<FontModel[]> GetFontData(string fontFile)
        {
            return this._fontSource.GetFontInfo(fontFile);
        }
    }
}
