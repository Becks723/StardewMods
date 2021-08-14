using System;
using System.Runtime.InteropServices;

namespace JunimoStudio.Core
{
    /// <summary>
    /// An accurate timer class that wraps timeSetEvent() function from the mutimeida API.
    /// </summary>
    /// <remarks>
    /// https://stackoverflow.com/a/31612770/14344451
    /// https://docs.microsoft.com/en-us/previous-versions/dd757634(v=vs.85)
    /// </remarks>
    public class MultimediaTimer
    {
        private delegate void TimerEventDel(int id, int msg, IntPtr user, int dw1, int dw2);
        private const int TIME_PERIODIC = 1;
        private const int EVENT_TYPE = TIME_PERIODIC;// + 0x100;  // TIME_KILL_SYNCHRONOUS causes a hang ?!

        [DllImport("winmm.dll")]
        private static extern int timeBeginPeriod(int msec);

        [DllImport("winmm.dll")]
        private static extern int timeEndPeriod(int msec);

        [DllImport("winmm.dll")]
        private static extern int timeSetEvent(int delay, int resolution, TimerEventDel handler, IntPtr user, int eventType);

        [DllImport("winmm.dll")]
        private static extern int timeKillEvent(int id);

        private readonly int mDelay;
        private readonly Action mAction;
        private int mTimerId;
        private readonly TimerEventDel mHandler;  // NOTE: declare at class scope so garbage collector doesn't release it!!!

        /// <summary>
        /// Initialize a new instance of <see cref="MultimediaTimer"/>.
        /// </summary>
        /// <param name="delay">Event delay, in milliseconds.</param>
        /// <param name="action">Pointer to a callback function that is called periodically upon expiration of periodic events.</param>
        public MultimediaTimer(int delay, Action action)
        {
            mDelay = delay;
            mAction = action;
            mHandler = new TimerEventDel(TimerCallback);
        }

        public void Start()
        {
            timeBeginPeriod(1); // 1: about 30ms delay
            mTimerId = timeSetEvent(mDelay, 0, mHandler, IntPtr.Zero, EVENT_TYPE);
        }

        public void Stop()
        {
            int err = timeKillEvent(mTimerId);
            timeEndPeriod(1);
            System.Threading.Thread.Sleep(100);// Ensure callbacks are drained
        }

        private void TimerCallback(int id, int msg, IntPtr user, int dw1, int dw2)
        {
            if (mTimerId != 0)
                mAction?.Invoke();
        }
    }
}
