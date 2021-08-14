using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunimoStudio
{
    [Flags]
    public enum InstrumentCategory
    {
        None = 0,
        Keyboards = 1,
        Strings = 2,
        Percussion = 4,
        Brass = 8,
        Woodwind = 16,
        Guitar = 32,
        All = Keyboards | Strings | Percussion | Brass | Woodwind | Guitar
    }
}
