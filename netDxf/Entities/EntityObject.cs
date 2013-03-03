#region netDxf, Copyright(C) 2013 Daniel Carvajal, Licensed under LGPL.

//                        netDxf library
// Copyright (C) 2013 Daniel Carvajal (haplokuon@gmail.com)
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
        Image
    }
    
    /// <summary>
    /// Represents a generic entity.
    /// </summary>
    public abstract class EntityObject
        : DxfObject
    {
        #region private fields

        private readonly EntityType type;
        private AciColor color;
        private Layer layer;
        private LineType lineType;
        private Lineweight lineweight;
        private double lineTypeScale;
        private bool isVisible;
        private Dictionary<string, XData> xData;

        #endregion

        #region constructors

        protected EntityObject(EntityType type, string dxfCode)
            : base(dxfCode)
        {
            this.type = type;
            this.color = AciColor.ByLayer;
            this.layer = Layer.Default;
            this.lineType = LineType.ByLayer;
            this.lineweight = Lineweight.ByLayer;
            this.lineTypeScale = 1.0;
            this.isVisible = true;
            this.xData = new Dictionary<string, XData>();
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the entity <see cref="EntityType">type</see>.
        /// </summary>
        public EntityType Type
        {
            get { return this.type; }
        }

        /// <summary>
        /// Gets or sets the entity <see cref="AciColor">color</see>.
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
        /// Gets or sets the entity <see cref="Layer">layer</see>.
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
        /// Gets or sets the entity <see cref="LineType">line type</see>.
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
        /// Gets or sets the entity line weight, one unit is always 1/100 mm (default = ByLayer).
        /// </summary>
        public Lineweight Lineweight
        {
            get { return this.lineweight; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                this.lineweight = value;
            }
        }

        /// <summary>
        /// Gets or sets the entity linetype scale.
        /// </summary>
        public double LineTypeScale
        {
            get { return lineTypeScale; }
            set
            {
                if (value <= 0 )
                    throw new ArgumentOutOfRangeException("value", value, "The linetype scale must be greater than zero.");
                lineTypeScale = value;
            }
        }

        /// <summary>
        /// Gets or set the entity visibility.
        /// </summary>
        public bool IsVisible
        {
            get { return isVisible; }
            set { isVisible = value; }
        }

        /// <summary>
        /// Gets or sets the entity <see cref="XData">extende data</see> (key: ApplicationRegistry.Name, value: XData).
        /// </summary>
        public Dictionary<string, XData> XData
        {
            get { return this.xData; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                this.xData = value;
            }
        }

        #endregion

        #region overrides

        /// <summary>
        /// Converts the value of this instance to its equivalent string representation.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            return this.type.ToString();
        }

        #endregion

    }
}