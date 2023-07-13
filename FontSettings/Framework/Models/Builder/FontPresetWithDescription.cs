using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Preset;

namespace FontSettings.Framework.Models.Builder
{
    internal class FontPresetWithDescription : FontPresetDecorator, IPresetWithDescription
    {
        private readonly Func<string?> _description;

        public string Description => this._description();

        public string Name => throw new NotSupportedException();

        public FontPresetWithDescription(FontPresetDecorator preset, string? description)
            : this(preset, () => description)
        {
        }

        public FontPresetWithDescription(FontPresetDecorator preset, Func<string?> description)
            : base(preset)
        {
            this._description = description;
        }
    }
}
