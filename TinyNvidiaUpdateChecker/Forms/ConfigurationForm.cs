using System;
using System.Windows.Forms;
using TinyNvidiaUpdateChecker.Handlers;

namespace TinyNvidiaUpdateChecker.Forms
{
    public partial class ConfigurationForm : Form
    {
        private bool originalCheckUpdates;
        private bool originalMinimalInstall;
        private bool originalExperimentalRepo;
        private string originalDriverType;
        private bool originalAutostart;

        public ConfigurationForm()
        {
            InitializeComponent();
        }

        public void OpenForm()
        {
            ShowDialog();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            bool restartRequired = false;
            string newDriverType = grdRadioButton.Checked ? "grd" : sdRadioButton.Checked ? "sd" : null;

            if (updateCheckBox.Checked != originalCheckUpdates)
            {
                ConfigurationHandler.SetSetting("Check for Updates", updateCheckBox.Checked.ToString().ToLower());
            }

            if (minimalCheckBox.Checked != originalMinimalInstall)
            {
                ConfigurationHandler.SetSetting("Minimal install", minimalCheckBox.Checked.ToString().ToLower());
            }

            if (experimentalCheckBox.Checked != originalExperimentalRepo)
            {
                ConfigurationHandler.SetSetting("Use Experimental Metadata", experimentalCheckBox.Checked.ToString().ToLower());
                restartRequired = true;
            }

            if (autorunCheckBox.Checked != originalAutostart)
            {
                if (autorunCheckBox.Checked)
                {
                    AutostartHelper.RegisterTask();
                }
                else
                {
                    AutostartHelper.UnregisterTask();
                }
            }

            if (newDriverType != originalDriverType)
            {
                ConfigurationHandler.SetSetting("Driver type", newDriverType);
                restartRequired = true;
            }

            if (restartRequired)
            {
                TaskDialogButton[] buttons = [new("OK") { Tag = "ok" }];
                ConfigurationHandler.ShowButtonDialog("Restart Required", "Some changes require a restart to take effect.", TaskDialogIcon.Information, buttons);
            }

            Close();
        }

        private void ConfigurationForm_Load(object sender, EventArgs e)
        {
            Enabled = false;

            originalCheckUpdates = ConfigurationHandler.ReadSettingBool("Check for Updates");
            originalMinimalInstall = ConfigurationHandler.ReadSettingBool("Minimal install");
            originalDriverType = ConfigurationHandler.ReadSetting("Driver type");
            originalExperimentalRepo = ConfigurationHandler.ReadSetting("Use Experimental Metadata", null, false) == "true";
            string gpuId = ConfigurationHandler.ReadSetting("GPU ID", null, false);
            originalAutostart = AutostartHelper.DoesTaskExists();

            updateCheckBox.Checked = originalCheckUpdates;
            experimentalCheckBox.Checked = originalExperimentalRepo;
            autorunCheckBox.Checked = originalAutostart;

            if (LibraryHandler.EvaluateLibrary() != null)
            {
                minimalCheckBox.Checked = originalMinimalInstall;
            }
            else
            {
                minimalCheckBox.Enabled = false;
            }

            switch (originalDriverType)
            {
                case "grd":
                    grdRadioButton.Checked = true;
                    break;
                case "sd":
                    sdRadioButton.Checked = true;
                    break;
            }

            if (gpuId != null && gpuId != "0")
            {
                multiGpuGroupBox.Enabled = true;
            }

            Enabled = true;
        }

        private void resetGpuButton_Click(object sender, EventArgs e)
        {
            multiGpuGroupBox.Enabled = false;
            ConfigurationHandler.SetSetting("GPU ID", "0");

            TaskDialogButton[] buttons = [new("OK") { Tag = "ok" }];
            ConfigurationHandler.ShowButtonDialog("GPU choice has been reset", "Please restart for changes to take effect.", TaskDialogIcon.Information, buttons);
        }

        private void autorunCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (autorunCheckBox.Checked)
            {
                DialogResult result = MessageBox.Show(
                    "TNUC will run quietly each time you log in, only prompting to update " +
                    "if a new driver is available.\n\n" +

                    "It will run from this location:\n" +
                    $"{Environment.ProcessPath}\n\n" +
                    "If you move, rename, or delete the file, autorun will stop working " +
                    "and you'll need to enable it again in the settings.\n\n" +
                    "Is this a permanent location for TNUC? If so, enable autorun now?\n" +
                    "(Don't forget to save for changes to apply)",
                    "Enable Autorun?",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (result == DialogResult.No) autorunCheckBox.Checked = false;
            }
        }
    }
}
