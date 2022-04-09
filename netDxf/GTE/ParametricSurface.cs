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
    internal abstract class ParametricSurface
    {
        protected double mUMin, mUMax, mVMin, mVMax;
        protected bool mRectangular;
        protected bool mConstructed;

        // Abstract base class for a parameterized surface X(u,v).  The
        // parametric domain is either rectangular or triangular.  Valid
        // (u,v) values for a rectangular domain satisfy
        //   umin <= u <= umax,  vmin <= v <= vmax
        // and valid (u,v) values for a triangular domain satisfy
        //   umin <= u <= umax,  vmin <= v <= vmax,
        //   (vmax-vmin)*(u-umin)+(umax-umin)*(v-vmax) <= 0
        protected ParametricSurface(double umin, double umax, double vmin, double vmax, bool rectangular)
        {
            this.mUMin = umin;
            this.mUMax = umax;
            this.mVMin = vmin;
            this.mVMax = vmax;
            this.mRectangular = rectangular;
            this.mConstructed = false;
        }

        // Member access.

        // To validate construction, create an object as shown:
        //     DerivedClassSurface<Real> surface(parameters);
        //     if (!surface) { <constructor failed, handle accordingly>; }
        public bool IsConstructed
        {
            get { return this.mConstructed; }
        }

        public double UMin
        {
            get { return this.mUMin; }
        }

        public double UMax
        {
            get { return this.mUMax; }
        }

        public double VMin
        {
            get { return this.mVMin; }
        }

        public double VMax
        {
            get { return this.mVMax; }
        }

        public bool IsRectangular
        {
            get { return this.mRectangular; }
        }

        // Evaluation of the surface.  The function supports derivative
        // calculation through order 2; that is, order <= 2 is required.  If
        // you want only the position, pass in order of 0.  If you want the
        // position and first-order derivatives, pass in order of 1, and so
        // on.  The output array 'jet' must have enough storage to support the
        // maximum order.  The values are ordered as: position X; first-order
        // derivatives dX/du, dX/dv; second-order derivatives d2X/du2,
        // d2X/dudv, d2X/dv2.
        public const int SUP_ORDER = 6;

        public abstract void Evaluate(double u, double v, int order, out Vector3[] jet);

        // Differential geometric quantities.
        public Vector3 GetPosition(double u, double v)
        {
            this.Evaluate(u, v, 0, out Vector3[] jet);
            return jet[0];
        }

        public Vector3 GetUTangent(double u, double v)
        {
            this.Evaluate(u, v, 1, out Vector3[] jet);
            return Vector3.Normalize(jet[1]);
        }

        public Vector3 GetVTangent(double u, double v)
        {
            this.Evaluate(u, v, 1, out Vector3[] jet);
            return Vector3.Normalize(jet[2]);
        }
    }
}