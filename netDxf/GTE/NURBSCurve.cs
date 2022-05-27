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

namespace netDxf.GTE
{
    public class NURBSCurve :
        ParametricCurve
    {
        private readonly BasisFunction basisFunction;
        private readonly Vector3[] controls;
        private readonly double[] weights;

        // Construction.  If the input controls is non-null, a copy is made of
        // the controls.  To defer setting the control points or weights, pass
        // null pointers and later access the control points or weights via
        // GetControls(), GetWeights(), SetControl(), or SetWeight() member
        // functions.  The domain is t in [t[d],t[n]], where t[d] and t[n] are
        // knots with d the degree and n the number of control points.
        public NURBSCurve(BasisFunctionInput input, Vector3[] controls, double[] weights)
            : base(0.0, 1.0)
        {
            this.basisFunction = new BasisFunction(input);

            // The mBasisFunction stores the domain but so does ParametricCurve.
            this.SetTimeInterval(this.basisFunction.MinDomain, this.basisFunction.MaxDomain);

            // The replication of control points for periodic splines is
            // avoided by wrapping the i-loop index in Evaluate.
            this.controls = new Vector3[input.NumControls];
            if (controls != null)
            {
                controls.CopyTo(this.controls, 0);
            }

            this.weights = new double[input.NumControls];
            if (weights != null)
            {
                weights.CopyTo(this.weights, 0);
            }

            this.isConstructed = true;
        }

        // Member access.
        public BasisFunction BasisFunction
        {
            get { return this.basisFunction; }
        }

        public int NumControls
        {
            get { return this.controls.Length; }
        }

        public Vector3[] Controls
        {
            get { return this.controls; }
        }

        public double[] Weights
        {
            get { return this.weights; }
        }

        public void SetControl(int i, Vector3 control)
        {
            if (0 <= i && i < this.NumControls)
            {
                this.controls[i] = control;
            }
        }

        public Vector3 GetControl(int i)
        {
            if (0 <= i && i < this.NumControls)
            {
                return this.controls[i];
            }

            // Invalid index, return something.
            return this.controls[0];
        }

        public void SetWeight(int i, double weight)
        {
            if (0 <= i && i < this.NumControls)
            {
                this.weights[i] = weight;
            }
        }

        public double GetWeight(int i)
        {
            if (0 <= i && i < this.NumControls)
            {
                return this.weights[i];
            }

            // Invalid index, return something.
            return this.weights[0];
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

            if (!this.isConstructed || order >= supOrder)
            {
                // Return a zero-valued jet for invalid state.
                return;
            }

            this.basisFunction.Evaluate(t, order, out int imin, out int imax);

            // Compute position.
            this.Compute(0, imin, imax, out Vector3 X, out double w);
            double invW = 1.0 / w;
            jet[0] = invW * X;

            if (order >= 1)
            {
                // Compute first derivative.
                this.Compute(1, imin, imax, out Vector3 xDer1, out double wDer1);
                jet[1] = invW * (xDer1 - wDer1 * jet[0]);

                if (order >= 2)
                {
                    // Compute second derivative.
                    this.Compute(2, imin, imax, out Vector3 xDer2, out double wDer2);
                    jet[2] = invW * (xDer2 - 2.0 * wDer1 * jet[1] - wDer2 * jet[0]);

                    if (order == 3)
                    {
                        // Compute third derivative.
                        this.Compute(3, imin, imax, out Vector3 xDer3, out double wDer3);
                        jet[3] = invW * (xDer3 - 3.0 * wDer1 * jet[2] - 3.0 * wDer2 * jet[1] - wDer3 * jet[0]);
                    }
                }
            }
        }

        // Support for Evaluate(...).
        protected void Compute(int order, int imin, int imax, out Vector3 x, out double w)
        {
            // The j-index introduces a tiny amount of overhead in order to
            // handle both aperiodic and periodic splines.  For aperiodic
            // splines, j = i always.

            int numControls = this.NumControls;
            x = Vector3.Zero;
            w = 0.0;
            for (int i = imin; i <= imax; ++i)
            {
                int j = (i >= numControls ? i - numControls : i);
                double tmp = this.basisFunction.GetValue(order, i) * this.weights[j];
                x += tmp * this.controls[j];
                w += tmp;
            }
        }
    };
}

