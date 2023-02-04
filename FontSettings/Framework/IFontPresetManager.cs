using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Models;
using FontSettings.Framework.Preset;

namespace FontSettings.Framework
{
    internal interface IFontPresetManager
    {
        void UpdatePreset(string name, FontPresetReal? preset);

        IEnumerable<FontPresetReal> GetPresets(LanguageInfo language, GameFontType fontType);

        bool IsReadOnlyPreset(FontPresetReal preset);

        bool IsValidPresetName(string? name, out InvalidPresetNameTypes? invalidType);
    }
}
