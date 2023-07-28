using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace FontSettings.Framework
{
    internal class ModConfigWatcher
    {
        private readonly ModConfig _config;

        private ModConfigSnapshot _snapshot;

        public event EventHandler TextShadowToggled;
        public event EventHandler ShadowColorGame1Changed;
        public event EventHandler TextColorChanged;

        public ModConfigWatcher(ModConfig config)
        {
            this._config = config;
        }

        public void Update()
        {
            ModConfigSnapshot snapshot = this.TakeSnapshot(this._config);

            if (snapshot.ChangedIn(x => x.DisableTextShadow, this._snapshot))
            {
                TextShadowToggled?.Invoke(this, EventArgs.Empty);
            }

            if (snapshot.ChangedIn(x => x.ShadowColorGame1, this._snapshot))
            {
                ShadowColorGame1Changed?.Invoke(this, EventArgs.Empty);
            }

            if (snapshot.ChangedIn(x => x.TextColor, this._snapshot))
            {
                TextColorChanged?.Invoke(this, EventArgs.Empty);
            }

            this._snapshot = snapshot;
        }

        private ModConfigSnapshot TakeSnapshot(ModConfig config)
        {
            return new ModConfigSnapshot()
            {
                DisableTextShadow = config.DisableTextShadow,
                ShadowColorGame1 = config.ShadowColorGame1,
                TextColor = config.TextColor
            };
        }

        private class ModConfigSnapshot
        {
            public bool DisableTextShadow;
            public Color ShadowColorGame1;
            public Color TextColor;

            public bool ChangedIn<TField>(Func<ModConfigSnapshot, TField> field, ModConfigSnapshot? contrast,
                IEqualityComparer<TField>? comparer = null)
            {
                // null的话就是第一次
                if (contrast == null)
                    return true;

                comparer ??= EqualityComparer<TField>.Default;

                TField thisField = field(this);
                TField thatField = field(contrast);

                return !comparer.Equals(thisField, thatField);
            }
        }
    }
}
