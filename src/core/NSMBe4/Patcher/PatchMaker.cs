/*
*   This file is part of NSMB Editor 5.
*
*   NSMB Editor 5 is free software: you can redistribute it and/or modify
*   it under the terms of the GNU General Public License as published by
*   the Free Software Foundation, either version 3 of the License, or
*   (at your option) any later version.
*
*   NSMB Editor 5 is distributed in the hope that it will be useful,
*   but WITHOUT ANY WARRANTY; without even the implied warranty of
*   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*   GNU General Public License for more details.
*
*   You should have received a copy of the GNU General Public License
*   along with NSMB Editor 5.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.IO;
using SM64DSe.core.Api;

namespace SM64DSe.core.NSMBe4.Patcher
{
    public class PatchMaker
    {
        private int ArenaLoOffs;
        Arm9BinaryHandler handler;
        DirectoryInfo targetDir;

        private uint codeAddr;

        public PatchMaker(DirectoryInfo targetDir)
        {
            handler = new Arm9BinaryHandler();
            this.targetDir = targetDir;
        }
        
        public PatchMaker(DirectoryInfo targetDir, uint codeAddr)
        {
            this.codeAddr = codeAddr;
            this.targetDir = targetDir;
        }

		public uint getCodeAddr()
        {
            if (handler == null)
                return this.codeAddr;
            
            handler.load();
            loadArenaLoOffsFile(targetDir);
            uint codeAddr = handler.readFromRamAddr(ArenaLoOffs, -1);
            return codeAddr;
		}
		
		public void restore()
		{
            handler.restoreFromBackup();
		}
		
        public void compilePatch()
        {
            uint codeAddr = getCodeAddr();
            PatchCompiler.compilePatch(codeAddr, targetDir);
        }

        public void generatePatch()
        {
        	int codeAddr = (int) getCodeAddr();
            Console.Out.WriteLine(String.Format("New code address: {0:X8}", codeAddr));

            FileInfo f = new FileInfo(targetDir.FullName + "/newcode.bin");
            if (!f.Exists) return;
            FileStream fs = f.OpenRead();

            byte[] newdata = new byte[fs.Length];
            fs.Read(newdata, 0, (int)fs.Length);
            fs.Close();

            NSMBe4ByteArrayOutputStream extradata = new NSMBe4ByteArrayOutputStream();

            extradata.write(newdata);
            extradata.align(4);
            int hookAddr = codeAddr + extradata.getPos();


            f = new FileInfo(targetDir.FullName + "/newcode.sym");
            StreamReader s = f.OpenText();

            while (!s.EndOfStream)
            {
                string l = s.ReadLine();

                int ind = -1;
                if (l.Contains("nsub_"))
                    ind = l.IndexOf("nsub_");
                if (l.Contains("hook_"))
                    ind = l.IndexOf("hook_");
                if (l.Contains("repl_"))
                    ind = l.IndexOf("repl_");

                if (ind != -1)
                {
                    int destRamAddr= parseHex(l.Substring(0, 8));    //Redirect dest addr
                    int ramAddr = parseHex(l.Substring(ind + 5, 8)); //Patched addr
                    uint val = 0;

                    int ovId = -1;
                    if (l.Contains("_ov_"))
                        ovId = parseHex(l.Substring(l.IndexOf("_ov_") + 4, 2));

                    int patchCategory = 0;

                    string cmd = l.Substring(ind, 4);
                    int thisHookAddr = 0;

                    switch(cmd)
                    {
                        case "nsub":
                            val = makeBranchOpcode(ramAddr, destRamAddr, false);
                            break;
                        case "repl":
                            val = makeBranchOpcode(ramAddr, destRamAddr, true);
                            break;
                        case "hook":
                            //Jump to the hook addr
                            thisHookAddr = hookAddr;
                            val = makeBranchOpcode(ramAddr, hookAddr, false);

                            uint originalOpcode = handler.readFromRamAddr(ramAddr, ovId);
                            
                            //TODO: Parse and fix original opcode in case of BL instructions
                            //so it's possible to hook over them too.
                            extradata.writeUInt(originalOpcode);
                            hookAddr += 4;
                            extradata.writeUInt(0xE92D5FFF); //push {r0-r12, r14}
                            hookAddr += 4;
                            extradata.writeUInt(makeBranchOpcode(hookAddr, destRamAddr, true));
                            hookAddr += 4;
                            extradata.writeUInt(0xE8BD5FFF); //pop {r0-r12, r14}
                            hookAddr += 4;
                            extradata.writeUInt(makeBranchOpcode(hookAddr, ramAddr+4, false));
                            hookAddr += 4;
                            extradata.writeUInt(0x12345678);
                            hookAddr += 4;
                            break;
                        default:
                            continue;
                    }

                    //Console.Out.WriteLine(String.Format("{0:X8}:{1:X8} = {2:X8}", patchCategory, ramAddr, val));
                    Console.Out.WriteLine(String.Format("              {0:X8} {1:X8}", destRamAddr, thisHookAddr));

                    handler.writeToRamAddr(ramAddr, val, ovId);
                }
            }

            s.Close();

            int newArenaOffs = codeAddr + extradata.getPos();
            handler.writeToRamAddr(ArenaLoOffs, (uint)newArenaOffs, -1);

            handler.sections.Add(new Arm9BinSection(extradata.getArray(), codeAddr, 0));
            handler.saveSections();
        }
        
        private static void AlignStream(Stream stream, int modulus)
        {
            byte[] zero = { 0x00 };
            while (stream.Position % modulus != 0)
                stream.Write(zero, 0, 1);
        }
        
        public void MakeOverlay(uint overlayId)
        {
            FileInfo f = new FileInfo(targetDir.FullName + "/newcode.bin");
            if (!f.Exists) return;
            FileStream fs = f.OpenRead();
            FileInfo symFile = new FileInfo(targetDir.FullName + "/newcode.sym");
            StreamReader symStr = symFile.OpenText();

            byte[] newdata = new byte[fs.Length];
            fs.Read(newdata, 0, (int)fs.Length);
            fs.Close();


            BinaryWriter newOvl = new BinaryWriter(new MemoryStream());
            BinaryReader newOvlR = new BinaryReader(newOvl.BaseStream);

            try
            {
                newOvl.Write(newdata);
                AlignStream(newOvl.BaseStream, 4);

                uint staticInitCount = 0;

                while (!symStr.EndOfStream)
                {
                    string line = symStr.ReadLine();

                    if (line.Contains("_Z4initv")) //gcc name mangling of init()
                    {
                        uint addr = (uint)parseHex(line.Substring(0, 8));
                        newOvl.Write(addr);
                        ++staticInitCount;
                    }
                }

                /*if (newOvl.BaseStream.Length > 0x4d20)
                    throw new InvalidDataException
                        ("The overlay must have no more than 19776 bytes; this one will have " + newOvl.BaseStream.Length);*/
                
                /*
                 TODO: replace by OverlaysManager
                Program.romEditor.GetManager<OverlaysManager>().Replace(
                        overlayId, 
                        newOvlR.ReadBytes((int)newOvl.BaseStream.Length)
                );
                */
                
                NitroOverlay ovl = new NitroOverlay(Program.m_ROM, overlayId);
                newOvl.BaseStream.Position = 0;
                ovl.SetInitializer(ovl.GetRAMAddr() + (uint)newOvl.BaseStream.Length - 4 * staticInitCount,
                    4 * staticInitCount);
                ovl.SetSize((uint)newOvl.BaseStream.Length);
                ovl.WriteBlock(0, newOvlR.ReadBytes((int)newOvl.BaseStream.Length));
                ovl.SaveChanges();
            }
            catch (Exception ex)
            {
                new ExceptionMessageBox("Error", ex).ShowDialog();
                return;
            }
            finally
            {
                symStr.Close();
                newOvl.Dispose();
                newOvlR.Close();
            }
        }

        private void loadArenaLoOffsFile(DirectoryInfo romdir)
        {
            FileInfo f = new FileInfo(romdir.FullName + "/arenaoffs.txt");
            StreamReader s = f.OpenText();
            string l = s.ReadLine();
            ArenaLoOffs = int.Parse(l, System.Globalization.NumberStyles.HexNumber);
            s.Close();
        }


        public static uint makeBranchOpcode(int srcAddr, int destAddr, bool withLink)
        {
            unchecked
            {
                uint res = (uint)0xEA000000;

                if (withLink)
                    res |= 0x01000000;

                int offs = (destAddr / 4) - (srcAddr / 4) - 2;
                offs &= 0x00FFFFFF;
                res |= (uint)offs;

                return res;
            }
        }


        public static uint parseUHex(string s)
        {
            return uint.Parse(s, System.Globalization.NumberStyles.HexNumber);
        }

        public static int parseHex(string s)
        {
            return int.Parse(s, System.Globalization.NumberStyles.HexNumber);
        }
    }
}
