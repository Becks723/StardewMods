using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Preset;

namespace FontSettings.Framework.Models.Builder
{
    internal class FontPresetWithName : FontPresetDecorator, IPresetWithName
    {
        private readonly Func<string?> _name;

        public string? Name => this._name();

        public FontPresetWithName(FontPresetDecorator preset, string? name)
            : this(preset, () => name)
        {
        }

        public FontPresetWithName(FontPresetDecorator preset, Func<string?> name)
            : base(preset)
        {
            this._name = name;
        }
    }
}
