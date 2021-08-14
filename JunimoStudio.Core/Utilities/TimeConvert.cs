using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunimoStudio.Core.Utilities
{
    public static class TimeConvert
    {
        public static int FromBarsToTicks(int bars, ITimeBasedObject settings)
        {
            int beatsPerBar = settings.TimeSignature.Numerator;
            return FromBeatsToTicks(beatsPerBar, settings);
        }

        public static int FromBeatsToTicks(int beats, ITimeBasedObject settings)
        {
            int tpq = settings.TicksPerQuarterNote;
            double quarterNotesPerBeat
                = 4d / settings.TimeSignature.Denominator;
            int ticksPerBeat = (int)(quarterNotesPerBeat * tpq);
            return (ticksPerBeat * beats);
        }

    }
}
