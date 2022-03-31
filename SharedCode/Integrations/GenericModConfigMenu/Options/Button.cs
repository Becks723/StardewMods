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
        /// <summary>The extra color to render based on original texture, to highlight a hover or clicked state.</summary>
        private Color _render;

        public event EventHandler Callback;

        public Vector2 LocalPosition { get; set; }

        public Vector2 Position => this.LocalPosition;

        public Vector2 Size { get; set; }

        /// <summary>Gets or sets the content to display on this button.</summary>
        /// <remarks>Current supports: <see cref="string"/>, <see cref="Texture2D"/>. Other type would be converted to string.</remarks>
        public object Content { get; set; }

        /// <summary>Gets or sets a source rectangle when <see cref="Content"/> is a <see cref="Texture2D"/>.</summary>
        public Rectangle? SourceRectangle { get; set; }

        public SpriteFont Font { get; set; } = Game1.dialogueFont;

        public float Scale { get; set; } = 1f;

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
            Color color = this._render;
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(432, 439, 9, 9), (int)this.Position.X, (int)this.Position.Y, this.Width, this.Height, color, 4f, false);

            float scale = this.Scale;
            var font = this.Font;
            string text = string.Empty;
            Vector2 drawOrigin;
            bool isText;
            switch (this.Content)
            {
                case Texture2D tex:
                    isText = false;
                    Rectangle srcRect = this.SourceRectangle ?? tex.Bounds;
                    drawOrigin = new Vector2(this.Bounds.Center.X - srcRect.Width / 2 * scale, this.Bounds.Center.Y - srcRect.Height / 2 * scale);
                    b.Draw(tex, drawOrigin, srcRect, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                    break;

                case string @string:
                    isText = true;
                    text = @string;
                    break;

                default:
                    isText = true;
                    text = this.Content?.ToString() ?? string.Empty;
                    break;
            }

            if (isText)
            {
                var strSize = font.MeasureString(text) * scale;
                drawOrigin = new Vector2(this.Bounds.Center.X - strSize.X / 2, this.Bounds.Center.Y - strSize.Y / 2);
                b.DrawString(font, text, drawOrigin, Game1.textColor, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                //Utility.drawTextWithShadow(b, text, font, drawOrigin, Game1.textColor, scale); // 中文乱码
            }
        }

        protected virtual void RaiseCallback(EventArgs e)
        {
            this.Callback?.Invoke(this, e);
        }
    }
}