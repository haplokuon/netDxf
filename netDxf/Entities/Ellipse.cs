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
    /// Represents an ellipse <see cref="netDxf.Entities.IEntityObject">entity</see>.
    /// </summary>
    public class Ellipse :
        DxfObject,
        IEntityObject
    {
        #region private fields

        private const EntityType TYPE = EntityType.Ellipse;
        private Vector3 center;
        private double majorAxis;
        private double minorAxis;
        private double rotation;
        private double startAngle;
        private double endAngle;
        private double thickness;
        private Layer layer;
        private AciColor color;
        private LineType lineType;
        private Vector3 normal;
        private int curvePoints;
        private Dictionary<ApplicationRegistry, XData> xData;

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
            : base(DxfObjectCode.Ellipse)
        {
            this.center = center;
            this.majorAxis = majorAxis;
            this.minorAxis = minorAxis;
            this.startAngle = 0.0;
            this.endAngle = 360.0;
            this.rotation = 0.0;
            this.curvePoints = 30;
            this.thickness = 0.0;
            this.layer = Layer.Default;
            this.color = AciColor.ByLayer;
            this.lineType = LineType.ByLayer;
            this.normal = Vector3.UnitZ;
        }

        /// <summary>
        /// Initializes a new instance of the <c>ellipse</c> class.
        /// </summary>
        public Ellipse()
            : base(DxfObjectCode.Ellipse)
        {
            this.center = Vector3.Zero;
            this.majorAxis = 1.0;
            this.minorAxis = 0.5;
            this.rotation = 0.0;
            this.startAngle = 0.0;
            this.endAngle = 360.0;
            this.rotation = 0.0;
            this.curvePoints = 30;
            this.thickness = 0.0;
            this.layer = Layer.Default;
            this.color = AciColor.ByLayer;
            this.lineType = LineType.ByLayer;
            this.normal = Vector3.UnitZ;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the ellipse <see cref="netDxf.Vector3">center</see>.
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
        /// Gets or sets the ellipse rotation along its normal.
        /// </summary>
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
        /// Gets or sets the ellipse <see cref="netDxf.Vector3">normal</see>.
        /// </summary>
        public Vector3 Normal
        {
            get { return this.normal; }
            set
            {
                value.Normalize();
                this.normal = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of points generated along the ellipse during the conversion to a polyline.
        /// </summary>
        public int CurvePoints
        {
            get { return this.curvePoints; }
            set { this.curvePoints = value; }
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
            get { return (MathHelper.IsEqual(this.startAngle + this.endAngle, 360.0)); }
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
        /// Converts the ellipse in a Polyline.
        /// </summary>
        /// <param name="precision">Number of vertexes generated.</param>
        /// <returns>A new instance of <see cref="Polyline">Polyline</see> that represents the ellipse.</returns>
        public Polyline ToPolyline(int precision)
        {
            List<Vector2> vertexes = this.PolygonalVertexes(precision);
            Vector3 ocsCenter = MathHelper.Transform(this.center,
                                                      this.normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);

            Polyline poly = new Polyline
            {
                Color = this.color,
                Layer = this.layer,
                LineType = this.lineType,
                Normal = this.normal,
                Elevation = ocsCenter.Z,
                Thickness = this.thickness
            };
            poly.XData=this.xData;

            foreach (Vector2 v in vertexes)
            {
                poly.Vertexes.Add(new PolylineVertex(v.X + ocsCenter.X, v.Y + ocsCenter.Y));
            }
            if (this.IsFullEllipse)
                poly.IsClosed = true;

            return poly;
        }

        /// <summary>
        /// Converts the ellipse in a list of vertexes.
        /// </summary>
        /// <param name="precision">Number of vertexes generated.</param>
        /// <returns>A list vertexes that represents the ellipse expresed in object coordinate system.</returns>
        public List<Vector2> PolygonalVertexes(int precision)
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
                    points.Add(this.PointFromEllipse(angle));
                }
            }
            return points;
        }

        private Vector2 PointFromEllipse(double degrees)
        {
            // Convert the basic input into something more usable
            Vector2 ptCenter = new Vector2(this.center.X, this.center.Y);
            double radians = degrees * MathHelper.DegToRad;

            // Calculate the radius of the ellipse for the given angle
            double a = this.majorAxis;
            double b = this.minorAxis;
            double eccentricity = Math.Sqrt(1 - (b * b) / (a * a));
            double radiusAngle = b / Math.Sqrt(1 - (eccentricity * eccentricity) * Math.Pow(Math.Cos(radians), 2));

            // Convert the radius back to Cartesian coordinates
            return new Vector2(ptCenter.X + radiusAngle*Math.Cos(radians), ptCenter.Y + radiusAngle*Math.Sin(radians));
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