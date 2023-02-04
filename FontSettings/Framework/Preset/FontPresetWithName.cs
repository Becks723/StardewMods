using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Models;

namespace FontSettings.Framework.Preset
{
    internal class FontPresetWithName : FontPresetReal, IPresetWithName
    {
        public string Name { get; }

        public FontPresetWithName(LanguageInfo language, GameFontType fontType, FontConfig_ settings, string name) 
            : base(language, fontType, settings)
        {
            this.Name = name;
        }

        public FontPresetWithName(FontPresetReal copy, string name) 
            : base(copy)
        {
            this.Name = name;
        }
    }
}
