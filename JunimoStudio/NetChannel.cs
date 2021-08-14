using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using JunimoStudio.Core;
using Microsoft.Xna.Framework;
using Netcode;

namespace JunimoStudio
{
    public class NetChannel : AbstractNetObjectWrapper<IChannel>
    {
        private IChannel _channel;

        [XmlIgnore]
        public override IChannel Core => this._channel;

        [XmlElement("name")]
        public readonly NetString name = new NetString();

        [XmlElement("mute")]
        public readonly NetBool mute = new NetBool();

        [XmlElement("effectTrack")]
        public readonly NetInt effectTrack = new NetInt();

        public readonly NetCollection<NetNote> notes = new NetCollection<NetNote>();

        [XmlElement("generator")]
        public readonly NetRef<NetPlugin> generator = new NetRef<NetPlugin>();

        public NetChannel()
            : base()
        {
        }

        public NetChannel(IChannel channel)
        {
            this.InitNetFields();

            this._channel = channel ?? throw new ArgumentNullException(nameof(channel));
            this.name.Value = channel.Name;
            this.effectTrack.Value = channel.EffectTrack;
            this.generator.Value = new NetPlugin(channel.Generator);
            channel.Notes.CollectionChanged += this.OnNoteCollectionChanged;
            foreach (INote note in channel.Notes)
                this.notes.Add(new NetNote(note));
        }

        public void Update(GameTime gametime)
        {
            // 手动同步属性。
            if (this._channel != null)
            {
                if (this.name.Value != this._channel.Name)
                    this.name.Value = this._channel.Name;
                if (this.mute.Value != this._channel.Mute)
                    this.mute.Value = this._channel.Mute;
                if (this.effectTrack.Value != this._channel.EffectTrack)
                    this.effectTrack.Value = this._channel.EffectTrack;
                if (this._channel.Generator != null
                    && this.generator.Value.uniqueId.Value != this._channel.Generator.UniqueId)
                {
                    this.generator.Value.uniqueId.Value = this._channel.Generator.UniqueId;
                }
            }
        }

        public override void RestoreCoreObject()
        {
            this._channel = Factory.Channel();

            foreach (NetNote netNote in this.notes)
            {
                netNote.RestoreCoreObject();
                this._channel.Notes.Add(netNote.Core);
            }
            this.generator.Value.RestoreCoreObject();

            this._channel.Notes.CollectionChanged += this.OnNoteCollectionChanged;
            this._channel.Name = this.name.Value;
            this._channel.Mute = this.mute.Value;
            this._channel.EffectTrack = this.effectTrack.Value;
            this._channel.Generator = this.generator.Value.Core as IInstrumentPlugin;
        }

        protected override void InitNetFields()
        {
            this.NetFields.AddFields(this.name, this.mute, this.effectTrack, this.generator);
        }

        private void OnNoteCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (object item in e.NewItems)
                    if (item is INote note)
                    {
                        NetNote netNote = new NetNote(note);
                        this.notes.Add(netNote);
                    }

            if (e.OldItems != null)
                foreach (object item in e.OldItems)
                    if (item is INote note)
                    {
                        NetNote toRemove = this.notes.FirstOrDefault(n => object.ReferenceEquals(n.Core, note));
                        this.notes.Remove(toRemove);
                    }
        }
    }
}
