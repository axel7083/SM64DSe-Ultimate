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
    }
}