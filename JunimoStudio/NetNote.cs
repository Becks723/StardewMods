using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using JunimoStudio.Core;
using Microsoft.Xna.Framework;
using Netcode;

namespace JunimoStudio
{
    public class NetNote : AbstractNetObjectWrapper<INote>
    {
        private INote _note;

        [XmlIgnore]
        public override INote Core => this._note;

        [XmlElement("pitch")]
        public readonly NetInt pitch = new NetInt(60); // default C5.

        [XmlElement("start")]
        public readonly NetLong start = new NetLong(0);

        [XmlElement("duration")]
        public readonly NetInt duration = new NetInt(120);

        [XmlElement("velocity")]
        public readonly NetInt velocity = new NetInt(100); // range 0 ~ 127

        [XmlElement("pan")]
        public readonly NetInt pan = new NetInt(63); // 0 for left, 127 for right.

        public NetNote()
            : base()
        {
        }

        public NetNote(INote note)
        {
            this._note = note ?? throw new ArgumentNullException(nameof(note));
            this.pitch.Value = note.Number;
            this.start.Value = note.Start;
            this.duration.Value = note.Duration;
            this.velocity.Value = note.Velocity;
            this.pan.Value = note.Pan;
        }

        public void Update(GameTime gameTime)
        {
            // 手动同步属性。
            INote note = this._note;
            if (note != null)
            {
                if (this.pitch.Value != note.Number)
                    this.pitch.Value = note.Number;
                if (this.start.Value != note.Start)
                    this.start.Value = note.Start;
                if (this.duration.Value != note.Duration)
                    this.duration.Value = note.Duration;
                if (this.velocity.Value != note.Velocity)
                    this.velocity.Value = note.Velocity;
                if (this.pan.Value != note.Pan)
                    this.pan.Value = note.Pan;
            }
        }

        public override void RestoreCoreObject()
        {
            this._note = Factory.Note();
            this._note.Number = this.pitch.Value;
            this._note.Start = this.start.Value;
            this._note.Duration = this.duration.Value;
            this._note.Velocity = this.velocity.Value;
            this._note.Pan = this.pan.Value;
        }

        protected override void InitNetFields()
        {
            this.NetFields.AddFields(this.pitch, this.start, this.duration, this.velocity, this.pan);
        }
    }
}
