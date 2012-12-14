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
        /// Line entity.
        /// </summary>
        Line,

        /// <summary>
        /// Polyline entity.
        /// </summary>
        Polyline,

        /// <summary>
        /// 3d polyline entity.
        /// </summary>
        Polyline3d,

        /// <summary>
        /// Lightweight polyline entity.
        /// </summary>
        LightWeightPolyline,

        /// <summary>
        /// Polyface mesh entity.
        /// </summary>
        PolyfaceMesh,

        /// <summary>
        /// Circle entity.
        /// </summary>
        Circle,

        /// <summary>
        /// Ellipse entity.
        /// </summary>
        Ellipse,

        /// <summary>
        /// Point entity.
        /// </summary>
        Point,

        /// <summary>
        /// Arc entity.
        /// </summary>
        Arc,

        /// <summary>
        /// Text string entity.
        /// </summary>
        Text,

        /// <summary>
        /// Multiline text string entity.
        /// </summary>
        MText,

        /// <summary>
        /// 3d face entity.
        /// </summary>
        Face3D,

        /// <summary>
        /// Solid .
        /// </summary>
        Solid,

        /// <summary>
        /// Block insertion entity.
        /// </summary>
        Insert,

        /// <summary>
        /// Hatch entity.
        /// </summary>
        Hatch,

        /// <summary>
        /// Attribute entity.
        /// </summary>
        Attribute,

        /// <summary>
        /// Attribute definition entity.
        /// </summary>
        AttributeDefinition,

        /// <summary>
        /// Lightweight polyline vertex entity.
        /// </summary>
        LightWeightPolylineVertex,

        /// <summary>
        /// Polyline vertex entity.
        /// </summary>
        PolylineVertex,

        /// <summary>
        /// Polyline 3d vertex entity.
        /// </summary>
        Polyline3dVertex,

        /// <summary>
        /// Polyface mesh vertex entity.
        /// </summary>
        PolyfaceMeshVertex,

        /// <summary>
        /// Polyface mesh face entity.
        /// </summary>
        PolyfaceMeshFace,

        /// <summary>
        /// Dimension entity.
        /// </summary>
        Dimension,

        /// <summary>
        /// A generic vertex entity.
        /// </summary>
        Vertex,

        /// <summary>
        /// A raster image entity.
        /// </summary>
        Image
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