using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace JunimoStudio.Menus.Controls
{
    internal class Slider : Element
    {
        public int RequestWidth { get; set; }

        public Action<Element> Callback { get; set; }

        protected bool Dragging;

        public override int Width => RequestWidth;
        public override int Height => 24;

        public override void Draw(SpriteBatch b) { }
    }

    internal class Slider<T> : Slider
    {
        public T Minimum { get; set; }
        public T Maximum { get; set; }
        public T Value { get; set; }

        public T Interval { get; set; }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Clicked)
                Dragging = true;
            if (Mouse.GetState().LeftButton == ButtonState.Released)
                Dragging = false;

            if (Dragging)
            {
                float perc = (Game1.getOldMouseX() - Position.X) / Width;
                Value = Value switch {
                    int => Util.Clamp<T>(Minimum, (T)(object)(int)(perc * ((int)(object)Maximum - (int)(object)Minimum) + (int)(object)Minimum), Maximum),
                    float => Util.Clamp<T>(Minimum, (T)(object)(perc * ((float)(object)Maximum - (float)(object)Minimum) + (float)(object)Minimum), Maximum),
                    _ => Value
                };

                Value = Util.Adjust(Value, Interval);

                Callback?.Invoke(this);
            }
        }

        public override void Draw(SpriteBatch b)
        {
            float perc = Value switch {
                int => ((int)(object)Value - (int)(object)Minimum) / (float)((int)(object)Maximum - (int)(object)Minimum),
                float => ((float)(object)Value - (float)(object)Minimum) / ((float)(object)Maximum - (float)(object)Minimum),
                _ => 0
            };

            Rectangle back = new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
            Rectangle front = new Rectangle((int)(Position.X + perc * (Width - 40)), (int)Position.Y, 40, Height);

            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), back.X, back.Y, back.Width, back.Height, Color.White, Game1.pixelZoom, false);
            b.Draw(Game1.mouseCursors, new Vector2(front.X, front.Y), new Rectangle(420, 441, 10, 6), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
        }
    }
}
