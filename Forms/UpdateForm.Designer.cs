
namespace Broadcast.SubForms
{
    partial class UpdateForm
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
            button1 = new Button();
            panel2 = new Panel();
            listBox1 = new ListBox();
            // Replace this line in InitializeComponent (if not already using the above using directive):
            // webView21 = new Microsoft.Web.WebView2.WinForms.WebView2();
            // with:
            webView21 = new Microsoft.Web.WebView2.WinForms.WebView2();
            panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)webView21).BeginInit();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(713, 415);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 2;
            button1.Text = "Close";
            button1.UseVisualStyleBackColor = true;
            button1.MouseClick += CloseForm;
            // 
            // panel2
            // 
            panel2.BackColor = Color.White;
            panel2.Controls.Add(listBox1);
            panel2.Location = new Point(12, 50);
            panel2.Name = "panel2";
            panel2.Padding = new Padding(1);
            panel2.Size = new Size(357, 349);
            panel2.TabIndex = 5;
            // 
            // listBox1
            // 
            listBox1.BorderStyle = BorderStyle.None;
            listBox1.FormattingEnabled = true;
            listBox1.ItemHeight = 15;
            listBox1.Location = new Point(14, 15);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(329, 315);
            listBox1.TabIndex = 0;
            // 
            // webView21
            // 
            webView21.AllowExternalDrop = true;
            webView21.BackColor = Color.White;
            webView21.CreationProperties = null;
            webView21.DefaultBackgroundColor = Color.White;
            webView21.Location = new Point(385, 50);
            webView21.Name = "webView21";
            webView21.Size = new Size(401, 349);
            webView21.TabIndex = 6;
            webView21.ZoomFactor = 1D;
            // 
            // UpdateForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(800, 450);
            Controls.Add(webView21);
            Controls.Add(panel2);
            Controls.Add(button1);
            Name = "UpdateForm";
            Text = "Update";
            panel2.ResumeLayout(false);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            ((System.ComponentModel.ISupportInitialize)webView21).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private Button button1;
        private Panel panel2;
        private ListBox listBox1;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView21;
    }
}