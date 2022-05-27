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
using System.Collections.Generic;
using System.Linq;

//// FOR INTERNAL USE ONLY (unit testing).  Do not define the symbol
//// GTE_ROOTS_LOW_DEGREE_UNIT_TEST in your own code.
//#if defined(GTE_ROOTS_LOW_DEGREE_UNIT_TEST)
//extern void RootsLowDegreeBlock(int32_t);
//#define GTE_ROOTS_LOW_DEGREE_BLOCK(block) RootsLowDegreeBlock(block)
//#else
//#define GTE_ROOTS_LOW_DEGREE_BLOCK(block)
//#endif

namespace netDxf.GTE
{
    // The Find functions return the number of roots, if any, and this number
    // of elements of the outputs are valid.  If the polynomial is identically
    // zero, Find returns 1.
    //
    // Some root-bounding algorithms for real-valued roots are mentioned next for
    // the polynomial p(t) = c[0] + c[1]*t + ... + c[d-1]*t^{d-1} + c[d]*t^d.
    //
    // 1. The roots must be contained by the interval [-M,M] where
    //   M = 1 + max{|c[0]|, ..., |c[d-1]|}/|c[d]| >= 1
    // is called the Cauchy bound.
    //
    // 2. You may search for roots in the interval [-1,1].  Define
    //   q(t) = t^d*p(1/t) = c[0]*t^d + c[1]*t^{d-1} + ... + c[d-1]*t + c[d]
    // The roots of p(t) not in [-1,1] are the roots of q(t) in [-1,1].
    //
    // 3. Between two consecutive roots of the derivative p'(t), say, r0 < r1,
    // the function p(t) is strictly monotonic on the open interval (r0,r1).
    // If additionally, p(r0) * p(r1) <= 0, then p(x) has a unique root on
    // the closed interval [r0,r1].  Thus, one can compute the derivatives
    // through order d for p(t), find roots for the derivative of order k+1,
    // then use these to bound roots for the derivative of order k.
    //
    // 4. Sturm sequences of polynomials may be used to determine bounds on the
    // roots.  This is a more sophisticated approach to root bounding than item 3.
    // Moreover, a Sturm sequence allows you to compute the number of real-valued
    // roots on a specified interval.
    //
    // 5. For the low-degree Solve* functions, see
    // https://www.geometrictools.com/Documentation/LowDegreePolynomialRoots.pdf

    public static class RootsPolynomial
    {
        // Low-degree root finders.  These use exact rational arithmetic for
        // theoretically correct root classification.  The roots themselves
        // are computed with mixed types (rational and floating-point
        // arithmetic).  The Rational type must support rational arithmetic
        // (+, -, *, /); for example, BSRational<UIntegerAP32> suffices.  The
        // Rational class must have single-input constructors where the input
        // is type Real.  This ensures you can call the Solve* functions with
        // floating-point inputs; they will be converted to Rational
        // implicitly.  The highest-order coefficients must be nonzero
        // (p2 != 0 for quadratic, p3 != 0 for cubic, and p4 != 0 for
        // quartic).
        public static void SolveQuadratic(double p0, double p1, double p2, out SortedDictionary<double, int> rmMap)
        {
            const double rat2 = 2.0;
            double q0 = p0 / p2;
            double q1 = p1 / p2;
            double q1half = q1 / rat2;
            double c0 = q0 - q1half * q1half;

            SortedDictionary<double, int> rmLocalMap = new SortedDictionary<double, int>();
            SolveDepressedQuadratic(c0, ref rmLocalMap);

            rmMap = new SortedDictionary<double, int>();
            foreach (KeyValuePair<double, int> rm in rmLocalMap)
            {
                double root = rm.Key - q1half;
                rmMap.Add(root, rm.Value);
            }
        }

        public static void SolveCubic(double p0, double p1, double p2, double p3, out SortedDictionary<double, int> rmMap)
        {
            const double rat2 = 2.0, rat3 = 3.0;
            double q0 = p0 / p3;
            double q1 = p1 / p3;
            double q2 = p2 / p3;
            double q2third = q2 / rat3;
            double c0 = q0 - q2third * (q1 - rat2 * q2third * q2third);
            double c1 = q1 - q2 * q2third;

            SortedDictionary<double, int> rmLocalMap = new SortedDictionary<double, int>();
            SolveDepressedCubic(c0, c1, ref rmLocalMap);

            rmMap = new SortedDictionary<double, int>();
            foreach (KeyValuePair<double, int> rm in rmLocalMap)
            {
                double root = rm.Key - q2third;
                rmMap.Add(root, rm.Value);
            }
        }

