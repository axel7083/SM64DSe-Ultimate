using System.ComponentModel;

namespace SM64DSe.Pages
{
    partial class StartupForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
            this.components = new System.ComponentModel.Container();
            this.recentProjects = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // recentProjects
            // 
            this.recentProjects.ColorDepth = System.Windows.Forms.ColorDepth.Depth24Bit;
            this.recentProjects.ImageSize = new System.Drawing.Size(256, 256);
            this.recentProjects.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // StartupForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(421, 537);
            this.Name = "StartupForm";
            this.Text = "StartupForm";
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.ImageList recentProjects;

        #endregion
    }
}