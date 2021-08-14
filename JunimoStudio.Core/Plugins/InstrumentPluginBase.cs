using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Midi;

namespace JunimoStudio.Core.Plugins
{
    public abstract class InstrumentPluginBase : PluginBase, IInstrumentPlugin
    {
        protected InstrumentPluginBase(Guid id, string name)
            : base(id, name, PluginCategory.Instrument)
        {
        }

        public abstract void ProcessMidi(IList<MidiEvent> events);
    }
}
