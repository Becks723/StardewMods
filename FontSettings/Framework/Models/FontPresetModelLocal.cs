using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Preset;

namespace FontSettings.Framework.Models
{
    internal class FontPresetModelLocal : FontPresetModel, IPresetWithKey<string>, IPresetWithName
    {
        public string Name { get; }

        public string Key { get; }

        public FontPresetModelLocal(FontPresetModel basedOn, string key)
            : base(basedOn.Context, basedOn.Settings)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));

            this.Key = key;
            this.Name = key;  // 根据实现，名称就是key
        }
    }
}
