using log4net;
using UnityEngine;

namespace ITXCM
{
    /// <summary>
    /// 通用日志输出类 重写Unity日志
    /// </summary>
    public class UnityLogger
    {
        public static void Init()
        {
            Application.logMessageReceived += OnLogMessageReceived;
            Log.Init("Unity");
        }

        public static ILog log = LogManager.GetLogger("Unity");

        private static void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            switch (type)
            {
                case LogType.Error:
                    log.ErrorFormat("{0}\r\n{1}", condition, stackTrace.Replace("\n", "\r\n"));
                    break;

                case LogType.Assert:
                    log.DebugFormat("{0}\r\n{1}", condition, stackTrace.Replace("\n", "\r\n"));
                    break;

                case LogType.Exception:
                    log.FatalFormat("{0\r\n{1}", condition, stackTrace.Replace("\n", "\r\n"));
                    break;

                case LogType.Warning:
                    log.WarnFormat("{0}\r\n{1}", condition, stackTrace.Replace("\n", "\r\n"));
                    break;

                default:
                    log.Info(condition);
                    break;
            }
        }
    }
}