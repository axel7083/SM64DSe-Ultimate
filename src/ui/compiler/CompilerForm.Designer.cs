using System.ComponentModel;

namespace SM64DSe.ui.compiler
{
    partial class CompilerForm
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
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("");
            this.addtarget = new System.Windows.Forms.Button();
            this.patches = new System.Windows.Forms.TabPage();
            this.TabContainer = new System.Windows.Forms.TabControl();
            this.overlays = new System.Windows.Forms.TabPage();
            this.compiledOverlaysList = new System.Windows.Forms.ListView();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.buildButton = new System.Windows.Forms.Button();
            this.TabContainer.SuspendLayout();
            this.overlays.SuspendLayout();
            this.SuspendLayout();
            // 
            // addtarget
            // 
            this.addtarget.Location = new System.Drawing.Point(12, 10);
            this.addtarget.Name = "addtarget";
            this.addtarget.Size = new System.Drawing.Size(80, 28);
            this.addtarget.TabIndex = 2;
            this.addtarget.Text = "Add target";
            this.addtarget.UseVisualStyleBackColor = true;
            this.addtarget.Click += new System.EventHandler(this.button1_Click);
            // 
            // patches
            // 
            this.patches.Location = new System.Drawing.Point(4, 24);
            this.patches.Name = "patches";
            this.patches.Padding = new System.Windows.Forms.Padding(3);
            this.patches.Size = new System.Drawing.Size(202, 313);
            this.patches.TabIndex = 1;
            this.patches.Text = "Patches";
            this.patches.UseVisualStyleBackColor = true;
            // 
            // TabContainer
            // 
            this.TabContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.TabContainer.Controls.Add(this.overlays);
            this.TabContainer.Controls.Add(this.patches);
            this.TabContainer.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TabContainer.Location = new System.Drawing.Point(12, 44);
            this.TabContainer.Name = "TabContainer";
            this.TabContainer.SelectedIndex = 0;
            this.TabContainer.Size = new System.Drawing.Size(567, 339);
            this.TabContainer.TabIndex = 1;
            // 
            // overlays
            // 
            this.overlays.Controls.Add(this.compiledOverlaysList);
            this.overlays.Location = new System.Drawing.Point(4, 24);
            this.overlays.Name = "overlays";
            this.overlays.Padding = new System.Windows.Forms.Padding(3);
            this.overlays.Size = new System.Drawing.Size(559, 311);
            this.overlays.TabIndex = 0;
            this.overlays.Text = "Overlays";
            this.overlays.UseVisualStyleBackColor = true;
            // 
            // compiledOverlaysList
            // 
            this.compiledOverlaysList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.compiledOverlaysList.Items.AddRange(new System.Windows.Forms.ListViewItem[] { listViewItem1 });
            this.compiledOverlaysList.Location = new System.Drawing.Point(3, 3);
            this.compiledOverlaysList.Name = "compiledOverlaysList";
            this.compiledOverlaysList.Size = new System.Drawing.Size(553, 305);
            this.compiledOverlaysList.TabIndex = 0;
            this.compiledOverlaysList.UseCompatibleStateImageBehavior = false;
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(23, 23);
            this.toolStripButton1.Text = "toolStripButton1";
            // 
            // buildButton
            // 
            this.buildButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buildButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.buildButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.buildButton.ForeColor = System.Drawing.Color.Black;
            this.buildButton.Image = global::SM64DSe.Properties.Resources.btnA1;
            this.buildButton.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.buildButton.Location = new System.Drawing.Point(506, 10);
            this.buildButton.Name = "buildButton";
            this.buildButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.buildButton.Size = new System.Drawing.Size(66, 28);
            this.buildButton.TabIndex = 3;
            this.buildButton.Text = "Build";
            this.buildButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buildButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.buildButton.UseVisualStyleBackColor = true;
            this.buildButton.Click += new System.EventHandler(this.buildButton_Click);
            // 
            // CompilerForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(591, 395);
            this.Controls.Add(this.buildButton);
            this.Controls.Add(this.addtarget);
            this.Controls.Add(this.TabContainer);
            this.Location = new System.Drawing.Point(15, 15);
            this.Name = "CompilerForm";
            this.TabContainer.ResumeLayout(false);
            this.overlays.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.ToolStripButton toolStripButton1;

        private System.Windows.Forms.Button addtarget;

        private System.Windows.Forms.ListView compiledOverlaysList;

        private System.Windows.Forms.Button buildButton;

        private System.Windows.Forms.TabControl TabContainer;
        private System.Windows.Forms.TabPage overlays;
        private System.Windows.Forms.TabPage patches;

        #endregion
    }
}