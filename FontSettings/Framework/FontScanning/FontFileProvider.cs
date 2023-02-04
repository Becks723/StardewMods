using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.FontInfomation;
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

        public ICollection<IFontFileScanner> Scanners { get; } = new List<IFontFileScanner>();

        public void RescanForFontFiles()
        {
            this._fontFiles = this.Scanners
                .Where(scanner => scanner != null)
                .SelectMany(scanner => scanner.ScanForFontFiles())
                .Distinct();
        }

        private readonly IFontInfoSource _fontSource = new FontInfoSource();
        public FontModel[] GetFontData(string fontFile)
        {
            return this._fontSource.GetFontInfo(fontFile);
        }
    }
}
