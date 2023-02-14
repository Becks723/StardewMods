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
    internal class TitleFontButton : ClickableTextureComponent
    {
        private readonly Rectangle _hoverSourceRect;
        private readonly Rectangle _idleSourceRect;

        private readonly Action _onClicked;

        public Point Position
        {
            get => this.bounds.Location;
            set => this.bounds.Location = value;
        }

        public TitleFontButton(Point position, Action onClicked)
            : base(
                  bounds: new Rectangle(position, Textures.TitleFontButton?.Bounds.Size ?? Point.Zero),
                  texture: Textures.TitleFontButton,
                  sourceRect: Textures.TitleFontButton?.Bounds ?? Rectangle.Empty,
                  scale: 3f,
                  drawShadow: false)
        {
            this._onClicked = onClicked;

            var texture = this.sourceRect;
            this._idleSourceRect = new Rectangle(0, 0, texture.Width / 2, texture.Height);
            this._hoverSourceRect = new Rectangle(texture.Width / 2, 0, texture.Width - this._idleSourceRect.Width, texture.Height);

            this.SetSourceRect(this._idleSourceRect);
        }

        public void Update()
        {
            var mousePos = Game1.getMousePosition();
            bool leftPressed = this.MouseLeftJustPressed();

            // update hover scale.
            this.tryHover(mousePos.X, mousePos.Y, 0.25f);

            // update hover sourceRect.
            if (this.bounds.Contains(mousePos))
            {
                if (this.sourceRect == this._idleSourceRect)
                    Game1.playSound("Cowboy_Footstep");

                this.SetSourceRect(this._hoverSourceRect);
            }
            else
            {
                this.SetSourceRect(this._idleSourceRect);
            }

            // update pressed.
            if (this.bounds.Contains(mousePos))
            {
                if (leftPressed)
                {
                    Game1.playSound("newArtifact");
                    this._onClicked();
                }
            }
        }

        public void Draw(SpriteBatch b)
        {
            this.draw(b);

            Game1.activeClickableMenu?.drawMouse(b);  // TODO: 不知道为什么有时候会覆盖鼠标，因此加上这一行确保鼠标绘制。
        }

        private void SetSourceRect(Rectangle sourceRect)
        {
            this.sourceRect = sourceRect;

            this.bounds.Size = new Vector2(
                sourceRect.Width * this.baseScale,
                sourceRect.Height * this.baseScale)
                .ToPoint();
        }

        private MouseState _lastMouse;
        private bool MouseLeftJustPressed()
        {
            MouseState mouse = Game1.input.GetMouseState();
            try
            {
                return this._lastMouse.LeftButton == ButtonState.Released
                    && mouse.LeftButton == ButtonState.Pressed;
            }
            finally
            {
                this._lastMouse = mouse;
            }
        }
    }
}
