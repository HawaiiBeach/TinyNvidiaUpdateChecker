using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using TinyNvidiaUpdateChecker.Handlers;

namespace TinyNvidiaUpdateChecker.Forms
{
    public partial class ReadyInstallForm : Form
    {
        string driverPath;
        string folderPath;
        List<string> tempFiles;
        bool hasDeletedTempFiles = false;
        bool hasRunInstaller = false;
        bool isInstallerRunning = false;

        public ReadyInstallForm(string driverPath, List<string> tempFiles)
        {
            this.driverPath = driverPath;
            this.folderPath = Path.GetDirectoryName(driverPath);
            this.tempFiles = tempFiles;

            InitializeComponent();
        }

        public static void handleInstall(string driverPath, bool minimized, List<string> tempFiles, bool keepDriver = false)
        {
            if (!minimized)
            {
                using ReadyInstallForm form = new(driverPath, tempFiles);
                form.ShowDialog();
            }
            else
            {
                // Quiet mode does not show this UI
                // Might change the behaviour in the future. Installing a GPU driver without user interaction is weird
                try
                {
                    MainConsole.WriteLine();
                    MainConsole.Write("Executing driver installer . . . ");

                    ProcessStartInfo startInfo = new(driverPath)
                    {
                        UseShellExecute = true,
                        Arguments = "/s /noreboot"
                    };

                    Process.Start(startInfo).WaitForExit();
                    MainConsole.Write("OK!");
                }
                catch
                {
                    MainConsole.WriteLine("Could not run the installer, try running it manually.");
                    MainConsole.WriteLine();
                    MainConsole.callExit(1);
                }

                MainConsole.WriteLine();

                string folderPath = Path.GetDirectoryName(driverPath);

                if (!keepDriver)
                {
                    try
                    {
                        Directory.Delete(folderPath, true);
                        MainConsole.WriteLine($"Cleaned up: {folderPath}");
                    }
                    catch
                    {
                        MainConsole.WriteLine($"Could not cleanup: {folderPath}");
                    }
                }
            }

        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (isInstallerRunning)
            {
                e.Cancel = true;
                MessageBox.Show("Please wait for the installer to finish.", "TinyNvidiaUpdateChecker", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!hasDeletedTempFiles && Directory.Exists(folderPath))
            {
                string message = "Temporary driver files have not been deleted. Do you want to delete them before closing?";

                if (!hasRunInstaller)
                {
                    message += "\n\nImportant note: It does not appear that the installer has been run yet. Deleting temporary files will require you to download the driver again.";
                }

                DialogResult result = MessageBox.Show(message, "TinyNvidiaUpdateChecker", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    bool success = deleteTempFiles();

                    if (success)
                    {
                        hasDeletedTempFiles = true;
                    }
                    else
                    {
                        MessageBox.Show("Could not delete temporary files!", "TinyNvidiaUpdateChecker", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        private void runBtn_Click(object sender, EventArgs e)
        {
            Enabled = false;
            deleteBtn.Enabled = false;
            isInstallerRunning = true;

            try
            {
                MainConsole.WriteLine();
                MainConsole.Write("Executing driver installer . . . ");

                ProcessStartInfo startInfo = new(driverPath)
                {
                    UseShellExecute = true
                };

                Process.Start(startInfo).WaitForExit();

                hasRunInstaller = true;
                MainConsole.Write("OK!");
                MainConsole.WriteLine();
                runBtn.Enabled = false;
                Focus();
            }
            catch (Exception)
            {
                MessageBox.Show("Could not start installer!", "TinyNvidiaUpdateChecker", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MainConsole.Write("ERROR!");
                MainConsole.WriteLine();
            }
            finally
            {
                isInstallerRunning = false;
                Enabled = true;
                deleteBtn.Enabled = true;
            }
        }

        private void deleteBtn_Click(object sender, EventArgs e)
        {
            bool success = deleteTempFiles();

            if (success)
            {

                hasDeletedTempFiles = true;
                deleteBtn.Enabled = false;
                runBtn.Enabled = false;
                folderBtn.Enabled = false;
                MessageBox.Show("Deleted temporary files", "TinyNvidiaUpdateChecker", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Could not delete temporary files!", "TinyNvidiaUpdateChecker", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void folderBtn_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(folderPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = folderPath,
                    UseShellExecute = true
                });
            }
        }

        // Delete temporary files, and the parent folder if it is empty
        private bool deleteTempFiles()
        {
            try
            {
                foreach (string entry in tempFiles)
                {
                    string path = Path.Combine(folderPath, entry);

                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                    else if (Directory.Exists(path))
                    {
                        Directory.Delete(path, true);
                    }
                }

                // If directory is empty after deleting tempFiles, delete it as well
                if (Directory.GetFiles(folderPath).Length == 0 && Directory.GetDirectories(folderPath).Length == 0)
                {
                    Directory.Delete(folderPath);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void ReadyInstallForm_Load(object sender, EventArgs e)
        {
            // Set exeLabel
            string exeName = Path.GetFileName(this.driverPath);
            exeLabel.Text = exeName;
        }

        private void ReadyInstallForm_Shown(object sender, EventArgs e)
        {
            // Flash and play sound
            this.Flash(true);
        }
    }
}
