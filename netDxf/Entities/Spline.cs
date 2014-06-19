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
using System.Collections.ObjectModel;

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
        /// <param name="degree">Degree of the spline curve.</param>
        public Spline(List<SplineVertex> controlPoints, double[] knots, short degree)
            : base(EntityType.Spline, DxfObjectCode.Spline)
        {
            if (degree < 1)
                throw new ArgumentException("The degree of the spline must be equal or greater than one.");
            if (controlPoints.Count < 2)
                throw new ArgumentException("The number of control points must be equal or greater than 2.");
            if(controlPoints.Count < degree + 1 )
                throw new ArgumentException("The number of control points must be equal or greater than the spline degree + 1.");
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
        /// <param name="degree">Degree of the spline curve.</param>
        /// <param name="periodic">Sets if the spline as periodic closed (default false).</param>
        public Spline(List<SplineVertex> controlPoints, short degree, bool periodic = false)
            : base(EntityType.Spline, DxfObjectCode.Spline)
        {
            if (degree < 1)
                throw new ArgumentException("The degree of the spline must be equal or greater than one.");
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
        /// Gets the spline degree.
        /// </summary>
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
            List<SplineVertex> copyControlPoints = new List<SplineVertex>();
            foreach (SplineVertex vertex in this.controlPoints)
            {
                copyControlPoints.Add((SplineVertex) vertex.Clone());
            }
            double[] copyKnots = new double[this.knots.Length];
            this.knots.CopyTo(copyKnots, 0);

            return new Spline(copyControlPoints, copyKnots, this.degree)
            {
                //EntityObject properties
                Color = this.color,
                Layer = this.layer,
                LineType = this.lineType,
                Lineweight = this.lineweight,
                LineTypeScale = this.lineTypeScale,
                Normal = this.normal,
                XData = this.xData,
                //Spline properties
            };
        }

        #endregion

    }
}
