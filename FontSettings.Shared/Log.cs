using StardewModdingAPI;

namespace FontSettings.Framework
{

    internal class Log : ILog
    {
        public static Log Instance { get; private set; }

        public static void Init(IMonitor monitor)
        {
            Instance = new Log(monitor);
        }

        private static IMonitor _monitor;

        private Log(IMonitor monitor)
        {
            _monitor = monitor;
        }

        public void TraceImpl(string message) => _monitor?.Log(message, LogLevel.Trace);
        public void DebugImpl(string message) => _monitor?.Log(message, LogLevel.Debug);
        public void InfoImpl(string message) => _monitor?.Log(message, LogLevel.Info);
        public void ErrorImpl(string message) => _monitor?.Log(message, LogLevel.Error);
        public void WarnImpl(string message) => _monitor?.Log(message, LogLevel.Warn);
        public void AlertImpl(string message) => _monitor?.Log(message, LogLevel.Alert);
    }
}
