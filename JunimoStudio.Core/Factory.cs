using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JunimoStudio.Core.Framework;
using JunimoStudio.Core.Plugins;
using JunimoStudio.Core.Plugins.Instruments;

namespace JunimoStudio.Core
{
    public static class Factory
    {
        public static IChannel Channel()
        {
            return new Channel(new NoteCollection(), null, 0);
        }

        public static IChannelManager ChannelManager()
        {
            return new ChannelManager();
        }

        public static INote Note()
        {
            return new Note(60, 0, 120, 100, 63);
        }

        public static TPlugin Plugin<TPlugin>(string uniqueId)
            where TPlugin : IPlugin
        {
            IPlugin plugin;
            plugin = uniqueId switch {
                Interfaces.MidiOut => new MidiOutPlugin(),
                Interfaces.Empty => new DummyPlugin(),
            };

            return (TPlugin)plugin;
        }

    }
}
