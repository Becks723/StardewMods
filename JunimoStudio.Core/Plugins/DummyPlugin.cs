using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Midi;

namespace JunimoStudio.Core.Plugins
{
    public class DummyPlugin : PluginBase, IInstrumentPlugin, IEffectPlugin
    {
        public DummyPlugin()
            : base(Guid.Empty, "Dummy Plugin", PluginCategory.Unknown)
        {
        }

        public void ProcessMidi(IList<MidiEvent> events)
        {
            throw new NotImplementedException();
        }
    }
}
