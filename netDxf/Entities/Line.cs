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
using System.Collections.Generic;
using netDxf.Tables;

namespace netDxf.Entities
{
    /// <summary>
    /// Represents a line <see cref="netDxf.Entities.IEntityObject">entity</see>.
    /// </summary>
    public class Line :
        DxfObject,
        IEntityObject
    {
        #region private fields

        private const EntityType TYPE = EntityType.Line;
        private Vector3d startPoint;
        private Vector3d endPoint;
        private double thickness;
        private AciColor color;
        private Layer layer;
        private LineType lineType;
        private Vector3d normal;
        private Dictionary<ApplicationRegistry, XData> xData;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>Line</c> class.
        /// </summary>
        /// <param name="startPoint">Line <see cref="Vector3d">start point.</see></param>
        /// <param name="endPoint">Line <see cref="Vector3d">end point.</see></param>
        public Line(Vector3d startPoint, Vector3d endPoint) 
            : base(DxfObjectCode.Line)
        {
            this.startPoint = startPoint;
            this.endPoint = endPoint;
            this.thickness = 0.0;
            this.layer = Layer.Default;
            this.color = AciColor.ByLayer;
            this.lineType = LineType.ByLayer;
            this.normal = Vector3d.UnitZ;
        }

        /// <summary>
        /// Initializes a new instance of the <c>Line</c> class.
        /// </summary>
        public Line()
            : base(DxfObjectCode.Line)
        {
            this.startPoint = Vector3d.Zero;
            this.endPoint = Vector3d.Zero;
            this.thickness = 0.0;
            this.layer = Layer.Default;
            this.color = AciColor.ByLayer;
            this.lineType = LineType.ByLayer;
            this.normal = Vector3d.UnitZ;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the line <see cref="netDxf.Vector3d">start point</see>.
        /// </summary>
        public Vector3d StartPoint
        {
            get { return this.startPoint; }
            set { this.startPoint = value; }
        }

        /// <summary>
        /// Gets or sets the line <see cref="netDxf.Vector3d">end point</see>.
        /// </summary>
        public Vector3d EndPoint
        {
            get { return this.endPoint; }
            set { this.endPoint = value; }
        }

        /// <summary>
        /// Gets or sets the line thickness.
        /// </summary>
        public double Thickness
        {
            get { return this.thickness ; }
            set { this.thickness = value; }
        }

        /// <summary>
        /// Gets or sets the line <see cref="netDxf.Vector3d">normal</see>.
        /// </summary>
        public Vector3d Normal
        {
            get { return this.normal; }
            set
            {
                if (Vector3d.Zero == value)
                    throw new ArgumentNullException("value","The normal can not be the zero vector");
                value.Normalize();
                this.normal = value;
            }
        }

        #endregion

        #region IEntityObject Members

       /// <summary>
        /// Gets the entity <see cref="netDxf.Entities.EntityType">type</see>.
        /// </summary>
        public EntityType Type
        {
            get { return TYPE; }
        }

        /// <summary>
        /// Gets or sets the entity <see cref="netDxf.AciColor">color</see>.
        /// </summary>
        public AciColor Color
        {
            get { return this.color; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                this.color = value;
            }
        }

        /// <summary>
        /// Gets or sets the entity <see cref="netDxf.Tables.Layer">layer</see>.
        /// </summary>
        public Layer Layer
        {
            get { return this.layer; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                this.layer = value;
            }
        }

        /// <summary>
        /// Gets or sets the entity <see cref="netDxf.Tables.LineType">line type</see>.
        /// </summary>
        public LineType LineType
        {
            get { return this.lineType; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                this.lineType = value;
            }
        }

        /// <summary>
        /// Gets or sets the entity <see cref="netDxf.XData">extende data</see>.
        /// </summary>
        public Dictionary<ApplicationRegistry, XData> XData
        {
            get { return this.xData; }
            set { this.xData = value; }
        }

        #endregion

        #region overrides

        /// <summary>
        /// Converts the value of this instance to its equivalent string representation.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            return TYPE.ToString();
        }

        #endregion

    }
}