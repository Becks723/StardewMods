using System.Collections.Generic;
using JunimoStudio.Menus.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System.ComponentModel;
using JunimoStudio.Core;

namespace JunimoStudio.Menus.Framework.ScrollViewers
{
    internal class BarNumbersViewer : ScrollViewer
    {
        private class BarNumberScrollContent : ScrollContentBase
        {
            /// <summary>Length of a tick.</summary>
            private readonly float _tickLength;

            private readonly ModConfig _config;

            private readonly ITimeBasedObject _timeSettings;

            /// <summary>List of bar number above each bar.</summary>
            private readonly List<ClickableTextureComponent> _barNumbers = new List<ClickableTextureComponent>();

            protected override Rectangle ScissorRectangle
                => new Rectangle((int)this.Owner.Position.X + 16, (int)this.Owner.Position.Y - 16, this.ViewportWidth, this.ViewportHeight);

            public override int ViewportWidth => this.Owner.Width - 32;

            public override int ViewportHeight => 33;

            public override int ExtentWidth => (int)(TimeLengthHelper.GetBarLength(this._timeSettings, this._tickLength) * 2) + 100;

            public override int ExtentHeight => 33 + 16;

            public BarNumberScrollContent(ScrollViewer owner, ModConfig config, ITimeBasedObject timeSettings, float tickLength)
                : base(owner)
            {
                this._config = config;
                this._timeSettings = timeSettings;
                this._tickLength = tickLength;

                this.CanHorizontallyScroll = true;
                this.CanVerticallyScroll = false;

                config.PianoRoll.PropertyChanged += this.OnConfigChanged;
                timeSettings.PropertyChanged += this.OnConfigChanged;

                this.InitLayout();

            }

            protected override void DrawNonScrollContent(SpriteBatch b)
            {
            }

            protected override void DrawScrollContent(SpriteBatch b)
            {
                foreach (ClickableTextureComponent num in this._barNumbers)
                {
                    num.draw(b);
                }
            }

            private void OnConfigChanged(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == nameof(this._timeSettings.Bpm))
                {

                }
                else if (e.PropertyName == nameof(this._timeSettings.TicksPerQuarterNote))
                {
                    this.InvalidateLayout();
                }
                else if (e.PropertyName == nameof(this._timeSettings.TimeSignature.Numerator))
                {
                    this.InvalidateLayout();
                }
                else if (e.PropertyName == nameof(this._timeSettings.TimeSignature.Denominator))
                {
                    this.InvalidateLayout();
                }
                else if (e.PropertyName == nameof(this._config.PianoRoll.Grid))
                {
                    this.InvalidateLayout();
                }
            }

            /// <summary>Invalid and redo layout.</summary>
            private void InvalidateLayout()
            {
                this._barNumbers.Clear();
                this.InitLayout();
                this.Owner.InvalidateScrollInfo();
            }

            private void InitLayout()
            {
                // init bar numbers above every bar seperator.
                for (int bar = 0; bar < 2; bar++)
                {
                    //ClickableTextureComponent num = new(
                    //    new Rectangle(
                    //        ScissorRectangle.X + 100 + TimeLengthHelper.GetBarLength(_config, _tickLength) * bar - 12,
                    //        ScissorRectangle.Y,
                    //        24, 33),
                    //    SpriteText.spriteTexture, new Rectangle(8 * (bar + 1), 18, 8, 11), 3f);
                    ClickableTextureComponent num = new(
                        new Rectangle(
                            this.ScissorRectangle.X + 100 + (int)(TimeLengthHelper.GetBarLength(this._timeSettings, this._tickLength) * bar) - 10,
                            this.ScissorRectangle.Y,
                            20, 28),
                        Game1.mouseCursors, new Rectangle(373 + 5 * bar, 56, 5, 7), 4f);

                    this._barNumbers.Add(num);
                }
            }
        }

        public BarNumbersViewer(Rectangle bounds, ModConfig config, ITimeBasedObject timeSettings, float tickLength)
            : base(bounds)
        {
            this.HorizontalScrollBarVisibility
                = this.VerticalScrollBarVisibility
                = ScrollBarVisibility.Hidden;
            this.Content = new BarNumberScrollContent(this, config, timeSettings, tickLength);
        }

        public override void Draw(SpriteBatch b)
        {
            this.DrawScrollBarAndContent(b);
        }
    }
}
