using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace JunimoStudio.Menus.Controls
{
    /// <summary>Represents an <see cref="Element"/> that has scrolling ability.</summary>
    public abstract class ScrollContent : Element, IScrollInfo
    {
        /// <summary>Only set using this field if you don't want to fire a <see cref="Scrolled"/> event.</summary>
        protected Vector2 _offset;

        /// <summary>Gets scroll delta length of line.</summary>
        protected virtual int LineDelta => 48;

        /// <summary>Gets scroll delta length of mouse wheel.</summary>
        protected virtual int MouseWheelDelta => 60;

        /// <summary>Occurs when <see cref="HorizontalOffset"/> or <see cref="VerticalOffset"/> changed.</summary>
        public event EventHandler<ScrollEventAgrs> Scrolled;

        public override int Width => ViewportWidth;

        public override int Height => ViewportHeight;

        protected ScrollContent()
        {
            Scrolled += (s, e) =>
            {
                bool horizontallyChanged = e.HorizontalOffset != e.OldHorizontalOffset;
                bool verticallyChanged = e.VerticalOffset != e.OldVerticalOffset;

                if (horizontallyChanged)
                {
                    var h = Owner.HorizontalScrollBar;
                    h.ScrollTo(
                       (int)((float)HorizontalOffset / (float)(ExtentWidth - ViewportWidth) * (h.RequestLength - h.ViewportSize)));
                }

                if (verticallyChanged)
                {
                    var v = Owner.VerticalScrollBar;
                    v.ScrollTo(
                       (int)((float)VerticalOffset / (float)(ExtentHeight - ViewportHeight) * (v.RequestLength - v.ViewportSize)));

                }
            };
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _offset.X = (int)((float)Owner.HorizontalScrollBar.Value / (float)Owner.HorizontalScrollBar.RequestLength * ExtentWidth);
            _offset.Y = (int)((float)Owner.VerticalScrollBar.Value / (float)Owner.VerticalScrollBar.RequestLength * ExtentHeight);

            if (!CanHorizontallyScroll && _offset.X != 0)
                SetHorizontalOffset(0);
            if (!CanVerticallyScroll && _offset.Y != 0)
                SetVerticalOffset(0);
        }

        #region IScrollInfo Members

        public ScrollViewer Owner { get; set; }

        public bool CanHorizontallyScroll { get; set; } = true;

        public bool CanVerticallyScroll { get; set; } = true;

        public abstract int ExtentWidth { get; }

        public abstract int ExtentHeight { get; }

        public virtual int ViewportWidth => Owner.Width;

        public virtual int ViewportHeight => Owner.Height;

        public virtual int HorizontalOffset => (int)_offset.X;

        public virtual int VerticalOffset => (int)_offset.Y;

        [Obsolete("Use MakeVisible(Vector2 point) instead")]
        public virtual void MakeVisible(Element element, Rectangle rect)
        {
        }

        public virtual void MakeVisible(Vector2 point)
        {
            // kill unavailable parameter value.
            point = new Vector2(
                MathHelper.Clamp(point.X, 0, ExtentWidth),
                MathHelper.Clamp(point.Y, 0, ExtentHeight));

            bool needHorizontalLeft = point.X < HorizontalOffset;
            bool needsHorizontalRight = HorizontalOffset + ViewportWidth < point.X;
            bool needsHorizontalScroll = needHorizontalLeft || needsHorizontalRight;

            bool needsVerticalUp = point.Y < VerticalOffset;
            bool needsVerticalDown = VerticalOffset + ViewportHeight < point.Y;
            bool needsVerticalScroll = needsVerticalUp || needsVerticalDown;

            // point inside viewport, no scroll needed.
            if (!needsHorizontalScroll && !needsVerticalScroll)
                return;

            // handles horizontal scroll.
            if (needsHorizontalScroll)
            {
                if (needHorizontalLeft)
                    SetHorizontalOffset((int)point.X);
                else if (needsHorizontalRight)
                    SetHorizontalOffset((int)point.X - ViewportWidth);
            }

            // handles vertical scroll.
            if (needsVerticalScroll)
            {
                if (needsVerticalUp)
                    SetVerticalOffset((int)point.Y);
                else if (needsVerticalDown)
                    SetVerticalOffset((int)point.Y - ViewportHeight);
            }
        }

        public virtual void SetHorizontalOffset(int offset)
        {
            if (offset < 0 || ViewportWidth >= ExtentWidth)
            {
                offset = 0;
            }
            else
            {
                if (offset + ViewportWidth >= ExtentWidth)
                {
                    offset = ExtentWidth - ViewportWidth;
                }
            }
            int old = (int)_offset.X;
            _offset.X = offset;
            OnScrolled(new ScrollEventAgrs(offset, old, VerticalOffset, VerticalOffset));
        }

        public virtual void SetVerticalOffset(int offset)
        {
            if (offset < 0 || ViewportHeight >= ExtentHeight)
            {
                offset = 0;
            }
            else
            {
                if (offset + ViewportHeight >= ExtentHeight)
                {
                    offset = ExtentHeight - ViewportHeight;
                }
            }
            int old = (int)_offset.Y;
            _offset.Y = offset;
            OnScrolled(new ScrollEventAgrs(HorizontalOffset, HorizontalOffset, offset, old));
        }

        #endregion

        #region Scroll...
        public void MouseWheelDown()
        {
            SetVerticalOffset(VerticalOffset + MouseWheelDelta);
        }

        public void MouseWheelUp()
        {
            SetVerticalOffset(VerticalOffset - MouseWheelDelta);
        }

        public void MouseWheelLeft()
        {
            SetHorizontalOffset(HorizontalOffset - MouseWheelDelta);
        }

        public void MouseWheelRight()
        {
            SetHorizontalOffset(HorizontalOffset + MouseWheelDelta);
        }

        public void LineDown()
        {
            SetVerticalOffset(VerticalOffset - LineDelta);
        }

        public void LineUp()
        {
            SetVerticalOffset(VerticalOffset + LineDelta);
        }

        public void LineLeft()
        {
            SetHorizontalOffset(HorizontalOffset - LineDelta);
        }

        public void LineRight()
        {
            SetHorizontalOffset(HorizontalOffset + LineDelta);
        }

        #endregion

        protected virtual void OnScrolled(ScrollEventAgrs e)
        {
            Scrolled?.Invoke(this, e);
        }
    }
}
