#region netDxf, Copyright(C) 2014 Daniel Carvajal, Licensed under LGPL.

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

namespace netDxf
{
    /// <summary>
    /// Utility math functions and constants.
    /// </summary>
    public class MathHelper
    {
        #region CoordinateSystem enum

        /// <summary>
        /// Defines the coordinate system reference.
        /// </summary>
        public enum CoordinateSystem
        {
            /// <summary>
            /// World coordinates.
            /// </summary>
            World,

            /// <summary>
            /// Object coordinates.
            /// </summary>
            Object
        }

        #endregion

        #region constants

        private static readonly double[,] conversionFactor = new[,]
                                                                 {
                                                                     {
                                                                         1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
                                                                     },
                                                                     {
                                                                         1, 1, 0.0833333333333333, 1.57828282828283E-05, 25.4, 2.54, 0.0254, 2.54E-05, 1000000, 1000, 0.0277777777777778, 254000000, 25400000, 25400, 0.254, 0.00254, 0.000254, 2.54E-11, 1.697885129158E-13, 8.231579395684E-19
                                                                     },
                                                                     {
                                                                         1, 12, 1, 0.000189393939393939, 304.8, 30.48, 0.3048, 0.0003048, 12000000, 12000, 0.333333333333333, 3048000000, 304800000, 304800, 3.048, 0.03048, 0.003048, 3.048E-10, 2.0374621549896E-12, 9.8778952748208E-18
                                                                     },
                                                                     {
                                                                         1, 63360, 5280, 1, 1609344, 160934.4, 1609.344, 1.609344, 63360000000, 63360000, 1760, 16093440000000, 1609344000000, 1609344000, 16093.44, 160.9344, 16.09344, 1.609344E-06, 1.07578001783451E-08, 5.21552870510538E-14
                                                                     },
                                                                     {
                                                                         1, 0.0393700787401575, 0.00328083989501312, 6.21371192237334E-07, 1, 0.1, 0.001, 1E-06, 39370.0787401575, 39.3700787401575, 0.00109361329833771, 10000000, 1000000, 1000, 0.01, 0.0001, 1E-05, 1E-12, 6.68458712266929E-15, 3.24077928963937E-20
                                                                     },
                                                                     {
                                                                         1, 0.393700787401575, 0.0328083989501312, 6.21371192237334E-06, 10, 1, 0.01, 1E-05, 393700.787401575, 393.700787401575, 0.0109361329833771, 100000000, 10000000, 10000, 0.1, 0.001, 0.0001, 1E-11, 6.68458712266929E-14, 3.24077928963937E-19
                                                                     },
                                                                     {
                                                                         1, 39.3700787401575, 3.28083989501312, 0.000621371192237334, 1000, 100, 1, 0.001, 39370078.7401575, 39370.0787401575, 1.09361329833771, 10000000000, 1000000000, 1000000, 10, 0.1, 0.01, 1E-09, 6.68458712266929E-12, 3.24077928963937E-17
                                                                     },
                                                                     {
                                                                         1, 39370.0787401575, 3280.83989501312, 0.621371192237334, 1000000, 100000, 1000, 1, 39370078740.1575, 39370078.7401575, 1093.61329833771, 10000000000000, 1000000000000, 1000000000, 10000, 100, 10, 1E-06, 6.68458712266929E-09, 3.24077928963937E-14
                                                                     },
                                                                     {
                                                                         1, 1E-06, 8.33333333333333E-08, 1.57828282828283E-11, 2.54E-05, 2.54E-06, 2.54E-08, 2.54E-11, 1, 0.001, 2.77777777777778E-08, 254, 25.4, 0.0254, 2.54E-07, 2.54E-09, 2.54E-10, 2.54E-17, 1.697885129158E-19, 8.231579395684E-25
                                                                     },
                                                                     {
                                                                         1, 0.001, 8.33333333333333E-05, 1.57828282828283E-08, 0.0254, 0.00254, 2.54E-05, 2.54E-08, 1000, 1, 2.77777777777778E-05, 254000, 25400, 25.4, 0.000254, 2.54E-06, 2.54E-07, 2.54E-14, 1.697885129158E-16, 8.231579395684E-22
                                                                     },
                                                                     {
                                                                         1, 36, 3, 0.000568181818181818, 914.4, 91.44, 0.9144, 0.0009144, 36000000, 36000, 1, 9144000000, 914400000, 914400, 9.144, 0.09144, 0.009144, 9.144E-10, 6.1123864649688E-12, 2.96336858244624E-17
                                                                     },
                                                                     {
                                                                         1, 3.93700787401575E-09, 3.28083989501312E-10, 6.21371192237334E-14, 1E-07, 1E-08, 1E-10, 1E-13, 0.00393700787401575, 3.93700787401575E-06, 1.09361329833771E-10, 1, 0.1, 0.0001, 1E-09, 1E-11, 1E-12, 1E-19, 6.68458712266929E-22, 3.24077928963937E-27
                                                                     },
                                                                     {
                                                                         1, 3.93700787401575E-08, 3.28083989501312E-09, 6.21371192237334E-13, 1E-06, 1E-07, 1E-09, 1E-12, 0.0393700787401575, 3.93700787401575E-05, 1.09361329833771E-09, 10, 1, 0.001, 1E-08, 1E-10, 1E-11, 1E-18, 6.68458712266929E-21, 3.24077928963937E-26
                                                                     },
                                                                     {
                                                                         1, 3.93700787401575E-05, 3.28083989501312E-06, 6.21371192237334E-10, 0.001, 0.0001, 1E-06, 1E-09, 39.3700787401575, 0.0393700787401575, 1.09361329833771E-06, 10000, 1000, 1, 1E-05, 1E-07, 1E-08, 1E-15, 6.68458712266929E-18, 3.24077928963937E-23
                                                                     },
                                                                     {
                                                                         1, 3.93700787401575, 0.328083989501312, 6.21371192237334E-05, 100, 10, 0.1, 0.0001, 3937007.87401575, 3937.00787401575, 0.109361329833771, 1000000000, 100000000, 100000, 1, 0.01, 0.001, 1E-10, 6.68458712266929E-13, 3.24077928963937E-18
                                                                     },
                                                                     {
                                                                         1, 393.700787401575, 32.8083989501312, 0.00621371192237334, 10000, 1000, 10, 0.01, 393700787.401575, 393700.787401575, 10.9361329833771, 100000000000, 10000000000, 10000000, 100, 1, 0.1, 1E-08, 6.68458712266929E-11, 3.24077928963937E-16
                                                                     },
                                                                     {
                                                                         1, 3937.00787401575, 328.083989501312, 0.0621371192237334, 100000, 10000, 100, 0.1, 3937007874.01575, 3937007.87401575, 109.361329833771, 1000000000000, 100000000000, 100000000, 1000, 10, 1, 1E-07, 6.68458712266929E-10, 3.24077928963937E-15
                                                                     },
                                                                     {
                                                                         1, 39370078740.1575, 3280839895.01312, 621371.192237334, 1000000000000, 100000000000, 1000000000, 1000000, 3.93700787401575E+16, 39370078740157.5, 1093613298.33771, 1E+19, 1E+18, 1E+15, 10000000000, 100000000, 10000000, 1, 0.00668458712266929, 3.24077928963937E-08
                                                                     },
                                                                     {
                                                                         1, 5889679948465.72, 490806662372.143, 92955807.2674514, 149597870691029, 14959787069102.9, 149597870691.029, 149597870.691029, 5.88967994846572E+18, 5.88967994846572E+15, 163602220790.714, 1.49597870691029E+21, 1.49597870691029E+20, 1.49597870691029E+17, 1495978706910.29, 14959787069.1029, 1495978706.91029, 149.597870691029, 1, 4.84813681109636E-06
                                                                     },
                                                                     {
                                                                         1, 1.21483369342744E+18, 1.01236141118953E+17, 19173511575559.3, 3.08567758130569E+19, 3.08567758130569E+18, 3.08567758130569E+16, 30856775813056.9, 1.21483369342744E+24, 1.21483369342744E+21, 3.37453803729844E+16, 3.08567758130569E+26, 3.0856775813057E+25, 3.0856775813057E+22, 3.08567758130569E+17, 3.0856775813057E+15, 308567758130570, 30856775.813057, 206264.806247054, 1
                                                                     }
                                                                 };