        public static void SolveQuartic(double p0, double p1, double p2, double p3, double p4, out SortedDictionary<double, int> rmMap)
        {
            const double rat2 = 2.0, rat3 = 3.0, rat4 = 4.0, rat6 = 6.0;
            double q0 = p0 / p4;
            double q1 = p1 / p4;
            double q2 = p2 / p4;
            double q3 = p3 / p4;
            double q3fourth = q3 / rat4;
            double q3fourthSqr = q3fourth * q3fourth;
            double c0 = q0 - q3fourth * (q1 - q3fourth * (q2 - q3fourthSqr * rat3));
            double c1 = q1 - rat2 * q3fourth * (q2 - rat4 * q3fourthSqr);
            double c2 = q2 - rat6 * q3fourthSqr;

            SortedDictionary<double, int> rmLocalMap = new SortedDictionary<double, int>();
            SolveDepressedQuartic(c0, c1, c2, ref rmLocalMap);

            rmMap= new SortedDictionary<double, int>();
            foreach (KeyValuePair<double, int> rm in rmLocalMap)
            {
                double root = rm.Key - q3fourth;
                rmMap.Add(root, rm.Value);
            }
        }

        // Return only the number of real-valued roots and their
        // multiplicities.  info.size() is the number of real-valued roots
        // and info[i] is the multiplicity of root corresponding to index i.
        public static void GetRootInfoQuadratic(double p0, double p1, double p2, out List<int> info)
        {
            const double rat2 = 2.0;
            double q0 = p0 / p2;
            double q1 = p1 / p2;
            double q1half = q1 / rat2;
            double c0 = q0 - q1half * q1half;

            info = new List<int>(2);
            GetRootInfoDepressedQuadratic(c0, ref info);
        }

        public static void GetRootInfoCubic(double p0, double p1, double p2, double p3, out List<int> info)
        {
            const double rat2 = 2.0, rat3 = 3.0;
            double q0 = p0 / p3;
            double q1 = p1 / p3;
            double q2 = p2 / p3;
            double q2third = q2 / rat3;
            double c0 = q0 - q2third * (q1 - rat2 * q2third * q2third);
            double c1 = q1 - q2 * q2third;

            info = new List<int>(3);
            GetRootInfoDepressedCubic(c0, c1, ref info);
        }

        public static void GetRootInfoQuartic(double p0, double p1, double p2, double p3, double p4, out List<int> info)
        {
            const double rat2 = 2.0, rat3 = 3.0, rat4 = 4.0, rat6 = 6.0;
            double q0 = p0 / p4;
            double q1 = p1 / p4;
            double q2 = p2 / p4;
            double q3 = p3 / p4;
            double q3fourth = q3 / rat4;
            double q3fourthSqr = q3fourth * q3fourth;
            double c0 = q0 - q3fourth * (q1 - q3fourth * (q2 - q3fourthSqr * rat3));
            double c1 = q1 - rat2 * q3fourth * (q2 - rat4 * q3fourthSqr);
            double c2 = q2 - rat6 * q3fourthSqr;

            info = new List<int>(4);
            GetRootInfoDepressedQuartic(c0, c1, c2, ref info);
        }

        // General equations: sum_{i=0}^{d} c(i)*t^i = 0.  The input array 'c'
        // must have at least d+1 elements and the output array 'root' must
        // have at least d elements.

        // Find the roots on (-infinity,+infinity).
        public static int Find(int degree, double[] c, int maxIterations, out double[] roots)
        {
            roots = new double[degree];

            if (degree >= 0 && c != null)
            {
                double zero = 0.0;
                while (degree >= 0 && Math.Abs(c[degree] - zero) < double.Epsilon)
                {
                    --degree;
                }

                if (degree > 0)
                {
                    // Compute the Cauchy bound.
                    const double one = 1.0;
                    double invLeading = one / c[degree];
                    double maxValue = zero;
                    for (int i = 0; i < degree; ++i)
                    {
                        double value = Math.Abs(c[i] * invLeading);
                        if (value > maxValue)
                        {
                            maxValue = value;
                        }
                    }
                    double bound = one + maxValue;

                    return FindRecursive(degree, c, -bound, bound, maxIterations, ref roots);
                }

                if (degree == 0)
                {
                    // The polynomial is a nonzero constant.
                    return 0;
                }
                
                // The polynomial is identically zero.
                roots[0] = zero;
                return 1;
            }
            
            // Invalid degree or c.
            return 0;
        }

