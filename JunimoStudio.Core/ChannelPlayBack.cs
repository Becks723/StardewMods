using System.Collections.Generic;
using System.Linq;

namespace JunimoStudio.Core
{
    public class ChannelPlayBack : MultimediaPlayback
    {
        private readonly IEnumerable<IChannel> _channels;

        public ChannelPlayBack(ITimeBasedObject timeSettings, IChannel channel)
            : this(timeSettings, new[] { channel })
        {
        }

        public ChannelPlayBack(ITimeBasedObject timeSettings, IEnumerable<IChannel> channels)
            : base(1, timeSettings)
        {
            _channels = channels;
            Ticked += (s, msPassed) =>
            {
                foreach (IChannel channel in _channels)
                {
                    // 一个channel里所有notes转化成midi信号。
                    var events = channel.Notes.SelectMany(n => n.ToMidiEvents());

                    // 此轮将要处理的midi信号。
                    var toDo = events
                        .Where(evnt => evnt.AbsoluteTime == _timeSettingsImpl.MillisecondsToTicks(msPassed))
                        .ToList();

                    // 处理midi信号。
                    channel.Generator?.ProcessMidi(toDo);
                }

                _msPassed++;

                StopIfEnd(_msPassed);
            };
        }

        /// <summary>当播放结束时，自动停止。</summary>
        /// <param name="msPassed">调用此方法时走过的毫秒数。</param>
        private void StopIfEnd(int msPassed)
        {
            // 其实停止就是把_state调成Stopped。
            // 所谓播放结束，就是当所有notes中最晚那个停止播放的音符停止时，就算结束。
            // 而最晚停止播放的那个音符，具体表现为 开始时间+时长 最大。这个值其实也就是整个播放一次所花的时间。

            // 所有notes。
            var allNotes = _channels.SelectMany(c => c.Notes);

            // 播放一次所花的时间（ticks）。
            long totalTicks = allNotes.Count() == 0
                ? 0
                : allNotes.Max(n => n.Start + n.Duration);

            // 换算成以毫秒为单位。
            int totalMs = _timeSettingsImpl.TicksToMilliseconds(totalTicks);

            // 满足条件。
            if (msPassed >= totalMs)
            {
                // 停止。
                Stop();
            }
        }

    }
}
