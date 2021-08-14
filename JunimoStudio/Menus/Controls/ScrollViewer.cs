using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JunimoStudio.Menus.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Menus;

namespace JunimoStudio.Menus.Controls
{
    public enum ScrollBarVisibility
    {
        Hidden,
        Auto,
        Visible
    }

    public class ScrollViewer : Container, IScrollable
    {
        private class ScrollImpl : ScrollContentBase
        {
            public Element Content { get; set; }

            protected override Rectangle ScissorRectangle =>
                new Rectangle((int)Position.X + 16, (int)Position.Y + 16, Width - 32, Height - 32);

            public override int ExtentWidth => 0;

            public override int ExtentHeight => 0;

            public ScrollImpl(ScrollViewer owner)
                : base(owner)
            {
            }

            protected override void DrawScrollContent(SpriteBatch b)
            { }

            protected override void DrawNonScrollContent(SpriteBatch b)
            {
                Content?.Draw(b);
            }
        }

        protected AdvancedScrollbar _horizontalScrollbar;

        protected AdvancedScrollbar _verticalScrollbar;

        private Vector2 _size;

        private ScrollContent _scrollContent;

        private Element _content;

        private Vector2 _oldScrollOffsetCached;

        private Vector2 _oldBoundViewerScrollOffsetCached;

        /// <summary>Update horizontal offset according to the scrollviewer bounded.</summary>
        protected Action<GameTime> _syncHorizontalOffset;

        /// <summary>Update vertical offset according to the scrollviewer bounded.</summary>
        protected Action<GameTime> _syncVerticalOffset;

        public Vector2 Size
        {
            get => _size;
            set
            {
                _size = value;
                UpdateScrollbar();
            }
        }

        public override int Width => (int)Size.X;

        public override int Height => (int)Size.Y;

        protected virtual Vector2 HorizontalScrollBarLocalPosition
            => new Vector2(_horizontalScrollbar.LocalPosition.X, Height + 48);

        protected virtual Vector2 VerticalScrollBarLocalPosition
            => new Vector2(Width + 48, _verticalScrollbar.LocalPosition.Y);

        internal AdvancedScrollbar HorizontalScrollBar => _horizontalScrollbar;

        internal AdvancedScrollbar VerticalScrollBar => _verticalScrollbar;

        internal protected ScrollContent ScrollContent
        {
            get => _scrollContent;
            set
            {
                _scrollContent = value;
                if (!object.ReferenceEquals(_scrollContent.Owner, this))
                    _scrollContent.Owner = this;
                _scrollContent.Parent = this;
                _scrollContent.LocalPosition = Vector2.Zero;
                UpdateScrollbarThumbLength();
            }
        }

        /// <summary>Gets or sets the content element inside the scrollviewer.</summary>
        public Element Content
        {
            get => _content;
            set
            {
                _content = value ?? throw new ArgumentNullException(nameof(value));
                if (value is ScrollContent scroll)
                {
                    ScrollContent = scroll;
                }
                else
                {
                    ScrollImpl scrollImpl = new ScrollImpl(this);
                    scrollImpl.Content = value;
                    ScrollContent = scrollImpl;
                }
            }
        }

        /// <summary>Occurs when scrolls.</summary>
        public event EventHandler<ScrollEventAgrs> Scroll;

        public int ExtentWidth { get; }

        public int ExtentHeight { get; }

        public int ViewportWidth { get; }

        public int ViewportHeight { get; }

        public ScrollBarVisibility HorizontalScrollBarVisibility
        {
            get => _horizontalScrollbar.Visibility;
            set => _horizontalScrollbar.Visibility = value;
        }

        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get => _verticalScrollbar.Visibility;
            set => _verticalScrollbar.Visibility = value;
        }

        public ScrollViewer()
        {
            _horizontalScrollbar = new AdvancedScrollbar { Orientation = Orientation.Horizontal, };
            _verticalScrollbar = new AdvancedScrollbar { Orientation = Orientation.Vertical, };
            AddChild(_horizontalScrollbar);
            AddChild(_verticalScrollbar);
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            UpdateScrollbar();
        }

