using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewValley;

namespace JunimoStudio.Input.Gestures
{
    public class MouseHoldGesture : MouseGestureBase
    {
        /// <summary>The minimum milliseconds delay a mouse button is held to regard it as a hold gesture. Otherwise do not trigger.</summary>
        private const int HOLD_DELAY = 200;

        /// <summary>A timer to store hold milliseconds delay.</summary>
        private double _holdTimer;

        private readonly int[] _timer = new int[5] { 0, 0, 0, 0, 0 };
        private int _btnCaptured;

        public override MouseButton Button { get; }

        public event EventHandler Triggered;

        public MouseHoldGesture(MouseButton targetButton)
        {
            Button = targetButton;
        }

        public override void Update(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();

            ButtonState targetBtnState = GetGivenButtonState(mouseState, Button);

            if (targetBtnState == ButtonState.Pressed)
            {
                _holdTimer += gameTime.ElapsedGameTime.TotalMilliseconds;
            }

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                _timer[LEFT]++;
                if (_timer[LEFT] > 30)
                {
                    OnTriggered();
                }
            }
            else if (_lastMouseState.RightButton == ButtonState.Pressed)
            {
                _timer[RIGHT]++;
                if (_timer[RIGHT] > 30)
                {
                    OnTriggered();
                }
            }
            else if (_lastMouseState.MiddleButton == ButtonState.Pressed)
            {
                _timer[MIDDLE]++;
                if (_timer[MIDDLE] > 30)
                {
                    OnTriggered();
                }
            }
            else if (_lastMouseState.XButton1 == ButtonState.Pressed)
            {
                _timer[X1]++;
                if (_timer[X1] > 30)
                {
                    OnTriggered();
                }
            }
            else if (_lastMouseState.XButton2 == ButtonState.Pressed)
            {
                _timer[X2]++;
                if (_timer[X2] > 30)
                {
                    OnTriggered();
                }
            }

            if (mouseState.LeftButton == ButtonState.Released)
            {
                _timer[LEFT]++;
                if (_timer[LEFT] > 30)
                {
                    OnTriggered();
                }
            }
            if (_lastMouseState.RightButton == ButtonState.Released)
            {
                _timer[RIGHT]++;
                if (_timer[RIGHT] > 30)
                {
                    OnTriggered();
                }
            }
            if (_lastMouseState.MiddleButton == ButtonState.Released)
            {
                _timer[MIDDLE]++;
                if (_timer[MIDDLE] > 30)
                {
                    OnTriggered();
                }
            }
            if (_lastMouseState.XButton1 == ButtonState.Released)
            {
                _timer[X1]++;
                if (_timer[X1] > 30)
                {
                    OnTriggered();
                }
            }
            if (_lastMouseState.XButton2 == ButtonState.Released)
            {
                _timer[X2]++;
                if (_timer[X2] > 30)
                {
                    OnTriggered();
                }
            }



            _lastMouseState = mouseState;
        }

        protected virtual void OnTriggered()
        {
            Triggered?.Invoke(this, new EventArgs());
        }

        protected override bool CanTrigger()
        {
            throw new NotImplementedException();
        }
    }
}
