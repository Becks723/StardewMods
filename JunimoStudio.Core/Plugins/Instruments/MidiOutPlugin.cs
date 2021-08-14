using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Midi;

namespace JunimoStudio.Core.Plugins.Instruments
{
    public class MidiOutPlugin : InstrumentPluginBase
    {
        private class MidiOutMidiProcessor : IMidiProcessor
        {
            private readonly MidiOut _midiOut;

            public MidiOutMidiProcessor(MidiOut midiOut)
            {
                this._midiOut = midiOut;
            }

            public void Process(IList<MidiEvent> midiEvents)
            {
                //foreach (MidiEvent ev in midiEvents)
                //{
                //    ev.Channel = _channel + 1; // 左边的范围1到16，右边范围0到15，所以对应的时候要加一。
                //    if (ev is PatchChangeEvent change)
                //    {
                //        change.Patch = _patch; // 左右范围都是0到127，不用加一。
                //    }
                //    _midiOut.Send(ev.GetAsShortMessage());
                //}

                foreach (MidiEvent @event in midiEvents)
                {
                    this._midiOut.Send(@event.GetAsShortMessage());
                }
            }
        }

        /// <summary>
        /// <see cref="MidiOut"/>在同一时间只能打开一个，就是说，它的实例同时间只能有一个，否则会报错。
        /// 于是用一个静态的字段保存那个唯一实例，如果再有new的，直接赋给那个需要new的。
        /// </summary>
        private static MidiOut _globalMidiOut;

        private readonly MidiOut _midiOut;

        private readonly IMidiProcessor _midiProcessor;
        private int _channel;
        private int _patch;

        public MidiOutPlugin()
            : this(0, new MidiOutMidiProcessor(_globalMidiOut ??= new MidiOut(0)))
        {
        }

        public MidiOutPlugin(int deviceNo, IMidiProcessor midiProcessor)
            : base(Guid.Parse(Interfaces.MidiOut), "Midi Out")
        {
            this._midiProcessor = midiProcessor;

            if (_globalMidiOut == null)
                _globalMidiOut = new MidiOut(deviceNo);
            this._midiOut = _globalMidiOut;
        }

        public int Channel
        {
            get => this._channel;
            set
            {
                if (value < 0 || value > 15)
                    throw new ArgumentOutOfRangeException(nameof(value));
                this._channel = value;
            }
        }

        public int Patch
        {
            get => this._patch;
            set
            {
                if (value < 0 || value > 127)
                    throw new ArgumentOutOfRangeException(nameof(value));
                this._patch = value;
            }
        }

        public override void ProcessMidi(IList<MidiEvent> events)
        {
            this._midiProcessor.Process(events);
        }

        public void SendNote(List<INote> notes)
        {
            var orderedNotes = notes.OrderBy(n => n.Start);
            foreach (INote note in orderedNotes)
            {
                this.ProcessMidi(note.ToMidiEvents());
                Thread.Sleep(note.TicksToMilliseconds(note.Duration));
            }
        }
    }
}
