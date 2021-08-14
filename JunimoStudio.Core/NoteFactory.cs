using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JunimoStudio.Core.Framework;

namespace JunimoStudio.Core
{
    public static class NoteFactory
    {
        public static INote Create(int pitch, long start, int duration)
        {
            INote note = new Note();
            note.Number = pitch;
            note.Start = start;
            note.Duration = duration;
            return note;
        }
    }
}
