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
    public abstract class ParametricCurve
    {
        protected const int DEFAULT_ROMBERG_ORDER = 8;
        protected const int DEFAULT_MAX_BISECTIONS = 1024;
        protected const int SUP_ORDER = 4;

        protected readonly double[] times;
        protected readonly double[] segmentLength;
        protected readonly double[] acumulatedLength;
        protected int rombergOrder;
        protected int maxBisections;
        protected bool isConstructed;

        // Abstract base class for a parameterized curve X(t), where t is the
        // parameter in [tmin,tmax] and X is an N-tuple position.  The first
        // constructor is for single-segment curves. The second constructor is
        // for multiple-segment curves. The times must be strictly increasing.
        protected ParametricCurve(double tmin, double tmax) :
            this(1, new []{tmin, tmax})
        {
        }

        protected ParametricCurve(int numSegments, double[] times)
        {
            this.times = new double[numSegments + 1];
            times.CopyTo(this.times, 0);
            this.segmentLength = new double[numSegments];
            this.acumulatedLength = new double[numSegments];
            this.rombergOrder = DEFAULT_ROMBERG_ORDER;
            this.maxBisections = DEFAULT_MAX_BISECTIONS;
            this.isConstructed = false;
        }

        // To validate construction, create an object as shown:
        // DerivedClassCurve<N, Real> curve(parameters);
        // if (!curve) { <constructor failed, handle accordingly>; }
        public bool IsConstructed
        {
            get { return this.isConstructed; }
        }

        // Member access.
        public double TMin
        {
            get { return this.times[0]; }
        }

        public double TMax
        {
            get { return this.times[this.times.Length - 1]; }
        }

        public int NumSegments
        {
            get { return this.segmentLength.Length; }
        }

        public double[] Times
        {
            get { return this.times; }
        }

        // Parameters used in GetLength(...), GetTotalLength() and
        // GetTime(...).

        // The default value is 8.
        public int RombergOrder
        {
            get { return this.rombergOrder;}
            set { this.rombergOrder = Math.Max(value, 1); }
        }

        // The default value is 1024.
        public int MaxBisections
        {
            get { return this.maxBisections;}
            set { this.maxBisections = Math.Max(value, 1); }
        }

        // This function applies only when the first constructor is used(two
        // times rather than a sequence of three or more times).
        public void SetTimeInterval(double tmin, double tmax)
        {
            if (this.times.Length == 2)
            {
                this.times[0] = tmin;
                this.times[1] = tmax;
            }
        }

        // Evaluation of the curve.  The function supports derivative
        // calculation through order 3; that is, order <= 3 is required.  If
        // you want/ only the position, pass in order of 0.  If you want the
        // position and first derivative, pass in order of 1, and so on.  The
        // output array 'jet' must have enough storage to support the maximum
        // order.  The values are ordered as: position, first derivative,
        // second derivative, third derivative.
        public abstract void Evaluate(double t, int order, out Vector3[] jet);

        // Differential geometric quantities.
        public Vector3 GetPosition(double t)
        {
            this.Evaluate(t, 0, out Vector3[] position);
            return position[0];
        }

        public Vector3 GetTangent(double t) 
        {
            // (position, tangent)
            this.Evaluate(t, 1, out Vector3[] jet);
            return Vector3.Normalize(jet[1]);
        }

        public double GetSpeed(double t)
        {
            // (position, tangent)
            this.Evaluate(t, 1, out Vector3[] jet);
            return jet[1].Modulus();
        }

        public double GetLength(double t0, double t1)
        {
            double LowerBound(double[] array, int first, int last, double val)
            {
                for (int i = first; i < last; i++)
                {
                    if (array[i] >= val)
                    {
                        return array[i];
                    }
                }

                return double.NaN;
            }

            double speed(double t)
            {
                return this.GetSpeed(t);
            }

            if (Math.Abs(this.segmentLength[0]) < double.Epsilon)
            {
                // Lazy initialization of lengths of segments.
                int numSegments = this.segmentLength.Length;
                double accumulated = 0;
                for (int i = 0, ip1 = 1; i < numSegments; ++i, ++ip1)
                {
                    this.segmentLength[i] = Integration.Romberg(this.rombergOrder, this.times[i], this.times[ip1], speed);
                    accumulated += this.segmentLength[i];
                    this.acumulatedLength[i] = accumulated;
                }
            }

            t0 = Math.Max(t0, this.TMin);
            t1 = Math.Min(t1, this.TMax);

            double iter0 = LowerBound(this.times, 0, this.times.Length, t0);
            int index0 = (int) (iter0 - this.times[0]);
            double iter1 = LowerBound(this.times, 0, this.times.Length, t1);
            int index1 = (int) (iter1 - this.times[0]);

            double length;
            if (index0 < index1)
            {
                length = 0;
                if (t0 < iter0)
                {
                    length += Integration.Romberg(this.rombergOrder, t0, this.times[index0], speed);
                }

                int isup;
                if (t1 < iter1)
                {
                    length += Integration.Romberg(this.rombergOrder, this.times[index1 - 1], t1, speed);
                    isup = index1 - 1;
                }
                else
                {
                    isup = index1;
                }
                for (int i = index0; i < isup; ++i)
                {
                    length += this.segmentLength[i];
                }
            }
            else
            {
                length = Integration.Romberg(this.rombergOrder, t0, t1, speed);
            }
            return length;
        }

        public double GetTotalLength()
        {
            double lastLength = this.acumulatedLength[this.acumulatedLength.Length - 1];
            if (Math.Abs(lastLength) < double.Epsilon)
            {
                // Lazy evaluation of the accumulated length array.
                return this.GetLength(this.TMin, this.TMax);
            }

            return lastLength;
        }

        // Inverse mapping of s = Length(t) given by t = Length^{-1}(s).  The
        // inverse length function generally cannot be written in closed form,
        // in which case it is not directly computable.  Instead, we can
        // specify s and estimate the root t for F(t) = Length(t) - s.  The
        // derivative is F'(t) = Speed(t) >= 0, so F(t) is non decreasing.  To
        // be robust, we use bisection to locate the root, although it is
        // possible to use a hybrid of Newton's method and bisection.  For
        // details, see the document
        // https://www.geometrictools.com/Documentation/MovingAlongCurveSpecifiedSpeed.pdf
        public double GetTime(double length)
        {
            if (length > 0)
            {
                if (length < this.GetTotalLength())
                {
                    double F(double t)
                    {
                        double speed(double z)
                        {
                            return this.GetSpeed(z);
                        }

                        return Integration.Romberg(this.rombergOrder, this.times[0], t, speed) - length;
                    }

                    // We know that F(tmin) < 0 and F(tmax) > 0, which allows us to
                    // use bisection.  Rather than bisect the entire interval, let's
                    // narrow it down with a reasonable initial guess.
                    double ratio = length / this.GetTotalLength();
                    double omratio = 1.0 - ratio;
                    double tmid = omratio * this.times[0] + ratio * this.times[this.times.Length - 1];
                    double fmid = F(tmid);
                    if (fmid > 0)
                    {
                        RootsBisection.Find(F, this.times[0], tmid, -1.0, 1.0, this.maxBisections, out tmid);
                    }
                    else if (fmid < 0)
                    {
                        RootsBisection.Find(F, tmid, this.times[this.times.Length - 1], -1.0, 1.0, this.maxBisections, out tmid);
                    }

                    return tmid;
                }
                    
                return this.times[this.times.Length - 1];
            }
            
            return this.times[0];
        }

        // Compute a subset of curve points according to the specified attribute.
        // The input 'numPoints' must be two or larger.
        // The array ts returns the 't' values for each point
        public Vector3[] SubdivideByTime(int numPoints, out double[] ts)
        {
            Vector3[] points = new Vector3[numPoints];
            ts = new double[numPoints];
            double delta = (this.times[this.times.Length - 1] - this.times[0]) / (numPoints - 1.0);
            for (int i = 0; i < numPoints; ++i)
            {
                double t = this.times[0] + delta * i;
                points[i] = this.GetPosition(t);
                ts[i] = t;
            }

            return points;
        }

        public Vector3[] SubdivideByLength(int numPoints, out double[] ts)
        {
            Vector3[] points = new Vector3[numPoints];
            ts = new double[numPoints];
            double delta = this.GetTotalLength() / (numPoints - 1);
            for (int i = 0; i < numPoints; ++i)
            {
                double length = delta * i;
                double t = this.GetTime(length);
                points[i] = this.GetPosition(t);
                ts[i] = t;
            }

            return points;
        }
    };
}
