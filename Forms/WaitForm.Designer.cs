namespace Broadcast.SubForms
{
    partial class WaitForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WaitForm));
            label1 = new Label();
            richTextBox1 = new RichTextBox();
            btnForce = new Button();
            btnReturn = new Button();
            pictureBox1 = new PictureBox();
            TimerCheckLocks = new System.Windows.Forms.Timer(components);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 18F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label1.Location = new Point(126, 18);
            label1.Name = "label1";
            label1.Size = new Size(289, 32);
            label1.TabIndex = 0;
            label1.Text = "Background Task Running";
            // 
            // richTextBox1
            // 
            richTextBox1.BorderStyle = BorderStyle.None;
            richTextBox1.Location = new Point(127, 72);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.ReadOnly = true;
            richTextBox1.Size = new Size(461, 109);
            richTextBox1.TabIndex = 1;
            richTextBox1.Text = resources.GetString("richTextBox1.Text");
            // 
            // btnForce
            // 
            btnForce.Location = new Point(128, 180);
            btnForce.Name = "btnForce";
            btnForce.Size = new Size(75, 23);
            btnForce.TabIndex = 2;
            btnForce.Text = "Force";
            btnForce.UseVisualStyleBackColor = true;
            btnForce.Click += btnForce_Click;
            // 
            // btnReturn
            // 
            btnReturn.Location = new Point(500, 180);
            btnReturn.Name = "btnReturn";
            btnReturn.Size = new Size(75, 23);
            btnReturn.TabIndex = 4;
            btnReturn.Text = "Return";
            btnReturn.UseVisualStyleBackColor = true;
            btnReturn.Click += btnReturn_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = Properties.Resources.caution;
            pictureBox1.Location = new Point(12, 45);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(104, 99);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 5;
            pictureBox1.TabStop = false;
            // 
            // TimerCheckLocks
            // 
            TimerCheckLocks.Enabled = true;
            TimerCheckLocks.Interval = 1000;
            TimerCheckLocks.Tick += CheckLocks;
            // 
            // WaitForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(628, 222);
            ControlBox = false;
            Controls.Add(pictureBox1);
            Controls.Add(btnReturn);
            Controls.Add(btnForce);
            Controls.Add(richTextBox1);
            Controls.Add(label1);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "WaitForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            SizeGripStyle = SizeGripStyle.Hide;
            Text = "Background Task Running";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private RichTextBox richTextBox1;
        private Button btnForce;
        private Button btnReturn;
        private PictureBox pictureBox1;
        private System.Windows.Forms.Timer TimerCheckLocks;
    }
}