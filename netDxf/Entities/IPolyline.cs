#region netDxf, Copyright(C) 2009 Daniel Carvajal, Licensed under LGPL.

//                        netDxf library
// Copyright (C) 2009 Daniel Carvajal (haplokuon@gmail.com)
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 

#endregion

using System;

namespace netDxf.Entities
{
    /// <summary>
    /// Defines the polyline type.
    /// </summary>
    /// <remarks>Bit flag.</remarks>
    [Flags]
    public enum PolylineTypeFlags
    {
        OpenPolyline = 0,
        ClosedPolylineOrClosedPolygonMeshInM = 1,
        CurveFit = 2,
        SplineFit = 4,
        Polyline3D = 8,
        PolygonMesh = 16,
        ClosedPolygonMeshInN = 32,
        PolyfaceMesh = 64,
        ContinuousLineTypePatter = 128
    }

    /// <summary>
    /// Defines the curves and smooth surface type.
    /// </summary>
    public enum SmoothType
    {
        /// <summary>
        /// No smooth surface fitted
        /// </summary>
        NoSmooth=0,
        /// <summary>
        /// Quadratic B-spline surface
        /// </summary>
        Quadratic=5,
        /// <summary>
        /// Cubic B-spline surface
        /// </summary>
        Cubic=6,
        /// <summary>
        /// Bezier surface
        /// </summary>
        Bezier=8
    }
  
    /// <summary>
    /// Represents a generic polyline.
    /// </summary>
    public interface IPolyline :
        IEntityObject
    {
        /// <summary>
        /// Gets the polyline type.
        /// </summary>
        PolylineTypeFlags Flags { get; }
    }
}