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

        /// <summary>
        /// Represents the smallest number.
        /// </summary>
        public const double Epsilon = 0.000000000001;
        /// <summary>
        /// Defines the max number of decimals of an angle. Trigonometric functions are very prone to round off errors.
        /// </summary>
        public const int MaxAngleDecimals = 12;
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
        /// 2*PI (360 degrees)
        /// </summary>
        public const double TwoPI = 2*Math.PI;

        /// <summary>
        /// Checks if a number is close to one.
        /// </summary>
        /// <param name="number">Simple precision number.</param>
        /// <param name="threshold">Tolerance.</param>
        /// <returns>True if its close to one or false in anyother case.</returns>
        public static bool IsOne(float number, float threshold)
        {
            return IsZero(number - 1, threshold);
        }

        /// <summary>
        /// Checks if a number is close to one.
        /// </summary>
        /// <param name="number">Simple precision number.</param>
        /// <returns>True if its close to one or false in anyother case.</returns>
        /// <remarks>By default a tolerance of the constant float.Epsilon will be used.</remarks>
        public static bool IsOne(float number)
        {
            return IsZero(number - 1);
        }

        /// <summary>
        /// Checks if a number is close to one.
        /// </summary>
        /// <param name="number">Double precision number.</param>
        /// <param name="threshold">Tolerance.</param>
        /// <returns>True if its close to one or false in anyother case.</returns>
        public static bool IsOne(double number, double threshold)
        {
            return IsZero(number - 1, threshold);
        }

        /// <summary>
        /// Checks if a number is close to one.
        /// </summary>
        /// <param name="number">Double precision number.</param>
        /// <returns>True if its close to one or false in anyother case.</returns>
        /// <remarks>By default a tolerance of the constant double.Epsilon will be used.</remarks>
        public static bool IsOne(double number)
        {
            return IsZero(number - 1);
        }

        /// <summary>
        /// Checks if a number is close to zero.
        /// </summary>
        /// <param name="number">Simple precision number.</param>
        /// <param name="threshold">Tolerance.</param>
        /// <returns>True if its close to one or false in anyother case.</returns>
        public static bool IsZero(float number, float threshold)
        {
            return (number >= -threshold && number <= threshold);
        }

        /// <summary>
        /// Checks if a number is close to zero.
        /// </summary>
        /// <param name="number">Simple precision number.</param>
        /// <returns>True if its close to one or false in anyother case.</returns>
        /// <remarks>By default a tolerance of the constant float.Epsilon will be used.</remarks>
        public static bool IsZero(float number)
        {
            return IsZero(number, float.Epsilon);
        }

        /// <summary>
        /// Checks if a number is close to zero.
        /// </summary>
        /// <param name="number">Double precision number.</param>
        /// <param name="threshold">Tolerance.</param>
        /// <returns>True if its close to one or false in anyother case.</returns>
        public static bool IsZero(double number, double threshold)
        {
            return number >= -threshold && number <= threshold;
        }

        /// <summary>
        /// Checks if a number is close to zero.
        /// </summary>
        /// <param name="number">Double precision number.</param>
        /// <returns>True if its close to one or false in anyother case.</returns>
        /// <remarks>By default a tolerance of the constant double.Epsilon will be used.</remarks>
        public static bool IsZero(double number)
        {
            return IsZero(number, double.Epsilon);
        }

        /// <summary>
        /// Checks if a number is equal to another.
        /// </summary>
        /// <param name="a">Simple precision number.</param>
        /// <param name="b">Simple precision number.</param>
        /// <param name="threshold">Tolerance.</param>
        /// <returns>True if its close to one or false in anyother case.</returns>
        public static bool IsEqual(float a, float b, float threshold)
        {
            return IsZero(a - b, threshold);
        }

        /// <summary>
        /// Checks if a number is equal to another.
        /// </summary>
        /// <param name="a">Double precision number.</param>
        /// <param name="b">Double precision number.</param>
        /// <returns>True if its close to one or false in anyother case.</returns>
        /// <remarks>By default a tolerance of the constant float.Epsilon will be used.</remarks>
        public static bool IsEqual(float a, float b)
        {
            return IsZero(a - b);
        }

        /// <summary>
        /// Checks if a number is equal to another.
        /// </summary>
        /// <param name="a">Double precision number.</param>
        /// <param name="b">Double precision number.</param>
        /// <param name="threshold">Tolerance.</param>
        /// <returns>True if its close to one or false in anyother case.</returns>
        public static bool IsEqual(double a, double b, double threshold)
        {
            return IsZero(a - b, threshold);
        }

        /// <summary>
        /// Checks if a number is equal to another.
        /// </summary>
        /// <param name="a">Double precision number.</param>
        /// <param name="b">Double precision number.</param>
        /// <returns>True if its close to one or false in anyother case.</returns>
        /// <remarks>By default a tolerance of the constant float.Epsilon will be used.</remarks>
        public static bool IsEqual(double a, double b)
        {
            return IsZero(a - b);
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
        public static IList<Vector3> Transform(IList<Vector3> points, Vector3 zAxis, CoordinateSystem from, CoordinateSystem to)
        {
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
    }
}