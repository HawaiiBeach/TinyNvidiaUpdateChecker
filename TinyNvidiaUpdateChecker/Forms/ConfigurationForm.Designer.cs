namespace TinyNvidiaUpdateChecker.Forms
{
    partial class ConfigurationForm
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
            groupBox1 = new System.Windows.Forms.GroupBox();
            autorunCheckBox = new System.Windows.Forms.CheckBox();
            minimalCheckBox = new System.Windows.Forms.CheckBox();
            updateCheckBox = new System.Windows.Forms.CheckBox();
            cancelButton = new System.Windows.Forms.Button();
            saveButton = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            experimentalCheckBox = new System.Windows.Forms.CheckBox();
            groupBox4 = new System.Windows.Forms.GroupBox();
            multiGpuGroupBox = new System.Windows.Forms.GroupBox();
            resetGpuButton = new System.Windows.Forms.Button();
            groupBox2 = new System.Windows.Forms.GroupBox();
            sdRadioButton = new System.Windows.Forms.RadioButton();
            grdRadioButton = new System.Windows.Forms.RadioButton();
            groupBox1.SuspendLayout();
            groupBox4.SuspendLayout();
            multiGpuGroupBox.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(autorunCheckBox);
            groupBox1.Controls.Add(minimalCheckBox);
            groupBox1.Controls.Add(updateCheckBox);
            groupBox1.Location = new System.Drawing.Point(12, 32);
            groupBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            groupBox1.Size = new System.Drawing.Size(215, 93);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "General";
            // 
            // autorunCheckBox
            // 
            autorunCheckBox.AutoSize = true;
            autorunCheckBox.Location = new System.Drawing.Point(12, 67);
            autorunCheckBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            autorunCheckBox.Name = "autorunCheckBox";
            autorunCheckBox.Size = new System.Drawing.Size(162, 19);
            autorunCheckBox.TabIndex = 3;
            autorunCheckBox.Text = "Enable autorun on logon?";
            toolTip1.SetToolTip(autorunCheckBox, "Run quietly in the background on user login, and prompts if update is available\r\nWARNING:\r\nMake sure you do not move TNUC when enabling this feature.\r\nMoving exe will cause autorun to stop working.");
            autorunCheckBox.UseVisualStyleBackColor = true;
            autorunCheckBox.CheckedChanged += autorunCheckBox_CheckedChanged;
            // 
            // minimalCheckBox
            // 
            minimalCheckBox.AutoSize = true;
            minimalCheckBox.Location = new System.Drawing.Point(12, 44);
            minimalCheckBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            minimalCheckBox.Name = "minimalCheckBox";
            minimalCheckBox.Size = new System.Drawing.Size(147, 19);
            minimalCheckBox.TabIndex = 2;
            minimalCheckBox.Text = "Enable minimal install?";
            minimalCheckBox.UseVisualStyleBackColor = true;
            // 
            // updateCheckBox
            // 
            updateCheckBox.AutoSize = true;
            updateCheckBox.Location = new System.Drawing.Point(12, 21);
            updateCheckBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            updateCheckBox.Name = "updateCheckBox";
            updateCheckBox.Size = new System.Drawing.Size(184, 19);
            updateCheckBox.TabIndex = 1;
            updateCheckBox.Text = "Check for updates on startup?";
            updateCheckBox.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            cancelButton.Location = new System.Drawing.Point(169, 332);
            cancelButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(55, 22);
            cancelButton.TabIndex = 8;
            cancelButton.Text = "Cancel";
            cancelButton.UseVisualStyleBackColor = true;
            cancelButton.Click += cancelButton_Click;
            // 
            // saveButton
            // 
            saveButton.Location = new System.Drawing.Point(81, 332);
            saveButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            saveButton.Name = "saveButton";
            saveButton.Size = new System.Drawing.Size(82, 22);
            saveButton.TabIndex = 7;
            saveButton.Text = "Save";
            saveButton.UseVisualStyleBackColor = true;
            saveButton.Click += saveButton_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(10, 7);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(115, 15);
            label1.TabIndex = 0;
            label1.Text = "Configuration Menu";
            // 
            // experimentalCheckBox
            // 
            experimentalCheckBox.AutoSize = true;
            experimentalCheckBox.Location = new System.Drawing.Point(10, 19);
            experimentalCheckBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            experimentalCheckBox.Name = "experimentalCheckBox";
            experimentalCheckBox.Size = new System.Drawing.Size(174, 19);
            experimentalCheckBox.TabIndex = 6;
            experimentalCheckBox.Text = "Use experimental data repo?";
            toolTip1.SetToolTip(experimentalCheckBox, "Uses an experimental GPU metadata repo.\r\nThis resolves issues with eGPUs and TNUC not able to identify GPUs by name.\r\nNOTE: Does not support Quadro (RTX Enterprise) drivers\r\nData provided by TechPowerUp");
            experimentalCheckBox.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            groupBox4.Controls.Add(experimentalCheckBox);
            groupBox4.Location = new System.Drawing.Point(12, 272);
            groupBox4.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            groupBox4.Name = "groupBox4";
            groupBox4.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            groupBox4.Size = new System.Drawing.Size(215, 44);
            groupBox4.TabIndex = 23;
            groupBox4.TabStop = false;
            groupBox4.Text = "Experimental";
            // 
            // multiGpuGroupBox
            // 
            multiGpuGroupBox.Controls.Add(resetGpuButton);
            multiGpuGroupBox.Enabled = false;
            multiGpuGroupBox.Location = new System.Drawing.Point(12, 198);
            multiGpuGroupBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            multiGpuGroupBox.Name = "multiGpuGroupBox";
            multiGpuGroupBox.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            multiGpuGroupBox.Size = new System.Drawing.Size(215, 70);
            multiGpuGroupBox.TabIndex = 22;
            multiGpuGroupBox.TabStop = false;
            multiGpuGroupBox.Text = "Multi GPU Setup";
            // 
            // resetGpuButton
            // 
            resetGpuButton.Location = new System.Drawing.Point(10, 20);
            resetGpuButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            resetGpuButton.Name = "resetGpuButton";
            resetGpuButton.Size = new System.Drawing.Size(122, 38);
            resetGpuButton.TabIndex = 5;
            resetGpuButton.Text = "Reset GPU choice\r\n(requires restart)";
            resetGpuButton.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(sdRadioButton);
            groupBox2.Controls.Add(grdRadioButton);
            groupBox2.Location = new System.Drawing.Point(12, 129);
            groupBox2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            groupBox2.Size = new System.Drawing.Size(215, 65);
            groupBox2.TabIndex = 21;
            groupBox2.TabStop = false;
            groupBox2.Text = "Driver type";
            // 
            // sdRadioButton
            // 
            sdRadioButton.AutoSize = true;
            sdRadioButton.Location = new System.Drawing.Point(9, 40);
            sdRadioButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            sdRadioButton.Name = "sdRadioButton";
            sdRadioButton.Size = new System.Drawing.Size(93, 19);
            sdRadioButton.TabIndex = 4;
            sdRadioButton.TabStop = true;
            sdRadioButton.Text = "Studio Driver";
            sdRadioButton.UseVisualStyleBackColor = true;
            // 
            // grdRadioButton
            // 
            grdRadioButton.AutoSize = true;
            grdRadioButton.Location = new System.Drawing.Point(9, 18);
            grdRadioButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            grdRadioButton.Name = "grdRadioButton";
            grdRadioButton.Size = new System.Drawing.Size(173, 19);
            grdRadioButton.TabIndex = 3;
            grdRadioButton.TabStop = true;
            grdRadioButton.Text = "Game Ready Driver (default)";
            grdRadioButton.UseVisualStyleBackColor = true;
            // 
            // ConfigurationForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(236, 365);
            Controls.Add(groupBox4);
            Controls.Add(multiGpuGroupBox);
            Controls.Add(groupBox2);
            Controls.Add(label1);
            Controls.Add(saveButton);
            Controls.Add(cancelButton);
            Controls.Add(groupBox1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.Execut‌​ablePath);
            Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            Name = "ConfigurationForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Configuration Menu";
            Load += ConfigurationForm_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            multiGpuGroupBox.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox minimalCheckBox;
        private System.Windows.Forms.CheckBox updateCheckBox;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.CheckBox autorunCheckBox;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.CheckBox experimentalCheckBox;
        private System.Windows.Forms.GroupBox multiGpuGroupBox;
        private System.Windows.Forms.Button resetGpuButton;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton sdRadioButton;
        private System.Windows.Forms.RadioButton grdRadioButton;
    }
}