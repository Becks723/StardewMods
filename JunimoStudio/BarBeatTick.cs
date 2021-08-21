//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace JunimoStudio
//{
//    /// <summary>
//    /// 简单的结构体，表示“小节-节拍-单位刻（tick）”。用来记录音乐中的时间。
//    /// </summary>
//    public struct BarBeatTick
//    {
//        private int _beatsPerBar;

//        private int _ticksPerBeat;

//        private int _bars;

//        private int _beats;

//        private int _ticks;

//        /// <summary>
//        /// Get or set the number of beats per bar.
//        /// </summary>
//        /// <exception cref="ArgumentException">Value less than 1.</exception>
//        public int BeatsPerBar
//        {
//            get => this._beatsPerBar;
//            set
//            {
//                if (this._beatsPerBar != value)
//                {
//                    if (value < 1)
//                        throw new ArgumentException(nameof(value));

//                    this._beatsPerBar = value;

//                    this.CheckCarryBit(ref this._beats, ref this._bars, value);
//                }
//            }
//        }

//        /// <summary>
//        /// Get or set the number of ticks per beat.
//        /// </summary>
//        /// <exception cref="ArgumentException">Value less than 1.</exception>
//        public int TicksPerBeat
//        {
//            get => this._ticksPerBeat;
//            set
//            {
//                if (this._ticksPerBeat != value)
//                {
//                    if (value < 1)
//                        throw new ArgumentException(nameof(value));

//                    var old = this._ticksPerBeat;
//                    this._ticksPerBeat = value;

//                    double p1 = (double)this._ticks / (double)old;
//                    this._ticks = (int)Math.Round(p1 * value, MidpointRounding.AwayFromZero);
//                }
//            }
//        }

//        /// <summary>
//        /// Get or set the bar part of current <see cref="BarBeatTick"/> structure.
//        /// </summary>
//        /// <exception cref="ArgumentException">Value less than 0.</exception>
//        public int Bars
//        {
//            get => this._bars;
//            set
//            {
//                if (this._bars != value)
//                {
//                    if (value < 0)
//                        throw new ArgumentException(nameof(value));
//                    this._bars = value;
//                }
//            }
//        }

//        /// <summary>
//        /// Get or set the beat part of current <see cref="BarBeatTick"/> structure.
//        /// </summary>
//        /// <exception cref="ArgumentException">Value less than 0.</exception>
//        public int Beats
//        {
//            get => this._beats;
//            set
//            {
//                if (this._beats != value)
//                {
//                    if (value < 0)
//                        throw new ArgumentException(nameof(value));

//                    this.CheckCarryBit(ref value, ref this._bars, this._beatsPerBar);
//                    this._beats = value;
//                }
//            }
//        }

//        /// <summary>
//        /// Get or set the tick part of current <see cref="BarBeatTick"/> structure.
//        /// </summary>   
//        /// <exception cref="ArgumentException">Value less than 0.</exception>
//        public int Ticks
//        {
//            get => this._ticks;
//            set
//            {
//                if (this._ticks != value)
//                {
//                    if (value < 0)
//                        throw new ArgumentException(nameof(value));

//                    this.CheckCarryBit(ref value, ref this._beats, this._ticksPerBeat);
//                    this.CheckCarryBit(ref this._beats, ref this._bars, this._beatsPerBar);
//                    this._ticks = value;
//                }
//            }
//        }

//        public int TotalTicks
//        {
//            get
//            {
//                return this._bars * this._beatsPerBar * this._ticksPerBeat + this._beats * this._ticksPerBeat + this._ticks;
//            }
//        }

//        /// <summary>
//        /// Initialize a new instance with a zero <see cref="BarBeatTick"/> value.
//        /// </summary>
//        /// <param name="beatsPerBar">The number of beats per bar.</param>
//        /// <param name="ticksPerBeat">The number of ticks per bar.</param>
//        public BarBeatTick(int beatsPerBar, int ticksPerBeat)
//        {
//            this._bars = this._beats = this._ticks = 0;
//            this._beatsPerBar = Math.Max(1, beatsPerBar);
//            this._ticksPerBeat = Math.Max(1, ticksPerBeat);
//        }


//        /// <summary>
//        /// Initialize a new instance with a zero <see cref="BarBeatTick"/> value.
//        /// </summary>
//        /// <param name="beatsPerBar">The number of beats per bar.</param>
//        /// <param name="ticksPerBeat">The number of ticks per bar.</param>
//        /// <param name="bars">The number of bars.</param>
//        /// <param name="beats">The number of beats.</param>
//        /// <param name="ticks">The number of ticks.</param>
//        public BarBeatTick(int beatsPerBar, int ticksPerBeat, int bars, int beats, int ticks)
//            : this(beatsPerBar, ticksPerBeat)
//        {
//            this.Bars = bars;
//            this.Beats = beats;
//            this.Ticks = ticks;
//        }

//        public override string ToString()
//        {
//            return $"{this.Bars + 1}:{this.Beats + 1}:{this.Ticks}";
//        }

//        /// <summary>
//        /// Convert the current <see cref="BarBeatTick"/> instance to an equal value in <see cref="TimeSpan"/>, 
//        /// according to <paramref name="bpm"/> (beats per minute).
//        /// </summary>
//        /// <param name="bpm">Given beats per minute.</param>
//        public TimeSpan ToTimeSpan(int bpm)
//        {
//            double totalBeats = this.BeatsPerBar * this.Bars + this.Beats + (double)this.Ticks / (double)this.TicksPerBeat;
//            return BeatTimeCalculate.ActualTime(totalBeats, bpm);
//        }

//        /// <summary>
//        /// Convert a given <see cref="BarBeatTick"/> instance <paramref name="bbt"/> to an equal value in <see cref="TimeSpan"/>, 
//        /// according to <paramref name="bpm"/> (beats per minute).
//        /// </summary>
//        /// <param name="bbt">Given <see cref="BarBeatTick"/> instance.</param>
//        /// <param name="bpm">Given beats per minute.</param>
//        public static TimeSpan ToTimeSpan(BarBeatTick bbt, int bpm)
//        {
//            return bbt.ToTimeSpan(bpm);
//        }

//        public static bool operator ==(BarBeatTick value1, BarBeatTick value2)
//        {
//            return value1.BeatsPerBar == value2.BeatsPerBar &&
//                value1.TicksPerBeat == value2.TicksPerBeat &&
//                value1.Bars == value2.Bars &&
//                value1.Beats == value2.Beats &&
//                value1.Ticks == value2.Ticks;

//        }

//        public static bool operator !=(BarBeatTick value1, BarBeatTick value2)
//        {
//            return !(value1 == value2);
//        }


//        private void CheckCarryBit(ref int secondary, ref int primary, int sign)
//        {
//            while (secondary >= sign)
//            {
//                secondary -= sign;
//                primary++;
//            }
//        }
//    }
//}
