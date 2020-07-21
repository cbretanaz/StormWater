using System;
using System.Diagnostics;
using System.IO;
using cfgMgr = System.Configuration.ConfigurationManager;
using lib = CoP.Enterprise.Utilities;
using NLog;
using NLog.Targets;
using nLogMgr = NLog.LogManager;

namespace CoP.Enterprise 
{
    public static class DALLog
    {
        private static string DefaultLogName
        {
            get
            { 
                var defLogNm = cfgMgr.AppSettings["NTLogName"];
                return string.IsNullOrEmpty(defLogNm) ? "SCA" : defLogNm;
            }
        }
        private static string DefaultSourceName => cfgMgr.AppSettings["NTSourceName"];

        private static ILogger Logger => nLogMgr.GetLogger(cfgMgr.AppSettings["nLogger"]);
        private static int? DefaultEventId => 
            int.TryParse(cfgMgr.AppSettings["NTLogEventId"], 
                out var id) && id != -1 ? (int?)id : null;

        public static string LogDirectory
        {
            get
            {
                var tgt = (FileTarget) nLogMgr.Configuration.FindTargetByName("LASHost");
                var logEvtInfo = new LogEventInfo {TimeStamp = DateTime.Now};
                var filNm = tgt.FileName.Render(logEvtInfo);
                if(!File.Exists(filNm)) throw new CoPIOException($"Log FIle {filNm} does not exist.");
                return lib.ExtractPath(filNm);
            }
        }

        /// <summary>
        /// Writes to log file (as configured in nLog configuration)
        /// and to Windows Event Logs, (if/as configured).
        /// (Event Log/Source defaults to event log 
        ///  specified in AppSettings["NTLogName"], 
        ///  or the log to which event source is registered.)
        /// </summary>
        /// <param name="level">None, Debug, Info, Warn, or Error</param>
        /// <param name="message">text to be written to log</param>
        /// <param name="eventId">Five digit code to identify type of event</param>
        /// <param name="category">Two-digit code for sub-classification of event</param>
        /// <param name="eventSource">eventSource for use in event log</param>
        /// <param name="excep">Exception to be processed and included in log</param>
        public static void Write(Level level, string message, 
            Exception excep = null, int? eventId = null, 
            string eventSource = null, short? category = null)
        { Write(level, message, eventId, category, 
            DefaultLogName, eventSource, null, excep);}

        /// <summary>
        /// Writes to log4Net log file (as configured in log4Net.config)
        /// and to Windows Event Logs, (if/as configured).
        /// (Event Log/Source defaults to event log 
        ///  specified in AppSettings["NTLogName"], 
        ///  or the log to which event source is registered.)
        /// </summary>
        /// <param name="level">None, Debug, Info, Warn, or Error</param>
        /// <param name="message">text to be written to log</param>
        /// <param name="eventSource">eventSource for use in event log</param>
        /// <param name="eX">Exception to be processed and included in log</param>
        public static void Write(Level level, string message, string eventSource, Exception eX = null)
        { Write(level, message, null, null, DefaultLogName, eventSource, null, eX); }

        /// <summary>
        /// Writes to log4Net log file (as configured in log4Net.config)
        /// and to Windows Event Logs, (if/as configured).
        /// (Event Log/Source defaults to event log 
        ///  specified in AppSettings["NTLogName"], 
        ///  or the log to which event source is registered.)
        /// </summary>
        /// <param name="level">None, Debug, Info, Warn, or Error</param>
        /// <param name="message">text to be written to log</param>
        /// <param name="eventId">Five digit code to identify type of event</param>
        /// <param name="category">Two-digit code for sub-classification of event</param>
        /// <param name="logName">Name of Event Log</param>
        /// <param name="eventSource">eventSource for use in event log</param>
        /// <param name="rawData">Binary data to be stored in event Log</param>
        /// <param name="eX">Exception to be processed and included in log</param>
        public static void Write(Level level, string message,
            int? eventId, short? category, string logName, string eventSource, 
            byte[] rawData, Exception eX = null)
        { LogWrite(level, message, eventId, category,
            logName, eventSource, rawData, eX); }

