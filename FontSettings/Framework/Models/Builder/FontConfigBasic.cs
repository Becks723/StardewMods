using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.Models.Builder
{
    internal sealed record FontConfigBasic : FontConfigDecorator
    {
        public FontConfigBasic(FontConfig config)
            : base(config)
        {
        }

        public override bool Supports<T>() => this is T;
        public override T GetInstance<T>() => this as T;
    }
}
