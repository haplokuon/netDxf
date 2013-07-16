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
        /// <param name="center">Ellipse <see cref="Vector3">center</see> in object coordinates.</param>
        /// <param name="majorAxis">Ellipse major axis.</param>
        /// <param name="minorAxis">Ellipse minor axis.</param>
        /// <remarks>The center Z coordinate represents the elevation of the arc along the normal.</remarks>
        public Ellipse(Vector3 center, double majorAxis, double minorAxis)
            : base(EntityType.Ellipse, DxfObjectCode.Ellipse)
        {
            this.center = center;
            this.majorAxis = majorAxis;
            this.minorAxis = minorAxis;
            this.startAngle = 0.0;
            this.endAngle = 360.0;
            this.rotation = 0.0;
            this.thickness = 0.0;
        }

        /// <summary>
        /// Initializes a new instance of the <c>Ellipse</c> class.
        /// </summary>
        /// <param name="center">Ellipse <see cref="Vector2">center</see> in object coordinates.</param>
        /// <param name="majorAxis">Ellipse major axis.</param>
        /// <param name="minorAxis">Ellipse minor axis.</param>
        /// <remarks>The center Z coordinate represents the elevation of the arc along the normal.</remarks>
        public Ellipse(Vector2 center, double majorAxis, double minorAxis)
            : this (new Vector3(center.X, center.Y, 0.0), majorAxis, minorAxis)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Ellipse</c> class.
        /// </summary>
        public Ellipse()
            : this(Vector3.Zero, 1.0, 0.5)
        {
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
                if(value<=0)
                    throw new ArgumentOutOfRangeException("value",value,"The major axis value must be greater than zero.");
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
                    throw new ArgumentOutOfRangeException("value", value, "The minor axis value must be greater than zero.");
                this.minorAxis = value;
            }
        }

        /// <summary>
        /// Gets or sets the ellipse local rotation along its normal.
        /// </summary>
        /// <remarks>The angle is measured in degrees.</remarks>
        public double Rotation
        {
            get { return this.rotation; }
            set { this.rotation = value; }
        }

        /// <summary>
        /// Gets or sets the ellipse start angle in degrees.
        /// </summary>
        /// <remarks><c>StartAngle</c> equals 0 and <c>EndAngle</c> equals 360 for a full ellipse.</remarks>
        public double StartAngle
        {
            get { return this.startAngle; }
            set { this.startAngle = value; }
        }

        /// <summary>
        /// Gets or sets the ellipse end angle in degrees.
        /// </summary>
        /// <remarks><c>StartAngle</c> equals 0 and <c>EndAngle</c> equals 360 for a full ellipse.</remarks>
        public double EndAngle
        {
            get { return this.endAngle; }
            set { this.endAngle = value; }
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
        /// Checks if the the actual instance is a full ellipse.
        /// </summary>
        public bool IsFullEllipse
        {
            get { return (MathHelper.IsEqual(Math.Abs(this.endAngle - this.startAngle), 360.0)); }
        }

        #endregion

        #region methods

        /// <summary>
        /// Converts the ellipse in a Polyline.
        /// </summary>
        /// <param name="precision">Number of vertexes generated.</param>
        /// <returns>A new instance of <see cref="LwPolyline">LightWeightPolyline</see> that represents the ellipse.</returns>
        public LwPolyline ToPolyline(int precision)
        {
            IEnumerable<Vector2> vertexes = this.PolygonalVertexes(precision);
            Vector3 ocsCenter = MathHelper.Transform(this.center, this.Normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            LwPolyline poly = new LwPolyline
                                  {
                                      Color = this.Color,
                                      IsVisible = this.IsVisible,
                                      Layer = this.Layer,
                                      LineType = this.LineType,
                                      LineTypeScale = this.LineTypeScale,
                                      Lineweight = this.Lineweight,
                                      XData = this.XData,
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

        internal double[] GetParameters()
        {
            double atan1;
            double atan2;
            if (this.IsFullEllipse)
            {
                atan1 = 0.0;
                atan2 = MathHelper.TwoPI;
            }
            else
            {
                Vector2 startPoint = new Vector2(this.Center.X, this.Center.Y) + this.PolarCoordinateRelativeToCenter(this.StartAngle);
                Vector2 endPoint = new Vector2(this.Center.X, this.Center.Y) + this.PolarCoordinateRelativeToCenter(this.EndAngle);
                double a = this.MajorAxis * 0.5;
                double b = this.MinorAxis * 0.5;
                double px1 = ((startPoint.X - this.Center.X) / a);
                double py1 = ((startPoint.Y - this.Center.Y) / b);
                double px2 = ((endPoint.X - this.Center.X) / a);
                double py2 = ((endPoint.Y - this.Center.Y) / b);

                atan1 = Math.Atan2(py1, px1);
                atan2 = Math.Atan2(py2, px2);
            }
            return new[]{atan1, atan2};
        }

        internal void SetParameters(double[] param)
        {
            double a = this.MajorAxis * 0.5;
            double b = this.MinorAxis * 0.5;

            Vector2 start = new Vector2(a * Math.Cos(param[0]), b * Math.Sin(param[0]));
            Vector2 end = new Vector2(a * Math.Cos(param[1]), b * Math.Sin(param[1]));
            // trigonometry functions are very prone to round off errors
            if (start.Equals(end))
            {
                this.startAngle = 0.0;
                this.endAngle = 360.0;
            }
            else
            {
                this.startAngle = Vector2.Angle(start) * MathHelper.RadToDeg;
                this.endAngle = Vector2.Angle(end) * MathHelper.RadToDeg;
            }            
        }

        /// <summary>
        /// Calculate the local point on the ellipse for a given angle relative to the center.
        /// </summary>
        /// <param name="angle">Angle in degrees.</param>
        /// <returns>A local point on the ellipse for the given angle relative to the center.</returns>
        private Vector2 PolarCoordinateRelativeToCenter(double angle)
        {
            double a = this.MajorAxis * 0.5;
            double b = this.MinorAxis * 0.5;
            double radians = angle * MathHelper.DegToRad;

            double a1 = a * Math.Sin(radians);
            double b1 = b * Math.Cos(radians);

            double radius = (a * b) / Math.Sqrt(b1 * b1 + a1 * a1);

            // convert the radius back to cartesian coordinates
            return new Vector2(radius * Math.Cos(radians), radius * Math.Sin(radians));
        }

        /// <summary>
        /// Converts the ellipse in a list of vertexes.
        /// </summary>
        /// <param name="precision">Number of vertexes generated.</param>
        /// <returns>A list vertexes that represents the ellipse expresed in object coordinate system.</returns>
        private IEnumerable<Vector2> PolygonalVertexes(int precision)
        {
            List<Vector2> points = new List<Vector2>();
            double beta = this.rotation * MathHelper.DegToRad;
            double sinbeta = Math.Sin(beta);
            double cosbeta = Math.Cos(beta);

            if (this.IsFullEllipse)
            {
                for (int i = 0; i < 360; i += 360/precision)
                {
                    double alpha = i * MathHelper.DegToRad;
                    double sinalpha = Math.Sin(alpha);
                    double cosalpha = Math.Cos(alpha);

                    double pointX = 0.5f * (this.majorAxis * cosalpha * cosbeta - this.minorAxis * sinalpha * sinbeta);
                    double pointY = 0.5f * (this.majorAxis * cosalpha * sinbeta + this.minorAxis * sinalpha * cosbeta);

                    points.Add(new Vector2(pointX, pointY));
                }
            }
            else
            {
                for (int i = 0; i <= precision; i++)
                {
                    double angle = this.startAngle + i * (this.endAngle - this.startAngle) / precision;
                    Vector2 point = this.PolarCoordinateRelativeToCenter(angle);
                    // we need to apply the ellipse rotation to the local point
                    double pointX = point.X * cosbeta - point.Y * sinbeta;
                    double pointY = point.X * sinbeta + point.Y * cosbeta;
                    points.Add(new Vector2(pointX, pointY));
                }
            }
            return points;
        }

        #endregion

        #region overrides

        /// <summary>
        /// Creates a new Ellipse that is a copy of the current instance.
        /// </summary>
        /// <returns>A new Ellipse that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new Ellipse
            {
                //EntityObject properties
                Color = this.color,
                Layer = this.layer,
                LineType = this.lineType,
                Lineweight = this.lineweight,
                LineTypeScale = this.lineTypeScale,
                Normal = this.normal,
                XData = this.xData,
                //Ellipse properties
                Center = this.center,
                MajorAxis = this.majorAxis,
                MinorAxis = this.minorAxis,
                Rotation = this.rotation,
                StartAngle = this.startAngle,
                EndAngle = this.endAngle,
                Thickness = this.thickness
            };
        }

        #endregion

    }
}
