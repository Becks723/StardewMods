using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontSettings.Framework.Preset
{
    internal class FontPresetWithKey : FontPresetWithName, IPresetWithKey<string>
    {
        public string Key { get; }

        public FontPresetWithKey(FontPresetWithName copy, string key)
            : base(copy)
        {
            this.Key = key;
        }

        protected FontPresetWithKey(FontPresetWithKey copy)
            : this(copy, copy.Key)
        {
        }
    }
}
