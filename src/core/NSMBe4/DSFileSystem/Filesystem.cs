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
using System.Collections.Generic;
using System.IO;

namespace SM64DSe.core.NSMBe4.DSFileSystem
{
    public abstract class Filesystem
    {
        public List<NSMBe4File> allFiles = new List<NSMBe4File>();
        public List<NSMBe4Directory> allDirs = new List<NSMBe4Directory>();
        protected Dictionary<int, NSMBe4File> filesById = new Dictionary<int, NSMBe4File>();
        protected Dictionary<int, NSMBe4Directory> dirsById = new Dictionary<int, NSMBe4Directory>();
        public NSMBe4Directory mainDir;

//        public FilesystemBrowser viewer;

        public NSMBe4File getFileById(int id)
        {
            if (!filesById.ContainsKey(id))
                return null;
            return filesById[id];
        }

        public NSMBe4File getFileByName(string name)
        {
            foreach (NSMBe4File f in allFiles)
                if (f.name == name)
                    return f;

            return null;
        }

        public NSMBe4Directory getDirByPath(string path)
        {
            string[] shit = path.Split(new char[] { '/' });
            NSMBe4Directory dir = mainDir;
            for (int i = 0; i < shit.Length; i++)
            {
                NSMBe4Directory newDir = null;
                foreach(NSMBe4Directory d in dir.childrenDirs)
                    if(d.name == shit[i])
                    {
                        newDir = d;
                        break;
                    }
                if(newDir == null) return null;

                dir = newDir;
            }
            return dir;
        }

        protected void addFile(NSMBe4File f)
        {
            allFiles.Add(f);
            if (filesById.ContainsKey(f.id))
                throw new Exception("Duplicate file ID");

            filesById.Add(f.id, f);
//            filesByName.Add(f.name, f);
        }


        protected void addDir(NSMBe4Directory d)
        {
            allDirs.Add(d);
            if(dirsById.ContainsKey(d.id))
                throw new Exception("Duplicate dir ID");
            dirsById.Add(d.id, d);
//            dirsByName.Add(d.name, d);
        }

        public int alignUp(int what, int align)
        {
            if (what % align != 0)
                what += align - what % align;
            return what;
        }
        public int alignDown(int what, int align)
        {
            what -= what % align;
            return what;
        }



        public virtual void fileMoved(NSMBe4File f)
        {
        }


        public uint readUInt(Stream s)
        {
            uint res = 0;
            for (int i = 0; i < 4; i++)
            {
                res |= (uint)s.ReadByte() << 8 * i;
            }
            return res;
        }

		//Saving and closing
		public virtual void save() {}
		public virtual void close() {}

        public virtual string getRomPath()
        {
            return "Wadafuq.";
        }
    }
}
