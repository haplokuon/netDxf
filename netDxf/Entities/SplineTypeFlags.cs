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

namespace netDxf.Entities
{
    /// <summary>
    /// Defines the spline type.
    /// </summary>
    /// <remarks>Bit flag.</remarks>
    [Flags]
    internal enum SplineTypeFlags
    {
        /// <summary>
        /// Default (open spline).
        /// </summary>
        Open = 0,

        /// <summary>
        /// Closed spline.
        /// </summary>
        Closed = 1,

        /// <summary>
        /// Periodic spline.
        /// </summary>
        Periodic = 2,

        /// <summary>
        /// Rational spline.
        /// </summary>
        Rational = 4,

        /// <summary>
        /// Planar.
        /// </summary>
        Planar = 8,

        /// <summary>
        /// Linear (planar bit is also set).
        /// </summary>
        Linear = 16,

        // in AutoCAD 2012 the flags can be greater than 70 despite the information that shows the DXF documentation these values are just a guess.
        FitChord = 32,
        FitSqrtChord = 64,
        FitUniform = 128,
        FitCustom = 256,
        Unknown = 512,

        /// <summary>
        /// Used by splines created by fit points.
        /// </summary>
        FitPointCreationMethod = 1024,

        /// <summary>
        /// Used for closed periodic splines.
        /// </summary>
        ClosedPeriodicSpline = 2048
    }
}