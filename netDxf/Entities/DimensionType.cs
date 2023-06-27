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

namespace netDxf.Entities
{
    /// <summary>
    /// Dimension type.
    /// </summary>
    public enum DimensionType
    {
        /// <summary>
        /// Rotated, horizontal, or vertical.
        /// </summary>
        Linear = 0,

        /// <summary>
        /// Aligned.
        /// </summary>
        Aligned = 1,

        /// <summary>
        /// Angular 2 lines.
        /// </summary>
        Angular = 2,

        /// <summary>
        /// Diameter.
        /// </summary>
        Diameter = 3,

        /// <summary>
        /// Radius.
        /// </summary>
        Radius = 4,

        /// <summary>
        /// Angular 3 points.
        /// </summary>
        Angular3Point = 5,

        /// <summary>
        /// Ordinate.
        /// </summary>
        Ordinate = 6,

        /// <summary>
        /// Arc Length.
        /// </summary>
        ArcLength = 7
    }
}