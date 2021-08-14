using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GuiLabs.Undo;
using JunimoStudio.Core;

namespace JunimoStudio.Actions.PianoRollActions
{
    internal class PianoRollDragAddNoteAction : PianoRollAddNoteAction
    {
        public PianoRollDragAddNoteAction(INoteCollection notes, INote note)
            : base(notes, note)
        {
        }

        public override bool TryToMerge(IAction followingAction)
        {
            PianoRollMoveNoteAction following = followingAction as PianoRollMoveNoteAction;
            if (following != null 
                && following.AllowToMergeWithPrevious 
                && following.Note == this._note)
            {
                this._note.Start = following.Note.Start;
                this._note.Number = following.Note.Number;
                return true;
            }

            return false;
        }
    }
}
