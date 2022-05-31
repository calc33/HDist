
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
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
            resources.ApplyResources(this.panelTop, "panelTop");
            this.panelTop.Controls.Add(this.labelStatus);
            this.panelTop.Controls.Add(this.labelDestDir);
            this.panelTop.Controls.Add(this.labelCompressDir);
            this.panelTop.Controls.Add(this.labelSourceDir);
            this.panelTop.Controls.Add(this.labelStatusTitle);
            this.panelTop.Controls.Add(this.labelDestDirTitle);
            this.panelTop.Controls.Add(this.labelCompressDirTitle);
            this.panelTop.Controls.Add(this.labelSourceDirTitle);
            this.panelTop.Name = "panelTop";
            // 
            // labelStatus
            // 
            resources.ApplyResources(this.labelStatus, "labelStatus");
            this.labelStatus.Name = "labelStatus";
            // 
            // labelDestDir
            // 
            resources.ApplyResources(this.labelDestDir, "labelDestDir");
            this.labelDestDir.Name = "labelDestDir";
            // 
            // labelCompressDir
            // 
            resources.ApplyResources(this.labelCompressDir, "labelCompressDir");
            this.labelCompressDir.Name = "labelCompressDir";
            // 
            // labelSourceDir
            // 
            resources.ApplyResources(this.labelSourceDir, "labelSourceDir");
            this.labelSourceDir.Name = "labelSourceDir";
            // 
            // labelStatusTitle
            // 
            resources.ApplyResources(this.labelStatusTitle, "labelStatusTitle");
            this.labelStatusTitle.Name = "labelStatusTitle";
            // 
            // labelDestDirTitle
            // 
            resources.ApplyResources(this.labelDestDirTitle, "labelDestDirTitle");
            this.labelDestDirTitle.Name = "labelDestDirTitle";
            // 
            // labelCompressDirTitle
            // 
            resources.ApplyResources(this.labelCompressDirTitle, "labelCompressDirTitle");
            this.labelCompressDirTitle.Name = "labelCompressDirTitle";
            // 
            // labelSourceDirTitle
            // 
            resources.ApplyResources(this.labelSourceDirTitle, "labelSourceDirTitle");
            this.labelSourceDirTitle.Name = "labelSourceDirTitle";
            // 
            // panelBottom
            // 
            resources.ApplyResources(this.panelBottom, "panelBottom");
            this.panelBottom.Controls.Add(this.buttonQuit);
            this.panelBottom.Controls.Add(this.buttonPause);
            this.panelBottom.Name = "panelBottom";
            // 
            // buttonQuit
            // 
            resources.ApplyResources(this.buttonQuit, "buttonQuit");
            this.buttonQuit.Name = "buttonQuit";
            this.buttonQuit.UseVisualStyleBackColor = true;
            this.buttonQuit.Click += new System.EventHandler(this.buttonQuit_Click);
            // 
            // buttonPause
            // 
            resources.ApplyResources(this.buttonPause, "buttonPause");
            this.buttonPause.Name = "buttonPause";
            this.buttonPause.UseVisualStyleBackColor = true;
            this.buttonPause.Click += new System.EventHandler(this.buttonPause_Click);
            // 
            // textBoxLog
            // 
            resources.ApplyResources(this.textBoxLog, "textBoxLog");
            this.textBoxLog.Name = "textBoxLog";
            // 
            // timerAutoQuit
            // 
            this.timerAutoQuit.Interval = 200;
            this.timerAutoQuit.Tick += new System.EventHandler(this.timerAutoQuit_Tick);
            // 
            // MainForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.textBoxLog);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.panelTop);
            this.Name = "MainForm";
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

