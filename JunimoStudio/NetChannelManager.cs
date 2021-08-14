using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using JunimoStudio.Core;
using JunimoStudio.Core.Plugins.Instruments;
using Microsoft.Xna.Framework;
using Netcode;

namespace JunimoStudio
{
    public class NetChannelManager : AbstractNetObjectWrapper<IChannelManager>
    {
        private IChannelManager _channelManager;

        [XmlIgnore]
        public override IChannelManager Core => this._channelManager;

        public readonly NetCollection<NetChannel> channels = new NetCollection<NetChannel>();

        public NetChannelManager()
            : base()
        {
        }

        public void Update(GameTime gameTime)
        {
            foreach (NetChannel netChannel in this.channels)
            {
                netChannel.Update(gameTime);
            }
        }

        public override void RestoreCoreObject()
        {
            this._channelManager = Factory.ChannelManager();

            // 初始化成员。
            foreach (NetChannel netChannel in this.channels)
            {
                netChannel.RestoreCoreObject();
                this._channelManager.Channels.Add(netChannel.Core);
            }

            // 订阅集合删减事件。
            this._channelManager.Channels.CollectionChanged += this.OnChannelCollectionChanged;
        }

        protected override void InitNetFields()
        {
            this.NetFields.AddFields(this.channels);
        }

        private void OnChannelCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (object item in e.NewItems)
                    if (item is IChannel channel)
                    {
                        NetChannel netChannel = new NetChannel(channel);
                        this.channels.Add(netChannel);
                    }

            if (e.OldItems != null)
                foreach (object item in e.OldItems)
                    if (item is IChannel channel)
                    {
                        NetChannel toRemove
                            = this.channels.FirstOrDefault(c => object.ReferenceEquals(c.Core, channel));
                        this.channels.Remove(toRemove);
                    }
        }

        private void Channels_OnValueAdded(NetChannel value)
        {
        }
    }

    public static class ChannelManagerExtensions
    {
        public static IChannel AddChannel<TGenerator>(this IChannelManager channelManager)
            where TGenerator : IInstrumentPlugin, new()
        {
            return channelManager.AddChannel(new TGenerator());
        }
    }
}
