#region netDxf, Copyright(C) 2014 Daniel Carvajal, Licensed under LGPL.

//                        netDxf library
// Copyright (C) 2014 Daniel Carvajal (haplokuon@gmail.com)
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
        /// 3d polyline entity.
        /// </summary>
        Polyline,

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
        /// Multiline entity.
        /// </summary>
        MLine,

        /// <summary>
        /// 3d face entity.
        /// </summary>
        Face3D,

        /// <summary>
        /// Solid.
        /// </summary>
        Solid,

        /// <summary>
        /// Spline (nonuniform rational B-splines NURBS).
        /// </summary>
        Spline,

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
        /// Dimension entity.
        /// </summary>
        Dimension,

        /// <summary>
        /// A raster image entity.
        /// </summary>
        Image,

        /// <summary>
        /// Ray entity.
        /// </summary>
        Ray,

        /// <summary>
        /// XLine entity.
        /// </summary>
        XLine,

        /// <summary>
        /// Vieport entity.
        /// </summary>
        Viewport
    }
}