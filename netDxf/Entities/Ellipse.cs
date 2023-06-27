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
using System.Diagnostics;
using netDxf.Tables;

namespace netDxf.Entities
{
    /// <summary>
    /// Represents an ellipse <see cref="EntityObject">entity</see>.
    /// </summary>
    public class Ellipse :
        EntityObject
    {
        #region private classes

        private static class ConicThroughFivePoints
        {
            private static double[] CoefficientsLine(Vector2 p1, Vector2 p2)
            {
                // line: A*x + B*y + C = 0
                double[] coefficients = new double[3];
                coefficients[0] = p1.Y - p2.Y; //A
                coefficients[1] = p2.X - p1.X; //B
                coefficients[2] = p1.X * p2.Y - p2.X * p1.Y; //C
                return coefficients;
            }

            private static double[] CoefficientsProductLines(double[] l1, double[] l2)
            {
                // lines product: A*x2 + B*xy + C*y2 + D*x + E*y + F = 0
                double[] coefficients = new double[6];
                coefficients[0] = l1[0] * l2[0]; //A
                coefficients[1] = l1[0] * l2[1] + l1[1] * l2[0]; //B
                coefficients[2] = l1[1] * l2[1]; //C
                coefficients[3] = l1[0] * l2[2] + l1[2] * l2[0]; //D
                coefficients[4] = l1[1] * l2[2] + l1[2] * l2[1]; //E
                coefficients[5] = l1[2] * l2[2]; //F
                return coefficients;
            }

            private static double Lambda(double[] alpha_beta, double[] gamma_delta, Vector2 p)
            {
                // conic families: alpha_beta + lambda*gamma_delta = 0
                double a1 = alpha_beta[0] * p.X * p.X;
                double b1 = alpha_beta[1] * p.X * p.Y;
                double c1 = alpha_beta[2] * p.Y * p.Y;
                double d1 = alpha_beta[3] * p.X;
                double e1 = alpha_beta[4] * p.Y;
                double f1 = alpha_beta[5];

                double a2 = gamma_delta[0] * p.X * p.X;
                double b2 = gamma_delta[1] * p.X * p.Y;
                double c2 = gamma_delta[2] * p.Y * p.Y;
                double d2 = gamma_delta[3] * p.X;
                double e2 = gamma_delta[4] * p.Y;
                double f2 = gamma_delta[5];

                double sum1 = a1 + b1 + c1 + d1 + e1 + f1;
                double sum2 = a2 + b2 + c2 + d2 + e2 + f2;

                if (MathHelper.IsZero(sum2))
                {
                    return double.NaN;
                }

                return -sum1 / sum2;
            }

            private static double[] ConicCoefficients(Vector2 point1, Vector2 point2, Vector2 point3, Vector2 point4, Vector2 point5)
            {
                double[] lineAlpha = CoefficientsLine(point1, point2);
                double[] lineBeta = CoefficientsLine(point3, point4);
                double[] lineGamma = CoefficientsLine(point1, point3);
                double[] lineDelta = CoefficientsLine(point2, point4);

                double[] alphaBeta = CoefficientsProductLines(lineAlpha, lineBeta);
                double[] gammaDelta = CoefficientsProductLines(lineGamma, lineDelta);

                double lambda = Lambda(alphaBeta, gammaDelta, point5);
                if (double.IsNaN(lambda))
                { 
                    // conic coefficients cannot be found, duplicate points
                    return null;
                }

                double[] coefficients = new double[6];
                coefficients[0] = alphaBeta[0] + lambda * gammaDelta[0];
                coefficients[1] = alphaBeta[1] + lambda * gammaDelta[1];
                coefficients[2] = alphaBeta[2] + lambda * gammaDelta[2];
                coefficients[3] = alphaBeta[3] + lambda * gammaDelta[3];
                coefficients[4] = alphaBeta[4] + lambda * gammaDelta[4];
                coefficients[5] = alphaBeta[5] + lambda * gammaDelta[5];

                return coefficients;
            }

            public static bool EllipseProperties(Vector2 point1, Vector2 point2, Vector2 point3, Vector2 point4, Vector2 point5, out Vector2 center, out double semiMajorAxis, out double semiMinorAxis, out double rotation)
            {         
                center = Vector2.NaN;
                semiMajorAxis = double.NaN;
                semiMinorAxis = double.NaN;
                rotation = double.NaN;

                double[] coefficients = ConicCoefficients(point1, point2, point3, point4, point5);
                if (coefficients == null)
                {
                    return false;
                }

                double a = coefficients[0];
                double b = coefficients[1];
                double c = coefficients[2];
                double d = coefficients[3];
                double e = coefficients[4];
                double f = coefficients[5];

                double q = b * b - 4 * a * c;
                           
                if (q >= 0)
                {
                    // not an ellipse
                    return false;
                }

                center.X = (2 * c * d - b * e) / q;
                center.Y = (2 * a * e - b * d) / q;

                double m = Math.Sqrt((a - c) * (a - c) + b * b);
                double n = 2 * (a * e * e + c * d * d - b * d * e + q * f);
                double axis1 = -Math.Sqrt(n * (a + c + m)) / q;
                double axis2 = -Math.Sqrt(n * (a + c - m)) / q;

                if (MathHelper.IsZero(b))
                {
                    // ellipse parallel to world axis
                    if (MathHelper.IsEqual(a, c))
                    {
                        rotation = 0.0;
                    }
                    else
                    {
                        rotation = a < c ? 0.0 : MathHelper.HalfPI;
                    }
                }
                else
                {
                    rotation = Math.Atan((c - a - Math.Sqrt((a - c) * (a - c) + b * b)) / b);
                }

                if (axis1 >= axis2)
                {
                    semiMajorAxis = axis1;
                    semiMinorAxis = axis2;
                }
                else
                {
                    semiMajorAxis = axis2;
                    semiMinorAxis = axis1;
                    rotation += MathHelper.HalfPI;
                }
                return true;
            }
        }

        #endregion

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
        /// <remarks>
        /// The center Z coordinate represents the elevation of the ellipse along the normal.
        /// The major axis is always measured along the ellipse local X axis,
        /// while the minor axis is along the local Y axis.
        /// </remarks>
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
        /// <remarks>
        /// The center Z coordinate represents the elevation of the ellipse along the normal.
        /// The major axis is always measured along the ellipse local X axis,
        /// while the minor axis is along the local Y axis.
        /// </remarks>
        public Ellipse(Vector3 center, double majorAxis, double minorAxis)
            : base(EntityType.Ellipse, DxfObjectCode.Ellipse)
        {
            this.center = center;

            if (majorAxis <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(majorAxis), majorAxis, "The major axis value must be greater than zero.");
            }

            if (minorAxis <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minorAxis), minorAxis, "The minor axis value must be greater than zero.");
            }

            if (minorAxis > majorAxis)
            {
                throw new ArgumentException("The major axis must be greater than the minor axis.");
            }

            this.majorAxis = majorAxis;
            this.minorAxis = minorAxis;
            this.startAngle = 0.0;
            this.endAngle = 0.0;
            this.rotation = 0.0;
            this.thickness = 0.0;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the ellipse <see cref="Vector3">center</see> in world coordinates.
        /// </summary>
        public Vector3 Center
        {
            get { return this.center; }
            set { this.center = value; }
        }

        /// <summary>
        /// Gets or sets the ellipse mayor axis.
        /// </summary>
        /// <remarks>The major axis is always measured along the ellipse local X axis.</remarks>
        public double MajorAxis
        {
            get { return this.majorAxis; }
        }

        /// <summary>
        /// Gets or sets the ellipse minor axis.
        /// </summary>
        /// <remarks>The minor axis is always measured along the ellipse local Y axis.</remarks>
        public double MinorAxis
        {
            get { return this.minorAxis; }
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
        /// Sets the ellipse major and minor axis from the two parameters.
        /// </summary>
        /// <param name="axis1">Ellipse axis.</param>
        /// <param name="axis2">Ellipse axis.</param>
        /// <remarks>
        /// It is not required that axis1 is greater than axis2. The larger value will be assigned as major axis and the lower as minor axis.
        /// </remarks>
        public void SetAxis(double axis1, double axis2)
        {
            if (axis1 <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(axis1), axis1, "The axis value must be greater than zero.");
            }

            if (axis2 <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(axis2), axis2, "The axis value must be greater than zero.");
            }

            if (axis2 > axis1)
            {
                this.majorAxis = axis2;
                this.minorAxis = axis1;
            }
            else
            {
                this.majorAxis = axis1;
                this.minorAxis = axis2;
            }
        }

        /// <summary>
        /// Calculate the local point on the ellipse for a given angle relative to the center.
        /// </summary>
        /// <param name="angle">Angle in degrees.</param>
        /// <returns>A local point on the ellipse for the given angle relative to the center.</returns>
        public Vector2 PolarCoordinateRelativeToCenter(double angle)
        {
            double a = this.MajorAxis * 0.5;
            double b = this.MinorAxis * 0.5;
            double radians = angle * MathHelper.DegToRad;

            double a1 = a * Math.Sin(radians);
            double b1 = b * Math.Cos(radians);

            double radius = (a * b) / Math.Sqrt(b1 * b1 + a1 * a1);

            // convert the radius back to Cartesian coordinates
            return new Vector2(radius * Math.Cos(radians), radius * Math.Sin(radians));
        }

        /// <summary>
        /// Converts the ellipse in a list of vertexes.
        /// </summary>
        /// <param name="precision">Number of vertexes generated.</param>
        /// <returns>A list vertexes that represents the ellipse expressed in object coordinate system.</returns>
        public List<Vector2> PolygonalVertexes(int precision)
        {
            if (precision < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(precision), precision, "The arc precision must be equal or greater than two.");
            }

            List<Vector2> points = new List<Vector2>();
            double beta = this.rotation * MathHelper.DegToRad;
            double sinBeta = Math.Sin(beta);
            double cosBeta = Math.Cos(beta);
            double start;
            double end;
            double steps;

            if (this.IsFullEllipse)
            {
                start = 0;
                end = MathHelper.TwoPI;
                steps = precision;
            }
            else
            {
                Vector2 startPoint = this.PolarCoordinateRelativeToCenter(this.startAngle);
                Vector2 endPoint = this.PolarCoordinateRelativeToCenter(this.endAngle);
                double a = 1 / (0.5 * this.majorAxis);
                double b = 1 / (0.5 * this.minorAxis);
                start = Math.Atan2(startPoint.Y * b, startPoint.X * a);
                end = Math.Atan2(endPoint.Y * b, endPoint.X * a);

                if (end < start)
                {
                    end += MathHelper.TwoPI;
                }
                steps = precision - 1;
            }
           
            double delta = (end - start) / steps;

            for (int i = 0; i < precision; i++)
            {
                double angle = start + delta * i;
                double sinAlpha = Math.Sin(angle);
                double cosAlpha = Math.Cos(angle);

                double pointX = 0.5 * (this.majorAxis * cosAlpha * cosBeta - this.minorAxis * sinAlpha * sinBeta);
                double pointY = 0.5 * (this.majorAxis * cosAlpha * sinBeta + this.minorAxis * sinAlpha * cosBeta);

                points.Add(new Vector2(pointX, pointY));
            }

            return points;
        }

        /// <summary>
        /// Converts the ellipse in a Polyline2D.
        /// </summary>
        /// <param name="precision">Number of vertexes generated.</param>
        /// <returns>A new instance of <see cref="Polyline2D">Polyline2D</see> that represents the ellipse.</returns>
        public Polyline2D ToPolyline2D(int precision)
        {
            List<Vector2> vertexes = this.PolygonalVertexes(precision);
            Vector3 ocsCenter = MathHelper.Transform(this.center, this.Normal, CoordinateSystem.World, CoordinateSystem.Object);
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
                Thickness = this.Thickness,
                IsClosed = this.IsFullEllipse
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
        /// <remarks>Matrix3 adopts the convention of using column vectors to represent a transformation matrix.</remarks>
        public override void TransformBy(Matrix3 transformation, Vector3 translation)
        {
            // NOTE: this is a generic implementation of the ellipse transformation,
            // for non rotated ellipses and/or uniform scaling the code can be simplified

            // rectangle that circumscribe the ellipse
            double semiMajorAxis = this.MajorAxis * 0.5;
            double semiMinorAxis = this.MinorAxis * 0.5;

            Vector2 p1 = new Vector2(-semiMajorAxis, semiMinorAxis);
            Vector2 p2 = new Vector2(semiMajorAxis, semiMinorAxis);
            Vector2 p3 = new Vector2(-semiMajorAxis, -semiMinorAxis);
            Vector2 p4 = new Vector2(semiMajorAxis, -semiMinorAxis);
            List<Vector2> ocsPoints = MathHelper.Transform(new[] {p1, p2, p3, p4}, this.Rotation * MathHelper.DegToRad, CoordinateSystem.Object, CoordinateSystem.World);

            Vector3 p1Prime = new Vector3(ocsPoints[0].X, ocsPoints[0].Y, 0.0);
            Vector3 p2Prime = new Vector3(ocsPoints[1].X, ocsPoints[1].Y, 0.0);
            Vector3 p3Prime = new Vector3(ocsPoints[2].X, ocsPoints[2].Y, 0.0);
            Vector3 p4Prime = new Vector3(ocsPoints[3].X, ocsPoints[3].Y, 0.0);
            List<Vector3> wcsPoints = MathHelper.Transform(new[] {p1Prime, p2Prime, p3Prime, p4Prime}, this.Normal, CoordinateSystem.Object, CoordinateSystem.World);
            for (int i = 0; i < wcsPoints.Count; i++)
            {
                wcsPoints[i] += this.Center;

                wcsPoints[i] = transformation * wcsPoints[i];
                wcsPoints[i] += translation;
            }

            Vector3 newNormal = transformation * this.Normal;
            if (Vector3.Equals(Vector3.Zero, newNormal))
            {
                newNormal = this.Normal;
            }

            List<Vector3> rectPoints = MathHelper.Transform(wcsPoints, newNormal, CoordinateSystem.World, CoordinateSystem.Object);
            
            // corners of the transformed rectangle that circumscribe the new ellipse        
            Vector2 pointA = new Vector2(rectPoints[0].X, rectPoints[0].Y);
            Vector2 pointB = new Vector2(rectPoints[1].X, rectPoints[1].Y);
            Vector2 pointC = new Vector2(rectPoints[2].X, rectPoints[2].Y);
            Vector2 pointD = new Vector2(rectPoints[3].X, rectPoints[3].Y);

            // the new ellipse is tangent at the mid points
            Vector2 pointM = Vector2.MidPoint(pointA, pointB);
            Vector2 pointN = Vector2.MidPoint(pointC, pointD);
            Vector2 pointH = Vector2.MidPoint(pointA, pointC);
            Vector2 pointK = Vector2.MidPoint(pointB, pointD);

            // we need to find a fifth point
            Vector2 origin = Vector2.MidPoint(pointH, pointK);
            Vector2 pointX = Vector2.MidPoint(pointH, origin); // a point along the OH segment

            // intersection line AC and line parallel to BC through pointX
            Vector2 pointY = MathHelper.FindIntersection(pointA, pointC - pointA, pointX, pointC - pointB);
            if (Vector2.IsNaN(pointY))
            {
                Debug.Assert(false, "The transformation cannot be applied.");
                return;
            }

            // find the fifth point in the ellipse
            Vector2 pointZ = MathHelper.FindIntersection(pointM, pointX - pointM, pointN, pointY - pointN);
            if(Vector2.IsNaN(pointZ))
            {
                Debug.Assert(false, "The transformation cannot be applied.");
                return;
            }
            
            Vector3 oldNormal = this.Normal;
            double oldRotation = this.Rotation * MathHelper.DegToRad;

            if (ConicThroughFivePoints.EllipseProperties(pointM, pointN, pointH, pointK, pointZ, out Vector2 _, out double newSemiMajorAxis, out double newSemiMinorAxis, out double newRotation))
            {
                double axis1 = 2 * newSemiMajorAxis;
                axis1 = MathHelper.IsZero(axis1) ? MathHelper.Epsilon : axis1;
                double axis2 = 2 * newSemiMinorAxis;
                axis2 = MathHelper.IsZero(axis2) ? MathHelper.Epsilon : axis2;

                this.Center = transformation * this.Center + translation;
                this.SetAxis(axis1, axis2);
                this.Rotation = newRotation * MathHelper.RadToDeg;
                this.Normal = newNormal;
            }
            else
            {
                Debug.Assert(false, "The transformation cannot be applied.");
                return;
            }

            if (this.IsFullEllipse)
            {
                return;
            }

            //if not full ellipse calculate start and end angles
            Vector2 start = this.PolarCoordinateRelativeToCenter(this.StartAngle);
            Vector2 end = this.PolarCoordinateRelativeToCenter(this.EndAngle);
            start = Vector2.Rotate(start, oldRotation);
            end = Vector2.Rotate(end, oldRotation);

            Vector3 wcsStart = MathHelper.Transform(new Vector3(start.X, start.Y, 0.0), oldNormal, CoordinateSystem.Object, CoordinateSystem.World);
            wcsStart = transformation * wcsStart;

            Vector3 wcsEnd = MathHelper.Transform(new Vector3(end.X, end.Y, 0.0), oldNormal, CoordinateSystem.Object, CoordinateSystem.World);
            wcsEnd = transformation * wcsEnd;

            Vector3 ocsStart = MathHelper.Transform(wcsStart, newNormal, CoordinateSystem.World, CoordinateSystem.Object);
            Vector3 ocsEnd = MathHelper.Transform(wcsEnd, newNormal, CoordinateSystem.World, CoordinateSystem.Object);

            Vector2 newStart = new Vector2(ocsStart.X, ocsStart.Y);
            Vector2 newEnd = new Vector2(ocsEnd.X, ocsEnd.Y);
            newStart = Vector2.Rotate(newStart, -newRotation);
            newEnd = Vector2.Rotate(newEnd, -newRotation);

            if (Math.Sign(transformation.M11 * transformation.M22 * transformation.M33) < 0)
            {
                this.EndAngle = Vector2.Angle(newStart) * MathHelper.RadToDeg;
                this.StartAngle = Vector2.Angle(newEnd) * MathHelper.RadToDeg;
            }
            else
            {
                this.StartAngle = Vector2.Angle(newStart) * MathHelper.RadToDeg;
                this.EndAngle = Vector2.Angle(newEnd) * MathHelper.RadToDeg;
            }
        }

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
                majorAxis = this.majorAxis,
                minorAxis = this.minorAxis,
                Rotation = this.rotation,
                StartAngle = this.startAngle,
                EndAngle = this.endAngle,
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