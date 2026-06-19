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
                OpenWithDefaultApp(logTarget.LogFilePath);
            }
        }

        // Open a file with its associated application. UseShellExecute must be set explicitly:
        // it defaults to false on .NET 8, where Process.Start(path) would otherwise try to *execute*
        // the file (throwing Win32Exception) instead of opening it with the shell. Safe on net48 too.
        private static void OpenWithDefaultApp(string path)
        {
            try
            {
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            }
            catch
            {
                // Opening the log viewer is a convenience only; never let it break add-in startup.
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
                if (!File.Exists(_openLogFilePath))
                {
                    // Ensure the log directory/file exist without leaking the stream handle.
                    Directory.CreateDirectory(Path.GetDirectoryName(_openLogFilePath));
                    File.Create(_openLogFilePath).Dispose();
                }
                // NOTE: we intentionally do NOT launch an editor on the log here. Auto-opening the
                // file on every startup was fragile (and on .NET 8 threw a Win32Exception when the
                // file was already held open by NLog). The log lives at _openLogFilePath; open it
                // manually when needed.
            }
            else
            {
                TaskDialog.Show("Logs Unspecified", "No log filepath was specified.");
            }
        }
    }
}
