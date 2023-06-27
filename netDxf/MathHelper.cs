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

namespace netDxf
{
    /// <summary>
    /// Utility math functions and constants.
    /// </summary>
    public static class MathHelper
    {
        #region constants

        /// <summary>
        /// Constant to transform an angle between degrees and radians.
        /// </summary>
        public const double DegToRad = Math.PI/180.0;

        /// <summary>
        /// Constant to transform an angle between degrees and radians.
        /// </summary>
        public const double RadToDeg = 180.0/Math.PI;

        /// <summary>
        /// Constant to transform an angle between degrees and gradians.
        /// </summary>
        public const double DegToGrad = 10.0/9.0;

        /// <summary>
        /// Constant to transform an angle between degrees and gradians.
        /// </summary>
        public const double GradToDeg = 9.0/10.0;

        /// <summary>
        /// PI/2 (90 degrees)
        /// </summary>
        public const double HalfPI = Math.PI*0.5;

        /// <summary>
        /// PI (180 degrees)
        /// </summary>
        public const double PI = Math.PI;

        /// <summary>
        /// 3*PI/2 (270 degrees)
        /// </summary>
        public const double ThreeHalfPI = 3*Math.PI*0.5;

        /// <summary>
        /// 2*PI (360 degrees)
        /// </summary>
        public const double TwoPI = 2*Math.PI;

        #endregion

        #region public properties

        private static double epsilon = 1e-12;

        /// <summary>
        /// Represents the smallest number used for comparison purposes.
        /// </summary>
        /// <remarks>
        /// The epsilon value must be a positive number greater than zero.
        /// </remarks>
        public static double Epsilon
        {
            get { return epsilon; }
            set
            {
                if (value <= 0.0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The epsilon value must be a positive number greater than zero.");
                }
                epsilon = value;
            }
        }

        #endregion

        #region static methods

        /// <summary>
        /// Returns a value indicating the sign of a double-precision floating-point number.
        /// </summary>
        /// <param name="number">Double precision number.
        /// </param>
        /// <returns>
        /// A number that indicates the sign of value.
        /// Return value, meaning:<br />
        /// -1 value is less than zero.<br />
        /// 0 value is equal to zero.<br />
        /// 1 value is greater than zero.
        /// </returns>
        /// <remarks>This method will test for values of numbers very close to zero.</remarks>
        public static int Sign(double number)
        {
            return IsZero(number) ? 0 : Math.Sign(number);
        }

        /// <summary>
        /// Returns a value indicating the sign of a double-precision floating-point number.
        /// </summary>
        /// <param name="number">Double precision number.</param>
        /// <param name="threshold">Tolerance.</param>
        /// <returns>
        /// A number that indicates the sign of value.
        /// Return value, meaning:<br />
        /// -1 value is less than zero.<br />
        /// 0 value is equal to zero.<br />
        /// 1 value is greater than zero.
        /// </returns>
        /// <remarks>This method will test for values of numbers very close to zero.</remarks>
        public static int Sign(double number, double threshold)
        {
            return IsZero(number, threshold) ? 0 : Math.Sign(number);
        }

        /// <summary>
        /// Checks if a number is close to one.
        /// </summary>
        /// <param name="number">Double precision number.</param>
        /// <returns>True if its close to one or false in any other case.</returns>
        public static bool IsOne(double number)
        {
            return IsOne(number, Epsilon);
        }

        /// <summary>
        /// Checks if a number is close to one.
        /// </summary>
        /// <param name="number">Double precision number.</param>
        /// <param name="threshold">Tolerance.</param>
        /// <returns>True if its close to one or false in any other case.</returns>
        public static bool IsOne(double number, double threshold)
        {
            return IsZero(number - 1, threshold);
        }

        /// <summary>
        /// Checks if a number is close to zero.
        /// </summary>
        /// <param name="number">Double precision number.</param>
        /// <returns>True if its close to one or false in any other case.</returns>
        public static bool IsZero(double number)
        {
            return IsZero(number, Epsilon);
        }

        /// <summary>
        /// Checks if a number is close to zero.
        /// </summary>
        /// <param name="number">Double precision number.</param>
        /// <param name="threshold">Tolerance.</param>
        /// <returns>True if its close to one or false in any other case.</returns>
        public static bool IsZero(double number, double threshold)
        {
            return number >= -threshold && number <= threshold;
        }

        /// <summary>
        /// Checks if a number is equal to another.
        /// </summary>
        /// <param name="a">Double precision number.</param>
        /// <param name="b">Double precision number.</param>
        /// <returns>True if its close to one or false in any other case.</returns>
        public static bool IsEqual(double a, double b)
        {
            return IsEqual(a, b, Epsilon);
        }

