using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Preset;

namespace FontSettings.Framework.Models.Builder
{
    internal class FontPresetWithKey<TKey> : FontPresetDecorator, IPresetWithKey<TKey>
    {
        public TKey Key { get; }

        public FontPresetWithKey(FontPresetDecorator preset, TKey key)
            : base(preset)
        {
            this.Key = key;
        }
    }
}