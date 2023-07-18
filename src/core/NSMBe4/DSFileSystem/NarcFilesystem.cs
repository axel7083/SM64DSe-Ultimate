﻿/*
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

using System.IO;

namespace SM64DSe.core.NSMBe4.DSFileSystem
{
    class NarcFilesystem : NitroFilesystem
    {

        public int fntOffset, fntSize;
        public int fatOffset, fatSize;

        public NarcFilesystem(NSMBe4File f)
            : base(new FileFilesystemSource(f, false))
        {
        }

        public NarcFilesystem(NSMBe4File f, bool compressed)
            : base(new FileFilesystemSource(f, compressed))
        {
        }

        public override void load()
        {

            //I have to do some tricky offset calculations here ...
            fatOffset= 0x1C;
            s.Seek(0x18, SeekOrigin.Begin); //number of files
            fatSize = (int)readUInt(s) * 8;

            s.Seek(fatSize + fatOffset + 4, SeekOrigin.Begin); //size of FNTB
            fntSize = (int)readUInt(s) - 8; //do not include header
            fntOffset = fatSize + fatOffset + 8;

            fileDataOffsetP = fntSize + fntOffset + 8;
            FntNsmBe4File = new PhysicalNsmBe4File(this, mainDir, -2, "fnt.bin", fntOffset, fntSize);
            FatNsmBe4File = new PhysicalNsmBe4File(this, mainDir, -3, "fat.bin", fatOffset, fatSize);

            base.load();
            loadNamelessFiles(mainDir);
        }

		//TODO: Find a better method of saving. Maybe on-demand (a button)?
        public override void fileMoved(NSMBe4File f)
        {
            source.save();
        }
    }
}
