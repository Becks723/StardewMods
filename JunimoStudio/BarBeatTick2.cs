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
//    public struct BarBeatTick2
//    {
//        private readonly Func<int> _fbeatsPerBar;

//        private readonly Func<int> _fticksPerBeat;

//        private int _lastTicksPerBeat;

//        private int _bars;

//        private int _beats;

//        private int _ticks;

//        ///// <summary>
//        ///// Get or set the number of beats per bar.
//        ///// </summary>
//        ///// <exception cref="ArgumentException">Value less than 1.</exception>
//        //public int BeatsPerBar
//        //{
//        //    get => this._beatsPerBar;
//        //    set
//        //    {
//        //        if (this._beatsPerBar != value)
//        //        {
//        //            if (value < 1)
//        //                throw new ArgumentException(nameof(value));

//        //            this._beatsPerBar = value;

//        //            this.CheckCarryBit(ref this._beats, ref this._bars, value);
//        //        }
//        //    }
//        //}

//        ///// <summary>
//        ///// 一拍内走过的单位刻（ticks）数。
//        ///// </summary>
//        //public int TicksPerBeat
//        //{
//        //    get => this._ticksPerBeat;
//        //    set
//        //    {
//        //        if (this._ticksPerBeat != value)
//        //        {
//        //            if (value < 1)
//        //                throw new ArgumentException(nameof(value));

//        //            var old = this._ticksPerBeat;
//        //            this._ticksPerBeat = value;

//        //            double p1 = (double)this._ticks / (double)old;
//        //            this._ticks = (int)Math.Round(p1 * value, MidpointRounding.AwayFromZero);
//        //        }
//        //    }
//        //}

//        /// <summary>当前<see cref="BarBeatTick2"/>结构中“小节”位的值。</summary>
//        /// <exception cref="ArgumentException">Value less than 0.</exception>
//        public int Bars
//        {
//            get => this._bars;
//            set
//            {
//                if (value < 0)
//                    throw new ArgumentOutOfRangeException(nameof(value));
//                this._bars = value;
//            }
//        }

//        /// <summary>当前<see cref="BarBeatTick2"/>结构中“拍”位的值。</summary>
//        /// <exception cref="ArgumentException">Value less than 0.</exception>
//        public int Beats
//        {
//            get
//            {
//                this.CheckCarryBit(ref this._beats, ref this._bars, this._fbeatsPerBar());
//                return this._beats;
//            }
//            set
//            {
//                if (value < 0)
//                    throw new ArgumentOutOfRangeException(nameof(value));

//                this.CheckCarryBit(ref value, ref this._bars, this._fbeatsPerBar());
//                this._beats = value;
//            }
//        }

//        /// <summary>当前<see cref="BarBeatTick2"/>结构中“单位刻（ticks）”位的值。</summary>
//        /// <exception cref="ArgumentException">Value less than 0.</exception>
//        public int Ticks
//        {
//            get
//            {
//                int tpb = this._fticksPerBeat();
//                if (tpb != this._lastTicksPerBeat)
//                {
//                    double p1 = (double)this._ticks / (double)this._lastTicksPerBeat;
//                    this._ticks = (int)Math.Round(p1 * tpb, MidpointRounding.AwayFromZero);

//                    this._lastTicksPerBeat = tpb;
//                }
//                this.CheckCarryBit(ref this._ticks, ref this._beats, this._fticksPerBeat());
//                this.CheckCarryBit(ref this._beats, ref this._bars, this._fbeatsPerBar());
//                return this._ticks;
//            }
//            set
//            {
//                if (value < 0)
//                    throw new ArgumentOutOfRangeException(nameof(value));

//                this.CheckCarryBit(ref value, ref this._beats, this._fticksPerBeat());
//                this.CheckCarryBit(ref this._beats, ref this._bars, this._fbeatsPerBar());
//                this._ticks = value;
//            }
//        }

//        public int TotalTicks
//        {
//            get
//            {
//                return
//                    this.Bars * this._fbeatsPerBar() * this._fticksPerBeat()
//                    + this.Beats * this._fticksPerBeat()
//                    + this.Ticks;
//            }
//        }

//        public BarBeatTick2(Func<int> beatsPerBar, Func<int> ticksPerBeat)
//            : this(beatsPerBar, ticksPerBeat, 0, 0, 0)
//        {
//        }

//        public BarBeatTick2(Func<int> beatsPerBar, Func<int> ticksPerBeat, int bars, int beats, int ticks)
//        {
//            this._bars = Math.Max(0, bars);
//            this._beats = Math.Max(0, beats);
//            this._ticks = Math.Max(0, ticks);
//            this._fbeatsPerBar = ValidateBeatsPerBar(beatsPerBar);
//            this._fticksPerBeat = ValidateTicksPerBeat(ticksPerBeat);
//            this._lastTicksPerBeat = this._fticksPerBeat();
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

//        public static bool operator ==(BarBeatTick2 value1, BarBeatTick2 value2)
//        {
//            return
//                value1.TotalTicks == value2.TotalTicks;
//        }

//        public static bool operator !=(BarBeatTick2 value1, BarBeatTick2 value2)
//        {
//            return !(value1 == value2);
//        }

//        public static bool operator >(BarBeatTick2 value1, BarBeatTick2 value2)
//        {
//            return 
//                value1.TotalTicks > value2.TotalTicks;
//        }

//        public static bool operator <(BarBeatTick2 value1, BarBeatTick2 value2)
//        {
//            return !(value1 == value2 || value1 > value2);
//        }

//        public static bool operator >=(BarBeatTick2 value1, BarBeatTick2 value2)
//        {
//            return value1 == value2 || value1 > value2;
//        }

//        public static bool operator <=(BarBeatTick2 value1, BarBeatTick2 value2)
//        {
//            return value1 == value2 || value1 < value2;
//        }

//        /// <summary>
//        /// 进位。
//        /// </summary>
//        /// <param name="thisBit">当前位的值。</param>
//        /// <param name="nextBit">下一位的值。</param>
//        /// <param name="sign">进制。</param>
//        private void CheckCarryBit(ref int thisBit, ref int nextBit, int sign)
//        {
//            while (thisBit >= sign)
//            {
//                thisBit -= sign;
//                nextBit++;
//            }
//        }

//        private static Func<int> ValidateBeatsPerBar(Func<int> beatsPerBar)
//        {
//            if (beatsPerBar == null)
//                throw new ArgumentNullException(nameof(beatsPerBar));

//            return () =>
//            {
//                int val = beatsPerBar();
//                if (val <= 0)
//                    throw new ArgumentOutOfRangeException(nameof(beatsPerBar));
//                return val;
//            };
//        }

//        private static Func<int> ValidateTicksPerBeat(Func<int> ticksPerBeat)
//        {
//            if (ticksPerBeat == null)
//                throw new ArgumentNullException(nameof(ticksPerBeat));

//            return () =>
//            {
//                int val = ticksPerBeat();
//                if (val <= 0)
//                    throw new ArgumentOutOfRangeException(nameof(ticksPerBeat));
//                return val;
//            };
//        }
//    }
//}
