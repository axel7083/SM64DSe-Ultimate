using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Serilog;
using SM64DSe.core.Api;

namespace SM64DSe.Patcher {
    
    /// <summary>
    /// Handles ARM 9 binaries.
    /// </summary>
    public class Arm9BinaryHandler {

        /// <summary>
        /// Directory for ASM patch stuff.
        /// </summary>
        public string ASMDir;

        public List<Arm9BinSection> sections;
        Arm9BinSection nullSection;
        
        public void newSection(int ramAddr, int ramLen, int fileOffs, int bssSize) {
            Log.Information(String.Format("Found section: {0:X8} - {1:X8} - {2:X8}", ramAddr, ramAddr + ramLen, ramAddr + ramLen + bssSize));
            
            byte[] data = Program.m_ROM.ReadBlock(Program.m_ROM.headerSize, ramLen);
            Arm9BinSection s = new Arm9BinSection(data, ramAddr, bssSize);
            sections.Add(s);
        }

        public void loadSections()
        {
            Log.Information("Arm9BinaryHandler: Loading sections.");
            
            sections = new List<Arm9BinSection>();

            uint copyTableBegin = (uint)(Program.m_ROM.Read32(getCodeSettingsOffs() + 0x00) - Program.m_ROM.ARM9RAMAddress); // 642912 => 3584 ??
            int copyTableEnd = (int)(Program.m_ROM.Read32(getCodeSettingsOffs() + 0x04) - Program.m_ROM.ARM9RAMAddress); // 642936
            int dataBegin = (int)(Program.m_ROM.Read32(getCodeSettingsOffs() + 0x08) - Program.m_ROM.ARM9RAMAddress); // 618496
            
            Log.Information(String.Format("Sections infos:\n- Table Begin: {0:X8}\n- Table End: {1:X8}\n- Data Begin: {2:X8}", copyTableBegin, copyTableEnd, dataBegin));

            newSection((int)Program.m_ROM.ARM9RAMAddress, dataBegin, 0x0, 0);
            sections[0].real = false;

            while (copyTableBegin < copyTableEnd) {
                int start = (int)Program.m_ROM.Read32(copyTableBegin + Program.m_ROM.headerSize);
                copyTableBegin += 4;
                int size = (int)Program.m_ROM.Read32(copyTableBegin + Program.m_ROM.headerSize);
                copyTableBegin += 4;
                int bsssize = (int)Program.m_ROM.Read32(copyTableBegin + Program.m_ROM.headerSize);
                copyTableBegin += 4;

                newSection(start, size, dataBegin, bsssize);
                dataBegin += size;
            }
        }
        //020985f0 02098620
        public void saveSections()
        {
            Log.Information("Saving sections...");
            Program.m_ROM.BeginRW();
            MemoryStream o = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(o);
            foreach (Arm9BinSection s in sections) {
                Log.Information(String.Format("Found section: {0:X8} - {1:X8} - {2:X8}",
                    s.ramAddr, s.ramAddr + s.len, s.ramAddr + s.len + s.bssSize));

                bw.Write(s.data);
                while (o.Length % 4 != 0) {
                    bw.Write((byte)0);
                }
            }

            uint sectionTableAddr = Program.m_ROM.ARM9RAMAddress + 0xE00; // 33574400 ???
            MemoryStream o2 = new MemoryStream();
            BinaryWriter bw2 = new BinaryWriter(o2);
            foreach (Arm9BinSection s in sections) {
                if (!s.real) continue;
                if (s.len == 0) continue;
                bw2.Write((uint)s.ramAddr);
                bw2.Write((uint)s.len);
                bw2.Write((uint)s.bssSize);
            }

            //Write BSS sections last
            //because they overwrite huge areas with zeros (?)
            foreach (Arm9BinSection s in sections) {
                if (!s.real) continue;
                if (s.len != 0) continue;
                bw2.Write((uint)s.ramAddr);
                bw2.Write((uint)s.len);
                bw2.Write((uint)s.bssSize);
            }

            byte[] data = o.ToArray();
            byte[] sectionTable = o2.ToArray();
            Array.Copy(sectionTable, 0, data, sectionTableAddr - Program.m_ROM.ARM9RAMAddress, sectionTable.Length);
            
            // THIS VERY IMPORTANT, TODO: ensure it works. ?????
            // Probably not working
            // when comparing with NSMBe4 we got a big difference from 0x009B000 to 0x00A0F50
            Program.m_ROM.WriteBlock(Program.m_ROM.headerSize, data);

            Program.m_ROM.Write32((getCodeSettingsOffs() + 0x00), (uint)sectionTableAddr);

            uint tableEnd = (uint)o2.Position + sectionTableAddr;
            
            Program.m_ROM.Write32((getCodeSettingsOffs() + 0x04), tableEnd);

            uint dataBegin = (uint)(sections[0].len + Program.m_ROM.ARM9RAMAddress);
            Program.m_ROM.Write32((getCodeSettingsOffs() + 0x08), dataBegin);
            
            Log.Information(
                String.Format("New sections infos:\n- Table Begin: {0:X8}\n- Table End: {1:X8}\n- Data Begin: {2:X8}", 
                    sectionTableAddr - Program.m_ROM.ARM9RAMAddress, 
                    tableEnd - Program.m_ROM.ARM9RAMAddress, 
                    dataBegin - Program.m_ROM.ARM9RAMAddress
                    ));
            Log.Information("Saving complete.");
        }

