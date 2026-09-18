using Microsoft.Win32.TaskScheduler;
using System;
using System.IO;
using System.Security.Principal;

namespace TinyNvidiaUpdateChecker.Handlers
{
    public class AutostartHelper
    {
        static string TaskName => $"TinyNvidiaUpdateChecker ({Environment.UserName})";

        /// <summary>
        /// Register a scheduled task to run TNUC with --quiet on user logon
        /// </summary>
        public static void RegisterTask()
        {
            using var ts = new TaskService();
            var td = ts.NewTask();

            td.RegistrationInfo.Description = "Checks for NVIDIA GPU driver updates on logon";
            td.Principal.LogonType = TaskLogonType.InteractiveToken;
            td.Principal.RunLevel = TaskRunLevel.LUA;

            td.Settings.StartWhenAvailable = true;
            td.Settings.RunOnlyIfNetworkAvailable = true;
            td.Settings.DisallowStartIfOnBatteries = false;
            td.Settings.StopIfGoingOnBatteries = false;
            td.Settings.MultipleInstances = TaskInstancesPolicy.IgnoreNew;

            td.Triggers.Add(new LogonTrigger
            {
                UserId = WindowsIdentity.GetCurrent().Name,
                Delay = TimeSpan.FromMinutes(1) // Runs 1min after login
            });

            var exe = Environment.ProcessPath!;
            td.Actions.Add(new ExecAction(exe, "--quiet", Path.GetDirectoryName(exe)));

            ts.RootFolder.RegisterTaskDefinition(TaskName, td);
        }

        /// <summary>
        /// Unregisters the autostart task (if it exists)
        /// </summary>
        public static void UnregisterTask()
        {
            using var ts = new TaskService();
            ts.RootFolder.DeleteTask(TaskName, exceptionOnNotExists: false);
        }

        /// <summary>
        /// Does the autostart task exist?
        /// </summary>
        public static bool DoesTaskExists()
        {
            using var ts = new TaskService();
            return ts.RootFolder.Tasks.Exists(TaskName);
        }
    }
}
