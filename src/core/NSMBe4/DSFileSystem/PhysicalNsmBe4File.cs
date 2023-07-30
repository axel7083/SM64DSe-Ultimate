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

namespace SM64DSe.core.NSMBe4.DSFileSystem
{
    public class PhysicalNsmBe4File : NsmBe4FileWithLock, IComparable
    {
        public bool isSystemFile;

		//File that specifies where the file begins.
        protected NSMBe4File BeginNsmBe4File;
        protected int beginOffset;
        
        //File that specifies where the file ends OR the file size.
        protected NSMBe4File EndNsmBe4File;
        protected int endOffset;
        protected bool endIsSize;
        
        //If true, file begin/size can't change at all.
        //TODO: Make sure these are set properly. I think they aren't.
        public bool canChangeOffset = true;
        public bool canChangeSize = true;

		//File begin offset
        public int fileBeginP;
        public int fileBegin { get { return fileBeginP; } }

        public int alignment = 4; // word align by default
        
        //For convenience
        public Stream filesystemStream { get { return ((PhysicalFilesystem)parent).s; } }
        public int filesystemDataOffset { get { return ((PhysicalFilesystem)parent).fileDataOffset; } }
        
        public PhysicalNsmBe4File(Filesystem parent, NSMBe4Directory parentDir, string name)
        	:base(parent, parentDir, name, -1)
        {
        }
    
        public PhysicalNsmBe4File(Filesystem parent, NSMBe4Directory parentDir, int id, string name, NSMBe4File alNsmBe4File, int alBeg, int alEnd)
        	:base(parent, parentDir, name, id)
        {
            this.BeginNsmBe4File = alNsmBe4File;
            this.EndNsmBe4File = alNsmBe4File;
            this.beginOffset = alBeg;
            this.endOffset = alEnd;
            refreshOffsets();
        }

        public PhysicalNsmBe4File(Filesystem parent, NSMBe4Directory parentDir, int id, string name, NSMBe4File alNsmBe4File, int alBeg, int alEnd, bool endsize)
        	:base(parent, parentDir, name, id)
        {
            this.BeginNsmBe4File = alNsmBe4File;
            this.EndNsmBe4File = alNsmBe4File;
            this.beginOffset = alBeg;
            this.endOffset = alEnd;
            this.endIsSize = endsize;
            refreshOffsets();
        }

        public PhysicalNsmBe4File(Filesystem parent, NSMBe4Directory parentDir, int id, string name, int alBeg, int alSize)
        	:base(parent, parentDir, name, id)
        {
            this.fileBeginP = alBeg;
            this.fileSizeP = alSize;
            this.canChangeOffset = false;
            this.canChangeSize = false;
            refreshOffsets();
        }

        public virtual void refreshOffsets()
        {
            if (BeginNsmBe4File != null)
                fileBeginP = (int)BeginNsmBe4File.getUintAt(beginOffset) + filesystemDataOffset;

            if (EndNsmBe4File != null)
            {
                int end = (int)EndNsmBe4File.getUintAt(endOffset);
                if (endIsSize)
                    fileSizeP = (int)end;
                else
                    fileSizeP = (int)end + filesystemDataOffset - fileBegin;
            }
        }

        public virtual void saveOffsets()
        {
            if (BeginNsmBe4File != null)
                BeginNsmBe4File.setUintAt(beginOffset, (uint)(fileBegin - filesystemDataOffset));

            if (EndNsmBe4File != null)
                if (endIsSize)
                    EndNsmBe4File.setUintAt(endOffset, (uint)fileSize);
                else
                    EndNsmBe4File.setUintAt(endOffset, (uint)(fileBegin + fileSize - filesystemDataOffset));
        }
	
		//Reading and writing!
		public override byte[] getInterval(int start, int end)
		{
			validateInterval(start, end);
			
			int len = end - start;
            byte[] file = new byte[len];
            filesystemStream.Seek(fileBegin+start, SeekOrigin.Begin);
            filesystemStream.Read(file, 0, len);
            return file;
		}

        public override byte[] getContents()
        {
            byte[] file = new byte[fileSize];
            filesystemStream.Seek(fileBegin, SeekOrigin.Begin);
            filesystemStream.Read(file, 0, file.Length);
            return file;
        }
		