        // If you know that p(tmin) * p(tmax) <= 0, then there must be at
        // least one root in [tmin, tmax].  Compute it using bisection.
        public static bool Find(int degree, double[] c, double tmin, double tmax, int maxIterations, out double root)
        {
            const double zero = 0.0;
            double pmin = Evaluate(degree, c, tmin);
            root = zero; // no root found

            if (Math.Abs(pmin - zero) < double.Epsilon)
            {
                root = tmin;
                return true;
            }
            double pmax = Evaluate(degree, c, tmax);
            if (Math.Abs(pmax - zero) < double.Epsilon)
            {
                root = tmax;
                return true;
            }

            if (pmin * pmax > zero)
            {
                // It is not known whether the interval bounds a root.
                return false;
            }

            if (tmin >= tmax)
            {
                // Invalid ordering of interval endpoints. 
                return false;
            }

            for (int i = 1; i <= maxIterations; i++)
            {
                root = 0.5 * (tmin + tmax);

                // This test is designed for 'float' or 'double' when tmin
                // and tmax are consecutive floating-point numbers.
                //if (root == tmin || root == tmax)
                if (Math.Abs(root - tmin) < double.Epsilon || Math.Abs(root - tmax) < double.Epsilon)
                {
                    break;
                }

                double p = Evaluate(degree, c, root);
                double product = p * pmin;
                if (product < zero)
                {
                    tmax = root;
                    pmax = p;
                }
                else if (product > zero)
                {
                    tmin = root;
                    pmin = p;
                }
                else
                {
                    break;
                }
            }

            return true;
        }

        // Support for the Solve* functions.
        private static void SolveDepressedQuadratic(double c0, ref SortedDictionary<double, int> rmMap)
        {
            const double zero = 0.0;
            if (c0 < zero)
            {
                // Two simple roots.
                double root1 = Math.Sqrt(-c0);
                double root0 = -root1;
                rmMap.Add(root0, 1);
                rmMap.Add(root1, 1);
                //GTE_ROOTS_LOW_DEGREE_BLOCK(0);
            }
            else if (Math.Abs(c0 - zero) < double.Epsilon)
            {
                // One double root.
                rmMap.Add(zero, 2);
                //GTE_ROOTS_LOW_DEGREE_BLOCK(1);
            }
            else  // c0 > 0
            {
                // A complex-conjugate pair of roots.
                // Complex z0 = -q1/2 - i*sqrt(c0);
                // Complex z0conj = -q1/2 + i*sqrt(c0);
                //GTE_ROOTS_LOW_DEGREE_BLOCK(2);
            }
        }

        private static void SolveDepressedCubic(double c0, double c1, ref SortedDictionary<double, int> rmMap)
        {
            // Handle the special case of c0 = 0, in which case the polynomial
            // reduces to a depressed quadratic.
            const double zero = 0;
            if (Math.Abs(c0 - zero) < double.Epsilon)
            {
                SolveDepressedQuadratic(c1, ref rmMap);
                if (rmMap.ContainsKey(zero))
                {
                    // The quadratic has a root of zero, so the multiplicity
                    // must be increased.
                    rmMap[zero] += 1;
                    //GTE_ROOTS_LOW_DEGREE_BLOCK(3);
                }
                else
                {
                    // The quadratic does not have a root of zero.  Insert the
                    // one for the cubic.
                    rmMap.Add(zero, 1);
                    //GTE_ROOTS_LOW_DEGREE_BLOCK(4);
                }
                return;
            }

            // Handle the special case of c0 != 0 and c1 = 0.
            const double oneThird = 1.0 / 3.0;
            if (Math.Abs(c1 - zero) < double.Epsilon)
            {
                // One simple real root.
                double root0;
                if (c0 > zero)
                {
                    root0 = -Math.Pow(c0, oneThird);
                    //GTE_ROOTS_LOW_DEGREE_BLOCK(5);
                }
                else
                {
                    root0 = Math.Pow(-c0, oneThird);
                    //GTE_ROOTS_LOW_DEGREE_BLOCK(6);
                }
                rmMap.Add(root0, 1);

                // One complex conjugate pair.
                // Complex z0 = root0*(-1 - i*sqrt(3))/2;
                // Complex z0conj = root0*(-1 + i*sqrt(3))/2;
                return;
            }

            // At this time, c0 != 0 and c1 != 0.
            const double rat2 = 2, rat3 = 3, rat4 = 4, rat27 = 27, rat108 = 108;
            double delta = -(rat4 * c1 * c1 * c1 + rat27 * c0 * c0);
            if (delta > zero)
            {
                // Three simple roots.
                double deltaDiv108 = delta / rat108;
                double betaRe = -c0 / rat2;
                double betaIm = Math.Sqrt(deltaDiv108);
                double theta = Math.Atan2(betaIm, betaRe);
                double thetaDiv3 = theta / rat3;
                double angle = thetaDiv3;
                double cs = Math.Cos(angle);
                double sn = Math.Sin(angle);
                double rhoSqr = betaRe * betaRe + betaIm * betaIm;
                double rhoPowThird = Math.Pow(rhoSqr, 1.0 / 6.0);
                double temp0 = rhoPowThird * cs;
                double temp1 = rhoPowThird * sn * Math.Sqrt(3.0);
                double root0 = rat2 * temp0;
                double root1 = -temp0 - temp1;
                double root2 = -temp0 + temp1;
                rmMap.Add(root0, 1);
                rmMap.Add(root1, 1);
                rmMap.Add(root2, 1);
                //GTE_ROOTS_LOW_DEGREE_BLOCK(7);
            }
            else if (delta < zero)
            {
                // One simple root.
                double deltaDiv108 = delta / rat108;
                double temp0 = -c0 / rat2;
                double temp1 = Math.Sqrt(-deltaDiv108);
                double temp2 = temp0 - temp1;
                double temp3 = temp0 + temp1;
                if (temp2 >= zero)
                {
                    temp2 = Math.Pow(temp2, oneThird);
                    //GTE_ROOTS_LOW_DEGREE_BLOCK(8);
                }
                else
                {
                    temp2 = -Math.Pow(-temp2, oneThird);
                    //GTE_ROOTS_LOW_DEGREE_BLOCK(9);
                }
                if (temp3 >= zero)
                {
                    temp3 = Math.Pow(temp3, oneThird);
                    //GTE_ROOTS_LOW_DEGREE_BLOCK(10);
                }
                else
                {
                    temp3 = -Math.Pow(-temp3, oneThird);
                    //GTE_ROOTS_LOW_DEGREE_BLOCK(11);
                }
                double root0 = temp2 + temp3;
                rmMap.Add(root0, 1);

                // One complex conjugate pair.
                // Complex z0 = (-root0 - i*sqrt(3*root0*root0+4*c1))/2;
                // Complex z0conj = (-root0 + i*sqrt(3*root0*root0+4*c1))/2;
            }
            else  // delta = 0
            {
                // One simple root and one double root.
                double root0 = -rat3 * c0 / (rat2 * c1);
                double root1 = -rat2 * root0;
                rmMap.Add(root0, 2);
                rmMap.Add(root1, 1);
                //GTE_ROOTS_LOW_DEGREE_BLOCK(12);
            }
        }

