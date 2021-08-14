using GuiLabs.Undo;
using JunimoStudio.Core;

namespace JunimoStudio.Actions.PianoRollActions
{
    /// <summary>Move note action, which can affect <see cref="INote.Start"/> and <see cref="INote.Number"/>.</summary>
    internal class PianoRollMoveNoteAction : ActionBase
    {
        /// <summary>The note to move.</summary>
        private readonly INote _note;

        /// <summary>Old start time.</summary>
        private readonly int _oldStart;

        /// <summary>New start time.</summary>
        private readonly int _newStart;

        /// <summary>Old pitch.</summary>
        private readonly int _oldPitch;

        /// <summary>New pitch.</summary>
        private readonly int _newPitch;

        /// <summary>The note to move.</summary>
        public INote Note => this._note;

        /// <summary>Old start time.</summary>
        public int OldStart => this._oldStart;

        /// <summary>New start time.</summary>
        public int NewStart => this._newStart;

        /// <summary>Old pitch.</summary>
        public int OldPitch => this._oldPitch;

        /// <summary>New pitch.</summary>
        public int NewPitch => this._newPitch;

        public PianoRollMoveNoteAction(INote note, int oldStart, int newStart, int oldPitch, int newPitch)
            : base("Piano roll move note")
        {
            this._note = note;
            this._oldStart = oldStart;
            this._newStart = newStart;
            this._oldPitch = oldPitch;
            this._newPitch = newPitch;
        }

        protected override void ExecuteCore()
        {
            this._note.Start = this._newStart;
            this._note.Number = this._newPitch;
        }

        protected override void UnExecuteCore()
        {
            this._note.Start = this._oldStart;
            this._note.Number = this._oldPitch;
        }

        public override bool TryToMerge(IAction followingAction)
        {
            PianoRollMoveNoteAction following = followingAction as PianoRollMoveNoteAction;

            if (following != null
                && following.AllowToMergeWithPrevious
                && object.ReferenceEquals(following.Note, this.Note))
            {
                this._note.Start = following.NewStart;
                this._note.Number = following.NewPitch;
                return true;
            }

            return false;
        }
    }
}
