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
    // The BSplineReduction class is an implementation of the algorithm in
    // https://www.geometrictools.com/Documentation/BSplineReduction.pdf
    // for least-squares fitting of points in the continuous sense by
    // an L2 integral norm.  The least-squares fitting implemented in the
    // file GteBSplineCurveFit.h is in the discrete sense by an L2 summation.
    // The intended use for this class is to take an open B-spline curve,
    // defined by its control points and degree, and reducing the number of
    // control points dramatically to obtain another curve that is close to
    // the original one.

    // The input numCtrlPoints must be 2 or larger.  The input degree must
    // satisfy the condition 1 <= degree <= inControls.size()-1.  The degree
    // of the output curve is the same as that of the input curve.  The input
    // fraction must be in [0,1].  If the fraction is 1, the output curve
    // is identical to the input curve.  If the fraction is too small to
    // produce a valid number of control points, outControls.size() is chosen
    // to be degree+1.
    public class BSplineReduction
    {
        // Input sample information.
        private readonly Vector3[] sampleData;

        // The reduced B-spline curve, open and with uniform knots.
        private readonly int degree;
        private readonly Vector3[] controlData;
        private readonly BasisFunction basisFunction;

        private readonly int[] quantity;
        private readonly int[] numKnots; // N+D+2
        private readonly double[][] knot;

        // For the integration-based least-squares fitting.
        private readonly int[] mBasis, mIndex;

        private BSplineReduction()
        {
            this.degree = 0;
            this.quantity = new[] {0, 0};
            this.numKnots = new[] {0, 0};
            this.mBasis = new[] {0, 0};
            this.knot = new double[2][];
            this.mIndex = new[] {0, 0};
        }

        public BSplineReduction(Vector3[] inControls, int degree, double fraction)
            : this()
        {
            int numSamples = inControls.Length;
            this.sampleData = inControls;
            Debug.Assert(numSamples >= 2 && 1 <= degree && degree < numSamples, "Invalid input.");

            // Clamp the number of control points to [degree+1,quantity-1].
            int numControls = (int) Math.Round(fraction * numSamples);
            if (numControls >= numSamples)
            {
                this.controlData = inControls;
                return;
            }

            if (numControls < degree + 1)
            {
                numControls = degree + 1;
            }

            // Set up basis function parameters.  Function 0 corresponds to
            // the output curve.  Function 1 corresponds to the input curve.
            this.degree = degree;
            this.quantity[0] = numControls;
            this.quantity[1] = numSamples;

            for (int j = 0; j <= 1; j++)
            {
                this.numKnots[j] = this.quantity[j] + this.degree + 1;
                this.knot[j] = new double[this.numKnots[j]];

                int i;
                for (i = 0; i <= this.degree; ++i)
                {
                    this.knot[j][i] = 0.0;
                }

                double denom = this.quantity[j] - this.degree;
                double factor = 1.0 / denom;
                for ( /**/; i < this.quantity[j]; ++i)
                {
                    this.knot[j][i] = (i - this.degree) * factor;
                }

                for ( /**/; i < this.numKnots[j]; ++i)
                {
                    this.knot[j][i] = 1.0;
                }
            }

            // Construct matrix A (depends only on the output basis function).
            double value, tmin, tmax;
            int i0, i1;

            this.mBasis[0] = 0;
            this.mBasis[1] = 0;

            double integrand(double t)
            {
                double value0 = this.F(this.mBasis[0], this.mIndex[0], this.degree, t);
                double value1 = this.F(this.mBasis[1], this.mIndex[1], this.degree, t);
                double result = value0 * value1;
                return result;
            }

            BandedMatrix A = new BandedMatrix(this.quantity[0], this.degree, this.degree);
            for (i0 = 0; i0 < this.quantity[0]; ++i0)
            {
                this.mIndex[0] = i0;
                tmax = this.MaxSupport(0, i0);

                for (i1 = i0; i1 <= i0 + this.degree && i1 < this.quantity[0]; ++i1)
                {
                    this.mIndex[1] = i1;
                    tmin = this.MinSupport(0, i1);

                    value = Integration.Romberg(8, tmin, tmax, integrand);
                    A[i0, i1] = value;
                    A[i1, i0] = value;
                }
            }

            // Construct A^{-1}.
            // scheme to invert A?
            GMatrix invA = new GMatrix(this.quantity[0], this.quantity[0]);
            bool invertible = A.ComputeInverse(invA.Elements.Vector);
            Debug.Assert(invertible, "Failed to invert matrix.");

            // Construct B (depends on both input and output basis functions).
            this.mBasis[1] = 1;
            GMatrix B = new GMatrix(this.quantity[0], this.quantity[1]);
            for (i0 = 0; i0 < this.quantity[0]; ++i0)
            {
                this.mIndex[0] = i0;
                double tmin0 = this.MinSupport(0, i0);
                double tmax0 = this.MaxSupport(0, i0);

                for (i1 = 0; i1 < this.quantity[1]; ++i1)
                {
                    this.mIndex[1] = i1;
                    double tmin1 = this.MinSupport(1, i1);
                    double tmax1 = this.MaxSupport(1, i1);

                    double[] interval0 = {tmin0, tmax0};
                    double[] interval1 = {tmin1, tmax1};
                    FIQueryIntervals result = new FIQueryIntervals(interval0, interval1);
                    if (result.NumIntersections == 2)
                    {
                        value = Integration.Romberg(8, result.Overlap[0], result.Overlap[1], integrand);

                        B[i0, i1] = value;
                    }
                    else
                    {
                        B[i0, i1] = 0.0;
                    }
                }
            }

            // Construct A^{-1}*B.
            GMatrix prod = invA * B;

            // Allocate output control points.
            // Construct the control points for the least-squares curve.
            this.controlData = new Vector3[numControls];
            for (i0 = 0; i0 < this.quantity[0]; ++i0)
            {
                for (i1 = 0; i1 < this.quantity[1]; ++i1)
                {
                    this.controlData[i0] += inControls[i1] * prod[i0, i1];
                }
            }

            // TRANSLATION NOTE
            // In the original code the first and last controls are not set to the same position of the sample data
            // but when reducing the control points of a curve I expect that
            // the first and last controls are exactly int the same positions as the first and last controls of the original curve
            // A similar situation we have with the BSplineFit class but vice versa.
            // The original code sets the first and last points to be the same as the original control points,
            // but in this case I do not expect them to be the same.
            // END

            // Set the first and last output control points to match the first
            // and last input samples.  This supports the application of
            // reducing keyframe data with B-spline curves.  The user expects
            // that the curve passes through the first and last positions in
            // order to support matching two consecutive keyframe sequences.
            this.controlData[0] = inControls[0];
            this.controlData[this.controlData.Length - 1] = inControls[inControls.Length - 1];

            BasisFunctionInput input = new BasisFunctionInput(numControls, degree);
            this.basisFunction = new BasisFunction(input);
        }

        // Access to input sample information.
        public int NumSamples
        {
            get { return this.quantity[1]; }
        }

        public Vector3[] SampleData
        {
            get { return this.sampleData; }
        }

        // Access to output control point and curve information.
        public int Degree
        {
            get { return this.degree; }
        }

        public int NumControls
        {
            get { return this.quantity[0]; }
        }

        public Vector3[] ControlData
        {
            get { return this.controlData; }
        }

        public BasisFunction BasisFunction
        {
            get { return this.basisFunction; }
        }

        private double MinSupport(int basis, int i)
        {
            return this.knot[basis][i];
        }

        private double MaxSupport(int basis, int i)
        {
            return this.knot[basis][i + 1 + this.degree];
        }

        private double F(int basis, int i, int j, double t)
        {
            if (j > 0)
            {
                double result = 0.0;
                double denom = this.knot[basis][i + j] - this.knot[basis][i];
                if (denom > 0.0)
                {
                    result += (t - this.knot[basis][i]) * this.F(basis, i, j - 1, t) / denom;
                }

                denom = this.knot[basis][i + j + 1] - this.knot[basis][i + 1];
                if (denom > 0.0)
                {
                    result += (this.knot[basis][i + j + 1] - t) * this.F(basis, i + 1, j - 1, t) / denom;
                }

                return result;
            }

            if (this.knot[basis][i] <= t && t < this.knot[basis][i + 1])
            {
                return 1.0;
            }

            return 0.0;
        }
    }
}