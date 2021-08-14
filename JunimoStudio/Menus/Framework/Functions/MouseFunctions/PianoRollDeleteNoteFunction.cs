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
    internal class PianoRollDeleteNoteFunction : PianoRollBaseMouseFunction
    {
        private readonly INoteCollection _notes;

        private readonly List<DisplayNote> _displayNotes;

        private INote _noteToDelete;

        public bool Deleting { get; private set; }

        public PianoRollDeleteNoteFunction(ActionManager actionManager, PianoRollMainScrollContent pianoRoll, INoteCollection notes, List<DisplayNote> displayNotes)
            : base(actionManager, pianoRoll, new MouseClickGesture(MouseButton.Right))
        {
            this._notes = notes;
            this._displayNotes = displayNotes;

            MouseClickGesture deleteNoteGes = this._editGesture as MouseClickGesture;
            deleteNoteGes.TriggerMoment = MouseClickStyle.WhenPressed;
            deleteNoteGes.CanTriggerCore = (e) => this.CanDelete(e.Position);
            deleteNoteGes.Clicked += this.OnCLicked;
        }

        private bool CanDelete(Point mousePos)
        {
            if (!this._pianoRoll.NoteAvailableBounds.Contains(mousePos))
                return false;

            Point extentMousePos = new Point(
                mousePos.X + this._pianoRoll.HorizontalOffset,
                mousePos.Y + this._pianoRoll.VerticalOffset);

            // 鼠标悬停在任何一个音符上面。
            this._noteToDelete = this._displayNotes
                .LastOrDefault(n => n.Bounds.Contains(extentMousePos))
                ?.Core;
            return (this._noteToDelete != null);
        }

        private void OnCLicked(object sender, MouseGestureEventArgs e)
        {
            this.Deleting = true;
            this._actionManager.RecordAction(
                new PianoRollRemoveNoteAction(this._notes, this._noteToDelete));
            this.Deleting = false;
        }
    }
}