        /// <summary>
        /// Represents the smallest number.
        /// </summary>
        public const double Epsilon = 0.000000000001;

        /// <summary>
        /// Constant to transform an angle between degrees and radians.
        /// </summary>
        public const double DegToRad = Math.PI/180.0;

        /// <summary>
        /// Constant to transform an angle between degrees and radians.
        /// </summary>
        public const double RadToDeg = 180.0/Math.PI;

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

        #region static methods

        /// <summary>
        /// Converts a value from one drawing unit to another.
        /// </summary>
        /// <param name="value">Number to convert.</param>
        /// <param name="from">Original drawing units.</param>
        /// <param name="to">Destination drawing units.</param>
        /// <returns>The converted value to the new drawing units.</returns>
        public static double ConvertUnit(double value, DrawingUnits from, DrawingUnits to)
        {
            return value*ConversionFactor(from, to);
        }

        /// <summary>
        /// Gets the conversion factor between drawing units.
        /// </summary>
        /// <param name="from">Original drawing units.</param>
        /// <param name="to">Destination drawing units.</param>
        /// <returns>The conversion factor between the drawing units.</returns>
        public static double ConversionFactor(DrawingUnits from, DrawingUnits to)
        {
            return conversionFactor[(int) from, (int) to];
        }

        /// <summary>
        /// Gets the conversion factor between image and drawing units.
        /// </summary>
        /// <param name="from">Original image units.</param>
        /// <param name="to">Destination drawing units.</param>
        /// <returns>The conversion factor between the units.</returns>
        public static double ConversionFactor(ImageUnits from, DrawingUnits to)
        {
            // more on the dxf format none sense, they don't even use the same integers for the drawing and image units
            int rasterUnits = 0;
            switch (from)
            {
                case ImageUnits.None:
                    rasterUnits = 0;
                    break;
                case ImageUnits.Millimeters:
                    rasterUnits = 4;
                    break;
                case ImageUnits.Centimeters:
                    rasterUnits = 5;
                    break;
                case ImageUnits.Meters:
                    rasterUnits = 6;
                    break;
                case ImageUnits.Kilometers:
                    rasterUnits = 7;
                    break;
                case ImageUnits.Inches:
                    rasterUnits = 1;
                    break;
                case ImageUnits.Feet:
                    rasterUnits = 1;
                    break;
                case ImageUnits.Yards:
                    rasterUnits = 10;
                    break;
                case ImageUnits.Miles:
                    rasterUnits = 3;
                    break;
            }
            return conversionFactor[rasterUnits, (int)to];
        }

