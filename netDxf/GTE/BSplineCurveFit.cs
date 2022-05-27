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

using System.Diagnostics;

namespace netDxf.GTE
{
    // The algorithm implemented here is based on the document
    // https://www.geometrictools.com/Documentation/BSplineCurveLeastSquaresFit.pdf
    public class BSplineCurveFit
    {
        // Input sample information.
        private readonly int numSamples;
        private readonly Vector3[] sampleData;

        // The fitted B-spline curve, open and with uniform knots.
        private readonly int degree;
        private readonly int numControls;
        private readonly Vector3[] controlData;
        private readonly BasisFunction basisFunction;

        // Construction.  The preconditions for calling the constructor are
        // 1 <= degree && degree < numControls <= numSamples - degree - 1.
        // The samples points are contiguous blocks of 'dimension' double values
        // stored in sampleData.
        public BSplineCurveFit(Vector3[] sampleData, int degree, int numControls)
        {
            this.numSamples = sampleData.Length;
            this.sampleData = sampleData;
            this.degree = degree;
            this.numControls = numControls;
            this.controlData = new Vector3[numControls];

            Debug.Assert(1 <= degree && degree < numControls, "Invalid degree.");
            Debug.Assert(sampleData != null, "Invalid sample data.");
            Debug.Assert(numControls <= this.numSamples - degree - 1, "Invalid number of controls.");

            BasisFunctionInput input = new BasisFunctionInput();
            input.NumControls = numControls;
            input.Degree = degree;
            input.Uniform = true;
            input.Periodic = false;
            input.NumUniqueKnots = numControls - degree + 1;
            input.UniqueKnots = new UniqueKnot[input.NumUniqueKnots];
            input.UniqueKnots[0].T = 0.0;
            input.UniqueKnots[0].Multiplicity = degree + 1;

            int last = input.NumUniqueKnots - 1;
            double factor = 1.0 / last;
            for (int i = 1; i < last; ++i)
            {
                input.UniqueKnots[i].T = factor * i;
                input.UniqueKnots[i].Multiplicity = 1;
            }
            input.UniqueKnots[last].T = 1.0;
            input.UniqueKnots[last].Multiplicity = degree + 1;
            this.basisFunction = new BasisFunction(input);

            // Fit the data points with a B-spline curve using a least-squares
            // error metric.  The problem is of the form A^T*A*Q = A^T*P,
            // where A^T*A is a banded matrix, P contains the sample data, and
            // Q is the unknown vector of control points.
            double tMultiplier = 1.0 / (this.numSamples - 1.0);
            double t;
            int i0, i1, i2, imin, imax;

            // Construct the matrix A^T*A.
            int degp1 = this.degree + 1;
            int numBands = this.numControls > degp1 ? degp1 : this.degree;
            BandedMatrix ATAMat = new BandedMatrix(this.numControls, numBands, numBands);
            for (i0 = 0; i0 < this.numControls; i0++)
            {
                for (i1 = 0; i1 < i0; i1++)
                {
                    ATAMat[i0, i1] = ATAMat[i1, i0];
                }

                int i1Max = i0 + this.degree;
                if (i1Max >= this.numControls)
                {
                    i1Max = this.numControls - 1;
                }

                for (i1 = i0; i1 <= i1Max; i1++)
                {
                    double value = 0.0;
                    for (i2 = 0; i2 < this.numSamples; i2++)
                    {
                        t = tMultiplier * i2;
                        this.basisFunction.Evaluate(t, 0, out imin, out imax);
                        if (imin <= i0 && i0 <= imax && imin <= i1 && i1 <= imax)
                        {
                            double b0 = this.basisFunction.GetValue(0, i0);
                            double b1 = this.basisFunction.GetValue(0, i1);
                            value += b0 * b1;
                        }
                    }
                    ATAMat[i0, i1] = value;
                }
            }

            // Construct the matrix A^T.
            double[] ATMat = new double[this.numControls * this.numSamples];

            for (i0 = 0; i0 < this.numControls; i0++)
            {
                for (i1 = 0; i1 < this.numSamples; i1++)
                {
                    t = tMultiplier * i1;
                    this.basisFunction.Evaluate(t, 0, out imin, out imax);
                    if (imin <= i0 && i0 <= imax)
                    {
                        ATMat[i0 * this.numSamples + i1] = this.basisFunction.GetValue(0, i0);
                    }
                }
            }

            // Compute X0 = (A^T*A)^{-1}*A^T by solving the linear system
            // A^T*A*X = A^T.
            bool solved = ATAMat.SolveSystem(ref ATMat, this.numSamples);
            Debug.Assert(solved, "Failed to solve linear system.");

            // The control points for the fitted curve are stored in the
            // vector Q = X0*P, where P is the vector of sample data.
            for (i0 = 0; i0 < this.numControls; i0++)
            {
                Vector3 Q = this.controlData[i0];
                for (i1 = 0; i1 < this.numSamples; i1++)
                {
                    Vector3 P = this.sampleData[i1];
                    double xValue = ATMat[i0 * this.numSamples + i1];
                    Q += xValue * P;
                }

                this.controlData[i0] = Q;
            }

            // TRANSLATION NOTE
            // In the original code the first and last controls are set to the same position of the sample data
            // but the purpose of fitting a curve that averages the points of the sample data I do not expect that
            // the first and last controls are exactly int the same positions as the first and last points of the sample data
            // A similar situation we have with the BSplineReduction class but vice versa.
            // The original code do not set the first and last points to be the same as the original control points,
            // but in this case I expect them to be the same.
            // END

            // Set the first and last output control points to match the first
            // and last input samples.  This supports the application of
            // fitting keyframe data with B-spline curves.  The user expects
            // that the curve passes through the first and last positions in
            // order to support matching two consecutive keyframe sequences.
            //this.controlData[0] = this.sampleData[0];
            //this.controlData[this.controlData.Length - 1] = this.sampleData[this.sampleData.Length - 1];
        }

        // Access to input sample information.
        public int NumSamples
        {
            get { return this.numSamples; }
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
            get { return this.numControls; }
        }

        public Vector3[] ControlData
        {
            get { return this.controlData; }
        }

        public BasisFunction BasisFunction
        {
            get { return this.basisFunction; }
        }

        // Evaluation of the B-spline curve.  It is defined for 0 <= t <= 1.
        // If a t-value is outside [0,1], an open spline clamps it to [0,1].
        // The caller must ensure that position[] has at least 'dimension'
        // elements.
        public void Evaluate(double t, int order, out Vector3 value)
        {
            this.basisFunction.Evaluate(t, order, out int imin, out int imax);

            Vector3 source = this.controlData[imin];
            double basisValue = this.basisFunction.GetValue(order, imin);
            value = basisValue * source;
            
            for (int i = imin + 1; i <= imax; i++)
            {
                source = this.controlData[i];
                basisValue = this.basisFunction.GetValue(order, i);
                value += basisValue * source;
            }
        }

        public Vector3 GetPosition(double t)
        {
            this.Evaluate(t, 0, out Vector3 position);
            return position;
        }
    };
}