        private static void SolveDepressedQuartic(double c0, double c1, double c2, ref SortedDictionary<double, int> rmMap)
        {
            // Handle the special case of c0 = 0, in which case the polynomial
            // reduces to a depressed cubic.
            const double zero = 0;
            if (Math.Abs(c0 - zero) < double.Epsilon)
            {
                SolveDepressedCubic(c1, c2, ref rmMap);
                if (rmMap.ContainsKey(zero))
                {
                    // The cubic has a root of zero, so the multiplicity must
                    // be increased.
                    rmMap[zero] += 1;
                    //GTE_ROOTS_LOW_DEGREE_BLOCK(13);

                }
                else
                {
                    // The cubic does not have a root of zero.  Insert the one
                    // for the quartic.
                    rmMap.Add(zero, 1);
                    //GTE_ROOTS_LOW_DEGREE_BLOCK(14);
                }
                return;
            }

            // Handle the special case of c1 = 0, in which case the quartic is
            // a biquadratic
            //   x^4 + c1*x^2 + c0 = (x^2 + c2/2)^2 + (c0 - c2^2/4)
            if (Math.Abs(c1 - zero) < double.Epsilon)
            {
                SolveBiquadratic(c0, c2, ref rmMap);
                return;
            }

            // At this time, c0 != 0 and c1 != 0, which is a requirement for
            // the general solver that must use a root of a special cubic
            // polynomial.
            const double rat2 = 2, rat4 = 4, rat8 = 8, rat12 = 12, rat16 = 16;
            const double rat27 = 27, rat36 = 36;
            double c0sqr = c0 * c0, c1sqr = c1 * c1, c2sqr = c2 * c2;
            double delta = c1sqr * (-rat27 * c1sqr + rat4 * c2 * (rat36 * c0 - c2sqr)) + rat16 * c0 * (c2sqr * (c2sqr - rat8 * c0) + rat16 * c0sqr);
            double a0 = rat12 * c0 + c2sqr;
            double a1 = rat4 * c0 - c2sqr;

            if (delta > zero)
            {
                if (c2 < zero && a1 < zero)
                {
                    // Four simple real roots.
                    SolveCubic(c1sqr - rat4 * c0 * c2, rat8 * c0, rat4 * c2, -rat8, out SortedDictionary<double, int> rmCubicMap);
                    double t = rmCubicMap.First().Key;
                    double alphaSqr = rat2 * t - c2;
                    double alpha = Math.Sqrt(alphaSqr);
                    double sgnC1;
                    if (c1 > zero)
                    {
                        sgnC1 = 1.0;
                        //GTE_ROOTS_LOW_DEGREE_BLOCK(15);
                    }
                    else
                    {
                        sgnC1 = -1.0;
                        //GTE_ROOTS_LOW_DEGREE_BLOCK(16);
                    }
                    double arg = t * t - c0;
                    double beta = sgnC1 * Math.Sqrt(Math.Max(arg, 0.0));
                    double D0 = alphaSqr - rat4 * (t + beta);
                    double sqrtD0 = Math.Sqrt(Math.Max(D0, 0.0));
                    double D1 = alphaSqr - rat4 * (t - beta);
                    double sqrtD1 = Math.Sqrt(Math.Max(D1, 0.0));
                    double root0 = (alpha - sqrtD0) / rat2;
                    double root1 = (alpha + sqrtD0) / rat2;
                    double root2 = (-alpha - sqrtD1) / rat2;
                    double root3 = (-alpha + sqrtD1) / rat2;
                    rmMap.Add(root0, 1);
                    rmMap.Add(root1, 1);
                    rmMap.Add(root2, 1);
                    rmMap.Add(root3, 1);
                }
                else // c2 >= 0 or a1 >= 0
                {
                    // Two complex-conjugate pairs.  The values alpha, D0
                    // and D1 are those of the if-block.
                    // Complex z0 = (alpha - i*sqrt(-D0))/2;
                    // Complex z0conj = (alpha + i*sqrt(-D0))/2;
                    // Complex z1 = (-alpha - i*sqrt(-D1))/2;
                    // Complex z1conj = (-alpha + i*sqrt(-D1))/2;
                    //GTE_ROOTS_LOW_DEGREE_BLOCK(17);
                }
            }
            else if (delta < zero)
            {
                // Two simple real roots, one complex-conjugate pair.
                SolveCubic(c1sqr - rat4 * c0 * c2, rat8 * c0, rat4 * c2, -rat8, out SortedDictionary<double, int> rmCubicMap);
                double t = rmCubicMap.First().Key;
                double alphaSqr = rat2 * t - c2;
                double alpha = Math.Sqrt(Math.Max(alphaSqr, 0.0));
                double sgnC1;
                if (c1 > zero)
                {
                    sgnC1 = 1.0;  // Leads to BLOCK(18)
                }
                else
                {
                    sgnC1 = -1.0;  // Leads to BLOCK(19)
                }
                double arg = t * t - c0;
                double beta = (sgnC1 * Math.Sqrt(Math.Max(arg, 0.0)));
                double root0, root1;
                if (sgnC1 > 0.0)
                {
                    double D1 = alphaSqr - rat4 * (t - beta);
                    double sqrtD1 = Math.Sqrt(Math.Max(D1, 0.0));
                    root0 = (-alpha - sqrtD1) / rat2;
                    root1 = (-alpha + sqrtD1) / rat2;

                    // One complex conjugate pair.
                    // Complex z0 = (alpha - i*sqrt(-D0))/2;
                    // Complex z0conj = (alpha + i*sqrt(-D0))/2;
                    //GTE_ROOTS_LOW_DEGREE_BLOCK(18);
                }
                else
                {
                    double D0 = alphaSqr - rat4 * (t + beta);
                    double sqrtD0 = Math.Sqrt(Math.Max(D0, 0.0));
                    root0 = (alpha - sqrtD0) / rat2;
                    root1 = (alpha + sqrtD0) / rat2;

                    // One complex conjugate pair.
                    // Complex z0 = (-alpha - i*sqrt(-D1))/2;
                    // Complex z0conj = (-alpha + i*sqrt(-D1))/2;
                    //GTE_ROOTS_LOW_DEGREE_BLOCK(19);
                }
                rmMap.Add(root0, 1);
                rmMap.Add(root1, 1);
            }
            else  // delta = 0
            {
                if (a1 > zero || (c2 > zero && (Math.Abs(a1 - zero) > double.Epsilon || Math.Abs(c1 - zero) > double.Epsilon)))
                {
                    // One double real root, one complex-conjugate pair.
                    const double rat9 = 9;
                    double root0 = -c1 * a0 / (rat9 * c1sqr - rat2 * c2 * a1);
                    rmMap.Add(root0, 2);

                    // One complex conjugate pair.
                    // Complex z0 = -root0 - i*sqrt(c2 + root0^2);
                    // Complex z0conj = -root0 + i*sqrt(c2 + root0^2);
                    //GTE_ROOTS_LOW_DEGREE_BLOCK(20);
                }
                else
                {
                    const double rat3 = 3;
                    if (Math.Abs(a0 - zero) > double.Epsilon)
                    {
                        // One double real root, two simple real roots.
                        const double rat9 = 9;
                        double root0 = -c1 * a0 / (rat9 * c1sqr - rat2 * c2 * a1);
                        double alpha = rat2 * root0;
                        double beta = c2 + rat3 * root0 * root0;
                        double discr = alpha * alpha - rat4 * beta;
                        double temp1 = Math.Sqrt(discr);
                        double root1 = (-alpha - temp1) / rat2;
                        double root2 = (-alpha + temp1) / rat2;
                        rmMap.Add(root0, 2);
                        rmMap.Add(root1, 1);
                        rmMap.Add(root2, 1);
                        //GTE_ROOTS_LOW_DEGREE_BLOCK(21);
                    }
                    else
                    {
                        // One triple real root, one simple real root.
                        double root0 = -rat3 * c1 / (rat4 * c2);
                        double root1 = -rat3 * root0;
                        rmMap.Add(root0, 3);
                        rmMap.Add(root1, 1);
                        //GTE_ROOTS_LOW_DEGREE_BLOCK(22);
                    }
                }
            }
        }

