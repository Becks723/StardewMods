using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CommandLine;

namespace FontSettings.CommandLine
{
    internal class MakeFontOption
    {
        [Option(shortName: 'f', longName: "out-format", Required = true, HelpText = "Output font format. One of 'spritefont', 'bmfont'.")]
        public FontFormat OutputFormat { get; set; } // required, specifies the output font format.

        [Option(shortName: 'n', longName: "out-name", Required = true, HelpText = "Name of the output file.")]
        public string OutputName { get; set; } // required, name of the output file (without extension).

        [Option(longName: "out-dir", Required = false, HelpText = "If given, overrides default output directory.")]
        public string OutputDirectory { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output"); // optional, if given, then overrides the default output directory.

        [Option(longName: "xnb", Required = false, Default = null, HelpText = "Whether to pack output into xnb file(s). Type 'true' or 'false'.")]
        public bool? OverrideInXnb { get; set; } // optional, whether to pack output into xnb file(s), default depend on format.
                                                 // SpriteFont -> true
                                                 // BmFont     -> false

        public bool InXnb => this.OverrideInXnb ?? this.DefaultInXnb();

        // the following two must be ONLY one provided.
        [Option("font-path", Required = false, HelpText = "Either full path or simple filename with extensions.")]
        public string? FontFilePath { get; set; } // optional, a either full path or filename with extensions.

        [Option("font-name", Required = false, HelpText = "Font name to search.")]
        public string? FontName { get; set; } // optional, the font name

        [Option("font-index", Required = false, Default = 0, HelpText = "When font file is a font collection (.ttc/.otc), specifies index of the correct font. Default 0.")]
        public int FontIndex { get; set; } // optional, when input font file is a font collection (.ttc/.otc), then specifies index of the correct font, default 0.

        [Option("size", Required = true, HelpText = "Font size, either in pixels or points. E.g. '12pt' means 12 points, '24.5px' means 24.5 pixels. 1px = 0.75pt")]
        public FontSize FontSize { get; set; } // required, either in pixels or points. E.g. "12pt" means 12 points, "24.5px" means 24.5 pixels.

        [Option("spacing", Required = false, Default = 0f, HelpText = "Distance between two adjacent characters, in pixels. Default 0.")]
        public float Spacing { get; set; } // optional, distance between two characters, in pixels, default 0.

        [Option("linespacing", Required = false, Default = null, HelpText = "Distance between two adjacent baselines, in pixels. Default depends on the input font.")]
        public int? LineSpacing { get; set; } = null; // optional, distance between two baselines, in pixels, default depends on the font.

        [Option("xoff", Required = false, Default = 0f, HelpText = "An x-offset applied to all characters, in pixels. Default 0.")]
        public float CharOffsetX { get; set; } // optional, an x-offset applied to all characters, in pixels, default 0.

        [Option("yoff", Required = false, Default = 0f, HelpText = "An y-offset applied to all characters, in pixels. Default 0.")]
        public float CharOffsetY { get; set; } // optional, same to offset x.

        // the following two must be ONLY one provided.
        public IEnumerable<char>? Characters { get; set; } // optional, 
        public string? TextFilePath { get; set; } // optional, specifies path to a txt file containing all the characters needed.

        [Option("page-size", Required = false, Default = null, HelpText = "Overrides default BmFont page size.")]
        public SizeI? PageSize { get; set; } // optional, bmfont-specific, overrides default bmfont page size.

        private bool DefaultInXnb()
        {
            switch (this.OutputFormat)
            {
                case FontFormat.SpriteFont:
                    return true;
                case FontFormat.BmFont:
                    return false;
                default:
                    throw new NotSupportedException();
            }
        }
    }

    internal enum FontFormat
    {
        SpriteFont,
        BmFont
    }
}
