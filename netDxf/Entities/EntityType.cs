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
    /// Defines the entity type.
    /// </summary>
    public enum EntityType
    {
        /// <summary>
        /// Arc entity.
        /// </summary>
        Arc,

        /// <summary>
        /// Circle entity.
        /// </summary>
        Circle,

        /// <summary>
        /// Dimension entity.
        /// </summary>
        Dimension,

        /// <summary>
        /// Ellipse entity.
        /// </summary>
        Ellipse,

        /// <summary>
        /// 3d face entity.
        /// </summary>
        Face3D,

        /// <summary>
        /// Hatch entity.
        /// </summary>
        Hatch,

        /// <summary>
        /// A raster image entity.
        /// </summary>
        Image,

        /// <summary>
        /// Block insertion entity.
        /// </summary>
        Insert,

        /// <summary>
        /// Leader entity.
        /// </summary>
        Leader,

        /// <summary>
        /// Line entity.
        /// </summary>
        Line,

        /// <summary>
        /// Mesh entity.
        /// </summary>
        Mesh,

        /// <summary>
        /// Multiline entity.
        /// </summary>
        MLine,

        /// <summary>
        /// Multiline text string entity.
        /// </summary>
        MText,

        /// <summary>
        /// Point entity.
        /// </summary>
        Point,

        /// <summary>
        /// Polyface mesh entity.
        /// </summary>
        PolyfaceMesh,

        /// <summary>
        /// Polygon mesh entity.
        /// </summary>
        PolygonMesh,

        /// <summary>
        /// Polyline2D entity.
        /// </summary>
        Polyline2D,

        /// <summary>
        /// Polyline3D entity.
        /// </summary>
        Polyline3D,

        /// <summary>
        /// Ray entity.
        /// </summary>
        Ray,

        /// <summary>
        /// Shape entity.
        /// </summary>
        Shape,

        /// <summary>
        /// Solid entity.
        /// </summary>
        Solid,

        /// <summary>
        /// Spline (nonuniform rational B-splines NURBS).
        /// </summary>
        Spline,

        /// <summary>
        /// Text string entity.
        /// </summary>
        Text,

        /// <summary>
        /// Tolerance entity.
        /// </summary>
        Tolerance,

        /// <summary>
        /// Trace entity.
        /// </summary>
        Trace,

        /// <summary>
        /// Underlay entity.
        /// </summary>
        Underlay,

        /// <summary>
        /// Viewport entity.
        /// </summary>
        Viewport,

        /// <summary>
        /// Wipeout entity.
        /// </summary>
        Wipeout,

        /// <summary>
        /// XLine entity.
        /// </summary>
        XLine
    }
}