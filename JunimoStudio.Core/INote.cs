using System.Collections.Generic;
using NAudio.Midi;

namespace JunimoStudio.Core
{
    /// <summary>表示一个音符。</summary>
    public interface INote : ITimeBasedObject
    {
        /// <summary>音高，范围0 ~ 127，从C0到G10。中央C为60，即C5。</summary>
        int Number { get; set; }

        /// <summary>力度值，范围0 ~ 127。</summary>
        int Velocity { get; set; }

        /// <summary>声像，范围0 ~ 127，0为最左，127为最右。63是中间。</summary>
        int Pan { get; set; }

        /// <summary>开始时间，单位为ticks。</summary>
        long Start { get; set; }

        /// <summary>时长，持续时间，单位为ticks。</summary>
        int Duration { get; set; }

        List<MidiEvent> ToMidiEvents();
    }
}
