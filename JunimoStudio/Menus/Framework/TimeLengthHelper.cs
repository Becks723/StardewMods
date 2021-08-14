using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JunimoStudio.Core;
using JunimoStudio.Core.Utilities;

namespace JunimoStudio.Menus.Framework
{
    /// <summary>工具类，用于将一些时间的设置转换成一些UI显示的信息。</summary>
    internal static class TimeLengthHelper
    {
        /// <summary>获取一拍的长度。</summary>
        /// <param name="time">时间设置。</param>
        /// <param name="tickLength">单位tick的长度。</param>
        /// <returns></returns>
        public static float GetBeatLength(ITimeBasedObject time, float tickLength)
        {
            return TimeConvert.FromBeatsToTicks(1, time) * tickLength;
        }

        /// <summary>获取一小节的长度。</summary>
        /// <param name="time">时间设置。</param>
        /// <param name="tickLength">单位tick的长度。</param>
        /// <returns></returns>
        public static float GetBarLength(ITimeBasedObject time, float tickLength)
        {
            return TimeConvert.FromBarsToTicks(1, time) * tickLength;
        }

        /// <summary>获取钢琴卷帘当前最小精度之间的长度。</summary>
        /// <param name="time">时间设置。</param>
        /// <param name="pianoRollConfig">钢琴卷帘设置。</param>
        /// <param name="tickLength">单位tick的长度。</param>
        /// <returns></returns>
        public static float GetLengthBetweenTwoGridLines(ITimeBasedObject time, PianoRollConfig pianoRollConfig, float tickLength)
        {
            return (GetTicksBetweenTwoGridLines(time, pianoRollConfig) * tickLength);
        }

        /// <summary>获取钢琴卷帘当前最小精度之间的tick数。</summary>
        /// <param name="time">时间设置。</param>
        /// <param name="pianoRollConfig">钢琴卷帘设置。</param>
        /// <returns></returns>
        public static int GetTicksBetweenTwoGridLines(ITimeBasedObject time, PianoRollConfig pianoRollConfig)
        {
            int tpq = time.TicksPerQuarterNote;
            int quarterNotesPerBeat
                = 4 / time.TimeSignature.Denominator;
            int ticksPerBeat = tpq * quarterNotesPerBeat;

            int ticksPerTwoGridlines = 0;
            switch (pianoRollConfig.Grid)
            {
                case GridResolution.Bar:
                    ticksPerTwoGridlines = ticksPerBeat * time.TimeSignature.Numerator;
                    break;
                case GridResolution.Beat:
                    ticksPerTwoGridlines = ticksPerBeat;
                    break;
                case GridResolution.HalfBeat:
                    ticksPerTwoGridlines = ticksPerBeat / 2;
                    break;
                case GridResolution.OneThirdBeat:
                    ticksPerTwoGridlines = ticksPerBeat / 3;
                    break;
                case GridResolution.QuarterBeat:
                    ticksPerTwoGridlines = ticksPerBeat / 4;
                    break;
                case GridResolution.OneSixthBeat:
                    ticksPerTwoGridlines = ticksPerBeat / 6;
                    break;
            }

            return ticksPerTwoGridlines;
        }
    }
}
