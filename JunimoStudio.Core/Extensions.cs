using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunimoStudio.Core
{
    public static class Extensions
    {
        /// <summary>
        /// 工具方法，将给定的<paramref name="ticks"/>换算成毫秒。
        /// </summary>
        /// <param name="tbo"></param>
        /// <param name="ticks"></param>
        /// <returns></returns>
        public static int TicksToMilliseconds(this ITimeBasedObject tbo, long ticks)
        {
            double totalQuarterNotes = ticks / (double)tbo.TicksPerQuarterNote;
            double totalBeats = tbo.TimeSignature.Denominator / 4d * totalQuarterNotes;
            double beatsPerMs = tbo.Bpm / 60000d;
            return (int)(totalBeats / beatsPerMs);
        }

        /// <summary>
        /// 工具方法，将给定的毫秒<paramref name="ms"/>换算成ticks。
        /// </summary>
        /// <param name="tbo"></param>
        /// <param name="ticks"></param>
        /// <returns></returns>
        public static long MillisecondsToTicks(this ITimeBasedObject tbo, int ms)
        {
            double beatsPerMs = tbo.Bpm / 60000d;
            double totalBeats = ms * beatsPerMs;
            double totalQuarterNotes = totalBeats * 4d / tbo.TimeSignature.Denominator;
            return (long)(totalQuarterNotes * tbo.TicksPerQuarterNote);
        }
    }
}
