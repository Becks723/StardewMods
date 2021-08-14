using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JunimoStudio.Core.Plugins.Instruments;

namespace JunimoStudio.Core
{
    public interface IChannelManager
    {
        IChannelList Channels { get; }

        IChannel AddChannel(string channelName, IInstrumentPlugin generator);

        IChannel AddChannel(IInstrumentPlugin generator);
    }
}
