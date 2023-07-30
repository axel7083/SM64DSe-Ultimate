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

using System.Collections.Generic;

namespace SM64DSe.core.NSMBe4.DSFileSystem
{
    public class NSMBe4Directory
    {
        private bool isSystemFolderP;
        public bool isSystemFolder { get { return isSystemFolderP; } }

        private string nameP;
        public string name { get { return nameP; } }

        private int idP;
        public int id { get { return idP; } }

        private NSMBe4Directory parentDirP;
        public NSMBe4Directory parentDir { get { return parentDirP; } }

        public List<NSMBe4File> childrenFiles = new List<NSMBe4File>();
        public List<NSMBe4Directory> childrenDirs = new List<NSMBe4Directory>();

        private Filesystem parent;

        public NSMBe4Directory(Filesystem parent, NSMBe4Directory parentDir, bool system, string name, int id)
        {
            this.parent = parent;
            this.parentDirP = parentDir;
            this.isSystemFolderP = system;
            this.nameP = name;
            this.idP = id;
        }

        public string getPath()
        {
            if (parentDir == null)
                return "FS";
            else
                return parentDir.getPath() + "/" + name;
        }
    }
}
