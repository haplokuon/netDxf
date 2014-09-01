#region netDxf, Copyright(C) 2014 Daniel Carvajal, Licensed under LGPL.

//                        netDxf library
// Copyright (C) 2014 Daniel Carvajal (haplokuon@gmail.com)
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
using System.Collections.ObjectModel;
using netDxf.Tables;

namespace netDxf.Entities
{
    /// <summary>
    /// Represents a spline curve <see cref="EntityObject">entity</see> (NURBS Non-Uniform Rational B-Splines).
    /// </summary>
    public class Spline :
        EntityObject
    {
        #region private fields

        private List<SplineVertex> controlPoints;
        private double[] knots;
        private readonly SplineTypeFlags flags;
        private readonly short degree;
        private readonly bool isClosed;
        private readonly bool isPeriodic;

        #endregion

        #region contructors

        /// <summary>
        /// Initializes a new instance of the <c>Spline</c> class.
        /// </summary>
        /// <param name="controlPoints">Spline control points.</param>
        /// <param name="knots">Spline knot vector.</param>
        /// <param name="degree">Degree of the spline curve.  Valid values are 1 (linear), degree 2 (quadratic), degree 3 (cubic), and so on up to degree 10.</param>
        public Spline(List<SplineVertex> controlPoints, double[] knots, short degree)
            : base(EntityType.Spline, DxfObjectCode.Spline)
        {
            if (degree < 1 || degree > 10)
                throw (new ArgumentOutOfRangeException("degree", degree, "The spline degree valid values range from 1 to 10."));
            if (controlPoints == null)
                throw new ArgumentNullException("controlPoints", "The Spline control points list cannot be null.");
            if (controlPoints.Count < 2)
                throw new ArgumentException("The number of control points must be equal or greater than 2.");
            if(controlPoints.Count < degree + 1 )
                throw new ArgumentException("The number of control points must be equal or greater than the spline degree + 1.");
            if (knots == null)
                throw new ArgumentNullException("knots", "The Spline knots list cannot be null.");
            if(knots.Length != controlPoints.Count + degree + 1)
                throw new ArgumentException("The number of knots must be equals to the number of control points + spline degree + 1.");

            this.controlPoints = controlPoints;
            this.knots = knots;
            this.degree = degree;
            this.isPeriodic = this.PeriodicTest(controlPoints, degree);
            if (this.isPeriodic)
            {
                this.isClosed = true;
                this.flags = SplineTypeFlags.Closed | SplineTypeFlags.Periodic | SplineTypeFlags.Rational;
            }
            else
            {
                this.isClosed = controlPoints[0].Location.Equals(controlPoints[controlPoints.Count - 1].Location);
                this.flags = this.isClosed ? SplineTypeFlags.Closed | SplineTypeFlags.Rational : SplineTypeFlags.Rational;
            }

        }

        /// <summary>
        /// Initializes a new instance of the <c>Spline</c> class.
        /// </summary>
        /// <param name="controlPoints">Spline control points.</param>
        /// <param name="periodic">Sets if the spline as periodic closed (default false).</param>
        /// <remarks>By default the degree of the spline is equal three.</remarks>
        public Spline(List<SplineVertex> controlPoints, bool periodic = false)
            : this(controlPoints, 3, periodic)
        {      
        }

        /// <summary>
        /// Initializes a new instance of the <c>Spline</c> class.
        /// </summary>
        /// <param name="controlPoints">Spline control points.</param>
        /// <param name="degree">Degree of the spline curve.  Valid values are 1 (linear), degree 2 (quadratic), degree 3 (cubic), and so on up to degree 10.</param>
        /// <param name="periodic">Sets if the spline as periodic closed (default false).</param>
        public Spline(List<SplineVertex> controlPoints, short degree, bool periodic = false)
            : base(EntityType.Spline, DxfObjectCode.Spline)
        {
            if (degree < 1 || degree > 10)
                throw (new ArgumentOutOfRangeException("degree", degree, "The spline degree valid values range from 1 to 10."));
            if (controlPoints == null)
                throw new ArgumentNullException("controlPoints", "The Spline control points list cannot be null.");
            if (controlPoints.Count < 2)
                throw new ArgumentException("The number of control points must be equal or greater than 2.");
            if (controlPoints.Count < degree + 1)
                throw new ArgumentException("The number of control points must be equal or greater than the spline degree + 1.");

            this.degree = degree;
            this.isPeriodic = periodic;
            if (this.isPeriodic)
            {
                this.isClosed = true;
                this.flags = SplineTypeFlags.Closed | SplineTypeFlags.Periodic | SplineTypeFlags.Rational;
            }
            else
            {
                this.isClosed = controlPoints[0].Location.Equals(controlPoints[controlPoints.Count - 1].Location);
                this.flags = this.isClosed ? SplineTypeFlags.Closed | SplineTypeFlags.Rational : SplineTypeFlags.Rational;
            }

            this.Create(controlPoints); 
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the spline <see cref="SplineVertex">control points</see> list.
        /// </summary>
        public ReadOnlyCollection<SplineVertex> ControlPoints
        {
            get { return this.controlPoints.AsReadOnly(); }
        }

        /// <summary>
        /// Gets or sets the polynomial degree of the resulting spline.
        /// </summary>
        /// <remarks>
        /// Valid values are 1 (linear), degree 2 (quadratic), degree 3 (cubic), and so on up to degree 10.
        /// </remarks>
        public short Degree
        {
            get { return this.degree; }
        }

        /// <summary>
        /// Gets if the spline is closed.
        /// </summary>
        public bool IsClosed
        {
            get { return this.isClosed; }
        }

        /// <summary>
        /// Gets if the spline is periodic.
        /// </summary>
        public bool IsPeriodic
        {
            get { return this.isPeriodic; }
        }

        /// <summary>
        /// Gets the spline knot vector.
        /// </summary>
        /// <remarks>By default a uniform knot vector is created.</remarks>
        public double[] Knots
        {
            get { return this.knots; }
        }

        #endregion

        #region internal properties

        /// <summary>
        /// Gets the spline type.
        /// </summary>
        internal SplineTypeFlags Flags
        {
            get { return this.flags; }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Sets all control point weigths to the specified number.
        /// </summary>
        /// <param name="weight">Control point weight.</param>
        public void SetUniformWeights(double weight)
        {
            foreach (SplineVertex controlPoint in this.controlPoints)
            {
                controlPoint.Weigth = weight;
            }
        }
        #endregion

        #region private methods
        
        private bool PeriodicTest(List<SplineVertex> points, short d)
        {
            bool periodic = false;
            for (int i = 0; i < d; i++)
                periodic = points[i].Location.Equals(points[points.Count + i - d].Location);
            return periodic;
        }

        private void Create(List<SplineVertex> points)
        {
            this.controlPoints = new List<SplineVertex>();

            int replicate = this.isPeriodic ? this.degree : 0;
            int numControlPoints = points.Count + replicate;

            foreach (SplineVertex controlPoint in points)
            {
                SplineVertex vertex = new SplineVertex(controlPoint.Location, controlPoint.Weigth);
                this.controlPoints.Add(vertex);
            }
            
            for (int i = 0; i < replicate; i++)
            {
                SplineVertex vertex = new SplineVertex(points[i].Location, points[i].Weigth);
                this.controlPoints.Add(vertex);
            }

            int numKnots = numControlPoints + this.degree + 1;
            this.knots = new double[numKnots];

            double factor = 1.0 / (numControlPoints - this.degree);
            if (!this.isPeriodic)
            {
                int i;
                for (i = 0; i <= this.degree; i++)
                    this.knots[i] = 0.0;

                for (; i < numControlPoints; i++)
                    this.knots[i] = (i - this.degree) * factor;

                for (; i < numKnots; i++)
                    this.knots[i] = 1;
            }
            else
            {
                for (int i = 0; i < numKnots; i++)
                    this.knots[i] = (i - this.degree) * factor;
            }
        }

        #endregion

        #region overrides

        /// <summary>
        /// Creates a new Spline that is a copy of the current instance.
        /// </summary>
        /// <returns>A new Spline that is a copy of this instance.</returns>
        public override object Clone()
        {
            List<SplineVertex> copyControlPoints = new List<SplineVertex>(this.controlPoints.Count);
            foreach (SplineVertex vertex in this.controlPoints)
            {
                copyControlPoints.Add((SplineVertex) vertex.Clone());
            }
            double[] copyKnots = new double[this.knots.Length];
            this.knots.CopyTo(copyKnots, 0);

            Dictionary<string, XData> copyXData = null;
            if (this.xData != null)
            {
                copyXData = new Dictionary<string, XData>();
                foreach (KeyValuePair<string, XData> data in this.xData)
                {
                    copyXData.Add(data.Key, (XData)data.Value.Clone());
                }
            }

            return new Spline(copyControlPoints, copyKnots, this.degree)
            {
                //EntityObject properties
                Layer = (Layer)this.layer.Clone(),
                LineType = (LineType)this.lineType.Clone(),
                Color = (AciColor)this.color.Clone(),
                Lineweight = (Lineweight)this.lineweight.Clone(),
                Transparency = (Transparency)this.transparency.Clone(),
                LineTypeScale = this.lineTypeScale,
                Normal = this.normal,
                XData = copyXData,
                //Spline properties
            };
        }

        #endregion

    }
}
