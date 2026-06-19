// Vendored from ASRR.Core (lib-asrr-core) so the plugin can build for both net48
// and net8.0-windows without depending on the net48-only ASRR submodules.
// Namespaces are kept identical to the original so existing `using` statements are unchanged.
// Source: https://github.com/ASRRtechnologies/lib-asrr-core (Log)
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.IO;
using System.Linq;
using static System.IO.File;

namespace ASRR.Core.Log
{
    public class NLogBasedLogConfiguration
    {
        public bool OpenOnStartUp { get; set; }
        public bool OpenOnButton { get; set; }
        public string LogName { get; set; }
        public string LogFilePath { get; set; }
        public string LogLayout { get; set; }
        public string NameFilter { get; set; }
        public string MinLevel { get; set; }
    }

    public static class LogHandler
    {
        private static Logger log;

        public static void AddLogTarget(NLogBasedLogConfiguration configuration)
        {
            try
            {
                var fileInfo = new FileInfo(configuration.LogFilePath);
                if (fileInfo.DirectoryName != null)
                    Directory.CreateDirectory(fileInfo.DirectoryName);

                if (!Exists(configuration.LogFilePath))
                    Create(configuration.LogFilePath);

                if (ReadAllLines(configuration.LogFilePath).ToList().Count() >
                    10000) //Archive file with timestamp, delete old file
                {
                    var date = DateTime.Now.ToString().Replace('/', '-');
                    date = date.Replace(':', '_');
                    var dirPath = Path.GetDirectoryName(configuration.LogFilePath);
                    var fileName = Path.GetFileName(configuration.LogFilePath);
                    fileName = fileName.Replace(".log", date + ".log");
                    Copy(configuration.LogFilePath, Path.Combine(dirPath, fileName));
                    Delete(configuration.LogFilePath);
                    Create(configuration.LogFilePath);
                }
            }
            catch
            {
                //Fail silently
            }

            configuration.OpenOnButton = true;

            var logConfiguration = LogManager.Configuration ?? new LoggingConfiguration();

            logConfiguration.RemoveTarget(configuration.LogName);
            var target = new FileTarget(configuration.LogName)
            {
                FileName = configuration.LogFilePath
            };

            logConfiguration.AddTarget(target);

            LogLevel level;
            switch (configuration.MinLevel)
            {
                case "Debug":
                    level = LogLevel.Debug;
                    break;
                case "Trace":
                    level = LogLevel.Trace;
                    break;
                case "Info":
                    level = LogLevel.Info;
                    break;
                case "Warn":
                    level = LogLevel.Warn;
                    break;
                case "Error":
                    level = LogLevel.Error;
                    break;
                default:
                    level = LogLevel.Trace;
                    break;
            }

            var loggingRule = new LoggingRule(configuration.NameFilter, level, target);
            logConfiguration.LoggingRules.Add(loggingRule);

            LogManager.Configuration = logConfiguration;
            log = LogManager.GetCurrentClassLogger();

            log.Debug("Using programmatic config");
        }

        public static void RemoveLogTarget(string name)
        {
            var logConfiguration = LogManager.Configuration;
            logConfiguration?.RemoveTarget(name);
        }

        public static void AddDefaultTempLogTarget(string name, string logFilePath)
        {
            var defaultConfiguration = new NLogBasedLogConfiguration
            {
                OpenOnStartUp = false,
                LogName = name,
                LogFilePath = logFilePath,
                LogLayout = "${date:format=yyyy-MM-dd hh\\:mm\\:ss} | ${level} | ${logger} | ${message} ${exception}",
                NameFilter = "*",
                MinLevel = "Trace"
            };

            AddLogTarget(defaultConfiguration);
        }

        public static void AddErrorReportTempLogTarget(string name, string logFilePath)
        {
            var errorReportConfiguration = new NLogBasedLogConfiguration
            {
                OpenOnStartUp = false,
                LogName = name,
                LogFilePath = logFilePath,
                LogLayout = "${level} | ${message} ${exception}",
                NameFilter = "*",
                MinLevel = "Warn"
            };

            AddLogTarget(errorReportConfiguration);
        }
    }
}
