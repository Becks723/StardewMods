using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Preset;

namespace FontSettings.Framework.Models
{
    internal class FontPresetExtensible : FontPreset, IPresetWithKey<string>, IPresetWithName, IPresetWithDescription
    {
        public string Description { get; set; }

        public string Name { get; set; }

        public string Key { get; set; }

        public FontPresetExtensible(FontPreset copy) 
            : base(copy)
        {
        }

        public FontPresetExtensible(FontContext context, FontConfig settings)
            : base(context, settings)
        {
        }
    }
}
