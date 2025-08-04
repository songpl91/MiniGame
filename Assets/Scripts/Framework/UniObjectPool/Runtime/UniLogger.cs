using UnityEngine;

namespace UniFramework.ObjectPool
{
    /// <summary>
    /// 对象池系统日志记录器
    /// 提供统一的日志输出接口
    /// </summary>
    internal static class UniLogger
    {
        /// <summary>
        /// 是否启用日志输出
        /// </summary>
        public static bool EnableLog { get; set; } = true;

        /// <summary>
        /// 日志标签
        /// </summary>
        private const string LOG_TAG = "[UniObjectPool]";

        /// <summary>
        /// 输出普通日志
        /// </summary>
        /// <param name="message">日志消息</param>
        public static void Log(string message)
        {
            if (!EnableLog)
                return;

            Debug.Log($"{LOG_TAG} {message}");
        }

        /// <summary>
        /// 输出警告日志
        /// </summary>
        /// <param name="message">警告消息</param>
        public static void Warning(string message)
        {
            if (!EnableLog)
                return;

            Debug.LogWarning($"{LOG_TAG} {message}");
        }

        /// <summary>
        /// 输出错误日志
        /// </summary>
        /// <param name="message">错误消息</param>
        public static void Error(string message)
        {
            if (!EnableLog)
                return;

            Debug.LogError($"{LOG_TAG} {message}");
        }

        /// <summary>
        /// 输出异常日志
        /// </summary>
        /// <param name="exception">异常对象</param>
        public static void Exception(System.Exception exception)
        {
            if (!EnableLog)
                return;

            Debug.LogException(exception);
        }

        /// <summary>
        /// 输出格式化日志
        /// </summary>
        /// <param name="format">格式字符串</param>
        /// <param name="args">参数</param>
        public static void LogFormat(string format, params object[] args)
        {
            if (!EnableLog)
                return;

            Debug.LogFormat($"{LOG_TAG} {format}", args);
        }

        /// <summary>
        /// 输出格式化警告日志
        /// </summary>
        /// <param name="format">格式字符串</param>
        /// <param name="args">参数</param>
        public static void WarningFormat(string format, params object[] args)
        {
            if (!EnableLog)
                return;

            Debug.LogWarningFormat($"{LOG_TAG} {format}", args);
        }

        /// <summary>
        /// 输出格式化错误日志
        /// </summary>
        /// <param name="format">格式字符串</param>
        /// <param name="args">参数</param>
        public static void ErrorFormat(string format, params object[] args)
        {
            if (!EnableLog)
                return;

            Debug.LogErrorFormat($"{LOG_TAG} {format}", args);
        }
    }
}