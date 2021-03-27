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

namespace netDxf.Entities
{
    /// <summary>
    /// Defines the geometric characteristic symbols for location, orientation, form, profile, and runout.
    /// </summary>
    public enum ToleranceGeometricSymbol
    {
        /// <summary>
        /// No geometric symbol.
        /// </summary>
        None,

        /// <summary>
        /// Position, type location.
        /// </summary>
        Position,

        /// <summary>
        /// Concentricity or coaxiality, type location.
        /// </summary>
        Concentricity,

        /// <summary>
        /// Symmetry, type location.
        /// </summary>
        Symmetry,

        /// <summary>
        /// Parallelism, type orientation.
        /// </summary>
        Parallelism,

        /// <summary>
        /// Perpendicularity, type orientation.
        /// </summary>
        Perpendicularity,

        /// <summary>
        /// Angularity, type orientation.
        /// </summary>
        Angularity,

        /// <summary>
        /// Cylindricity, type form.
        /// </summary>
        Cylindricity,

        /// <summary>
        /// Flatness, type form.
        /// </summary>
        Flatness,

        /// <summary>
        /// Circularity or roundness, type form.
        /// </summary>
        Roundness,

        /// <summary>
        /// Straightness, type form.
        /// </summary>
        Straightness,

        /// <summary>
        /// Profile of a surface, type profile.
        /// </summary>
        ProfileSurface,

        /// <summary>
        /// Profile of a line, type profile.
        /// </summary>
        ProfileLine,

        /// <summary>
        /// Circular runout, type runout.
        /// </summary>
        CircularRunout,

        /// <summary>
        /// Total runout, type runout.
        /// </summary>
        TotalRunOut
    }
}