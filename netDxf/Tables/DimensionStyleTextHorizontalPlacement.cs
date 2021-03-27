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
    /// Controls the vertical placement of dimension text in relation to the dimension line.
    /// </summary>
    public enum DimensionStyleTextHorizontalPlacement
    {
        /// <summary>
        /// Centers the dimension text along the dimension line between the extension lines.
        /// </summary>
        Centered = 0,

        /// <summary>
        /// Left-justifies the text with the first extension line along the dimension line.
        /// </summary>
        AtExtLines1 = 1,

        /// <summary>
        /// Right-justifies the text with the second extension line along the dimension line.
        /// </summary>
        AtExtLine2 = 2,

        /// <summary>
        /// Positions the text over or along the first extension line.
        /// </summary>
        OverExtLine1 = 3,

        /// <summary>
        /// Positions the text over or along the second extension line.
        /// </summary>
        OverExtLine2 = 4
    }
}