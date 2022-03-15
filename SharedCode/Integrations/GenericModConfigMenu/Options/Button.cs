using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace CodeShared.Integrations.GenericModConfigMenu.Options
{
    public class Button
    {
        private readonly Texture2D _texture = Game1.mouseCursors;

        private readonly Rectangle _sourceRect = new Rectangle(432, 439, 9, 9);

        /// <summary>The extra color to render based on original texture, to highlight a hover or clicked state.</summary>
        private Color _render;

        public EventHandler Callback { get; set; }

        public Vector2 LocalPosition { get; set; }

        public Vector2 Position => this.LocalPosition;

        public Vector2 Size { get; set; }

        public int Width => (int)this.Size.X;

        public int Height => (int)this.Size.Y;

        public Rectangle Bounds => new Rectangle((int)this.Position.X, (int)this.Position.Y, this.Width, this.Height);

        public bool Hover { get; set; }

        public bool ClickGestured { get; set; }

        public bool Clicked { get; private set; }

        private bool _holding;

        private MouseState _lastState;
        public void Update(GameTime gameTime)
        {
            int mouseX;
            int mouseY;
            if (Constants.TargetPlatform == GamePlatform.Android)
            {
                mouseX = Game1.getMouseX();
                mouseY = Game1.getMouseY();
            }
            else
            {
                mouseX = Game1.getOldMouseX();
                mouseY = Game1.getOldMouseY();
            }

            var mouseState = Game1.input.GetMouseState();

            this.Hover = this.Bounds.Contains(mouseX, mouseY);
            if (!this._holding)
            {
                if (this.Hover && mouseState.LeftButton is ButtonState.Pressed && this._lastState.LeftButton is ButtonState.Released)
                    this._holding = true;
            }
            else
            {
                this._holding = mouseState.LeftButton is ButtonState.Pressed;
            }

            this.Clicked = this.Hover && mouseState.LeftButton is ButtonState.Released && this._lastState.LeftButton is ButtonState.Pressed;

            this._render = this._holding
                ? new Color(150, 150, 150)
                : (this.Hover ? Color.LightGray : Color.White);

            if (this.Clicked)
            {
                this.RaiseCallback(EventArgs.Empty);
            }

            this._lastState = mouseState;
        }

        public void Draw(SpriteBatch b)
        {
            IClickableMenu.drawTextureBox(b, this._texture, this._sourceRect, (int)this.Position.X, (int)this.Position.Y, this.Width, this.Height, this._render, 4f, false);
        }

        protected virtual void RaiseCallback(EventArgs e)
        {
            this.Callback?.Invoke(this, e);
        }
    }
}
