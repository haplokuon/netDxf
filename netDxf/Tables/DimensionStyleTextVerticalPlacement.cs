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

namespace netDxf.Tables
{
    /// <summary>
    /// Controls the placement of dimension text.
    /// </summary>
    public enum DimensionStyleTextVerticalPlacement
    {
        /// <summary>
        /// Centers the dimension text between the two parts of the dimension line.
        /// </summary>
        Centered = 0,

        /// <summary>
        /// Places the dimension text above the dimension line.
        /// </summary>
        Above = 1,

        /// <summary>
        /// Places the dimension text on the side of the dimension line farthest away from the first defining point.
        /// </summary>
        Outside = 2,

        /// <summary>
        /// Places the dimension text to conform to a Japanese Industrial Standards (JIS) representation.
        /// </summary>
        JIS = 3,

        /// <summary>
        /// Places the dimension text under the dimension line.
        /// </summary>
        Below = 4
    }
}