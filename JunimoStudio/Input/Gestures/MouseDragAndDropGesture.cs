using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace JunimoStudio.Input.Gestures
{
    public class MouseDragAndDropGesture : MouseGestureBase
    {
        /// <summary>Whether <see cref="Button"/> is dragging at this frame.</summary>
        private bool _dragging;

        private MouseGestureEventArgs _e;

        public override MouseButton Button { get; }

        /// <summary>Fires the first time <see cref="Button"/> is held, which is marked as the begining of this gesture cycle.</summary>
        public event EventHandler<MouseGestureEventArgs> Down;

        public event EventHandler<MouseGestureEventArgs> Dragging;

        public event EventHandler<MouseGestureEventArgs> Dropped;

        public Func<MouseGestureEventArgs, bool> CanTriggerCore { get; set; }

        public MouseDragAndDropGesture(MouseButton targetButton)
        {
            Button = targetButton;

            _lastMouseState = Mouse.GetState();
        }

        public override void Update(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();

            ButtonState lastButtonState = GetGivenButtonState(_lastMouseState, Button);
            ButtonState buttonState = GetGivenButtonState(mouseState, Button);
            bool startPoint = lastButtonState == ButtonState.Released && buttonState == ButtonState.Pressed;
            bool dragging = lastButtonState == ButtonState.Pressed && buttonState == ButtonState.Pressed;
            bool endPoint = lastButtonState == ButtonState.Pressed && buttonState == ButtonState.Released;

            if (startPoint)
            {
                MouseGestureEventArgs e = new MouseGestureEventArgs(Button, new Point(mouseState.X, mouseState.Y));
                _e = e;

                if (CanTrigger())
                    Down?.Invoke(this, e);
            }
            else if (dragging && !_updateLocked)
            {
                Dragging?.Invoke(this,
                    new MouseGestureEventArgs(Button, new Point(mouseState.X, mouseState.Y)));
            }
            else if (endPoint && !_updateLocked)
            {
                Dropped?.Invoke(this,
                    new MouseGestureEventArgs(Button, new Point(mouseState.X, mouseState.Y)));
            }

            _lastMouseState = mouseState;
        }

        protected override bool CanTrigger()
        {
            bool result = false;
            if (CanTriggerCore == null)
                result = true;
            else
                result = CanTriggerCore.Invoke(_e);

            if (result)
                RestoreUpdate();
            else
                SuppressUpdate();

            return result;
        }
    }
}
