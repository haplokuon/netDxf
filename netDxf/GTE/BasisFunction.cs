// This is a translation to C# from the original C++ code of the Geometric Tool Library
// Original license
// David Eberly, Geometric Tools, Redmond WA 98052
// Copyright (c) 1998-2022
// Distributed under the Boost Software License, Version 1.0.
// https://www.boost.org/LICENSE_1_0.txt
// https://www.geometrictools.com/License/Boost/LICENSE_1_0.txt
// Version: 6.0.2022.01.06

using System;
using System.Diagnostics;

namespace netDxf.GTE
{
    internal struct UniqueKnot
    {
        private double t;
        private int multiplicity;

        public UniqueKnot(double t, int multiplicity)
        {
            this.t = t;
            this.multiplicity = multiplicity;
        }

        public double T
        {
            get { return this.t; }
            set { this.t = value; }
        }

        public int Multiplicity
        {
            get { return this.multiplicity; }
            set { this.multiplicity = value; }
        }
    }

    internal struct BasisFunctionInput
    {
        private int numControls;
        private int degree;
        private bool uniform;
        private bool periodic;
        private int numUniqueKnots;
        private UniqueKnot[] uniqueKnots;

        // Construct an open uniform curve with t in [0,1].
        public BasisFunctionInput(int inNumControls, int inDegree)
        {
            this.numControls = inNumControls;
            this.degree = inDegree;
            this.uniform = true;
            this.periodic = false;
            this.numUniqueKnots = this.numControls - this.degree + 1;
            this.uniqueKnots = new UniqueKnot[this.numUniqueKnots];
            this.uniqueKnots[0] = new UniqueKnot(0, this.degree + 1);
            for (int i = 1; i <= this.numUniqueKnots - 2; i++)
            {
                this.uniqueKnots[i] = new UniqueKnot(i / (this.numUniqueKnots - 1.0), 1);
            }

            this.uniqueKnots[this.uniqueKnots.Length - 1] = new UniqueKnot(1, this.degree + 1);
        }

        public int NumControls
        {
            get { return this.numControls; }
        }

        public int Degree
        {
            get { return this.degree; }
            set { this.degree = value; }
        }

        public bool Uniform
        {
            get { return this.uniform; }
            set { this.uniform = value; }
        }

        public bool Periodic
        {
            get { return this.periodic; }
            set { this.periodic = value; }
        }

        public int NumUniqueKnots
        {
            get { return this.numUniqueKnots; }
        }

        public UniqueKnot[] UniqueKnots
        {
            get { return this.uniqueKnots; }
        }
    }

    // Let n be the number of control points. Let d be the degree, where
    // 1 <= d <= n-1.  The number of knots is k = n + d + 1.  The knots
    // are t[i] for 0 <= i < k and must be non decreasing, t[i] <= t[i+1],
    // but a knot value can be repeated.  Let s be the number of distinct
    // knots.  Let the distinct knots be u[j] for 0 <= j < s, so u[j] <
    // u[j+1] for all j.  The set of u[j] is called a 'breakpoint
    // sequence'.  Let m[j] >= 1 be the multiplicity; that is, if t[i] is
    // the first occurrence of u[j], then t[i+r] = t[i] for 1 <= r < m[j].
    // The multiplicities have the constraints m[0] <= d+1, m[s-1] <= d+1,
    // and m[j] <= d for 1 <= j <= s-2.  Also, k = sum_{j=0}^{s-1} m[j],
    // which says the multiplicities account for all k knots.
    //
    // Given a knot vector (t[0],...,t[n+d]), the domain of the
    // corresponding B-spline curve is the interval [t[d],t[n]].
    //
    // The corresponding B-spline or NURBS curve is characterized as
    // follows.  See "Geometric Modeling with Splines: An Introduction" by
    // Elaine Cohen, Richard F. Riesenfeld and Gershon Elber, AK Peters,
    // 2001, Natick MA.  The curve is 'open' when m[0] = m[s-1] = d+1;
    // otherwise, it is 'floating'.  An open curve is uniform when the
    // knots t[d] through t[n] are equally spaced; that is, t[i+1] - t[i]
    // are a common value for d <= i <= n-1.  By implication, s = n-d+1
    // and m[j] = 1 for 1 <= j <= s-2.  An open curve that does not
    // satisfy these conditions is said to be nonuniform.  A floating
    // curve is uniform when m[j] = 1 for 0 <= j <= s-1 and t[i+1] - t[i]
    // are a common value for 0 <= i <= k-2; otherwise, the floating curve
    // is nonuniform.
    //
    // A special case of a floating curve is a periodic curve.  The intent
    // is that the curve is closed, so the first and last control points
    // should be the same, which ensures C^{0} continuity.  Higher-order
    // continuity is obtained by repeating more control points.  If the
    // control points are P[0] through P[n-1], append the points P[0]
    // through P[d-1] to ensure C^{d-1} continuity.  Additionally, the
    // knots must be chosen properly.  You may choose t[d] through t[n] as
    // you wish.  The other knots are defined by
    //   t[i] - t[i-1] = t[n-d+i] - t[n-d+i-1]
    //   t[n+i] - t[n+i-1] = t[d+i] - t[d+i-1]
    // for 1 <= i <= d.
    internal class BasisFunction
    {
        private struct Key
        {
            private double mKnotValue;
            private int mKnotIndex;

