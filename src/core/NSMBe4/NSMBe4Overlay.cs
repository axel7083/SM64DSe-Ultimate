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

using SM64DSe.core.NSMBe4.DSFileSystem;

namespace SM64DSe.core.NSMBe4
{
    public class NSMBe4Overlay
    {
		public NSMBe4File f;
        private NSMBe4File _ovTableNsmBe4File;
        private uint ovTableOffs;
        
        public uint ovId { get { return _ovTableNsmBe4File.getUintAt((int)ovTableOffs + 0x00); } }
        public uint ramAddr { get { return _ovTableNsmBe4File.getUintAt((int)ovTableOffs + 0x04); } }
        public uint ramSize { get { return _ovTableNsmBe4File.getUintAt((int)ovTableOffs + 0x08); } }
        public uint bssSize { get { return _ovTableNsmBe4File.getUintAt((int)ovTableOffs + 0x0C); } }
        public uint staticInitStart { get { return _ovTableNsmBe4File.getUintAt((int)ovTableOffs + 0x10); } }
        public uint staticInitEnd { get { return _ovTableNsmBe4File.getUintAt((int)ovTableOffs + 0x14); } }

        public bool isCompressed
        {
            get
            {
                byte b = _ovTableNsmBe4File.getByteAt((int)ovTableOffs + 0x1F);
                return (b & 0x1) != 0;
            }
            set
            {
                byte b = _ovTableNsmBe4File.getByteAt((int)ovTableOffs + 0x1F);
                b &= 0xFE; // clear bit
                if(value)
                    b |= 0x1;
                _ovTableNsmBe4File.setByteAt((int)ovTableOffs + 0x1F, b);
            }
        }

        public NSMBe4Overlay(NSMBe4File nsmBe4File, NSMBe4File ovTableNsmBe4File, uint ovTableOffs)
        {
        	this.f = nsmBe4File;
            this._ovTableNsmBe4File = ovTableNsmBe4File;
            this.ovTableOffs = ovTableOffs;
        }

        public byte[] getDecompressedContents()
        {
            byte[] data = f.getContents();
            if (isCompressed)
                data = NSMBe4ROM.DecompressOverlay(data);
            return data;
        }

        public void decompress()
        {
            if (isCompressed)
            {
                byte[] data = f.getContents();
                data = NSMBe4ROM.DecompressOverlay(data);
                f.beginEdit(this);
                f.replace(data, this);
                f.endEdit(this);
                isCompressed = false;
            }
        }

        public bool containsRamAddr(int addr)
        {
            return addr >= ramAddr && addr < ramAddr + ramSize;
        }

        public uint readFromRamAddr(int addr)
        {
            decompress();

            addr -= (int)ramAddr;
            return f.getUintAt(addr);
        }

        public void writeToRamAddr(int addr, uint val)
        {
            decompress();
            addr -= (int)ramAddr;
            f.setUintAt(addr, val);
        }
    }
}
