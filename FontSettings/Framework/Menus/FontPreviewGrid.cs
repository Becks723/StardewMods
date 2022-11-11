using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValleyUI;
using StardewValleyUI.Controls;

namespace FontSettings.Framework.Menus
{
    internal class FontPreviewGrid : Grid
    {
        private readonly int _borderWidth;

        private static readonly UIPropertyInfo IsMergedProperty
            = new UIPropertyInfo(nameof(IsMerged), typeof(bool), typeof(FontPreviewGrid), false, OnIsMergedChanged);
        public bool IsMerged
        {
            get { return this.GetValue<bool>(IsMergedProperty); }
            set { this.SetValue(IsMergedProperty, value); }
        }

        private static void OnIsMergedChanged(object sender, UIPropertyChangedEventArgs e)
        {
            var grid = sender as FontPreviewGrid;

            grid.OnIsMergedChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        public FontExampleLabel VanillaFontExample { get; }

        public FontExampleLabel CurrentFontExample { get; }

        public FontPreviewGrid(int borderWidth)
        {
            this.VanillaFontExample = new FontExampleLabel() { Forground = Color.Gray * 0.67f };
            this.CurrentFontExample = new FontExampleLabel() { Forground = Game1.textColor };

            this._borderWidth = borderWidth;

            this.OnIsMergedChanged(true, false);
        }

        private void OnIsMergedChanged(bool oldValue, bool newValue)
        {
            this.ColumnDefinitions.Clear();
            this.Children.Clear();

            if (newValue)
            {
                var border = new FontExampleBorder();
                border.Box = TextureBoxes.Patterns;
                border.DrawShadow = false;
                this.Children.Add(border);
                {
                    this.VanillaFontExample.HorizontalAlignment = HorizontalAlignment.Stretch;
                    this.VanillaFontExample.VerticalAlignment = VerticalAlignment.Stretch;
                    this.CurrentFontExample.HorizontalAlignment = HorizontalAlignment.Stretch;
                    this.CurrentFontExample.VerticalAlignment = VerticalAlignment.Stretch;

                    var examples = new MergedFontExampleLabels(this.VanillaFontExample, this.CurrentFontExample);
                    examples.HorizontalAlignment = HorizontalAlignment.Center;
                    examples.VerticalAlignment = VerticalAlignment.Center;
                    border.Child = examples;
                }
            }
            else
            {
                this.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnit.Percent) });
                this.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnit.Percent) });
                {
                    var vanillaBorder = new FontExampleBorder();
                    vanillaBorder.Box = TextureBoxes.Patterns;
                    vanillaBorder.DrawShadow = false;
                    vanillaBorder.Margin = new Thickness(0, 0, this._borderWidth / 6, 0);
                    this.Children.Add(vanillaBorder);
                    this.SetColumn(vanillaBorder, 0);
                    {
                        this.VanillaFontExample.HorizontalAlignment = HorizontalAlignment.Center;
                        this.VanillaFontExample.VerticalAlignment = VerticalAlignment.Center;
                        vanillaBorder.Child = this.VanillaFontExample;
                    }

                    var currentBorder = new FontExampleBorder();
                    currentBorder.Box = TextureBoxes.Patterns;
                    currentBorder.DrawShadow = false;
                    currentBorder.Margin = new Thickness(this._borderWidth / 6, 0, 0, 0);
                    this.Children.Add(currentBorder);
                    this.SetColumn(currentBorder, 1);
                    {
                        this.CurrentFontExample.HorizontalAlignment = HorizontalAlignment.Center;
                        this.CurrentFontExample.VerticalAlignment = VerticalAlignment.Center;
                        currentBorder.Child = this.CurrentFontExample;
                    }
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

            protected override void ArrangeOverride(Vector2 availableSize)
            {
                this._vanilla.Arrange(Vector2.Zero, availableSize);
                this._current.Arrange(Vector2.Zero, availableSize);
            }
        }
    }
}