            public Key(double knotValue, int knotIndex)
            {
                this.mKnotValue = knotValue;
                this.mKnotIndex = knotIndex;
            }

            public double KnotValue
            {
                get { return this.mKnotValue; }
            }

            public int KnotIndex
            {
                get { return this.mKnotIndex; }
            }
        }


        // Constructor inputs and values derived from them.
        private int mNumControls;
        private int mDegree;
        private double mTMin, mTMax, mTLength;
        private bool mOpen;
        private bool mUniform;
        private bool mPeriodic;
        private UniqueKnot[] mUniqueKnots;
        private double[] mKnots;

        // Lookup information for the GetIndex() function.  The first element
        // of the pair is a unique knot value.  The second element is the
        // index in mKnots[] for the last occurrence of that knot value.
        private Key[] mKeys;

        // Storage for the basis functions and their first three derivatives;
        // mJet[i] is array[d+1][n+d].
        private double[][][] mJet;

        // Construction and destruction.  The determination that the curve is
        // open or floating is based on the multiplicities.  The 'uniform'
        // input is used to avoid misclassifications due to floating-point
        // rounding errors.  Specifically, the breakpoints might be equally
        // spaced (uniform) as real numbers, but the floating-point
        // representations can have rounding errors that cause the knot
        // differences not to be exactly the same constant.  A periodic curve
        // can have uniform or nonuniform knots.  This object makes copies of
        // the input arrays.
        public BasisFunction(BasisFunctionInput input)
        {
            this.Create(input);
        }

        // Support for explicit creation in classes that have std::array
        // members involving BasisFunction.  This is a call-once function.
        public void Create(BasisFunctionInput input)
        {
            Debug.Assert(input.NumControls >= 2, "Invalid number of control points.");
            Debug.Assert(1 <= input.Degree && input.Degree < input.NumControls, "Invalid degree.");
            Debug.Assert(input.NumUniqueKnots >= 2, "Invalid number of unique knots.");

            this.mNumControls = input.Periodic ? input.NumControls + input.Degree : input.NumControls;
            this.mDegree = input.Degree;
            this.mTMin = 0;
            this.mTMax = 0;
            this.mTLength = 0;
            this.mOpen = false;
            this.mUniform = input.Uniform;
            this.mPeriodic = input.Periodic;
            this.mJet = new double[4][][];

            this.mUniqueKnots = new UniqueKnot[input.UniqueKnots.Length];
            input.UniqueKnots.CopyTo(this.mUniqueKnots, 0);

            double u = this.mUniqueKnots[0].T;
            for (int i = 1; i < input.NumUniqueKnots - 1; i++)
            {
                double uNext = this.mUniqueKnots[i].T;
                Debug.Assert(u < uNext, "Unique knots are not strictly increasing.");
                u = uNext;
            }

            int mult0 = this.mUniqueKnots[0].Multiplicity;
            Debug.Assert(mult0 >= 1 && mult0 <= this.mDegree + 1, "Invalid first multiplicity.");

            int mult1 = this.mUniqueKnots[this.mUniqueKnots.Length -1].Multiplicity;
            Debug.Assert(mult1 >= 1 && mult1 <= this.mDegree + 1, "Invalid last multiplicity.");

            for (int i = 1; i <= input.NumUniqueKnots - 2; i++)
            {
                int mult = this.mUniqueKnots[i].Multiplicity;
                Debug.Assert(mult >= 1 && mult <= this.mDegree + 1, "Invalid interior multiplicity.");
            }

            this.mOpen = mult0 == mult1 && mult0 == this.mDegree + 1;

            this.mKnots = new double[this.mNumControls + this.mDegree + 1];
            this.mKeys = new Key[input.NumUniqueKnots];
            int sum = 0;
            for (int i = 0, j = 0; i < input.NumUniqueKnots; i++)
            {
                double tCommon = this.mUniqueKnots[i].T;
                int mult = this.mUniqueKnots[i].Multiplicity;
                for (int k = 0; k < mult; k++, j++)
                {
                    this.mKnots[j] = tCommon;
                }

                this.mKeys[i] = new Key(tCommon, sum - 1);
                sum += mult;
            }
            
            this.mTMin = this.mKnots[this.mDegree];
            this.mTMax = this.mKnots[this.mNumControls];
            this.mTLength = this.mTMax - this.mTMin;

            int numRows = this.mDegree + 1;
            int numCols = this.mNumControls + this.mDegree;
            for (int i = 0; i < 4; ++i)
            {
                this.mJet[i] = new double[numRows][];
                for (int j = 0; j < numRows; j++)
                {
                    this.mJet[i][j] = new double[numCols];
                }
            }
        }

