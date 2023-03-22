using System;
using System.Collections.Generic;
using System.Text;

namespace FontSettings.Shared.CommandLine
{
    internal class MakeFontOption
    {
        public FontFormat OutputFormat { get; set; } // required, specifies the output font format.

        public string OutputName { get; set; } // required, name of the output file (without extension).

        public string OutputDirectory { get; set; } // optional, if given, then overrides the default output directory.

        public bool InXnb { get; set; } // optional, whether to pack output into xnb file(s), default depend on format.
                                                                                                // SpriteFont -> true
                                                                                                // BmFont     -> false

        // the following two must be ONLY one provided.
        public string? FontFilePath { get; set; } // optional, a either full path or filename with extensions.
        public string? FontName { get; set; } // optional, the font name

        public int FontIndex { get; set; } // optional, when input font file is a font collection (.ttc/.otc), then specifies index of the correct font, default 0.
       
        public float FontSize { get; set; } // required, either in pixel or point. E.g. "12pt" means 12 points, "24.5px" means 24.5 pixels.

        public float Spacing { get; set; } // optional, distance between two characters, in pixel, default 0.

        public float LineSpacing { get; set; } // optional, distance between two baselines, in pixel, default depends on the font.

        public float CharOffsetX { get; set; } // optional, an x-offset applied to all characters, in pixel, default 0.

        public float CharOffsetY { get; set; } // optional, same to offset x.

        // the following two must be ONLY one provided.
        public IEnumerable<char>? Characters { get; set; } // optional, 
        public string? TextFilePath { get; set; } // optional, specifies path to a txt file containing all the characters needed.
    }

    internal enum FontFormat
    {
        SpriteFont,
        BmFont
    }
}
