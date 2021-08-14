using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace JunimoStudio.Input.Gestures
{
    public class MouseClickGesture : MouseGestureBase
    {
        private MouseGestureEventArgs _e;

        public override MouseButton Button { get; }

        /// <summary>Gets or sets when to fire the gesture event.</summary>
        public MouseClickStyle TriggerMoment { get; set; }

        public event EventHandler<MouseGestureEventArgs> Clicked;

        public Func<MouseGestureEventArgs, bool> CanTriggerCore { get; set; }

        public MouseClickGesture(MouseButton targetButton)
        {
            Button = targetButton;

            _lastMouseState = Mouse.GetState();
        }

        public override void Update(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();
            bool onPressed = TriggerMoment == MouseClickStyle.WhenPressed;

            bool startPoint = onPressed ?
                GetGivenButtonState(_lastMouseState, Button) == ButtonState.Released
                && GetGivenButtonState(mouseState, Button) == ButtonState.Pressed
                : GetGivenButtonState(_lastMouseState, Button) == ButtonState.Pressed
                && GetGivenButtonState(mouseState, Button) == ButtonState.Released;

            if (startPoint)
            {
                MouseGestureEventArgs e = new MouseGestureEventArgs(Button, new Point(mouseState.X, mouseState.Y));
                _e = e;

                if (CanTrigger())
                    Clicked?.Invoke(this, e);
            }

            _lastMouseState = mouseState;
        }

        protected override bool CanTrigger()
        {
            if (CanTriggerCore == null)
                return true;
            return CanTriggerCore.Invoke(_e);
        }
    }
}
