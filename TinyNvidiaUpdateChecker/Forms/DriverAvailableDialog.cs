using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TinyNvidiaUpdateChecker.Forms;
using TinyNvidiaUpdateChecker.Handlers;

namespace TinyNvidiaUpdateChecker
{
    public partial class DriverAvailableDialog : Form
    {
        SelectedBtn selectedBtn = SelectedBtn.IGNORE;
        NvidiaDriver selectedDriver;
        List<NvidiaDriver> nvidiaDrivers;
        string releaseNotes;
        bool updateReleaseNotes = true;

        public DriverAvailableDialog(List<NvidiaDriver> nvidiaDrivers, string releaseNotes)
        {
            ArgumentNullException.ThrowIfNull(nvidiaDrivers);
            if (nvidiaDrivers.Count == 0 || nvidiaDrivers.Any(x => x == null))
                throw new ArgumentException("At least one valid driver is required", nameof(nvidiaDrivers));

            InitializeComponent();
            contextMenuStrip1.Renderer = new CleanMenuRenderer();
            this.nvidiaDrivers = nvidiaDrivers;
            this.releaseNotes = releaseNotes;
        }

        public static (SelectedBtn selectedBtn, NvidiaDriver selectedDriver) ShowGUI(List<NvidiaDriver> nvidiaDrivers, string releaseNotes)
        {
            using DriverAvailableDialog form = new(nvidiaDrivers, releaseNotes);
            form.ShowDialog();

            return (form.selectedBtn, form.selectedDriver);
        }

        private void DriverDialog_Load(object sender, EventArgs e)
        {
            // Add each driver and assign uiIdx
            foreach (NvidiaDriver driver in this.nvidiaDrivers)
            {
                int index = versionBox.Items.Add(driver.title);
                driver.uiIdx = index;
            }

            // Set recommended driver as default choice, or fallback to first driver
            selectedDriver = nvidiaDrivers.Find(x => x.recommended) ?? nvidiaDrivers[0];

            // This will trigger SelectedIndexChanged event
            versionBox.SelectedIndex = selectedDriver.uiIdx;
        }

