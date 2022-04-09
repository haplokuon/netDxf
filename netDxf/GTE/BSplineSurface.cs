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
    internal class BSplineSurface
        : ParametricSurface
    {
        private BasisFunction[] mBasisFunction = new BasisFunction[2];
        private int[] mNumControls = new int[2];
        private Vector3[] mControls;

        // Construction.  If the input controls is non-null, a copy is made of
        // the controls.  To defer setting the control points, pass a null
        // pointer and later access the control points via GetControls() or
        // SetControl() member functions.  The input 'controls' must be stored
        // in row-major order, control[i0 + numControls0*i1].  As a 2D array,
        // this corresponds to control2D[i1][i0].
        public BSplineSurface(BasisFunctionInput[] input, Vector3[] controls)
            : base(0, 1, 0, 1, true)
        {
            for (int i = 0; i < 2; ++i)
            {
                this.mNumControls[i] = input[i].NumControls;
                this.mBasisFunction[i] = new BasisFunction(input[i]);
            }

            // The mBasisFunction stores the domain but so does
            // ParametricCurve.
            this.mUMin = this.mBasisFunction[0].MinDomain;
            this.mUMax = this.mBasisFunction[0].MaxDomain;
            this.mVMin = this.mBasisFunction[1].MinDomain;
            this.mVMax = this.mBasisFunction[1].MaxDomain;

            // The replication of control points for periodic splines is
            // avoided by wrapping the i-loop index in Evaluate.
            int numControls = this.mNumControls[0] * this.mNumControls[1];
            this.mControls = new Vector3[numControls];
            if (controls != null)
            {
                controls.CopyTo(this.mControls, 0);
            }

            this.mConstructed = true;
        }

        // Member access.  The index 'dim' must be in {0,1}.
        public BasisFunction GetBasisFunction(int dim)
        {
            return this.mBasisFunction[dim];
        }

        public int GetNumControls(int dim)
        {
            return this.mNumControls[dim];
        }

        public Vector3[] GetControls()
        {
            return this.mControls;
        }

        public void SetControl(int i0, int i1, in Vector3 control)
        {
            if (0 <= i0 && i0 < this.GetNumControls(0) && 0 <= i1 && i1 < this.GetNumControls(1))
            {
                this.mControls[i0 + this.mNumControls[0] * i1] = control;
            }
        }

        public Vector3 GetControl(int i0, int i1)
        {
            if (0 <= i0 && i0 < this.GetNumControls(0) && 0 <= i1 && i1 < this.GetNumControls(1))
            {
                return this.mControls[i0 + this.mNumControls[0] * i1];
            }
            else
            {
                return this.mControls[0];
            }
        }

        // Evaluation of the surface.  The function supports derivative
        // calculation through order 2; that is, order <= 2 is required.  If
        // you want only the position, pass in order of 0.  If you want the
        // position and first-order derivatives, pass in order of 1, and so
        // on.  The output array 'jet' must have enough storage to support the
        // maximum order.  The values are ordered as: position X; first-order
        // derivatives dX/du, dX/dv; second-order derivatives d2X/du2,
        // d2X/dudv, d2X/dv2.
        // u and v [0,1]
        public override void Evaluate(double u, double v, int order, out Vector3[] jet)
        {
            int supOrder = SUP_ORDER;
            jet = new Vector3[supOrder];

            if (!this.mConstructed || order >= supOrder)
            {
                // Return a zero-valued jet for invalid state.
                for (int i = 0; i < supOrder; i++)
                {
                    jet[i] = Vector3.Zero;
                }

                return;
            }

            this.mBasisFunction[0].Evaluate(u, order, out int iumin, out int iumax);
            this.mBasisFunction[1].Evaluate(v, order, out int ivmin, out int ivmax);

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

            int numControls0 = this.mNumControls[0];
            int numControls1 = this.mNumControls[1];
            Vector3 result = Vector3.Zero;
            for (int iv = ivmin; iv <= ivmax; iv++)
            {
                double tmpv = this.mBasisFunction[1].GetValue(vOrder, iv);
                int jv = iv >= numControls1 ? iv - numControls1 : iv;
                for (int iu = iumin; iu <= iumax; iu++)
                {
                    double tmpu = this.mBasisFunction[0].GetValue(uOrder, iu);
                    int ju = iu >= numControls0 ? iu - numControls0 : iu;
                    result += tmpu * tmpv * this.mControls[ju + numControls0 * jv];
                }
            }

            return result;
        }
    }
}