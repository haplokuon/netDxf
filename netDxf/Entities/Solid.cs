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
    /// Represents a solid <see cref="IEntityObject">entity</see>.
    /// </summary>
    public class Solid :
        DxfObject,
        IEntityObject
    {
        #region private fields

        private const EntityType TYPE = EntityType.Solid;
        private Vector3d firstVertex;
        private Vector3d secondVertex;
        private Vector3d thirdVertex;
        private Vector3d fourthVertex;
        private double thickness;
        private Vector3d normal;
        private Layer layer;
        private AciColor color;
        private LineType lineType;
        private Dictionary<ApplicationRegistry, XData> xData;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>Solid</c> class.
        /// </summary>
        /// <param name="firstVertex">Solid <see cref="Vector3d">first vertex</see>.</param>
        /// <param name="secondVertex">Solid <see cref="Vector3d">second vertex</see>.</param>
        /// <param name="thirdVertex">Solid <see cref="Vector3d">third vertex</see>.</param>
        /// <param name="fourthVertex">Solid <see cref="Vector3d">fourth vertex</see>.</param>
        public Solid(Vector3d firstVertex, Vector3d secondVertex, Vector3d thirdVertex, Vector3d fourthVertex)
            : base(DxfObjectCode.Solid)
        {
            this.firstVertex = firstVertex;
            this.secondVertex = secondVertex;
            this.thirdVertex = thirdVertex;
            this.fourthVertex = fourthVertex;
            this.thickness = 0.0;
            this.normal = Vector3d.UnitZ;
            this.layer = Layer.Default;
            this.color = AciColor.ByLayer;
            this.lineType = LineType.ByLayer;
        }

        /// <summary>
        /// Initializes a new instance of the <c>Solid</c> class.
        /// </summary>
        public Solid()
            : base(DxfObjectCode.Solid)
        {
            this.firstVertex = Vector3d.Zero;
            this.secondVertex = Vector3d.Zero;
            this.thirdVertex = Vector3d.Zero;
            this.fourthVertex = Vector3d.Zero;
            this.thickness = 0.0f;
            this.normal = Vector3d.UnitZ;
            this.layer = Layer.Default;
            this.color = AciColor.ByLayer;
            this.lineType = LineType.ByLayer;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the first solid <see cref="netDxf.Vector3d">vertex</see>.
        /// </summary>
        public Vector3d FirstVertex
        {
            get { return this.firstVertex; }
            set { this.firstVertex = value; }
        }

        /// <summary>
        /// Gets or sets the second solid <see cref="netDxf.Vector3d">vertex</see>.
        /// </summary>
        public Vector3d SecondVertex
        {
            get { return this.secondVertex; }
            set { this.secondVertex = value; }
        }

        /// <summary>
        /// Gets or sets the third solid <see cref="netDxf.Vector3d">vertex</see>.
        /// </summary>
        public Vector3d ThirdVertex
        {
            get { return this.thirdVertex; }
            set { this.thirdVertex = value; }
        }

        /// <summary>
        /// Gets or sets the fourth solid <see cref="netDxf.Vector3d">vertex</see>.
        /// </summary>
        public Vector3d FourthVertex
        {
            get { return this.fourthVertex; }
            set { this.fourthVertex = value; }
        }

        /// <summary>
        /// Gets or sets the thickness of the solid.
        /// </summary>
        public double Thickness
        {
            get { return this.thickness; }
            set { this.thickness = value; }
        }

        /// <summary>
        /// Gets or sets the solid <see cref="netDxf.Vector3d">normal</see>.
        /// </summary>
        public Vector3d Normal
        {
            get { return this.normal; }
            set
            {
                if (Vector3d.Zero == value)
                    throw new ArgumentNullException("value", "The normal can not be the zero vector");
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