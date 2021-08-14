using System;
using System.Collections.Generic;
using System.Linq;
using GuiLabs.Undo;
using JunimoStudio.Actions.PianoRollActions;
using JunimoStudio.Core;
using JunimoStudio.Input;
using JunimoStudio.Input.Gestures;
using Microsoft.Xna.Framework;

namespace JunimoStudio.Menus.Framework.Functions.MouseFunctions
{
    internal class PianoRollMoveNoteFunction : PianoRollBaseMouseFunction
    {
        private readonly List<DisplayNote> _displayNotes;

        private readonly Func<bool> _adding;

        private INote _noteToMove;

        private int _startOffset;

        private int _pitchOffset;

        private bool _firstAction = true;

        public bool Moving { get; private set; }

        public PianoRollMoveNoteFunction(ActionManager actionManager, PianoRollMainScrollContent pianoRoll, List<DisplayNote> displayNotes, Func<bool> adding)
            : base(actionManager, pianoRoll, new MouseDragAndDropGesture(MouseButton.Left))
        {
            this._displayNotes = displayNotes;
            this._adding = adding;

            MouseDragAndDropGesture moveNoteGes = this._editGesture as MouseDragAndDropGesture;
            moveNoteGes.CanTriggerCore = (e) => this.CanMove(e.Position);

            moveNoteGes.Down += this.OnMouseDown;
            moveNoteGes.Dragging += this.DuringDragging;
            moveNoteGes.Dropped += this.OnDropped;
        }

        private bool CanMove(Point mousePos)
        {
            if (!this._pianoRoll.NoteAvailableBounds.Contains(mousePos))
                return false;

            Point extentMousePos = new Point(
                mousePos.X + this._pianoRoll.HorizontalOffset,
                mousePos.Y + this._pianoRoll.VerticalOffset);

            this._noteToMove = this._displayNotes
                .LastOrDefault(n =>
                    n.MoveBounds.Contains(extentMousePos) &&
                   !n.ResizeBounds.Contains(extentMousePos))
                ?.Core;

            return
                !this._adding()
                && this._noteToMove != null;
        }

        private void OnMouseDown(object sender, MouseGestureEventArgs e)
        {
            this.Moving = true;

            Point mousePos = e.Position;

            // cache the grab offset of mouse.
            // cache mouse down point offset relative to the note left-top vertex.
            int mouseStart = this._pianoRoll.GetNoteTicksAtXPos(mousePos.X);
            int mousePitch = this._pianoRoll.GetNotePitchAtYPos(mousePos.Y);
            this._startOffset = mouseStart - (int)this._noteToMove.Start;
            this._pitchOffset = mousePitch - this._noteToMove.Number;
        }

        private void DuringDragging(object sender, MouseGestureEventArgs e)
        {
            Point mousePos = e.Position;

            int mouseStart = this._pianoRoll.GetNoteTicksAtXPos(mousePos.X, true);
            int mousePitch = this._pianoRoll.GetNotePitchAtYPos(mousePos.Y, true);
            int newStart = Math.Max(0, mouseStart - this._startOffset);
            int newPitch = (int)MathHelper.Clamp(mousePitch - this._pitchOffset, Constants.MinNoteNumber, Constants.MaxNoteNumber);

            var moveNoteAction = new PianoRollMoveNoteAction(
                this._noteToMove, (int)this._noteToMove.Start, newStart, this._noteToMove.Number, newPitch);

            if (this._firstAction)
            {
                this._firstAction = false;
                moveNoteAction.AllowToMergeWithPrevious = false;
            }
            else
            {
                moveNoteAction.AllowToMergeWithPrevious = true;
            }

            this._actionManager.RecordAction(moveNoteAction);
        }

        private void OnDropped(object sender, MouseGestureEventArgs e)
        {
            this.Moving = false;
            this._firstAction = true;
        }
    }
}
