namespace Broadcast
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            flowLayoutPanel1 = new FlowLayoutPanel();
            statusStrip = new StatusStrip();
            toolStripStatusLabel = new ToolStripStatusLabel();
            panel = new Panel();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Location = new Point(7, 8);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(111, 452);
            flowLayoutPanel1.TabIndex = 0;
            // 
            // statusStrip
            // 
            statusStrip.AllowDrop = true;
            statusStrip.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel });
            statusStrip.Location = new Point(0, 467);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(732, 22);
            statusStrip.TabIndex = 1;
            statusStrip.Text = "statusStrip";
            // 
            // toolStripStatusLabel
            // 
            toolStripStatusLabel.Name = "toolStripStatusLabel";
            toolStripStatusLabel.Size = new Size(112, 17);
            toolStripStatusLabel.Text = "toolStripStatusLabel";
            toolStripStatusLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // panel
            // 
            panel.Location = new Point(134, 11);
            panel.Name = "panel";
            panel.Size = new Size(595, 449);
            panel.TabIndex = 2;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(732, 489);
            Controls.Add(panel);
            Controls.Add(statusStrip);
            Controls.Add(flowLayoutPanel1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "MainForm";
            Text = "Simulator Broadcast";
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        public FlowLayoutPanel flowLayoutPanel1;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel toolStripStatusLabel;
        private Panel panel;
    }
}
