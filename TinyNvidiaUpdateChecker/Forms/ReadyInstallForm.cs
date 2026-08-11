using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace TinyNvidiaUpdateChecker.Forms
{
    public partial class ReadyInstallForm : Form
    {
        string driverPath;
        string folderPath;
        bool hasDeletedTempFiles = false;
        bool hasRunInstaller = false;
        bool isInstallerRunning = false;

        public ReadyInstallForm(string driverPath)
        {
            this.driverPath = driverPath;
            this.folderPath = Path.GetDirectoryName(driverPath);
            InitializeComponent();
        }

        public static void handleInstall(string driverPath, bool minimized, bool keepDriver = false)
        {
            if (!minimized)  
            {
                using ReadyInstallForm form = new(driverPath);
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
                    try
                    {
                        Directory.Delete(folderPath, true);
                        hasDeletedTempFiles = true;
                    }
                    catch
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
            try
            {
                Directory.Delete(folderPath, true);

                hasDeletedTempFiles = true;
                deleteBtn.Enabled = false;
                runBtn.Enabled = false;
                folderBtn.Enabled = false;
                MessageBox.Show("Deleted temporary files", "TinyNvidiaUpdateChecker", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
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
    }
}