        /// <summary>
        /// Checks if a number is equal to another.
        /// </summary>
        /// <param name="a">Double precision number.</param>
        /// <param name="b">Double precision number.</param>
        /// <param name="threshold">Tolerance.</param>
        /// <returns>True if its close to one or false in any other case.</returns>
        public static bool IsEqual(double a, double b, double threshold)
        {
            return IsZero(a - b, threshold);
        }

        /// <summary>
        /// Transforms a point between coordinate systems.
        /// </summary>
        /// <param name="point">Point to transform.</param>
        /// <param name="rotation">Rotation angle in radians.</param>
        /// <param name="from">Point coordinate system.</param>
        /// <param name="to">Coordinate system of the transformed point.</param>
        /// <returns>Transformed point.</returns>
        public static Vector2 Transform(Vector2 point, double rotation, CoordinateSystem from, CoordinateSystem to)
        {
            // if the rotation is 0 no transformation is needed the transformation matrix is the identity
            if (IsZero(rotation))
            {
                return point;
            }

            double sin = Math.Sin(rotation);
            double cos = Math.Cos(rotation);
            switch (from)
            {
                case CoordinateSystem.World when to == CoordinateSystem.Object:
                    return new Vector2(point.X*cos + point.Y*sin, -point.X*sin + point.Y*cos);
                case CoordinateSystem.Object when to == CoordinateSystem.World:
                    return new Vector2(point.X*cos - point.Y*sin, point.X*sin + point.Y*cos);
                default:
                    return point;
            }
        }

        /// <summary>
        /// Transforms a point list between coordinate systems.
        /// </summary>
        /// <param name="points">Point list to transform.</param>
        /// <param name="rotation">Rotation angle in radians.</param>
        /// <param name="from">Point coordinate system.</param>
        /// <param name="to">Coordinate system of the transformed point.</param>
        /// <returns>Transformed point list.</returns>
        public static List<Vector2> Transform(IEnumerable<Vector2> points, double rotation, CoordinateSystem from, CoordinateSystem to)
        {
            if (points == null)
            {
                throw new ArgumentNullException(nameof(points));
            }

            // if the rotation is 0 no transformation is needed the transformation matrix is the identity
            if (IsZero(rotation))
            {
                return new List<Vector2>(points);
            }

            double sin = Math.Sin(rotation);
            double cos = Math.Cos(rotation);

            List<Vector2> transPoints;
            switch (from)
            {
                case CoordinateSystem.World when to == CoordinateSystem.Object:
                {
                    transPoints = new List<Vector2>();
                    foreach (Vector2 p in points)
                    {
                        transPoints.Add(new Vector2(p.X * cos + p.Y * sin, -p.X * sin + p.Y * cos));
                    }
                    return transPoints;
                }
                case CoordinateSystem.Object when to == CoordinateSystem.World:
                {
                    transPoints = new List<Vector2>();
                    foreach (Vector2 p in points)
                    {
                        transPoints.Add(new Vector2(p.X * cos - p.Y * sin, p.X * sin + p.Y * cos));
                    }
                    return transPoints;
                }
                default:
                    return new List<Vector2>(points);
            }
        }

        /// <summary>
        /// Transforms a point between coordinate systems.
        /// </summary>
        /// <param name="point">Point to transform.</param>
        /// <param name="zAxis">Object normal vector.</param>
        /// <param name="from">Point coordinate system.</param>
        /// <param name="to">Coordinate system of the transformed point.</param>
        /// <returns>Transformed point.</returns>
        public static Vector3 Transform(Vector3 point, Vector3 zAxis, CoordinateSystem from, CoordinateSystem to)
        {
            Matrix3 trans = ArbitraryAxis(zAxis);
            switch (from)
            {
                case CoordinateSystem.World when to == CoordinateSystem.Object:
                    trans = trans.Transpose();
                    return trans * point;
                case CoordinateSystem.Object when to == CoordinateSystem.World:
                    return trans * point;
                default:
                    return point;
            }
        }

