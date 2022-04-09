#region netDxf library licensed under the MIT License
// 
//                       netDxf library
// Copyright (c) 2019-2021 Daniel Carvajal (haplokuon@gmail.com)
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
using System.Linq;

namespace netDxf
{
    /// <summary>
    /// Represent a cubic bezier curve.
    /// </summary>
    public class BezierCurveCubic :
        BezierCurve
    {
        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>BezierCurve</c> class.
        /// </summary>
        /// <param name="controlPoints">A list of four control points.</param>
        /// <remarks>
        /// The list must contain four control points.
        /// The first index represents the start point or anchor,
        /// the second represents the first control point,
        /// the third the second control point,
        /// and the last the end point.
        /// </remarks>
        public BezierCurveCubic(IEnumerable<Vector3> controlPoints)
            : base(controlPoints)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>BezierCurve</c> class.
        /// </summary>
        /// <param name="startPoint">Start anchor point.</param>
        /// <param name="firstControlPoint">First control point.</param>
        /// <param name="secondControlPoint">Second control point.</param>
        /// <param name="endPoint">End anchor point.</param>
        public BezierCurveCubic(Vector3 startPoint, Vector3 firstControlPoint, Vector3 secondControlPoint, Vector3 endPoint)
            : base(new[]{startPoint, firstControlPoint, secondControlPoint, endPoint})
        {
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the curve start point.
        /// </summary>
        public Vector3 StartPoint
        {
            get { return this.controlPoints[0]; }
            set { this.controlPoints[0] = value; }
        }

        /// <summary>
        /// Gets or sets the first control point.
        /// </summary>
        public Vector3 FirstControlPoint
        {
            get { return this.controlPoints[1]; }
            set { this.controlPoints[1] = value; }
        }

        /// <summary>
        /// Gets or sets the second control point.
        /// </summary>
        public Vector3 SecondControlPoint
        {
            get { return this.controlPoints[2]; }
            set { this.controlPoints[2] = value; }
        }

        /// <summary>
        /// Gets or sets the curve end point.
        /// </summary>
        public Vector3 EndPoint
        {
            get { return this.controlPoints[3]; }
            set { this.controlPoints[3] = value; }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Obtains a point along the curve at parameter t.
        /// </summary>
        /// <param name="t">Parameter t, between 0.0 and 1.0.</param>
        /// <returns>A point along the curve.</returns>
        public override Vector3 CalculatePoint(double t)
        {
            if (t < 0.0 || t > 1.0)
            {
                throw new ArgumentOutOfRangeException(nameof(t), t, "The parameter t must be between 0.0 and 1.0.");
            }

            double c = 1.0 - t;
            return c * c * c * this.StartPoint + 3 * t * c * c * this.FirstControlPoint + 3 * c * t * t * this.SecondControlPoint + t * t * t * this.EndPoint;
        }

        /// <summary>
        /// Calculates the tangent vector at parameter t.
        /// </summary>
        /// <param name="t">Parameter t, between 0.0 and 1.0.</param>
        /// <returns>A normalized tangent vector.</returns>
        public override Vector3 CalculateTangent(double t)
        {
            if (t < 0.0 || t > 1.0)
            {
                throw new ArgumentOutOfRangeException(nameof(t), t, "The parameter t must be between 0.0 and 1.0.");
            }

            double c = 1.0 - t;
            return Vector3.Normalize(
                -c * c * this.StartPoint + (c * c - 2 * c * t) * this.FirstControlPoint + (2 * c * t - t * t) * this.SecondControlPoint + t * t * this.EndPoint
            );
        }

        /// <summary>
        /// Splits the actual bezier curve in two at parameter t.
        /// </summary>
        /// <param name="t">Parameter t, between 0.0 and 1.0.</param>
        /// <returns>The two curves result of dividing the actual curve at parameter t.</returns>
        public BezierCurveCubic[] Split(double t)
        {
            if (t < 0.0 || t > 1.0)
            {
                throw new ArgumentOutOfRangeException(nameof(t), t, "The parameter t must be between 0.0 and 1.0.");
            }

            Vector3 p12 = (this.FirstControlPoint - this.StartPoint) * t + this.StartPoint;
            Vector3 p23 = (this.SecondControlPoint - this.FirstControlPoint) * t + this.FirstControlPoint;
            Vector3 p123 = (p23 - p12) * t + p12;
            Vector3 p34 = (this.EndPoint - this.SecondControlPoint) * t + this.SecondControlPoint;
            Vector3 p234 = (p34 - p23) * t + p23;
            Vector3 breakPoint = (p234 - p123) * t + p123;

            return new[]
            {
                new BezierCurveCubic(this.StartPoint, p12, p123, breakPoint),
                new BezierCurveCubic(breakPoint, p234, p34, this.EndPoint)
            };
        }

        /// <summary>
        /// Switch the bezier curve direction.
        /// </summary>
        public void Reverse()
        {
            Array.Reverse(this.controlPoints);
        }

        /// <summary>
        /// Converts the bezier curve in a list of vertexes.
        /// </summary>
        /// <param name="precision">Number of vertexes generated.</param>
        /// <returns>A list vertexes that represents the bezier curve.</returns>
        public List<Vector3> PolygonalVertexes(int precision)
        {
            if (precision < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(precision), precision, "The precision must be equal or greater than two.");
            }

            List<Vector3> vertexes = new List<Vector3>();
            double delta = 1.0 / (precision - 1);

            for (int i = 0; i < precision; i++)
            {
                double t = delta * i;
                vertexes.Add(this.CalculatePoint(t));
            }

            return vertexes;
        }

        /// <summary>
        /// Generate a list of continuous cubic bezier curves that passes through a set of points.
        /// </summary>
        /// <param name="fitPoints">List of points.</param>
        /// <returns>A list of cubic bezier curves.</returns>
        /// <returns>
        /// Original https://www.codeproject.com/Articles/31859/Draw-a-Smooth-Curve-through-a-Set-of-2D-Points-wit by Oleg V. Polikarpotchkin and Peter Lee.
        /// Modified to allow the use of 3D points, and other minor changes to accomodate the existing classes of this library.<br />
        /// The total number of curves returned will be equal to the number of fit points minus 1,
        /// therefore this method is not suitable to use over large number of fit points,
        /// where other, more computational heavy methods, like the least-squares bezier curve fitting would return a less amount of curves.
        /// In such cases, it is advisable to perform some method to reduce the number of points and to avoid duplicates or very close points.
        /// </returns>
        public static List<BezierCurveCubic> CreateFromFitPoints(IEnumerable<Vector3> fitPoints)
        {
            if (fitPoints == null)
            {
                throw new ArgumentNullException(nameof(fitPoints));
            }

            Vector3[] points = fitPoints.ToArray();
            int numFitPoints = points.Length;
            if (numFitPoints < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(fitPoints), numFitPoints, "At least two fit points required.");
            }

            int n = numFitPoints - 1;

            List<BezierCurveCubic> curves = new List<BezierCurveCubic>();
            Vector3 firstControlPoint;
            Vector3 secondControlPoint;

            if (n == 1)
            {
                // Special case: Bezier curve should be a straight line.
                firstControlPoint = points[0] + (points[1] - points[0]) / 3.0;
                secondControlPoint =  points[1] + (points[0] - points[1]) / 3.0;

                curves.Add(new BezierCurveCubic(points[0], firstControlPoint, secondControlPoint, points[1]));
                return curves;
            }

            // Calculate first Bezier control points
            // Right hand side vector
            double[] rhs = new double[n];

            // Set right hand side X values
            for (int i = 1; i < n - 1; i++)
            {
                rhs[i] = 4.0 * points[i].X + 2.0 * points[i + 1].X;

            }
            rhs[0] = points[0].X + 2.0 * points[1].X;
            rhs[n - 1] = (8.0 * points[n - 1].X + points[n].X) / 2.0;
            // Get first control points X-values
            double[] x = GetFirstControlPoints(rhs);

            // Set right hand side Y values
            for (int i = 1; i < n - 1; i++)
            {
                rhs[i] = 4.0 * points[i].Y + 2.0 * points[i + 1].Y;
            }
            rhs[0] = points[0].Y + 2.0 * points[1].Y;
            rhs[n - 1] = (8.0 * points[n - 1].Y + points[n].Y) / 2.0;
            // Get first control points Y-values
            double[] y = GetFirstControlPoints(rhs);

            // Set right hand side Y values
            for (int i = 1; i < n - 1; i++)
            {
                rhs[i] = 4.0 * points[i].Z + 2.0 * points[i + 1].Z;
            }
            rhs[0] = points[0].Z + 2.0 * points[1].Z;
            rhs[n - 1] = (8.0 * points[n - 1].Z + points[n].Z) / 2.0;
            // Get first control points Z-values
            double[] z = GetFirstControlPoints(rhs);

            // create the curves
            for (int i = 0; i < n; i++)
            {
                // First control point
                firstControlPoint = new Vector3(x[i], y[i], z[i]);

                // Second control point
                if (i < n - 1)
                {
                    secondControlPoint = new Vector3(
                        2 * points[i + 1].X - x[i + 1], 
                        2 * points[i + 1].Y - y[i + 1], 
                        2 * points[i + 1].Z - z[i + 1]);
                }
                else
                {
                    secondControlPoint = new Vector3(
                        (points[n].X + x[n - 1]) / 2.0,
                        (points[n].Y + y[n - 1]) / 2.0,
                        (points[n].Z + z[n - 1]) / 2.0);
                }

                curves.Add(new BezierCurveCubic(points[i], firstControlPoint, secondControlPoint, points[i + 1]));
            }

            return curves;
        }

        #endregion

        #region private methods

        /// <summary>
        /// Solves a tri-diagonal system for one of coordinates (X, Y, or Z) of first Bezier control points.
        /// </summary>
        /// <param name="rhs">Right hand side vector.</param>
        /// <returns>Solution vector.</returns>
        private static double[] GetFirstControlPoints(double[] rhs)
        {
            int n = rhs.Length;
            double[] x = new double[n]; // Solution vector.
            double[] tmp = new double[n]; // Temp workspace.

            double b = 2.0;
            x[0] = rhs[0] / b;
            for (int i = 1; i < n; i++) // Decomposition and forward substitution.
            {
                tmp[i] = 1 / b;
                b = (i < n - 1 ? 4.0 : 3.5) - tmp[i];
                x[i] = (rhs[i] - x[i - 1]) / b;
            }

            for (int i = 1; i < n; i++)
            {
                x[n - i - 1] -= tmp[n - i] * x[n - i]; // Back substitution.
            }

            return x;
        }

        #endregion
    }
}