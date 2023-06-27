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

namespace netDxf.Header
{
    /// <summary>
    /// Defines the shape of the point entities.
    /// </summary>
    public enum PointShape
    {
        /// <summary>
        /// A dot.
        /// </summary>
        Dot = 0,

        /// <summary>
        /// No shape.
        /// </summary>
        Empty = 1,

        /// <summary>
        /// Plus sign.
        /// </summary>
        Plus = 2,

        /// <summary>
        /// Cross sign.
        /// </summary>
        Cross = 3,

        /// <summary>
        /// A line going upwards.
        /// </summary>
        Line = 4,

        /// <summary>
        /// A circle and a dot.
        /// </summary>
        CircleDot = 32,

        /// <summary>
        /// Only a circle shape.
        /// </summary>
        CircleEmpty = 33,

        /// <summary>
        /// A circle and a plus sign.
        /// </summary>
        CirclePlus = 34,

        /// <summary>
        /// A circle and a cross sign.
        /// </summary>
        CircleCross = 35,

        /// <summary>
        /// A circle and a line.
        /// </summary>
        CircleLine = 36,

        /// <summary>
        /// A square and a dot.
        /// </summary>
        SquareDot = 64,

        /// <summary>
        /// Only a square shape.
        /// </summary>
        SquareEmpty = 65,

        /// <summary>
        /// A square and a plus sign.
        /// </summary>
        SquarePlus = 66,

        /// <summary>
        /// A square and a cross sign.
        /// </summary>
        SquareCross = 67,

        /// <summary>
        /// A square and a line.
        /// </summary>
        SquareLine = 68,

        /// <summary>
        /// A circle, a square, and a dot.
        /// </summary>
        CircleSquareDot = 96,

        /// <summary>
        /// A circle and a square.
        /// </summary>
        CircleSquareEmpty = 97,

        /// <summary>
        /// A circle, a square, and a plus sign.
        /// </summary>
        CircleSquarePlus = 98,

        /// <summary>
        /// A circle, a square, and a cross sign.
        /// </summary>
        CircleSquareCross = 99,

        /// <summary>
        /// A circle, a square, and a line.
        /// </summary>
        CircleSquareLine = 100
    }
}