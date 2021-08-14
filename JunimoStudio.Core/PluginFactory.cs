using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JunimoStudio.Core.Plugins.Instruments;

namespace JunimoStudio.Core
{
    public static class PluginFactory
    {
        //internal Dictionary<Guid, Func<IPlugin>>

        public static TPlugin Load<TPlugin>(string uniqueId)
            where TPlugin : IPlugin
        {
            IPlugin plugin;
            plugin = uniqueId switch {
                Interfaces.MidiOut => new MidiOutPlugin(),
            };

            return (TPlugin)plugin;
        }
    }
}
