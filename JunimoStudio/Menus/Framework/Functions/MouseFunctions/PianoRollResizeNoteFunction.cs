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
    internal class PianoRollResizeNoteFunction : PianoRollBaseMouseFunction
    {
        private readonly List<DisplayNote> _displayNotes;

        private INote _noteToResize;

        private int _mouseDownOffsetX;

        private bool _firstAction = true;

        public bool Resizing { get; private set; }

        public PianoRollResizeNoteFunction(ActionManager actionManager, PianoRollMainScrollContent pianoRoll, List<DisplayNote> displayNotes)
            : base(actionManager, pianoRoll, new MouseDragAndDropGesture(MouseButton.Left))
        {
            this._displayNotes = displayNotes;

            MouseDragAndDropGesture resizeNoteGes = this._editGesture as MouseDragAndDropGesture;
            resizeNoteGes.CanTriggerCore = (e) => this.CanResize(e.Position);
            resizeNoteGes.Down += this.OnMouseDown;
            resizeNoteGes.Dragging += this.DuringDragging;
            resizeNoteGes.Dropped += this.OnDropped;
        }

        private bool CanResize(Point mousePos)
        {
            if (!this._pianoRoll.NoteAvailableBounds.Contains(mousePos))
                return false;

            Point extentMousePos = new Point(
                mousePos.X + this._pianoRoll.HorizontalOffset,
                mousePos.Y + this._pianoRoll.VerticalOffset);

            this._noteToResize = this._displayNotes
                .LastOrDefault(n => n.ResizeBounds.Contains(extentMousePos))
                ?.Core;
            return (this._noteToResize != null);
        }

        private void OnMouseDown(object sender, MouseGestureEventArgs e)
        {
            this.Resizing = true;

            Point mousePos = e.Position;

            int mouseTicks = this._pianoRoll.GetNoteTicksAtXPos(mousePos.X, false);
            this._mouseDownOffsetX = mouseTicks - ((int)this._noteToResize.Start + this._noteToResize.Duration);
        }

        private void DuringDragging(object sender, MouseGestureEventArgs e)
        {
            Point mousePos = e.Position;

            int ticks = this._pianoRoll.GetNoteTicksAtXPos(mousePos.X, true);
            int realTicks = ticks - this._mouseDownOffsetX;

            int newDuration = realTicks - (int)this._noteToResize.Start;
            newDuration = Math.Max(1, newDuration);

            var resizeAction = new PianoRollResizeNoteAction(this._noteToResize, this._noteToResize.Duration, newDuration);

            if (this._firstAction)
            {
                this._firstAction = false;
                resizeAction.AllowToMergeWithPrevious = false;
            }
            else
            {
                resizeAction.AllowToMergeWithPrevious = true;
            }

            this._actionManager.RecordAction(resizeAction);
        }

        private void OnDropped(object sender, MouseGestureEventArgs e)
        {
            this.Resizing = false;
            this._firstAction = true;
        }
    }
}
