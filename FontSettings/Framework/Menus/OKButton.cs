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
    internal class OKButton : OptionsElement
    {
        private readonly ClickableTextureComponent _innerButton;

        private int _dotsCount;

        private double _timeCounter;

        private bool _lastGreyedOut;

        public bool IsProcessing { get; private set; }

        public event EventHandler Clicked;

        public OKButton(int x, int y)
            : base(string.Empty)
        {
            this.bounds.X = x;
            this.bounds.Y = y;
            this.bounds.Width = 64;
            this.bounds.Height = 64;
            this._innerButton = new(new Rectangle(this.bounds.X, this.bounds.Y, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f);
        }

        public void SetStateProcessing()
        {
            if (this.IsProcessing) return;

            this.IsProcessing = true;
            this._lastGreyedOut = this.greyedOut;
            this.greyedOut = true;  // 禁用ok键。
        }

        public void SetStateCompleted(bool success, string message)
        {
            if (!this.IsProcessing) return;

            this.IsProcessing = false;
            this.greyedOut = this._lastGreyedOut;
            if (success)
            {
                Game1.addHUDMessage(new HUDMessage(message, null));
                Game1.playSound("money");
            }
            else
            {
                Game1.addHUDMessage(new HUDMessage(message, HUDMessage.error_type));
                Game1.playSound("cancel");
            }
        }

        public override void receiveLeftClick(int x, int y)
        {
            this._innerButton.scale -= 0.25f;
            this._innerButton.scale = Math.Max(0.75f, this._innerButton.baseScale);

            if (!this.greyedOut)
            {
                this.RaiseClicked(EventArgs.Empty);
                Game1.playSound("coin");
            }
        }

        public override void draw(SpriteBatch b, int slotX, int slotY, IClickableMenu context = null)
        {
            this._innerButton.Draw(b, new Vector2(slotX, slotY), (this.greyedOut ? 0.33f : 1f) * Color.White);

            if (this.IsProcessing)
            {
                string loadingText = I18n.OptionsOkButton_Loading();
                for (int i = 0; i < this._dotsCount; i++)
                    loadingText += '.';

                b.DrawString(Game1.dialogueFont, loadingText, new(slotX + this._innerButton.bounds.Right + IClickableMenu.borderWidth / 2, slotY + this._innerButton.bounds.Y), Game1.textColor);
            }
        }

        public void PerformHoverAction(int x, int y)
        {
            this._innerButton.tryHover(x, y);
        }

        public void Update(GameTime gameTime)
        {
            this._timeCounter -= gameTime.ElapsedGameTime.TotalSeconds;
            if (this._timeCounter <= 0)
            {
                this._timeCounter += 0.75;
                this._dotsCount++;
                if (this._dotsCount > 3)
                    this._dotsCount = 0;
            }
        }

        protected virtual void RaiseClicked(EventArgs e)
        {
            Clicked?.Invoke(this, e);
        }
    }

}
