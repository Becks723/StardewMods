using System.Collections.Generic;
using System.Linq;
using JunimoStudio.Core;
using JunimoStudio.Menus.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using _Rectangle = JunimoStudio.Menus.Controls.Shapes.Rectangle;
using JConstants = JunimoStudio.Core.Constants;

namespace JunimoStudio.Menus.Framework.ScrollViewers
{
    internal class KeyboardViewer : ScrollViewer
    {
        private class KeyboardScrollContent : ScrollContentBase
        {
            private readonly int _totalPitches = JConstants.MaxNoteNumber + 1;

            /// <summary>白键。</summary>
            private readonly List<_Rectangle> _whiteKeys = new List<_Rectangle>();

            /// <summary>黑键。</summary>
            private readonly List<_Rectangle> _blackKeys = new List<_Rectangle>();

            private readonly Dictionary<int, _Rectangle> _pitchKeyPair = new Dictionary<int, _Rectangle>();

            private readonly float _noteHeight;

            private readonly float _keyboardWidth;

            protected override Rectangle ScissorRectangle =>
                new Rectangle((int)this.Position.X + 16, (int)this.Position.Y + 16, this.ViewportWidth, this.ViewportHeight);

            public override int ViewportWidth => 100;

            public override int ViewportHeight => this.Owner.Height - 32;

            public override int ExtentWidth => 100;

            public override int ExtentHeight => (int)(this._totalPitches * this._noteHeight);

            public KeyboardScrollContent(ScrollViewer owner, float noteHeight, float keyboardWidth)
                : base(owner)
            {
                this._noteHeight = noteHeight;
                this._keyboardWidth = keyboardWidth;
                this.CanHorizontallyScroll = true;
                this.CanVerticallyScroll = true;
                this.InitPianoKeyboard_Overlay();
            }

            protected override void DrawNonScrollContent(SpriteBatch b)
            {
            }

            protected override void DrawScrollContent(SpriteBatch b)
            {
                foreach (_Rectangle key in this._whiteKeys)
                    key.Draw(b);
                foreach (_Rectangle key in this._blackKeys)
                    key.Draw(b);
            }

            /// <summary>Same size for black and white keys.</summary>
            private void InitPianoKeyboard_SameSize()
            {
                for (int n = 0; n < this._totalPitches; n++)
                {
                    _Rectangle r = new _Rectangle();
                    r.LocalPosition = new Vector2(this.ScissorRectangle.X, this.ScissorRectangle.Y + n * this._noteHeight);
                    r.Size = new Vector2(100, this._noteHeight);

                    int rem = (this._totalPitches - n) % 12;
                    if (rem == 2 || rem == 4 || rem == 7 || rem == 9 || rem == 11)
                        r.Fill = r.Stroke = Color.Black;
                    else
                    {
                        r.Fill = Color.WhiteSmoke;
                        r.Stroke = Color.SandyBrown;
                    }
                    if (rem == 1)
                    {
                        r.Fill = Color.LightGray;
                        r.Stroke = Color.LightGray;
                    }
                    this._whiteKeys.Add(r);
                }
            }

            /// <summary>Each black key overlays on the middle of two white keys, just like a piano.</summary>
            private void InitPianoKeyboard_Overlay()
            {
                float whiteKeyWidth = this._keyboardWidth;
                float blackKeyWidth = whiteKeyWidth / 1.5f;

                float nextY = 0;
                for (int n = 0; n < this._totalPitches; n++)
                {
                    _Rectangle r = new _Rectangle();

                    // 白键。
                    if (n % 12 == 0 || n % 12 == 2 || n % 12 == 4 || n % 12 == 5 || n % 12 == 7 || n % 12 == 9 || n % 12 == 11)
                    {
                        float height =
                            (n % 12 == 2 || n % 12 == 7 || n % 12 == 9)
                            ? 2f * this._noteHeight
                            : 1.5f * this._noteHeight;
                        nextY += height;

                        r.LocalPosition = new Vector2(this.ScissorRectangle.X, this.ScissorRectangle.Y + this.ExtentHeight - nextY);
                        r.Size = new Vector2(whiteKeyWidth, height);

                        r.Fill =
                            (n % 12 == 0) // C
                            ? Color.LightGray
                            : Color.WhiteSmoke;

                        this._whiteKeys.Add(r);
                    }

                    // 黑键。
                    else
                    {
                        r.LocalPosition 
                            = new Vector2(this.ScissorRectangle.X, this.ScissorRectangle.Y + this.ExtentHeight - (n + 1) * this._noteHeight);
                        r.Size = new Vector2(blackKeyWidth, this._noteHeight);
                        r.Fill = Color.Black;

                        this._blackKeys.Add(r);
                    }

                    this._pitchKeyPair.Add(this._totalPitches - n, r);
                }
            }
        }

        public KeyboardViewer(Rectangle bounds, float noteHeight, float keyboardWidth)
            : base(bounds)
        {
            this.HorizontalScrollBarVisibility = this.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            this.Content = new KeyboardScrollContent(this, noteHeight, keyboardWidth);
        }

        public override void Draw(SpriteBatch b)
        {
            this.DrawScrollBarAndContent(b);
        }
    }
}
