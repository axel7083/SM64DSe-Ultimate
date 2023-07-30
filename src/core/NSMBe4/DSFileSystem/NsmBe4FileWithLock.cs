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

using System;
using System.Collections.Generic;

namespace SM64DSe.core.NSMBe4.DSFileSystem
{
    public abstract class NsmBe4FileWithLock : NSMBe4File
    {

		public NsmBe4FileWithLock() {}
		
		public NsmBe4FileWithLock(Filesystem parent, NSMBe4Directory parentDir, string name, int id)
			: base(parent, parentDir, name, id)
		{
			
		}

		// HANDLE EDITIONS

		// Invariants:
		// editedBy == null || editedIntervals.count == 0
		// No two intervals in editedIntervals have intersection
		
		protected Object editedBy;
		protected List<Interval> editedIntervals = new List<Interval>();
		
		protected struct Interval {
			public int start, end;
			public Interval(int start, int end)
			{
				this.start = start;
				this.end = end;
			}
		}
		
		////
		
				
        public override void beginEdit(Object editor)
        {
            if (editedBy != null || editedIntervals.Count != 0)
                throw new AlreadyEditingException(this);
 
            startEdition();
            editedBy = editor;
        }

        public override void endEdit(Object editor)
        {
            if (editor == null || editor != editedBy)
                throw new Exception("Not correct editor: " + name);

            endEdition();
            editedBy = null;
        }
        
        public override void beginEditInterval(int start, int end)
        {
        	validateInterval(start, end);
        	
            if (editedBy != null)
                throw new AlreadyEditingException(this);
			
        	foreach(Interval i in editedIntervals)
        		if(i.start < end && start < i.end)
	                throw new AlreadyEditingException(this);
            
            if (editedIntervals.Count == 0)
                startEdition();
            
            editedIntervals.Add(new Interval(start, end));
        }
        
        public override void endEditInterval(int start, int end)
        {
        	validateInterval(start, end);

        	if(!editedIntervals.Remove(new Interval(start, end)))
                throw new Exception("Not correct interval: " + name);

			if(editedIntervals.Count == 0)
				endEdition();
        }

        public override bool beingEditedBy(Object ed)
        {
            return ed == editedBy;
        }
        
        protected bool isAGoodEditor(Object editor)
        {
        	return editor == editedBy;
        }
        
        public virtual void startEdition() {}
        public virtual void endEdition() {}
    }
}
