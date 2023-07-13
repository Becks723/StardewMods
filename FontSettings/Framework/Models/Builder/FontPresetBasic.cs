using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Preset;

namespace FontSettings.Framework.Models.Builder
{
    internal sealed class FontPresetBasic : FontPresetDecorator
    {
        public FontPresetBasic(FontPreset basicPreset)
            : base(basicPreset)
        {
        }

        public override bool Supports<T>() => this is T;
        public override T GetInstance<T>() => this as T;
    }
}
