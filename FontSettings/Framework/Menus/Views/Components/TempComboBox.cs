using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using StardewValleyUI;
using StardewValleyUI.Controls;

namespace FontSettings.Framework.Menus.Views.Components
{
    /// <summary>A temporary class to handle ComboBox drop down glitch when large data.</summary>
    internal class TempComboBox : ComboBox
    {
        private readonly bool _isSimplified;

        public TempComboBox(bool isSimplified)
        {
            this._isSimplified = isSimplified;
        }

        protected override Element CreateItemWrapper()
        {
            if (!this._isSimplified)
                return base.CreateItemWrapper();
            else
            {
                var comboBoxItem = new SimplifiedComboBoxItem();
                this.SetComboBox(comboBoxItem, this);
                return comboBoxItem;
            }
        }

        private static readonly PropertyInfo _comboBoxItem_ComboBox = typeof(ComboBoxItem)
            .GetProperty("ComboBox",
                BindingFlags.Instance | BindingFlags.NonPublic);
        private void SetComboBox(ComboBoxItem item, ComboBox comboBox)
        {
            _comboBoxItem_ComboBox.SetValue(item, comboBox);  // PropertyInfo null not allowed.
        }

        private class SimplifiedComboBoxItem : ComboBoxItem
        {
            protected override ControlAppearance GetDefaultAppearance()
            {
                return StardewValleyUI.Appearance.ForControl(new SimpleComboBoxItemAppearance());
            }

            private class SimpleComboBoxItemAppearance : ControlAppearanceBuilder<ComboBoxItem>
            {
                protected override Element Build(AppearanceBuildContext<ComboBoxItem> context)
                {
                    var contentPresenter = new ContentPresenter();
                    context.DefinePart("PART_ContentPresenter", contentPresenter);
                    return contentPresenter;
                }
            }
        }
    }
}
