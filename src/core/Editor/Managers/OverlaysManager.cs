using System;
using System.IO;
using System.Linq;
using Serilog;

namespace SM64DSe.core.Api
{
    public class OverlaysManager : Manager
    {
        public OverlaysManager(NitroROM m_ROM) : base(m_ROM)
        {
        }

        public void DumpAll()
        {
            var overlaysCount = m_ROM.m_OverlayEntries.Length;
            for (var i = 0; i < overlaysCount; i++)
            {
                var overlay = new NitroOverlay(m_ROM, (uint)i);
                var filename = "DecompressedOverlays/overlay_" + i.ToString("0000") + ".bin";
                var dir = "DecompressedOverlays";
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.WriteAllBytes(filename, overlay.m_Data);
            }
        }

        public NitroROM.OverlayEntry[] GetOverlayEntries()
        {
            return m_ROM.GetOverlayEntries();
        }

        public void Decompress(uint ovlId)
        {
            var ovl = new NitroOverlay(m_ROM, ovlId);
            ovl.SaveChanges();
        }

        // Platform specific
        public void Dump(uint ovlId, string filename)
        {
            File.WriteAllBytes(filename, new NitroOverlay(m_ROM, ovlId).m_Data);
        }

        // Platform specific
        public void Replace(uint ovlId, string filename)
        {
            var ovl = new NitroOverlay(m_ROM, ovlId);
            ovl.Clear();
            ovl.WriteBlock(0, File.ReadAllBytes(filename));
            ovl.SaveChanges();
        }
        
        public void Replace(uint ovlId, byte[] data)
        {
            var ovl = new NitroOverlay(m_ROM, ovlId);
            ovl.Clear();
            ovl.WriteBlock(0, data);
            ovl.SaveChanges();
        }

        public uint GetLevelOverlayID(int levelId)
        {
            return m_ROM.GetLevelOverlayID(levelId);
        }

        public NitroOverlay GetOverlay(uint ovlId)
        {
            return new NitroOverlay(m_ROM, ovlId);
        }

        public NitroFile GetCollisionFileID(uint ovlId)
        {
            var currentOverlay = new NitroOverlay(m_ROM, ovlId);
            return m_ROM.GetFileFromInternalID(currentOverlay.GetCollisionFileID());
        }

        public uint GetOverlayCount()
        {
            return m_ROM.getOverlayCount();
        }

        public uint CreateNewOverlay(uint ramAddress = 0)
        {
            if (!m_ROM.StartFilesystemEdit())
                throw new Exception("Cannot edit filesystem.");
            
            uint nOvlId = GetOverlayCount() + 1; 
            Log.Information("Creating new overlay: " + nOvlId + ".");
            
            var entries = new NitroROM.OverlayEntry[nOvlId];
            m_ROM.m_OverlayEntries.CopyTo(entries, 0);
            entries[nOvlId - 1] = new NitroROM.OverlayEntry()
            {
                FileID = (ushort)m_ROM.m_FileEntries.Count(),
                BSSSize = 0,
                EntryOffset = m_ROM.OVTOffset * (nOvlId - 1) * 0x20,
                Flags = 0,
                ID = nOvlId,
                RAMAddress = ramAddress,
                RAMSize = 0x20,
                StaticInitEnd = 4,
                StaticInitStart = 0
            };
            m_ROM.m_OverlayEntries = entries;
            
            // Add to file system
            var nFileId = Program.romEditor.GetManager<FileManager>().CreateNewFile();
            
            m_ROM.OVTSize = (uint)(0x20 * nOvlId);
            m_ROM.FATSize = (uint)(0x8 * nFileId);
            m_ROM.RewriteSizeTables();
            m_ROM.SaveFilesystem();
            m_ROM.EndRW();
            return nOvlId;
        }
    }
}