using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JunimoStudio.Core.Plugins.Instruments;
using NAudio.Midi;

namespace JunimoStudio.Core
{
    /// <summary>A framwork which provides basic playback functions.</summary>
    public interface IPlayback
    {
        void StartPlayback();

        void Pause();

        void Stop();

        void SeekTo(long ticks);
    }
}
