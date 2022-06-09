using StardewModdingAPI;

namespace FontSettings.Framework
{
    internal interface ILog
    {
        public static void Trace(string message) => Log.Instance.TraceImpl(message);
        public static void Debug(string message) => Log.Instance.DebugImpl(message);
        public static void Info(string message) => Log.Instance.InfoImpl(message);
        public static void Error(string message) => Log.Instance.ErrorImpl(message);
        public static void Warn(string message) => Log.Instance.WarnImpl(message);
        public static void Alert(string message) => Log.Instance.AlertImpl(message);

        public void TraceImpl(string message);

        public void DebugImpl(string message);

        public void InfoImpl(string message);

        public void ErrorImpl(string message);

        public void WarnImpl(string message);

        public void AlertImpl(string message);
    }

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
