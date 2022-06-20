using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace FontSettings.Framework.Menus
{
    internal class OptionsDropDown : StardewValley.Menus.OptionsDropDown, IHoverable
    {
        public int MaxDisplayRows { get; set; } = 13;

        public int RowHeight { get; set; } = 44;

        private int _lastSlotX, _lastSlotY;

        private int _topIndex = 0;

        private bool _isScrolling = false;

        private int _hoveredIndex = 0;

        private readonly ClickableTextureComponent _upArrow;

        private readonly ClickableTextureComponent _downArrow;

        private readonly ClickableTextureComponent _scrollBar;

        private readonly ClickableComponent _scrollBarRunner;

        public int OptionsCount => this.dropDownOptions?.Count ?? 0;

        public bool IsExpanded { get; set; }

        public event EventHandler SelectionChanged;

        public OptionsDropDown(string label, int x = -1, int y = -1)
            : base(label, int.MaxValue, x, y)
        {
            this._upArrow = new(new Rectangle(0, 0, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f);
            this._downArrow = new(new Rectangle(0, 0, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f);
            this._scrollBar = new(new Rectangle(0, 0, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f);
            this._scrollBarRunner = new(new Rectangle(0, 0, this._scrollBar.bounds.Width, 0), "");
        }

        public bool CanScroll(Point mousePos)
        {
            Rectangle dropDown = new(this._lastSlotX + this.dropDownBounds.X, this._lastSlotY + this.dropDownBounds.Y, this.dropDownBounds.Width, this.dropDownBounds.Height);
            Rectangle scrollBar = new(this._lastSlotX + this._scrollBarRunner.bounds.X, this._lastSlotY + this._scrollBarRunner.bounds.Y, this._scrollBarRunner.bounds.Width, this._scrollBarRunner.bounds.Height);

            return !this.greyedOut && this.IsExpanded &&
                (dropDown.Contains(mousePos)
                || scrollBar.Contains(mousePos));
        }

        public virtual void ReceiveScrollWheelAction(int direction)
        {
            if (direction > 0)
                this.ScrollUp();
            else if (direction < 0)
                this.ScrollDown();
        }

        public override void receiveLeftClick(int x, int y)
        {
            if (this.greyedOut) return;

            if (!this.IsExpanded)
                // expand.
                if (this.bounds.Contains(x, y))
                {
                    Game1.playSound("shwip");
                    this.IsExpanded = true;
                    selected = this;
                    this.PerformHoverAction(x, y);
                    this.startingSelected = this.selectedOption;

                    this.dropDownBounds.Y = this.bounds.Y + this.recentSlotY;
                    this.dropDownBounds.Height = Math.Min(this.OptionsCount, this.MaxDisplayRows) * this.bounds.Height;
                    this.dropDownBounds.Y = Math.Min(this.dropDownBounds.Y, Game1.uiViewport.Height - this.dropDownBounds.Height - this.recentSlotY);
                    this._scrollBarRunner.bounds.Height = this.dropDownBounds.Height - this._upArrow.bounds.Height - this._downArrow.bounds.Height - 8;

                    this._upArrow.setPosition(this.dropDownBounds.Right + 8, this.dropDownBounds.Y);
                    this._downArrow.setPosition(this.dropDownBounds.Right + 8, this.dropDownBounds.Bottom - 48);
                    this._scrollBar.bounds.X = this._upArrow.bounds.X + 12;
                    this._scrollBarRunner.bounds.X = this._scrollBar.bounds.X;
                    this._scrollBarRunner.bounds.Y = this._upArrow.bounds.Bottom + 4;
                    this.UpdateScrollBar();
                }
        }

        public void ReceiveGlobalLeftClick(int x, int y)
        {
            if (this.greyedOut) return;

            if (this.IsExpanded)
                // select.
                if (this.dropDownBounds.Contains(x, y))
                {
                    Game1.playSound("drumkit6");
                    this.selectedOption = this._hoveredIndex;
                    if (this.startingSelected != this.selectedOption)
                        this.RaiseSelectionChanged(EventArgs.Empty);

                    // collapse.
                    selected = null;
                    this.IsExpanded = false;
                    this.startingSelected = -1;
                }
                else if (this._upArrow.containsPoint(x, y))
                    this.ScrollUp();
                else if (this._downArrow.containsPoint(x, y))
                    this.ScrollDown();
                else if (this._scrollBarRunner.containsPoint(x, y))
                {
                    this._isScrolling = true;
                    this.leftClickHeld(x, y);
                }
                else
                {
                    // collapse.
                    selected = null;
                    this.IsExpanded = false;
                    this._hoveredIndex = this.selectedOption;
                    this.startingSelected = -1;
                }
        }

        public void PerformHoverAction(int x, int y)
        {
            if (Game1.options.SnappyMenus) return;

            if (this.IsExpanded)
                if (this.dropDownBounds.Contains(x, y))
                    this._hoveredIndex = this._topIndex + (y - this.dropDownBounds.Y) / this.RowHeight;
        }

        public override void leftClickHeld(int x, int y)
        {
            if (this.greyedOut || Game1.options.SnappyMenus) return;

            if (this.IsExpanded)
                if (this._isScrolling)
                {
                    int newTopIndex = (int)((y - this._scrollBarRunner.bounds.Y) / (float)(this._scrollBarRunner.bounds.Height - this._scrollBar.bounds.Height) * this.OptionsCount) - 1;
                    this.ScrollTo(newTopIndex);
                }
        }

        public override void leftClickReleased(int x, int y)
        {
            this._isScrolling = false;
            //base.leftClickReleased(x, y);

            //if (!this.greyedOut && this.dropDownOptions.Count > 0)
            //{
            //    if (this.dropDownBounds.Contains(x, y) || (Game1.options.gamepadControls && !Game1.lastCursorMotionWasMouse))
            //    {
            //        if (this.startingSelected != this.selectedOption)
            //            this.RaiseSelectionChanged(EventArgs.Empty);
            //    }
            //}
        }

        /// <summary>和base方法相比，仅有下面两处改动。</summary>
        public override void receiveKeyPress(Keys key)
        {
            if (!Game1.options.SnappyMenus || this.greyedOut) return;

            if (selected == null)
                if (Game1.options.doesInputListContain(Game1.options.moveRightButton, key))
                {
                    this.selectedOption++;
                    if (this.selectedOption >= this.dropDownOptions.Count)
                        this.selectedOption = 0;

                    selected = this;
                    this.RaiseSelectionChanged(EventArgs.Empty);   // 改动
                    selected = null;
                }
                else if (Game1.options.doesInputListContain(Game1.options.moveLeftButton, key))
                {
                    this.selectedOption--;
                    if (this.selectedOption < 0)
                        this.selectedOption = this.dropDownOptions.Count - 1;

                    selected = this;
                    this.RaiseSelectionChanged(EventArgs.Empty);   // 改动
                    selected = null;
                }
            else if (Game1.options.doesInputListContain(Game1.options.moveDownButton, key))
            {
                Game1.playSound("shiny4");
                this.selectedOption++;
                if (this.selectedOption >= this.dropDownOptions.Count)
                    this.selectedOption = 0;
            }
            else if (Game1.options.doesInputListContain(Game1.options.moveUpButton, key))
            {
                Game1.playSound("shiny4");
                this.selectedOption--;
                if (this.selectedOption < 0)
                    this.selectedOption = this.dropDownOptions.Count - 1;
            }
        }

        public override void draw(SpriteBatch b, int slotX, int slotY, IClickableMenu context = null)
        {
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
                Utility.drawTextWithShadow(b, displayed_text, font, new Vector2(label_start_x, label_start_y), this.greyedOut ? Game1.textColor * 0.33f : Game1.textColor, 1f, 0.1f);
            }

            this.recentSlotY = slotY;
            float alpha = this.greyedOut ? 0.33f : 1f;
            if (this.IsExpanded)
            {
                IClickableMenu.drawTextureBox(b, Game1.mouseCursors, dropDownBGSource, slotX + this.dropDownBounds.X, slotY + this.dropDownBounds.Y, this.dropDownBounds.Width, this.dropDownBounds.Height, Color.White * alpha, 4f, drawShadow: false, 0.97f);
                for (int i = this._topIndex; i < Math.Min(this._topIndex + this.MaxDisplayRows, this.OptionsCount); i++)
                {
                    if (i == this._hoveredIndex)
                        b.Draw(Game1.staminaRect, new Rectangle(slotX + this.dropDownBounds.X, slotY + this.dropDownBounds.Y + (i - this._topIndex) * this.bounds.Height, this.dropDownBounds.Width, this.bounds.Height), new Rectangle(0, 0, 1, 1), Color.Wheat, 0f, Vector2.Zero, SpriteEffects.None, 0.975f);
                    b.DrawString(Game1.smallFont, this.dropDownDisplayOptions[i], new Vector2(slotX + this.dropDownBounds.X + 4, slotY + this.dropDownBounds.Y + 8 + (i - this._topIndex) * this.bounds.Height), Game1.textColor * alpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.98f);
                }

                b.Draw(Game1.mouseCursors, new Vector2(slotX + this.bounds.X + this.bounds.Width - 48, slotY + this.bounds.Y), dropDownButtonSource, Color.Wheat * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.981f);

                IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), this._scrollBarRunner.bounds.X + slotX, this._scrollBarRunner.bounds.Y + slotY, this._scrollBarRunner.bounds.Width, this._scrollBarRunner.bounds.Height, Color.White, 4f, drawShadow: false, draw_layer: 0.982f);
                this._upArrow.Draw(b, new Vector2(slotX, slotY), Color.White, 1f);
                this._downArrow.Draw(b, new Vector2(slotX, slotY), Color.White, 1f);
                this._scrollBar.Draw(b, new Vector2(slotX, slotY), Color.White, 1f);

                this._lastSlotX = slotX;
                this._lastSlotY = slotY;
            }
            else
            {
                IClickableMenu.drawTextureBox(b, Game1.mouseCursors, dropDownBGSource, slotX + this.bounds.X, slotY + this.bounds.Y, this.bounds.Width - 48, this.bounds.Height, Color.White * alpha, 4f, drawShadow: false);
                b.DrawString(Game1.smallFont, this.selectedOption < this.dropDownDisplayOptions.Count && this.selectedOption >= 0 ? this.dropDownDisplayOptions[this.selectedOption] : "", new Vector2(slotX + this.bounds.X + 4, slotY + this.bounds.Y + 8), Game1.textColor * alpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.88f);
                b.Draw(Game1.mouseCursors, new Vector2(slotX + this.bounds.X + this.bounds.Width - 48, slotY + this.bounds.Y), dropDownButtonSource, Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
            }
        }

        public void ScrollUp()
        {
            this.ScrollTo(this._topIndex - 1);
        }

        public void ScrollDown()
        {
            this.ScrollTo(this._topIndex + 1);
        }

        public void ScrollTo(int newTopIndex)
        {
            if (this._topIndex != newTopIndex)
            {
                bool playSound = true;
                if (newTopIndex < 0)
                {
                    newTopIndex = 0;
                    playSound = false;
                }
                else if (newTopIndex > this.OptionsCount - this.MaxDisplayRows)
                {
                    newTopIndex = this.OptionsCount - this.MaxDisplayRows;
                    playSound = false;
                }

                this._topIndex = newTopIndex;
                if (playSound)
                    Game1.playSound("shiny4");
            }

            this.UpdateScrollBar();
        }

        protected virtual void RaiseSelectionChanged(EventArgs e)
        {
            SelectionChanged?.Invoke(this, e);
        }

        private void UpdateScrollBar()
        {
            this._scrollBar.bounds.Y = this._upArrow.bounds.Bottom + 4 + (int)(this._topIndex * ((double)this._scrollBarRunner.bounds.Height / this.OptionsCount));
            this._scrollBar.bounds.Y = Math.Min(this._scrollBar.bounds.Y, this._scrollBarRunner.bounds.Bottom);
        }
    }
}
