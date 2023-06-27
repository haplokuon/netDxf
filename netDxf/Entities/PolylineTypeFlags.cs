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
    /// Defines the polyline type.
    /// </summary>
    /// <remarks>Bit flag.</remarks>
    [Flags]
    internal enum PolylineTypeFlags
    {
        /// <summary>
        /// Default (open polyline).
        /// </summary>
        OpenPolyline = 0,

        /// <summary>
        /// This is a closed polyline (or a polygon mesh closed in the M direction).
        /// </summary>
        ClosedPolylineOrClosedPolygonMeshInM = 1,

        /// <summary>
        /// Curve-fit vertexes have been added.
        /// </summary>
        CurveFit = 2,

        /// <summary>
        /// Spline-fit vertexes have been added.
        /// </summary>
        SplineFit = 4,

        /// <summary>
        /// This is a 3D polyline.
        /// </summary>
        Polyline3D = 8,

        /// <summary>
        /// This is a 3D polygon mesh.
        /// </summary>
        PolygonMesh = 16,

        /// <summary>
        /// The polygon mesh is closed in the N direction.
        /// </summary>
        ClosedPolygonMeshInN = 32,

        /// <summary>
        /// The polyline is a polyface mesh.
        /// </summary>
        PolyfaceMesh = 64,

        /// <summary>
        /// The line type pattern is generated continuously around the vertexes of this polyline.
        /// </summary>
        ContinuousLinetypePattern = 128
    }
}