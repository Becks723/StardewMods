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

        public override bool Supports<T>()
        {
            return typeof(T).IsAssignableFrom(typeof(IWithPixelZoom))
                || base.Supports<T>();
        }

        public override T GetInstance<T>()
        {
            if (typeof(T).IsAssignableFrom(typeof(IWithPixelZoom)))
                return this as T;

            return base.GetInstance<T>();
        }
    }
}
