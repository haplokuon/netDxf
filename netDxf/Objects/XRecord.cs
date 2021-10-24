#region netDxf library licensed under the MIT License
// 
//                       netDxf library
// Copyright (c) 2019-2021 Daniel Carvajal (haplokuon@gmail.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// 
#endregion

using System.Collections.Generic;

namespace netDxf.Objects
{
    internal sealed class XRecord
    {
        #region private fields

        private DictionaryCloningFlags flags;
        private string handle;
        private string ownerHandle;
        private readonly string codename;
        private readonly List<XRecordEntry> entries;

        #endregion

        #region constructor

        public XRecord()
        {
            this.codename = DxfObjectCode.XRecord;
            this.handle = string.Empty;
            this.ownerHandle = string.Empty;
            this.flags = DictionaryCloningFlags.KeepExisting;
            this.entries = new List<XRecordEntry>();
        }

        #endregion

        #region public properties

        public string Handle
        {
            get { return this.handle; }
            set { this.handle = value; }
        }

        public string OwnerHandle
        {
            get { return this.ownerHandle; }
            set { this.ownerHandle = value; }
        }

        public string Codename
        {
            get { return this.codename; }
        }

        public DictionaryCloningFlags Flags
        {
            get { return this.flags; }
            set { this.flags = value; }
        }

        public List<XRecordEntry> Entries
        {
            get { return this.entries; }
        }

        #endregion
    }
}
