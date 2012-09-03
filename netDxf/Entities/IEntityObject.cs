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
        /// lightweight polyline.
        /// </summary>
        LightWeightPolyline,

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
        /// lightweight polyline vertex.
        /// </summary>
        LightWeightPolylineVertex,

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
    /// Represents a generic entity.
    /// </summary>
    public interface IEntityObject
    {
        /// <summary>
        /// Gets the entity <see cref="EntityType">type</see>.
        /// </summary>
        EntityType Type { get; }
        
        /// <summary>
        /// Gets or sets the entity <see cref="AciColor">color</see>.
        /// </summary>
        AciColor Color { get; set; }

        /// <summary>
        /// Gets or sets the entity <see cref="Layer">layer</see>.
        /// </summary>
        Layer Layer { get; set; }

        /// <summary>
        /// Gets or sets the entity <see cref="LineType">line type</see>.
        /// </summary>
        LineType LineType { get; set; }

        /// <summary>
        /// Gets or sets the entity <see cref="XData">extended data</see>.
        /// </summary>
        Dictionary<ApplicationRegistry, XData> XData { get; set; }
    }
}