        public ScrollViewer(Rectangle bounds)
            : this()
        {
            LocalPosition = new Vector2(bounds.X, bounds.Y);
            Size = new Vector2(bounds.Width, bounds.Height);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _verticalScrollbar.Update(gameTime);
            _horizontalScrollbar.Update(gameTime);

            //if (_scrollContent != null)
            //{
            //    _scrollContent.SetVerticalOffset(
            //        (int)((float)_verticalScrollbar.Value / (float)_verticalScrollbar.RequestLength * _scrollContent.ExtentHeight));
            //    _scrollContent.SetHorizontalOffset(
            //        (int)((float)_horizontalScrollbar.Value / (float)_horizontalScrollbar.RequestLength * _scrollContent.ExtentWidth));
            //}

            _scrollContent?.Update(gameTime);

            _syncHorizontalOffset?.Invoke(gameTime);
            _syncVerticalOffset?.Invoke(gameTime);
        }

        public override void Draw(SpriteBatch b)
        {
            IClickableMenu.drawTextureBox(b, (int)Position.X, (int)Position.Y, Width, Height, Color.White);

            DrawScrollBarAndContent(b);
        }

        /// <summary>
        /// Called by an <see cref="IScrollInfo"/> interface that is attached to a <see cref="ScrollViewer"/> when the value of any scrolling property size changes.
        /// Scrolling properties include offset, extent, or viewport.
        /// </summary>
        /// <remarks>
        /// This method handles invalidation of other elements, such as scroll bars, that depend on the scrolling properties of this <see cref="ScrollViewer"/>.
        /// </remarks>
        public void InvalidateScrollInfo()
        {
            // if scroll content's viewport greater than extent, i.e., when scrollbars are ignored, set offset to zero.
            if (_scrollContent.ViewportWidth >= _scrollContent.ExtentWidth)
                _scrollContent.SetHorizontalOffset(0);
            if (_scrollContent.ViewportHeight >= _scrollContent.ExtentHeight)
                _scrollContent.SetVerticalOffset(0);

            UpdateScrollbarThumbLength();
        }

        public virtual void OnMouseWheel(int direction)
        {
            if (_scrollContent == null)
                return;

            Point mousePos = new Point(Mouse.GetState().X, Mouse.GetState().Y);
            bool within = Bounds.Contains(mousePos) || _verticalScrollbar.Bounds.Contains(mousePos);

            if (_scrollContent.CanVerticallyScroll && within)
            {
                if (direction > 0)
                    _scrollContent.MouseWheelUp();
                if (direction < 0)
                    _scrollContent.MouseWheelDown();
            }
        }

        public virtual void BindHorizontalOffsetTo(ScrollViewer scrollViewer)
        {
            if (scrollViewer == null)
                throw new ArgumentNullException(nameof(scrollViewer));
            if (scrollViewer.ScrollContent == null)
                throw new ArgumentNullException(nameof(scrollViewer.ScrollContent));

            _syncHorizontalOffset = (gameTime) =>
            {
                int newOffsetX = _scrollContent.HorizontalOffset;
                bool thisXChanged = newOffsetX != _oldScrollOffsetCached.X;
                int newBoundViewerOffsetX = scrollViewer.ScrollContent.HorizontalOffset;
                bool boundViewerXChanged = newBoundViewerOffsetX != _oldBoundViewerScrollOffsetCached.X;

                if (thisXChanged)
                {
                    var scon = scrollViewer.ScrollContent;
                    var h = scrollViewer.HorizontalScrollBar;
                    h.ScrollTo((int)((float)newOffsetX / (float)(scon.ExtentWidth - scon.ViewportWidth) * (h.RequestLength - h.ViewportSize)));
                    scon.Update(gameTime);
                    //scrollViewer.ScrollContent.SetHorizontalOffset(newOffsetX);
                }
                else if (boundViewerXChanged)
                {
                    var scon = _scrollContent;
                    var h = _horizontalScrollbar;
                    h.ScrollTo((int)((float)newBoundViewerOffsetX / (float)(scon.ExtentWidth - scon.ViewportWidth) * (h.RequestLength - h.ViewportSize)));
                    scon.Update(gameTime);
                    //_scrollContent.SetHorizontalOffset(newBoundViewerOffsetX);
                }

                _oldScrollOffsetCached = new Vector2(_scrollContent.HorizontalOffset, _scrollContent.VerticalOffset);
                _oldBoundViewerScrollOffsetCached = new Vector2(scrollViewer.ScrollContent.HorizontalOffset, scrollViewer.ScrollContent.VerticalOffset);
            };
        }

