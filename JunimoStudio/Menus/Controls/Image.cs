using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JunimoStudio.Menus.Controls
{
    internal class Image : Element
    {
        public Texture2D Texture { get; set; }
        public Rectangle? TextureRect { get; set; }
        public float Scale { get; set; } = 1;

        public Action<Element> Callback { get; set; }

        public override int Width => (int)GetActualSize().X;
        public override int Height => (int)GetActualSize().Y;
        public override string HoveredSound => Callback != null ? "shiny4" : null;

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Clicked)
                Callback?.Invoke(this);
        }

        public Vector2 GetActualSize()
        {
            if (TextureRect.HasValue)
                return new Vector2(TextureRect.Value.Width, TextureRect.Value.Height) * Scale;
            else
                return new Vector2(Texture.Width, Texture.Height) * Scale;
        }

        public override void Draw(SpriteBatch b)
        {
            b.Draw(Texture, Position, TextureRect, Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 1);
        }
    }
}
