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

namespace FontSettings.Framework.Menus
{
    internal class FontPreviewGrid : Grid
    {
        private readonly TextureBox _box;

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

        public Orientation Orientation { get; set; } = Orientation.Vertical;

        public FontExampleLabel VanillaFontExample { get; }

        public FontExampleLabel CurrentFontExample { get; }

        public FontPreviewGrid(TextureBox box)
        {
            this._box = box;

            this.VanillaFontExample = new FontExampleLabel() { Forground = Color.Gray * 0.67f };
            this.CurrentFontExample = new FontExampleLabel() { Forground = Game1.textColor };

            this.OnIsMergedChanged(true, false);
        }

        private void OnIsMergedChanged(bool oldValue, bool newValue)
        {
            bool horiz = this.Orientation == Orientation.Horizontal;
            if (horiz)
                this.ColumnDefinitions.Clear();
            else
                this.RowDefinitions.Clear();
            this.Children.Clear();

            if (newValue)
            {
                var border = new TextureBoxBorder();
                border.Box = _box;
                border.DrawShadow = false;
                this.Children.Add(border);
                {
                    Grid grid = new Grid();
                    border.Child = grid;
                    {
                        var titleLabel = new Label();
                        titleLabel.Font = FontType.SpriteText;
                        titleLabel.HorizontalAlignment = HorizontalAlignment.Left;
                        titleLabel.VerticalAlignment = VerticalAlignment.Top;
                        grid.Children.Add(titleLabel);

                        this.VanillaFontExample.HorizontalAlignment = HorizontalAlignment.Stretch;
                        this.VanillaFontExample.VerticalAlignment = VerticalAlignment.Stretch;
                        this.CurrentFontExample.HorizontalAlignment = HorizontalAlignment.Stretch;
                        this.CurrentFontExample.VerticalAlignment = VerticalAlignment.Stretch;

                        var examples = new MergedFontExampleLabels(this.VanillaFontExample, this.CurrentFontExample);
                        examples.HorizontalAlignment = HorizontalAlignment.Center;
                        examples.VerticalAlignment = VerticalAlignment.Center;
                        grid.Children.Add(examples);
                    }
                }
            }
            else
            {
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
                    vanillaBorder.Box = _box;
                    vanillaBorder.DrawShadow = false;
                    this.Children.Add(vanillaBorder);
                    if (horiz)
                        this.SetColumn(vanillaBorder, 0);
                    else
                        this.SetRow(vanillaBorder, 0);
                    {
                        Grid grid = new Grid();
                        vanillaBorder.Child = grid;
                        {
                            var titleLabel = new Label();
                            titleLabel.Text = I18n.Ui_MainMenu_VanillaExample();
                            titleLabel.Font = FontType.SpriteText;
                            titleLabel.HorizontalAlignment = HorizontalAlignment.Left;
                            titleLabel.VerticalAlignment = VerticalAlignment.Top;
                            grid.Children.Add(titleLabel);

                            this.VanillaFontExample.HorizontalAlignment = HorizontalAlignment.Center;
                            this.VanillaFontExample.VerticalAlignment = VerticalAlignment.Center;
                            grid.Children.Add(this.VanillaFontExample);
                        }
                    }

                    var currentBorder = new TextureBoxBorder();
                    currentBorder.Box = _box;
                    currentBorder.DrawShadow = false;
                    this.Children.Add(currentBorder);
                    if (horiz)
                        this.SetColumn(currentBorder, 1);
                    else
                        this.SetRow(currentBorder, 1);
                    {
                        Grid grid = new Grid();
                        currentBorder.Child = grid;
                        {
                            var titleLabel = new Label();
                            titleLabel.Text = I18n.Ui_MainMenu_CurrentExample();
                            titleLabel.Font = FontType.SpriteText;
                            titleLabel.HorizontalAlignment = HorizontalAlignment.Left;
                            titleLabel.VerticalAlignment = VerticalAlignment.Top;
                            grid.Children.Add(titleLabel);

                            this.CurrentFontExample.HorizontalAlignment = HorizontalAlignment.Center;
                            this.CurrentFontExample.VerticalAlignment = VerticalAlignment.Center;
                            grid.Children.Add(this.CurrentFontExample);
                        }
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
