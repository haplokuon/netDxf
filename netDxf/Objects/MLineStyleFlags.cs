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

using System;

namespace netDxf.Objects
{
    /// <summary>
    /// Flags (bit-coded).
    /// </summary>
    [Flags]
    public enum MLineStyleFlags
    {
        /// <summary>
        /// None.
        /// </summary>
        None = 0,

        /// <summary>
        /// Fill on.
        /// </summary>
        FillOn = 1,

        /// <summary>
        /// Display miters at the joints (inner vertexes).
        /// </summary>
        DisplayJoints = 2,

        /// <summary>
        /// Start square (line) cap.
        /// </summary>
        StartSquareCap = 16,

        /// <summary>
        /// Start inner arcs cap.
        /// </summary>
        StartInnerArcsCap = 32,

        /// <summary>
        /// Start round (outer arcs) cap.
        /// </summary>
        StartRoundCap = 64,

        /// <summary>
        /// End square (line) cap.
        /// </summary>
        EndSquareCap = 256,

        /// <summary>
        /// End inner arcs cap.
        /// </summary>
        EndInnerArcsCap = 512,

        /// <summary>
        /// End round (outer arcs) cap.
        /// </summary>
        EndRoundCap = 1024
    }
}