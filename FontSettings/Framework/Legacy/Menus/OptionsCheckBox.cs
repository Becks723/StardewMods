using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using StardewValley.Menus;

namespace FontSettings.Framework.Menus
{
    internal class OptionsCheckBox : OptionsCheckbox
    {
        public event EventHandler ValueChanged;

        public OptionsCheckBox(string label, bool initValue = false, int x = -1, int y = -1)
            : base(label, int.MaxValue, x, y)
        {
            this.isChecked = initValue;
        }

        public override void receiveLeftClick(int x, int y)
        {
            if (!this.greyedOut)
            {
                Game1.playSound("drumkit6");
                selected = this;
                this.isChecked = !this.isChecked;
                this.RaiseValueChanged(EventArgs.Empty);
                selected = null;
            }
        }

        protected virtual void RaiseValueChanged(EventArgs e)
        {
            ValueChanged?.Invoke(this, e);
        }
    }
}
