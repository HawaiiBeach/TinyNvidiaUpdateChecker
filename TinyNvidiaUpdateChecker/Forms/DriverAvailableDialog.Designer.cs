using System.Windows.Forms;

namespace TinyNvidiaUpdateChecker
{
    partial class DriverAvailableDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DriverAvailableDialog));
            DownloadInstallButton = new Button();
            DownloadBtn = new Button();
            NotesBtn = new Button();
            titleLabel = new Label();
            groupBox1 = new GroupBox();
            typeLabel = new Label();
            sizeLabel = new Label();
            releasedLabel = new Label();
            versionLabel = new Label();
            IgnoreBtn = new Button();
            webBrowser1 = new WebBrowser();
            toolTip1 = new ToolTip(components);
            driverLabel = new Label();
            configButton = new Button();
            versionBox = new ComboBox();
            installItem = new ToolStripMenuItem();
            keepCheckBox = new ToolStripMenuItem();
            contextMenuStrip1 = new ContextMenuStrip(components);
            groupBox1.SuspendLayout();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // DownloadInstallButton
            // 
            DownloadInstallButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            DownloadInstallButton.Location = new System.Drawing.Point(9, 265);
            DownloadInstallButton.Name = "DownloadInstallButton";
            DownloadInstallButton.Size = new System.Drawing.Size(92, 45);
            DownloadInstallButton.TabIndex = 0;
            DownloadInstallButton.Text = "Install Now";
            toolTip1.SetToolTip(DownloadInstallButton, "Download and install the driver now.\r\nThe driver will silently download (and perform minimal install, if enabled) in the background.\r\nOnce installation is ready, you will be notified.");
            DownloadInstallButton.UseVisualStyleBackColor = true;
            DownloadInstallButton.Click += DownloadInstallButton_Click;
            // 
            // DownloadBtn
            // 
            DownloadBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            DownloadBtn.Location = new System.Drawing.Point(109, 265);
            DownloadBtn.Name = "DownloadBtn";
            DownloadBtn.Size = new System.Drawing.Size(92, 45);
            DownloadBtn.TabIndex = 1;
            DownloadBtn.Text = "Download";
            toolTip1.SetToolTip(DownloadBtn, resources.GetString("DownloadBtn.ToolTip"));
            DownloadBtn.UseVisualStyleBackColor = true;
            DownloadBtn.Click += DownloadBtn_Click;
            // 
            // NotesBtn
            // 
            NotesBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            NotesBtn.Location = new System.Drawing.Point(310, 228);
            NotesBtn.Name = "NotesBtn";
            NotesBtn.Size = new System.Drawing.Size(232, 31);
            NotesBtn.TabIndex = 2;
            NotesBtn.Text = "View Release Notes PDF";
            toolTip1.SetToolTip(NotesBtn, "View the full PDF release notes, which contains:\r\n- What's new\r\n- What's fixed\r\n- Open issues");
            NotesBtn.UseVisualStyleBackColor = true;
            NotesBtn.Click += NotesBtn_Click;
            // 
            // titleLabel
            // 
            titleLabel.AutoSize = true;
            titleLabel.Location = new System.Drawing.Point(8, 8);
            titleLabel.Name = "titleLabel";
            titleLabel.Size = new System.Drawing.Size(210, 15);
            titleLabel.TabIndex = 5;
            titleLabel.Text = "A new graphics card driver is available!\r\n";
            // 
            // groupBox1
            // 
            groupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            groupBox1.Controls.Add(typeLabel);
            groupBox1.Controls.Add(sizeLabel);
            groupBox1.Controls.Add(releasedLabel);
            groupBox1.Controls.Add(versionLabel);
            groupBox1.Location = new System.Drawing.Point(310, 20);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(232, 134);
            groupBox1.TabIndex = 6;
            groupBox1.TabStop = false;
            groupBox1.Text = "Driver Information";
            // 
            // typeLabel
            // 
            typeLabel.Location = new System.Drawing.Point(5, 100);
            typeLabel.Name = "typeLabel";
            typeLabel.Size = new System.Drawing.Size(226, 28);
            typeLabel.TabIndex = 7;
            typeLabel.Text = "Type: ";
            typeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // sizeLabel
            // 
            sizeLabel.Location = new System.Drawing.Point(5, 72);
            sizeLabel.Name = "sizeLabel";
            sizeLabel.Size = new System.Drawing.Size(226, 28);
            sizeLabel.TabIndex = 6;
            sizeLabel.Text = "Size: ";
            sizeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // releasedLabel
            // 
            releasedLabel.Location = new System.Drawing.Point(5, 44);
            releasedLabel.Name = "releasedLabel";
            releasedLabel.Size = new System.Drawing.Size(226, 28);
            releasedLabel.TabIndex = 5;
            releasedLabel.Text = "Released: ";
            releasedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // versionLabel
            // 
            versionLabel.Location = new System.Drawing.Point(5, 16);
            versionLabel.Name = "versionLabel";
            versionLabel.Size = new System.Drawing.Size(226, 28);
            versionLabel.TabIndex = 5;
            versionLabel.Text = "Version: ";
            versionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            toolTip1.SetToolTip(versionLabel, "The version of the graphics drivers");
            // 
            // IgnoreBtn
            // 
            IgnoreBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            IgnoreBtn.Location = new System.Drawing.Point(209, 265);
            IgnoreBtn.Name = "IgnoreBtn";
            IgnoreBtn.Size = new System.Drawing.Size(92, 45);
            IgnoreBtn.TabIndex = 3;
            IgnoreBtn.Text = "Ignore";
            IgnoreBtn.UseVisualStyleBackColor = true;
            IgnoreBtn.Click += IgnoreBtn_Click;
            // 
            // webBrowser1
            // 
            webBrowser1.AllowNavigation = false;
            webBrowser1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            webBrowser1.IsWebBrowserContextMenuEnabled = false;
            webBrowser1.Location = new System.Drawing.Point(10, 29);
            webBrowser1.MinimumSize = new System.Drawing.Size(18, 19);
            webBrowser1.Name = "webBrowser1";
            webBrowser1.ScriptErrorsSuppressed = true;
            webBrowser1.Size = new System.Drawing.Size(289, 230);
            webBrowser1.TabIndex = 4;
            webBrowser1.WebBrowserShortcutsEnabled = false;
            webBrowser1.DocumentCompleted += webBrowser1_DocumentCompleted;
            // 
            // driverLabel
            // 
            driverLabel.AutoSize = true;
            driverLabel.Location = new System.Drawing.Point(311, 262);
            driverLabel.Name = "driverLabel";
            driverLabel.Size = new System.Drawing.Size(135, 15);
            driverLabel.TabIndex = 9;
            driverLabel.Text = "Choose driver (optional)";
            toolTip1.SetToolTip(driverLabel, "You can choose to install an older driver.\r\nIf you are not sure, do not change this setting.");
            // 
            // configButton
            // 
            configButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            configButton.Location = new System.Drawing.Point(310, 174);
            configButton.Name = "configButton";
            configButton.Size = new System.Drawing.Size(232, 31);
            configButton.TabIndex = 7;
            configButton.Text = "Open Configuration Menu";
            configButton.UseVisualStyleBackColor = true;
            configButton.Click += configButton_Click;
            // 
            // versionBox
            // 
            versionBox.DropDownStyle = ComboBoxStyle.DropDownList;
            versionBox.FlatStyle = FlatStyle.Popup;
            versionBox.FormattingEnabled = true;
            versionBox.Location = new System.Drawing.Point(311, 283);
            versionBox.Name = "versionBox";
            versionBox.Size = new System.Drawing.Size(231, 23);
            versionBox.TabIndex = 8;
            versionBox.SelectedIndexChanged += versionBox_SelectedIndexChanged;
            // 
            // installItem
            // 
            installItem.Name = "installItem";
            installItem.Size = new System.Drawing.Size(227, 22);
            installItem.Text = "Install Now >";
            installItem.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            installItem.Click += installItem_Click;
            // 
            // keepCheckBox
            // 
            keepCheckBox.CheckOnClick = true;
            keepCheckBox.Name = "keepCheckBox";
            keepCheckBox.Size = new System.Drawing.Size(227, 22);
            keepCheckBox.Text = "Custom Download Location?";
            keepCheckBox.ToolTipText = "Choose where downloaded drivers will be saved?";
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { installItem, keepCheckBox });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(228, 48);
            contextMenuStrip1.Closing += contextMenuStrip1_Closing;
            // 
            // DriverAvailableDialog
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(554, 318);
            Controls.Add(driverLabel);
            Controls.Add(versionBox);
            Controls.Add(configButton);
            Controls.Add(webBrowser1);
            Controls.Add(IgnoreBtn);
            Controls.Add(groupBox1);
            Controls.Add(titleLabel);
            Controls.Add(NotesBtn);
            Controls.Add(DownloadBtn);
            Controls.Add(DownloadInstallButton);
            FormBorderStyle = FormBorderStyle.None;
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.Execut‌​ablePath);
            Name = "DriverAvailableDialog";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "TinyNvidiaUpdateChecker - Update Dialog";
            Load += DriverDialog_Load;
            Shown += DriverDialog_Shown;
            groupBox1.ResumeLayout(false);
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button DownloadInstallButton;
        private System.Windows.Forms.Button DownloadBtn;
        private System.Windows.Forms.Button NotesBtn;
        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label versionLabel;
        private System.Windows.Forms.Label releasedLabel;
        private System.Windows.Forms.Button IgnoreBtn;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private ToolTip toolTip1;
        private Label sizeLabel;
        private Button configButton;
        private Label typeLabel;
        private ComboBox versionBox;
        private Label driverLabel;
        private ToolStripMenuItem installItem;
        private ToolStripMenuItem keepCheckBox;
        private ContextMenuStrip contextMenuStrip1;
    }
}