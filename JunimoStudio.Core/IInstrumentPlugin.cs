using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Midi;

namespace JunimoStudio.Core
{
    public interface IInstrumentPlugin : IPlugin
    {
        void ProcessMidi(IList<MidiEvent> events);
    }
}
