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
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(12, 36);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(412, 64);
            label1.TabIndex = 0;
            label1.Text = "NVIDIA driver is ready to be installed.\r\nWhat do you want to do?";
            // 
            // folderBtn
            // 
            folderBtn.Location = new System.Drawing.Point(12, 418);
            folderBtn.Name = "folderBtn";
            folderBtn.Size = new System.Drawing.Size(196, 46);
            folderBtn.TabIndex = 3;
            folderBtn.Text = "Show folder";
            folderBtn.UseVisualStyleBackColor = true;
            folderBtn.Click += folderBtn_Click;
            // 
            // runBtn
            // 
            runBtn.Location = new System.Drawing.Point(12, 205);
            runBtn.Name = "runBtn";
            runBtn.Size = new System.Drawing.Size(196, 46);
            runBtn.TabIndex = 1;
            runBtn.Text = "Run installer";
            runBtn.UseVisualStyleBackColor = true;
            runBtn.Click += runBtn_Click;
            // 
            // deleteBtn
            // 
            deleteBtn.Enabled = false;
            deleteBtn.Location = new System.Drawing.Point(12, 294);
            deleteBtn.Name = "deleteBtn";
            deleteBtn.Size = new System.Drawing.Size(196, 87);
            deleteBtn.TabIndex = 2;
            deleteBtn.Text = "Delete temporary files";
            deleteBtn.UseVisualStyleBackColor = true;
            deleteBtn.Click += deleteBtn_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(214, 212);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(230, 32);
            label2.TabIndex = 0;
            label2.Text = "Run NVIDIA installer";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(213, 307);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(398, 64);
            label3.TabIndex = 0;
            label3.Text = "Delete driver and temporary files.\r\nRun this after finishing driver install.\r\n";
            // 
            // ReadyInstallForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(623, 476);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(deleteBtn);
            Controls.Add(runBtn);
            Controls.Add(folderBtn);
            Controls.Add(label1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.Execut‌​ablePath);
            Name = "ReadyInstallForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "TinyNvidiaUpdateChecker - NVIDIA driver install ready";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button folderBtn;
        private System.Windows.Forms.Button runBtn;
        private System.Windows.Forms.Button deleteBtn;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
    }
}