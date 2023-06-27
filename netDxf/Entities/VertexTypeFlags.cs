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
    /// Defines the vertex type.
    /// </summary>
    [Flags]
    internal enum VertexTypeFlags
    {
        /// <summary>
        /// Default (2D polyline vertex).
        /// </summary>
        Polyline2DVertex = 0,

        /// <summary>
        /// Extra vertex created by curve-fitting.
        /// </summary>
        CurveFittingExtraVertex = 1,

        /// <summary>
        /// Curve-fit tangent defined for this vertex.
        /// A curve-fit tangent direction of 0 may be omitted from DXF output but is significant if this bit is set.
        /// </summary>
        CurveFitTangent = 2,

        /// <summary>
        /// Not used.
        /// </summary>
        NotUsed = 4,

        /// <summary>
        /// Spline vertex created by spline-fitting.
        /// </summary>
        SplineVertexFromSplineFitting = 8,

        /// <summary>
        /// Spline frame control point.
        /// </summary>
        SplineFrameControlPoint = 16,

        /// <summary>
        /// 3D polyline vertex.
        /// </summary>
        Polyline3DVertex = 32,

        /// <summary>
        /// 3D polygon mesh vertex.
        /// </summary>
        Polygon3DMeshVertex = 64,

        /// <summary>
        /// Polyface mesh vertex.
        /// </summary>
        PolyfaceMeshVertex = 128
    }
}