        public virtual void BindVerticalOffsetTo(ScrollViewer scrollViewer)
        {
            if (scrollViewer == null)
                _syncVerticalOffset = null;
            if (scrollViewer.ScrollContent == null)
                throw new ArgumentNullException(nameof(scrollViewer.ScrollContent));

            _syncVerticalOffset = (gameTime) =>
            {
                int newOffsetY = _scrollContent.VerticalOffset;
                bool thisYChanged = newOffsetY != _oldScrollOffsetCached.Y;
                int newBoundViewerOffsetY = scrollViewer.ScrollContent.VerticalOffset;
                bool boundViewerYChanged = newBoundViewerOffsetY != _oldBoundViewerScrollOffsetCached.Y;

                if (thisYChanged)
                {
                    var scon = scrollViewer.ScrollContent;
                    var v = scrollViewer.VerticalScrollBar;
                    v.ScrollTo((int)((float)newOffsetY / (float)(scon.ExtentHeight - scon.ViewportHeight) * (v.RequestLength - v.ViewportSize)));
                    scon.Update(gameTime);
                    //scrollViewer.ScrollContent.SetVerticalOffset(newOffsetY);
                }

                else if (boundViewerYChanged)
                {
                    var scon = _scrollContent;
                    var v = _verticalScrollbar;
                    v.ScrollTo((int)((float)newBoundViewerOffsetY / (float)(scon.ExtentHeight - scon.ViewportHeight) * (v.RequestLength - v.ViewportSize)));
                    scon.Update(gameTime);
                    //_scrollContent.SetVerticalOffset(newBoundViewerOffsetY);
                }

                _oldScrollOffsetCached = new Vector2(_scrollContent.HorizontalOffset, _scrollContent.VerticalOffset);
                _oldBoundViewerScrollOffsetCached = new Vector2(scrollViewer.ScrollContent.HorizontalOffset, scrollViewer.ScrollContent.VerticalOffset);
            };
        }

        #region IScrollable Members

        public void ScrollBy(float offset, Orientation orientation)
        {
            if (orientation == Orientation.Horizontal)
            {
                _horizontalScrollbar.ScrollBy(offset, orientation);
            }
            else
            {
                _verticalScrollbar.ScrollBy(offset, orientation);
            }
        }

        public void ScrollTo(float destination, Orientation orientation)
        {
            if (orientation == Orientation.Horizontal)
            {
                _horizontalScrollbar.ScrollTo(destination, orientation);
            }
            else
            {
                _verticalScrollbar.ScrollTo(destination, orientation);
            }
        }

        #endregion

        protected virtual void OnScroll(ScrollEventAgrs e)
        {
            Scroll?.Invoke(this, e);
        }

        /// <summary>A draw method that handles scrollBars and scrollContent drawing, for derived class to use in convenience.</summary>
        protected void DrawScrollBarAndContent(SpriteBatch b)
        {
            if (HorizontalScrollBarVisibility == ScrollBarVisibility.Visible
                || (HorizontalScrollBarVisibility == ScrollBarVisibility.Auto && _horizontalScrollbar.ViewportSize < _horizontalScrollbar.RequestLength))
            {
                _horizontalScrollbar.Draw(b);
            }
            if (VerticalScrollBarVisibility == ScrollBarVisibility.Visible
                || (VerticalScrollBarVisibility == ScrollBarVisibility.Auto && _verticalScrollbar.ViewportSize < _verticalScrollbar.RequestLength))
            {
                _verticalScrollbar.Draw(b);
            }

            _scrollContent?.Draw(b);
        }

        /// <summary>Update scrollbars' position, size, viewportSize, etc if necessary.</summary>
        protected virtual void UpdateScrollbar()
        {
            _verticalScrollbar.LocalPosition = VerticalScrollBarLocalPosition;
            _horizontalScrollbar.LocalPosition = HorizontalScrollBarLocalPosition;
            _verticalScrollbar.RequestLength = (int)Size.Y;
            _horizontalScrollbar.RequestLength = (int)Size.X;
            UpdateScrollbarThumbLength();
        }

        /// <summary>Update length (viewportSize) of scrollbars whenever scrollviewer's size or content changed.</summary>
        protected void UpdateScrollbarThumbLength()
        {
            ScrollContent scroll = _scrollContent;
            if (scroll == null)
                return;
            _horizontalScrollbar.ViewportSize =
                (int)((double)scroll.ViewportWidth / (double)scroll.ExtentWidth * _horizontalScrollbar.RequestLength);
            _verticalScrollbar.ViewportSize =
                (int)((double)scroll.ViewportHeight / (double)scroll.ExtentHeight * _verticalScrollbar.RequestLength);
        }
    }
}
