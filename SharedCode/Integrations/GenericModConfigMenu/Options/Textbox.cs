using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;

namespace CodeShared.Integrations.GenericModConfigMenu.Options
{
    internal class Textbox : IKeyboardSubscriber
    {
        private class Header
        {
            private readonly Textbox _textbox;

            private int StrLength => this._textbox.String.Length;

            public int Value { get; private set; }

            public event EventHandler Changed;

            public Header(Textbox textbox)
            {
                this._textbox = textbox;
            }

            /// <summary>Sets header with a new value.</summary>
            public void SetHeader(int index)  // index of a character, the header is then set ahead of the character.
            {
                if (index < 0 || index > this.StrLength)
                    return;

                this.Value = index;

                Changed?.Invoke(this, new EventArgs());
            }

            /// <summary>Sets header with a delta value.</summary>
            public void SetHeaderDelta(int deltaIndex)
            {
                this.SetHeader(this.Value + deltaIndex);
            }

            public void ForwardOne()
            {
                this.SetHeaderDelta(1);
            }

            public void BackOne()
            {
                this.SetHeaderDelta(-1);
            }
        }

        private readonly Texture2D _texture;
        private readonly SpriteFont _font;
        private readonly Header _header;
        private string _string;

        /// <summary>Max visible length of text in the textbox.</summary>
        private float TextboxLength => this.Width - 32;

        /// <summary>Actual length of String.</summary>
        private float TextLength => this._font.MeasureString(this.String).X;

        /// <summary>X-coord offset from left edge.</summary>
        private float _offset;

        private bool _isMovingHeader;

        /// <summary>Caret distance to the visible textbox left edge.</summary>
        private float CaretLength
        {
            get
            {
                return this._font.MeasureString(this.String.Substring(0, this._header.Value)).X;
            }
        }

        private int StrLength => this.String.Length;

        public virtual string String
        {
            get => this._string ?? string.Empty;
            set
            {
                this._string = value;
                this._header.SetHeader(this.String.Length);
            }
        }

        private bool _selected;
        public bool Selected
        {
            get => this._selected;
            set
            {
                if (this._selected != value)
                {
                    this._selected = value;
                    if (this._selected)
                        Game1.keyboardDispatcher.Subscriber = this;
                    else
                        if (Game1.keyboardDispatcher.Subscriber == this)
                        Game1.keyboardDispatcher.Subscriber = null;
                }
            }
        }

        public event EventHandler Callback;


        public Vector2 LocalPosition { get; set; }

        public Vector2 Position => this.LocalPosition;

        public int Width { get; set; } = 192;

        public int Height => 48;

        public Rectangle Bounds => new Rectangle((int)this.Position.X, (int)this.Position.Y, this.Width, this.Height);

        public bool Hover { get; set; }

        public bool ClickGestured { get; set; }

        public bool Clicked => this.Hover && this.ClickGestured;

        public Textbox()
        {
            this._texture = Game1.content.Load<Texture2D>("LooseSprites\\textBox");
            this._font = Game1.smallFont;
            this._header = new Header(this);
            this._header.Changed += this.OnHeaderChanged;
        }


        private MouseState _lastState;
        private KeyboardState _lastKBState;

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

            bool newHover = this.Bounds.Contains(mouseX, mouseY);
            this.Hover = newHover;

            var mouseState = Game1.input.GetMouseState();
            bool leftClicked = mouseState.LeftButton is ButtonState.Pressed && this._lastState.LeftButton is ButtonState.Released;
            this.ClickGestured = leftClicked;

            if (this.ClickGestured)
            {
                if (this.Hover)
                {
                    this.Selected = true;

                    int index = this.GetIndex(this._offset + mouseX - 14 - this.Position.X);
                    this._header.SetHeader(index);
                }
                else
                {
                    this.Selected = false;
                }
            }

            var kbState = Game1.input.GetKeyboardState();

            if (kbState.IsKeyUp(Keys.Left) && kbState.IsKeyUp(Keys.Right))
            {
                this._isMovingHeader = false;
            }

            this._lastState = mouseState;
            this._lastKBState = kbState;
        }

        public void Draw(SpriteBatch b)
        {
            this.DrawTextbox(b);

            // Copied from game code - caret
            string str = this.String ?? string.Empty;

            b.InNewScissoredState(
                new Rectangle(this.Bounds.X + 14, this.Bounds.Y, this.Bounds.Width - 28, this.Bounds.Height),
                new Vector2(this._offset, 0),
                () =>
                {
                    // draw caret.
                    if ((DateTime.UtcNow.Millisecond % 1000 >= 500 && this.Selected) || this._isMovingHeader)
                    {
                        b.Draw(Game1.staminaRect, new Rectangle((int)this.Position.X + 14 + (int)this.CaretLength, (int)this.Position.Y + 8, 4, 32), Game1.textColor);
                    }

                    b.DrawString(this._font, str, this.Position + new Vector2(16, 12), Game1.textColor);
                }
            );
        }

