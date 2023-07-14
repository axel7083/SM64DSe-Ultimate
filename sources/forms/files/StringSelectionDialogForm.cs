using System;
using System.Windows.Forms;

namespace SM64DSe.sources.forms {
    public class StringSelectionDialogForm : Form
    {
        private ListBox listBox;
        //private TextBox searchBox;
        private Button selectButton;

        private string[] items;
        private Action<ushort> onItemSelected;

        public StringSelectionDialogForm(string[] items, Action<ushort> onItemSelected)
        {
            this.items = items;
            this.onItemSelected = onItemSelected;

            InitializeComponent();
            PopulateList();
        }

        private void InitializeComponent()
        {
            this.listBox = new ListBox();
            //this.searchBox = new TextBox();
            this.selectButton = new Button();

            // Set up the list box
            this.listBox.Dock = DockStyle.Fill;
            this.listBox.SelectedIndexChanged += OnItemSelected;

            // Set up the search box
            //this.searchBox.Dock = DockStyle.Top;
            //this.searchBox.TextChanged += OnSearchTextChanged;

            // Set up the select button
            this.selectButton.Dock = DockStyle.Bottom;
            this.selectButton.Text = "Select";
            this.selectButton.Enabled = false;
            this.selectButton.Click += OnSelectButtonClick;

            // Set up the form
            this.Text = "Select a string";
            this.ClientSize = new System.Drawing.Size(300, 300);
            this.Controls.Add(this.listBox);
            this.Controls.Add(this.selectButton);
            //this.Controls.Add(this.searchBox);
        }

        private void PopulateList()
        {
            this.listBox.Items.AddRange(this.items);
        }

        // TODO: 
        /*private void OnSearchTextChanged(object sender, EventArgs e)
        {
            this.listBox.Items.Clear();
            
            // Filter the list box based on the search text
            string searchText = this.searchBox.Text.ToLower();
            
            if (searchText == "")
            {
                this.listBox.Items.AddRange(items);
                return;
            }

            this.listBox.BeginUpdate();
            foreach (var item in items)
            {
                if (item.Contains(searchText))
                    this.listBox.Items.Add(item);
            }
            
            this.listBox.EndUpdate();
        }*/

        private void OnItemSelected(object sender, EventArgs e)
        {
            // Enable the select button if exactly one item is selected
            this.selectButton.Enabled = this.listBox.SelectedItems.Count == 1;
        }

        private void OnSelectButtonClick(object sender, EventArgs e)
        {
            // Call the callback with the selected item
            string selectedItem = this.listBox.SelectedItem.ToString();
            this.onItemSelected((ushort) this.listBox.SelectedIndex);

            // Close the dialog
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
