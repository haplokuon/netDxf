#region netDxf, Copyright(C) 2013 Daniel Carvajal, Licensed under LGPL.

//                        netDxf library
// Copyright (C) 2013 Daniel Carvajal (haplokuon@gmail.com)
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 

#endregion

using System.Collections.Generic;

namespace netDxf.Objects
{
    /// <summary>
    /// Duplicate record cloning flag (determines how to merge duplicate entries).
    /// </summary>
    internal enum ClonningFlag
    {
        NotApplicable = 0,
        KeepExisting = 1,
        UseClone = 2,
        XrefName = 3,
        Name = 4,
        UnmangleName = 5
    }

    internal class DictionaryObject :
        DxfObject
    {
        private readonly Dictionary<string, string> entries;
        private readonly string handleToOwner;
        private bool isHardOwner;
        private ClonningFlag clonning;

        public DictionaryObject(string handleToOwner)
            : base(DxfObjectCode.Dictionary)
        {
            this.handleToOwner = handleToOwner;
            this.isHardOwner = false;
            this.clonning = ClonningFlag.KeepExisting;
            this.entries = new Dictionary<string, string>();
        }

        
        public string HandleToOwner
        {
            get { return handleToOwner; }
        }

        /// <summary>
        /// Gets the entries dictionary (key: owner entry handle, value: name)
        /// </summary>
        public Dictionary<string, string> Entries
        {
            get { return entries; }
        }

        public bool IsHardOwner
        {
            get { return isHardOwner; }
            set { isHardOwner = value; }
        }

        public ClonningFlag Clonning
        {
            get { return clonning; }
            set { clonning = value; }
        }
    }
}

