namespace SM64DSe.core.Api
{
    public class FileManager : Manager
    {
        public FileManager(NitroROM m_ROM): base(m_ROM) { }

        public NitroROM.OverlayEntry[] GetOverlayEntries()
        {
            return this.m_ROM.GetOverlayEntries();
        }
        
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
    }
}