        private static void SolveBiquadratic(double c0, double c2, ref SortedDictionary<double, int> rmMap)
        {
            // Solve 0 = x^4 + c2*x^2 + c0 = (x^2 + c2/2)^2 + a1, where
            // a1 = c0 - c2^2/2.  We know that c0 != 0 at the time of the
            // function call, so x = 0 is not a root.  The condition c1 = 0
            // implies the quartic Delta = 256*c0*a1^2.

            const double zero = 0, rat2 = 2, rat256 = 256;
            double c2Half = c2 / rat2;
            double a1 = c0 - c2Half * c2Half;
            double delta = rat256 * c0 * a1 * a1;
            if (delta > zero)
            {
                if (c2 < zero)
                {
                    if (a1 < zero)
                    {
                        // Four simple roots.
                        double temp0 = Math.Sqrt(-a1);
                        double temp1 = -c2Half - temp0;
                        double temp2 = -c2Half + temp0;
                        double root1 = Math.Sqrt(temp1);
                        double root0 = -root1;
                        double root2 = Math.Sqrt(temp2);
                        double root3 = -root2;
                        rmMap.Add(root0, 1);
                        rmMap.Add(root1, 1);
                        rmMap.Add(root2, 1);
                        rmMap.Add(root3, 1);
                        //GTE_ROOTS_LOW_DEGREE_BLOCK(23);
                    }
                    else  // a1 > 0
                    {
                        // Two simple complex conjugate pairs.
                        // double thetaDiv2 = atan2(sqrt(a1), -c2/2) / 2.0;
                        // double cs = cos(thetaDiv2), sn = sin(thetaDiv2);
                        // double length = pow(c0, 0.25);
                        // Complex z0 = length*(cs + i*sn);
                        // Complex z0conj = length*(cs - i*sn);
                        // Complex z1 = length*(-cs + i*sn);
                        // Complex z1conj = length*(-cs - i*sn);
                        //GTE_ROOTS_LOW_DEGREE_BLOCK(24);
                    }
                }
                else  // c2 >= 0
                {
                    // Two simple complex conjugate pairs.
                    // Complex z0 = -i*sqrt(c2/2 - sqrt(-a1));
                    // Complex z0conj = +i*sqrt(c2/2 - sqrt(-a1));
                    // Complex z1 = -i*sqrt(c2/2 + sqrt(-a1));
                    // Complex z1conj = +i*sqrt(c2/2 + sqrt(-a1));
                    //GTE_ROOTS_LOW_DEGREE_BLOCK(25);
                }
            }
            else if (delta < zero)
            {
                // Two simple real roots.
                double temp0 = Math.Sqrt(-a1);
                double temp1 = -c2Half + temp0;
                double root1 = Math.Sqrt(temp1);
                double root0 = -root1;
                rmMap.Add(root0, 1);
                rmMap.Add(root1, 1);

                // One complex conjugate pair.
                // Complex z0 = -i*sqrt(c2/2 + sqrt(-a1));
                // Complex z0conj = +i*sqrt(c2/2 + sqrt(-a1));
                //GTE_ROOTS_LOW_DEGREE_BLOCK(26);
            }
            else  // delta = 0
            {
                if (c2 < zero)
                {
                    // Two double real roots.
                    double root1 = Math.Sqrt(-c2Half);
                    double root0 = -root1;
                    rmMap.Add(root0, 2);
                    rmMap.Add(root1, 2);
                    //GTE_ROOTS_LOW_DEGREE_BLOCK(27);
                }
                else  // c2 > 0
                {
                    // Two double complex conjugate pairs.
                    // Complex z0 = -i*sqrt(c2/2);  // multiplicity 2
                    // Complex z0conj = +i*sqrt(c2/2);  // multiplicity 2
                    //GTE_ROOTS_LOW_DEGREE_BLOCK(28);
                }
            }
        }