        /// <summary>
        /// Transforms a point list between coordinate systems.
        /// </summary>
        /// <param name="points">Points to transform.</param>
        /// <param name="zAxis">Object normal vector.</param>
        /// <param name="from">Points coordinate system.</param>
        /// <param name="to">Coordinate system of the transformed points.</param>
        /// <returns>Transformed point list.</returns>
        public static List<Vector3> Transform(IEnumerable<Vector3> points, Vector3 zAxis, CoordinateSystem from, CoordinateSystem to)
        {
            if (points == null)
            {
                throw new ArgumentNullException(nameof(points));
            }

            Matrix3 trans = ArbitraryAxis(zAxis);
            List<Vector3> transPoints;
            switch (from)
            {
                case CoordinateSystem.World when to == CoordinateSystem.Object:
                {
                    transPoints = new List<Vector3>();
                    trans = trans.Transpose();
                    foreach (Vector3 p in points)
                    {
                        transPoints.Add(trans * p);
                    }
                    return transPoints;
                }
                case CoordinateSystem.Object when to == CoordinateSystem.World:
                {
                    transPoints = new List<Vector3>();
                    foreach (Vector3 p in points)
                    {
                        transPoints.Add(trans * p);
                    }
                    return transPoints;
                }
                default:
                    return new List<Vector3>(points);
            }
        }

        /// <summary>
        /// Transform a 2d point from object coordinates to world coordinates.
        /// </summary>
        /// <param name="point">Points to transform.</param>
        /// <param name="zAxis">Object normal vector.</param>
        /// <param name="elevation">Object elevation.</param>
        /// <returns>Transformed point.</returns>
        public static Vector3 Transform(Vector2 point, Vector3 zAxis, double elevation)
        {
            Matrix3 trans = ArbitraryAxis(zAxis);
            return trans * new Vector3(point.X, point.Y, elevation);
        }

        /// <summary>
        /// Transform a 2d point list from object coordinates to world coordinates.
        /// </summary>
        /// <param name="points">Point to transform.</param>
        /// <param name="zAxis">Object normal vector.</param>
        /// <param name="elevation">Object elevation.</param>
        /// <returns>Transformed points.</returns>
        public static List<Vector3> Transform(IEnumerable<Vector2> points, Vector3 zAxis, double elevation)
        {
            if (points == null)
            {
                throw new ArgumentNullException(nameof(points));
            }

            List<Vector3> transPoints = new List<Vector3>();
            Matrix3 trans = ArbitraryAxis(zAxis);
            foreach (Vector2 p in points)
            {
                transPoints.Add(trans * new Vector3(p.X, p.Y, elevation));
            }
            return transPoints;
        }

        /// <summary>
        /// Transform a 3d point from world coordinates to object coordinates.
        /// </summary>
        /// <param name="point">Point to transform.</param>
        /// <param name="zAxis">Object normal vector.</param>
        /// <param name="elevation">Z axis value of the transformed point.</param>
        /// <returns>Transformed point.</returns>
        public static Vector2 Transform(Vector3 point, Vector3 zAxis, out double elevation)
        {
            Matrix3 trans = ArbitraryAxis(zAxis).Transpose();
            Vector3 p = trans * point;
            elevation = p.Z;
            return new Vector2(p.X, p.Y);
        }

        /// <summary>
        /// Transform a 3d point list from world coordinates to object coordinates.
        /// </summary>
        /// <param name="points">Points to transform.</param>
        /// <param name="zAxis">Object normal vector.</param>
        /// <param name="elevation">Average Z axis value of the transformed points.</param>
        /// <returns>Transformed points.</returns>
        public static List<Vector2> Transform(IEnumerable<Vector3> points, Vector3 zAxis, out double elevation)
        {
            if (points == null)
            {
                throw new ArgumentNullException(nameof(points));
            }

            List<Vector2> transPoints = new List<Vector2>();
            Matrix3 trans = ArbitraryAxis(zAxis).Transpose();
            elevation = 0.0;
            foreach (Vector3 point in points)
            {
                Vector3 p = trans * point;
                elevation += p.Z;
                transPoints.Add(new Vector2(p.X, p.Y));
            }

            elevation /= transPoints.Count;
            return transPoints;
        }

        /// <summary>
        /// Gets the rotation matrix from the normal vector (extrusion direction) of an entity.
        /// </summary>
        /// <param name="zAxis">Normal vector.</param>
        /// <returns>Rotation matrix.</returns>
        public static Matrix3 ArbitraryAxis(Vector3 zAxis)
        {
            zAxis.Normalize();

            if (zAxis.Equals(Vector3.UnitZ))
            {
                return Matrix3.Identity;
            }

            Vector3 wY = Vector3.UnitY;
            Vector3 wZ = Vector3.UnitZ;
            Vector3 aX;

            if ((Math.Abs(zAxis.X) < 1 / 64.0) && (Math.Abs(zAxis.Y) < 1 / 64.0))
            {
                aX = Vector3.CrossProduct(wY, zAxis);
            }
            else
            {
                aX = Vector3.CrossProduct(wZ, zAxis);
            }

            aX.Normalize();

            Vector3 aY = Vector3.CrossProduct(zAxis, aX);
            aY.Normalize();

            return new Matrix3(aX.X, aY.X, zAxis.X, aX.Y, aY.Y, zAxis.Y, aX.Z, aY.Z, zAxis.Z);
        }

