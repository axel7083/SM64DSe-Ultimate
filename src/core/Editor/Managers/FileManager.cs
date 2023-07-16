using System.Collections.Generic;
using System.Windows.Forms;

namespace SM64DSe.core.Api
{
    public class FileManager: Manager
    {
        public FileManager(NitroROM m_ROM): base(m_ROM) { }

        public NitroROM.FileEntry[] GetFileEntries()
        {
            return this.m_ROM.GetFileEntries();
        }
        
        public NitroROM.DirEntry[] GetDirEntries()
        {
            return this.m_ROM.GetDirEntries();
        }

        public NARC CreateNarcInstance(ushort id)
        {
            return new NARC(this.m_ROM, id);
        }

        public ushort GetFileIDFromName(string name)
        {
            return this.m_ROM.GetFileIDFromName(name);
        }

        public NitroFile GetFileFromName(string name)
        {
            return this.m_ROM.GetFileFromName(name);
        }

        public void DecompressLz77WithHeader(string filename)
        {
            NitroFile file = GetFileFromName(filename);
            file.SaveChanges();
        }

        public void DecompressLz77(string filename)
        {
            NitroFile file = GetFileFromName(filename);
            file.ForceDecompression();
            file.SaveChanges();
        }

        public void CompressLz77(string filename)
        {
            NitroFile file = GetFileFromName(filename);
            file.ForceCompression();
            file.SaveChanges();
        }
        
        public void CompressLz77WithHeader(string filename)
        {
            NitroFile file = GetFileFromName(filename);
            file.Compress();
            file.SaveChanges();
        }

        public ushort GetDirIDFromName(string filename)
        {
            return this.m_ROM.GetDirIDFromName(filename);
        }

        public void Replace(ushort fileId, string filename)
        {
            NitroFile file = new NitroFile(m_ROM, fileId);

            file.Clear();
            file.WriteBlock(0, System.IO.File.ReadAllBytes(filename));
            file.SaveChanges();
        }

        public bool StartFilesystemEdit()
        {
            return this.m_ROM.StartFilesystemEdit();
        }

        public void StopFilesystemEdit(bool save)
        {
            if (save)
                this.m_ROM.SaveFilesystem();
            else
                this.m_ROM.RevertFilesystem();
            this.m_ROM.EndRW();
        }

        public void RenameDir(string dir, string newName, TreeNode root)
        {
            m_ROM.RenameDir(dir, newName, root);
        }

        public void RenameFile(string file, string newName, TreeNode root)
        {
            m_ROM.RenameFile(file, newName, root);
        }

        public void RemoveDir(string dir, TreeNode root)
        {
            m_ROM.RemoveDir(dir, root);
        }

        public void RemoveFile(string file, TreeNode root)
        {
            m_ROM.RemoveFile(file, root);
        }

        public void AddFile(
            string path,
            List<string> filenames,
            List<string> fullNames,
            TreeNode root)
        {
            m_ROM.AddFile(path, filenames, fullNames, root);
        }

        public void AddDir(string path, string name, TreeNode root)
        {
            m_ROM.AddDir(path, name, root);
        }
    }
}