        /// <summary>
        /// Gets the conversion factor between units.
        /// </summary>
        /// <param name="from">Original value units.</param>
        /// <param name="to">Destination value units.</param>
        /// <returns>The conversion factor between the passed units.</returns>
        public static double ConversionFactor(DrawingUnits from, ImageUnits to)
        {
            return 1 / ConversionFactor(to, from);
        }

        /// <summary>
        /// Checks if a number is close to one.
        /// </summary>
        /// <param name="number">Double precision number.</param>
        /// <param name="threshold">Tolerance.</param>
        /// <returns>True if its close to one or false in anyother case.</returns>
        public static bool IsOne(double number, double threshold = Epsilon)
        {
            return IsZero(number - 1, threshold);
        }

        /// <summary>
        /// Checks if a number is close to zero.
        /// </summary>
        /// <param name="number">Double precision number.</param>
        /// <param name="threshold">Tolerance.</param>
        /// <returns>True if its close to one or false in anyother case.</returns>
        public static bool IsZero(double number, double threshold = Epsilon)
        {
            return number >= -threshold && number <= threshold;
        }

        /// <summary>
        /// Checks if a number is equal to another.
        /// </summary>
        /// <param name="a">Double precision number.</param>
        /// <param name="b">Double precision number.</param>
        /// <param name="threshold">Tolerance.</param>
        /// <returns>True if its close to one or false in anyother case.</returns>
        public static bool IsEqual(double a, double b, double threshold = Epsilon)
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
        /// <returns>Transormed point.</returns>
        public static Vector2 Transform(Vector2 point, double rotation, CoordinateSystem from, CoordinateSystem to)
        {
            // if the rotation is 0 no transformation is needed the transformation matrix is the identity
            if (IsZero(rotation)) return point;

            double sin = Math.Sin(rotation);
            double cos = Math.Cos(rotation);
            if (from == CoordinateSystem.World && to == CoordinateSystem.Object)
            {
                return new Vector2(point.X*cos + point.Y*sin, -point.X*sin + point.Y*cos);
            }
            if (from == CoordinateSystem.Object && to == CoordinateSystem.World)
            {
                return new Vector2(point.X*cos - point.Y*sin, point.X*sin + point.Y*cos);
            }
            return point;
        }