        /// <summary>
        /// Calculates the minimum distance between a point and a line.
        /// </summary>
        /// <param name="p">A point.</param>
        /// <param name="origin">Line origin point.</param>
        /// <param name="dir">Line direction.</param>
        /// <returns>The minimum distance between the point and the line.</returns>
        public static double PointLineDistance(Vector3 p, Vector3 origin, Vector3 dir)
        {
            double t = Vector3.DotProduct(dir, p - origin);
            Vector3 pPrime = origin + t * dir;
            Vector3 vec = p - pPrime;
            double distanceSquared = Vector3.DotProduct(vec, vec);
            return Math.Sqrt(distanceSquared);
        }

        /// <summary>
        /// Calculates the minimum distance between a point and a line.
        /// </summary>
        /// <param name="p">A point.</param>
        /// <param name="origin">Line origin point.</param>
        /// <param name="dir">Line direction.</param>
        /// <returns>The minimum distance between the point and the line.</returns>
        public static double PointLineDistance(Vector2 p, Vector2 origin, Vector2 dir)
        {
            double t = Vector2.DotProduct(dir, p - origin);
            Vector2 pPrime = origin + t * dir;
            Vector2 vec = p - pPrime;
            double distanceSquared = Vector2.DotProduct(vec, vec);
            return Math.Sqrt(distanceSquared);
        }

