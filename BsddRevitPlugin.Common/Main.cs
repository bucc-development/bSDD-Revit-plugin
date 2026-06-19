using ASRR.Core.Log;
using Autodesk.Revit.UI;
using BsddRevitPlugin.Logic.UI.View;
using NLog;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Threading;

namespace BsddRevitPlugin.Common
{
    public class Main
    {
        private static Main _instance;
        private static readonly object InstanceLock = new object();
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private string _openLogFilePath;


        private Main()
        {
            SetupLogs();

            Log.Trace($"Initialized bSDD Revit plugin");
        }

        internal static Main Instance
        {
            get
            {
                lock(InstanceLock)
                {
                    return _instance ?? (_instance = new Main());
                }
            }
        }

        private void SetupLogs()
        {
            var logTarget = CreateLogTarget();
            LogHandler.AddLogTarget(logTarget);
            _openLogFilePath = logTarget.LogFilePath;

            if (logTarget.OpenOnStartUp)
            {
                Process.Start(logTarget.LogFilePath);
            }
        }

        private NLogBasedLogConfiguration CreateLogTarget()
        {
            // Store logs under the per-user local app data folder so it works without a hardcoded C:\TEMP.
            var logFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "BsddRevitPlugin", "logs", "main.log");

            return new NLogBasedLogConfiguration
            {
                LogFilePath = logFilePath,
                LogName = "BsddRevitPluginMainLog",
                MinLevel = "Trace",
                OpenOnStartUp = false,
                NameFilter = "*"
            };
        }

        public void OpenLogs()
        {
            if (_openLogFilePath != null)
            {
                if (!File.Exists(_openLogFilePath)) File.Create(_openLogFilePath);
#if DEBUG
                Process.Start(_openLogFilePath);
#endif
            }
            else
            {
                TaskDialog.Show("Logs Unspecified", "No log filepath was specified.");
            }
        }
    }
}