        /// <summary>
        /// Transforms a point between coordinate systems.
        /// </summary>
        /// <param name="point">Point to transform.</param>
        /// <param name="zAxis">Object normal vector.</param>
        /// <param name="from">Point coordinate system.</param>
        /// <param name="to">Coordinate system of the transformed point.</param>
        /// <returns>Transormed point.</returns>
        public static Vector3 Transform(Vector3 point, Vector3 zAxis, CoordinateSystem from, CoordinateSystem to)
        {
            // if the normal is (0,0,1) no transformation is needed the transformation matrix is the identity
            if (zAxis.Equals(Vector3.UnitZ)) return point;

            Matrix3 trans = ArbitraryAxis(zAxis);
            if (from == CoordinateSystem.World && to == CoordinateSystem.Object)
            {
                trans = trans.Traspose();
                return trans*point;
            }
            if (from == CoordinateSystem.Object && to == CoordinateSystem.World)
            {
                return trans*point;
            }
            return point;
        }

        /// <summary>
        /// Transforms a point list between coordinate systems.
        /// </summary>
        /// <param name="points">Points to transform.</param>
        /// <param name="zAxis">Object normal vector.</param>
        /// <param name="from">Points coordinate system.</param>
        /// <param name="to">Coordinate system of the transformed points.</param>
        /// <returns>Transormed point list.</returns>
        public static List<Vector3> Transform(List<Vector3> points, Vector3 zAxis, CoordinateSystem from, CoordinateSystem to)
        {
            if (zAxis.Equals(Vector3.UnitZ)) return points;

            Matrix3 trans = ArbitraryAxis(zAxis);
            List<Vector3> transPoints;
            if (from == CoordinateSystem.World && to == CoordinateSystem.Object)
            {
                transPoints = new List<Vector3>();
                trans = trans.Traspose();
                foreach (Vector3 p in points)
                {
                    transPoints.Add(trans*p);
                }
                return transPoints;
            }
            if (from == CoordinateSystem.Object && to == CoordinateSystem.World)
            {
                transPoints = new List<Vector3>();
                foreach (Vector3 p in points)
                {
                    transPoints.Add(trans*p);
                }
                return transPoints;
            }
            return points;
        }

        /// <summary>
        /// Gets the rotation matrix from the normal vector (extrusion direction) of an entity.
        /// </summary>
        /// <param name="zAxis">Normal vector.</param>
        /// <returns>Rotation matriz.</returns>
        public static Matrix3 ArbitraryAxis(Vector3 zAxis)
        {
            zAxis.Normalize();
            Vector3 wY = Vector3.UnitY;
            Vector3 wZ = Vector3.UnitZ;
            Vector3 aX;

            if ((Math.Abs(zAxis.X) < 1/64.0) && (Math.Abs(zAxis.Y) < 1/64.0))
                aX = Vector3.CrossProduct(wY, zAxis);
            else
                aX = Vector3.CrossProduct(wZ, zAxis);

            aX.Normalize();

            Vector3 aY = Vector3.CrossProduct(zAxis, aX);
            aY.Normalize();

            return new Matrix3(aX.X, aY.X, zAxis.X, aX.Y, aY.Y, zAxis.Y, aX.Z, aY.Z, zAxis.Z);
        }

