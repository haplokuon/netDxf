#region netDxf library licensed under the MIT License, Copyright © 2009-2021 Daniel Carvajal (haplokuon@gmail.com)
// 
//                        netDxf library
// Copyright © 2021 Daniel Carvajal (haplokuon@gmail.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software
// and associated documentation files (the “Software”), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion

namespace netDxf.Tables
{
    /// <summary>
    /// Represents a simple linetype segment.
    /// </summary>
    public class LinetypeSimpleSegment :
        LinetypeSegment
    {
        #region private fields

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>LinetypeSimpleSegment</c> class.
        /// </summary>
        public LinetypeSimpleSegment() : this(0.0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>LinetypeSimpleSegment</c> class.
        /// </summary>
        /// <param name="length">Dash or space length of the segment.</param>
        public LinetypeSimpleSegment(double length) : base (LinetypeSegmentType.Simple, length)
        {
        }

        #endregion

        #region public properties

        #endregion

        #region overrides

        /// <summary>
        /// Creates a new <c>LinetypeSimpleSegment</c> that is a copy of the current instance.
        /// </summary>
        /// <returns>A new <c>LinetypeSimpleSegment</c> that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new LinetypeSimpleSegment(this.Length);
        }

        #endregion
    }
}