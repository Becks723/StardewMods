using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Models;

namespace FontSettings.Framework.Preset
{
    internal class FontPresetWithDescription : FontPresetWithName, IPresetWithDescription
    {
        public string Description { get; }

        public FontPresetWithDescription(LanguageInfo language, GameFontType fontType, FontConfig_ settings, string name, string description)
            : base(language, fontType, settings, name)
        {
            this.Description = description;
        }
    }
}
