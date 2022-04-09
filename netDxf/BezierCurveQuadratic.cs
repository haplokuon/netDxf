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

namespace netDxf
{
    /// <summary>
    /// Represent a quadratic bezier curve.
    /// </summary>
    public class BezierCurveQuadratic :
        BezierCurve
    {
        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>BezierCurveQuadratic</c> class.
        /// </summary>
        /// <param name="controlPoints">A list of three control points.</param>
        /// <remarks>
        /// The list must contain three control points.
        /// The first index represents the start point or anchor,
        /// the second represents the control point,
        /// and the last the end point.
        /// </remarks>
        public BezierCurveQuadratic(IEnumerable<Vector3> controlPoints)
            : base(controlPoints)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>BezierCurve</c> class.
        /// </summary>
        /// <param name="startPoint">Start anchor point.</param>
        /// <param name="controlPoint">Second control point.</param>
        /// <param name="endPoint">End anchor point.</param>
        public BezierCurveQuadratic(Vector3 startPoint, Vector3 controlPoint, Vector3 endPoint)
            : base (new []{startPoint, controlPoint, endPoint})
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
        /// Gets or sets the control point.
        /// </summary>
        public Vector3 ControlPoint
        {
            get { return this.controlPoints[1]; }
            set { this.controlPoints[1] = value; }
        }

        /// <summary>
        /// Gets or sets the curve end point.
        /// </summary>
        public Vector3 EndPoint
        {
            get { return this.controlPoints[2]; }
            set { this.controlPoints[2] = value; }
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
            return c * c * this.StartPoint + 2 * t * c * this.ControlPoint + t * t * this.EndPoint;
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
            return Vector3.Normalize(c * this.StartPoint + (1 - 2 * t) * this.ControlPoint + t * this.EndPoint);
        }

        /// <summary>
        /// Splits the actual bezier curve in two at parameter t.
        /// </summary>
        /// <param name="t">Parameter t, between 0.0 and 1.0.</param>
        /// <returns>The two curves result of dividing the actual curve at parameter t.</returns>
        public BezierCurveQuadratic[] Split(double t)
        {
            if (t < 0.0 || t > 1.0)
            {
                throw new ArgumentOutOfRangeException(nameof(t), t, "The parameter t must be between 0.0 and 1.0.");
            }

            Vector3 p12 = (this.ControlPoint - this.StartPoint) * t + this.StartPoint;
            Vector3 p23 = (this.EndPoint - this.ControlPoint) * t + this.ControlPoint;
            Vector3 breakPoint = (p23 - p12) * t + p12;

            return new[]
            {
                new BezierCurveQuadratic(this.StartPoint, p12, breakPoint),
                new BezierCurveQuadratic(breakPoint, p23, this.EndPoint)
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

        #endregion
    }
}