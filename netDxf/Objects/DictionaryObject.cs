#region netDxf library licensed under the MIT License
// 
//                       netDxf library
// Copyright (c) Daniel Carvajal (haplokuon@gmail.com)
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
    internal class DictionaryObject :
        DxfObject
    {
        #region private fields

        private readonly Dictionary<string, string> entries;
        private bool isHardOwner;
        private DictionaryCloningFlags cloning;

        #endregion

        #region private fields

        public DictionaryObject(DxfObject owner)
            : base(DxfObjectCode.Dictionary)
        {
            this.isHardOwner = true;
            this.cloning = DictionaryCloningFlags.KeepExisting;
            this.entries = new Dictionary<string, string>();
            this.Owner = owner;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the entries dictionary (key: owner entry handle, value: name)
        /// </summary>
        public Dictionary<string, string> Entries
        {
            get { return this.entries; }
        }

        /// <summary>
        /// Gets or sets if the dictionary object is hard owner.
        /// </summary>
        public bool IsHardOwner
        {
            get { return this.isHardOwner; }
            set { this.isHardOwner = value; }
        }

        /// <summary>
        /// Gets or sets the dictionary object cloning flags.
        /// </summary>
        public DictionaryCloningFlags Cloning
        {
            get { return this.cloning; }
            set { this.cloning = value; }
        }

        #endregion
    }
}