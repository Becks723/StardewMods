using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace JunimoStudio.Menus.Controls
{
    internal class Dropdown : Element
    {
        public int RequestWidth { get; set; }
        public int MaxValuesAtOnce { get; set; }
        public Texture2D Texture { get; set; } = Game1.mouseCursors;
        public Rectangle BackgroundTextureRect { get; set; } = OptionsDropDown.dropDownBGSource;
        public Rectangle ButtonTextureRect { get; set; } = OptionsDropDown.dropDownButtonSource;

        public string Value
        {
            get => Choices[ActiveChoice];
            set { if (Choices.Contains(value)) ActiveChoice = Array.IndexOf(Choices, value); }
        }
        public int ActiveChoice { get; set; }

        public int ActivePosition { get; set; }
        public string[] Choices { get; set; } = new[] { "null" };

        public bool Dropped;

        public Action<Element> Callback;

        public static Dropdown ActiveDropdown;

        public override int Width => Math.Max(300, Math.Min(500, RequestWidth));
        public override int Height => 44;
        public override string ClickedSound => "shwip";

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Clicked)
            {
                Dropped = true;
                Parent.RenderLast = this;
            }

            if (Dropped)
            {
                if (Mouse.GetState().LeftButton == ButtonState.Released)
                {
                    Game1.playSound("drumkit6");
                    Dropped = false;
                    if (Parent.RenderLast == this)
                        Parent.RenderLast = null;
                }

                var bounds2 = new Rectangle((int)Position.X, (int)Position.Y, Width, Height * MaxValuesAtOnce);
                if (bounds2.Contains(Game1.getOldMouseX(), Game1.getOldMouseY()))
                {
                    int choice = (Game1.getOldMouseY() - (int)Position.Y) / Height;
                    ActiveChoice = choice + ActivePosition;

                    Callback?.Invoke(this);
                }
            }

            if (Dropped)
                ActiveDropdown = this;
            else
                ActivePosition = Math.Min(ActiveChoice, Choices.Length - MaxValuesAtOnce);
        }

        public void ReceiveScrollWheelAction(int direction)
        {
            if (Dropped)
                ActivePosition = Math.Min(Math.Max(ActivePosition - direction / 120, 0), Choices.Length - MaxValuesAtOnce);
            else
                ActiveDropdown = null;
        }

        public void DrawOld(SpriteBatch b)
        {
            IClickableMenu.drawTextureBox(b, Texture, BackgroundTextureRect, (int)Position.X, (int)Position.Y, Width - 48, Height, Color.White, 4, false);
            b.DrawString(Game1.smallFont, Value, new Vector2(Position.X + 4, Position.Y + 8), Game1.textColor);
            b.Draw(Texture, new Vector2(Position.X + Width - 48, Position.Y), ButtonTextureRect, Color.White, 0, Vector2.Zero, 4, SpriteEffects.None, 0);

            if (Dropped)
            {
                int tall = Choices.Length * Height;
                IClickableMenu.drawTextureBox(b, Texture, BackgroundTextureRect, (int)Position.X, (int)Position.Y, Width - 48, tall, Color.White, 4, false);
                for (int i = 0; i < Choices.Length; ++i)
                {
                    if (i == ActiveChoice)
                        b.Draw(Game1.staminaRect, new Rectangle((int)Position.X + 4, (int)Position.Y + i * Height, Width - 48 - 8, Height), null, Color.Wheat, 0, Vector2.Zero, SpriteEffects.None, 0.98f);
                    b.DrawString(Game1.smallFont, Choices[i], new Vector2(Position.X + 4, Position.Y + i * Height + 8), Game1.textColor, 0, Vector2.Zero, 1, SpriteEffects.None, 1);
                }
            }
        }

        public override void Draw(SpriteBatch b)
        {
            IClickableMenu.drawTextureBox(b, Texture, BackgroundTextureRect, (int)Position.X, (int)Position.Y, Width - 48, Height, Color.White, 4, false);
            b.DrawString(Game1.smallFont, Value, new Vector2(Position.X + 4, Position.Y + 8), Game1.textColor);
            b.Draw(Texture, new Vector2(Position.X + Width - 48, Position.Y), ButtonTextureRect, Color.White, 0, Vector2.Zero, 4, SpriteEffects.None, 0);

            if (Dropped)
            {
                int maxValues = MaxValuesAtOnce;
                int start = ActivePosition;
                int end = Math.Min(Choices.Length, start + maxValues);
                int tall = Math.Min(maxValues, Choices.Length - ActivePosition) * Height;
                IClickableMenu.drawTextureBox(b, Texture, BackgroundTextureRect, (int)Position.X, (int)Position.Y, Width - 48, tall, Color.White, 4, false);
                for (int i = start; i < end; ++i)
                {
                    if (i == ActiveChoice)
                        b.Draw(Game1.staminaRect, new Rectangle((int)Position.X + 4, (int)Position.Y + (i - ActivePosition) * Height, Width - 48 - 8, Height), null, Color.Wheat, 0, Vector2.Zero, SpriteEffects.None, 0.98f);
                    b.DrawString(Game1.smallFont, Choices[i], new Vector2(Position.X + 4, Position.Y + (i - ActivePosition) * Height + 8), Game1.textColor, 0, Vector2.Zero, 1, SpriteEffects.None, 1);
                }
            }
        }
    }
}
