using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValleyUI;
using StardewValleyUI.Controls;

namespace FontSettings.Framework.Menus.Views.Components
{
    internal class SwitchTextureButton<TEnum> : TextureButton
        where TEnum : struct, Enum
    {
        private readonly Action<TEnum, SwitchTextureButton<TEnum>> _onSwitched;

        public SwitchTextureButton(Action<TEnum, SwitchTextureButton<TEnum>> onSwitched, TEnum defaultFlag = default)
        {
            this._onSwitched = onSwitched;

            this.Flag = defaultFlag;
        }

        private static readonly UIPropertyInfo FlagProperty
            = new UIPropertyInfo(nameof(Flag), typeof(TEnum), typeof(SwitchTextureButton<>), default(TEnum), OnSwitched);
        public TEnum Flag
        {
            get { return this.GetValue<TEnum>(FlagProperty); }
            set { this.SetValue(FlagProperty, value); }
        }

        private static void OnSwitched(object sender, UIPropertyChangedEventArgs e)
        {
            var button = (SwitchTextureButton<TEnum>)sender;
            TEnum newFlag = (TEnum)e.NewValue;

            button._onSwitched(newFlag, button);
        }

        protected override void RaiseClick(EventArgs e)
        {
            base.RaiseClick(e);

            this.Flag = this.Flag.Next();
        }
    }
}
