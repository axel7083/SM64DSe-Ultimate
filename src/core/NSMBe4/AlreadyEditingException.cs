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
using SM64DSe.core.NSMBe4.DSFileSystem;

namespace SM64DSe.core.NSMBe4
{
    public class AlreadyEditingException : Exception
    {
        public NSMBe4File f;
        public AlreadyEditingException(NSMBe4File f)
        {
            this.f = f;
        }

        public override string Message
        {
            get
            {
                return "Already editing file: " + f.name;
            }
        }
    }
}
