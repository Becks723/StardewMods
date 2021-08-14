using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Midi;

namespace JunimoStudio
{
    /// <summary>Provides function to process and pass on the incoming midi messages.</summary>
    public interface IMidiProcessor
    {
        void Process(IList<MidiMessage> messages);
    }
}
