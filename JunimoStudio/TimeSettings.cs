using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JunimoStudio.Core;
using JunimoStudio.Core.Framework;
using StardewModdingAPI;

namespace JunimoStudio
{
    internal class TimeSettings : ITimeBasedObject
    {
        private readonly ITimeBasedObject _timeImpl = new TimeBasedObject();

        public int TicksPerQuarterNote
        {
            get => this._timeImpl.TicksPerQuarterNote;
            set => this._timeImpl.TicksPerQuarterNote = value;
        }

        public int Bpm
        {
            get => this._timeImpl.Bpm;
            set => this._timeImpl.Bpm = value;
        }

        public ITimeSignature TimeSignature => this._timeImpl.TimeSignature;

        public event PropertyChangedEventHandler PropertyChanged
        {
            add => this._timeImpl.PropertyChanged += value;
            remove => this._timeImpl.PropertyChanged -= value;
        }
    }
}
