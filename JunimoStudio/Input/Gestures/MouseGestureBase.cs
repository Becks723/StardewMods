using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace JunimoStudio.Input.Gestures
{
    public abstract class MouseGestureBase
    {
        #region Mouse Button IDs
        protected const int LEFT = 0;
        protected const int MIDDLE = 1;
        protected const int RIGHT = 2;
        protected const int X1 = 3;
        protected const int X2 = 4;
        #endregion

        /// <summary>Cached mouse state since last update.</summary>
        protected MouseState _lastMouseState;

        protected bool _updateLocked;

        /// <summary>The mouse button whose gesture is catched.</summary>
        public abstract MouseButton Button { get; }

        /// <summary>Update frame to check for and trigger gesture.</summary>
        public abstract void Update(GameTime gameTime);

        public void SuppressUpdate()
        {
            if (!_updateLocked)
                _updateLocked = true;
        }

        public void RestoreUpdate()
        {
            if (_updateLocked)
                _updateLocked = false;
        }

        protected abstract bool CanTrigger();

        protected ButtonState GetGivenButtonState(MouseState mouseState, MouseButton button)
        {
            return button switch {
                MouseButton.Left => mouseState.LeftButton,
                MouseButton.Middle => mouseState.MiddleButton,
                MouseButton.Right => mouseState.RightButton,
                MouseButton.XButton1 => mouseState.XButton1,
                MouseButton.XButton2 => mouseState.XButton2,
                _ => mouseState.LeftButton,
            };
        }

        protected MouseButton ButtonById(int id)
        {
            return id switch {
                0 => MouseButton.Left,
                1 => MouseButton.Middle,
                2 => MouseButton.Right,
                3 => MouseButton.XButton1,
                4 => MouseButton.XButton2,
                _ => MouseButton.Left,
            };
        }
    }
}
