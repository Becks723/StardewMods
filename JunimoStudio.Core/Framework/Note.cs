using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using NAudio.Midi;

namespace JunimoStudio.Core.Framework
{
    /// <summary>A basic implementation of <see cref="INote"/>.</summary>
    internal class Note : INote
    {
        private readonly ITimeBasedObject _timeSettingsImpl;

        private int _num;

        private long _start;

        private int _duration;

        private int _vel;

        private int _pan;

        private int _oldTpqChached;

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual int Number
        {
            get => this._num;
            set
            {
                if (value < Constants.MinNoteNumber || value > Constants.MaxNoteNumber)
                    throw new ArgumentOutOfRangeException(nameof(value));
                this._num = value;
                this.RaisePropertyChanged();
            }
        }

        public virtual int Duration
        {
            get => this._duration;
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                this._duration = value;
                this.RaisePropertyChanged();

            }
        }

        public virtual long Start
        {
            get => this._start;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                this._start = value;
                this.RaisePropertyChanged();
            }
        }

        /// <inheritdoc/>
        public virtual int Velocity
        {
            get => this._vel;
            set
            {
                if (value < 0 || value > 127)
                    throw new ArgumentOutOfRangeException(nameof(value));

                this._vel = value;
                this.RaisePropertyChanged();
            }
        }

        public virtual int Pan
        {
            get => this._pan;
            set
            {
                if (value < 0 || value > 127)
                    throw new ArgumentOutOfRangeException(nameof(value));

                this._pan = value;
                this.RaisePropertyChanged();
            }
        }

        #region ITimeBasedObject Members
        public int TicksPerQuarterNote
        {
            get => this._timeSettingsImpl.TicksPerQuarterNote;
            set => this._timeSettingsImpl.TicksPerQuarterNote = value;
        }

        public int Bpm
        {
            get => this._timeSettingsImpl.Bpm;
            set => this._timeSettingsImpl.Bpm = value;
        }

        public ITimeSignature TimeSignature
        {
            get => this._timeSettingsImpl.TimeSignature;
        }
        #endregion

        public Note()
            : this(pitch: 60, start: 0, duration: 120, vel: 100, pan: 100)
        {
        }

        public Note(int pitch, long start, int duration, int vel, int pan)
            : this(new TimeBasedObject(), pitch, start, duration, vel, pan)
        {

        }

        public Note(ITimeBasedObject timeSettingsImpl, int pitch, long start, int duration, int vel, int pan)
        {
            this._timeSettingsImpl = timeSettingsImpl;
            this._timeSettingsImpl.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(this.TicksPerQuarterNote))
                {
                    int before = this._oldTpqChached;
                    int after = this.TicksPerQuarterNote;
                    this.Start = (long)((double)this._start / (double)before * after);
                    this.Duration = (int)((double)this._duration / (double)before * after);

                    // 手动保存变化后的值，因为每次变化都要用到上一次的值。
                    this._oldTpqChached = this.TicksPerQuarterNote;
                }
                else if (e.PropertyName == nameof(this.Bpm))
                {

                }
                else if (e.PropertyName == nameof(this.TimeSignature.Numerator))
                {

                }
                else if (e.PropertyName == nameof(this.TimeSignature.Denominator))
                {

                }
            };

            this._num = pitch;
            this._start = start;
            this._duration = duration;
            this._vel = vel;
            this._pan = pan;
        }

        public List<MidiEvent> ToMidiEvents()
        {
            List<MidiEvent> events = new();

            // 一些属性的简单说明

            // MidiEvent中：
            // AbsoluteTime - the number of ticks from the start of the MIDI file (calculated by adding the deltas for all previous events).
            // Channel - the MIDI channel number from 1 to 16.
            // DeltaTime - the number of ticks after the previous event in the MIDI file.

            // NoteEvent中：
            // NoteNumber - the MIDI note number in the range 0 - 127.
            // Velocity - the MIDI note velocity in the range 0 - 127. If the command code is NoteOn and the velocity is 0, then most synthesizers will interpret this as a note off event.

            // NoteOnEvent中：
            // NoteLength - the note length in ticks. Adjusting this value will change the absolutetime of the associated note off event.

            PatchChangeEvent change = new(
                absoluteTime: this._start,
                channel: 1,
                patchNumber: 0);

            NoteOnEvent noteOn = new(
                absoluteTime: this._start,
                channel: 1,
                noteNumber: this._num,
                velocity: this._vel,
                duration: this._duration);
            events.Add(change);
            events.Add(noteOn);

            return events;
        }

        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(
                this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
