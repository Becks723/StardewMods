using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunimoStudio
{
    /// <summary>
    /// Notes that classified by time.
    /// </summary>
    public enum NoteDuration
    {
        /// <summary>A double whole note.</summary>
        Breve,

        /// <summary>A whole note.</summary>
        Semibreve,

        /// <summary>A half note.</summary>
        Minim,

        /// <summary>A quarter note.</summary>
        Crotchet,

        /// <summary>An eighth note.</summary>
        Quaver,

        /// <summary>A sixteenth note.</summary>
        Semiquaver,

        /// <summary>A thirty-second note.</summary>
        Demisemiquaver,

        /// <summary>A sixty-fourth note.</summary>
        SixtyFourth,

        /// <summary>A one hundred and twenty-eighth note.</summary>
        OneHundredAndTwentyEighth,
    }
}
