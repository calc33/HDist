
namespace HCopy
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.panelTop = new System.Windows.Forms.Panel();
            this.labelStatus = new System.Windows.Forms.Label();
            this.labelDestDir = new System.Windows.Forms.Label();
            this.labelCompressDir = new System.Windows.Forms.Label();
            this.labelSourceDir = new System.Windows.Forms.Label();
            this.labelStatusTitle = new System.Windows.Forms.Label();
            this.labelDestDirTitle = new System.Windows.Forms.Label();
            this.labelCompressDirTitle = new System.Windows.Forms.Label();
            this.labelSourceDirTitle = new System.Windows.Forms.Label();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.buttonQuit = new System.Windows.Forms.Button();
            this.buttonPause = new System.Windows.Forms.Button();
            this.textBoxLog = new System.Windows.Forms.TextBox();
            this.timerAutoQuit = new System.Windows.Forms.Timer(this.components);
            this.panelTop.SuspendLayout();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.labelStatus);
            this.panelTop.Controls.Add(this.labelDestDir);
            this.panelTop.Controls.Add(this.labelCompressDir);
            this.panelTop.Controls.Add(this.labelSourceDir);
            this.panelTop.Controls.Add(this.labelStatusTitle);
            this.panelTop.Controls.Add(this.labelDestDirTitle);
            this.panelTop.Controls.Add(this.labelCompressDirTitle);
            this.panelTop.Controls.Add(this.labelSourceDirTitle);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(624, 80);
            this.panelTop.TabIndex = 3;
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(65, 61);
            this.labelStatus.Margin = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(16, 15);
            this.labelStatus.TabIndex = 7;
            this.labelStatus.Text = "...";
            // 
            // labelDestDir
            // 
            this.labelDestDir.AutoSize = true;
            this.labelDestDir.Location = new System.Drawing.Point(65, 44);
            this.labelDestDir.Margin = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.labelDestDir.Name = "labelDestDir";
            this.labelDestDir.Size = new System.Drawing.Size(16, 15);
            this.labelDestDir.TabIndex = 6;
            this.labelDestDir.Text = "...";
            // 
            // labelCompressDir
            // 
            this.labelCompressDir.AutoSize = true;
            this.labelCompressDir.Location = new System.Drawing.Point(65, 27);
            this.labelCompressDir.Margin = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.labelCompressDir.Name = "labelCompressDir";
            this.labelCompressDir.Size = new System.Drawing.Size(16, 15);
            this.labelCompressDir.TabIndex = 5;
            this.labelCompressDir.Text = "...";
            // 
            // labelSourceDir
            // 
            this.labelSourceDir.AutoSize = true;
            this.labelSourceDir.Location = new System.Drawing.Point(65, 10);
            this.labelSourceDir.Margin = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.labelSourceDir.Name = "labelSourceDir";
            this.labelSourceDir.Size = new System.Drawing.Size(16, 15);
            this.labelSourceDir.TabIndex = 4;
            this.labelSourceDir.Text = "...";
            // 
            // labelStatusTitle
            // 
            this.labelStatusTitle.AutoSize = true;
            this.labelStatusTitle.Location = new System.Drawing.Point(22, 61);
            this.labelStatusTitle.Margin = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.labelStatusTitle.Name = "labelStatusTitle";
            this.labelStatusTitle.Size = new System.Drawing.Size(43, 15);
            this.labelStatusTitle.TabIndex = 3;
            this.labelStatusTitle.Text = "状況：";
            this.labelStatusTitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelDestDirTitle
            // 
            this.labelDestDirTitle.AutoSize = true;
            this.labelDestDirTitle.Location = new System.Drawing.Point(9, 44);
            this.labelDestDirTitle.Margin = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.labelDestDirTitle.Name = "labelDestDirTitle";
            this.labelDestDirTitle.Size = new System.Drawing.Size(56, 15);
            this.labelDestDirTitle.TabIndex = 2;
            this.labelDestDirTitle.Text = "コピー先：";
            this.labelDestDirTitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelCompressDirTitle
            // 
            this.labelCompressDirTitle.AutoSize = true;
            this.labelCompressDirTitle.Location = new System.Drawing.Point(14, 27);
            this.labelCompressDirTitle.Margin = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.labelCompressDirTitle.Name = "labelCompressDirTitle";
            this.labelCompressDirTitle.Size = new System.Drawing.Size(51, 15);
            this.labelCompressDirTitle.TabIndex = 1;
            this.labelCompressDirTitle.Text = "(圧縮)：";
            this.labelCompressDirTitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelSourceDirTitle
            // 
            this.labelSourceDirTitle.AutoSize = true;
            this.labelSourceDirTitle.Location = new System.Drawing.Point(9, 10);
            this.labelSourceDirTitle.Margin = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.labelSourceDirTitle.Name = "labelSourceDirTitle";
            this.labelSourceDirTitle.Size = new System.Drawing.Size(56, 15);
            this.labelSourceDirTitle.TabIndex = 0;
            this.labelSourceDirTitle.Text = "コピー元：";
            this.labelSourceDirTitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.buttonQuit);
            this.panelBottom.Controls.Add(this.buttonPause);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 282);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(624, 41);
            this.panelBottom.TabIndex = 5;
            // 
            // buttonQuit
            // 
            this.buttonQuit.Location = new System.Drawing.Point(316, 6);
            this.buttonQuit.Name = "buttonQuit";
            this.buttonQuit.Size = new System.Drawing.Size(85, 23);
            this.buttonQuit.TabIndex = 1;
            this.buttonQuit.Text = "終了中断(5)";
            this.buttonQuit.UseVisualStyleBackColor = true;
            this.buttonQuit.Click += new System.EventHandler(this.buttonQuit_Click);
            // 
            // buttonPause
            // 
            this.buttonPause.Location = new System.Drawing.Point(225, 6);
            this.buttonPause.Name = "buttonPause";
            this.buttonPause.Size = new System.Drawing.Size(85, 23);
            this.buttonPause.TabIndex = 0;
            this.buttonPause.Text = "一時停止";
            this.buttonPause.UseVisualStyleBackColor = true;
            this.buttonPause.Click += new System.EventHandler(this.buttonPause_Click);
            // 
            // textBoxLog
            // 
            this.textBoxLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxLog.Location = new System.Drawing.Point(0, 80);
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxLog.Size = new System.Drawing.Size(624, 202);
            this.textBoxLog.TabIndex = 6;
            // 
            // timerAutoQuit
            // 
            this.timerAutoQuit.Interval = 200;
            this.timerAutoQuit.Tick += new System.EventHandler(this.timerAutoQuit_Tick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 323);
            this.Controls.Add(this.textBoxLog);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.panelTop);
            this.Name = "MainForm";
            this.Text = "HCopy";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.panelBottom.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label labelSourceDir;
        private System.Windows.Forms.Label labelStatusTitle;
        private System.Windows.Forms.Label labelDestDirTitle;
        private System.Windows.Forms.Label labelCompressDirTitle;
        private System.Windows.Forms.Label labelSourceDirTitle;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.TextBox textBoxLog;
        private System.Windows.Forms.Label labelCompressDir;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Label labelDestDir;
        private System.Windows.Forms.Button buttonPause;
        private System.Windows.Forms.Button buttonQuit;
        private System.Windows.Forms.Timer timerAutoQuit;
    }
}

