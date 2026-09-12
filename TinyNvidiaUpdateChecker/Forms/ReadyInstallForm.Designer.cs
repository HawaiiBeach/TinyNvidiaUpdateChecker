namespace TinyNvidiaUpdateChecker.Forms
{
    partial class ReadyInstallForm
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
            label1 = new System.Windows.Forms.Label();
            folderBtn = new System.Windows.Forms.Button();
            runBtn = new System.Windows.Forms.Button();
            deleteBtn = new System.Windows.Forms.Button();
            exeLabel = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(11, 9);
            label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(241, 30);
            label1.TabIndex = 0;
            label1.Text = "NVIDIA driver update is ready to be installed.\r\nWhat do you want to do?";
            // 
            // folderBtn
            // 
            folderBtn.Location = new System.Drawing.Point(11, 230);
            folderBtn.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            folderBtn.Name = "folderBtn";
            folderBtn.Size = new System.Drawing.Size(106, 22);
            folderBtn.TabIndex = 7;
            folderBtn.Text = "Show folder";
            folderBtn.UseVisualStyleBackColor = true;
            folderBtn.Click += folderBtn_Click;
            // 
            // runBtn
            // 
            runBtn.Location = new System.Drawing.Point(11, 130);
            runBtn.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            runBtn.Name = "runBtn";
            runBtn.Size = new System.Drawing.Size(106, 22);
            runBtn.TabIndex = 3;
            runBtn.Text = "Run installer";
            runBtn.UseVisualStyleBackColor = true;
            runBtn.Click += runBtn_Click;
            // 
            // deleteBtn
            // 
            deleteBtn.Enabled = false;
            deleteBtn.Location = new System.Drawing.Point(11, 172);
            deleteBtn.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            deleteBtn.Name = "deleteBtn";
            deleteBtn.Size = new System.Drawing.Size(106, 41);
            deleteBtn.TabIndex = 5;
            deleteBtn.Text = "Delete temporary files";
            deleteBtn.UseVisualStyleBackColor = true;
            deleteBtn.Click += deleteBtn_Click;
            // 
            // exeLabel
            // 
            exeLabel.AutoSize = true;
            exeLabel.Location = new System.Drawing.Point(11, 86);
            exeLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            exeLabel.Name = "exeLabel";
            exeLabel.Size = new System.Drawing.Size(64, 15);
            exeLabel.TabIndex = 2;
            exeLabel.Text = "{exeName}";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(120, 178);
            label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(196, 30);
            label3.TabIndex = 6;
            label3.Text = "Delete driver and temporary files.\r\nRun this after finishing driver install.\r\n";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(11, 71);
            label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(61, 15);
            label2.TabIndex = 1;
            label2.Text = "File name:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(121, 134);
            label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(113, 15);
            label4.TabIndex = 4;
            label4.Text = "Run NVIDIA installer";
            // 
            // ReadyInstallForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(376, 262);
            Controls.Add(label4);
            Controls.Add(label2);
            Controls.Add(label3);
            Controls.Add(exeLabel);
            Controls.Add(deleteBtn);
            Controls.Add(runBtn);
            Controls.Add(folderBtn);
            Controls.Add(label1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.Execut‌​ablePath);
            Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ReadyInstallForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "NVIDIA driver update ready";
            Load += ReadyInstallForm_Load;
            Shown += ReadyInstallForm_Shown;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button folderBtn;
        private System.Windows.Forms.Button runBtn;
        private System.Windows.Forms.Button deleteBtn;
        private System.Windows.Forms.Label exeLabel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
    }
}