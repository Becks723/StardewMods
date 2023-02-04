using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.Models
{
    internal record BmFontConfig : FontConfig_, IWithPixelZoom
    {
        public float PixelZoom { get; }

        public BmFontConfig(FontConfig_ original, float pixelZoom) 
            : base(original)
        {
            this.PixelZoom = pixelZoom;
        }

        public BmFontConfig(bool Enabled, string FontFilePath, int FontIndex, float FontSize, float Spacing, float LineSpacing, float CharOffsetX, float CharOffsetY, IEnumerable<CharacterRange> CharacterRanges, float PixelZoom) 
            : base(Enabled, FontFilePath, FontIndex, FontSize, Spacing, LineSpacing, CharOffsetX, CharOffsetY, CharacterRanges)
        {
            this.PixelZoom = PixelZoom;
        }
    }
}
