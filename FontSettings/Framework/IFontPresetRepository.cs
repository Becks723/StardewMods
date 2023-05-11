using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Models;
using FontSettings.Framework.Preset;

namespace FontSettings.Framework
{
    internal interface IFontPresetRepository
    {
        IEnumerable<FontPreset> ReadPresets(FontConfigKey key);

        void WritePreset(string name, FontPreset? preset);
    }
}
