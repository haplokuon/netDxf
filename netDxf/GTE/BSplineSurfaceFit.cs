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
    // https://www.geometrictools.com/Documentation/BSplineSurfaceLeastSquaresFit.pdf

    public class BSplineSurfaceFit
    {
        // Input sample information.
        private readonly int[] numSamples;
        private readonly Vector3[] sampleData;

        // The fitted B-spline surface, open and with uniform knots.
        private readonly int[] degree;
        private readonly int[] numControls;
        private readonly Vector3[] controlData;
        private readonly BasisFunction[] basisFunctions;

        // Construction.  The preconditions for calling the constructor are
        //   1 <= degree0 && degree0 + 1 < numControls0 <= numSamples0
        //   1 <= degree1 && degree1 + 1 < numControls1 <= numSamples1
        // The sample data must be in row-major order.  The control data is
        // also stored in row-major order.
        public BSplineSurfaceFit(int degree0, int numControls0, int numSamples0, int degree1, int numControls1, int numSamples1, Vector3[] sampleData)
        {
            Debug.Assert(1 <= degree0 && degree0 + 1 < numControls0, "Invalid degree.");
            Debug.Assert(numControls0 <= numSamples0, "Invalid number of controls.");
            Debug.Assert(1 <= degree1 && degree1 + 1 < numControls1, "Invalid degree.");
            Debug.Assert(numControls1 <= numSamples1, "Invalid number of controls.");
            Debug.Assert(sampleData != null, "Invalid sample data.");

            this.sampleData = sampleData;
            this.controlData = new Vector3[numControls0 * numControls1];
            this.degree = new[] {degree0, degree1};
            this.numSamples = new[] {numSamples0, numSamples1};
            this.numControls = new[] {numControls0, numControls1};
            this.basisFunctions = new BasisFunction[2];

            BasisFunctionInput input = new BasisFunctionInput();
            double[] tMultiplier = new double[2];
            int dim;
            for (dim = 0; dim < 2; dim++)
            {
                input.NumControls = this.numControls[dim];
                input.Degree = this.degree[dim];
                input.Uniform = true;
                input.Periodic = false;
                input.NumUniqueKnots = this.numControls[dim] - this.degree[dim] + 1;
                input.UniqueKnots = new UniqueKnot[input.NumUniqueKnots];
                input.UniqueKnots[0].T = 0.0;
                input.UniqueKnots[0].Multiplicity = this.degree[dim] + 1;
                int last = input.NumUniqueKnots - 1;
                double factor = 1.0 / last;
                for (int i = 1; i < last; i++)
                {
                    input.UniqueKnots[i].T = factor * i;
                    input.UniqueKnots[i].Multiplicity = 1;
                }

                input.UniqueKnots[last].T = 1.0;
                input.UniqueKnots[last].Multiplicity = this.degree[dim] + 1;
                tMultiplier[dim] = 1.0 / (this.numSamples[dim] - 1.0);

                this.basisFunctions[dim] = new BasisFunction(input);
            }

            // Fit the data points with a B-spline surface using a
            // least-squares error metric.  The problem is of the form
            // A0^T*A0*Q*A1^T*A1 = A0^T*P*A1, where A0^T*A0 and A1^T*A1 are
            // banded matrices, P contains the sample data, and Q is the
            // unknown matrix of control points.
            double t;
            int i0, i1, i2, imin, imax;

            // Construct the matrices A0^T*A0 and A1^T*A1.
            BandedMatrix[] ATAMat =
            {
                new BandedMatrix(this.numControls[0], this.degree[0] + 1, this.degree[0] + 1),
                new BandedMatrix(this.numControls[1], this.degree[1] + 1, this.degree[1] + 1)
            };

            for (dim = 0; dim < 2; dim++)
            {
                for (i0 = 0; i0 < this.numControls[dim]; i0++)
                {
                    for (i1 = 0; i1 < i0; i1++)
                    {
                        ATAMat[dim][i0, i1] = ATAMat[dim][i1, i0];
                    }

                    int i1Max = i0 + this.degree[dim];
                    if (i1Max >= this.numControls[dim])
                    {
                        i1Max = this.numControls[dim] - 1;
                    }

                    for (i1 = i0; i1 <= i1Max; i1++)
                    {
                        double value = 0;
                        for (i2 = 0; i2 < this.numSamples[dim]; i2++)
                        {
                            t = tMultiplier[dim] * i2;
                            this.basisFunctions[dim].Evaluate(t, 0, out imin, out imax);
                            if (imin <= i0 && i0 <= imax && imin <= i1 && i1 <= imax)
                            {
                                double b0 = this.basisFunctions[dim].GetValue(0, i0);
                                double b1 = this.basisFunctions[dim].GetValue(0, i1);
                                value += b0 * b1;
                            }
                        }

                        ATAMat[dim][i0, i1] = value;
                    }
                }
            }

            // Construct the matrices A0^T and A1^T.  A[d]^T has
            // mNumControls[d] rows and mNumSamples[d] columns.
            double[][] ATMat = new double[2][];
            for (dim = 0; dim < 2; dim++)
            {
                ATMat[dim] = new double[this.numControls[dim] * this.numSamples[dim]];
                for (i0 = 0; i0 < this.numControls[dim]; i0++)
                {
                    for (i1 = 0; i1 < this.numSamples[dim]; i1++)
                    {
                        t = tMultiplier[dim] * i1;
                        this.basisFunctions[dim].Evaluate(t, 0, out imin, out imax);
                        if (imin <= i0 && i0 <= imax)
                        {
                            ATMat[dim][i0 * this.numSamples[dim] + i1] = this.basisFunctions[dim].GetValue(0, i0);
                        }
                    }
                }
            }

            // Compute X0 = (A0^T*A0)^{-1}*A0^T and X1 = (A1^T*A1)^{-1}*A1^T
            // by solving the linear systems A0^T*A0*X0 = A0^T and
            // A1^T*A1*X1 = A1^T.
            for (dim = 0; dim < 2; dim++)
            {
                bool solved = ATAMat[dim].SolveSystem(ref ATMat[dim], this.numSamples[dim]);
                Debug.Assert(solved, "Failed to solve linear system in BSplineSurfaceFit constructor.");
            }

            // The control points for the fitted surface are stored in the matrix
            // Q = X0*P*X1^T, where P is the matrix of sample data.
            for (i1 = 0; i1 < this.numControls[1]; i1++)
            {
                for (i0 = 0; i0 < this.numControls[0]; i0++)
                {
                    Vector3 sum = Vector3.Zero;
                    for (int j1 = 0; j1 < this.numSamples[1]; j1++)
                    {
                        double x1Value = ATMat[1][i1 * this.numSamples[1] + j1];
                        for (int j0 = 0; j0 < this.numSamples[0]; j0++)
                        {
                            double x0Value = ATMat[0][i0 * this.numSamples[0] + j0];
                            Vector3 sample = this.sampleData[j0 + this.numSamples[0] * j1];
                            sum += x0Value * x1Value * sample;
                        }
                    }

                    this.controlData[i0 + this.numControls[0] * i1] = sum;
                }
            }
        }

        // Access to input sample information.
        public int NumSamples(int dimension)
        {
            return this.numSamples[dimension];
        }

        public Vector3[] SampleData
        {
            get { return this.sampleData; }
        }

        // Access to output control point and surface information.
        public int Degree(int dimension)
        {
            return this.degree[dimension];
        }

        public int NumControls(int dimension)
        {
            return this.numControls[dimension];
        }

        public Vector3[] ControlData
        {
            get { return this.controlData; }
        }

        public BasisFunction BasisFunction(int dimension)
        {
            return this.basisFunctions[dimension];
        }

        // Evaluation of the B-spline surface.  It is defined for
        // 0 <= u <= 1 and 0 <= v <= 1.  If a parameter value is outside
        // [0,1], it is clamped to [0,1].
        public Vector3 GetPosition(double u, double v)
        {
            this.basisFunctions[0].Evaluate(u, 0, out int iumin, out int iumax);
            this.basisFunctions[1].Evaluate(v, 0, out int ivmin, out int ivmax);

            Vector3 position = Vector3.Zero;
            for (int iv = ivmin; iv <= ivmax; iv++)
            {
                double value1 = this.basisFunctions[1].GetValue(0, iv);
                for (int iu = iumin; iu <= iumax; iu++)
                {
                    double value0 = this.basisFunctions[0].GetValue(0, iu);
                    Vector3 control = this.controlData[iu + this.numControls[0] * iv];
                    position += value0 * value1 * control;
                }
            }

            return position;
        }
    }
}