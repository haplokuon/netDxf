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

namespace netDxf.Collections
{
    internal class DxfObjectReferences
    {
        private readonly Dictionary<DxfObject, int> references;

        public DxfObjectReferences()
        {
            this.references = new Dictionary<DxfObject, int>();
        }

        public bool IsEmpty()
        {
            return this.references.Count == 0;
        }

        public void Add(DxfObject item)
        {
            if (this.references.ContainsKey(item))
            {
                this.references[item] += 1;
            }
            else
            {
                this.references.Add(item, 1);
            }
        }

        public void Add(IEnumerable<DxfObjectReference> refs)
        {
            foreach (DxfObjectReference newReference in refs)
            {
                if (this.references.ContainsKey(newReference.Reference))
                {
                    this.references[newReference.Reference] += newReference.Uses;
                }
                else
                {
                    this.references.Add(newReference.Reference, newReference.Uses);
                }
            }
        }

        public bool Remove(DxfObject item)
        {
            if (this.references.ContainsKey(item))
            {
                this.references[item] -= 1;

                if (this.references[item] == 0)
                {
                    this.references.Remove(item);
                }

                return true;
            }

            return false;
        }

        public List<DxfObjectReference> ToList()
        {
            List<DxfObjectReference> refs = new List<DxfObjectReference>();
            foreach (KeyValuePair<DxfObject, int> pair in this.references)
            {
                refs.Add(new DxfObjectReference(pair.Key, pair.Value));
            }

            return refs;
        }
    }
}
