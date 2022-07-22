using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace FontSettings.Framework.Menus
{
    /// <summary>什么时候引发slider的ValueChanged事件。（貌似只对键鼠操作生效）</summary>
    internal enum ChangeOccasion
    {
        Immediately,
        WhenMouseReleased
    }

    internal class OptionsSlider<T> : OptionsSlider
    {
        private float _mouseOffsetOnThumb;

        private float _thumbOffset;
        private T _lastValue;

        private T _minValue;
        private T _maxValue;
        private T _interval;
        private T _value;

        public T MinValue
        {
            get => this._minValue;
            set
            {
                this._minValue = value;
                this._thumbOffset = this.ValueToOffset(this.Value); // TODO: 检查新值合理性。
            }
        }

        public T MaxValue
        {
            get => this._maxValue;
            set
            {
                this._maxValue = value;
                this._thumbOffset = this.ValueToOffset(this.Value); // TODO: 检查新值合理性。
            }
        }

        public T Interval
        {
            get => this._interval;
            set
            {
                this._interval = value;
                this._thumbOffset = this.ValueToOffset(this.Value); // TODO: 检查新值合理性。
            }
        }

        public T Value
        {
            get => this._value;
            set
            {
                this._value = value;
                this._thumbOffset = this.ValueToOffset(value); // TODO: 检查新值合理性。
            }
        }

        public event EventHandler ValueChanged;

        public ChangeOccasion ValueChangeOccasion { get; set; } = ChangeOccasion.WhenMouseReleased;

        public OptionsSlider(string label, int x = -1, int y = -1)
            : base(label, int.MaxValue, x, y)
        {
        }

        public override void leftClickHeld(int x, int y)
        {
            if (!this.greyedOut)
            {
                base.leftClickHeld(x, y);

                if (this.ValueChangeOccasion is ChangeOccasion.Immediately)
                    this._lastValue = this.Value;

                float thumbX = x - this._mouseOffsetOnThumb;
                if (thumbX < this.bounds.X)
                    this.Value = this.MinValue;
                else if (thumbX > this.bounds.Right - 40)
                    this.Value = this.MaxValue;
                else
                {
                    this.Value = this.Value switch
                    {
                        int => (T)(object)(int)((int)(object)this.MinValue + (thumbX - this.bounds.X) / (this.bounds.Width - 40) * ((int)(object)this.MaxValue - (int)(object)this.MinValue)),
                        float => (T)(object)((float)(object)this.MinValue + (thumbX - this.bounds.X) / (this.bounds.Width - 40) * ((float)(object)this.MaxValue - (float)(object)this.MinValue)),
                        _ => this.Value
                    };

                    // interval
                    this.Value = this.Value switch
                    {
                        int i => (T)(object)(i - i % (int)(object)this.Interval),
                        float f => (T)(object)(f - f % (float)(object)this.Interval),
                        _ => this.Value
                    };
                }

                this._thumbOffset = this.ValueToOffset(this.Value);

                if (!this._lastValue.Equals(this.Value) && this.ValueChangeOccasion is ChangeOccasion.Immediately)
                    ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public override void receiveLeftClick(int x, int y)
        {
            Vector2 thumbPos = this.GetThumbPosition(0, 0);
            Rectangle thumbBounds = new((int)thumbPos.X, (int)thumbPos.Y, sliderButtonRect.Width * 4, sliderButtonRect.Height * 4);
            if (thumbBounds.Contains(x, y))
                this._mouseOffsetOnThumb = x - thumbPos.X;
            else
            {
                this._thumbOffset = x - thumbBounds.Width / 2;
                this._mouseOffsetOnThumb = thumbBounds.Width / 2;
            }
            this._lastValue = this.Value;
            base.receiveLeftClick(x, y);
        }

        public override void leftClickReleased(int x, int y)
        {
            if (!this._lastValue.Equals(this.Value) && this.ValueChangeOccasion is ChangeOccasion.WhenMouseReleased)
                ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        public override void draw(SpriteBatch b, int slotX, int slotY, IClickableMenu context = null)
        {
            float alpha = this.greyedOut ? 0.33f : 1f;

            // base.base.draw()
            {
                int label_start_x = slotX + this.bounds.X + this.bounds.Width + 8 + (int)this.labelOffset.X;
                int label_start_y = slotY + this.bounds.Y + (int)this.labelOffset.Y;
                string displayed_text = this.label;
                SpriteFont font = Game1.dialogueFont;
                if (context != null)
                {
                    int max_width = context.width - 64;
                    int menu_start_x = context.xPositionOnScreen;
                    if (font.MeasureString(this.label).X + label_start_x > max_width + menu_start_x)
                    {
                        int allowed_space = max_width + menu_start_x - label_start_x;
                        font = Game1.smallFont;
                        displayed_text = Game1.parseText(this.label, font, allowed_space);
                        label_start_y -= (int)((font.MeasureString(displayed_text).Y - font.MeasureString("T").Y) / 2f);
                    }
                }
                Utility.drawTextWithShadow(b, displayed_text, font, new Vector2(label_start_x, label_start_y), Game1.textColor * alpha, 1f, 0.1f);
            }

            // slider.
            {
                // track.
                IClickableMenu.drawTextureBox(b, Game1.mouseCursors, sliderBGSource, slotX + this.bounds.X, slotY + this.bounds.Y, this.bounds.Width, this.bounds.Height, Color.White * alpha, 4f, drawShadow: false);

                Vector2 position = new(slotX + this.bounds.X + this._thumbOffset, slotY + this.bounds.Y);

                // thumb.
                b.Draw(Game1.mouseCursors, position, sliderButtonRect, Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);

                // display value.
                b.DrawString(Game1.dialogueFont, this.Value?.ToString() ?? string.Empty, new(position.X, position.Y + this.bounds.Height + 4), Game1.textColor * alpha);
            }
        }

        private Vector2 GetThumbPosition(int slotX, int slotY)
        {
            float percent = this.Value switch
            {
                int i => ((float)i - (int)(object)this.MinValue) / ((int)(object)this.MaxValue - (int)(object)this.MinValue),
                float f => (f - (float)(object)this.MinValue) / ((float)(object)this.MaxValue - (float)(object)this.MinValue),
                _ => 0f
            };
            return new Vector2(slotX + this.bounds.X + (this.bounds.Width - 40) * percent, slotY + this.bounds.Y);
        }

        private float ValueToOffset(T value)
        {
            float percent = value switch
            {
                int i => ((float)i - (int)(object)this.MinValue) / ((int)(object)this.MaxValue - (int)(object)this.MinValue),
                float f => (f - (float)(object)this.MinValue) / ((float)(object)this.MaxValue - (float)(object)this.MinValue),
                _ => 0f
            };
            return (this.bounds.Width - 40) * percent;
        }
    }
}
