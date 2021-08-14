using System.Collections.Generic;
using NAudio.Midi;

namespace JunimoStudio.Core
{
    public interface IMidiProcessor
    {
        void Process(IList<MidiEvent> midiEvents);
    }
}