        // Support for the GetNumRoots* functions.
        private static void GetRootInfoDepressedQuadratic(double c0, ref List<int> info)
        {
            const double zero = 0;
            if (c0 < zero)
            {
                // Two simple roots.
                info.Add(1);
                info.Add(1);
            }
            else if (Math.Abs(c0 - zero) < double.Epsilon)
            {
                // One double root.
                info.Add(2);  // root is zero
            }
            else  // c0 > 0
            {
                // A complex-conjugate pair of roots.
            }
        }

        private static void GetRootInfoDepressedCubic(double c0, double c1, ref List<int> info)
        {
            // Handle the special case of c0 = 0, in which case the polynomial
            // reduces to a depressed quadratic.
            const double zero = 0;
            if (Math.Abs(c0 - zero) < double.Epsilon)
            {
                if (Math.Abs(c1 - zero) < double.Epsilon)
                {
                    info.Add(3);  // triple root of zero
                }
                else
                {
                    info.Add(1);  // simple root of zero
                    GetRootInfoDepressedQuadratic(c1, ref info);
                }
                return;
            }

            const double rat4 = 4, rat27 = 27;
            double delta = -(rat4 * c1 * c1 * c1 + rat27 * c0 * c0);
            if (delta > zero)
            {
                // Three simple real roots.
                info.Add(1);
                info.Add(1);
                info.Add(1);
            }
            else if (delta < zero)
            {
                // One simple real root.
                info.Add(1);
            }
            else  // delta = 0
            {
                // One simple real root and one double real root.
                info.Add(1);
                info.Add(2);
            }
        }

