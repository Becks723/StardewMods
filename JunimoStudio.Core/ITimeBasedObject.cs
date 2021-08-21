using System.ComponentModel;

namespace JunimoStudio.Core
{
    /// <summary>表示一个受若干时间因素影响的对象。</summary>
    public interface ITimeBasedObject : INotifyPropertyChanged
    {
        /// <summary>一个四分音符内走过的单位刻（ticks）数。</summary>
        int TicksPerQuarterNote { get; set; }

        /// <summary>一分钟内走过的节拍数。</summary>
        int Bpm { get; set; }

        /// <summary>拍号。</summary>
        ITimeSignature TimeSignature { get; }
    }
}
