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
	//Seriouisy, wtf.
    public class BannerNsmBe4File : PhysicalNsmBe4File
    {
        public BannerNsmBe4File(Filesystem parent, NSMBe4Directory parentDir, NSMBe4File headerNsmBe4File)
            : base(parent, parentDir, -9, "banner.bin", headerNsmBe4File, 0x68, 0, true)
        {
            EndNsmBe4File = null;
            fileSizeP = 0x840;
            refreshOffsets();
        }

        //Hack to prevent stack overflow...
        private bool updatingCrc = false;

        public void updateCRC16()
        {
            updatingCrc = true;
            byte[] contents = getContents();
            byte[] checksumArea = new byte[0x820];
            Array.Copy(contents, 0x20, checksumArea, 0, 0x820);
            ushort checksum = NSMBe4ROM.CalcCRC16(checksumArea);
            setUshortAt(2, checksum);
            Console.Out.WriteLine("UPDATING BANNER CHECKSUM!!!!");
            updatingCrc = false;
        }

        public override void endEdition()
        {
            base.endEdition();
            if(!updatingCrc)
                updateCRC16();
        }
    }
}
