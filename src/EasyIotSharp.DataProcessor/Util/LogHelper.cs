﻿using log4net;
using System;
using System.Collections.Generic;
using System.Text;
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log.xml", Watch = false)]
namespace EasyIotSharp.DataProcessor.Util
{
    public class LogHelper
    {
        private static ILog log = log4net.LogManager.GetLogger("LogFileAppender");
        private static readonly ILog historyLog = log4net.LogManager.GetLogger("HistoryDataAppender");
        // 添加一个 SQL 日志记录的方法
        public static void LogSql(string sql, object parameters)
        {
            // 使用 Info 或 Debug 级别记录 SQL 和参数
            log.Info("Executing SQL: " + sql);
            log.Info("With Parameters: " + parameters.ToString());
        }

        #region Debug

        /// <summary>

        /// 调试

        /// </summary>

        /// <param name="message"></param>

        public static void Debug(object message)
        {

            log.Debug(message);

        }

        public static void Debug(object message, Exception exp)
        {

            log.Debug(message, exp);

        }

        public static void DebugFormat(string format, object arg0)
        {

            log.DebugFormat(format, arg0);

        }

        public static void DebugFormat(string format, params object[] args)
        {

            log.DebugFormat(format, args);

        }

        public static void DebugFormat(IFormatProvider provider, string format, params object[] args)
        {

            log.DebugFormat(provider, format, args);

        }

        public static void DebugFormat(string format, object arg0, object arg1)
        {

            log.DebugFormat(format, arg0, arg1);

        }

        public static void DebugFormat(string format, object arg0, object arg1, object arg2)
        {

            log.DebugFormat(format, arg0, arg1, arg2);

        }

        #endregion

        #region Error

        /// <summary>

        /// 错误

        /// </summary>

        /// <param name="message"></param>

        public static void Error(object message)
        {
            // log.Info(ProjectBaseInfo.DBPath);
            log.Error(message);

        }

        public static void Error(object message, Exception exception)
        {

            log.Error(message, exception);

        }

        public static void ErrorFormat(string format, object arg0)
        {

            log.ErrorFormat(format, arg0);

        }

        public static void ErrorFormat(string format, params object[] args)
        {

            log.ErrorFormat(format, args);

        }

        public static void ErrorFormat(IFormatProvider provider, string format, params object[] args)
        {

            log.ErrorFormat(provider, format, args);

        }

        public static void ErrorFormat(string format, object arg0, object arg1)
        {

            log.ErrorFormat(format, arg0, arg1);

        }

        public static void ErrorFormat(string format, object arg0, object arg1, object arg2)
        {

            log.ErrorFormat(format, arg0, arg1, arg2);

        }

        #endregion

        #region Fatal

        /// <summary>

        /// 致命的,毁灭性的

        /// </summary>

        /// <param name="message"></param>

        public static void Fatal(object message)
        {

            log.Fatal(message);

        }

        public static void Fatal(object message, Exception exception)
        {

            log.Fatal(message, exception);

        }

        public static void FatalFormat(string format, object arg0)
        {

            log.FatalFormat(format, arg0);

        }

        public static void FatalFormat(string format, params object[] args)
        {

            log.FatalFormat(format, args);

        }

        public static void FatalFormat(IFormatProvider provider, string format, params object[] args)
        {

            log.FatalFormat(provider, format, args);

        }

        public static void FatalFormat(string format, object arg0, object arg1)
        {

            log.FatalFormat(format, arg0, arg1);

        }

        public static void FatalFormat(string format, object arg0, object arg1, object arg2)
        {

            log.FatalFormat(format, arg0, arg1, arg2);

        }

        #endregion

        #region Info

        /// <summary>

        /// 信息

        /// </summary>

        /// <param name="message"></param>

        public static void Info(object message)
        {
            //log.Info(ProjectBaseInfo.DBPath);
            log.Info(message);

        }

        public static void Info(object message, Exception exception)
        {

            log.Info(message, exception);

        }

        public static void InfoFormat(string format, object arg0)
        {

            log.InfoFormat(format, arg0);

        }

        public static void InfoFormat(string format, params object[] args)
        {

            log.InfoFormat(format, args);

        }

        public static void InfoFormat(IFormatProvider provider, string format, params object[] args)
        {

            log.InfoFormat(provider, format, args);

        }

        public static void InfoFormat(string format, object arg0, object arg1)
        {

            log.InfoFormat(format, arg0, arg1);

        }

        public static void InfoFormat(string format, object arg0, object arg1, object arg2)
        {

            log.InfoFormat(format, arg0, arg1, arg2);

        }

        #endregion

        #region Warn

        /// <summary>

        /// 警告,注意,通知

        /// </summary>

        /// <param name="message"></param>

        public static void Warn(object message)
        {

            log.Warn(message);

        }

        public static void Warn(object message, Exception exception)
        {

            log.Warn(message, exception);

        }

        public static void WarnFormat(string format, object arg0)
        {

            log.WarnFormat(format, arg0);

        }

        public static void WarnFormat(string format, params object[] args)
        {

            log.WarnFormat(format, args);

        }

        public static void WarnFormat(IFormatProvider provider, string format, params object[] args)
        {

            log.WarnFormat(provider, format, args);

        }

        public static void WarnFormat(string format, object arg0, object arg1)
        {

            log.WarnFormat(format, arg0, arg1);

        }

        public static void WarnFormat(string format, object arg0, object arg1, object arg2)
        {

            log.WarnFormat(format, arg0, arg1, arg2);

        }

        #endregion

        /// <summary>
        /// 历史数据存储
        /// </summary>
        /// <param name="message">message</param>
        public static void HistoryData(object message)
        {
            historyLog.Fatal(message);
        }
    }
}
