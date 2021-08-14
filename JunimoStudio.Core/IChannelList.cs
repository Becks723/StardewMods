using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunimoStudio.Core
{
    public interface IChannelList : IEnumerable<IChannel>, INotifyCollectionChanged
    {
        IChannel this[string channelName] { get; }

        void Add(IChannel channel);

        bool Remove(string channelName);
    }
}
