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

        private readonly List<Vector3> fitPoints;
        private readonly SplineCreationMethod creationMethod;
        private Vector3? startTangent;
        private Vector3? endTangent;
        private SplineKnotParameterization knotParameterization = SplineKnotParameterization.FitChord;
        private double knotTolerance = 0.0000001;
        private double ctrlPointTolerance = 0.0000001;
        private double fitTolerance = 0.0000000001;

        private readonly List<SplineVertex> controlPoints;
        private double[] knots;
        private readonly short degree;
        private bool isClosedPeriodic;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>Spline</c> class.
        /// </summary>
        /// <param name="fitPoints">Spline fit points.</param>
        /// <remarks>
        /// The resulting spline curve will be created from a list of cubic bezier curves that passes through the specified fit points.
        /// </remarks>
        public Spline(IEnumerable<Vector3> fitPoints)
            : this(BezierCurveCubic.CreateFromFitPoints(fitPoints))
        {
            this.creationMethod = SplineCreationMethod.FitPoints;
            this.fitPoints = new List<Vector3>(fitPoints);
        }

        /// <summary>
        /// Initializes a new instance of the <c>Spline</c> class.
        /// </summary>
        /// <param name="curves">List of cubic bezier curves.</param>
        public Spline(IEnumerable<BezierCurveQuadratic> curves)
            : this(curves, 2)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Spline</c> class.
        /// </summary>
        /// <param name="curves">List of cubic bezier curves.</param>
        public Spline(IEnumerable<BezierCurveCubic> curves)
            : this(curves, 3)
        {
        }

        private Spline(IEnumerable<BezierCurve> curves, short degree)
            : base(EntityType.Spline, DxfObjectCode.Spline)
        {
            this.degree = degree;
            this.isClosedPeriodic = false;
            this.fitPoints = new List<Vector3>();

            // control points and fit points
            this.controlPoints = new List<SplineVertex>();
            foreach (BezierCurve curve in curves)
            {
                foreach (Vector3 point in curve.ControlPoints)
                {
                    this.controlPoints.Add(new SplineVertex(point));
                }
            }

            this.creationMethod = SplineCreationMethod.ControlPoints;
            this.knots = ÇreateBezierKnotVector(this.controlPoints.Count, this.degree);
        }

        /// <summary>
        /// Initializes a new instance of the <c>Spline</c> class.
        /// </summary>
        /// <param name="controlPoints">Spline control points.</param>
        /// <remarks>By default the degree of the spline is equal three.</remarks>
        public Spline(IEnumerable<SplineVertex> controlPoints)
            : this(controlPoints, 3, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Spline</c> class.
        /// </summary>
        /// <param name="controlPoints">Spline control points.</param>
        /// <param name="closedPeriodic">Sets if the spline as periodic closed (default false).</param>
        /// <remarks>By default the degree of the spline is equal three.</remarks>
        public Spline(IEnumerable<SplineVertex> controlPoints, bool closedPeriodic)
            : this(controlPoints, 3, closedPeriodic)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Spline</c> class.
        /// </summary>
        /// <param name="controlPoints">Spline control points.</param>
        /// <param name="degree">Degree of the spline curve.  Valid values are 1 (linear), degree 2 (quadratic), degree 3 (cubic), and so on up to degree 10.</param>
        public Spline(IEnumerable<SplineVertex> controlPoints, short degree)
            : this(controlPoints, degree, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Spline</c> class.
        /// </summary>
        /// <param name="controlPoints">Spline control points.</param>
        /// <param name="degree">Degree of the spline curve.  Valid values are 1 (linear), degree 2 (quadratic), degree 3 (cubic), and so on up to degree 10.</param>
        /// <param name="closedPeriodic">Sets if the spline as periodic closed (default false).</param>
        public Spline(IEnumerable<SplineVertex> controlPoints, short degree, bool closedPeriodic)
            : base(EntityType.Spline, DxfObjectCode.Spline)
        {
            // spline degree
            if (degree < 1 || degree > 10)
            {
                throw new ArgumentOutOfRangeException(nameof(degree), degree, "The spline degree valid values range from 1 to 10.");
            }
            this.degree = degree;

            // control points
            if (controlPoints == null)
            {
                throw new ArgumentNullException(nameof(controlPoints));
            }

            // create control points
            this.controlPoints = new List<SplineVertex>(controlPoints);
            if (this.controlPoints.Count < 2)
            {
                throw new ArgumentException("The number of control points must be equal or greater than 2.");
            }

            if (this.controlPoints.Count < degree + 1)
            {
                throw new ArgumentException("The number of control points must be equal or greater than the spline degree + 1.");
            }

            this.isClosedPeriodic = closedPeriodic;

            //// for periodic splines
            //if (this.isPeriodic)
            //{
            //    this.isClosed = true;
            //    int replicate = this.degree;
            //    for (int i = 0; i < replicate; i++)
            //    {
            //        this.controlPoints.Add(ctrl[ctrl.Length - replicate + i]);
            //    }
            //}
            //this.controlPoints.AddRange(ctrl);

            this.creationMethod = SplineCreationMethod.ControlPoints;

            this.fitPoints = new List<Vector3>();

            this.knots = CreateKnotVector(this.controlPoints.Count, this.degree, this.isClosedPeriodic);
        }

        /// <summary>
        /// Initializes a new instance of the <c>Spline</c> class.
        /// </summary>
        /// <param name="controlPoints">Spline control points.</param>
        /// <param name="knots">Spline knot vector.</param>
        /// <param name="degree">Degree of the spline curve. Valid values are 1 (linear), degree 2 (quadratic), degree 3 (cubic), and so on up to degree 10.</param>
        public Spline(IEnumerable<SplineVertex> controlPoints, IEnumerable<double> knots, short degree)
            : this(controlPoints, knots, degree, null, SplineCreationMethod.ControlPoints, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Spline</c> class.
        /// </summary>
        /// <param name="controlPoints">Spline control points.</param>
        /// <param name="knots">Spline knot vector.</param>
        /// <param name="degree">Degree of the spline curve.  Valid values are 1 (linear), degree 2 (quadratic), degree 3 (cubic), and so on up to degree 10.</param>
        /// <param name="fitPoints">Spine fit points.</param>
        /// <param name="method">Spline creation method.</param>
        /// <param name="closedPeriodic">Sets if the spline as periodic closed (default false).</param>
        internal Spline(IEnumerable<SplineVertex> controlPoints, IEnumerable<double> knots, short degree, IEnumerable<Vector3> fitPoints, SplineCreationMethod method, bool closedPeriodic)
            : base(EntityType.Spline, DxfObjectCode.Spline)
        {
            // spline degree
            if (degree < 1 || degree > 10)
            {
                throw new ArgumentOutOfRangeException(nameof(degree), degree, "The spline degree valid values range from 1 to 10.");
            }

            this.degree = degree;
            
            // control points
            if (controlPoints == null)
            {
                if (method == SplineCreationMethod.ControlPoints)
                {
                    throw new ArgumentNullException(nameof(controlPoints), "Cannot create a spline without control points if its creation method is with control points.");
                }

                this.controlPoints = new List<SplineVertex>();
            }
            else
            {
                this.controlPoints = new List<SplineVertex>(controlPoints);
                if (this.controlPoints.Count < 2)
                {
                    throw new ArgumentOutOfRangeException(nameof(controlPoints), this.controlPoints.Count, "The number of control points must be equal or greater than 2.");
                }

                if (this.controlPoints.Count < degree + 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(controlPoints), this.controlPoints.Count, "The number of control points must be equal or greater than the spline degree + 1.");
                }

                // knots
                if (knots == null)
                {
                    throw new ArgumentNullException(nameof(knots));
                }

                this.knots = knots.ToArray();
                int numKnots;
                if (closedPeriodic)
                {
                    numKnots = this.controlPoints.Count + 2 * degree + 1;
                }
                else
                {
                    numKnots = this.controlPoints.Count + degree + 1;
                }
                if (this.knots.Length != numKnots)
                {
                    throw new ArgumentException("Invalid number of knots.");
                }

                //if (this.knots.Length != this.controlPoints.Count + degree + 1)
                //{
                //    throw new ArgumentException("The number of knots must be equals to the number of control points + spline degree + 1.");
                //}
            }

            // fit points
            if (fitPoints == null)
            {
                if (method == SplineCreationMethod.FitPoints)
                {
                    throw new ArgumentNullException(nameof(fitPoints), "Cannot create a spline without fit points if its creation method is with fit points.");
                }
                this.fitPoints = new List<Vector3>();
            }
            else
            {
                this.fitPoints = new List<Vector3>(fitPoints);
            }

            this.creationMethod = method;
            this.isClosedPeriodic = closedPeriodic;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the spline <see cref="Vector3">fit points</see> list.
        /// </summary>
        public IReadOnlyList<Vector3> FitPoints
        {
            get { return this.fitPoints; }
        }

        /// <summary>
        /// Gets or sets the spline curve start tangent.
        /// </summary>
        /// <remarks>Only applicable to splines created with fit points.</remarks>
        public Vector3? StartTangent
        {
            get { return this.startTangent; }
            set { this.startTangent = value; }
        }

        /// <summary>
        /// Gets or sets the spline curve end tangent.
        /// </summary>
        /// <remarks>Only applicable to splines created with fit points.</remarks>
        public Vector3? EndTangent
        {
            get { return this.endTangent; }
            set { this.endTangent = value; }
        }

        /// <summary>
        /// Gets or set the knot parameterization computational method.
        /// </summary>
        /// <remarks>
        /// Not usable. When initializing a Spline through a set of fit points, the resulting spline is approximated creating a list of cubic bezier curves.
        /// It is only informative for splines that has been loaded from a DXF file.
        /// </remarks>
        public SplineKnotParameterization KnotParameterization
        {
            get { return this.knotParameterization; }
            set { this.knotParameterization = value; }
        }

        /// <summary>
        /// Gets the spline creation method.
        /// </summary>
        public SplineCreationMethod CreationMethod
        {
            get { return this.creationMethod; }
        }

        /// <summary>
        /// Gets or sets the knot tolerance.
        /// </summary>
        public double KnotTolerance
        {
            get { return this.knotTolerance; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The knot tolerance must be greater than zero.");
                }

                this.knotTolerance = value;
            }
        }

        /// <summary>
        /// Gets or sets the control point tolerance.
        /// </summary>
        public double CtrlPointTolerance
        {
            get { return this.ctrlPointTolerance; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The control point tolerance must be greater than zero.");
                }

                this.ctrlPointTolerance = value;
            }
        }

        /// <summary>
        /// Gets or sets the fit point tolerance.
        /// </summary>
        public double FitTolerance
        {
            get { return this.fitTolerance; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The fit tolerance must be greater than zero.");
                }

                this.fitTolerance = value;
            }
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
        /// <remarks>
        /// An Spline is closed when the start and end control points are the same.
        /// </remarks>
        public bool IsClosed
        {
            get
            {
                return this.controlPoints[0].Position.Equals(this.controlPoints[this.controlPoints.Count - 1].Position);
            }
        }

        /// <summary>
        /// Gets or sets if the spline is closed and periodic.
        /// </summary>
        /// <remarks>
        /// A periodic spline is always closed creating a smooth continuity at the end points. <br />
        /// Changing the property will rebuild the knot vector.
        /// </remarks>
        public bool IsClosedPeriodic
        {
            get { return this.isClosedPeriodic; }
            set
            {
                if (this.isClosedPeriodic != value)
                {
                    if (value && this.IsClosed)
                    {
                        this.controlPoints.RemoveAt(this.controlPoints.Count - 1);
                    }
                    this.knots = CreateKnotVector(this.controlPoints.Count, this.degree, value);
                }
                
                this.isClosedPeriodic = value;
            }
        }

        /// <summary>
        /// Gets the spline <see cref="SplineVertex">control points</see> list.
        /// </summary>
        public IReadOnlyList<SplineVertex> ControlPoints
        {
            get { return this.controlPoints; }
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

        #region public methods

        /// <summary>
        /// Switch the spline direction.
        /// </summary>
        public void Reverse()
        {
            this.fitPoints.Reverse();
            this.controlPoints.Reverse();
            Vector3? tmp = this.startTangent;
            this.startTangent = -this.endTangent;
            this.endTangent = -tmp;
        }

        /// <summary>
        /// Sets all control point weights to the specified number.
        /// </summary>
        /// <param name="weight">Control point weight.</param>
        public void SetUniformWeights(double weight)
        {
            foreach (SplineVertex controlPoint in this.controlPoints)
            {
                controlPoint.Weight = weight;
            }
        }

        /// <summary>
        /// Converts the spline in a list of vertexes.
        /// </summary>
        /// <param name="precision">Number of vertexes generated.</param>
        /// <returns>A list vertexes that represents the spline.</returns>
        public List<Vector3> PolygonalVertexes(int precision)
        {
            return NurbsEvaluator(this.controlPoints, this.knots, this.degree, this.IsClosed, this.isClosedPeriodic, precision);
        }

        /// <summary>
        /// Converts the spline in a Polyline3D.
        /// </summary>
        /// <param name="precision">Number of vertexes generated.</param>
        /// <returns>A new instance of <see cref="Polyline3D">Polyline3D</see> that represents the spline.</returns>
        public Polyline3D ToPolyline3D(int precision)
        {
            IEnumerable<Vector3> vertexes = this.PolygonalVertexes(precision);
            bool closed = this.IsClosed || this.IsClosedPeriodic;
            Polyline3D poly = new Polyline3D (vertexes)
            {
                Layer = (Layer) this.Layer.Clone(),
                Linetype = (Linetype) this.Linetype.Clone(),
                Color = (AciColor) this.Color.Clone(),
                Lineweight = this.Lineweight,
                Transparency = (Transparency) this.Transparency.Clone(),
                LinetypeScale = this.LinetypeScale,
                Normal = this.Normal,
                IsClosed = closed
            };

            return poly;
        }

        /// <summary>
        /// Converts the spline in a Polyline2D.
        /// </summary>
        /// <param name="precision">Number of vertexes generated.</param>
        /// <returns>A new instance of <see cref="Polyline2D">Polyline2D</see> that represents the spline.</returns>
        /// <remarks>
        /// The resulting polyline will be a projection of the actual spline into the plane defined by its normal vector.
        /// </remarks>
        public Polyline2D ToPolyline2D(int precision)
        {
            List<Vector3> vertexes3D = this.PolygonalVertexes(precision);
            List<Vector2> vertexes2D = MathHelper.Transform(vertexes3D, this.Normal, out double _);
            bool closed = this.IsClosed || this.IsClosedPeriodic;
            Polyline2D polyline2D = new Polyline2D(vertexes2D)
            {
                Layer = (Layer) this.Layer.Clone(),
                Linetype = (Linetype) this.Linetype.Clone(),
                Color = (AciColor) this.Color.Clone(),
                Lineweight = this.Lineweight,
                Transparency = (Transparency) this.Transparency.Clone(),
                LinetypeScale = this.LinetypeScale,
                Normal = this.Normal,
                IsClosed = closed
            };

            return polyline2D;
        }

        /// <summary>
        /// Calculate points along a NURBS curve.
        /// </summary>
        /// <param name="ctrlPoints">List of spline control points.</param>
        /// <param name="knots">List of spline knot points. If null the knot vector will be automatically generated.</param>
        /// <param name="degree">Spline degree.</param>
        /// <param name="isClosed">Specifies if the spline is closed.</param>
        /// <param name="isClosedPeriodic">Specifies if the spline is closed and periodic.</param>
        /// <param name="precision">Number of vertexes generated.</param>
        /// <returns>A list vertexes that represents the spline.</returns>
        /// <remarks>
        /// NURBS evaluator provided by mikau16 based on Michael V. implementation, roughly follows the notation of http://cs.mtu.edu/~shene/PUBLICATIONS/2004/NURBS.pdf
        /// Added a few modifications to make it work for open, closed, and periodic closed splines.
        /// </remarks>
        public static List<Vector3> NurbsEvaluator(List<SplineVertex> ctrlPoints, double[] knots, int degree, bool isClosed, bool isClosedPeriodic, int precision)
        {
            if (ctrlPoints == null)
            {
                throw new ArgumentNullException(nameof(ctrlPoints), "A spline entity with control points is required.");
            }

            if (ctrlPoints.Count == 0)
            {
                throw new ArgumentException("A spline entity with control points is required.", nameof(ctrlPoints));
            }

            if (precision < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(precision), precision, "The precision must be equal or greater than two.");
            }

            if (knots == null)
            {
                knots = CreateKnotVector(ctrlPoints.Count, degree, isClosedPeriodic);
            }
            else
            {
                int numKnots;
                if (isClosedPeriodic)
                {
                    numKnots = ctrlPoints.Count + 2 * degree + 1;
                }
                else
                {
                    numKnots = ctrlPoints.Count + degree + 1;
                }
                if (knots.Length != numKnots)
                {
                    throw new ArgumentException("Invalid number of knots.");
                }
            }

            List<SplineVertex> ctrl = new List<SplineVertex>();
            if (isClosedPeriodic)
            {
                for (int i = 0; i < degree; i++)
                {
                    ctrl.Add(ctrlPoints[ctrlPoints.Count - degree + i]);
                }
                ctrl.AddRange(ctrlPoints);
            }
            else
            {
                ctrl.AddRange(ctrlPoints);
            }

            double uStart;
            double uEnd;
            List<Vector3> vertexes = new List<Vector3>();

            if (isClosed)
            {
                uStart = knots[0];
                uEnd = knots[knots.Length - 1];
            }
            else if (isClosedPeriodic)
            {
                uStart = knots[degree];
                uEnd = knots[knots.Length - degree - 1];
            }
            else
            {
                precision -= 1;
                uStart = knots[0];
                uEnd = knots[knots.Length - 1];
            }

            double uDelta = (uEnd - uStart) / precision;

            for (int i = 0; i < precision; i++)
            {
                double u = uStart + uDelta * i;
                vertexes.Add(C(ctrl, knots, degree, u));
            }

            if (!(isClosed || isClosedPeriodic))
            {
                vertexes.Add(ctrl[ctrl.Count - 1].Position);
            }

            return vertexes;
        }

        #endregion

        #region private methods

        //private static double[] GenerateKnotVectorUniform(int count, double min, double max)
        //{
        //    double[] knots = new double[count]; // Knots
        //    double frac = (max - min) / (knots.Length - 1);
        //    double total = min;

        //    // Generates knot values - Uniformly
        //    for (int i = 0; i < knots.Length; i++)
        //    {
        //        if (i == knots.Length - 1)
        //        {
        //            knots[i] = max;
        //            break;
        //        }

        //        knots[i] = total;
        //        total += frac;
        //    }

        //    // Returns knots
        //    return knots;
        //}

        private static double[] CreateKnotVector(int numControlPoints, int degree, bool isPeriodic)
        {
            // create knot vector
            int numKnots;
            double[] knots;

            if (!isPeriodic)
            {
                numKnots = numControlPoints + degree + 1;
                knots = new double[numKnots];

                int i;
                for (i = 0; i <= degree; i++)
                {
                    knots[i] = 0.0;
                }

                for (; i < numControlPoints; i++)
                {
                    knots[i] = i - degree;
                }

                for (; i < numKnots; i++)
                {
                    knots[i] = numControlPoints - degree;
                }
            }
            else
            {
                numKnots = numControlPoints + 2 * degree + 1;
                knots = new double[numKnots];

                double factor = 1.0 / (numControlPoints - degree);
                for (int i = 0; i < numKnots; i++)
                {
                    knots[i] = (i - degree) * factor;
                }
            }

            return knots;
        }

        private static double[] ÇreateBezierKnotVector(int numControlPoints, int degree)
        {
            // create knot vector
            int numKnots = numControlPoints + degree + 1;
            double[] knots = new double[numKnots];

            int np = degree + 1;
            int nc =  numKnots / np;
            double fact = 1.0 / nc;
            int index = 1;

            for (int i = 0; i < numKnots;)
            {
                double knot;

                if (i < np)
                {
                    knot = 0.0;
                }
                else if (i >= numKnots - np)
                {
                    knot = 1.0;
                }
                else
                {
                    knot = fact * index;
                    index += 1;
                }

                for (int j = 0; j < np; j++)
                {
                    knots[i] = knot;
                    i += 1;
                }
            }

            return knots;
        }

        private static Vector3 C(List<SplineVertex> ctrlPoints, double[] knots, int degree, double u)
        {
            Vector3 vectorSum = Vector3.Zero;
            double denominatorSum = 0.0;

            // optimization suggested by ThVoss
            for (int i = 0; i < ctrlPoints.Count; i++)
            {
                double n = N(knots, i, degree, u);
                denominatorSum += n * ctrlPoints[i].Weight;
                vectorSum += ctrlPoints[i].Weight * n * ctrlPoints[i].Position;
            }

            // avoid possible divided by zero error, this should never happen
            if (Math.Abs(denominatorSum) < double.Epsilon)
            {
                return Vector3.Zero;
            }

            return (1.0 / denominatorSum) * vectorSum;
        }

        private static double N(double[] knots, int i, int p, double u)
        {
            if (p <= 0)
            {
                if (knots[i] <= u && u < knots[i + 1])
                {
                    return 1;
                }

                return 0.0;
            }

            double leftCoefficient = 0.0;
            if (!(Math.Abs(knots[i + p] - knots[i]) < MathHelper.Epsilon))
            {
                leftCoefficient = (u - knots[i]) / (knots[i + p] - knots[i]);
            }

            double rightCoefficient = 0.0; // article contains error here, denominator is Knots[i + p + 1] - Knots[i + 1]
            if (!(Math.Abs(knots[i + p + 1] - knots[i + 1]) < double.Epsilon))
            {
                rightCoefficient = (knots[i + p + 1] - u) / (knots[i + p + 1] - knots[i + 1]);
            }

            return leftCoefficient * N(knots, i, p - 1, u) + rightCoefficient * N(knots, i + 1, p - 1, u);
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
            foreach (SplineVertex vertex in this.ControlPoints)
            {
                vertex.Position = transformation * vertex.Position + translation;
            }

            for (int i = 0; i < this.fitPoints.Count; i++)
            {
                this.fitPoints[i] = transformation * this.fitPoints[i] + translation;
            }

            Vector3 newNormal = transformation * this.Normal;
            if (Vector3.Equals(Vector3.Zero, newNormal))
            {
                newNormal = this.Normal;
            }
            this.Normal = newNormal;
        }

        /// <summary>
        /// Creates a new Spline that is a copy of the current instance.
        /// </summary>
        /// <returns>A new Spline that is a copy of this instance.</returns>
        public override object Clone()
        {
            Spline entity;
            if (this.creationMethod == SplineCreationMethod.FitPoints)
            {
                entity = new Spline(new List<Vector3>(this.fitPoints))
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
                    //Spline properties
                    KnotParameterization = this.KnotParameterization,
                    StartTangent = this.startTangent,
                    EndTangent = this.endTangent
                };
            }
            else
            {
                List<SplineVertex> copyControlPoints = new List<SplineVertex>(this.controlPoints.Count);
                foreach (SplineVertex vertex in this.controlPoints)
                {
                    copyControlPoints.Add((SplineVertex) vertex.Clone());
                }

                entity = new Spline(copyControlPoints, this.knots, this.degree, this.fitPoints, this.creationMethod, this.isClosedPeriodic)
                {
                    //EntityObject properties
                    Layer = (Layer) this.Layer.Clone(),
                    Linetype = (Linetype) this.Linetype.Clone(),
                    Color = (AciColor) this.Color.Clone(),
                    Lineweight = this.Lineweight,
                    Transparency = (Transparency) this.Transparency.Clone(),
                    LinetypeScale = this.LinetypeScale,
                    Normal = this.Normal,
                    //Spline properties
                    KnotParameterization = this.KnotParameterization,
                    StartTangent = this.startTangent,
                    EndTangent = this.endTangent
                };
            }


            foreach (XData data in this.XData.Values)
            {
                entity.XData.Add((XData) data.Clone());
            }

            return entity;
        }

        #endregion
    }
}