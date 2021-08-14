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
    internal class PianoRollAddNoteFunction : PianoRollBaseMouseFunction
    {
        private readonly INoteCollection _notes;

        private readonly List<DisplayNote> _displayNotes;

        private INote _noteToAdd;

        public int NoteDurationCached { get; set; }

        public bool Adding { get; private set; }

        public PianoRollAddNoteFunction(ActionManager actionManager, PianoRollMainScrollContent pianoRoll, ITimeBasedObject timeSettings, INoteCollection notes, List<DisplayNote> displayNotes)
            : base(actionManager, pianoRoll, new MouseDragAndDropGesture(MouseButton.Left))
        {
            this._notes = notes;
            this._displayNotes = displayNotes;
            this.NoteDurationCached = timeSettings.TicksPerQuarterNote;

            MouseDragAndDropGesture addNoteGes = this._editGesture as MouseDragAndDropGesture;
            addNoteGes.CanTriggerCore = (e) => this.CanAdd(e.Position);
            addNoteGes.Down += this.OnMouseDown;
            addNoteGes.Dragging += this.DuringDragging;
            addNoteGes.Dropped += this.OnDropped;
        }

        private bool CanAdd(Point mousePos)
        {
            if (!this._pianoRoll.NoteAvailableBounds.Contains(mousePos))
                return false;

            Point extentMousePos = new Point(
                mousePos.X + this._pianoRoll.HorizontalOffset,
                mousePos.Y + this._pianoRoll.VerticalOffset);

            // 鼠标没悬停在任何一个音符上。
            return !this._displayNotes
                .Any(n =>
                    n.MoveBounds.Contains(extentMousePos) ||
                    n.ResizeBounds.Contains(extentMousePos));
        }

        private void OnMouseDown(object sender, MouseGestureEventArgs e)
        {
            this.Adding = true;

            Point mousePos = e.Position;

            int start = this._pianoRoll.GetNoteTicksAtXPos(mousePos.X);
            int pitch = this._pianoRoll.GetNotePitchAtYPos(mousePos.Y);
            int duration = this.NoteDurationCached;
            this._noteToAdd = NoteFactory.Create(pitch, start, duration);

            this._actionManager.RecordAction(new PianoRollAddNoteAction(this._notes, this._noteToAdd));
        }

        private void DuringDragging(object sender, MouseGestureEventArgs e)
        {
            Point mousePos = e.Position;
            int start = this._pianoRoll.GetNoteTicksAtXPos(mousePos.X, true);
            int pitch = this._pianoRoll.GetNotePitchAtYPos(mousePos.Y, true);
            this._noteToAdd.Start = Math.Max(0, start);
            this._noteToAdd.Number = pitch;
        }

        private void OnDropped(object sender, MouseGestureEventArgs e)
        {
            this.Adding = false;
        }
    }
}