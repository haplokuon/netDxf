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
    public class PolylineVertex :
        IVertex
    {
      
        #region private fields

        protected const string DXF_NAME = DxfEntityCode.Vertex;
        protected const EntityType TYPE = EntityType.PolylineVertex;
        protected VertexTypeFlags flags;
        protected Vector2 location;
        protected float beginThickness;
        protected float endThickness;
        protected float bulge;
        protected AciColor color;
        protected Layer layer;
        protected LineType lineType;
        protected readonly List<XData> xData;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the PolylineVertex class.
        /// </summary>
        public PolylineVertex()
        {
            this.flags = VertexTypeFlags.PolylineVertex;
            this.location = Vector2.Zero;
            this.layer = Layer.Default;
            this.color = AciColor.Bylayer;
            this.lineType = LineType.ByLayer;
            this.bulge = 0.0f;
            this.beginThickness = 0.0f;
            this.endThickness = 0.0f;
            this.xData = new List<XData>();
        }

        /// <summary>
        /// Initializes a new instance of the PolylineVertex class.
        /// </summary>
        /// <param name="location">Polyline vertex coordinates.</param>
        public PolylineVertex(Vector2 location)
        {
            this.flags = VertexTypeFlags.PolylineVertex;
            this.location = location;
            this.layer = Layer.Default;
            this.color = AciColor.Bylayer;
            this.lineType = LineType.ByLayer;
            this.bulge = 0.0f;
            this.beginThickness = 0.0f;
            this.endThickness = 0.0f;
            this.xData = new List<XData>();
        }

       /// <summary>
        /// Initializes a new instance of the PolylineVertex class.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        public PolylineVertex(float x, float y)
        {
            this.flags = VertexTypeFlags.PolylineVertex;
            this.location = new Vector2(x, y);
            this.layer = Layer.Default;
            this.color = AciColor.Bylayer;
            this.lineType = LineType.ByLayer;
            this.bulge = 0.0f;
            this.beginThickness = 0.0f;
            this.endThickness = 0.0f;
            this.xData = new List<XData>();
        }

        #endregion

        #region public properties

        public VertexTypeFlags Flags
        {
            get { return this.flags; }
        }

        /// <summary>
        /// Gets or sets the polyline vertex coordinates.
        /// </summary>
        public Vector2 Location
        {
            get { return this.location; }
            set { this.location = value; }
        }

        /// <summary>
        /// Gets or sets the light weight polyline begin thickness.
        /// </summary>
        public float BeginThickness
        {
            get { return this.beginThickness; }
            set { this.beginThickness = value; }
        }

        /// <summary>
        /// Gets or sets the light weight polyline end thickness.
        /// </summary>
        public float EndThickness
        {
            get { return this.endThickness; }
            set { this.endThickness = value; }
        }

        /// <summary>
        /// Gets or set the light weight polyline bulge.Accepted values range from 0 to 1.
        /// </summary>
        /// <remarks>
        /// The bulge is the tangent of one fourth the included angle for an arc segment, 
        /// made negative if the arc goes clockwise from the start point to the endpoint. 
        /// A bulge of 0 indicates a straight segment, and a bulge of 1 is a semicircle.
        /// </remarks>
        public float Bulge
        {
            get { return this.bulge; }
            set
            {
                if (this.bulge < 0.0 || this.bulge > 1.0f)
                {
                    throw new ArgumentOutOfRangeException("value", value, "The bulge must be a value between zero and one");
                }
                this.bulge = value;
            }
        }

        #endregion

        #region IEntityObject Members

        /// <summary>
        /// Gets the dxf code that represents the entity.
        /// </summary>
        public string DxfName
        {
            get { return DXF_NAME; }
        }

        /// <summary>
        /// Gets the entity type.
        /// </summary>
        public EntityType Type
        {
            get { return TYPE; }
        }

        /// <summary>
        /// Gets or sets the entity color.
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
        /// Gets or sets the entity layer.
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
        /// Gets or sets the entity line type.
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
        /// Gets or sets the entity extended data.
        /// </summary>
        public List<XData> XData
        {
            get { return this.xData; }
        }

        #endregion

        #region overrides

        public override string ToString()
        {
            return String.Format("{0} {1}", TYPE, this.location);
        }

        #endregion
    }
}