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
    public class Arc :
        IEntityObject 
    {
        #region private fields

        private const string DXF_NAME = DxfEntityCode.Arc;
        private const EntityType TYPE = EntityType.Arc;
        private Vector3 center;
        private float radius;
        private float startAngle;
        private float endAngle;
        private float thickness;
        private Vector3 normal;
        private AciColor color;
        private Layer layer;
        private LineType lineType;
        private readonly List<XData> xData;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the arc class.
        /// </summary>
        public Arc(Vector3 center, float radius, float startAngle, float endAngle)
        {
            this.center = center;
            this.radius = radius;
            this.startAngle = startAngle;
            this.endAngle = endAngle;
            this.thickness = 0.0f;
            this.layer = Layer.Default;
            this.color = AciColor.Bylayer;
            this.lineType = LineType.ByLayer;
            this.normal = Vector3.UnitZ;
            this.xData = new List<XData>();
        }

        /// <summary>
        /// Initializes a new instance of the arc class.
        /// </summary>
        public Arc()
        {
            this.center = Vector3.Zero;
            this.radius = 0.0f;
            this.startAngle = 0.0f;
            this.endAngle = 0.0f;
            this.thickness = 0.0f;
            this.layer = Layer.Default;
            this.color = AciColor.Bylayer;
            this.lineType = LineType.ByLayer;
            this.normal = Vector3.UnitZ;
            this.xData = new List<XData>();
        }

        #endregion

        #region public properties

        public Vector3 Center
        {
            get { return this.center; }
            set { this.center = value; }
        }

        public float Radius
        {
            get { return this.radius; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("value", value.ToString());
                this.radius = value;
            }
        }

        public float StartAngle
        {
            get { return this.startAngle; }
            set { this.startAngle = value; }
        }

        public float EndAngle
        {
            get { return this.endAngle; }
            set { this.endAngle = value; }
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
                if (Vector3.Zero == value)
                    throw new ArgumentNullException("value", "The normal can not be the zero vector");
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
        /// Converts the arc in a list of vertexes.
        /// </summary>
        /// <param name="precision">Number of vertexes generated.</param>
        /// <returns>The vertexes are expresed in object coordinate system.</returns>
        public List<Vector2> PoligonalVertexes(byte precision)
        {
            if (precision < 2)
                throw new ArgumentOutOfRangeException("precision", precision, "The arc precision must be greater or equal to two");

            List<Vector2> ocsVertexes = new List<Vector2>();
            float start = (float) (this.startAngle*MathHelper.DegToRad);
            float end = (float) (this.endAngle*MathHelper.DegToRad);
            float angle = (end - start) / precision;

            for (int i = 0; i <= precision; i++)
            {
                float sine = (float)(this.radius * Math.Sin(start + angle * i));
                float cosine = (float)(this.radius * Math.Cos(start + angle * i));
                ocsVertexes.Add(new Vector2(cosine + this.center.X, sine + this.center.Y));
            }

            return ocsVertexes;
        }

        /// <summary>
        /// Converts the arc in a list of vertexes.
        /// </summary>
        /// <param name="precision">Number of vertexes generated.</param>
        /// <param name="weldThreshold">Tolerance to consider if two new generated vertexes are equal.</param>
        /// <returns>The vertexes are expresed in object coordinate system.</returns>
        public List<Vector2> PoligonalVertexes(byte precision, float weldThreshold)
        {
            if (precision < 2)
                throw new ArgumentOutOfRangeException("precision", precision, "The arc precision must be greater or equal to two");

            List<Vector2> ocsVertexes = new List<Vector2>();
            float start = (float)(this.startAngle * MathHelper.DegToRad);
            float end = (float)(this.endAngle * MathHelper.DegToRad);

            if (2*this.radius >= weldThreshold)
            {
                float angulo = (end - start)/precision;
                Vector2 prevPoint;
                Vector2 firstPoint;

                float sine = (float) (this.radius*Math.Sin(start));
                float cosine = (float) (this.radius*Math.Cos(start));
                firstPoint = new Vector2(cosine + this.center.X, sine + this.center.Y);
                ocsVertexes.Add(firstPoint);
                prevPoint = firstPoint;

                for (int i = 1; i <= precision; i++)
                {
                    sine = (float) (this.radius*Math.Sin(start + angulo*i));
                    cosine = (float) (this.radius*Math.Cos(start + angulo*i));
                    Vector2 point = new Vector2(cosine + this.center.X, sine + this.center.Y);

                    if (!point.Equals(prevPoint, weldThreshold) && !point.Equals(firstPoint, weldThreshold))
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