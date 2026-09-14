using System.Windows.Forms;

namespace TinyNvidiaUpdateChecker
{
    partial class DriverDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DriverDialog));
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
            configButton = new Button();
            contextMenuStrip1 = new ContextMenuStrip(components);
            installItem = new ToolStripMenuItem();
            keepCheckBox = new ToolStripMenuItem();
            versionBox = new ComboBox();
            groupBox1.SuspendLayout();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // DownloadInstallButton
            // 
            DownloadInstallButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            DownloadInstallButton.Location = new System.Drawing.Point(33, 256);
            DownloadInstallButton.Name = "DownloadInstallButton";
            DownloadInstallButton.Size = new System.Drawing.Size(95, 52);
            DownloadInstallButton.TabIndex = 0;
            DownloadInstallButton.Text = "Install Now";
            toolTip1.SetToolTip(DownloadInstallButton, resources.GetString("DownloadInstallButton.ToolTip"));
            DownloadInstallButton.UseVisualStyleBackColor = true;
            DownloadInstallButton.Click += DownloadInstallButton_Click;
            // 
            // DownloadBtn
            // 
            DownloadBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            DownloadBtn.Location = new System.Drawing.Point(134, 256);
            DownloadBtn.Name = "DownloadBtn";
            DownloadBtn.Size = new System.Drawing.Size(95, 52);
            DownloadBtn.TabIndex = 1;
            DownloadBtn.Text = "Download Only";
            toolTip1.SetToolTip(DownloadBtn, resources.GetString("DownloadBtn.ToolTip"));
            DownloadBtn.UseVisualStyleBackColor = true;
            DownloadBtn.Click += DownloadBtn_Click;
            // 
            // NotesBtn
            // 
            NotesBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            NotesBtn.Location = new System.Drawing.Point(234, 256);
            NotesBtn.Name = "NotesBtn";
            NotesBtn.Size = new System.Drawing.Size(95, 52);
            NotesBtn.TabIndex = 2;
            NotesBtn.Text = "View Release Notes";
            toolTip1.SetToolTip(NotesBtn, "View the full pdf release notes, which contains:\r\n- what's new\r\n- what's fixed\r\n- open issues\r\n\r\nand more!");
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
            groupBox1.Location = new System.Drawing.Point(278, 46);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(264, 134);
            groupBox1.TabIndex = 6;
            groupBox1.TabStop = false;
            groupBox1.Text = "Driver Information";
            // 
            // typeLabel
            // 
            typeLabel.Location = new System.Drawing.Point(5, 100);
            typeLabel.Name = "typeLabel";
            typeLabel.Size = new System.Drawing.Size(259, 28);
            typeLabel.TabIndex = 7;
            typeLabel.Text = "Type: ";
            typeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // sizeLabel
            // 
            sizeLabel.Location = new System.Drawing.Point(5, 72);
            sizeLabel.Name = "sizeLabel";
            sizeLabel.Size = new System.Drawing.Size(259, 28);
            sizeLabel.TabIndex = 6;
            sizeLabel.Text = "Size: ";
            sizeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // releasedLabel
            // 
            releasedLabel.Location = new System.Drawing.Point(5, 44);
            releasedLabel.Name = "releasedLabel";
            releasedLabel.Size = new System.Drawing.Size(259, 28);
            releasedLabel.TabIndex = 5;
            releasedLabel.Text = "Released: ";
            releasedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // versionLabel
            // 
            versionLabel.Location = new System.Drawing.Point(5, 16);
            versionLabel.Name = "versionLabel";
            versionLabel.Size = new System.Drawing.Size(259, 28);
            versionLabel.TabIndex = 5;
            versionLabel.Text = "Version: ";
            versionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            toolTip1.SetToolTip(versionLabel, "The version of the graphics drivers");
            // 
            // IgnoreBtn
            // 
            IgnoreBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            IgnoreBtn.Location = new System.Drawing.Point(335, 256);
            IgnoreBtn.Name = "IgnoreBtn";
            IgnoreBtn.Size = new System.Drawing.Size(95, 52);
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
            webBrowser1.Location = new System.Drawing.Point(10, 27);
            webBrowser1.MinimumSize = new System.Drawing.Size(18, 19);
            webBrowser1.Name = "webBrowser1";
            webBrowser1.ScriptErrorsSuppressed = true;
            webBrowser1.Size = new System.Drawing.Size(262, 223);
            webBrowser1.TabIndex = 4;
            webBrowser1.WebBrowserShortcutsEnabled = false;
            webBrowser1.DocumentCompleted += webBrowser1_DocumentCompleted;
            // 
            // configButton
            // 
            configButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            configButton.Location = new System.Drawing.Point(511, 12);
            configButton.Name = "configButton";
            configButton.Size = new System.Drawing.Size(31, 32);
            configButton.TabIndex = 7;
            configButton.Text = "⚙";
            toolTip1.SetToolTip(configButton, "Open configuration");
            configButton.UseVisualStyleBackColor = true;
            configButton.Click += configButton_Click;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { installItem, keepCheckBox });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(163, 48);
            contextMenuStrip1.Closing += contextMenuStrip1_Closing;
            // 
            // installItem
            // 
            installItem.Name = "installItem";
            installItem.Size = new System.Drawing.Size(162, 22);
            installItem.Text = "Install Now >";
            installItem.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            installItem.Click += installItem_Click;
            // 
            // keepCheckBox
            // 
            keepCheckBox.CheckOnClick = true;
            keepCheckBox.Name = "keepCheckBox";
            keepCheckBox.Size = new System.Drawing.Size(162, 22);
            keepCheckBox.Text = "Keep driver files?";
            keepCheckBox.ToolTipText = "Choose custom download location and keep driver files";
            // 
            // versionBox
            // 
            versionBox.DropDownStyle = ComboBoxStyle.DropDownList;
            versionBox.FormattingEnabled = true;
            versionBox.Location = new System.Drawing.Point(278, 186);
            versionBox.Name = "versionBox";
            versionBox.Size = new System.Drawing.Size(264, 23);
            versionBox.TabIndex = 8;
            versionBox.SelectedIndexChanged += versionBox_SelectedIndexChanged;
            // 
            // DriverDialog
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(554, 318);
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
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "DriverDialog";
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
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem customSaveLocationItem;
        private ToolStripMenuItem installItem;
        private ToolStripMenuItem keepCheckBox;
        private ToolStripTextBox toolStripTextBox1;
        private Button configButton;
        private Label typeLabel;
        private ComboBox versionBox;
    }
}