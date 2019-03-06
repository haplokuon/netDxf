#region netDxf library, Copyright (C) 2009-2019 Daniel Carvajal (haplokuon@gmail.com)

//                        netDxf library
// Copyright (C) 2009-2019 Daniel Carvajal (haplokuon@gmail.com)
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
    /// Represents an ellipse <see cref="EntityObject">entity</see>.
    /// </summary>
    public class Ellipse :
        EntityObject
    {
        #region private fields

        private Vector3 center;
        private double majorAxis;
        private double minorAxis;
        private double rotation;
        private double startAngle;
        private double endAngle;
        private double thickness;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>Ellipse</c> class.
        /// </summary>
        public Ellipse()
            : this(Vector3.Zero, 1.0, 0.5)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Ellipse</c> class.
        /// </summary>
        /// <param name="center">Ellipse <see cref="Vector2">center</see> in object coordinates.</param>
        /// <param name="majorAxis">Ellipse major axis.</param>
        /// <param name="minorAxis">Ellipse minor axis.</param>
        /// <remarks>The center Z coordinate represents the elevation of the arc along the normal.</remarks>
        public Ellipse(Vector2 center, double majorAxis, double minorAxis)
            : this(new Vector3(center.X, center.Y, 0.0), majorAxis, minorAxis)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Ellipse</c> class.
        /// </summary>
        /// <param name="center">Ellipse <see cref="Vector3">center</see> in object coordinates.</param>
        /// <param name="majorAxis">Ellipse major axis.</param>
        /// <param name="minorAxis">Ellipse minor axis.</param>
        /// <remarks>The center Z coordinate represents the elevation of the arc along the normal.</remarks>
        public Ellipse(Vector3 center, double majorAxis, double minorAxis)
            : base(EntityType.Ellipse, DxfObjectCode.Ellipse)
        {
            this.center = center;
            if (majorAxis <= 0)
                throw new ArgumentOutOfRangeException(nameof(majorAxis), majorAxis, "The major axis value must be greater than zero.");
            this.majorAxis = majorAxis;
            if (minorAxis <= 0)
                throw new ArgumentOutOfRangeException(nameof(minorAxis), minorAxis, "The minor axis value must be greater than zero.");
            this.minorAxis = minorAxis;
            this.startAngle = 0.0;
            this.endAngle = 0.0;
            this.rotation = 0.0;
            this.thickness = 0.0;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the ellipse <see cref="Vector3">center</see>.
        /// </summary>
        /// <remarks>The center Z coordinate represents the elevation of the arc along the normal.</remarks>
        public Vector3 Center
        {
            get { return this.center; }
            set { this.center = value; }
        }

        /// <summary>
        /// Gets or sets the ellipse mayor axis.
        /// </summary>
        public double MajorAxis
        {
            get { return this.majorAxis; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The major axis value must be greater than zero.");
                this.majorAxis = value;
            }
        }

        /// <summary>
        /// Gets or sets the ellipse minor axis.
        /// </summary>
        public double MinorAxis
        {
            get { return this.minorAxis; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The minor axis value must be greater than zero.");
                this.minorAxis = value;
            }
        }

        /// <summary>
        /// Gets or sets the ellipse local rotation in degrees along its normal.
        /// </summary>
        public double Rotation
        {
            get { return this.rotation; }
            set { this.rotation = MathHelper.NormalizeAngle(value); }
        }

        /// <summary>
        /// Gets or sets the ellipse start angle in degrees.
        /// </summary>
        /// <remarks>To get a full ellipse set the start angle equal to the end angle.</remarks>
        public double StartAngle
        {
            get { return this.startAngle; }
            set { this.startAngle = MathHelper.NormalizeAngle(value); }
        }

        /// <summary>
        /// Gets or sets the ellipse end angle in degrees.
        /// </summary>
        /// <remarks>To get a full ellipse set the end angle equal to the start angle.</remarks>
        public double EndAngle
        {
            get { return this.endAngle; }
            set { this.endAngle = MathHelper.NormalizeAngle(value); }
        }

        /// <summary>
        /// Gets or sets the ellipse thickness.
        /// </summary>
        public double Thickness
        {
            get { return this.thickness; }
            set { this.thickness = value; }
        }

        /// <summary>
        /// Checks if the actual instance is a full ellipse.
        /// </summary>
        /// <remarks>An ellipse is considered full when its start and end angles are equal.</remarks>
        public bool IsFullEllipse
        {
            get { return MathHelper.IsEqual(this.startAngle, this.endAngle); }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Calculate the local point on the ellipse for a given angle relative to the center.
        /// </summary>
        /// <param name="angle">Angle in degrees.</param>
        /// <returns>A local point on the ellipse for the given angle relative to the center.</returns>
        public Vector2 PolarCoordinateRelativeToCenter(double angle)
        {
            double a = this.MajorAxis*0.5;
            double b = this.MinorAxis*0.5;
            double radians = angle*MathHelper.DegToRad;

            double a1 = a*Math.Sin(radians);
            double b1 = b*Math.Cos(radians);

            double radius = (a*b)/Math.Sqrt(b1*b1 + a1*a1);

            // convert the radius back to cartesian coordinates
            return new Vector2(radius*Math.Cos(radians), radius*Math.Sin(radians));
        }

        /// <summary>
        /// Converts the ellipse in a list of vertexes.
        /// </summary>
        /// <param name="precision">Number of divisions.</param>
        /// <returns>A list vertexes that represents the ellipse expressed in object coordinate system.</returns>
        public List<Vector2> PolygonalVertexes(int precision)
        {
            List<Vector2> points = new List<Vector2>();
            double beta = this.rotation * MathHelper.DegToRad;
            double sinbeta = Math.Sin(beta);
            double cosbeta = Math.Cos(beta);

            if (this.IsFullEllipse)
            {
                double delta = MathHelper.TwoPI/precision;
                for (int i = 0; i < precision; i++)
                {
                    double angle = delta*i;
                    double sinalpha = Math.Sin(angle);
                    double cosalpha = Math.Cos(angle);

                    double pointX = 0.5*(this.majorAxis*cosalpha*cosbeta - this.minorAxis*sinalpha*sinbeta);
                    double pointY = 0.5*(this.majorAxis*cosalpha*sinbeta + this.minorAxis*sinalpha*cosbeta);

                    points.Add(new Vector2(pointX, pointY));
                }
            }
            else
            {
                double start = this.startAngle;
                double end = this.endAngle;
                if (end < start) end += 360.0;
                double delta = (end - start) / precision;
                for (int i = 0; i <= precision; i++)
                {
                    Vector2 point = this.PolarCoordinateRelativeToCenter(start + delta*i);
                    // we need to apply the ellipse rotation to the local point
                    double pointX = point.X * cosbeta - point.Y * sinbeta;
                    double pointY = point.X * sinbeta + point.Y * cosbeta;
                    points.Add(new Vector2(pointX, pointY));
                }
            }
            return points;
        }

        /// <summary>
        /// Converts the ellipse in a Polyline.
        /// </summary>
        /// <param name="precision">Number of divisions.</param>
        /// <returns>A new instance of <see cref="LwPolyline">LightWeightPolyline</see> that represents the ellipse.</returns>
        public LwPolyline ToPolyline(int precision)
        {
            IEnumerable<Vector2> vertexes = this.PolygonalVertexes(precision);
            Vector3 ocsCenter = MathHelper.Transform(this.center, this.Normal, CoordinateSystem.World, CoordinateSystem.Object);
            LwPolyline poly = new LwPolyline
            {
                Layer = (Layer) this.Layer.Clone(),
                Linetype = (Linetype) this.Linetype.Clone(),
                Color = (AciColor) this.Color.Clone(),
                Lineweight = this.Lineweight,
                Transparency = (Transparency) this.Transparency.Clone(),
                LinetypeScale = this.LinetypeScale,
                Normal = this.Normal,
                Elevation = ocsCenter.Z,
                Thickness = this.Thickness,
                IsClosed = this.IsFullEllipse
            };

            foreach (Vector2 v in vertexes)
            {
                poly.Vertexes.Add(new LwPolylineVertex(v.X + ocsCenter.X, v.Y + ocsCenter.Y));
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
        /// <remarks>When a non-uniform scaling is applied to a rotated ellipses the result it is not correct.</remarks>
        public override void TransformBy(Matrix3 transformation, Vector3 translation)
        {
            Vector3 newCenter;
            Vector3 newNormal;
            double newMajorAxis;
            double newMinorAxis;
            double newRotation;
            double newScale;

            newCenter = transformation * this.Center + translation;
            newNormal = transformation * this.Normal;

            Matrix3 transOW = MathHelper.ArbitraryAxis(this.Normal);
            transOW *= Matrix3.RotationZ(this.Rotation * MathHelper.DegToRad);

            Matrix3 transWO = MathHelper.ArbitraryAxis(newNormal);
            transWO = transWO.Transpose();

            Vector3 v = transOW * Vector3.UnitX;
            v = transformation * v;
            v = transWO * v;
            double angle = Vector2.Angle(new Vector2(v.X, v.Y));

            //Vector3 v1 = new Vector3(0.5 * this.MajorAxis, 0.5 * this.minorAxis, 0.0);
            //Vector3 v2 = new Vector3(-0.5 * this.MajorAxis, 0.5 * this.minorAxis, 0.0);
            //v1 = transOW * v1;
            //v1 = transformation * v1;
            //v1 = transWO * v1;

            //v2 = transOW * v2;
            //v2 = transformation * v2;
            //v2 = transWO * v2;

            //Vector3 v1 = new Vector3(0.5 * this.MajorAxis, 0.0, 0.0);
            //Vector3 v2 = new Vector3(0.0, 0.5 * this.minorAxis, 0.0);
            //v1 = transOW * v1;
            //v1 = transformation * v1;
            //v1 = transWO * v1;

            //v2 = transOW * v2;
            //v2 = transformation * v2;
            //v2 = transWO * v2;

            //double b = Vector2.Angle(new Vector2(v1.X, v1.Y)) * MathHelper.RadToDeg;

            double sign = Math.Sign(transformation.M11 * transformation.M22 * transformation.M33) < 0 ? 180 : 0;
            newRotation = sign + angle * MathHelper.RadToDeg;

            //transWO = Matrix3.RotationZ(newRotation * MathHelper.DegToRad).Transpose() * transWO;

            //Vector3 s = transOW * new Vector3(this.MajorAxis, this.MinorAxis, 0.0);
            //s = transformation * s;
            //s = transWO * s;

            //newMajorAxis = s.X <= 0 ? MathHelper.Epsilon : s.X;
            //newMinorAxis = s.Y <= 0 ? MathHelper.Epsilon : s.Y;

            newScale = newNormal.Modulus();

            newMajorAxis = this.MajorAxis * newScale;
            newMajorAxis = MathHelper.IsZero(newMajorAxis) ? MathHelper.Epsilon : newMajorAxis;

            newMinorAxis = this.MinorAxis * newScale;
            newMinorAxis = MathHelper.IsZero(newMinorAxis) ? MathHelper.Epsilon : newMinorAxis;

            this.Center = newCenter;
            this.Normal = newNormal;
            this.Rotation = newRotation;

            if (newMinorAxis > newMajorAxis)
            {
                this.MajorAxis = newMinorAxis;
                this.MinorAxis = newMajorAxis;
            }
            else
            {
                this.MajorAxis = newMajorAxis;
                this.MinorAxis = newMinorAxis;
            }
        }

        //public override void TransformBy2(Matrix3 transformation, Vector3 translation)
        //{
        //    Vector3 newCenter;
        //    Vector3 newNormal;
        //    double newMajorAxis;
        //    double newMinorAxis;
        //    double newRotation;

        //    newCenter = transformation * this.Center + translation;
        //    newNormal = transformation * this.Normal;

        //    Matrix3 transOW = MathHelper.ArbitraryAxis(this.Normal);
        //    transOW *= Matrix3.RotationZ(this.Rotation * MathHelper.DegToRad);

        //    Matrix3 transWO = MathHelper.ArbitraryAxis(newNormal);
        //    transWO = transWO.Transpose();

        //    Vector3 v = transOW * Vector3.UnitX;
        //    v = transformation * v;
        //    v = transWO * v;
        //    double angle = Vector2.Angle(new Vector2(v.X, v.Y));

        //    newRotation = angle * MathHelper.RadToDeg;

        //    transWO = Matrix3.RotationZ(newRotation * MathHelper.DegToRad).Transpose() * transWO;

        //    Vector3 s = transOW * new Vector3(this.MajorAxis, this.MinorAxis, 0.0);
        //    s = transformation * s;
        //    s = transWO * s;

        //    newMajorAxis = s.X <= 0 ? MathHelper.Epsilon : s.X;
        //    newMinorAxis = s.Y <= 0 ? MathHelper.Epsilon : s.Y;

        //    this.Center = newCenter;
        //    this.Normal = newNormal;
        //    this.Rotation = newRotation;
        //    if (newMinorAxis > newMajorAxis)
        //    {
        //        this.MajorAxis = newMinorAxis;
        //        this.MinorAxis = newMajorAxis;
        //    }
        //    else
        //    {
        //        this.MajorAxis = newMajorAxis;
        //        this.MinorAxis = newMinorAxis;
        //    }
        //}

        /// <summary>
        /// Creates a new Ellipse that is a copy of the current instance.
        /// </summary>
        /// <returns>A new Ellipse that is a copy of this instance.</returns>
        public override object Clone()
        {
            Ellipse entity = new Ellipse
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
                //Ellipse properties
                Center = this.center,
                MajorAxis = this.majorAxis,
                MinorAxis = this.minorAxis,
                Rotation = this.rotation,
                StartAngle = this.startAngle,
                EndAngle = this.endAngle,
                Thickness = this.thickness
            };

            foreach (XData data in this.XData.Values)
                entity.XData.Add((XData) data.Clone());

            return entity;
        }

        #endregion
    }
}