using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace JunimoStudio.Menus.Controls
{
    public class Button2 : Element
    {
        private readonly Texture2D _texture = Game1.mouseCursors;

        private readonly Rectangle _sourceRect = new Rectangle(432, 439, 9, 9);

        /// <summary>The extra color to render based on original texture, to highlight a hover or clicked state.</summary>
        private Color _render;

        public Action<Button2> Callback { get; set; }

        public Vector2 Size { get; set; }

        public override int Width => (int)Size.X;

        public override int Height => (int)Size.Y;

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _render = Clicked 
                ? new Color(150, 150, 150) 
                : (Hover ? Color.LightGray : Color.White);

            if (Clicked)
            {
                Callback?.Invoke(this);
            }
        }

        public override void Draw(SpriteBatch b)
        {
            IClickableMenu.drawTextureBox(b, _texture, _sourceRect, (int)Position.X, (int)Position.Y, Width, Height, _render, 4f, false);
        }
    }
}
