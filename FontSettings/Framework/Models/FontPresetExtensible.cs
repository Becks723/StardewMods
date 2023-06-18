using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Preset;

namespace FontSettings.Framework.Models
{
    internal sealed class FontPresetExtensible : FontPreset
    {
        private readonly IEnumerable<IExtensible> _extensibles;

        public FontPresetExtensible(FontPreset basedOn, params IExtensible[] extensibles)
            : this(basedOn, extensibles.AsEnumerable())
        {
        }

        public FontPresetExtensible(FontPreset basedOn, IEnumerable<IExtensible> extensibles)
            : base(basedOn)
        {
            this._extensibles = extensibles;
        }

        public override bool Supports<T>()
        {
            return this.GetInstance<T>() != null;
        }

        public override T GetInstance<T>()
        {
            foreach (IExtensible extensible in this._extensibles)
            {
                if (extensible == null)
                    continue;

                if (extensible.TryGetInstance(out T instance))  // TODO: 当前：如果有多个符合，取第一个。如果想取其他的怎么办？
                    return instance;
            }

            return base.GetInstance<T>();
        }
    }
}
