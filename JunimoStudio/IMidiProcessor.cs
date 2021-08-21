using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Midi;

namespace JunimoStudio
{
    /// <summary>用于处理流入的midi信号。</summary>
    public interface IMidiProcessor
    {
        void Process(IList<MidiMessage> messages);
    }
}
