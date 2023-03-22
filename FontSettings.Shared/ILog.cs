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
}
