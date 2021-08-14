using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace JunimoStudio.Menus.Controls
{
    public class ScrollBarScrollEventArgs : EventArgs
    {
        internal ScrollBarScrollEventArgs(int value, int oldValue)
        {
            Value = value;
            OldValue = oldValue;
        }

        public int Value { get; }

        public int OldValue { get; }
    }

    public class AdvancedScrollbar : Element, IScrollable
    {
        /// <summary>Field of <see cref="Visibility"/>.</summary>
        private ScrollBarVisibility _visibility;

        /// <summary>Field of <see cref="Orientation"/>.</summary>
        private Orientation _orientation;

        /// <summary>Whether thumb is being grabbed.</summary>
        private bool _grabbingThumb;

        /// <summary>
        /// When <see cref="_grabbingThumb"/> is true, 
        /// the mouse position at scroll direction (up-down in vertical bar, left-right in horizontal bar) relative to the start of bar.</summary>
        private int _mousePositionOnThumb;

        /// <summary>Whether this scrollbar instance is set to hidden.</summary>
        private bool _hidden;

        /// <summary>The bounds of scroll thumb.</summary>
        private Rectangle _thumb;

        /// <summary>The bounds of track.</summary>
        private Rectangle _track;

        /// <summary>Field of <see cref="ScrollPercent"/>.</summary>
        protected float _scrollPercent;

        /// <summary>Field of <see cref="ViewportSize"/>.</summary>
        private int _viewportSize = 40;

        /// <summary>Occurs when the value of scrollbar changed.</summary>
        public event EventHandler<ScrollBarScrollEventArgs> Scroll;

        /// <summary>Gets or sets the length of the bar.</summary>
        public int RequestLength { get; set; }

        /// <summary>Gets or sets bar orientation.</summary>
        public Orientation Orientation
        {
            get => _orientation;
            set
            {
                _orientation = value;
                UpdateThumbAndTrack();
            }
        }

        /// <summary>Gets or sets bar width.</summary>
        public override int Width => Orientation == Orientation.Vertical ? 24 : RequestLength;

        /// <summary>Gets or sets bar height.</summary>
        public override int Height => Orientation == Orientation.Vertical ? RequestLength : 24;

        /// <summary>Gets or sets visibility of the <see cref="AdvancedScrollbar"/> instance.</summary>
        public ScrollBarVisibility Visibility
        {
            get => _visibility;
            set
            {
                _visibility = value;
                switch (value)
                {
                    case ScrollBarVisibility.Hidden:
                        _hidden = true;
                        break;
                    case ScrollBarVisibility.Auto:
                        _hidden = (_viewportSize >= RequestLength);
                        break;
                    case ScrollBarVisibility.Visible:
                        _hidden = false;
                        break;
                }
            }
        }

        /// <summary>Gets or sets the current scroll offset, in percentage. Min is 0, and max is 100.</summary>
        public float ScrollPercent
        {
            get => _scrollPercent;
            protected set
            {
                if (_scrollPercent != value && value >= 0 && value <= 100)
                {
                    _scrollPercent = value;
                }
            }
        }

        /// <summary>Gets current value (offset) of the scrollbar.</summary>
        public int Value
        {
            get
            {
                return GetThumbOffset(_scrollPercent);
            }
        }

        /// <summary>Gets or sets the amount of the scrollable content that is currently visible. It also determines the length of thumb.</summary>
        public int ViewportSize
        {
            get => _viewportSize;
            set
            {
                if (_viewportSize != value && value > 0)
                {
                    _viewportSize = value;
                }
            }
        }

        public AdvancedScrollbar()
        {
        }

        /// <summary>
        /// Scroll the scrollbar from its current position by a given amount.
        /// </summary>
        /// <param name="offset"></param>
        public void ScrollBy(int offset)
        {
            int oldThumbOffset = GetThumbOffset(_scrollPercent);
            int newThumbOffset = oldThumbOffset + offset;
            ScrollPercent = GetScrollPercent(newThumbOffset);
        }

        /// <summary>
        /// Scroll the scrollbar to a given value.
        /// </summary>
        /// <param name="destination"></param>
        public void ScrollTo(int destination)
        {
            ScrollPercent = GetScrollPercent(destination);
        }

        public void ScrollBy(float offset, Orientation orientation)
        {
            if (orientation == this.Orientation)
                ScrollBy((int)offset);
        }

        public void ScrollTo(float destination, Orientation orientation)
        {
            if (orientation == this.Orientation)
                ScrollTo((int)destination);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // update thumb everytime for potential scrollbar position changes.
            UpdateThumbAndTrack();

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

            // handle mouse left click on the track would cause thumb scroll to mouse posotion.
            {
                if (Clicked && _track.Contains(mouseX, mouseY) && !_thumb.Contains(mouseX, mouseY)) // ignore if mouse down inside the thumb bounds. 
                {
                    int newThumbOffset = Orientation == Orientation.Vertical ?
                        mouseY - (int)Position.Y - _viewportSize / 2
                        : mouseX - (int)Position.X - _viewportSize / 2;
                    ScrollPercent = GetScrollPercent(newThumbOffset);

                    // manually set if newThumbOffset out of the two edges.
                    if (newThumbOffset < 0)
                        ScrollPercent = 0;
                    if (newThumbOffset > RequestLength - _viewportSize)
                        ScrollPercent = 100;

                    UpdateThumbAndTrack();
                }
            }

            // handle grab thumb.
            {
                // start of a grab thumb cycle.
                if (Clicked && _thumb.Contains(mouseX, mouseY) && !_grabbingThumb)
                {
                    _grabbingThumb = true;
                    _mousePositionOnThumb = Orientation == Orientation.Vertical ? mouseY - _thumb.Y : mouseX - _thumb.X;
                }

                // end of a grab thumb cycle.
                if (Mouse.GetState().LeftButton == ButtonState.Released && _grabbingThumb)
                {
                    _grabbingThumb = false;
                    _mousePositionOnThumb = 0;
                }

                // inside the grab thumb cycle...
                if (_grabbingThumb)
                {
                    int newThumbOffset = Orientation == Orientation.Vertical ?
                        mouseY - _mousePositionOnThumb - (int)Position.Y
                        : mouseX - _mousePositionOnThumb - (int)Position.X;
                    ScrollPercent = GetScrollPercent(newThumbOffset);

                    // avoid thumb stuck on both edges of bar when mouse is moving fast. maunally set them.
                    if (newThumbOffset <= 0)
                        ScrollPercent = 0;
                    if (newThumbOffset >= RequestLength - _viewportSize)
                        ScrollPercent = 100;

                    UpdateThumbAndTrack();
                }
            }
        }

        public override void Draw(SpriteBatch b)
        {
            // do not draw anything if _hidden.
            if (_hidden)
                return;

            // draw track.
            {
                IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6),
                    (int)Position.X, (int)Position.Y, Width, Height, Color.White, Game1.pixelZoom, false);
            }

            // draw thumb.
            {
                if (_viewportSize < RequestLength)
                {
                    bool vertical = Orientation == Orientation.Vertical;
                    float rotation = vertical ? 0f : -MathHelper.PiOver2;
                    Vector2 rotationOrigin = vertical ? Vector2.Zero : new Vector2(6, 0);

                    b.Draw(
                        Game1.mouseCursors,
                        new Vector2(_thumb.X, _thumb.Y),
                        new Rectangle(435, 463, 6, 3), Color.White, rotation, rotationOrigin, 4f, SpriteEffects.None, 0.77f);

                    for (int i = 0; i < _viewportSize / 4 - 5; i++)
                    {
                        b.Draw(
                            Game1.mouseCursors,
                            new Vector2(
                                _thumb.X + (!vertical ? (3 + i) * 4 : 0),
                                _thumb.Y + (vertical ? (3 + i) * 4 : 0)),
                            new Rectangle(435, 466, 6, 1), Color.White, rotation, rotationOrigin, 4f, SpriteEffects.None, 0.77f);

                        if (i == _viewportSize / 4 - 5 - 1) // fill the body at last draw.
                            b.Draw(
                                Game1.mouseCursors,
                                new Vector2(
                                    !vertical ? _thumb.Right - 12 : _thumb.X,
                                    vertical ? _thumb.Bottom - 12 : _thumb.Y),
                                new Rectangle(435, 466, 6, 1), Color.White, rotation, rotationOrigin, 4f, SpriteEffects.None, 0.77f);
                    }

                    b.Draw(
                        Game1.mouseCursors,
                        new Vector2(
                            !vertical ? _thumb.Right - 8 : _thumb.X,
                            vertical ? _thumb.Bottom - 8 : _thumb.Y),
                        new Rectangle(435, 471, 6, 2), Color.White, rotation, rotationOrigin, 4f, SpriteEffects.None, 0.77f);
                }
            }

            if (_grabbingThumb)
            {

            }
        }

        protected virtual void OnScroll(ScrollBarScrollEventArgs e)
        {
            Scroll?.Invoke(this, e);
        }

        /// <summary>
        /// Update thumb bounds when grabbing, orientation changed, some properties of scrollbar changed.
        /// Update track bounds when orientation changed, some properties of scrollbar changed.
        /// </summary>
        private void UpdateThumbAndTrack()
        {
            // when viewport size greater than bar length, thumb will be ignored.
            // also, if _hidden, to prevent thumb being grabbed, thumb will be empty.
            if (_hidden || _viewportSize >= RequestLength)
            {
                _thumb = Rectangle.Empty;
                return;
            }

            _track = Bounds;

            if (Orientation == Orientation.Vertical)
            {
                _thumb = new Rectangle((int)Position.X, (int)Position.Y + GetThumbOffset(_scrollPercent), 24, _viewportSize);
            }
            else
            {
                _thumb = new Rectangle((int)Position.X + GetThumbOffset(_scrollPercent), (int)Position.Y, _viewportSize, 24);
            }
        }

        /// <summary>Get the value of thumb offset relative to start of scrollbar, according to its <see cref="RequestLength"/> and <see cref="ScrollPercent"/>.</summary>
        private int GetThumbOffset(float scrollPercent)
        {
            int thumbLength = _viewportSize;
            return (int)((RequestLength - thumbLength) * (scrollPercent / 100));
        }

        private float GetScrollPercent(int thumbOffset)
        {
            int thumbLength = _viewportSize;
            return (float)thumbOffset / (float)(RequestLength - thumbLength) * 100f;
        }
    }
}
