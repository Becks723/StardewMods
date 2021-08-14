using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunimoStudio.Core
{
    /// <summary>Represents a time signature interface.</summary>
    public interface ITimeSignature
    {
        /// <summary>Gets or sets the numerator of current time signature, which indicates the number of beats per bar (measure).</summary>
        int Numerator { get; set; }

        /// <summary>Gets or sets the denominator of current time signature, which indicates the length of each beat.</summary>
        int Denominator { get; set; }
    }
}
