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
    public class BSplineSurface
        : ParametricSurface
    {
        private readonly BasisFunction[] basisFunctions;
        private readonly int[] numControls;
        private readonly Vector3[] controls;

        // Construction.  If the input controls is non-null, a copy is made of
        // the controls.  To defer setting the control points, pass a null
        // pointer and later access the control points via GetControls() or
        // SetControl() member functions.  The input 'controls' must be stored
        // in row-major order, control[i0 + numControls0*i1].  As a 2D array,
        // this corresponds to control2D[i1][i0].
        public BSplineSurface(BasisFunctionInput input0, BasisFunctionInput input1, Vector3[] controls)
            : base(0.0, 1.0, 0.0, 1.0, true)
        {
            this.basisFunctions = new[] {new BasisFunction(input0), new BasisFunction(input1)};
            this.numControls = new[] {input0.NumControls, input1.NumControls};

            // The mBasisFunction stores the domain but so does ParametricCurve.
            this.uMin = this.basisFunctions[0].MinDomain;
            this.uMax = this.basisFunctions[0].MaxDomain;
            this.vMin = this.basisFunctions[1].MinDomain;
            this.vMax = this.basisFunctions[1].MaxDomain;

            // The replication of control points for periodic splines is
            // avoided by wrapping the i-loop index in Evaluate.
            this.controls = new Vector3[this.numControls[0] * this.numControls[1]];
            if (controls != null)
            {
                controls.CopyTo(this.controls, 0);
            }

            this.isConstructed = true;
        }

        // Member access.  The index 'dim' must be in {0,1}.
        public BasisFunction BasisFunction(int dim)
        {
            return this.basisFunctions[dim];
        }

        public int NumControls(int dim)
        {
            return this.numControls[dim];
        }

        public Vector3[] Controls()
        {
            return this.controls;
        }

        public void SetControl(int i0, int i1, Vector3 control)
        {
            if (0 <= i0 && i0 < this.NumControls(0) && 0 <= i1 && i1 < this.NumControls(1))
            {
                this.controls[i0 + this.numControls[0] * i1] = control;
            }
        }

        public Vector3 GetControl(int i0, int i1)
        {
            if (0 <= i0 && i0 < this.NumControls(0) && 0 <= i1 && i1 < this.NumControls(1))
            {
                return this.controls[i0 + this.numControls[0] * i1];
            }

            return this.controls[0];
        }

        // Evaluation of the surface. The function supports derivative
        // calculation through order 2; that is, order <= 2 is required. If
        // you want only the position, pass in order of 0. If you want the
        // position and first-order derivatives, pass in order of 1, and so
        // on. The output array 'jet' must have enough storage to support the
        // maximum order. The values are ordered as: position X; first-order
        // derivatives dX/du, dX/dv; second-order derivatives d2X/du2,
        // d2X/dudv, d2X/dv2.
        // u and v [0,1]
        public override void Evaluate(double u, double v, int order, out Vector3[] jet)
        {
            int supOrder = SUP_ORDER;
            jet = new Vector3[supOrder];

            if (!this.isConstructed || order >= supOrder)
            {
                // Return a zero-valued jet for invalid state.
                return;
            }

            this.basisFunctions[0].Evaluate(u, order, out int iumin, out int iumax);
            this.basisFunctions[1].Evaluate(v, order, out int ivmin, out int ivmax);

            // Compute position.
            jet[0] = this.Compute(0, 0, iumin, iumax, ivmin, ivmax);
            if (order >= 1)
            {
                // Compute first-order derivatives.
                jet[1] = this.Compute(1, 0, iumin, iumax, ivmin, ivmax);
                jet[2] = this.Compute(0, 1, iumin, iumax, ivmin, ivmax);
                if (order >= 2)
                {
                    // Compute second-order derivatives.
                    jet[3] = this.Compute(2, 0, iumin, iumax, ivmin, ivmax);
                    jet[4] = this.Compute(1, 1, iumin, iumax, ivmin, ivmax);
                    jet[5] = this.Compute(0, 2, iumin, iumax, ivmin, ivmax);
                }
            }
        }

        // Support for Evaluate(...).
        private Vector3 Compute(int uOrder, int vOrder, int iumin, int iumax, int ivmin, int ivmax)
        {
            // The j*-indices introduce a tiny amount of overhead in order to
            // handle both aperiodic and periodic splines.  For aperiodic
            // splines, j* = i* always.

            int numControls0 = this.numControls[0];
            int numControls1 = this.numControls[1];
            Vector3 result = Vector3.Zero;
            for (int iv = ivmin; iv <= ivmax; iv++)
            {
                double tmpv = this.basisFunctions[1].GetValue(vOrder, iv);
                int jv = iv >= numControls1 ? iv - numControls1 : iv;
                for (int iu = iumin; iu <= iumax; iu++)
                {
                    double tmpu = this.basisFunctions[0].GetValue(uOrder, iu);
                    int ju = iu >= numControls0 ? iu - numControls0 : iu;
                    result += tmpu * tmpv * this.controls[ju + numControls0 * jv];
                }
            }

            return result;
        }
    }
}