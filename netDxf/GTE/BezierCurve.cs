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
    public class BezierCurve :
        ParametricCurve
    {
        private readonly int degree;
        private readonly int numControls;
        private readonly Vector3[][] controls;
        private readonly double[][] choose;

        // Construction and destruction.  The number of control points must be
        // degree + 1.  This object copies the input array.  The domain is t
        // in [0,1].  To validate construction, create an object as shown:
        //     BezierCurve<N, Real> curve(parameters);
        //     if (!curve) { <constructor failed, handle accordingly>; }
        public BezierCurve(Vector3[] controls, int degree)
            : base(0.0, 1.0)
        {
            Debug.Assert(degree >= 2 && controls != null, "Invalid input.");

            this.degree = degree;
            this.numControls = degree + 1;
            this.choose = new double[this.NumControls][];

            // Copy the controls.
            this.controls = new Vector3[SUP_ORDER][];
            this.controls[0] = new Vector3[this.NumControls];
            controls.CopyTo(this.controls[0], 0);

            // Compute first-order differences.
            this.controls[1] = new Vector3[this.numControls - 1];
            for (int i = 0, ip1 = 1; ip1 < this.numControls; i++, ip1++)
            {
                this.controls[1][i] = this.controls[0][ip1] - this.controls[0][i];
            }

            // Compute second-order differences.
            this.controls[2] = new Vector3[this.numControls - 2];
            for (int i = 0, ip1 = 1, ip2 = 2; ip2 < this.numControls; i++, ip1++, ip2++)
            {
                this.controls[2][i] = this.controls[1][ip1] - this.controls[1][i];
            }

            // Compute third-order differences.
            if (degree >= 3)
            {
                this.controls[3] = new Vector3[this.numControls - 3];
                for (int i = 0, ip1 = 1, ip3 = 3; ip3 < this.numControls; i++, ip1++, ip3++)
                {
                    this.controls[3][i] = this.controls[2][ip1] - this.controls[2][i];
                }
            }

            // Compute combinatorial values Choose(n,k) and store in mChoose[n][k].
            // The values mChoose[r][c] are invalid for r < c; that is, we use only
            // the entries for r >= c.
            this.choose[0] = new[] {1.0};
            this.choose[1] = new[] {1.0, 1.0};
            for (int i = 2; i <= this.degree; i++)
            {
                this.choose[i] = new double[i + 1];
                this.choose[i][0] = 1.0;
                this.choose[i][i] = 1.0;
                for (int j = 1; j < i; j++)
                {
                    this.choose[i][j] = this.choose[i - 1][j - 1] + this.choose[i - 1][j];
                }
            }

            this.isConstructed = true;
        }

        // Member access.
        public int Degree
        {
            get { return this.degree; }
        }

        public int NumControls
        {
            get { return this.numControls; }
        }

        public Vector3[] Controls
        {
            get { return this.controls[0]; }
        }

        // Evaluation of the curve.  The function supports derivative
        // calculation through order 3; that is, order <= 3 is required.  If
        // you want/ only the position, pass in order of 0.  If you want the
        // position and first derivative, pass in order of 1, and so on.  The
        // output array 'jet' must have enough storage to support the maximum
        // order.  The values are ordered as: position, first derivative,
        // second derivative, third derivative.
        public override void Evaluate(double t, int order, out Vector3[] jet)
        {
            const int supOrder = SUP_ORDER;
            jet = new Vector3[supOrder];

            if (!this.isConstructed || order >= SUP_ORDER)
            {
                // Return a zero-valued jet for invalid state.
                return;
            }

            // Compute position.
            double omt = 1.0 - t;
            jet[0] = this.Compute(t, omt, 0);
            if (order >= 1)
            {
                // Compute first derivative.
                jet[1] = this.Compute(t, omt, 1);
                if (order >= 2)
                {
                    // Compute second derivative.
                    jet[2] = this.Compute(t, omt, 2);
                    if (order >= 3)
                    {
                        // Compute third derivative.
                        if (this.degree >= 3)
                        {
                            jet[3] = this.Compute(t, omt, 3);
                        }
                        else
                        {
                            jet[3] = Vector3.Zero;
                        }
                    }
                }
            }
        }

        // Support for Evaluate(...).
        protected Vector3 Compute(double t, double omt, int order)
        {
            Vector3 result = omt * this.controls[order][0];

            double tpow = t;
            int isup = this.degree - order;
            for (int i = 1; i < isup; i++)
            {
                double c = this.choose[isup][i] * tpow;
                result = (result + c * this.controls[order][i]) * omt;
                tpow *= t;
            }
            result = result + tpow * this.controls[order][isup];

            int multiplier = 1;
            for (int i = 0; i < order; i++)
            {
                multiplier *= this.degree - i;
            }
            result *= multiplier;

            return result;
        }

    };
}
