using System.Collections.Generic;
using System.Windows.Forms;
using Serilog;

namespace SM64DSe.core.Api
{
    public class FileManager : Manager
    {
        public FileManager(NitroROM m_ROM) : base(m_ROM)
        {
        }

        public NitroROM.FileEntry[] GetFileEntries()
        {
            return m_ROM.GetFileEntries();
        }

        public NitroROM.DirEntry[] GetDirEntries()
        {
            return m_ROM.GetDirEntries();
        }

        public NARC CreateNarcInstance(ushort id)
        {
            return new NARC(m_ROM, id);
        }

        public ushort GetFileIDFromName(string name)
        {
            return m_ROM.GetFileIDFromName(name);
        }

        public NitroFile GetFileFromName(string name)
        {
            return m_ROM.GetFileFromName(name);
        }

        public NitroFile GetFileFromId(ushort id)
        {
            return m_ROM.GetFileFromInternalID(id);
        }

        public void DecompressLz77WithHeader(string filename)
        {
            var file = GetFileFromName(filename);
            file.SaveChanges();
        }

        public void DecompressLz77(string filename)
        {
            var file = GetFileFromName(filename);
            file.ForceDecompression();
            file.SaveChanges();
        }

        public void CompressLz77(string filename)
        {
            var file = GetFileFromName(filename);
            file.ForceCompression();
            file.SaveChanges();
        }

        public void CompressLz77WithHeader(string filename)
        {
            var file = GetFileFromName(filename);
            file.Compress();
            file.SaveChanges();
        }

        public ushort GetDirIDFromName(string filename)
        {
            return m_ROM.GetDirIDFromName(filename);
        }

        public void Replace(ushort fileId, string filename)
        {
            var file = new NitroFile(m_ROM, fileId);

            file.Clear();
            file.WriteBlock(0, System.IO.File.ReadAllBytes(filename));
            file.SaveChanges();
        }

        public bool StartFilesystemEdit()
        {
            return m_ROM.StartFilesystemEdit();
        }

        public void StopFilesystemEdit(bool save)
        {
            if (save)
                m_ROM.SaveFilesystem();
            else
                m_ROM.RevertFilesystem();
            m_ROM.EndRW();
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

        public uint CreateNewFile()
        {
            uint nFileId = (uint)(GetFileEntries().Length + 1); 
            Log.Information("Creating new file: " + nFileId + ".");
            
            var entries = new NitroROM.FileEntry[nFileId];
            GetFileEntries().CopyTo(entries, 0);
            entries[nFileId - 1] = new NitroROM.FileEntry()
            {
                FullName = "",
                Name = "",
                Data = new byte[0x20],
                ID = (ushort)nFileId,
                InternalID = 0xFFFF,
                Offset = uint.MaxValue,
                ParentID = 0,
                Size = 0x20
            };
            m_ROM.m_FileEntries = entries;
            return nFileId;
        }
    }
}