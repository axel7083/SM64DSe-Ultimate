/*
    Copyright 2012 Kuribo64

    This file is part of SM64DSe.

    SM64DSe is free software: you can redistribute it and/or modify it under
    the terms of the GNU General Public License as published by the Free
    Software Foundation, either version 3 of the License, or (at your option)
    any later version.

    SM64DSe is distributed in the hope that it will be useful, but WITHOUT ANY 
    WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
    FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along 
    with SM64DSe. If not, see http://www.gnu.org/licenses/.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace SM64DSe.sources.forms
{
    public partial class TextEditorForm : Form
    {
        public TextEditorForm()
        {
            InitializeComponent();
        }

        private Dictionary<string, string> languagesDict;
        private string currentLanguage = null;
        private bool copyMessage;
        
        private void TextEditorForm_Load(object sender, EventArgs e)
        {
            languagesDict = Program.m_ROM.textEditor.GetLanguages();
            foreach (var language in languagesDict)
            {
                btnLanguages.DropDownItems.Add(language.Key);
            }
        }

        private int selectedIndex;
        private void lbxMsgList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbxMsgList.SelectedIndex != -1)
            {
                string selectedText = lbxMsgList.Items[lbxMsgList.SelectedIndex].ToString();
                selectedIndex = Int32.Parse(selectedText.Substring(1, 4), System.Globalization.NumberStyles.HexNumber);

                var details = Program.m_ROM.textEditor.getMessageDetails(selectedIndex);
                tbxMsgPreview.Text = details.message;
                width_numeric.Value = details.width;
                height_numeric.Value = details.height;

                if (copyMessage)
                    txtEdit.Text = details.message;
            }
        }

        private void btnUpdateString_Click(object sender, EventArgs e)
        {
            if (lbxMsgList.SelectedIndex != -1)
            {
                Program.m_ROM.textEditor.UpdateEntries(txtEdit.Text, selectedIndex);
                lbxMsgList.Items[lbxMsgList.SelectedIndex] = Program.m_ROM.textEditor.getMessageDetails(selectedIndex).shortMessage;
            }
        }

        private void btnSaveAll_Click(object sender, EventArgs e)
        {
            Program.m_ROM.textEditor.WriteData();

            int index = lbxMsgList.SelectedIndex;
            lbxMsgList.SelectedIndex = index;
            Program.m_ROM.UpdateStrings();
        }
        
        private void btnCoins_Click(object sender, EventArgs e)
        {
            txtEdit.Text += "[\\r]C";
        }

        private void btnStarFull_Click(object sender, EventArgs e)
        {
            txtEdit.Text += "[\\r]S";
        }

        private void btnStarEmpty_Click(object sender, EventArgs e)
        {
            txtEdit.Text += "[\\r]s";
        }

        private void btnDPad_Click(object sender, EventArgs e)
        {
            // FE05030000 doesn't always appear, depends on message
            txtEdit.Text += "[\\r]D";
        }

        private void btnA_Click(object sender, EventArgs e)
        {
            txtEdit.Text += "[\\r]A";
        }

        private void btnB_Click(object sender, EventArgs e)
        {
            txtEdit.Text += "[\\r]B";
        }

        private void btnX_Click(object sender, EventArgs e)
        {
            txtEdit.Text += "[\\r]X";
        }

        private void btnY_Click(object sender, EventArgs e)
        {
            txtEdit.Text += "[\\r]Y";
        }

        private void btnL_Click(object sender, EventArgs e)
        {
            txtEdit.Text += "[\\r]FE05030005";
        }

        private void btnR_Click(object sender, EventArgs e)
        {
            txtEdit.Text += "[\\r]FE05030006";
        }

        void btnLanguages_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            currentLanguage = e.ClickedItem.Text;
            loadContent();
            btnImport.Enabled = true; btnExport.Enabled = true;
        }

        private void loadContent()
        {
            if (currentLanguage == null)
                return;
            
            Program.m_ROM.textEditor.LoadLanguagesFile(languagesDict[currentLanguage]);

            var shortMessages = Program.m_ROM.textEditor.GetAllShortMessages();
            
            lbxMsgList.Items.Clear();
            lbxMsgList.Items.AddRange(shortMessages);
        }
        
        private void btnHelp_Click(object sender, EventArgs e)
        {
            MessageBox.Show("To begin editing, select a language using the drop-down menu. This will display \n" +
                            "all of the languages available for your ROM Version.\n\n" +
                            "Next, click on the string you want to edit on the left-hand side.\n" +
                            "The full text will then be displayed in the upper-right box.\n\n" +
                            "Type your new text in the text box on the right-hand side.\n" +
                            "When done editing an entry, click 'Update String'.\n\nWhen you have finished, click " +
                            "on 'Save Changes'\n\n" +
                            "Use the buttons under the text editing box to insert the special characters.\n" + 
                            "[\\r] is the special character used by the text editor to indicate special characters.\n");
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            ImportXML();
        }

        private void ImportXML()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "XML Document (.xml)|*.xml";//Filter by .xml
            DialogResult dlgeResult = ofd.ShowDialog();
            if (dlgeResult == DialogResult.Cancel)
                return;

            lbxMsgList.Items.Clear();
            lbxMsgList.BeginUpdate();

            using (XmlReader reader = XmlReader.Create(ofd.FileName))
            {
                Program.m_ROM.textEditor.ImportXML(reader);
            }

            loadContent();
        }

        private void ExportXML()
        {
            SaveFileDialog saveXML = new SaveFileDialog();
            saveXML.FileName = "SM64DS Texts";//Default name
            saveXML.DefaultExt = ".xml";//Default file extension
            saveXML.Filter = "XML Document (.xml)|*.xml";//Filter by .xml
            if (saveXML.ShowDialog() == DialogResult.Cancel)
                return;

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "  ";
            settings.NewLineChars = "\r\n";
            settings.NewLineHandling = NewLineHandling.Replace;
            using (XmlWriter writer = XmlWriter.Create(saveXML.FileName, settings))
            {
                Program.m_ROM.textEditor.ExportXML(writer);
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            ExportXML();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (currentLanguage == null)
                return;

            string searchString = txtSearch.Text;
            if (searchString == null || searchString.Equals(""))
            {
                lbxMsgList.BeginUpdate();
                lbxMsgList.Items.Clear();

                lbxMsgList.Items.AddRange(Program.m_ROM.textEditor.GetAllShortMessages());

                lbxMsgList.EndUpdate();
            }
            else
            {
                lbxMsgList.BeginUpdate();
                lbxMsgList.Items.Clear();

                var m_MsgData = Program.m_ROM.textEditor.GetAllMessages();
                
                string searchStringLower = searchString.ToLowerInvariant();
                List<int> matchingIndices = new List<int>();
                for (int i = 0; i < m_MsgData.Length; i++)
                {
                    if (m_MsgData[i].ToLowerInvariant().Contains(searchStringLower))
                        matchingIndices.Add(i);
                }

                var shortMessages = Program.m_ROM.textEditor.GetAllShortMessages();
                foreach (int index in matchingIndices)
                {
                    lbxMsgList.Items.Add(shortMessages[index]);
                }
                
                lbxMsgList.EndUpdate();
            }
        }

        private void width_numeric_ValueChanged(object sender, EventArgs e)
        {
            if (lbxMsgList.SelectedIndex != -1)
                Program.m_ROM.textEditor.updateWidth((ushort)width_numeric.Value, selectedIndex);
        }

        private void height_numeric_ValueChanged(object sender, EventArgs e)
        {
            if (lbxMsgList.SelectedIndex != -1)
                Program.m_ROM.textEditor.updateHeight((ushort)height_numeric.Value, selectedIndex);
        }

        private void chkCopy_CheckedChanged(object sender, EventArgs e)
        {
            copyMessage = !copyMessage;
        }
    }
}
