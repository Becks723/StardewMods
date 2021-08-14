using System;
using GuiLabs.Undo;
using JunimoStudio.Input.Gestures;
using Microsoft.Xna.Framework;

namespace JunimoStudio.Menus.Framework.Functions.MouseFunctions
{
    internal abstract class BaseMouseFunction
    {
        protected readonly MouseGestureBase _editGesture;

        protected readonly ActionManager _actionManager;

        public BaseMouseFunction(ActionManager actionManager, MouseGestureBase editGesture)
        {
            this._actionManager = actionManager ?? throw new ArgumentNullException(nameof(actionManager));
            this._editGesture = editGesture ?? throw new ArgumentNullException(nameof(editGesture));
        }

        public virtual void Update(GameTime gameTime)
        {
            this._editGesture.Update(gameTime);
        }
    }
}
