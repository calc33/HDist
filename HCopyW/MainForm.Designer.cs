
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            panelTop = new System.Windows.Forms.Panel();
            labelStatus = new System.Windows.Forms.Label();
            labelDestDir = new System.Windows.Forms.Label();
            labelCompressDir = new System.Windows.Forms.Label();
            labelSourceDir = new System.Windows.Forms.Label();
            labelStatusTitle = new System.Windows.Forms.Label();
            labelDestDirTitle = new System.Windows.Forms.Label();
            labelCompressDirTitle = new System.Windows.Forms.Label();
            labelSourceDirTitle = new System.Windows.Forms.Label();
            panelBottom = new System.Windows.Forms.Panel();
            buttonQuit = new System.Windows.Forms.Button();
            buttonPause = new System.Windows.Forms.Button();
            textBoxLog = new System.Windows.Forms.TextBox();
            timerAutoQuit = new System.Windows.Forms.Timer(components);
            panelTop.SuspendLayout();
            panelBottom.SuspendLayout();
            SuspendLayout();
            // 
            // panelTop
            // 
            panelTop.Controls.Add(labelStatus);
            panelTop.Controls.Add(labelDestDir);
            panelTop.Controls.Add(labelCompressDir);
            panelTop.Controls.Add(labelSourceDir);
            panelTop.Controls.Add(labelStatusTitle);
            panelTop.Controls.Add(labelDestDirTitle);
            panelTop.Controls.Add(labelCompressDirTitle);
            panelTop.Controls.Add(labelSourceDirTitle);
            resources.ApplyResources(panelTop, "panelTop");
            panelTop.Name = "panelTop";
            // 
            // labelStatus
            // 
            resources.ApplyResources(labelStatus, "labelStatus");
            labelStatus.Name = "labelStatus";
            // 
            // labelDestDir
            // 
            resources.ApplyResources(labelDestDir, "labelDestDir");
            labelDestDir.Name = "labelDestDir";
            // 
            // labelCompressDir
            // 
            resources.ApplyResources(labelCompressDir, "labelCompressDir");
            labelCompressDir.Name = "labelCompressDir";
            // 
            // labelSourceDir
            // 
            resources.ApplyResources(labelSourceDir, "labelSourceDir");
            labelSourceDir.Name = "labelSourceDir";
            // 
            // labelStatusTitle
            // 
            resources.ApplyResources(labelStatusTitle, "labelStatusTitle");
            labelStatusTitle.Name = "labelStatusTitle";
            // 
            // labelDestDirTitle
            // 
            resources.ApplyResources(labelDestDirTitle, "labelDestDirTitle");
            labelDestDirTitle.Name = "labelDestDirTitle";
            // 
            // labelCompressDirTitle
            // 
            resources.ApplyResources(labelCompressDirTitle, "labelCompressDirTitle");
            labelCompressDirTitle.Name = "labelCompressDirTitle";
            // 
            // labelSourceDirTitle
            // 
            resources.ApplyResources(labelSourceDirTitle, "labelSourceDirTitle");
            labelSourceDirTitle.Name = "labelSourceDirTitle";
            // 
            // panelBottom
            // 
            panelBottom.Controls.Add(buttonQuit);
            panelBottom.Controls.Add(buttonPause);
            resources.ApplyResources(panelBottom, "panelBottom");
            panelBottom.Name = "panelBottom";
            // 
            // buttonQuit
            // 
            resources.ApplyResources(buttonQuit, "buttonQuit");
            buttonQuit.Name = "buttonQuit";
            buttonQuit.UseVisualStyleBackColor = true;
            buttonQuit.Click += buttonQuit_Click;
            // 
            // buttonPause
            // 
            resources.ApplyResources(buttonPause, "buttonPause");
            buttonPause.Name = "buttonPause";
            buttonPause.UseVisualStyleBackColor = true;
            buttonPause.Click += buttonPause_Click;
            // 
            // textBoxLog
            // 
            resources.ApplyResources(textBoxLog, "textBoxLog");
            textBoxLog.Name = "textBoxLog";
            // 
            // timerAutoQuit
            // 
            timerAutoQuit.Interval = 200;
            timerAutoQuit.Tick += timerAutoQuit_Tick;
            // 
            // MainForm
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(textBoxLog);
            Controls.Add(panelBottom);
            Controls.Add(panelTop);
            Name = "MainForm";
            FormClosing += MainForm_FormClosing;
            FormClosed += MainForm_FormClosed;
            Load += MainForm_Load;
            Shown += MainForm_Shown;
            panelTop.ResumeLayout(false);
            panelTop.PerformLayout();
            panelBottom.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();

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

