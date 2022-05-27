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
    public abstract class ParametricSurface
    {
        protected double uMin, uMax, vMin, vMax;
        protected bool isRectangular;
        protected bool isConstructed;

        // Abstract base class for a parameterized surface X(u,v).  The
        // parametric domain is either rectangular or triangular.  Valid
        // (u,v) values for a rectangular domain satisfy
        //   umin <= u <= umax,  vmin <= v <= vmax
        // and valid (u,v) values for a triangular domain satisfy
        //   umin <= u <= umax,  vmin <= v <= vmax,
        //   (vmax-vmin)*(u-umin)+(umax-umin)*(v-vmax) <= 0
        protected ParametricSurface(double umin, double umax, double vmin, double vmax, bool isRectangular)
        {
            this.uMin = umin;
            this.uMax = umax;
            this.vMin = vmin;
            this.vMax = vmax;
            this.isRectangular = isRectangular;
            this.isConstructed = false;
        }

        // Member access.

        // To validate construction, create an object as shown:
        //     DerivedClassSurface<Real> surface(parameters);
        //     if (!surface) { <constructor failed, handle accordingly>; }
        public bool IsConstructed
        {
            get { return this.isConstructed; }
        }

        public double UMin
        {
            get { return this.uMin; }
        }

        public double UMax
        {
            get { return this.uMax; }
        }

        public double VMin
        {
            get { return this.vMin; }
        }

        public double VMax
        {
            get { return this.vMax; }
        }

        public bool IsIsRectangular
        {
            get { return this.isRectangular; }
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