        /// <summary>
        /// Checks if a point is inside a line segment.
        /// </summary>
        /// <param name="p">A point.</param>
        /// <param name="start">Segment start point.</param>
        /// <param name="end">Segment end point.</param>
        /// <returns>Zero if the point is inside the segment, 1 if the point is after the end point, and -1 if the point is before the start point.</returns>
        /// <remarks>
        /// For testing purposes a point is considered inside a segment,
        /// if it falls inside the volume from start to end of the segment that extends infinitely perpendicularly to its direction.
        /// Later, if needed, you can use the PointLineDistance method, if the distance is zero the point is along the line defined by the start and end points.
        /// </remarks>
        public static int PointInSegment(Vector3 p, Vector3 start, Vector3 end)
        {
            Vector3 dir = end - start;
            Vector3 pPrime = p - start;
            double t = Vector3.DotProduct(dir, pPrime);
            if (t < 0)
            {
                return -1;
            }
            double dot = Vector3.DotProduct(dir, dir);
            if (t > dot)
            {
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// Checks if a point is inside a line segment.
        /// </summary>
        /// <param name="p">A point.</param>
        /// <param name="start">Segment start point.</param>
        /// <param name="end">Segment end point.</param>
        /// <returns>Zero if the point is inside the segment, 1 if the point is after the end point, and -1 if the point is before the start point.</returns>
        /// <remarks>
        /// For testing purposes a point is considered inside a segment,
        /// if it falls inside the area from start to end of the segment that extends infinitely perpendicularly to its direction.
        /// Later, if needed, you can use the PointLineDistance method, if the distance is zero the point is along the line defined by the start and end points.
        /// </remarks>
        public static int PointInSegment(Vector2 p, Vector2 start, Vector2 end)
        {
            Vector2 dir = end - start;
            Vector2 pPrime = p - start;
            double t = Vector2.DotProduct(dir, pPrime);
            if (t < 0)
            {
                return -1;
            }
            double dot = Vector2.DotProduct(dir, dir);
            if (t > dot)
            {
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// Calculates the intersection point of two lines.
        /// </summary>
        /// <param name="point0">First line origin point.</param>
        /// <param name="dir0">First line direction.</param>
        /// <param name="point1">Second line origin point.</param>
        /// <param name="dir1">Second line direction.</param>
        /// <returns>The intersection point between the two lines.</returns>
        /// <remarks>If the lines are parallel the method will return a <see cref="Vector2.NaN">Vector2.NaN</see>.</remarks>
        public static Vector2 FindIntersection(Vector2 point0, Vector2 dir0, Vector2 point1, Vector2 dir1)
        {
            return FindIntersection(point0, dir0, point1, dir1, Epsilon);
        }

        /// <summary>
        /// Calculates the intersection point of two lines.
        /// </summary>
        /// <param name="point0">First line origin point.</param>
        /// <param name="dir0">First line direction.</param>
        /// <param name="point1">Second line origin point.</param>
        /// <param name="dir1">Second line direction.</param>
        /// <param name="threshold">Tolerance.</param>
        /// <returns>The intersection point between the two lines.</returns>
        /// <remarks>If the lines are parallel the method will return a <see cref="Vector2.NaN">Vector2.NaN</see>.</remarks>
        public static Vector2 FindIntersection(Vector2 point0, Vector2 dir0, Vector2 point1, Vector2 dir1, double threshold)
        {
            // test for parallelism.
            if (Vector2.AreParallel(dir0, dir1, threshold))
            {
                return Vector2.NaN;
            }

            // lines are not parallel
            Vector2 v = point1 - point0;
            double cross = Vector2.CrossProduct(dir0, dir1);
            double s = (v.X * dir1.Y - v.Y * dir1.X) / cross;
            return point0 + s * dir0;
        }

        /// <summary>
        /// Normalizes the value of an angle in degrees between [0, 360[.
        /// </summary>
        /// <param name="angle">Angle in degrees.</param>
        /// <returns>The equivalent angle in the range [0, 360[.</returns>
        /// <remarks>Negative angles will be converted to its positive equivalent.</remarks>
        public static double NormalizeAngle(double angle)
        {
            double normalized = angle % 360.0;
            if (IsZero(normalized) || IsEqual(Math.Abs(normalized), 360.0))
            {
                return 0.0;
            }

            if (normalized < 0)
            {
                return 360.0 + normalized;
            }

            return normalized;
        }

        /// <summary>
        /// Round off a numeric value to the nearest of another value.
        /// </summary>
        /// <param name="number">Number to round off.</param>
        /// <param name="roundTo">The number will be rounded to the nearest of this value.</param>
        /// <returns>The number rounded to the nearest value.</returns>
        public static double RoundToNearest(double number, double roundTo)
        {
            double multiplier = Math.Round(number/roundTo, 0);
            return multiplier * roundTo;
        }
        
        /// <summary>
        /// Obtains the data for an arc that has a start point, an end point, and a bulge value.
        /// </summary>
        /// <param name="startPoint">Arc start point.</param>
        /// <param name="endPoint">Arc end point.</param>
        /// <param name="bulge">Arc bulge value.</param>
        /// <returns>A Tuple(center, radius, startAngle in degrees, endAngle in degrees) with the arc data.</returns>
        public static Tuple<Vector2, double, double, double> ArcFromBulge(Vector2 startPoint, Vector2 endPoint, double bulge)
        {
            if (IsZero(bulge))
            {
                throw new ArgumentOutOfRangeException(nameof(bulge), bulge, "The bulge value must be different than zero to make an arc.");
            }
            double theta = 4 * Math.Atan(Math.Abs(bulge));
            double dist = 0.5 * Vector2.Distance(startPoint, endPoint);
            double gamma = 0.5 * (Math.PI - theta);
            double phi = Vector2.Angle(startPoint, endPoint) + Math.Sign(bulge) * gamma;

            double radius = dist / Math.Sin(0.5 * theta);
            Vector2 center = new Vector2(startPoint.X + radius * Math.Cos(phi), startPoint.Y + radius * Math.Sin(phi));
            double startAngle;
            double endAngle;
            if (bulge > 0)
            {
                startAngle = NormalizeAngle(Vector2.Angle(startPoint - center) * RadToDeg);
                endAngle = NormalizeAngle(startAngle + theta * RadToDeg);
            }
            else
            {
                endAngle = NormalizeAngle(Vector2.Angle(startPoint - center) * RadToDeg);
                startAngle = NormalizeAngle(endAngle - theta * RadToDeg);
            }

            return new Tuple<Vector2, double, double, double>(center, radius, startAngle, endAngle);
        }

        /// <summary>
        /// Obtains the start point, end point, and bulge value from an arc.
        /// </summary>
        /// <param name="center">Arc center.</param>
        /// <param name="radius">Arc radius.</param>
        /// <param name="startAngle">Arc start angle in degrees.</param>
        /// <param name="endAngle">Arc end angle in degrees.</param>
        /// <returns>A Tuple(start point, end point, bulge value) for the specified arc data.</returns>
        public static Tuple<Vector2, Vector2, double> ArcToBulge(Vector2 center, double radius, double startAngle, double endAngle)
        {
            if (radius <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(radius), radius, "The arc radius larger than zero.");
            }

            double arcAngle = (endAngle - startAngle) * DegToRad;
            double bulge = Math.Tan(arcAngle / 4);
            Vector2 startPoint = Vector2.Polar(center, radius, startAngle * DegToRad);
            Vector2 endPoint = Vector2.Polar(center, radius, endAngle * DegToRad);

            return new Tuple<Vector2, Vector2, double>(startPoint, endPoint, bulge);
        }

        #endregion
    }
}