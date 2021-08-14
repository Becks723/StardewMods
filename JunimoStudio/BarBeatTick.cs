using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunimoStudio
{
    public struct BarBeatTick
    {
        private int _beatsPerBar;
        private int _ticksPerBeat;
        private int _bars;
        private int _beats;
        private int _ticks;

        /// <summary>
        /// Get or set the number of beats per bar.
        /// </summary>
        /// <exception cref="ArgumentException">Value less than 1.</exception>
        public int BeatsPerBar
        {
            get => _beatsPerBar;
            set
            {
                if (_beatsPerBar != value)
                {
                    if (value < 1)
                        throw new ArgumentException(nameof(value));

                    _beatsPerBar = value;

                    CheckCarryBit(ref _beats, ref _bars, value);
                }
            }
        }

        /// <summary>
        /// Get or set the number of ticks per beat.
        /// </summary>
        /// <exception cref="ArgumentException">Value less than 1.</exception>
        public int TicksPerBeat
        {
            get => _ticksPerBeat;
            set
            {
                if (_ticksPerBeat != value)
                {
                    if (value < 1)
                        throw new ArgumentException(nameof(value));

                    var old = _ticksPerBeat;
                    _ticksPerBeat = value;

                    double p1 = (double)_ticks / (double)old;
                    _ticks = (int)Math.Round(p1 * value, MidpointRounding.AwayFromZero);
                }
            }
        }

        /// <summary>
        /// Get or set the bar part of current <see cref="BarBeatTick"/> structure.
        /// </summary>
        /// <exception cref="ArgumentException">Value less than 0.</exception>
        public int Bars
        {
            get => _bars;
            set
            {
                if (_bars != value)
                {
                    if (value < 0)
                        throw new ArgumentException(nameof(value));
                    _bars = value;
                }
            }
        }

        /// <summary>
        /// Get or set the beat part of current <see cref="BarBeatTick"/> structure.
        /// </summary>
        /// <exception cref="ArgumentException">Value less than 0.</exception>
        public int Beats
        {
            get => _beats;
            set
            {
                if (_beats != value)
                {
                    if (value < 0)
                        throw new ArgumentException(nameof(value));

                    CheckCarryBit(ref value, ref _bars, _beatsPerBar);
                    _beats = value;
                }
            }
        }

        /// <summary>
        /// Get or set the tick part of current <see cref="BarBeatTick"/> structure.
        /// </summary>   
        /// <exception cref="ArgumentException">Value less than 0.</exception>
        public int Ticks
        {
            get => _ticks;
            set
            {
                if (_ticks != value)
                {
                    if (value < 0)
                        throw new ArgumentException(nameof(value));

                    CheckCarryBit(ref value, ref _beats, _ticksPerBeat);
                    CheckCarryBit(ref _beats, ref _bars, _beatsPerBar);
                    _ticks = value;
                }
            }
        }

        public int TotalTicks
        {
            get
            {
                return _bars * _beatsPerBar * _ticksPerBeat + _beats * _ticksPerBeat + _ticks;
            }
        }

        /// <summary>
        /// Initialize a new instance with a zero <see cref="BarBeatTick"/> value.
        /// </summary>
        /// <param name="beatsPerBar">The number of beats per bar.</param>
        /// <param name="ticksPerBeat">The number of ticks per bar.</param>
        public BarBeatTick(int beatsPerBar, int ticksPerBeat)
        {
            _bars = _beats = _ticks = 0;
            _beatsPerBar = Math.Max(1, beatsPerBar);
            _ticksPerBeat = Math.Max(1, ticksPerBeat);
        }


        /// <summary>
        /// Initialize a new instance with a zero <see cref="BarBeatTick"/> value.
        /// </summary>
        /// <param name="beatsPerBar">The number of beats per bar.</param>
        /// <param name="ticksPerBeat">The number of ticks per bar.</param>
        /// <param name="bars">The number of bars.</param>
        /// <param name="beats">The number of beats.</param>
        /// <param name="ticks">The number of ticks.</param>
        public BarBeatTick(int beatsPerBar, int ticksPerBeat, int bars, int beats, int ticks)
            : this(beatsPerBar, ticksPerBeat)
        {
            Bars = bars;
            Beats = beats;
            Ticks = ticks;
        }

        public override string ToString()
        {
            return $"{Bars + 1}:{Beats + 1}:{Ticks}";
        }

        /// <summary>
        /// Convert the current <see cref="BarBeatTick"/> instance to an equal value in <see cref="TimeSpan"/>, 
        /// according to <paramref name="bpm"/> (beats per minute).
        /// </summary>
        /// <param name="bpm">Given beats per minute.</param>
        public TimeSpan ToTimeSpan(int bpm)
        {
            double totalBeats = BeatsPerBar * Bars + Beats + (double)Ticks / (double)TicksPerBeat;
            return BeatTimeCalculate.ActualTime(totalBeats, bpm);
        }

        /// <summary>
        /// Convert a given <see cref="BarBeatTick"/> instance <paramref name="bbt"/> to an equal value in <see cref="TimeSpan"/>, 
        /// according to <paramref name="bpm"/> (beats per minute).
        /// </summary>
        /// <param name="bbt">Given <see cref="BarBeatTick"/> instance.</param>
        /// <param name="bpm">Given beats per minute.</param>
        public static TimeSpan ToTimeSpan(BarBeatTick bbt, int bpm)
        {
            return bbt.ToTimeSpan(bpm);
        }

        public static bool operator ==(BarBeatTick value1,BarBeatTick value2)
        {
            return value1.BeatsPerBar == value2.BeatsPerBar &&
                value1.TicksPerBeat == value2.TicksPerBeat &&
                value1.Bars == value2.Bars &&
                value1.Beats == value2.Beats &&
                value1.Ticks == value2.Ticks;

        }

        public static bool operator !=(BarBeatTick value1, BarBeatTick value2)
        {
            return !(value1 == value2);
        }


        private void CheckCarryBit(ref int secondary, ref int primary, int sign)
        {
            while (secondary >= sign)
            {
                secondary -= sign;
                primary++;
            }
        }
    }
}
