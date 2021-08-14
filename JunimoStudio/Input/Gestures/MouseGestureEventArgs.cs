using System;
using Microsoft.Xna.Framework;

namespace JunimoStudio.Input.Gestures
{
    public class MouseGestureEventArgs : EventArgs
    {
        public MouseButton TargetButton { get; }

        /// <summary>Gets mouse position when the associated event is fired.</summary>
        public Point Position { get; }

        public MouseGestureEventArgs(MouseButton targetButton, Point position)
        {
            TargetButton = targetButton;
            Position = position;
        }
    }
}
