using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValleyUI;
using StardewValleyUI.Controls;
using StardewValleyUI.Controls.Primitives;

namespace FontSettings.Framework.Menus.Views.Components
{
    internal class FontPreviewGrid : Grid
    {
        private readonly TextureBox _box;
        private readonly float _padding;

        private static readonly UIPropertyInfo ModeProperty
            = new UIPropertyInfo(nameof(Mode), typeof(PreviewMode), typeof(FontPreviewGrid), PreviewMode.Normal, OnModeChanged);
        public PreviewMode Mode
        {
            get { return this.GetValue<PreviewMode>(ModeProperty); }
            set { this.SetValue(ModeProperty, value); }
        }

        private static void OnModeChanged(object sender, UIPropertyChangedEventArgs e)
        {
            var grid = sender as FontPreviewGrid;

            grid.OnModeChanged((PreviewMode)e.OldValue, (PreviewMode)e.NewValue);
        }

        public Orientation Orientation { get; set; } = Orientation.Vertical;

        public FontExampleLabel VanillaFontExample { get; }

        public FontExampleLabel CurrentFontExample { get; }

        public FontPreviewGrid(TextureBox box = null, float padding = 8)
        {
            this._box = box ?? TextureBoxes.Default;
            this._padding = padding;

            this.VanillaFontExample = new FontExampleLabel()
            {
                Forground = Color.Gray * 0.67f,
                Wrapping = TextWrapping.Enable
            };
            this.CurrentFontExample = new FontExampleLabel()
            {
                Forground = Game1.textColor,
                Wrapping = TextWrapping.Enable
            };

            this.OnModeChanged(PreviewMode.Normal, PreviewMode.Normal);
        }

        private void OnModeChanged(PreviewMode oldValue, PreviewMode newValue)
        {
            bool horiz = this.Orientation == Orientation.Horizontal;

            this.Children.Clear();  // 务必先于清空row/column。
            if (horiz)
                this.ColumnDefinitions.Clear();
            else
                this.RowDefinitions.Clear();

            switch (newValue)
            {
                case PreviewMode.Normal:
                    this.ChangeToNormal();
                    break;

                case PreviewMode.Compare:
                    this.ChangeToCompare();
                    break;

                case PreviewMode.PreciseCompare:
                    this.ChangeToPreciseCompare();
                    break;
            }
        }

        private void ChangeToNormal()
        {
            bool horiz = this.Orientation == Orientation.Horizontal;

            var border = new TextureBoxBorder();
            border.Box = this._box;
            border.DrawShadow = false;
            border.Padding += new Thickness(this._padding);
            this.Children.Add(border);
            {
                ScrollViewer view = new ScrollViewer();
                view.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                view.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                view.ShowsBackground = false;
                border.Child = view;
                {
                    this.CurrentFontExample.HorizontalAlignment = HorizontalAlignment.Stretch;
                    this.CurrentFontExample.VerticalAlignment = VerticalAlignment.Stretch;
                    view.Content = this.CurrentFontExample;
                }
            }
        }

        private void ChangeToCompare()
        {
            bool horiz = this.Orientation == Orientation.Horizontal;

            if (horiz)
            {
                this.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnit.Percent) });
                this.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnit.Percent) });
            }
            else
            {
                this.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnit.Percent) });
                this.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnit.Percent) });
            }
            {
                var vanillaBorder = new TextureBoxBorder();
                vanillaBorder.Box = this._box;
                vanillaBorder.DrawShadow = false;
                vanillaBorder.Padding += new Thickness(this._padding);
                this.Children.Add(vanillaBorder);
                if (horiz)
                    this.SetColumn(vanillaBorder, 0);
                else
                    this.SetRow(vanillaBorder, 0);
                {
                    ScrollViewer view = new ScrollViewer();
                    view.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                    view.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                    view.ShowsBackground = false;
                    vanillaBorder.Child = view;
                    {
                        this.VanillaFontExample.HorizontalAlignment = HorizontalAlignment.Stretch;
                        this.VanillaFontExample.VerticalAlignment = VerticalAlignment.Stretch;
                        view.Content = this.VanillaFontExample;
                    }
                }

                var currentBorder = new TextureBoxBorder();
                currentBorder.Box = this._box;
                currentBorder.DrawShadow = false;
                currentBorder.Padding += new Thickness(this._padding);
                this.Children.Add(currentBorder);
                if (horiz)
                    this.SetColumn(currentBorder, 1);
                else
                    this.SetRow(currentBorder, 1);
                {
                    ScrollViewer view = new ScrollViewer();
                    view.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                    view.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                    view.ShowsBackground = false;
                    currentBorder.Child = view;
                    {
                        this.CurrentFontExample.HorizontalAlignment = HorizontalAlignment.Stretch;
                        this.CurrentFontExample.VerticalAlignment = VerticalAlignment.Stretch;
                        view.Content = this.CurrentFontExample;
                    }
                }
            }
        }

        private void ChangeToPreciseCompare()
        {
            bool horiz = this.Orientation == Orientation.Horizontal;

            var border = new TextureBoxBorder();
            border.Box = this._box;
            border.DrawShadow = false;
            border.Padding += new Thickness(this._padding);
            this.Children.Add(border);
            {
                ScrollViewer view = new ScrollViewer();
                view.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                view.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                view.ShowsBackground = false;
                border.Child = view;
                {
                    var examples = new MergedFontExampleLabels(this.VanillaFontExample, this.CurrentFontExample);
                    this.VanillaFontExample.HorizontalAlignment = HorizontalAlignment.Left;
                    this.VanillaFontExample.VerticalAlignment = VerticalAlignment.Top;
                    this.CurrentFontExample.HorizontalAlignment = HorizontalAlignment.Left;
                    this.CurrentFontExample.VerticalAlignment = VerticalAlignment.Top;

                    view.Content = examples;
                }
            }
        }

        private class MergedFontExampleLabels : Container
        {
            private readonly FontExampleLabel _vanilla;
            private readonly FontExampleLabel _current;

            public MergedFontExampleLabels(FontExampleLabel vanilla, FontExampleLabel current)
            {
                this._vanilla = vanilla;
                this._current = current;

                this.Children.Add(vanilla);
                this.Children.Add(current);
            }

            protected override Vector2 MeasureOverride(Vector2 availableSize)
            {
                var vSize = this._vanilla.Measure(availableSize);
                var cSize = this._current.Measure(availableSize);

                return new Vector2(
                    Math.Max(vSize.X, cSize.X),
                    Math.Max(vSize.Y, cSize.Y));
            }

            protected override Vector2 ArrangeOverride(Vector2 availableSize)
            {
                var finalRect = new RectangleF(Vector2.Zero, availableSize);
                this._vanilla.Arrange(finalRect);
                this._current.Arrange(finalRect);
                return availableSize;
            }
        }
    }
}
