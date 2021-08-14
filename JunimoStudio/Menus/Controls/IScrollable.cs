using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunimoStudio.Menus.Controls
{
    public interface IScrollable
    {
        /// <summary>
        /// Scroll by a given amount.
        /// </summary>
        /// <param name="offset">A value as offset.</param>
        /// <param name="orientation">Direction of the scroll.</param>
        public void ScrollBy(float offset, Orientation orientation);

        /// <summary>
        /// Scroll to a given value.
        /// </summary>
        /// <param name="destination">A value to scroll to.</param>
        /// <param name="orientation">Direction of the scroll.</param>
        public void ScrollTo(float destination, Orientation orientation);
    }
}
