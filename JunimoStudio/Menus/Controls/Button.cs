using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace JunimoStudio.Menus.Controls
{
    public class Button : Element
    {
        public Texture2D Texture { get; set; }
        public Rectangle IdleTextureRect { get; set; }
        public Rectangle HoverTextureRect { get; set; }

        public Action<Element> Callback { get; set; }

        private float Scale = 1f;

        public Button(Texture2D tex)
        {
            Texture = tex;
            IdleTextureRect = new Rectangle(0, 0, tex.Width / 2, tex.Height);
            HoverTextureRect = new Rectangle(tex.Width / 2, 0, tex.Width / 2, tex.Height);
        }

        public override int Width => IdleTextureRect.Width;
        public override int Height => IdleTextureRect.Height;
        public override string HoveredSound => "Cowboy_Footstep";

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Scale = Hover ? Math.Min(Scale + 0.013f, 1.083f) : Math.Max(Scale - 0.013f, 1f);

            if (Clicked)
                Callback?.Invoke(this);
        }

        public override void Draw(SpriteBatch b)
        {
            Vector2 origin = new Vector2(Texture.Width / 4f, Texture.Height / 2f);
            b.Draw(Texture, Position + origin, Hover ? HoverTextureRect : IdleTextureRect, Color.White, 0f, origin, Scale, SpriteEffects.None, 0f);
            Game1.activeClickableMenu?.drawMouse(b);
        }
    }
}
