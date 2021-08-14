using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JunimoStudio.Menus.Controls
{
    /// <summary>Represents the main scrollable region inside a <see cref="ScrollViewer"/> control.</summary>
    public interface IScrollInfo
    {
        /// <summary>Gets or sets the owner <see cref="ScrollViewer"/> of this <see cref="IScrollInfo"/>.</summary>
        ScrollViewer Owner { get; set; }

        /// <summary>Gets or sets whether this <see cref="IScrollInfo"/> supports scrolling horizontally.</summary>
        bool CanHorizontallyScroll { get; set; }

        /// <summary>Gets or sets whether this <see cref="IScrollInfo"/> supports scrolling vertically.</summary>
        bool CanVerticallyScroll { get; set; }

        /// <summary>Gets the horizontal size of extent.</summary>
        int ExtentWidth { get; }

        /// <summary>Gets the vertical size of extent.</summary>
        int ExtentHeight { get; }

        /// <summary>Gets the horizontal size of the viewport for this <see cref="IScrollInfo"/>.</summary>
        int ViewportWidth { get; }

        /// <summary>Gets the vertical size of the viewport for this <see cref="IScrollInfo"/>.</summary>
        int ViewportHeight { get; }

        /// <summary>Gets the horizontal size of the offset for this scrolled <see cref="IScrollInfo"/>.</summary>
        int HorizontalOffset { get; }

        /// <summary>Gets the vertical size of the offset for this scrolled <see cref="IScrollInfo"/>.</summary>
        int VerticalOffset { get; }

        /// <summary>Sets the amount of horizontal offset.</summary>
        /// <param name="value">Horizontal offset to set.</param>
        void SetHorizontalOffset(int value);

        /// <summary>Sets the amount of vertical offset.</summary>
        /// <param name="value">Vertical offset to set.</param>
        void SetVerticalOffset(int value);

        /// <summary>Forces content to scroll until the coordinate space of a <see cref="Element"/> object is visible.</summary>
        /// <param name="element">A <see cref="Element"/> object that becomes visible.</param>
        /// <param name="rect">A bounding rectangle that identifies the coordinate space to make visible.</param>
        [Obsolete("Use MakeVisible(Vector2 point) instead")]
        void MakeVisible(Element element, Rectangle rect);

        /// <summary>Forces content to scroll until the coordinate space of a <see cref="Vector2"/> struct is visible.</summary>
        /// <param name="point">A <see cref="Vector2"/> that identifies the coordinate space to make visible.</param>
        void MakeVisible(Vector2 point);
    }
}
