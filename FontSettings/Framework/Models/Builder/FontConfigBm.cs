using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.Models.Builder
{
    internal record FontConfigBm : FontConfigDecorator, IWithPixelZoom
    {
        public float PixelZoom { get; }

        public FontConfigBm(FontConfigDecorator config, float pixelZoom)
            : base(config)
        {
            this.PixelZoom = pixelZoom;
        }
    }
}
