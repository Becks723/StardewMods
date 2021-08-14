using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using JunimoStudio.Core;
using JunimoStudio.Menus.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JunimoStudio.Menus.Framework.ScrollViewers
{
    internal class ChannelRackViewer : ScrollViewer
    {
        private class ChannelRackScrollContent : ScrollContentBase
        {
            private readonly IChannelManager _channelManager;

            private RootElement _root;

            private readonly List<ChannelItem> _channelItems = new List<ChannelItem>();

            private readonly Action<string> _openPianoRoll;

            protected override Rectangle ScissorRectangle
                => new Rectangle((int)this.Position.X + 16, (int)this.Position.Y + 16, this.Width - 32, this.Height - 32);

            public override int ExtentWidth => 40 + 100 + 40 + 200 + 40;

            public override int ExtentHeight
                => 40 * 2 + this._channelManager.Channels.Count() * 60 + (this._channelManager.Channels.Count() - 1) * 20;

            public ChannelRackScrollContent(ScrollViewer owner, IChannelManager channelManager, Action<string> openPianoRoll)
                : base(owner)
            {
                this._channelManager = channelManager;
                this._openPianoRoll = openPianoRoll;

                this.CanHorizontallyScroll = this.CanVerticallyScroll = true;
                this.Init();
            }

            public override void Update(GameTime gameTime)
            {
                base.Update(gameTime);

                this._root.Update(gameTime);
            }

            protected override void DrawNonScrollContent(SpriteBatch b)
            { }

            protected override void DrawScrollContent(SpriteBatch b)
            {
                this._root.Draw(b);
            }

            private void Init()
            {
                this._root = new RootElement();
                this.Owner.AddChild(this._root);

                // init _channelItems with existing channels.
                int i = 0;
                foreach (IChannel channel in this._channelManager.Channels)
                {
                    ChannelItem channelItem
                        = new ChannelItem(channel, new Rectangle(0, 120 * i, 200, 120), this._openPianoRoll);
                    this._channelItems.Add(channelItem);
                    this._root.AddChild(channelItem);
                    i++;
                }

                // registration. whenever source channels changed, update _channelItems.
                this._channelManager.Channels.CollectionChanged += (s, e) =>
                {
                    if (e.Action == NotifyCollectionChangedAction.Add)
                    {
                        int index = e.NewStartingIndex;
                        foreach (object item in e.NewItems)
                        {
                            IChannel channel = (IChannel)item;
                            ChannelItem channelItem
                                = new ChannelItem(channel, new Rectangle(0, 120 * index, 200, 120), this._openPianoRoll);
                            this._channelItems.Insert(index, channelItem);
                            this._root.AddChild(channelItem);
                            index++;
                        }
                    }

                    if (e.Action == NotifyCollectionChangedAction.Remove)
                    {
                        int index = e.OldStartingIndex;
                        foreach (object item in e.OldItems)
                        {
                            this._root.RemoveChild(this._channelItems[index]);
                            this._channelItems.RemoveAt(index);
                            index++;
                        }
                    }
                };
            }
        }

        public ChannelRackViewer(Rectangle bounds, IChannelManager channelManager, Action<string> openPianoRoll)
            : base(bounds)
        {
            this.Content = new ChannelRackScrollContent(this, channelManager, openPianoRoll);
        }

        public override void Draw(SpriteBatch b)
        {
            base.Draw(b);
        }
    }
}
