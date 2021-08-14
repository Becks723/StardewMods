using System;
using GuiLabs.Undo;
using JunimoStudio.Input.Gestures;

namespace JunimoStudio.Menus.Framework.Functions.MouseFunctions
{
    internal class PianoRollBaseMouseFunction : BaseMouseFunction
    {
        protected readonly PianoRollMainScrollContent _pianoRoll;

        public PianoRollBaseMouseFunction(ActionManager actionManager, PianoRollMainScrollContent pianoRoll, MouseGestureBase editGesture)
            : base(actionManager, editGesture)
        {
            this._pianoRoll = pianoRoll ?? throw new ArgumentNullException(nameof(pianoRoll));
        }
    }
}
