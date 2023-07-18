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

namespace SM64DSe.core.NSMBe4.DSFileSystem
{
    public class NitroROMFilesystem : NitroFilesystem
    {
        public PhysicalNsmBe4File Arm7BinNsmBe4File, Arm7OvNsmBe4File, Arm9OvNsmBe4File, BannerNsmBe4File;
        public PhysicalNsmBe4File Arm9BinNsmBe4File;
        public PhysicalNsmBe4File RsaSigNsmBe4File;
        public HeaderNsmBe4File HeaderNsmBe4File;
        private string filename;

        public NitroROMFilesystem(String n)
            : base(new ExternalFilesystemSource(n))
        {
            filename = n;
        }

        public override string getRomPath()
        {
            return filename;
        }

        public override void load()
        {
            HeaderNsmBe4File = new HeaderNsmBe4File(this, mainDir);

            FntNsmBe4File = new PhysicalNsmBe4File(this, mainDir, -1, "fnt.bin", HeaderNsmBe4File, 0x40, 0x44, true);
            FatNsmBe4File = new PhysicalNsmBe4File(this, mainDir, -2, "fat.bin", HeaderNsmBe4File, 0x48, 0x4C, true);

            base.load();

            Arm9OvNsmBe4File = new PhysicalNsmBe4File(this, mainDir, -3, "arm9ovt.bin", HeaderNsmBe4File, 0x50, 0x54, true);
            Arm7OvNsmBe4File = new PhysicalNsmBe4File(this, mainDir, -4, "arm7ovt.bin", HeaderNsmBe4File, 0x58, 0x5C, true);
            //            arm9binFile = new Arm9BinFile(this, mainDir, headerFile);
            //            File arm9binFile2 = new PhysicalFile(this, mainDir, true, -2, "arm9.bin", headerFile, 0x20, 0xC, true);
            Arm9BinNsmBe4File = new PhysicalNsmBe4File(this, mainDir, -5, "arm9.bin", HeaderNsmBe4File, 0x20, 0x2C, true);
            Arm9BinNsmBe4File.alignment = 0x1000;
            Arm9BinNsmBe4File.canChangeOffset = false;
            Arm7BinNsmBe4File = new PhysicalNsmBe4File(this, mainDir, -6, "arm7.bin", HeaderNsmBe4File, 0x30, 0x3C, true);
            Arm7BinNsmBe4File.alignment = 0x200; //Not sure what should be used here...
            BannerNsmBe4File = new BannerNsmBe4File(this, mainDir, HeaderNsmBe4File);
            BannerNsmBe4File.alignment = 0x200; //Not sure what should be used here...

            uint rsaOffs = HeaderNsmBe4File.getUintAt(0x1000);

            if (rsaOffs == 0)
            {
                rsaOffs = HeaderNsmBe4File.getUintAt(0x80);
                HeaderNsmBe4File.setUintAt(0x1000, rsaOffs);
            }

            RsaSigNsmBe4File = new PhysicalNsmBe4File(this, mainDir, -7, "rsasig.bin", (int)rsaOffs, 136);
            RsaSigNsmBe4File.canChangeOffset = false;

            addFile(HeaderNsmBe4File);
            mainDir.childrenFiles.Add(HeaderNsmBe4File);
            addFile(Arm9OvNsmBe4File);
            mainDir.childrenFiles.Add(Arm9OvNsmBe4File);
            addFile(Arm7OvNsmBe4File);
            mainDir.childrenFiles.Add(Arm7OvNsmBe4File);
            addFile(Arm9BinNsmBe4File);
            mainDir.childrenFiles.Add(Arm9BinNsmBe4File);
            addFile(Arm7BinNsmBe4File);
            mainDir.childrenFiles.Add(Arm7BinNsmBe4File);
            addFile(BannerNsmBe4File);
            mainDir.childrenFiles.Add(BannerNsmBe4File);
            addFile(RsaSigNsmBe4File);
            mainDir.childrenFiles.Add(RsaSigNsmBe4File);

            loadOvTable("overlay7", -99, mainDir, Arm7OvNsmBe4File);
            loadOvTable("overlay9", -98, mainDir, Arm9OvNsmBe4File);
            loadNamelessFiles(mainDir);
        }

        private void loadOvTable(String dirName, int id, Directory parent, NSMBe4File table)
        {
            Directory dir = new Directory(this, parent, true, dirName, id);
            addDir(dir);
            parent.childrenDirs.Add(dir);

            NSMBe4ByteArrayInputStream tbl = new NSMBe4ByteArrayInputStream(table.getContents());

            int i = 0;
            while (tbl.lengthAvailable(32))
            {
                uint ovId = tbl.readUInt();
                uint ramAddr = tbl.readUInt();
                uint ramSize = tbl.readUInt();
                uint bssSize = tbl.readUInt();
                uint staticInitStart = tbl.readUInt();
                uint staticInitEnd = tbl.readUInt();
                ushort fileID = tbl.readUShort();
                tbl.skip(6); //unused 0's

                NSMBe4File f = loadFile(dirName+"_"+ovId+".bin", fileID, dir);
//                f.isSystemFile = true;

                i++;
            }
        }

        public override void fileMoved(NSMBe4File f)
        {
            if (!NSMBe4ROM.dlpMode)
            {
                uint end = (uint)getFilesystemEnd();
                HeaderNsmBe4File.setUintAt(0x80, end);
                HeaderNsmBe4File.UpdateCRC16();
            }
        }
    }
}
