using CyberDog.Controls;

namespace Broadcast.SubForms
{
    partial class StartUp
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
            LogPanel = new LogPanel();
            pictureBox1 = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // LogPanel
            // 
            LogPanel.Enabled = false;
            LogPanel.Location = new Point(13, 262);
            LogPanel.Name = "LogPanel";
            LogPanel.Size = new Size(687, 127);
            LogPanel.TabIndex = 1;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = Properties.Resources.airplane;
            pictureBox1.Location = new Point(184, 5);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(333, 251);
            pictureBox1.TabIndex = 2;
            pictureBox1.TabStop = false;
            // 
            // StartUp
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(713, 401);
            Controls.Add(pictureBox1);
            Controls.Add(LogPanel);
            Margin = new Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "StartUp";
            Padding = new Padding(10);
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "StartUp";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);

        }

        #endregion

        private PictureBox pictureBox1;
        public static LogPanel LogPanel;
    }
}
