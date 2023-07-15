using System.IO;

namespace SM64DSe.core.Api
{
    public class OverlaysManager : Manager
    {
        public OverlaysManager(NitroROM m_ROM): base(m_ROM) { }
        
        public void DumpAll()
        {
            var overlaysCount = this.m_ROM.m_OverlayEntries.Length;
            for (var i = 0; i < overlaysCount; i++)
            {
                NitroOverlay overlay = new NitroOverlay(m_ROM, (uint)i);
                string filename = "DecompressedOverlays/overlay_" + i.ToString("0000") + ".bin";
                string dir = "DecompressedOverlays";
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.WriteAllBytes(filename, overlay.m_Data);
            }
        }

        public void Decompress(uint ovlId)
        {
            NitroOverlay ovl = new NitroOverlay(m_ROM, ovlId);
            ovl.SaveChanges();
        }

        // Platform specific
        public void Dump(uint ovlId, string filename)
        {
            File.WriteAllBytes(filename, new NitroOverlay(Program.m_ROM, ovlId).m_Data);
        }

        // Platform specific
        public void Replace(uint ovlId, string filename)
        {
            NitroOverlay ovl = new NitroOverlay(Program.m_ROM, ovlId);
            ovl.Clear();
            ovl.WriteBlock(0, System.IO.File.ReadAllBytes(filename));
            ovl.SaveChanges();
        }
    }
}