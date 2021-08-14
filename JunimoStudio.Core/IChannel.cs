using JunimoStudio.Core.Plugins.Instruments;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace JunimoStudio.Core
{
    public interface IChannel : IDisposable
    {
        string Name { get; set; }

        INoteCollection Notes { get; }

        /// <summary>
        /// Get or set whether current channel is mute. <see langword="true"/> to mute the channel, <see langword="false"/> to unmute the channel.
        /// </summary>
        bool Mute { get; set; }

        IInstrumentPlugin Generator { get; set; }

        int EffectTrack { get; set; }
    }
}
