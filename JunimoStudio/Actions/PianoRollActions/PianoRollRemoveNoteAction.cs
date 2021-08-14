using JunimoStudio.Core;

namespace JunimoStudio.Actions.PianoRollActions
{
    internal class PianoRollRemoveNoteAction : ActionBase
    {
        /// <summary>A collection of notes where to remove.</summary>
        private readonly INoteCollection _notes;

        /// <summary>The note to remove.</summary>
        private readonly INote _note;

        public PianoRollRemoveNoteAction(INoteCollection notes, INote note)
            : base("Piano roll delete note")
        {
            this._notes = notes;
            this._note = note;
        }

        protected override void ExecuteCore()
        {
            // remove.
            this._notes.Remove(this._note);
        }

        protected override void UnExecuteCore()
        {
            // add.
            this._notes.Add(this._note);
        }
    }
}
