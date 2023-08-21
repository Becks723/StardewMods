using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace FontSettings.Framework
{
    internal class FontExportSettings
    {
        // Common
        public FontFormat Format { get; }
        public bool InXnb { get; }
        public string OutputDirectory { get; }
        public string OutputFileName { get; }  // without file extension

        // xnb specific
        public XnbPlatform XnbPlatform { get; }
        public GameFramework GameFramework { get; }
        public GraphicsProfile GraphicsProfile { get; }
        public bool IsCompressed { get; }

        // BmFont specific
        public int PageWidth { get; }  // TODO: 放到IBmFontConfig里去？
        public int PageHeight { get; }  // TODO: 放到IBmFontConfig里去？

        public FontExportSettings(FontFormat format, string outputDirectory, string outputFileName, XnbPlatform xnbPlatform, GameFramework gameFramework, GraphicsProfile graphicsProfile, bool isCompressed, int pageWidth, int pageHeight)
            : this(format, inXnb: true, outputDirectory, outputFileName, xnbPlatform, gameFramework, graphicsProfile, isCompressed, pageWidth, pageHeight)
        {
        }

        public FontExportSettings(FontFormat format, bool inXnb, string outputDirectory, string outputFileName, XnbPlatform xnbPlatform, GameFramework gameFramework, GraphicsProfile graphicsProfile, bool isCompressed, int pageWidth, int pageHeight)
        {
            this.Format = format;
            this.InXnb = inXnb;
            this.OutputDirectory = outputDirectory;
            this.OutputFileName = outputFileName;
            this.XnbPlatform = xnbPlatform;
            this.GameFramework = gameFramework;
            this.GraphicsProfile = graphicsProfile;
            this.IsCompressed = isCompressed;
            this.PageWidth = pageWidth;
            this.PageHeight = pageHeight;
        }
    }
}
