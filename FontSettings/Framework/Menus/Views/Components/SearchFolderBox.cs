using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValleyUI;
using StardewValleyUI.Controls;

namespace FontSettings.Framework.Menus.Views.Components
{
    internal class SearchFolderBox : Selector
    {
        internal readonly static UIPropertyInfo NewFolderCommandProperty
            = new UIPropertyInfo(nameof(NewFolderCommand), typeof(ICommand), typeof(SearchFolderBox), null);
        public ICommand NewFolderCommand
        {
            get { return this.GetValue<ICommand>(NewFolderCommandProperty); }
            set { this.SetValue(NewFolderCommandProperty, value); }
        }

        internal readonly static UIPropertyInfo DeleteFolderCommandProperty
            = new UIPropertyInfo(nameof(DeleteFolderCommand), typeof(ICommand), typeof(SearchFolderBox), null);
        public ICommand DeleteFolderCommand
        {
            get { return this.GetValue<ICommand>(DeleteFolderCommandProperty); }
            set { this.SetValue(DeleteFolderCommandProperty, value); }
        }

        internal readonly static UIPropertyInfo EditFolderCommandProperty
            = new UIPropertyInfo(nameof(EditFolderCommand), typeof(ICommand), typeof(SearchFolderBox), null);
        public ICommand EditFolderCommand
        {
            get { return this.GetValue<ICommand>(EditFolderCommandProperty); }
            set { this.SetValue(EditFolderCommandProperty, value); }
        }

        public SearchFolderBox()
        {
            SelectionChanged += this.OnSelectionChanged;
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            this.UpdateItemIsSelectedProperty(this.SelectedIndex);
        }

        protected internal override Element CreateItemWrapper()
        {
            return new SearchFolderBoxItem() { Owner = this };
        }

        protected override ControlAppearance GetDefaultAppearance()
        {
            return StardewValleyUI.Appearance.ForControl(new SearchFolderBoxAppearance());
        }

        private void UpdateItemIsSelectedProperty(int index)
        {
            if (index == -1)
            {
                // unselect all.
                foreach (Element elem in this.Items)
                {
                    var sfbi = elem as SearchFolderBoxItem;
                    Debug.Assert(sfbi != null);

                    sfbi.IsSelected = false;
                }
            }
            else
            {
                this.UpdateItemIsSelectedProperty(
                    this.Items[index] as SearchFolderBoxItem);
            }
        }

        internal void UpdateItemIsSelectedProperty(SearchFolderBoxItem item)
        {
            item.IsSelected = true;

            // Notify all other unselected items. (selection only one at a time)
            foreach (Element elem in this.Items)
            {
                var sfbi = elem as SearchFolderBoxItem;
                Debug.Assert(sfbi != null);

                if (!object.ReferenceEquals(sfbi, item))
                    sfbi.IsSelected = false;
            }
        }
    }
}