        //************* Direct Implementation ********************************************
        /// <summary>
        /// Writes to log4Net log file (as configured in log4Net.config)
        /// and to Windows Event Logs, (if/as configured).
        /// (Event Log/Source defaults to event log 
        ///  specified in AppSettings["NTLogName"], 
        ///  or the log to which event source is registered.)
        /// </summary>
        /// <param name="logName">Name of Event Log</param>
        /// <param name="eventSource">eventSource for use in event log</param>
        /// <param name="level">None, Debug, Info, Warn, or Error</param>
        /// <param name="message">text to be written to log</param>
        /// <param name="eventId">Five digit code to identify type of event</param>
        /// <param name="category">Two-digit code for sub-classification of event</param>
        public static void WriteToNTLog(string logName, string eventSource, 
            Level level, string message, int? eventId, short? category)
        { NTLog(message, level, logName, eventSource, eventId, category); }

        private static void LogWrite(Level level, string message,
            int? eventId, short? category, string logName, string source,
            byte[] rawData, Exception eX = null)
        {
            var eXMessage = eX?.Details(message, true) ?? message;
            WriteNLog(level, eXMessage, eX);

            //switch (level)
            //{
            //    default:
            //        NameLog(logName).Debug(eXMessage);
            //        break;
            //    case Level.Info:
            //        NameLog(logName).Info(eXMessage);
            //        break;
            //    case Level.Warn:
            //        NameLog(logName).Warn(eXMessage);
            //        break;
            //    case Level.Error:
            //        NameLog(logName).Error(eXMessage);
            //        break;
            //}

            Enum.TryParse(cfgMgr.AppSettings["NTLog"], out Level logFilterLevel);
            if (logFilterLevel != Level.None && level >= logFilterLevel)
                NTLog(eXMessage, level, logName, source ?? DefaultSourceName, 
                    eventId ?? DefaultEventId, category, rawData);
        }

        private static void WriteNLog(Level level, string message, 
                                      Exception eX = null)
        {
            switch (level)
            {
                case Level.Trace:
                    Logger.Trace(message);
                    break;
                case Level.Debug:
                    Logger.Debug(message);
                    break;
                case Level.Info:
                    Logger.Info(message);
                    break;
                case Level.Warn:
                    Logger.Warn(message);
                    break;
                case Level.Error:
                    Logger.Error(eX, message);
                    break;
                case Level.Fatal:
                    Logger.Fatal(eX, message);
                    break;
                case Level.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(level), level, null);
            }
        }

        private static void NTLog(string msg, Level entryLevel, 
            string logName, string source, int? eventId, 
            short? category, byte[] rawData = null)
        {
            if (string.IsNullOrEmpty(logName) && string.IsNullOrEmpty(source))
                throw new ArgumentException(
                    "Either logName or source must be specified.");
            // ------------------------------------------------
            if (string.IsNullOrEmpty(logName)) 
                logName = EventLog.LogNameFromSourceName(source, ".");

            if (string.IsNullOrEmpty(source)) source = DefaultLogName;

            //  ****************************************************************
            // ignore SecurityException: caused by inability to read Security Event Log
            try { if (lib.IsAdministrator&& !EventLog.SourceExists(source))
                    EventLog.CreateEventSource(source, logName); }
            catch (SecurityException) {/*Ignore*/ return; } 
            //  ****************************************************************

            var evtLog = new EventLog(logName) { Source = source };
            var elTyp = 
                entryLevel == Level.Warn?  EventLogEntryType.Warning:
                entryLevel == Level.Error? EventLogEntryType.Error: 
                                           EventLogEntryType.Information;
            WriteNTLog(evtLog, msg, elTyp, eventId, category, rawData);
        }

        private static void WriteNTLog(EventLog elog, 
            string msg, EventLogEntryType type, 
            int? eventId, short? category, byte[] rawData)
        {
            if (!eventId.HasValue) elog.WriteEntry(msg, type);
            else if (!category.HasValue) elog.WriteEntry(msg, type, eventId.Value);
            else if (rawData == null) elog.WriteEntry(msg, type, eventId.Value, category.Value);
            else elog.WriteEntry(msg, type, eventId.Value, category.Value, rawData);
        }

        public enum Level {None=0, Trace=1, Debug=2, Info=3, Warn=4, Error=5, Fatal=6 }
    }
}
