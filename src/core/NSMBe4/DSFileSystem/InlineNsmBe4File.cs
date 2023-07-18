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
    public class InlineNsmBe4File : NsmBe4FileWithLock
    {
        private int inlineOffs;
        private int inlineLen;
        private NSMBe4File _parentNsmBe4File;
        public InlineNsmBe4File(NSMBe4File parentNsmBe4, int offs, int len, string name)
        {
        	nameP = name;
        	fileSizeP = len;
            _parentNsmBe4File = parentNsmBe4;
            inlineOffs = offs;
            inlineLen = len;
        }

        public override byte[] getContents()
        {
        	return _parentNsmBe4File.getInterval(inlineOffs, inlineOffs+inlineLen);
        }

        public override void replace(byte[] newFile, object editor)
        {
            if (!isAGoodEditor(editor))
                throw new Exception("NOT CORRECT EDITOR " + name);
            if(newFile.Length != inlineLen)
            	throw new Exception("Trying to resize an InlineFile: "+name);
            
            _parentNsmBe4File.replaceInterval(newFile, inlineOffs);
        }
        
		public override byte[] getInterval(int start, int end)
		{
			validateInterval(start, end);
			return _parentNsmBe4File.getInterval(inlineOffs+start, inlineOffs+end);
		}
		
        public override void replaceInterval(byte[] newFile, int start)
		{
			validateInterval(start, start+newFile.Length);
			_parentNsmBe4File.replaceInterval(newFile, inlineOffs+start);
		}

        public override void startEdition() 
        {
        	_parentNsmBe4File.beginEditInterval(inlineOffs, inlineOffs+inlineLen);
        }

        public override void endEdition() 
        {
        	_parentNsmBe4File.endEditInterval(inlineOffs, inlineOffs+inlineLen);
        }

    }
}
