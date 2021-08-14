using GuiLabs.Undo;
using JunimoStudio.Core;

namespace JunimoStudio.Actions.PianoRollActions
{
    /// <summary>Resize note action, which can affect <see cref="INote.Duration"/>.</summary>
    internal class PianoRollResizeNoteAction : ActionBase
    {
        /// <summary>The note to resize.</summary>
        private readonly INote _note;

        /// <summary>Old duration.</summary>
        private readonly int _oldDur;

        /// <summary>New duration.</summary>
        private readonly int _newDur;

        /// <summary>The note to resize.</summary>
        public INote Note => this._note;

        /// <summary>New duration.</summary>
        public int NewDuration => this._newDur;

        public PianoRollResizeNoteAction(INote note, int oldDur, int newDur)
            : base("Piano roll resize note")
        {
            this._note = note;
            this._oldDur = oldDur;
            this._newDur = newDur;
        }

        protected override void ExecuteCore()
        {
            this._note.Duration = this._newDur;
        }

        protected override void UnExecuteCore()
        {
            this._note.Duration = this._oldDur;
        }

        public override bool TryToMerge(IAction followingAction)
        {
            PianoRollResizeNoteAction following = followingAction as PianoRollResizeNoteAction;

            if (following != null
                && following.AllowToMergeWithPrevious
                && object.ReferenceEquals(following.Note, this.Note))
            {
                this._note.Duration = following.NewDuration;
                return true;
            }

            return false;
        }

    }
}
