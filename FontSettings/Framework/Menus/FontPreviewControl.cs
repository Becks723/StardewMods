using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
    internal class FontPreviewControl : Control
    {
        private const string PART_CONTAINER = "PART_Container";

        private readonly TextureBox _box;

        private AlignedContainer _container;

        private static readonly UIPropertyInfo IsComparingProperty
            = new UIPropertyInfo(nameof(IsComparing), typeof(bool), typeof(FontPreviewControl), false, OnIsComparingChanged);
        public bool IsComparing
        {
            get { return this.GetValue<bool>(IsComparingProperty); }
            set { this.SetValue(IsComparingProperty, value); }
        }

        private static void OnIsComparingChanged(object sender, UIPropertyChangedEventArgs e)
        {
            var ctrl = (FontPreviewControl)sender;

            ctrl.OnIsComparingChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        public FontExampleLabel VanillaFontExample { get; }

        public FontExampleLabel CurrentFontExample { get; }

        public FontPreviewControl(TextureBox box)
        {
            this._box = box;

            this.VanillaFontExample = new FontExampleLabel()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Wrapping = TextWrapping.Enable,
                Forground = Color.Gray * 0.67f
            };
            this.CurrentFontExample = new FontExampleLabel()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Wrapping = TextWrapping.Enable,
                Forground = Game1.textColor
            };

            this.OnIsComparingChanged(true, false);
        }

        private void OnIsComparingChanged(bool oldValue, bool newValue)
        {
            if (oldValue == newValue) return;

            if (this._container != null)
            {
                if (newValue)
                {
                    this._container.Children.Clear();
                    this._container.Children.Add(this.VanillaFontExample);
                    this._container.Children.Add(this.CurrentFontExample);
                }
                else
                {
                    this._container.Children.Clear();
                    this._container.Children.Add(this.CurrentFontExample);
                }
            }
        }

        protected override void OnAppearanceChanged(ControlAppearance oldAppearance, ControlAppearance newAppearance)
        {
            base.OnAppearanceChanged(oldAppearance, newAppearance);

            this._container = this.GetAppearancePart<AlignedContainer>(PART_CONTAINER);
        }

        protected override ControlAppearance GetDefaultAppearance()
        {
            return StardewValleyUI.Appearance.ForControl<FontPreviewControl>(context =>
            {
                var border = new TextureBoxBorder();
                border.Box = TextureBoxes.Default;
                {
                    ScrollViewer scr = new ScrollViewer();
                    scr.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                    scr.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                    border.Child = scr;
                    {
                        var container = new AlignedContainer();
                        context.DefinePart(PART_CONTAINER, container);
                        scr.Content = container;
                    }
                }
                return border;
            });
        }

        private class AlignedContainer : Container
        {
            //protected override void OnPropertyChanged(UIPropertyChangedEventArgs e)
            //{
            //    base.OnPropertyChanged(e);

            //    if (e.Property == WidthProperty)
            //    {
            //        foreach (Element child in this.Children)
            //        {
            //            if (child == null)
            //                continue;

            //            child.MaxWidth = this.Width;
            //        }
            //    }
            //}

            protected override void OnChildrenChanged(NotifyCollectionChangedEventArgs e)
            {
                base.OnChildrenChanged(e);

                this.InvalidateMeasure();
            }

            protected override Vector2 MeasureOverride(Vector2 availableSize)
            {
                Vector2 measureSize = Vector2.Zero;

                foreach (Element child in this.Children)
                {
                    if (child == null)
                        continue;

                    Vector2 childMeasureSize = child.Measure(availableSize);

                    measureSize.X = Math.Max(measureSize.X, childMeasureSize.X);
                    measureSize.Y = Math.Max(measureSize.Y, childMeasureSize.Y);
                }

                return measureSize;
            }

            protected override void ArrangeOverride(Vector2 availableSize)
            {
                foreach (Element child in this.Children)
                {
                    if (child == null)
                        continue;

                    child.Arrange(Vector2.Zero, availableSize);
                }
            }
        }
    }
}
