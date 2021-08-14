using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JunimoStudio.Core;

namespace JunimoStudio
{
    internal static class BeatTimeCalculate
    {
        /// <summary>
        /// Calculate actual time of certain beats.
        /// </summary>
        /// <param name="beats">The number of beats.</param>
        /// <param name="bpm">Tempo. (beats per minute)</param>
        /// <returns>Total time of the beats.</returns>
        public static TimeSpan ActualTime(double beats, int bpm)
        {
            double secondsPerBeat = 60d / bpm;
            double totalSeconds = secondsPerBeat * beats;
            return new TimeSpan((long)(totalSeconds * TimeSpan.TicksPerSecond));
        }

        /// <summary>
        /// Calculate actual time of one bar.
        /// </summary>
        /// <param name="bpm">Beats per minute.</param>
        /// <param name="timeSignature">Time signature.</param>
        /// <returns>Total time per bar.</returns>
        public static TimeSpan ActualTime(int bpm, ITimeSignature timeSignature)
        {
            int beatsPerBar = timeSignature.Numerator;
            double secondsPerBeat = 60d / bpm;
            double secondsPerBar = secondsPerBeat * beatsPerBar;
            return new TimeSpan((long)(secondsPerBar * TimeSpan.TicksPerSecond));
        }
    }
}