        /// <summary>
        /// Calculates the minimum distance between a point and a line.
        /// </summary>
        /// <param name="q">A point.</param>
        /// <param name="origin">Line origin point.</param>
        /// <param name="dir">Line direction.</param>
        /// <returns>The minimum distance between the point and the line.</returns>
        public static double PointLineDistance(Vector3 q, Vector3 origin, Vector3 dir)
        {
            double t = Vector3.DotProduct(dir, q - origin);
            Vector3 qPrime = origin + t*dir;
            Vector3 vec = q - qPrime;
            double distanceSquared = Vector3.DotProduct(vec, vec);
            return Math.Sqrt(distanceSquared);
        }

        public static void OffsetLine(Vector3 start, Vector3 end, Vector3 normal, double offset, out Vector3 newStart, out Vector3 newEnd)
        {
            Vector3 dir = end - start;
            dir.Normalize();
            Vector3 perp = Vector3.CrossProduct(normal, dir);
            newStart = start + perp*offset;
            newEnd = end + perp*offset;
        }

        public static void OffsetLine(Vector2 start, Vector2 end, double offset, out Vector2 newStart, out Vector2 newEnd)
        {
            Vector2 dir = end - start;
            dir.Normalize();
            Vector2 perp = Vector2.Perpendicular(dir);
            newStart = start + perp*offset;
            newEnd = end + perp*offset;
        }

        /// <summary>
        /// Calculates the intersection point of two lines.
        /// </summary>
        /// <param name="point0">First line origin point.</param>
        /// <param name="dir0">First line direction.</param>
        /// <param name="point1">Second line origin point.</param>
        /// <param name="dir1">Second line direction.</param>
        /// <param name="intersection">Intersection point of the two lines.</param>
        /// <param name="threshold">Tolerance.</param>
        /// <returns>0 if there is an intersection point, 1 if the lines are parallel or 2 if the lines are the same.</returns>
        public static int FindIntersection(Vector2 point0, Vector2 dir0, Vector2 point1, Vector2 dir1, out Vector2 intersection, double threshold = Epsilon)
        {
            // Use a relative error test to test for parallelism. This effectively is a threshold on the angle between D0 and D1.
            Vector2 vect = point1 - point0;
            double cross = Vector2.CrossProduct(dir0, dir1);
            double sqrCross = cross*cross;
            double sqrLen0 = dir0.X*dir0.X + dir0.Y*dir0.Y;
            double sqrLen1 = dir1.X*dir1.X + dir1.Y*dir1.Y;
            if (sqrCross > threshold * sqrLen0 * sqrLen1)
            {
                // lines are not parallel
                double s = (vect.X*dir1.Y - vect.Y*dir1.X)/cross;
                intersection = point0 + s*dir0;
                return 0;
            }
            // lines are parallel
            double sqrLenE = vect.X*vect.X + vect.Y*vect.Y;
            cross = vect.X*dir0.Y - vect.Y*dir0.X;
            sqrCross = cross*cross;
            if (sqrCross > threshold * sqrLen0 * sqrLenE)
            {
                // lines are different
                intersection = Vector2.Zero;
                return 1;
            }
            // lines are the same
            intersection = Vector2.Zero;
            return 2;
        }

        /// <summary>
        /// Normalizes the value of an angle in degrees between [0, 360[.
        /// </summary>
        /// <param name="angle">Angle in degrees.</param>
        /// <returns>The equivalent angle in the range [0, 360[.</returns>
        /// <remarks>Negative angles will be converted to its positive equivalent.</remarks>
        public static double NormalizeAngle(double angle)
        {
            double c = angle%360;
            if (c < 0)
                return 360.0 + c;
            return c;
        }

        #endregion
    }
}