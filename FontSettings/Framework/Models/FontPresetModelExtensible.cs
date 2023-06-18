using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Preset;
using StardewModdingAPI;

namespace FontSettings.Framework.Models
{
    internal class FontPresetModelExtensible : FontPresetModel, IPresetWithKey<string>, IPresetFromContentPack
    {
        private readonly ExtendType _extendType;

        public string Key { get; set; }

        public IContentPack SContentPack { get; set; }

        public FontPresetModelExtensible(FontPresetModel basedOn, ExtendType extendType)
            : base(basedOn.Context, basedOn.Settings)
        {
            this._extendType = extendType;
        }

        public override bool Supports<T>()
        {
            if (typeof(T) == typeof(IPresetWithKey<string>))
                return this._extendType == ExtendType.WithKey;
            else if (typeof(T) == typeof(IPresetFromContentPack))
                return this._extendType == ExtendType.FromContentPack;
            else
                return base.Supports<T>();
        }

        internal enum ExtendType
        {
            WithKey,
            FromContentPack
        }
    }
}
