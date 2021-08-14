using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunimoStudio.Core.Framework
{
    internal class ChannelList : IChannelList
    {
        private readonly IList<IChannel> _channels;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public ChannelList()
        {
            _channels = new List<IChannel>();
        }

        public IChannel this[string channelName]
        {
            get
            {
                IChannel channel = FindChannelByName(channelName);
                if (channel == null)
                    throw new KeyNotFoundException(nameof(channelName));

                return channel;
            }
        }

        public void Add(IChannel channel)
        {
            if (channel == null)
                throw new ArgumentNullException(nameof(channel));

            if (channel.Name == null)
            {
                throw new InvalidOperationException("Channel name cannot be null.");
            }
            if (FindChannelByName(channel.Name) != null)
            {
                throw new InvalidOperationException("Channel name already exists.");
            }

            _channels.Add(channel);
            RaiseCollectionChanged(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, channel));
        }

        public bool Remove(string channelName)
        {
            bool removed = false;

            IChannel target = FindChannelByName(channelName);
            if (target != null)
            {
                _channels.Remove(target);
                RaiseCollectionChanged(
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, target));
                target.Dispose();
                target = null;
                removed = true;
            }

            return removed;
        }

        public IEnumerator<IChannel> GetEnumerator()
        {
            return _channels.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected void RaiseCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        private IChannel FindChannelByName(string channelName)
        {
            return _channels.FirstOrDefault(c => c.Name == channelName);
        }
    }
}
