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

namespace netDxf.GTE
{
    // Compute a root of a function F(t) on an interval [t0, t1].  The caller
    // specifies the maximum number of iterations, in case you want limited
    // accuracy for the root.  However, the function is designed for native types
    // (Real = float/double).  If you specify a sufficiently large number of
    // iterations, the root finder bisects until either F(t) is identically zero
    // [a condition dependent on how you structure F(t) for evaluation] or the
    // midpoint (t0 + t1)/2 rounds numerically to tmin or tmax.  Of course, it
    // is required that t0 < t1.  The return value of Find is:
    //   0: F(t0)*F(t1) > 0, we cannot determine a root
    //   1: F(t0) = 0 or F(t1) = 0
    //   2..maxIterations:  the number of bisections plus one
    //   maxIterations+1:  the loop executed without a break (no convergence)
    public static class RootsBisection
    {
        // Use this function when F(t0) and F(t1) are not already known.
        public static int Find(Func<double, double> f, double t0, double t1, int maxIterations, out double root)
        {
            // Set 'root' initially to avoid "potentially uninitialized
            // variable" warnings by a compiler.
            root = t0;

            if (t0 < t1)
            {
                // Test the endpoints to see whether F(t) is zero.
                double f0 = f(t0);
                if (Math.Abs(f0) < double.Epsilon)
                {
                    root = t0;
                    return 1;
                }

                double f1 = f(t1);
                if (Math.Abs(f1) < double.Epsilon)
                {
                    root = t1;
                    return 1;
                }

                if (f0 * f1 > 0.0)
                {
                    // It is not known whether the interval bounds a root.
                    return 0;
                }

                int i;
                for (i = 2; i <= maxIterations; ++i)
                {
                    root = 0.5 * (t0 + t1);
                    if (Math.Abs(root - t0) < double.Epsilon || Math.Abs(root - t1) < double.Epsilon)
                    {
                        // The numbers t0 and t1 are consecutive
                        // floating-point numbers.
                        break;
                    }

                    double fm = f(root);
                    double product = fm * f0;
                    if (product < 0.0)
                    {
                        t1 = root;
                        f1 = fm;
                    }
                    else if (product > 0.0)
                    {
                        t0 = root;
                        f0 = fm;
                    }
                    else
                    {
                        break;
                    }
                }
                return i;
            }
           
            // The interval endpoints are invalid.
            return 0;
        }

        // If f0 = F(t0) and f1 = F(t1) are already known, pass them to the
        // bisector.  This is useful when |f0| or |f1| is infinite, and you
        // can pass sign(f0) or sign(f1) rather than then infinity because
        // the bisector cares only about the signs of f.
        public static int Find(Func<double, double> f, double t0, double t1, double f0, double f1, int maxIterations, out double root)
        {
            // Set 'root' initially to avoid "potentially uninitialized
            // variable" warnings by a compiler.
            root = t0;

            if (t0 < t1)
            {
                // Test the endpoints to see whether F(t) is zero.
                if (Math.Abs(f0) < double.Epsilon)
                {
                    root = t0;
                    return 1;
                }

                if (Math.Abs(f1) < double.Epsilon)
                {
                    root = t1;
                    return 1;
                }

                if (f0 * f1 > 0.0)
                {
                    // It is not known whether the interval bounds a root.
                    return 0;
                }

                int i;
                root = t0;
                for (i = 2; i <= maxIterations; ++i)
                {
                    root = 0.5 * (t0 + t1);
                    if (Math.Abs(root - t0) < double.Epsilon || Math.Abs(root - t1) < double.Epsilon)
                    {
                        // The numbers t0 and t1 are consecutive
                        // floating-point numbers.
                        break;
                    }

                    double fm = f(root);
                    double product = fm * f0;
                    if (product < 0.0)
                    {
                        t1 = root;
                        f1 = fm;
                    }
                    else if (product > 0.0)
                    {
                        t0 = root;
                        f0 = fm;
                    }
                    else
                    {
                        break;
                    }
                }
                return i;
            }

            // The interval endpoints are invalid.
            return 0;
        }
    };
}

