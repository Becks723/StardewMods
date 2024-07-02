using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValleyUI;
using StardewValleyUI.Controls;

namespace FontSettings.Framework.Menus.Views.Components
{
    internal class SearchFolderBoxItem : ContentControl
    {
        internal SearchFolderBox Owner { get; set; }

        internal readonly static UIPropertyInfo IsSelectedProperty
            = new UIPropertyInfo(nameof(IsSelected), typeof(bool), typeof(SearchFolderBoxItem), false);
        public bool IsSelected
        {
            get { return this.GetValue<bool>(IsSelectedProperty); }
            set { this.SetValue(IsSelectedProperty, value); }
        }

        protected override void OnMousePressed(MouseButtonEventArgs e)
        {
            base.OnMousePressed(e);

            this.Owner?.UpdateItemIsSelectedProperty(this);
            this.Owner?.UpdateSelection(this.Content);
        }

        #region Appearance impl
        private SolidColorElement _colorBlock;

        protected override ControlAppearance GetDefaultAppearance()
        {
            return StardewValleyUI.Appearance.ForControl<SearchFolderBoxItem>(builder: context =>
            {
                SearchFolderBoxItem item = context.Target;

                ((INotifyPropertyChanged)item).PropertyChanged += this.OnIsSelectedChanged;
                Grid grid = new Grid();
                {
                    var solidColor = this._colorBlock = new SolidColorElement();
                    solidColor.Color = Color.Transparent;
                    grid.Children.Add(solidColor);

                    var contentPresenter = new ContentPresenter();
                    context.DefinePart(ContentControl.CONTENT_PRESENTER_PART, contentPresenter);
                    grid.Children.Add(contentPresenter);
                }
                return grid;
            },
            destroyer: context =>
            {
                SearchFolderBoxItem item = context.Target;

                ((INotifyPropertyChanged)item).PropertyChanged -= this.OnIsSelectedChanged;
            });
        }

        private void OnIsSelectedChanged(object sender, PropertyChangedEventArgs e)
        {
            var item = (SearchFolderBoxItem)sender;

            if (e.PropertyName == nameof(SearchFolderBoxItem.IsSelected))
                this.OnIsSelectedChanged(item, item.IsSelected);
        }

        private void OnIsSelectedChanged(SearchFolderBoxItem item, bool isSelected)
        {
            var colorBlock = this._colorBlock;
            if (colorBlock == null)
                throw new InvalidOperationException("Not Build yet!");

            colorBlock.Color = isSelected
                ? Color.Wheat
                : Color.Transparent;
        }

        private class SolidColorElement : Element
        {
            public Color Color { get; set; }

            protected override void DrawOverride(SpriteBatch b)
            {
                b.Draw(Game1.staminaRect, this.Bounds, this.Color);
            }
        }
        #endregion
    }
}
