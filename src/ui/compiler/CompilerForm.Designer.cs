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
            this.dls = new System.Windows.Forms.TabPage();
            this.tabs = new System.Windows.Forms.TabControl();
            this.overlays = new System.Windows.Forms.TabPage();
            this.compiledOverlaysList = new System.Windows.Forms.ListView();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.explorerButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.patchTargetFolder = new System.Windows.Forms.TextBox();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.buildButton = new System.Windows.Forms.Button();
            this.tabs.SuspendLayout();
            this.overlays.SuspendLayout();
            this.tabPage1.SuspendLayout();
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
            this.addtarget.Click += new System.EventHandler(this.AddTarget_Click);
            // 
            // dls
            // 
            this.dls.Location = new System.Drawing.Point(4, 24);
            this.dls.Name = "dls";
            this.dls.Padding = new System.Windows.Forms.Padding(3);
            this.dls.Size = new System.Drawing.Size(715, 334);
            this.dls.TabIndex = 1;
            this.dls.Text = "Dynamic Libraries";
            this.dls.UseVisualStyleBackColor = true;
            // 
            // tabs
            // 
            this.tabs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.tabs.Controls.Add(this.overlays);
            this.tabs.Controls.Add(this.dls);
            this.tabs.Controls.Add(this.tabPage1);
            this.tabs.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabs.Location = new System.Drawing.Point(12, 44);
            this.tabs.Name = "tabs";
            this.tabs.SelectedIndex = 0;
            this.tabs.Size = new System.Drawing.Size(723, 362);
            this.tabs.TabIndex = 1;
            // 
            // overlays
            // 
            this.overlays.Controls.Add(this.compiledOverlaysList);
            this.overlays.Location = new System.Drawing.Point(4, 24);
            this.overlays.Name = "overlays";
            this.overlays.Padding = new System.Windows.Forms.Padding(3);
            this.overlays.Size = new System.Drawing.Size(715, 334);
            this.overlays.TabIndex = 0;
            this.overlays.Text = "Overlays";
            this.overlays.UseVisualStyleBackColor = true;
            // 
            // compiledOverlaysList
            // 
            this.compiledOverlaysList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.compiledOverlaysList.Items.AddRange(new System.Windows.Forms.ListViewItem[] { listViewItem1 });
            this.compiledOverlaysList.Location = new System.Drawing.Point(6, 3);
            this.compiledOverlaysList.Name = "compiledOverlaysList";
            this.compiledOverlaysList.Size = new System.Drawing.Size(709, 328);
            this.compiledOverlaysList.TabIndex = 0;
            this.compiledOverlaysList.UseCompatibleStateImageBehavior = false;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.explorerButton);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.patchTargetFolder);
            this.tabPage1.Location = new System.Drawing.Point(4, 24);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(715, 334);
            this.tabPage1.TabIndex = 2;
            this.tabPage1.Text = "Patch configuration";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // explorerButton
            // 
            this.explorerButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.explorerButton.BackColor = System.Drawing.Color.Transparent;
            this.explorerButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.explorerButton.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.explorerButton.FlatAppearance.BorderSize = 0;
            this.explorerButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.explorerButton.ForeColor = System.Drawing.Color.White;
            this.explorerButton.Image = global::SM64DSe.Properties.Resources.folder_icon;
            this.explorerButton.Location = new System.Drawing.Point(669, 31);
            this.explorerButton.Name = "explorerButton";
            this.explorerButton.Size = new System.Drawing.Size(44, 36);
            this.explorerButton.TabIndex = 2;
            this.explorerButton.UseVisualStyleBackColor = false;
            this.explorerButton.Click += new System.EventHandler(this.explorerButton_Click);
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.label1.Location = new System.Drawing.Point(6, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(125, 16);
            this.label1.TabIndex = 1;
            this.label1.Text = "Target Folder";
            // 
            // patchTargetFolder
            // 
            this.patchTargetFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.patchTargetFolder.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.patchTargetFolder.Location = new System.Drawing.Point(6, 39);
            this.patchTargetFolder.Name = "patchTargetFolder";
            this.patchTargetFolder.ReadOnly = true;
            this.patchTargetFolder.Size = new System.Drawing.Size(657, 22);
            this.patchTargetFolder.TabIndex = 0;
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
            this.buildButton.Image = global::SM64DSe.Properties.Resources.btnA1;
            this.buildButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buildButton.Location = new System.Drawing.Point(669, 10);
            this.buildButton.Name = "buildButton";
            this.buildButton.Size = new System.Drawing.Size(61, 28);
            this.buildButton.TabIndex = 3;
            this.buildButton.Text = "Build";
            this.buildButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.buildButton.UseVisualStyleBackColor = true;
            this.buildButton.Click += new System.EventHandler(this.buildButton_Click);
            // 
            // CompilerForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(747, 418);
            this.Controls.Add(this.buildButton);
            this.Controls.Add(this.addtarget);
            this.Controls.Add(this.tabs);
            this.Location = new System.Drawing.Point(15, 15);
            this.Name = "CompilerForm";
            this.tabs.ResumeLayout(false);
            this.overlays.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Button explorerButton;

        private System.Windows.Forms.Button buildButton;

        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TextBox patchTargetFolder;
        private System.Windows.Forms.Label label1;

        private System.Windows.Forms.ToolStripButton toolStripButton1;

        private System.Windows.Forms.Button addtarget;

        private System.Windows.Forms.ListView compiledOverlaysList;

        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.TabPage overlays;
        private System.Windows.Forms.TabPage dls;

        #endregion
    }
}