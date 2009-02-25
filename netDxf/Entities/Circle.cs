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
    /// Represents a circle.
    /// </summary>
    public class Circle :
        IEntityObject
    {
        #region private fields

        private const string DXF_NAME = DxfEntityCode.Circle;
        private const EntityType TYPE = EntityType.Circle;
        private Vector3 center;
        private float radius;
        private float thickness;
        private Layer layer;
        private AciColor color;
        private LineType lineType;
        private Vector3 normal;
        private readonly List<XData> xData;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the circle class.
        /// </summary>
        public Circle(Vector3 center, float radius)
        {
            this.center = center;
            this.radius = radius;
            this.thickness = 0.0f;
            this.layer = Layer.Default;
            this.color = AciColor.Bylayer;
            this.lineType = LineType.ByLayer;
            this.normal = Vector3.UnitZ;
            this.xData = new List<XData>();
        }

        /// <summary>
        /// Initializes a new instance of the circle class.
        /// </summary>
        public Circle()
        {
            this.center = Vector3.Zero;
            this.radius = 1.0f;
            this.thickness = 0.0f;
            this.layer = Layer.Default;
            this.color = AciColor.Bylayer;
            this.lineType = LineType.ByLayer;
            this.normal = Vector3.UnitZ;
            this.xData = new List<XData>();
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the circle center.
        /// </summary>
        public Vector3 Center
        {
            get { return this.center; }
            set { this.center = value; }
        }

        /// <summary>
        /// Gets or set the circle radius.
        /// </summary>
        public float Radius
        {
            get { return this.radius; }
            set { this.radius = value; }
        }

        public float Thickness
        {
            get { return this.thickness; }
            set { this.thickness = value; }
        }

        public Vector3 Normal
        {
            get { return this.normal; }
            set
            {
                value.Normalize();
                this.normal = value;
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

        #region public methods

        /// <summary>
        /// Converts the circle in a list of vertexes.
        /// </summary>
        /// <param name="precision">Number of vertexes generated.</param>
        /// <returns>The vertexes are expresed in object coordinate system.</returns>
        public List<Vector2> PoligonalVertexes(byte precision)
        {
            if (precision < 3)
                throw new ArgumentOutOfRangeException("precision", precision, "The circle precision must be greater or equal to three");

            List<Vector2> ocsVertexes = new List<Vector2>();

            float angle = (float)(MathHelper.TwoPI / precision);

                for (int i = 0; i < precision; i++)
                {
                    float sine = (float)(this.radius * Math.Sin(MathHelper.HalfPI + angle * i));
                    float cosine = (float)(this.radius * Math.Cos(MathHelper.HalfPI + angle * i));
                    ocsVertexes.Add(new Vector2(cosine + this.center.X, sine + this.center.Y));
                }

            return ocsVertexes;
        }

        /// <summary>
        /// Converts the circle in a list of vertexes.
        /// </summary>
        /// <param name="precision">Number of vertexes generated.</param>
        /// <param name="weldThreshold">Tolerance to consider if two new generated vertexes are equal.</param>
        /// <returns>The vertexes are expresed in object coordinate system.</returns>
        public List<Vector2> PoligonalVertexes(byte precision, float weldThreshold)
        {
            if (precision < 3)
                throw new ArgumentOutOfRangeException("precision", precision, "The circle precision must be greater or equal to three");

            List<Vector2> ocsVertexes = new List<Vector2>();

            if (2*this.radius >= weldThreshold)
            {
                float angulo = (float)(MathHelper.TwoPI / precision);
                Vector2 prevPoint;
                Vector2 firstPoint;

                float sine = (float)(this.radius * Math.Sin(MathHelper.HalfPI * 0.5));
                float cosine = (float)(this.radius * Math.Cos(MathHelper.HalfPI * 0.5));
                firstPoint = new Vector2(cosine + this.center.X, sine + this.center.Y);
                ocsVertexes.Add(firstPoint);
                prevPoint = firstPoint;

                for (int i = 1; i < precision; i++)
                {
                    sine = (float) (this.radius*Math.Sin(MathHelper.HalfPI + angulo*i));
                    cosine = (float)(this.radius * Math.Cos(MathHelper.HalfPI + angulo * i));
                    Vector2 point = new Vector2(cosine + this.center.X, sine + this.center.Y);

                    if (!point.Equals(prevPoint, weldThreshold) && 
                        !point.Equals(firstPoint, weldThreshold))
                    {
                        ocsVertexes.Add(point);
                        prevPoint = point;
                    }
                }
            }

            return ocsVertexes;
        }

        #endregion

        #region overrides

        public override string ToString()
        {
            return TYPE.ToString();
        }

        #endregion
    }
}