using System.Windows.Forms;

namespace SharpDXExample
{
    partial class Form1
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
            labelTop = new Label();
            labelBottom = new Label();
            dxHost = new GfxControls.Forms.D3D11Host();
            SuspendLayout();
            // 
            // labelTop
            // 
            labelTop.Dock = DockStyle.Top;
            labelTop.Font = new System.Drawing.Font("Segoe UI", 16F);
            labelTop.Location = new System.Drawing.Point(0, 0);
            labelTop.Name = "labelTop";
            labelTop.Size = new System.Drawing.Size(800, 30);
            labelTop.TabIndex = 1;
            labelTop.Text = "SharpDX WinForms Example";
            labelTop.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelBottom
            // 
            labelBottom.Dock = DockStyle.Bottom;
            labelBottom.Font = new System.Drawing.Font("Segoe UI", 16F);
            labelBottom.Location = new System.Drawing.Point(0, 420);
            labelBottom.Name = "labelBottom";
            labelBottom.Size = new System.Drawing.Size(800, 30);
            labelBottom.TabIndex = 2;
            labelBottom.Text = "label1";
            labelBottom.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // dxHost
            // 
            dxHost.Dock = DockStyle.Fill;
            dxHost.Location = new System.Drawing.Point(0, 30);
            dxHost.Name = "dxHost";
            dxHost.Size = new System.Drawing.Size(800, 390);
            dxHost.TabIndex = 3;
            dxHost.Text = "d3D11Host1";
            // 
            // Form1
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(dxHost);
            Controls.Add(labelBottom);
            Controls.Add(labelTop);
            Name = "Form1";
            Text = "D3D11Host SharpDX WPF Example";
            ResumeLayout(false);
        }

        #endregion

        private Label labelTop;
        private Label labelBottom;
        private GfxControls.Forms.D3D11Host dxHost;
    }
}
