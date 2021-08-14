using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunimoStudio.Core
{
    /// <summary>Represents an object affected by certain time factors.</summary>
    public interface ITimeBasedObject : INotifyPropertyChanged
    {
        /// <summary>Gets or sets the number of ticks per quarter note.</summary>
        int TicksPerQuarterNote { get; set; }

        /// <summary>Gets or sets the number of beats played in one minute.</summary>
        int Bpm { get; set; }

        /// <summary>Gets or sets the time signature.</summary>
        ITimeSignature TimeSignature { get; }
    }
}
