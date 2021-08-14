using JunimoStudio.Core;

namespace JunimoStudio.Actions.PianoRollActions
{
    internal class PianoRollAddNoteAction : ActionBase
    {
        /// <summary>A collection of notes where to add.</summary>
        protected readonly INoteCollection _notes;

        /// <summary>The note to add.</summary>
        protected readonly INote _note;

        public PianoRollAddNoteAction(INoteCollection notes, INote note)
            : base("Piano roll add note")
        {
            this._notes = notes;
            this._note = note;
        }

        protected override void ExecuteCore()
        {
            // add.
            this._notes.Add(this._note);
        }

        protected override void UnExecuteCore()
        {
            // remove.
            this._notes.Remove(this._note);
        }
    }
}
