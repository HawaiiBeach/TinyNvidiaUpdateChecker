namespace TinyNvidiaUpdateChecker.Forms
{
    partial class ComponentChooserForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ComponentChooserForm));
            checkedListBox = new System.Windows.Forms.CheckedListBox();
            okButton = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            richTextBox = new System.Windows.Forms.RichTextBox();
            noneLabel = new System.Windows.Forms.LinkLabel();
            allLabel = new System.Windows.Forms.LinkLabel();
            latestLabel = new System.Windows.Forms.LinkLabel();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            SuspendLayout();
            // 
            // checkedListBox
            // 
            checkedListBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            checkedListBox.FormattingEnabled = true;
            checkedListBox.Location = new System.Drawing.Point(20, 19);
            checkedListBox.Margin = new System.Windows.Forms.Padding(5);
            checkedListBox.Name = "checkedListBox";
            checkedListBox.Size = new System.Drawing.Size(626, 724);
            checkedListBox.TabIndex = 0;
            checkedListBox.SelectedValueChanged += checkedListBox_SelectedValueChanged;
            // 
            // okButton
            // 
            okButton.Location = new System.Drawing.Point(20, 818);
            okButton.Margin = new System.Windows.Forms.Padding(5);
            okButton.Name = "okButton";
            okButton.Size = new System.Drawing.Size(109, 46);
            okButton.TabIndex = 5;
            okButton.Text = "OK";
            okButton.UseVisualStyleBackColor = true;
            okButton.Click += okButton_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(141, 824);
            label1.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(479, 32);
            label1.TabIndex = 6;
            label1.Text = "Choose the components you want to install";
            // 
            // richTextBox
            // 
            richTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            richTextBox.Location = new System.Drawing.Point(658, 19);
            richTextBox.Margin = new System.Windows.Forms.Padding(5);
            richTextBox.Name = "richTextBox";
            richTextBox.ReadOnly = true;
            richTextBox.Size = new System.Drawing.Size(458, 746);
            richTextBox.TabIndex = 1;
            richTextBox.Text = "";
            // 
            // noneLabel
            // 
            noneLabel.AutoSize = true;
            noneLabel.Location = new System.Drawing.Point(20, 770);
            noneLabel.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            noneLabel.Name = "noneLabel";
            noneLabel.Size = new System.Drawing.Size(73, 32);
            noneLabel.TabIndex = 2;
            noneLabel.TabStop = true;
            noneLabel.Text = "None";
            noneLabel.LinkClicked += noneLabel_LinkClicked;
            // 
            // allLabel
            // 
            allLabel.AutoSize = true;
            allLabel.Location = new System.Drawing.Point(141, 770);
            allLabel.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            allLabel.Name = "allLabel";
            allLabel.Size = new System.Drawing.Size(41, 32);
            allLabel.TabIndex = 3;
            allLabel.TabStop = true;
            allLabel.Text = "All";
            toolTip1.SetToolTip(allLabel, "Choose all components");
            allLabel.LinkClicked += allLabel_LinkClicked;
            // 
            // latestLabel
            // 
            latestLabel.AutoSize = true;
            latestLabel.Location = new System.Drawing.Point(227, 770);
            latestLabel.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            latestLabel.Name = "latestLabel";
            latestLabel.Size = new System.Drawing.Size(113, 32);
            latestLabel.TabIndex = 4;
            latestLabel.TabStop = true;
            latestLabel.Text = "Last used";
            toolTip1.SetToolTip(latestLabel, "Apply the last used components saved in config");
            latestLabel.LinkClicked += latestLabel_LinkClicked;
            // 
            // ComponentChooserForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1136, 886);
            Controls.Add(latestLabel);
            Controls.Add(allLabel);
            Controls.Add(noneLabel);
            Controls.Add(richTextBox);
            Controls.Add(label1);
            Controls.Add(okButton);
            Controls.Add(checkedListBox);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.Execut‌​ablePath);
            Margin = new System.Windows.Forms.Padding(5);
            Name = "ComponentChooserForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Choose Components";
            Load += ComponentChooserForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.CheckedListBox checkedListBox;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RichTextBox richTextBox;
        private System.Windows.Forms.LinkLabel noneLabel;
        private System.Windows.Forms.LinkLabel allLabel;
        private System.Windows.Forms.LinkLabel latestLabel;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}