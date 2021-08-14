using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;

namespace JunimoStudio.Menus.Controls
{
    internal class Textbox : Element, IKeyboardSubscriber
    {
        private readonly Texture2D Tex;
        private readonly SpriteFont Font;

        public virtual string String { get; set; }

        private bool SelectedImpl;
        public bool Selected
        {
            get => SelectedImpl;
            set
            {
                if (SelectedImpl == value)
                    return;

                SelectedImpl = value;
                if (SelectedImpl)
                    Game1.keyboardDispatcher.Subscriber = this;
                else
                    if (Game1.keyboardDispatcher.Subscriber == this)
                    Game1.keyboardDispatcher.Subscriber = null;
            }
        }

        public Action<Element> Callback { get; set; }

        public Textbox()
        {
            Tex = Game1.content.Load<Texture2D>("LooseSprites\\textBox");
            Font = Game1.smallFont;
        }

        public override int Width => 192;
        public override int Height => 48;

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (ClickGestured && Callback != null)
                Selected = Hover;
        }

        public override void Draw(SpriteBatch b)
        {
            b.Draw(Tex, Position, Color.White);

            // Copied from game code - caret
            string text = String;
            Vector2 vector2;
            for (vector2 = Font.MeasureString(text); vector2.X > (double)192; vector2 = Font.MeasureString(text))
                text = text.Substring(1);
            if (DateTime.UtcNow.Millisecond % 1000 >= 500 && Selected)
                b.Draw(Game1.staminaRect, new Rectangle((int)Position.X + 16 + (int)vector2.X + 2, (int)Position.Y + 8, 4, 32), Game1.textColor);

            b.DrawString(Font, text, Position + new Vector2(16, 12), Game1.textColor);
        }

        protected virtual void ReceiveInput(string str)
        {
            String += str;
            Callback?.Invoke(this);
        }

        public void RecieveTextInput(char inputChar)
        {
            ReceiveInput(inputChar.ToString());

            // Copied from game code
            switch (inputChar)
            {
                case '"':
                    return;
                case '$':
                    Game1.playSound("money");
                    break;
                case '*':
                    Game1.playSound("hammer");
                    break;
                case '+':
                    Game1.playSound("slimeHit");
                    break;
                case '<':
                    Game1.playSound("crystal");
                    break;
                case '=':
                    Game1.playSound("coin");
                    break;
                default:
                    Game1.playSound("cowboy_monsterhit");
                    break;
            }
        }

        public void RecieveTextInput(string text)
        {
            ReceiveInput(text);
        }

        public void RecieveCommandInput(char command)
        {
            if (command == '\b' && String.Length > 0)
            {
                Game1.playSound("tinyWhip");
                String = String.Substring(0, String.Length - 1);
                Callback?.Invoke(this);
            }
        }

        public void RecieveSpecialInput(Keys key)
        {
        }
    }
}