        private static void GetRootInfoDepressedQuartic(double c0, double c1, double c2, ref List<int> info)
        {
            // Handle the special case of c0 = 0, in which case the polynomial
            // reduces to a depressed cubic.
            const double zero = 0;
            if (Math.Abs(c0 - zero) < double.Epsilon)
            {
                if (Math.Abs(c1 - zero) < double.Epsilon)
                {
                    if (Math.Abs(c2 - zero) < double.Epsilon)
                    {
                        info.Add(4);  // quadruple root of zero
                    }
                    else
                    {
                        info.Add(2);  // double root of zero
                        GetRootInfoDepressedQuadratic(c2, ref info);
                    }
                }
                else
                {
                    info.Add(1);  // simple root of zero
                    GetRootInfoDepressedCubic(c1, c2, ref info);
                }
                return;
            }

            // Handle the special case of c1 = 0, in which case the quartic is
            // a biquadratic
            //   x^4 + c1*x^2 + c0 = (x^2 + c2/2)^2 + (c0 - c2^2/4)
            if (Math.Abs(c1 - zero) < double.Epsilon)
            {
                GetRootInfoBiquadratic(c0, c2, ref info);
                return;
            }

            // At this time, c0 != 0 and c1 != 0, which is a requirement for
            // the general solver that must use a root of a special cubic
            // polynomial.
            const double rat4 = 4, rat8 = 8, rat12 = 12, rat16 = 16;
            const double rat27 = 27, rat36 = 36;
            double c0sqr = c0 * c0, c1sqr = c1 * c1, c2sqr = c2 * c2;
            double delta = c1sqr * (-rat27 * c1sqr + rat4 * c2 * (rat36 * c0 - c2sqr)) + rat16 * c0 * (c2sqr * (c2sqr - rat8 * c0) + rat16 * c0sqr);
            double a0 = rat12 * c0 + c2sqr;
            double a1 = rat4 * c0 - c2sqr;

            if (delta > zero)
            {
                if (c2 < zero && a1 < zero)
                {
                    // Four simple real roots.
                    info.Add(1);
                    info.Add(1);
                    info.Add(1);
                    info.Add(1);
                }
                else // c2 >= 0 or a1 >= 0
                {
                    // Two complex-conjugate pairs.
                }
            }
            else if (delta < zero)
            {
                // Two simple real roots, one complex-conjugate pair.
                info.Add(1);
                info.Add(1);
            }
            else  // delta = 0
            {
                if (a1 > zero || (c2 > zero && (Math.Abs(a1 - zero) > double.Epsilon || Math.Abs(c1 - zero) > double.Epsilon)))
                {
                    // One double real root, one complex-conjugate pair.
                    info.Add(2);
                }
                else
                {
                    if (Math.Abs(a0 - zero) > double.Epsilon)
                    {
                        // One double real root, two simple real roots.
                        info.Add(2);
                        info.Add(1);
                        info.Add(1);
                    }
                    else
                    {
                        // One triple real root, one simple real root.
                        info.Add(3);
                        info.Add(1);
                    }
                }
            }
        }

