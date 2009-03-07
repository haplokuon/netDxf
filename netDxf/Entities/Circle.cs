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
    /// Represents a circle <see cref="netDxf.Entities.IEntityObject">entity</see>.
    /// </summary>
    public class Circle :
        DxfObject,
        IEntityObject
    {
        #region private fields

        private const EntityType TYPE = EntityType.Circle;
        private Vector3f center;
        private float radius;
        private float thickness;
        private Layer layer;
        private AciColor color;
        private LineType lineType;
        private Vector3f normal;
        private Dictionary<ApplicationRegistry, XData> xData;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>Circle</c> class.
        /// </summary>
        /// <param name="center">Circle <see cref="Vector3f">center</see> in object coordinates.</param>
        /// <param name="radius">Circle radius.</param>
        /// <remarks>The center Z coordinate represents the elevation of the arc along the normal.</remarks>
        public Circle(Vector3f center, float radius) : base(DxfObjectCode.Circle)
        {
            this.center = center;
            this.radius = radius;
            this.thickness = 0.0f;
            this.layer = Layer.Default;
            this.color = AciColor.ByLayer;
            this.lineType = LineType.ByLayer;
            this.normal = Vector3f.UnitZ;
        }

        /// <summary>
        /// Initializes a new instance of the <c>Circle</c> class.
        /// </summary>
        public Circle() : base(DxfObjectCode.Circle)
        {
            this.center = Vector3f.Zero;
            this.radius = 1.0f;
            this.thickness = 0.0f;
            this.layer = Layer.Default;
            this.color = AciColor.ByLayer;
            this.lineType = LineType.ByLayer;
            this.normal = Vector3f.UnitZ;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the circle <see cref="netDxf.Vector3f">center</see>.
        /// </summary>
        /// <remarks>The center Z coordinate represents the elevation of the arc along the normal.</remarks>
        public Vector3f Center
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

        /// <summary>
        /// Gets or sets the arc thickness.
        /// </summary>
        public float Thickness
        {
            get { return this.thickness; }
            set { this.thickness = value; }
        }

        /// <summary>
        /// Gets or sets the circle <see cref="netDxf.Vector3f">normal</see>.
        /// </summary>
        public Vector3f Normal
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

        #region public methods

        /// <summary>
        /// Converts the circle in a list of vertexes.
        /// </summary>
        /// <param name="precision">Number of vertexes generated.</param>
        /// <returns>A list vertexes that represents the circle expresed in object coordinate system.</returns>
        public List<Vector2f> PoligonalVertexes(int precision)
        {
            if (precision < 3)
                throw new ArgumentOutOfRangeException("precision", precision, "The circle precision must be greater or equal to three");

            List<Vector2f> ocsVertexes = new List<Vector2f>();

            float angle = (float) (MathHelper.TwoPI/precision);

            for (int i = 0; i < precision; i++)
            {
                float sine = (float) (this.radius*Math.Sin(MathHelper.HalfPI + angle*i));
                float cosine = (float) (this.radius*Math.Cos(MathHelper.HalfPI + angle*i));
                ocsVertexes.Add(new Vector2f(cosine + this.center.X, sine + this.center.Y));
            }

            return ocsVertexes;
        }

        /// <summary>
        /// Converts the circle in a list of vertexes.
        /// </summary>
        /// <param name="precision">Number of vertexes generated.</param>
        /// <param name="weldThreshold">Tolerance to consider if two new generated vertexes are equal.</param>
        /// <returns>A list vertexes that represents the circle expresed in object coordinate system.</returns>
        public List<Vector2f> PoligonalVertexes(int precision, float weldThreshold)
        {
            if (precision < 3)
                throw new ArgumentOutOfRangeException("precision", precision, "The circle precision must be greater or equal to three");

            List<Vector2f> ocsVertexes = new List<Vector2f>();

            if (2*this.radius >= weldThreshold)
            {
                float angulo = (float) (MathHelper.TwoPI/precision);
                Vector2f prevPoint;
                Vector2f firstPoint;

                float sine = (float) (this.radius*Math.Sin(MathHelper.HalfPI*0.5));
                float cosine = (float) (this.radius*Math.Cos(MathHelper.HalfPI*0.5));
                firstPoint = new Vector2f(cosine + this.center.X, sine + this.center.Y);
                ocsVertexes.Add(firstPoint);
                prevPoint = firstPoint;

                for (int i = 1; i < precision; i++)
                {
                    sine = (float) (this.radius*Math.Sin(MathHelper.HalfPI + angulo*i));
                    cosine = (float) (this.radius*Math.Cos(MathHelper.HalfPI + angulo*i));
                    Vector2f point = new Vector2f(cosine + this.center.X, sine + this.center.Y);

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