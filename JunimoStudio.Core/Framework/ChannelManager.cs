using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JunimoStudio.Core.Plugins.Instruments;

namespace JunimoStudio.Core.Framework
{
    internal class ChannelManager : IChannelManager
    {
        private readonly IChannelList _channels;

        public IChannelList Channels => _channels;

        public ChannelManager()
            : this(new ChannelList())
        {

        }

        public ChannelManager(IChannelList channels)
        {
            _channels = channels ?? throw new ArgumentNullException(nameof(channels));
        }

        public IChannel AddChannel(string channelName, IInstrumentPlugin generator)
        {
            IChannel channel = new Channel(
                notes: new NoteCollection(),
                generator: generator,
                existingChannelNum: _channels.Count());
            if (channelName != null)
                channel.Name = channelName;
            _channels.Add(channel);
            return channel;
        }

        public IChannel AddChannel(IInstrumentPlugin generator)
        {
            return AddChannel(null, generator);
        }
    }
}
