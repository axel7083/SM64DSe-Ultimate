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
    public class LzNsmBe4File : NsmBe4FileWithLock
    {
        private NSMBe4File _parentNsmBe4File;
        private CompressionType comp;

        public enum CompressionType : int
        {
            None,
            LZ,
            // Used for palette files, they might be compressed, might not
            MaybeLZ,
            LZWithHeader
        }
        
        public LzNsmBe4File(NSMBe4File parentNsmBe4, CompressionType ct)
        {
        	nameP = parentNsmBe4.name;
            _parentNsmBe4File = parentNsmBe4;
			comp = ct;

            // If we think it might be compressed, try decompressing it. If it succeeds, assume it was compressed.
            if (comp == CompressionType.MaybeLZ) {
                try {
                    NSMBe4ROM.LZ77_Decompress(_parentNsmBe4File.getContents());
                    comp = CompressionType.LZ;
                } catch {
                    comp = CompressionType.None;
                }
            }

            if (comp == CompressionType.None)
        		fileSizeP = parentNsmBe4.fileSize;
            else if (comp == CompressionType.LZ)
            	fileSizeP = NSMBe4ROM.LZ77_GetDecompressedSize(parentNsmBe4.getInterval(0, 4));
            else if (comp == CompressionType.LZWithHeader)
            	fileSizeP = NSMBe4ROM.LZ77_GetDecompressedSizeWithHeader(parentNsmBe4.getInterval(0, 8));
        }

        public override byte[] getContents()
        {
            if(comp == CompressionType.LZWithHeader)
                return NSMBe4ROM.LZ77_DecompressWithHeader(_parentNsmBe4File.getContents());
            else if(comp == CompressionType.LZ)
                return NSMBe4ROM.LZ77_Decompress(_parentNsmBe4File.getContents());
            else 
            	return _parentNsmBe4File.getContents();
        }

        public override void replace(byte[] newFile, object editor)
        {
            if (!isAGoodEditor(editor))
                throw new Exception("NOT CORRECT EDITOR " + name);

            if(comp == CompressionType.LZWithHeader)
                _parentNsmBe4File.replace(NSMBe4ROM.LZ77_Compress(newFile, true), this);
            else if(comp == CompressionType.LZ)
                _parentNsmBe4File.replace(NSMBe4ROM.LZ77_Compress(newFile, false), this);
            else 
            	_parentNsmBe4File.replace(newFile, this);
        }
        
		public override byte[] getInterval(int start, int end)
		{
            if (comp == CompressionType.None)
	            return _parentNsmBe4File.getInterval(start, end);
    
            byte[] data;
            if(comp == CompressionType.LZWithHeader)
                data = NSMBe4ROM.LZ77_DecompressWithHeader(_parentNsmBe4File.getContents());
            else
                data = NSMBe4ROM.LZ77_Decompress(_parentNsmBe4File.getContents());

            int len = end-start;
            byte[] thisdata = new byte[len];
            Array.Copy(data, start, thisdata, 0, len);
            return thisdata;
   		}
		
        public override void replaceInterval(byte[] newFile, int start)
		{
			validateInterval(start, start+newFile.Length);

            if (comp == CompressionType.None)
            	_parentNsmBe4File.replaceInterval(newFile, start);
			else
            {
                byte[] data;
                if (comp == CompressionType.LZWithHeader)
                    data = NSMBe4ROM.LZ77_DecompressWithHeader(_parentNsmBe4File.getContents());
                else
                    data = NSMBe4ROM.LZ77_Decompress(_parentNsmBe4File.getContents());
                Array.Copy(newFile, 0, data, start, newFile.Length);
                _parentNsmBe4File.replace(NSMBe4ROM.LZ77_Compress(data, comp == CompressionType.LZWithHeader), this);
            }
        }

        public override void startEdition() 
        {
        	_parentNsmBe4File.beginEdit(this);
        }
        public override void endEdition() 
        {
        	_parentNsmBe4File.endEdit(this);
        }
    }
}

