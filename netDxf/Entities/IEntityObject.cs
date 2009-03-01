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

using System.Collections.Generic;
using netDxf.Tables;

namespace netDxf.Entities
{
    /// <summary>
    /// Defines the entity type.
    /// </summary>
    public enum EntityType
    {
        /// <summary>
        /// line.
        /// </summary>
        Line,

        /// <summary>
        /// polyline.
        /// </summary>
        Polyline,

        /// <summary>
        /// 3d polyline .
        /// </summary>
        Polyline3d,

        /// <summary>
        /// light weight polyline.
        /// </summary>
        LwPolyline,

        /// <summary>
        /// polyface mesh.
        /// </summary>
        PolyfaceMesh,

        /// <summary>
        /// circle.
        /// </summary>
        Circle,

        /// <summary>
        /// nurbs curve
        /// </summary>
        NurbsCurve,

        /// <summary>
        /// ellipse.
        /// </summary>
        Ellipse,

        /// <summary>
        /// point.
        /// </summary>
        Point,

        /// <summary>
        /// arc.
        /// </summary>
        Arc,

        /// <summary>
        /// text string.
        /// </summary>
        Text,

        /// <summary>
        /// 3d face.
        /// </summary>
        Face3D,

        /// <summary>
        /// solid.
        /// </summary>
        Solid,

        /// <summary>
        /// block insertion.
        /// </summary>
        Insert,

        /// <summary>
        /// hatch.
        /// </summary>
        Hatch,

        /// <summary>
        /// attribute.
        /// </summary>
        Attribute,

        /// <summary>
        /// attribute definition.
        /// </summary>
        AttributeDefinition,

        /// <summary>
        /// polyline vertex.
        /// </summary>
        PolylineVertex,

        /// <summary>
        /// polyline 3d vertex.
        /// </summary>
        Polyline3dVertex,

        /// <summary>
        /// polyface mesh vertex.
        /// </summary>
        PolyfaceMeshVertex,

        /// <summary>
        /// polyface mesh face.
        /// </summary>
        PolyfaceMeshFace,

        /// <summary>
        /// dim.
        /// </summary>
        Dimension,

        /// <summary>
        /// A generi Vertex
        /// </summary>
        Vertex
    }

    /// <summary>
    /// Dxf entities codes.
    /// </summary>
    public sealed class DxfEntityCode
    {
        /// <summary>
        /// line.
        /// </summary>
        public const string Line = "LINE";

        /// <summary>
        /// ellipse.
        /// </summary>
        public const string Ellipse = "ELLIPSE";

        /// <summary>
        /// polyline.
        /// </summary>
        public const string Polyline = "POLYLINE";

        /// <summary>
        /// light weight polyline.
        /// </summary>
        public const string LwPolyline = "LWPOLYLINE";

        /// <summary>
        /// circle.
        /// </summary>
        public const string Circle = "CIRCLE";

        /// <summary>
        /// point.
        /// </summary>
        public const string Point = "POINT";

        /// <summary>
        /// arc.
        /// </summary>
        public const string Arc = "ARC";

        /// <summary>
        /// solid.
        /// </summary>
        public const string Solid = "SOLID";

        /// <summary>
        /// text string.
        /// </summary>
        public const string Text = "TEXT";

        /// <summary>
        /// 3d face.
        /// </summary>
        public const string Face3D = "3DFACE";

        /// <summary>
        /// block insertion.
        /// </summary>
        public const string Insert = "INSERT";

        /// <summary>
        /// hatch.
        /// </summary>
        public const string Hatch = "HATCH";

        /// <summary>
        /// attribute definition.
        /// </summary>
        public const string AttributeDefinition = "ATTDEF";

        /// <summary>
        /// attribute.
        /// </summary>
        public const string Attribute = "ATTRIB";

        /// <summary>
        /// vertex.
        /// </summary>
        public const string Vertex = "VERTEX";

        /// <summary>
        /// dim.
        /// </summary>
        public const string Dimension = "DIMENSION";
    }

    /// <summary>
    /// Represents a generic entity.
    /// </summary>
    public interface IEntityObject
    {
        /// <summary>
        /// Gets the entity <see cref="EntityType">type</see>.
        /// </summary>
        EntityType Type { get; }

        /// <summary>
        /// Gets the dxf code that represents the entity.
        /// </summary>
        string DxfName { get; }

        /// <summary>
        /// Gets or sets the entity <see cref="AciColor">color</see>.
        /// </summary>
        AciColor Color { get; set; }

        /// <summary>
        /// Gets or sets the entity <see cref="Layer">layer</see>.
        /// </summary>
        Layer Layer { get; set; }

        /// <summary>
        /// Gets or sets the entity <see cref="LineType">line type</see.
        /// </summary>
        LineType LineType { get; set; }

        /// <summary>
        /// Gets or sets the entity <see cref="XData">extended data</see.
        /// </summary>
        List<XData> XData { get; }
    }
}