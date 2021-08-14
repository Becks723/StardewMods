using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JunimoStudio.Core.Framework;
using JunimoStudio.Core.Plugins.Instruments;

namespace JunimoStudio.Core
{
    public class MultimediaPlayback : IPlayback
    {
        protected enum State
        {
            Playing,
            Paused,
            Stopped
        }

        protected readonly ITimeBasedObject _timeSettingsImpl;
        protected readonly MultimediaTimer _timer;
        protected int _msPassed;
        protected State _state;
        public event EventHandler<int> Ticked;

        public MultimediaPlayback(int interval, ITimeBasedObject timeSettings)
        {
            _state = State.Stopped;
            _timer = new MultimediaTimer(interval, OnTicked);
            _timeSettingsImpl = timeSettings ?? new TimeBasedObject();
        }

        #region IPlayback Members

        public virtual void Pause()
        {
            if (_state == State.Paused)
                return;

            _timer.Stop();
            _state = State.Paused;
        }

        public virtual void SeekTo(long ticks)
        {
            _msPassed = _timeSettingsImpl.TicksToMilliseconds(ticks);

            if (_msPassed > 0 && _state == State.Stopped)
                _state = State.Paused;
        }

        public virtual void StartPlayback()
        {
            if (_state == State.Playing)
                return;

            _timer.Start();
            _state = State.Playing;
        }

        public virtual void Stop()
        {
            if (_state == State.Stopped)
                return;

            _timer.Stop();
            _msPassed = 0;
            _state = State.Stopped;
        }
        #endregion

        protected virtual void OnTicked()
        {
            var handler = Ticked;
            if (handler != null)
                handler.Invoke(this, _msPassed);
        }
    }
}