        public override void replaceInterval(byte[] newFile, int start)
		{
			validateInterval(start, start+newFile.Length);
			if(!editedIntervals.Contains(new Interval(start, start+newFile.Length)) && editedBy == null)
	            throw new Exception("NOT CORRECT EDITOR " + name);
            filesystemStream.Seek(fileBegin+start, SeekOrigin.Begin);
            filesystemStream.Write(newFile, 0, newFile.Length);
		}
		
		//TODO: Clean up this mess.
        public override void replace(byte[] newFile, object editor)
        {
            if(!isAGoodEditor(editor))
                throw new Exception("NOT CORRECT EDITOR " + name);

            if(newFile.Length != fileSize && !canChangeSize)
                throw new Exception("TRYING TO RESIZE CONSTANT-SIZE FILE: " + name);

            int newStart = fileBegin;

            //if we insert a bigger file it might not fit in the current place
            if (newFile.Length > fileSize) 
            {
                if (canChangeOffset && !(parent is NarcFilesystem))
                {
                    newStart = ((PhysicalFilesystem)parent).findFreeSpace(newFile.Length, alignment);
                    if (newStart % alignment != 0)
                        newStart += alignment - newStart % alignment;
                }
                else
                {
                	//TODO: Keep the list always sorted in order to avoid stupid useless sorts.
                    parent.allFiles.Sort();
                    if (!(parent.allFiles.IndexOf(this) == parent.allFiles.Count - 1))
                    {
                        PhysicalNsmBe4File nextNsmBe4File = (PhysicalNsmBe4File) parent.allFiles[parent.allFiles.IndexOf(this) + 1];
                        ((PhysicalFilesystem)parent).moveAllFiles(nextNsmBe4File, fileBegin + newFile.Length);
                    }
                }
            }
            //This is for keeping NARC filesystems compact. Sucks.
            else if(parent is NarcFilesystem)
            {
                parent.allFiles.Sort();
                if (!(parent.allFiles.IndexOf(this) == parent.allFiles.Count - 1))
                {
                    PhysicalNsmBe4File nextNsmBe4File = (PhysicalNsmBe4File) parent.allFiles[parent.allFiles.IndexOf(this) + 1];
                    ((PhysicalFilesystem)parent).moveAllFiles(nextNsmBe4File, fileBegin + newFile.Length);
                }
            }
            
            //Stupid check.
            if (newStart % alignment != 0)
                Console.Out.Write("Warning: File is not being aligned: " + name + ", at " + newStart.ToString("X"));

            //write the file
            filesystemStream.Seek(newStart, SeekOrigin.Begin);
            filesystemStream.Write(newFile, 0, newFile.Length);
            
            //This should be handled in NarcFilesystem instead, in fileMoved (?)
            if(parent is NarcFilesystem)
            {
            	PhysicalNsmBe4File lastNsmBe4File = (PhysicalNsmBe4File) parent.allFiles[parent.allFiles.Count - 1];
                filesystemStream.SetLength(lastNsmBe4File.fileBegin + lastNsmBe4File.fileSize + 16);
			}
			
            //update ending pos
            fileBeginP = newStart;
            fileSizeP = newFile.Length;
            saveOffsets();

			//Updates total used rom size in header, and/or other stuff.
            parent.fileMoved(this); 
        }

        public void moveTo(int newOffs)
        {
            if (newOffs % alignment != 0)
                Console.Out.Write("Warning: File is not being aligned: " + name + ", at " + newOffs.ToString("X"));

            byte[] data = getContents();
            filesystemStream.Seek(newOffs, SeekOrigin.Begin);
            filesystemStream.Write(data, 0, data.Length);

            fileBeginP = newOffs;
            saveOffsets();
        }
		

		//Misc crap
		
        public int CompareTo(object obj)
        {
            PhysicalNsmBe4File f = (PhysicalNsmBe4File) obj;
            if (fileBegin == f.fileBegin)
                return fileSize.CompareTo(f.fileSize);
            return fileBegin.CompareTo(f.fileBegin);
        }

        public bool isAddrInFdile(int addr)
        {
            return addr >= fileBegin && addr < fileBegin + fileSize;
        }
    }
}
