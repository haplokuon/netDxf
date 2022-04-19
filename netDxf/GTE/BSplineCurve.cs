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
    public class BSplineCurve :
        ParametricCurve
    {
        private readonly BasisFunction basisFunction;
        private readonly Vector3[] controls;

        // Construction.  If the input controls is non-null, a copy is made of
        // the controls.  To defer setting the control points, pass a null
        // pointer and later access the control points via GetControls() or
        // SetControl() member functions.  The domain is t in [t[d],t[n]],
        // where t[d] and t[n] are knots with d the degree and n the number of
        // control points.
        public BSplineCurve(BasisFunctionInput input, Vector3[] controls)
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

        // Evaluation of the curve.  The function supports derivative
        // calculation through order 3; that is, order <= 3 is required.  If
        // you want/ only the position, pass in order of 0.  If you want the
        // position and first derivative, pass in order of 1, and so on.  The
        // output array 'jet' must have enough storage to support the maximum
        // order.  The values are ordered as: position, first derivative,
        // second derivative, third derivative.
        public override void Evaluate(double t, int order, out Vector3[] jet)
        {
            int supOrder = SUP_ORDER;
            jet = new Vector3[supOrder];

            if (!this.isConstructed || order >= supOrder)
            {
                // Return a zero-valued jet for invalid state.
                return;
            }

            this.basisFunction.Evaluate(t, order, out int imin, out int imax);

            // Compute position.
            jet[0] = this.Compute(0, imin, imax);
            if (order >= 1)
            {
                // Compute first derivative.
                jet[1] = this.Compute(1, imin, imax);
                if (order >= 2)
                {
                    // Compute second derivative.
                    jet[2] = this.Compute(2, imin, imax);
                    if (order == 3)
                    {
                        jet[3] = this.Compute(3, imin, imax);
                    }
                }
            }
        }

        // Support for Evaluate(...).
        private Vector3 Compute(int order, int imin, int imax)
        {
            // The j-index introduces a tiny amount of overhead in order to handle
            // both aperiodic and periodic splines.  For aperiodic splines, j = i
            // always.

            int numControls = this.NumControls;
            Vector3 result = Vector3.Zero;
            for (int i = imin; i <= imax; i++)
            {
                double tmp = this.basisFunction.GetValue(order, i);
                int j = i >= numControls ? i - numControls : i;
                result += tmp * this.controls[j];
            }
            return result;
        }
    };
}