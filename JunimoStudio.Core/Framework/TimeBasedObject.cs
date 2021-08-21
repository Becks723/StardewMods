using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace JunimoStudio.Core.Framework
{
    public class TimeBasedObject : ITimeBasedObject
    {
        private class _TimeSignature : ITimeSignature
        {
            private readonly int[] _numeratorRange = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };

            private readonly int[] _denominatorRange = new int[] { 2, 4, 8, 16 };

            private int _numerator;

            private int _demominator;

            private readonly TimeBasedObject _tbo;

            public int Numerator
            {
                get => this._numerator;
                set
                {
                    if (this._numerator != value)
                    {
                        if (this._numeratorRange.All(v => value != v))
                            throw new ArgumentOutOfRangeException(nameof(value));

                        this._numerator = value;
                        this._tbo.RaisePropertyChanged();
                    }
                }
            }

            public int Denominator
            {
                get => this._demominator;
                set
                {
                    if (this._demominator != value)
                    {
                        if (this._denominatorRange.All(v => value != v))
                            throw new ArgumentOutOfRangeException(nameof(value));

                        this._demominator = value;
                        this._tbo.RaisePropertyChanged();
                    }
                }
            }

            public _TimeSignature(TimeBasedObject tbo)
            {
                this._tbo = tbo;
                this._numerator = Constants.DEFAULT_TIMESIGNATURE_NUMERATOR;
                this._demominator = Constants.DEFAULT_TIMESIGNATURE_DENOMERATOR;
            }
        }

        private int _tpq;

        private int _bpm;

        private readonly _TimeSignature _timeSignature;

        public event PropertyChangedEventHandler PropertyChanged;

        public int TicksPerQuarterNote
        {
            get => this._tpq;
            set
            {
                if (this._tpq != value)
                {
                    if (value <= 0)
                        throw new ArgumentOutOfRangeException(nameof(value));

                    this._tpq = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public int Bpm
        {
            get => this._bpm;
            set
            {
                if (this._bpm != value)
                    if (value <= 0)
                        throw new ArgumentOutOfRangeException(nameof(value));

                this._bpm = value;
                this.RaisePropertyChanged();
            }
        }

        public ITimeSignature TimeSignature => this._timeSignature;

        public TimeBasedObject()
        {
            this._bpm = Constants.DEFAULT_BPM;
            this._tpq = Constants.DEFAULT_TICKSPERQUARTERNOTE;
            this._timeSignature = new _TimeSignature(this);
        }

        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new(propertyName));
        }
    }
}
