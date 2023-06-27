#region netDxf library licensed under the MIT License
// 
//                       netDxf library
// Copyright (c) Daniel Carvajal (haplokuon@gmail.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// 
#endregion

using System;
using System.Collections.Generic;
using netDxf.Tables;

namespace netDxf.Entities
{
    /// <summary>
    /// Represents a circle <see cref="EntityObject">entity</see>.
    /// </summary>
    public class Circle :
        EntityObject
    {
        #region private fields

        private Vector3 center;
        private double radius;
        private double thickness;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>Circle</c> class.
        /// </summary>
        public Circle()
            : this(Vector3.Zero, 1.0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Circle</c> class.
        /// </summary>
        /// <param name="center">Circle <see cref="Vector3">center</see> in world coordinates.</param>
        /// <param name="radius">Circle radius.</param>
        public Circle(Vector3 center, double radius)
            : base(EntityType.Circle, DxfObjectCode.Circle)
        {
            this.center = center;
            if (radius <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(radius), radius, "The circle radius must be greater than zero.");
            }
            this.radius = radius;
            this.thickness = 0.0;
        }

        /// <summary>
        /// Initializes a new instance of the <c>Circle</c> class.
        /// </summary>
        /// <param name="center">Circle <see cref="Vector2">center</see> in world coordinates.</param>
        /// <param name="radius">Circle radius.</param>
        public Circle(Vector2 center, double radius)
            : this(new Vector3(center.X, center.Y, 0.0), radius)
        {
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the circle <see cref="Vector3">center</see> in world coordinates.
        /// </summary>
        public Vector3 Center
        {
            get { return this.center; }
            set { this.center = value; }
        }

        /// <summary>
        /// Gets or set the circle radius.
        /// </summary>
        public double Radius
        {
            get { return this.radius; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The circle radius must be greater than zero.");
                }
                this.radius = value;
            }
        }

        /// <summary>
        /// Gets or sets the circle thickness.
        /// </summary>
        public double Thickness
        {
            get { return this.thickness; }
            set { this.thickness = value; }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Converts the circle in a list of vertexes.
        /// </summary>
        /// <param name="precision">Number of vertexes generated.</param>
        /// <returns>A list vertexes that represents the circle expressed in object coordinate system.</returns>
        public List<Vector2> PolygonalVertexes(int precision)
        {
            if (precision < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(precision), precision, "The precision must be equal or greater than two.");
            }

            List<Vector2> ocsVertexes = new List<Vector2>();

            double delta = MathHelper.TwoPI / precision;

            for (int i = 0; i < precision; i++)
            {
                double angle = delta * i;
                double sine = this.radius * Math.Sin(angle);
                double cosine = this.radius * Math.Cos(angle);
                ocsVertexes.Add(new Vector2(cosine, sine));
            }
            return ocsVertexes;
        }

        /// <summary>
        /// Converts the circle in a Polyline2D.
        /// </summary>
        /// <param name="precision">Number of vertexes generated.</param>
        /// <returns>A new instance of <see cref="Polyline2D">Polyline2D</see> that represents the circle.</returns>
        public Polyline2D ToPolyline2D(int precision)
        {
            IEnumerable<Vector2> vertexes = this.PolygonalVertexes(precision);
            Vector3 ocsCenter = MathHelper.Transform(this.Center, this.Normal, CoordinateSystem.World, CoordinateSystem.Object);

            Polyline2D poly = new Polyline2D
            {
                Layer = (Layer) this.Layer.Clone(),
                Linetype = (Linetype) this.Linetype.Clone(),
                Color = (AciColor) this.Color.Clone(),
                Lineweight = this.Lineweight,
                Transparency = (Transparency) this.Transparency.Clone(),
                LinetypeScale = this.LinetypeScale,
                Normal = this.Normal,
                Elevation = ocsCenter.Z,
                Thickness = this.thickness,
                IsClosed = true
            };
            foreach (Vector2 v in vertexes)
            {
                poly.Vertexes.Add(new Polyline2DVertex(v.X + ocsCenter.X, v.Y + ocsCenter.Y));
            }
            return poly;
        }

        #endregion

        #region overrides

        /// <summary>
        /// Moves, scales, and/or rotates the current entity given a 3x3 transformation matrix and a translation vector.
        /// </summary>
        /// <param name="transformation">Transformation matrix.</param>
        /// <param name="translation">Translation vector.</param>
        /// <remarks>
        /// Non-uniform scaling is not supported, create an ellipse from the circle data and transform that instead.<br />
        /// Matrix3 adopts the convention of using column vectors to represent a transformation matrix.
        /// </remarks>
        public override void TransformBy(Matrix3 transformation, Vector3 translation)
        {
            Vector3 newCenter = transformation * this.Center + translation;
            Vector3 newNormal = transformation * this.Normal;
            if (Vector3.Equals(Vector3.Zero, newNormal))
            {
                newNormal = this.Normal;
            }

            Matrix3 transOW = MathHelper.ArbitraryAxis(this.Normal);
            Matrix3 transWO = MathHelper.ArbitraryAxis(newNormal).Transpose();

            Vector3 axis = transOW * new Vector3(this.Radius, 0.0, 0.0);
            axis = transformation * axis;
            axis = transWO * axis;
            Vector2 axisPoint = new Vector2(axis.X, axis.Y);
            double newRadius = axisPoint.Modulus();
            if (MathHelper.IsZero(newRadius))
            {
                newRadius = MathHelper.Epsilon;
            }

            this.Normal = newNormal;
            this.Center = newCenter;
            this.Radius = newRadius;
        }

        /// <summary>
        /// Creates a new Circle that is a copy of the current instance.
        /// </summary>
        /// <returns>A new Circle that is a copy of this instance.</returns>
        public override object Clone()
        {
            Circle entity = new Circle
            {
                //EntityObject properties
                Layer = (Layer) this.Layer.Clone(),
                Linetype = (Linetype) this.Linetype.Clone(),
                Color = (AciColor) this.Color.Clone(),
                Lineweight = this.Lineweight,
                Transparency = (Transparency) this.Transparency.Clone(),
                LinetypeScale = this.LinetypeScale,
                Normal = this.Normal,
                IsVisible = this.IsVisible,
                //Circle properties
                Center = this.center,
                Radius = this.radius,
                Thickness = this.thickness
            };

            foreach (XData data in this.XData.Values)
            {
                entity.XData.Add((XData) data.Clone());
            }

            return entity;
        }

        #endregion
    }
}