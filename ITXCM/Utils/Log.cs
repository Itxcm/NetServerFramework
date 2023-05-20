using log4net;
namespace ITXCM
{
    public static class Log
    {
        private static ILog log;

        public static void Init(string name) => log = LogManager.GetLogger(name);

        public static void Info(object message) => log.Info(message);

        public static void InfoFormat(string format, params object[] args) => log.InfoFormat(format, args);

        public static void Warning(object message) => log.Warn(message);

        public static void WarningFormat(string format, params object[] args) => log.WarnFormat(format, args);

        public static void Error(object message) => log.Error(message);

        public static void ErrorFormat(string format, params object[] args) => log.ErrorFormat(format, args);

        public static void Fatal(object message) => log.Fatal(message);

        public static void FatalFormat(string format, params object[] args) => log.FatalFormat(format, args);
    }
}