using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using JunimoStudio.Core.ComponentModel;

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
                get => _numerator;
                set
                {
                    if (_numerator != value)
                    {
                        if (_numeratorRange.All(v => value != v))
                            throw new ArgumentOutOfRangeException(nameof(value));

                        _numerator = value;
                        _tbo.RaisePropertyChanged();
                    }
                }
            }

            public int Denominator
            {
                get => _demominator;
                set
                {
                    if (_demominator != value)
                    {
                        if (_denominatorRange.All(v => value != v))
                            throw new ArgumentOutOfRangeException(nameof(value));

                        _demominator = value;
                        _tbo.RaisePropertyChanged();
                    }
                }
            }

            public _TimeSignature(TimeBasedObject tbo)
            {
                _tbo = tbo;
                _numerator = _demominator = 4;
            }
        }

        private int _tpq;
        private int _bpm;
        private readonly _TimeSignature _timeSignature;

        public event PropertyChangedEventHandler PropertyChanged;

        public int TicksPerQuarterNote
        {
            get => _tpq;
            set
            {
                if (_tpq != value)
                {
                    if (value <= 0)
                        throw new ArgumentOutOfRangeException(nameof(value));

                    _tpq = value;
                    RaisePropertyChanged();
                }
            }
        }

        public int Bpm
        {
            get => _bpm;
            set
            {
                if (_bpm != value)
                    if (value <= 0)
                        throw new ArgumentOutOfRangeException(nameof(value));

                _bpm = value;
                RaisePropertyChanged();
            }
        }

        public ITimeSignature TimeSignature
        {
            get => _timeSignature;
        }

        public TimeBasedObject()
        {
            _bpm = 100;
            _tpq = 120;
            _timeSignature = new _TimeSignature(this);
        }

        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new(propertyName));
        }
    }
}
