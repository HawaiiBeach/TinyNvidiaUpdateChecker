using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using TinyNvidiaUpdateChecker.Forms;
using TinyNvidiaUpdateChecker.Handlers;

namespace TinyNvidiaUpdateChecker
{
    public partial class DriverDialog : Form
    {
        static SelectedBtn selectedBtn;
        static NvidiaDriver selectedDriver;
        List<NvidiaDriver> nvidiaDrivers;
        string releaseNotes;
        float notesScale;

        public DriverDialog(List<NvidiaDriver> nvidiaDrivers, string releaseNotes)
        {
            InitializeComponent();
            this.nvidiaDrivers = nvidiaDrivers;
            this.releaseNotes = releaseNotes;
        }

        public static (SelectedBtn selectedBtn, NvidiaDriver selectedDriver) ShowGUI(List<NvidiaDriver> nvidiaDrivers, string releaseNotes)
        {
            using DriverDialog form = new(nvidiaDrivers, releaseNotes);
            form.ShowDialog();

            return (selectedBtn, selectedDriver);
        }

        private void DriverDialog_Load(object sender, EventArgs e)
        {
            webBrowser1.DocumentText = releaseNotes;
            notesScale = this.CreateGraphics().DpiX;

            // Add each driver and assign uiIdx
            foreach (NvidiaDriver driver in this.nvidiaDrivers)
            {
                int index = versionBox.Items.Add(driver.title);
                driver.uiIdx = index;
            }

            // Assign default selected driver
            versionBox.SelectedIndex = 0;
            selectedDriver = nvidiaDrivers.Find(x => x.recommended);

            // Trigger SelectedIndexChanged event to update labels with the default driver information
            VersionBoxChangedIndex();
        }

        private void NotesBtn_Click(object sender, EventArgs e)
        {
            string pdfUrl = null;

            if (selectedDriver.type == "grd")
            {
                pdfUrl = $"https://international.download.nvidia.com/Windows/{selectedDriver.version}/{selectedDriver.version}-win11-win10-release-notes.pdf";
            } else
            {
                pdfUrl = $"https://international.download.nvidia.com/Windows/{selectedDriver.version}/{selectedDriver.version}-win10-win11-nsd-release-notes.pdf";
            }

            try
            {
                Process.Start(new ProcessStartInfo(pdfUrl) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            webBrowser1.Document.ExecCommand("SelectAll", false, "null");
            webBrowser1.Document.ExecCommand("FontName", false, "Microsoft Sans Serif");
            if (notesScale > 96)
            {
                webBrowser1.Document.ExecCommand("FontSize", false, 1);
            }
            else
            {
                webBrowser1.Document.ExecCommand("FontSize", false, 2);
            }

            webBrowser1.Document.ExecCommand("Unselect", false, "null");
        }

        private void IgnoreBtn_Click(object sender, EventArgs e)
        {
            selectedBtn = SelectedBtn.IGNORE;
            Close();
        }

        private void DownloadInstallButton_Click(object sender, EventArgs e)
        {
            contextMenuStrip1.Show(DownloadInstallButton, 0, DownloadInstallButton.Height);
        }

        private void DownloadBtn_Click(object sender, EventArgs e)
        {
            selectedBtn = SelectedBtn.DLEXTRACT;
            Close();
        }

        private void contextMenuStrip1_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
            {
                var hovered = contextMenuStrip1.Items
                    .OfType<ToolStripMenuItem>()
                    .FirstOrDefault(i => i.Bounds.Contains(contextMenuStrip1.PointToClient(Cursor.Position)));

                if (hovered != null && hovered.Text == "Keep driver files?")
                {
                    e.Cancel = true;
                }
            }
        }

        private void configButton_Click(object sender, EventArgs e)
        {
            ConfigurationForm configForm = new();
            Hide();
            configForm.OpenForm();
            Show();
        }

        private void installItem_Click(object sender, EventArgs e)
        {
            selectedBtn = keepCheckBox.Checked ? SelectedBtn.DLINSTALLCUSTOM : SelectedBtn.DLINSTALL;
            Close();
        }

        private void DriverDialog_Shown(object sender, EventArgs e)
        {
            // Flash and play sound
            this.Flash(true);
        }

        private void versionBox_SelectedIndexChanged(object sender, EventArgs e) { VersionBoxChangedIndex(); }

        private void VersionBoxChangedIndex()
        {
            // Find selected driver based on uiIdx
            selectedDriver = nvidiaDrivers.Find(x => x.uiIdx == versionBox.SelectedIndex);

            // Date
            int dateDiff = (DateTime.Now - selectedDriver.releaseDate).Days; // how many days between the two dates
            string daysAgoFromRelease;

            if (dateDiff == 1)
            {
                daysAgoFromRelease = $"{dateDiff} day ago";
            }
            else if (dateDiff < 1)
            {
                daysAgoFromRelease = "today";
            }
            else
            {
                daysAgoFromRelease = $"{dateDiff} days ago";
            }

            if (selectedDriver.releaseDate == DateTime.MinValue)
            {
                daysAgoFromRelease = "unknown";
            }

            toolTip1.SetToolTip(releasedLabel, selectedDriver.releaseDate.ToShortDateString());
            string driverTypeLabel = selectedDriver.type == "grd" ? "Game Ready Driver" : "Studio Driver";

            releasedLabel.Text = $"Released: {daysAgoFromRelease}";
            versionLabel.Text = $"Version: {selectedDriver.version} (you're on {MainConsole.OfflineGPUVersion})";
            sizeLabel.Text = $"Size: {selectedDriver.fileSizeEst}";
            typeLabel.Text = $"Type: {driverTypeLabel}";
        }

        public enum SelectedBtn
        {
            DLINSTALL,
            DLINSTALLCUSTOM,
            DLEXTRACT,
            IGNORE
        }
    }
}
