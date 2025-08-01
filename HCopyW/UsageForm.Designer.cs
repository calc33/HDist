namespace HCopy
{
    partial class UsageForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UsageForm));
            panelBottom = new System.Windows.Forms.Panel();
            buttonClose = new System.Windows.Forms.Button();
            textBoxUsage = new System.Windows.Forms.TextBox();
            textBoxCommandLine = new System.Windows.Forms.TextBox();
            panelBottom.SuspendLayout();
            SuspendLayout();
            // 
            // panelBottom
            // 
            panelBottom.Controls.Add(buttonClose);
            panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelBottom.Location = new System.Drawing.Point(0, 403);
            panelBottom.Name = "panelBottom";
            panelBottom.Size = new System.Drawing.Size(800, 47);
            panelBottom.TabIndex = 0;
            // 
            // buttonClose
            // 
            buttonClose.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            buttonClose.Location = new System.Drawing.Point(709, 12);
            buttonClose.Name = "buttonClose";
            buttonClose.Size = new System.Drawing.Size(75, 23);
            buttonClose.TabIndex = 0;
            buttonClose.Text = "閉じる";
            buttonClose.UseVisualStyleBackColor = true;
            // 
            // textBoxUsage
            // 
            textBoxUsage.BackColor = System.Drawing.SystemColors.Window;
            textBoxUsage.Dock = System.Windows.Forms.DockStyle.Fill;
            textBoxUsage.Location = new System.Drawing.Point(0, 83);
            textBoxUsage.Multiline = true;
            textBoxUsage.Name = "textBoxUsage";
            textBoxUsage.ReadOnly = true;
            textBoxUsage.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            textBoxUsage.Size = new System.Drawing.Size(800, 320);
            textBoxUsage.TabIndex = 1;
            textBoxUsage.TabStop = false;
            textBoxUsage.WordWrap = false;
            // 
            // textBoxCommandLine
            // 
            textBoxCommandLine.BorderStyle = System.Windows.Forms.BorderStyle.None;
            textBoxCommandLine.Dock = System.Windows.Forms.DockStyle.Top;
            textBoxCommandLine.Location = new System.Drawing.Point(0, 0);
            textBoxCommandLine.Multiline = true;
            textBoxCommandLine.Name = "textBoxCommandLine";
            textBoxCommandLine.ReadOnly = true;
            textBoxCommandLine.Size = new System.Drawing.Size(800, 83);
            textBoxCommandLine.TabIndex = 2;
            textBoxCommandLine.TabStop = false;
            textBoxCommandLine.Text = "aaa";
            // 
            // UsageForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(textBoxUsage);
            Controls.Add(textBoxCommandLine);
            Controls.Add(panelBottom);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "UsageForm";
            Text = "HCopyW";
            Load += UsageForm_Load;
            panelBottom.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.TextBox textBoxUsage;
        private System.Windows.Forms.TextBox textBoxCommandLine;
    }
}