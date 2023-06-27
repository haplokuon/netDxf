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

namespace netDxf
{
    /// <summary>
    /// Represent a reference to a TableObject.
    /// </summary>
    public class DxfObjectReference
    {
        private readonly DxfObject reference;
        private readonly int uses;

        /// <summary>
        /// Initializes a new instance of the <c>DxfObjectReference</c> class.
        /// </summary>
        /// <param name="reference">DxfObject reference.</param>
        /// <param name="uses">Number of times the specified reference uses the table object.</param>
        public DxfObjectReference(DxfObject reference, int uses)
        {
            this.reference = reference;
            this.uses = uses;
        }

        /// <summary>
        /// Gets the DxfObject that references the table object.
        /// </summary>
        public DxfObject Reference
        {
            get { return this.reference; }
        }

        /// <summary>
        /// Gets the number of times this reference uses the table object.
        /// </summary>
        public int Uses
        {
            get { return this.uses; }
        }
    }
}
