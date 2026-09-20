using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace TinyNvidiaUpdateChecker.Handlers
{
    internal class UpdateHandler
    {
        public static void SearchForUpdate(string[] args)
        {
            ConsoleHelper.Write("Searching for Update . . . ");

            try {
                string response = MainConsole.SendGetRequest(MainConsole.updateUrl);
                GitHubAPIReleaseRoot release = JsonConvert.DeserializeObject<GitHubAPIReleaseRoot>(response);
                MainConsole.onlineVer = release.tag_name[1..];

                Asset exeFile = release.assets.Where(x => x.name == "TinyNvidiaUpdateChecker.exe").First();
                string downloadUrl = exeFile.browser_download_url;
                string serverHash = exeFile.digest[7..];
                string changelog = release.body;

                ConsoleHelper.Write("OK!");
                ConsoleHelper.WriteLine();

                if (new Version(MainConsole.onlineVer).CompareTo(new Version(MainConsole.offlineVer)) > 0) {
                    ConsoleHelper.WriteLine("There is a update available for TinyNvidiaUpdateChecker!");

                    if (!MainConsole.confirmDL && !MainConsole.dryRun) {
                        TaskDialogButton[] buttons = [
                            new("Update Now") { Tag = "update" },
                            new("Ignore") { Tag = "no" }
                        ];

                        string dialog = ConfigurationHandler.ShowButtonDialog("New Client Update Available", changelog, TaskDialogIcon.Information, buttons);

                        if (dialog == "update") {
                            UpdateNow(args, downloadUrl, serverHash);
                        }
                    }
                }
            } catch (Exception) {
                MainConsole.onlineVer = "0.0.0";
                ConsoleHelper.WriteLine("Update check unavailable, continuing without client update.");
            }

            if (MainConsole.debug) {
                ConsoleHelper.WriteLine($"offlineVer: {MainConsole.offlineVer}");
                ConsoleHelper.WriteLine($"onlineVer:  {MainConsole.onlineVer}");
            }

            ConsoleHelper.WriteLine();
        }

        private static void UpdateNow(string[] args, string downloadUrl, string serverHash)
        {
            string currentExe = Path.GetFullPath(Environment.ProcessPath);
            string backupExe = currentExe + ".old";
            string tempFile = Path.Combine(Path.GetTempPath(), "TinyNvidiaUpdateChecker.tmp");
            bool movedToBackup = false;

            try {
                File.Move(currentExe, backupExe, true);
                movedToBackup = true;

                ConsoleHelper.WriteLine();
                ConsoleHelper.Write("Downloading update . . . ");

                MainConsole.HandleDownload(downloadUrl, tempFile).GetAwaiter().GetResult();

                ConsoleHelper.WriteLine("OK!");
                ConsoleHelper.Write("Validating checksum . . . ");

                // Validate checksum SHA256
                string tempHash = CalculateSHA256(tempFile);

                if (tempHash != null && tempHash == serverHash) {
                    ConsoleHelper.WriteLine("OK!");
                    ConsoleHelper.WriteLine();

                    File.Move(tempFile, currentExe);
                    ConsoleHelper.WriteLine("Relaunching now!");

                    string runArgs = string.Join(" ", args) + " --cleanup-update";
                    Process.Start(new ProcessStartInfo(currentExe) { UseShellExecute = true, Arguments = runArgs });
                    Environment.Exit(0);
                }

                ConsoleHelper.WriteLine("ERROR!");
                ConsoleHelper.WriteLine("Checksum mismatch!");
                ConsoleHelper.WriteLine();
                ConsoleHelper.WriteLine($"Calculated Hash: {tempHash}");
                ConsoleHelper.WriteLine($"Server Hash:     {serverHash}");
            } catch (UnauthorizedAccessException) {
                ConsoleHelper.WriteLine("ERROR!");
                ConsoleHelper.WriteLine();
                ConsoleHelper.WriteLine("Access to update the current TNUC installation was denied due to unauthorized access. Please rerun TNUC as admin, or update manually.");
            } catch (Exception ex) {
                ConsoleHelper.WriteLine("ERROR!");
                ConsoleHelper.WriteLine();
                ConsoleHelper.WriteLine(ex.ToString());
            } finally {
                // Remove temp file if not deleted by updater
                if (File.Exists(tempFile)) {
                    try { File.Delete(tempFile); } catch { }
                }

                // If updater failed (currentExe is missing) and backupExe still exists, try to restore it
                if (movedToBackup && !File.Exists(currentExe) && File.Exists(backupExe)) {
                    try { File.Move(backupExe, currentExe, true); } catch { }
                }
            }

            ConsoleHelper.WriteLine("Automatic update failed, please update manually.");
            ConsoleHelper.WriteLine();
        }

        public static string CalculateSHA256(string filePath)
        {
            using SHA256 sha256 = SHA256.Create();
            try
            {
                using FileStream stream = File.OpenRead(filePath);
                string hash = BitConverter.ToString(sha256.ComputeHash(stream))
                    .Replace("-", "")
                    .ToLowerInvariant();
                return hash;
            }
            catch
            {
                ConsoleHelper.WriteLine("ERROR");
                ConsoleHelper.WriteLine();
                return null;
            }
        }
    }
}