        private void NotesBtn_Click(object sender, EventArgs e)
        {
            if (selectedDriver == null) return;
            string pdfUrl = null;

            // selected driver has pdfUrl
            if (!string.IsNullOrWhiteSpace(selectedDriver.pdfUrl))
            {
                pdfUrl = selectedDriver.pdfUrl;
            }
            // Fallback to constructing the URL based on driver type and version
            else if (selectedDriver.downloadUrl?.Contains("Quadro_Certified", StringComparison.OrdinalIgnoreCase) == true)
            {
                pdfUrl = $"https://international.download.nvidia.com/Windows/Quadro_Certified/{selectedDriver.version}/{selectedDriver.version}-win10-win11-nvidia-rtx-quadro-release-notes.pdf";
            }
            else if (selectedDriver.type is "grd" or "notebook")
            {
                pdfUrl = $"https://international.download.nvidia.com/Windows/{selectedDriver.version}/{selectedDriver.version}-win11-win10-release-notes.pdf";
            }
            else if (selectedDriver.type is "sd" or "sd-notebook")
            {
                pdfUrl = $"https://international.download.nvidia.com/Windows/{selectedDriver.version}/{selectedDriver.version}-win10-win11-nsd-release-notes.pdf";
            }

            try
            {
                if (string.IsNullOrWhiteSpace(pdfUrl))
                {
                    MessageBox.Show(this, "Release notes are unavailable for this driver.", "TinyNvidiaUpdateChecker",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                Process.Start(new ProcessStartInfo(pdfUrl) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteLine(ex.ToString());
            }
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

                if (hovered != null && hovered.Text == keepCheckBox.Text)
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

        private async void VersionBoxChangedIndex()
        {
            // Find selected driver based on uiIdx
            selectedDriver = nvidiaDrivers.Find(x => x.uiIdx == versionBox.SelectedIndex);
            if (selectedDriver == null) return;

            NvidiaDriver driver = selectedDriver;

            // If selected driver is missing release date & file size (caused by experimental metadata)
            if (selectedDriver.releaseDate == DateTime.MinValue)
            {
                try
                {
                    releasedLabel.Text = "Released: loading...";
                    sizeLabel.Text = "Size: loading...";

                    (long fileSize, DateTime releaseDate) = await Task.Run(() => MainConsole.GetDriverMetadataFromNvidia(driver.downloadUrl));
                    driver.releaseDate = releaseDate;
                    driver.fileSize = fileSize;
                    driver.fileSizeEst = fileSize >= 0 ? Math.Round(fileSize / 1024d / 1024d) + " MiB" : "unknown";
                }
                catch (Exception ex)
                {
                    driver.fileSizeEst = "unknown";
                    ConsoleHelper.WriteLine($"Driver metadata unavailable: {ex.Message}");
                }
            }

            // Ensure UI is still valid and the selected driver hasn't changed during async operation
            if (IsDisposed || Disposing || selectedDriver != driver) return;

            // Construct date label
            // How many days between the two dates
            int dateDiff = (DateTime.Now - selectedDriver.releaseDate).Days;
            string releasedLabelStr;

            if (selectedDriver.releaseDate == DateTime.MinValue)
            {
                releasedLabelStr = "unknown";
            }
            else if (dateDiff == 1)
            {
                releasedLabelStr = $"{dateDiff} day ago";
            }
            else if (dateDiff < 1)
            {
                releasedLabelStr = "today";
            }
            else if (dateDiff < 30)
            {
                releasedLabelStr = $"{dateDiff} days ago";
            }
            else
            {
                int months = dateDiff / 30;
                releasedLabelStr = months == 1 ? "1 month ago" : $"{months} months ago";
            }

            // Set release description
            // NOTE: Experimental metadata doesn't support individual release notes
            if (updateReleaseNotes)
            {
                if (webBrowser1.Document == null)
                {
                    webBrowser1.Navigate("about:blank");
                    while (webBrowser1.ReadyState != WebBrowserReadyState.Complete)
                    {
                        Application.DoEvents();
                    }
                }

                // Smaller font size for high DPI displays
                string fontSize = DeviceDpi > 96 ? "8pt" : "10pt";

                // Format html with inline styles
                string styledHtml = $@"<html>
                    <head><meta charset='UTF-8'></head>
                    <body style='font-family: ""Microsoft Sans Serif""; font-size: {fontSize};'>
                        {selectedDriver.releaseNotes ?? releaseNotes}
                    </body>
                </html>";

                // Set new release notes
                webBrowser1.Document.OpenNew(true);
                webBrowser1.Document.Write(styledHtml);
                webBrowser1.Refresh();
            }

            // Don't update release notes if relying on global releaseNotes (applies to experimental metadata)
            if (selectedDriver.releaseNotes == null && updateReleaseNotes)
                updateReleaseNotes = false;

            toolTip1.SetToolTip(releasedLabel, selectedDriver.releaseDate.ToShortDateString());
            releasedLabel.Text = $"Released: {releasedLabelStr}";

            versionLabel.Text = $"Version: {selectedDriver.version} (you're on {MainConsole.OfflineGPUVersion})";
            sizeLabel.Text = $"Size: {selectedDriver.fileSizeEst}";
            typeLabel.Text = $"Type: {selectedDriver.typeLabel}";
        }

        public enum SelectedBtn
        {
            DLINSTALL,
            DLINSTALLCUSTOM,
            DLEXTRACT,
            IGNORE
        }
    }

public class CleanMenuRenderer : ToolStripProfessionalRenderer
    {
        public CleanMenuRenderer() : base(new CleanColors()) { }

        protected override void OnRenderImageMargin(ToolStripRenderEventArgs e) { }

        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Scale checkbox size dynamically based on item height (~55%)
            int itemHeight = e.Item.Height;
            int boxSize = (int)(itemHeight * 0.55f);

            // Ensure even dimension for crisp line rendering
            if (boxSize % 2 != 0) boxSize--;

            // Center vertically and apply proportional left padding
            int x = (int)(itemHeight * 0.25f);
            int y = (itemHeight - boxSize) / 2;

            Rectangle boxRect = new(x, y, boxSize, boxSize);

            // Render checkbox background
            using (var brush = new SolidBrush(Color.FromArgb(0, 120, 215)))
            {
                e.Graphics.FillRectangle(brush, boxRect);
            }

            // Render dynamically scaled checkmark
            float penWidth = Math.Max(1.5f, boxSize / 8.0f);
            using Pen pen = new(Color.White, penWidth);
            pen.StartCap = LineCap.Round;
            pen.EndCap = LineCap.Round;

            PointF p1 = new(boxRect.Left + boxSize * 0.22f, boxRect.Top + boxSize * 0.52f);
            PointF p2 = new(boxRect.Left + boxSize * 0.44f, boxRect.Top + boxSize * 0.74f);
            PointF p3 = new(boxRect.Left + boxSize * 0.80f, boxRect.Top + boxSize * 0.26f);

            e.Graphics.DrawLines(pen, [p1, p2, p3]);
        }
    }

    public class CleanColors : ProfessionalColorTable
    {
        public override Color ToolStripDropDownBackground => Color.White;
        public override Color MenuBorder => Color.FromArgb(204, 204, 204);
        public override Color MenuItemSelected => Color.FromArgb(235, 235, 235);
        public override Color MenuItemBorder => Color.Transparent;
    }
}