        private void DrawTextbox(SpriteBatch b)
        {
            b.Draw(this._texture, this.Position, new Rectangle(0, 0, 16, this._texture.Height), Color.White);
            for (int i = 0; i < this.Width - 16 - 12; i++)
            {
                b.Draw(this._texture, this.Position + new Vector2(16 + i, 0), new Rectangle(16, 0, 1, this._texture.Height), Color.White);
            }
            b.Draw(this._texture, new Vector2(this.Bounds.Right - 12, this.Bounds.Y), new Rectangle(this._texture.Width - 12, 0, 12, this._texture.Height), Color.White);
        }

        /// <summary>Get the index of the char at a given offset.</summary>
        /// <param name="offset">Offset to left edge of text.</param>
        private int GetIndex(float offset)
        {
            string text = this.String;

            offset = Math.Clamp(offset, 0, this.TextLength);
            if (offset == 0)
                return 0;
            else if (offset == this.TextLength)
                return text.Length;

            int off = (int)offset;
            int loIndex = 0;
            int hiIndex = text.Length;
            int midIndex;

            while (loIndex <= hiIndex)
            {
                midIndex = (int)Math.Ceiling((loIndex + hiIndex) / 2.0);
                int midLength = (int)this._font.MeasureString(text.Substring(0, midIndex + 1)).X;

                if (midLength == off)
                {
                    // 成功
                    return midIndex;
                }
                else if (midLength < off)
                {
                    // 取右半段
                    if (loIndex == midIndex)
                        return midIndex;
                    loIndex = midIndex;
                }
                else
                {
                    // 取左半段
                    if (hiIndex == midIndex)
                        return midIndex;
                    hiIndex = midIndex;
                }
            }

            throw new InvalidOperationException();
        }

        protected virtual void ReceiveInput(string str)
        {
            this._string = new StringBuilder(this.StrLength + str.Length)
                .Append(this.String.Substring(0, this._header.Value))
                .Append(str)
                .Append(this.String.Substring(this._header.Value))
                .ToString();
            this._header.SetHeaderDelta(str.Length);

            this.RaiseCallback(EventArgs.Empty);
        }

        public void RecieveTextInput(char inputChar)
        {
            this.ReceiveInput(inputChar.ToString());

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
            this.ReceiveInput(text);
        }

        public void RecieveCommandInput(char command)
        {
            switch (command)
            {
                case '\b':
                    this.Backspace();
                    break;
            }
        }

        public void RecieveSpecialInput(Keys key)
        {
            switch (key)
            {
                case Keys.Left:
                    this._header.BackOne();
                    this._isMovingHeader = true;
                    break;

                case Keys.Right:
                    this._header.ForwardOne();
                    this._isMovingHeader = true;
                    break;

                case Keys.Delete:
                    this.Delete();
                    break;
            }
        }

        public void Backspace()
        {
            if (this.CanBackspace())
            {
                this._string = new StringBuilder(this.StrLength - 1)
                    .Append(this.String.Substring(0, this._header.Value - 1))
                    .Append(this.String.Substring(this._header.Value))
                    .ToString();
                this._header.BackOne();
                Game1.playSound("tinyWhip");
                this.RaiseCallback(EventArgs.Empty);
            }
        }

        public void Delete()
        {
            int header = this._header.Value;
            if (header < this.StrLength)
            {
                this._string = this.String.Remove(header, 1);
                Game1.playSound("tinyWhip");
                this.RaiseCallback(EventArgs.Empty);
            }
        }

        protected virtual void RaiseCallback(EventArgs e)
        {
            this.OnCallback();
            this.Callback?.Invoke(this, e);
        }

        protected virtual void OnCallback()
        {

        }

        private void OnHeaderChanged(object sender, EventArgs e)
        {
            int header = (sender as Header).Value;
            string beforeHeader = this.String.Substring(0, header);
            float beforeHeaderLength = this._font.MeasureString(beforeHeader).X;

            if (beforeHeaderLength < this._offset)
            {
                this._offset = beforeHeaderLength;
            }
            else if (beforeHeaderLength > this._offset + this.TextboxLength)
            {
                this._offset = beforeHeaderLength - this.TextboxLength;
            }
        }

        private bool CanBackspace()
        {
            return this.StrLength > 0 && this._header.Value > 0;
        }

        private string TailorString(float length, string value, bool takeHeadOrTail, out int rmChars)
        {
            rmChars = 0;
            float curLength;
            while ((curLength = this._font.MeasureString(value).X) > length)
            {
                value = takeHeadOrTail
                    ? value.Substring(0, value.Length - 1)
                    : value.Substring(1);
                rmChars++;
            }

            return value;
        }

        private string TailorString(float length, string value, bool takeHeadOrTail, out int startIndex, out int strLength)
        {
            string result = value;
            float curLength;
            while ((curLength = this._font.MeasureString(value).X) > length)
            {
                result = takeHeadOrTail
                    ? result.Substring(0, value.Length - 1)
                    : result.Substring(1);
            }

            startIndex = takeHeadOrTail
                ? value.IndexOf(result)
                : value.LastIndexOf(result);
            strLength = result.Length;

            return result;
        }
    }
}
