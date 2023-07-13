using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Preset;

namespace FontSettings.Framework.Models.Builder
{
    internal class FontPresetDecorator : FontPreset
    {
        private FontPreset _preset;

        protected FontPresetDecorator(FontPreset copy)
            : base(copy)
        {
        }

        protected FontPresetDecorator(FontPresetDecorator preset)
            : base(preset)
        {
            this._preset = preset;
        }

        public override bool Supports<T>()
        {
            return this._preset.Supports<T>()
                || base.Supports<T>();
        }

        public override T GetInstance<T>()
        {
            T t = this._preset.GetInstance<T>();
            if (t != null)
                return t;

            t = base.GetInstance<T>();
            return t;
        }
    }
}
