using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace FontSettings.Framework.Menus
{
    internal class OptionsSelector : OptionsElement, IHoverable
    {
        private readonly ClickableTextureComponent _leftArrow;

        private readonly ClickableTextureComponent _rightArrow;

        private readonly Func<object, object> _getPrevious;
        private readonly Func<object, object> _getNext;
        private object _selectedChoice;

        public IEnumerable Choices { get; set; }

        public virtual Func<object, string> DisplayChoiceParser { get; set; }

        public virtual object SelectedChoice
        {
            get => this._selectedChoice;
            set
            {
                if (this._selectedChoice != value)
                {
                    this._selectedChoice = value;
                    this.RaiseSelectionChanged(EventArgs.Empty);
                }
            }
        }

        public event EventHandler SelectionChanged;

        public OptionsSelector(Func<object, object> getPrevious, Func<object, object> getNext, object defaultSelectedChoice)
            : base(string.Empty)
        {
            this._getPrevious = getPrevious;
            this._getNext = getNext;
            this.SelectedChoice = defaultSelectedChoice;

            this._leftArrow = new(new Rectangle(this.bounds.X, this.bounds.Y, 48, 44), Game1.mouseCursors, new(352, 495, 12, 11), 4f);
            this._rightArrow = new(new Rectangle(this.bounds.X, this.bounds.Y, 48, 44), Game1.mouseCursors, new(365, 495, 12, 11), 4f);
        }

        public override void receiveLeftClick(int x, int y)
        {
            base.receiveLeftClick(x, y);

            if (!this.greyedOut)
                if (this._leftArrow.containsPoint(x, y))
                {
                    this.SelectedChoice = this._getPrevious(this.SelectedChoice);
                    Game1.playSound("smallSelect");
                }
                else if (this._rightArrow.containsPoint(x, y))
                {
                    this.SelectedChoice = this._getNext(this.SelectedChoice);
                    Game1.playSound("smallSelect");
                }
        }

        public void PerformHoverAction(int x, int y)
        {
            if (!this.greyedOut)
            {
                this._leftArrow.tryHover(x, y);
                this._rightArrow.tryHover(x, y);
            }
        }

        public override void draw(SpriteBatch b, int slotX, int slotY, IClickableMenu context = null)
        {
            string currentChoice = this.DisplayChoiceParser != null ? this.DisplayChoiceParser(this.SelectedChoice) : this.SelectedChoice?.ToString();
            currentChoice ??= string.Empty;

            Vector2 size = this.MeasureString(currentChoice);

            if (context is OptionsPage optionsPage)
                this._rightArrow.bounds.X = optionsPage.optionSlots[0].bounds.Width - 28 - this._rightArrow.bounds.Width;
            else
                this._rightArrow.bounds.X = context.xPositionOnScreen + context.width - (this._leftArrow.bounds.X + slotX - context.xPositionOnScreen) - slotX;

            this.bounds.Width = this._rightArrow.bounds.Right - this._leftArrow.bounds.Left;
            this.bounds.Height = Math.Max(this._leftArrow.bounds.Height, (int)size.Y);

            this._leftArrow.Draw(b, new Vector2(slotX, slotY));
            this._rightArrow.Draw(b, new Vector2(slotX, slotY));
            this.DrawString(b, currentChoice, new Vector2(slotX + this._leftArrow.bounds.Right + (this._rightArrow.bounds.Left - this._leftArrow.bounds.Right) / 2 - size.X / 2, slotY + (int)(this._leftArrow.bounds.Center.Y - size.Y / 2)), Game1.textColor);
        }

        protected virtual Vector2 MeasureString(string text)
        {
            return Game1.smallFont.MeasureString(text);
        }

        protected virtual void DrawString(SpriteBatch b, string text, Vector2 position, Color color)
        {
            b.DrawString(Game1.smallFont, text, position, color);
        }

        protected virtual void RaiseSelectionChanged(EventArgs e)
        {
            SelectionChanged?.Invoke(this, e);
        }
    }

    internal class OptionsSelector<T> : OptionsSelector
    {
        public new T SelectedChoice
        {
            get => (T)base.SelectedChoice;
            set => base.SelectedChoice = value;
        }

        public new Func<T, string> DisplayChoiceParser
        {
            get
            {
                if (base.DisplayChoiceParser is null)
                    return null;
                else
                    return (choice) => base.DisplayChoiceParser(choice);
            }
            set
            {
                if (value is null)
                    base.DisplayChoiceParser = null;
                else
                    base.DisplayChoiceParser = (choice) => value((T)choice);
            }
        }

        public OptionsSelector(Func<T, T> getPrevious, Func<T, T> getNext, T defaultSelectedChoice)
            : base(
                  (cur) => getPrevious((T)cur),
                  (cur) => getNext((T)cur),
                  defaultSelectedChoice)
        {
        }
    }
}
