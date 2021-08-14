using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace JunimoStudio.Input.Gestures
{
    public class MouseDoubleClickGesture : MouseGestureBase
    {
        /// <summary>Maximum time delay in milliseconds. The second click would be regarded as a normal click if delay is greater than this value. </summary>
        public const int SECOND_CLICK_DELAY = 500;

        /// <summary>A timer for examining two clicks' time interval.</summary>
        private double _timer;

        /// <summary>Whether this gesture is first captured.</summary>
        private bool _captured;

        private bool _firstClicked;

        public override MouseButton Button { get; }

        public event EventHandler<MouseGestureEventArgs> DoubleClicked;

        public MouseDoubleClickGesture(MouseButton targetButton)
        {
            Button = targetButton;

            _lastMouseState = Mouse.GetState();
        }

        public override void Update(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();

            bool cycleStartPoint
                = GetGivenButtonState(_lastMouseState, Button) == ButtonState.Pressed
                && GetGivenButtonState(mouseState, Button) == ButtonState.Released
                && !_firstClicked;
            bool cycling = _firstClicked;
            bool secondClicked
                = GetGivenButtonState(_lastMouseState, Button) == ButtonState.Pressed
                && GetGivenButtonState(mouseState, Button) == ButtonState.Released
                && _firstClicked;

            if (cycleStartPoint)
            {
                _firstClicked = true;
                _timer = 0;
            }
            else if (cycling)
            {
                if (secondClicked
                    && _timer <= SECOND_CLICK_DELAY)
                {
                    OnDoubleClicked(new MouseGestureEventArgs(Button, new Point(mouseState.X, mouseState.Y)));
                    _firstClicked = false;
                    _timer = 0;
                    return;
                }

                _timer += gameTime.ElapsedGameTime.TotalMilliseconds;

                // out of max delay range. Reset. 
                if (_timer > SECOND_CLICK_DELAY)
                {
                    _firstClicked = false;
                    _timer = 0;
                }
            }

            _lastMouseState = mouseState;
        }

        protected virtual void OnDoubleClicked(MouseGestureEventArgs e)
        {
            DoubleClicked?.Invoke(this, e);
        }

        protected override bool CanTrigger()
        {
            throw new NotImplementedException();
        }
    }
}