        private static void GetRootInfoBiquadratic(double c0, double c2, ref List<int> info)
        {
            // Solve 0 = x^4 + c2*x^2 + c0 = (x^2 + c2/2)^2 + a1, where
            // a1 = c0 - c2^2/2.  We know that c0 != 0 at the time of the
            // function call, so x = 0 is not a root.  The condition c1 = 0
            // implies the quartic Delta = 256*c0*a1^2.

            const double zero = 0, rat2 = 2, rat256 = 256;
            double c2Half = c2 / rat2;
            double a1 = c0 - c2Half * c2Half;
            double delta = rat256 * c0 * a1 * a1;
            if (delta > zero)
            {
                if (c2 < zero)
                {
                    if (a1 < zero)
                    {
                        // Four simple roots.
                        info.Add(1);
                        info.Add(1);
                        info.Add(1);
                        info.Add(1);
                    }
                    else  // a1 > 0
                    {
                        // Two simple complex conjugate pairs.
                    }
                }
                else  // c2 >= 0
                {
                    // Two simple complex conjugate pairs.
                }
            }
            else if (delta < zero)
            {
                // Two simple real roots, one complex conjugate pair.
                info.Add(1);
                info.Add(1);
            }
            else  // delta = 0
            {
                if (c2 < zero)
                {
                    // Two double real roots.
                    info.Add(2);
                    info.Add(2);
                }
                else  // c2 > 0
                {
                    // Two double complex conjugate pairs.
                }
            }
        }

        // Support for the Find functions.
        private static int FindRecursive(int degree, double[] c, double tmin, double tmax, int maxIterations, ref double[] roots)
        {
            // The base of the recursion.
            double zero = 0.0;
            double root = zero;
            int numRoots;

            if (degree == 1)
            {
                if (Math.Abs(c[1] - zero) > double.Epsilon)
                {
                    root = -c[0] / c[1];
                    numRoots = 1;
                }
                else if (Math.Abs(c[0] - zero) < double.Epsilon)
                {
                    root = zero;
                    numRoots = 1;
                }
                else
                {
                    numRoots = 0;
                }

                if (numRoots > 0 && tmin <= root && root <= tmax)
                {
                    roots[0] = root;
                    return 1;
                }
                return 0;
            }

            // Find the roots of the derivative polynomial scaled by 1/degree.
            // The scaling avoids the factorial growth in the coefficients;
            // for example, without the scaling, the high-order term x^d
            // becomes (d!)*x through multiple differentiations.  With the
            // scaling we instead get x.  This leads to better numerical
            // behavior of the root finder.
            int derivDegree = degree - 1;
            double[] derivCoeff = new double[derivDegree + 1];
            double[] derivRoots = new double[derivDegree];
            for (int i = 0, ip1 = 1; i <= derivDegree; ++i, ++ip1)
            {
                derivCoeff[i] = c[ip1] * ip1 / degree;
            }
            int numDerivRoots = FindRecursive(degree - 1, derivCoeff, tmin, tmax, maxIterations, ref derivRoots);

            numRoots = 0;
            if (numDerivRoots > 0)
            {
                // Find root on [tmin,derivRoots[0]].
                if (Find(degree, c, tmin, derivRoots[0], maxIterations, out root))
                {
                    roots[numRoots++] = root;
                }

                // Find root on [derivRoots[i],derivRoots[i+1]].
                for (int i = 0, ip1 = 1; i <= numDerivRoots - 2; ++i, ++ip1)
                {
                    if (Find(degree, c, derivRoots[i], derivRoots[ip1], maxIterations, out root))
                    {
                        roots[numRoots++] = root;
                    }
                }

                // Find root on [derivRoots[numDerivRoots-1],tmax].
                if (Find(degree, c, derivRoots[numDerivRoots - 1], tmax, maxIterations, out root))
                {
                    roots[numRoots++] = root;
                }
            }
            else
            {
                // The polynomial is monotone on [tmin,tmax], so has at most one root.
                if (Find(degree, c, tmin, tmax, maxIterations, out root))
                {
                    roots[numRoots++] = root;
                }
            }
            return numRoots;
        }

        private static double Evaluate(int degree, double[] c, double t)
        {
            int i = degree;
            double result = c[i];
            while (--i >= 0)
            {
                result = t * result + c[i];
            }
            return result;
        }
    };
}