        // Member access.
        public int NumControls
        {
            get { return this.mNumControls; }
        }

        public int Degree
        {
            get { return this.mDegree; }
        }

        public int NumUniqueKnots
        {
            get { return this.mUniqueKnots.Length; }
        }

        public int NumKnots
        {
            get { return this.mKnots.Length; }
        }

        public double MinDomain
        {
            get { return this.mTMin; }
        }

        public double MaxDomain
        {
            get { return this.mTMax; }
        }

        public bool IsOpen
        {
            get { return this.mOpen; }
        }

        public bool IsUniform
        {
            get { return this.mUniform; }
        }

        public bool IsPeriodic
        {
            get { return this.mPeriodic; }
        }

        public UniqueKnot[] UniqueKnots
        {
            get { return this.mUniqueKnots; }
        }

        public double[] Knots
        {
            get { return this.mKnots; }
        }

        // Evaluation of the basis function and its derivatives through 
        // order 3.  For the function value only, pass order 0.  For the
        // function and first derivative, pass order 1, and so on.
        public void Evaluate(double t, int order, out int minIndex, out int maxIndex)
        {
            Debug.Assert(order <= 3, "Invalid order.");

            int i = this.GetIndex(ref t);
            this.mJet[0][0][i] = 1;

            if (order >= 1)
            {
                this.mJet[1][0][i] = 0;
                if (order >= 2)
                {
                    this.mJet[2][0][i] = 0;
                    if (order >= 3)
                    {
                        this.mJet[3][0][i] = 0;
                    }
                }
            }

            double n0 = t - this.mKnots[i], n1 = this.mKnots[i + 1] - t;
            double e0, e1, d0, d1, invD0, invD1;
            int j;
            for (j = 1; j <= this.mDegree; j++)
            {
                d0 = this.mKnots[i + j] - this.mKnots[i];
                d1 = this.mKnots[i + 1] - this.mKnots[i - j + 1];
                invD0 = d0 > 0 ? 1 / d0 : 0;
                invD1 = d1 > 0 ? 1 / d1 : 0;

                e0 = n0 * this.mJet[0][j - 1][i];
                this.mJet[0][j][i] = e0 * invD0;
                e1 = n1 * this.mJet[0][j - 1][i - j + 1];
                this.mJet[0][j][i - j] = e1 * invD1;

                if (order >= 1)
                {
                    e0 = n0 * this.mJet[1][j - 1][i] + this.mJet[0][j - 1][i];
                    this.mJet[1][j][i] = e0 * invD0;
                    e1 = n1 * this.mJet[1][j - 1][i - j + 1] - this.mJet[0][j - 1][i - j + 1];
                    this.mJet[1][j][i - j] = e1 * invD1;

                    if (order >= 2)
                    {
                        e0 = n0 * this.mJet[2][j - 1][i] + 2 * this.mJet[1][j - 1][i];
                        this.mJet[2][j][i] = e0 * invD0;
                        e1 = n1 * this.mJet[2][j - 1][i - j + 1] - 2 * this.mJet[1][j - 1][i - j + 1];
                        this.mJet[2][j][i - j] = e1 * invD1;

                        if (order >= 3)
                        {
                            e0 = n0 * this.mJet[3][j - 1][i] + 3 * this.mJet[2][j - 1][i];
                            this.mJet[3][j][i] = e0 * invD0;
                            e1 = n1 * this.mJet[3][j - 1][i - j + 1] - 3 * this.mJet[2][j - 1][i - j + 1];
                            this.mJet[3][j][i - j] = e1 * invD1;
                        }
                    }
                }
            }

            for (j = 2; j <= this.mDegree; j++)
            {
                for (int k = i - j + 1; k < i; k++)
                {
                    n0 = t - this.mKnots[k];
                    n1 = this.mKnots[k + j + 1] - t;
                    d0 = this.mKnots[k + j] - this.mKnots[k];
                    d1 = this.mKnots[k + j + 1] - this.mKnots[k + 1];
                    invD0 = d0 > 0 ? 1 / d0 : 0;
                    invD1 = d1 > 0 ? 1 / d1 : 0;

                    e0 = n0 * this.mJet[0][j - 1][k];
                    e1 = n1 * this.mJet[0][j - 1][k + 1];
                    this.mJet[0][j][k] = e0 * invD0 + e1 * invD1;

                    if (order >= 1)
                    {
                        e0 = n0 * this.mJet[1][j - 1][k] + this.mJet[0][j - 1][k];
                        e1 = n1 * this.mJet[1][j - 1][k + 1] - this.mJet[0][j - 1][k + 1];
                        this.mJet[1][j][k] = e0 * invD0 + e1 * invD1;

                        if (order >= 2)
                        {
                            e0 = n0 * this.mJet[2][j - 1][k] + 2 * this.mJet[1][j - 1][k];
                            e1 = n1 * this.mJet[2][j - 1][k + 1] - 2 * this.mJet[1][j - 1][k + 1];
                            this.mJet[2][j][k] = e0 * invD0 + e1 * invD1;

                            if (order >= 3)
                            {
                                e0 = n0 * this.mJet[3][j - 1][k] + 3 * this.mJet[2][j - 1][k];
                                e1 = n1 * this.mJet[3][j - 1][k + 1] - 3 * this.mJet[2][j - 1][k + 1];
                                this.mJet[3][j][k] = e0 * invD0 + e1 * invD1;
                            }
                        }
                    }
                }
            }

            minIndex = i - this.mDegree;
            maxIndex = i;
        }

