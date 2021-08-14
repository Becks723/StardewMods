using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunimoStudio.Menus.Controls
{
    public class ScrollEventAgrs : EventArgs
    {
        internal ScrollEventAgrs(
            //bool fire,
            int horizontalOffset,
            int oldHorizontalOffset,
            int verticalOffset,
            int oldVerticalOffset)
        {
            //Fire = fire;
            HorizontalOffset = horizontalOffset;
            OldHorizontalOffset = oldHorizontalOffset;
            VerticalOffset = verticalOffset;
            OldVerticalOffset = oldVerticalOffset;
        }

        /// <summary>Gets whether to fire the event. For internal implementation help.</summary>
        //internal bool Fire { get; }

        public int HorizontalOffset { get; }

        public int OldHorizontalOffset { get; }

        public int VerticalOffset { get; }

        public int OldVerticalOffset { get; }

    }
}
