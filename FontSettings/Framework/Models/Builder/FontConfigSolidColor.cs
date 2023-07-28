using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace FontSettings.Framework.Models.Builder
{
    internal record FontConfigSolidColor : FontConfigDecorator, IWithSolidColor
    {
        public Color SolidColor { get; }

        public FontConfigSolidColor(FontConfigDecorator config, Color solidColor)
            : base(config)
        {
            this.SolidColor = solidColor;
        }
    }
}
