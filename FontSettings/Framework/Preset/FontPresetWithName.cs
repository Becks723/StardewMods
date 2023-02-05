using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Models;

namespace FontSettings.Framework.Preset
{
    internal class FontPresetWithName : FontPreset, IPresetWithName
    {
        public string Name { get; }

        public FontPresetWithName(LanguageInfo language, GameFontType fontType, FontConfig settings, string name) 
            : base(language, fontType, settings)
        {
            this.Name = name;
        }

        public FontPresetWithName(FontPreset copy, string name) 
            : base(copy)
        {
            this.Name = name;
        }

        protected FontPresetWithName(FontPresetWithName copy)
            : this(copy, copy.Name)
        {
        }
    }
}
