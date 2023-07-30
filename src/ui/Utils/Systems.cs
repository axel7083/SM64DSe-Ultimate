using System.Windows.Forms;

namespace SM64DSe.ui.Utils
{
    public static class Systems
    {
        public static string pickFolder(string description)
        {
            FolderBrowserDialog fd = new FolderBrowserDialog();
            fd.Description = description;
            return fd.ShowDialog() == DialogResult.OK ? fd.SelectedPath : null;
        }

        public static string pickFile(string filename, string defaultExt, string filter)
        {
            OpenFileDialog mOpenFileDialogue = new OpenFileDialog();
            
            mOpenFileDialogue.FileName = filename;
            mOpenFileDialogue.DefaultExt = defaultExt;
            mOpenFileDialogue.Filter = filter;

            if (mOpenFileDialogue.ShowDialog() == DialogResult.OK)
            {
                return mOpenFileDialogue.FileName;
            }

            return null;
        }
    }
}