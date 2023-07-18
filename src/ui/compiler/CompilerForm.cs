using System;
using System.Drawing;
using System.Windows.Forms;
using SM64DSe.core.Api;
using SM64DSe.core.models;

namespace SM64DSe.ui.compiler
{
    public partial class CompilerForm : Form
    {
        
        private ColumnHeader numberColumnHeader;
        private ColumnHeader textColumnHeader;
        private ColumnHeader buildStatusColumnHeader;
        
        private void AddListViewItem(string number, string text, string status)
        {
            ListViewItem item = new ListViewItem(number);
            item.SubItems.Add(text);
            
            item.BackColor = Color.Chocolate;
            item.SubItems.Add(status);

            compiledOverlaysList.Items.Add(item);
        }
        
        public CompilerForm()
        {
            InitializeComponent();
            
            this.compiledOverlaysList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.compiledOverlaysList.FullRowSelect = true;
            this.compiledOverlaysList.HideSelection = false;
            this.compiledOverlaysList.TabIndex = 0;
            this.compiledOverlaysList.UseCompatibleStateImageBehavior = false;
            this.compiledOverlaysList.View = System.Windows.Forms.View.Details;

            
            this.numberColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.textColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.buildStatusColumnHeader = new System.Windows.Forms.ColumnHeader();
            
            // Number Column Header
            this.numberColumnHeader.Text = "Target";
            this.numberColumnHeader.Width = 80;

            // Text Column Header
            this.textColumnHeader.Text = "Description";
            this.textColumnHeader.Width = 300;

            // Button Column Header
            this.buildStatusColumnHeader.Text = "Build status";
            this.buildStatusColumnHeader.Width = 125;

            Resize += OnResize;
            forceResize();

            updateOverlaysList();
        }

        private void updateOverlaysList()
        {
            this.compiledOverlaysList.Clear();
            this.compiledOverlaysList.Columns.AddRange(new[] {
                this.numberColumnHeader,
                this.textColumnHeader,
                this.buildStatusColumnHeader
            });
            Program.romEditor.GetManager<CompilerManager>().targets.ForEach(target =>
            {
                AddListViewItem("[" + target.OverlayId + "]", target.Path, target.Status.ToString());
            });
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string path = ui.Utils.Systems.pickFolder("Choose a target");
            if (path == null)
                return;

            Program.romEditor.GetManager<CompilerManager>().addTarget(
                new CompilerTarget(154, path, CompilationStatus.PENDING)
                );
            updateOverlaysList();
        }

        private void forceResize()
        {
            if(textColumnHeader != null)
                textColumnHeader.Width = compiledOverlaysList.Width - numberColumnHeader.Width - buildStatusColumnHeader.Width;
        }

        private void OnResize(object sender, EventArgs e)
        {
            forceResize();
        }
        
        private void buildButton_Click(object sender, EventArgs e)
        {
            Program.romEditor.GetManager<CompilerManager>().build();
            updateOverlaysList();
        }
    }
}