        private int _codeSettingsOffs = -1;

        public uint getCodeSettingsOffs() {
            // Find the end of the settings
            // This old method doesn't work with The Legendary Starfy :\ -Treeki
            //return (int)(f.getUintAt(0x90C) - ROM.arm9RAMAddress);
            if (_codeSettingsOffs == -1) {
                for (uint i = 0; i < 0x8000; i += 4) {
                    if (Program.m_ROM.Read32(i) == 0xDEC00621 && Program.m_ROM.Read32(i + 4) == 0x2106C0DE) {
                        _codeSettingsOffs = (int)i - 0x1C;
                        break;
                    }
                }
            }

            return (uint)_codeSettingsOffs; // 2776
        }


        public int decompressionRamAddr {
            get {
                int value = (int)Program.m_ROM.Read32(getCodeSettingsOffs() + 0x14 - Program.m_ROM.ARM9RAMAddress);
                return value;
            }

            set {
                Program.m_ROM.Write32(getCodeSettingsOffs() + 0x14 - Program.m_ROM.ARM9RAMAddress, (uint)value);
            }
        }

        public bool isCompressed {
            get {
                return decompressionRamAddr != 0;
            }
        }

        public void writeToRamAddr(int ramAddr, uint val, int ovlId) //DY: added numBytes parameter
        {
            if (ovlId != -1)
            {
                NitroROM.OverlayEntry overlayEntry = Program.m_ROM.m_OverlayEntries[ovlId];
                if (overlayEntry.RAMAddress <= ramAddr && overlayEntry.RAMSize + overlayEntry.RAMAddress > ramAddr)
                {
                    NitroOverlay overlay = Program.romEditor.GetManager<OverlaysManager>().GetOverlay(overlayEntry.ID);
                    uint addr = (uint)(ramAddr - overlay.m_RAMAddr);
                    overlay.Write8(addr, (byte)val);
                    overlay.Write8(addr + 1, (byte)(val >> 8));
                    overlay.Write8(addr + 2, (byte)(val >> 16));
                    overlay.Write8(addr + 3, (byte)(val >> 24));
                }
            }
                
            foreach (Arm9BinSection s in sections)
                if (s.containsRamAddr(ramAddr)) {
                    s.writeToRamAddr(ramAddr, val, 4);    //DY: added numBytes parameter
                    return;
                }
            
            throw new Exception("WRITE: Addr " + ramAddr + " is not in arm9 binary or overlays");
        }

        public uint readFromRamAddr(int ramAddr, int ovlId)
        {
            if(ovlId != -1)
            {
                NitroROM.OverlayEntry overlay = Program.m_ROM.m_OverlayEntries[ovlId];
                if (overlay.RAMAddress <= ramAddr && overlay.RAMSize + overlay.RAMAddress > ramAddr)
                {
                    byte[] data = Program.romEditor.GetManager<OverlaysManager>().GetOverlay(overlay.ID)
                        .ReadBlock((uint)ramAddr, 4);
                    return (uint)(data[0] | (data[1]<<8) | (data[2]<<16) | (data[3]<<24)); 
                }

                throw new Exception("READ: Overlay ID "+ovlId+" not found :(");
            }
            else
            {
                foreach (Arm9BinSection s in sections)
                    if(s.containsRamAddr(ramAddr))
                        return s.readFromRamAddr(ramAddr);
                
                throw new Exception("READ: Addr "+ramAddr+" is not in arm9 binary");
            }
        }
    }

}