        // Access the results of the call to Evaluate(...).  The index i must
        // satisfy minIndex <= i <= maxIndex.  If it is not, the function
        // returns zero.  The separation of evaluation and access is based on
        // local control of the basis function; that is, only the accessible
        // values are (potentially) not zero.
        public double GetValue(int order, int i)
        {
            if (order < 4)
            {
                if (0 <= i && i < this.mNumControls + this.mDegree)
                {
                    return this.mJet[order][this.mDegree][i];
                }

                throw new ArgumentException("Invalid index.", nameof(i));
            }

            throw new ArgumentException("Invalid order.", nameof(order));
        }

        // Determine the index i for which knot[i] <= t < knot[i+1].  The
        // t-value is modified (wrapped for periodic splines, clamped for
        // non periodic splines).
        private int GetIndex(ref double t)
        {
            // Find the index i for which knot[i] <= t < knot[i+1].
            if (this.mPeriodic)
            {
                // Wrap to [tmin,tmax].
                double r = (t - this.mTMin) % this.mTLength;
                if (r < 0.0)
                {
                    r += this.mTLength;
                }

                t = this.mTMin + r;
            }

            // Clamp to [tmin,tmax]. For the periodic case, this handles
            // small numerical rounding errors near the domain endpoints.
            if (t <= this.mTMin)
            {
                t = this.mTMin;
                return this.mDegree;
            }

            if (t >= this.mTMax)
            {
                t = this.mTMax;
                return this.mNumControls - 1;
            }

            // At this point, tmin < t < tmax.
            foreach (Key key in this.mKeys)
            {
                if (t < key.KnotValue)
                {
                    return key.KnotIndex;
                }
            }

            // We should not reach this code.
            throw new Exception("Unexpected condition.");
        }
    }
}