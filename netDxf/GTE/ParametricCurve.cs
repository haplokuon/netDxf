// This is a translation to C# from the original C++ code of the Geometric Tool Library
// David Eberly, Geometric Tools, Redmond WA 98052
// Copyright (c) 1998-2022
// Distributed under the Boost Software License, Version 1.0.
// https://www.boost.org/LICENSE_1_0.txt
// https://www.geometrictools.com/License/Boost/LICENSE_1_0.txt
// Version: 6.0.2022.01.06

namespace netDxf.GTE
{
    internal abstract class ParametricCurve
    {
        //protected const int DEFAULT_ROMBERG_ORDER = 8;
        //protected const int DEFAULT_MAX_BISECTIONS = 1024;
        protected const int SUP_ORDER = 4;

        //protected double[] mTime;
        //protected double[] mSegmentLength;
        //protected double[] mAccumulatedLength;
        //protected int mRombergOrder;
        //protected int mMaxBisections;
        protected double mTMin;
        protected double mTMax;
        protected bool mConstructed;

        // Abstract base class for a parameterized curve X(t), where t is the
        // parameter in [tmin,tmax] and X is an N-tuple position.  The first
        // constructor is for single-segment curves. The second constructor is
        // for multiple-segment curves. The times must be strictly increasing.
        protected ParametricCurve(double tmin, double tmax)
            {
                this.mTMin = tmin;
                this.mTMax = tmax;
                //this.mSegmentLength = new[] {0.0};
                //this.mAccumulatedLength = new[] {0.0};
                //this.mRombergOrder = DEFAULT_ROMBERG_ORDER;
                //this.mMaxBisections = DEFAULT_MAX_BISECTIONS;
                this.mConstructed = false;

            }

        //protected ParametricCurve(int numSegments, double[] times)
        //{
        //    this.mTime = new double[numSegments + 1];
        //    times.CopyTo(this.mTime, 0);
        //    //this.mSegmentLength = new[] {0.0};
        //    //this.mAccumulatedLength = new[] {0.0};
        //    //this.mRombergOrder = DEFAULT_ROMBERG_ORDER;
        //    //this.mMaxBisections = DEFAULT_MAX_BISECTIONS;
        //    this.mConstructed = false;

        //}


        // To validate construction, create an object as shown:
        //     DerivedClassCurve<N, Real> curve(parameters);
        //     if (!curve) { <constructor failed, handle accordingly>; }
        public bool IsConstructed
        {
            get { return this.mConstructed; }
        }

        // Member access.
        public double TMin
        {
            get { return this.mTMin; }
        }

        public double TMax
        {
            get { return this.mTMax; }
        }

        //public int GetNumSegments() 
        //{
        //    return this.mSegmentLength.Length;
        //}

        //public double[] GetTimes() 
        //{
        //    return this.mTime;
        //}

        // This function applies only when the first constructor is used (two
        // times rather than a sequence of three or more times).
        //public void SetTimeInterval(double tmin, double tmax)
        //{
        //    if (mTime.Length == 2)
        //    {
        //        mTime[0] = tmin;
        //        mTime[1] = tmax;
        //    }
        //}

        // Parameters used in GetLength(...), GetTotalLength() and
        // GetTime(...).

        // The default value is 8.
        //public void SetRombergOrder(int order)
        //{
        //    mRombergOrder = Math.Max(order, 1);
        //}

        // The default value is 1024.
        //public void SetMaxBisections(int maxBisections)
        //{
        //    mMaxBisections = Math.Max(maxBisections, 1);
        //}

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
            Evaluate(t, 0, out Vector3[] position);
            return position[0];
        }

        public Vector3 GetTangent(double t) 
        {
            // (position, tangent)
            Evaluate(t, 1, out Vector3[] jet);
            return Vector3.Normalize(jet[1]);
        }

        public double GetSpeed(double t)
        {
            // (position, tangent)
            Evaluate(t, 1, out Vector3[] jet);
            return jet[1].Modulus();
        }

        //public static double LowerBound(double[] array, int first, int last, double val)
        //{
        //    for (int i = first; i < last; i++)
        //    {
        //        if (array[i] > val)
        //        {
        //            return array[i];
        //        }
        //    }

        //    return array[first];
        //}

        //public double GetLength(double t0, double t1)
        //{
        //    double speed(double t)
        //    {
        //        return this.GetSpeed(t);
        //    }

        //    if (Math.Abs(this.mSegmentLength[0]) < double.Epsilon)
        //    {
        //        // Lazy initialization of lengths of segments.
        //        int numSegments = this.mSegmentLength.Length;
        //        double accumulated = 0;
        //        for (int i = 0, ip1 = 1; i < numSegments; ++i, ++ip1)
        //        {
        //            mSegmentLength[i] = Integration<Real>::Romberg(mRombergOrder, mTime[i], mTime[ip1], speed);
        //            accumulated += mSegmentLength[i];
        //            mAccumulatedLength[i] = accumulated;
        //        }
        //    }

        //    t0 = Math.Max(t0, GetTMin());
        //    t1 = Math.Min(t1, GetTMax());
            
        //    double iter0 = LowerBound(this.mTime, 0, this.mTime.Length, t0);
        //    int index0 = (int) (iter0 - this.mTime[0]);
        //    double iter1 = LowerBound(this.mTime, 0, this.mTime.Length, t1);
        //    int index1 = (int) (iter1 - this.mTime[0]);

        //    double length;
        //    if (index0 < index1)
        //    {
        //        length = 0;
        //        if (t0 < iter0)
        //        {
        //            length += Integration<Real>::Romberg(mRombergOrder, t0, mTime[index0], speed);
        //        }

        //        int isup;
        //        if (t1 < iter1)
        //        {
        //            length += Integration<Real>::Romberg(mRombergOrder, mTime[static_cast<size_t>(index1) - 1], t1, speed);
        //            isup = index1 - 1;
        //        }
        //        else
        //        {
        //            isup = index1;
        //        }
        //        for (int i = index0; i < isup; ++i)
        //        {
        //            length += mSegmentLength[i];
        //        }
        //    }
        //    else
        //    {
        //        length = Integration<Real>::Romberg(mRombergOrder, t0, t1, speed);
        //    }
        //    return length;
        //}

        //public double GetTotalLength()
        //{
        //    double lastLength = this.mAccumulatedLength[this.mAccumulatedLength.Length - 1];
        //    if (Math.Abs(lastLength) < double.Epsilon)
        //    {
        //        // Lazy evaluation of the accumulated length array.
        //        return GetLength(mTime[0], mTime[this.mTime.Length - 1]);
        //    }

        //    return lastLength;
        //}

        //// Inverse mapping of s = Length(t) given by t = Length^{-1}(s).  The
        //// inverse length function generally cannot be written in closed form,
        //// in which case it is not directly computable.  Instead, we can
        //// specify s and estimate the root t for F(t) = Length(t) - s.  The
        //// derivative is F'(t) = Speed(t) >= 0, so F(t) is non decreasing.  To
        //// be robust, we use bisection to locate the root, although it is
        //// possible to use a hybrid of Newton's method and bisection.  For
        //// details, see the document
        //// https://www.geometrictools.com/Documentation/MovingAlongCurveSpecifiedSpeed.pdf
        //public double GetTime(double length)
        //{
        //    if (length > 0)
        //    {
        //        if (length < GetTotalLength())
        //        {
        //            double F (double t)
        //            {
        //                double speed(double z)
        //                {
        //                    return this.GetSpeed(z);
        //                }

        //                return Integration<Real>::Romberg(mRombergOrder, mTime[0], t, speed) - length;
        //            };

        //            // We know that F(tmin) < 0 and F(tmax) > 0, which allows us to
        //            // use bisection.  Rather than bisect the entire interval, let's
        //            // narrow it down with a reasonable initial guess.
        //            double ratio = length / GetTotalLength();
        //            double omratio = 1 - ratio;
        //            double tmid = omratio * mTime[0] + ratio * mTime[this.mTime.Length - 1];
        //            double fmid = F(tmid);
        //            if (fmid > 0)
        //            {
        //                RootsBisection<Real>::Find(F, mTime[0], tmid, -1, 1, mMaxBisections, tmid);
        //            }
        //            else if (fmid < 0)
        //            {
        //                RootsBisection<Real>::Find(F, tmid, mTime[this.mTime.Length - 1], -1, 1, mMaxBisections, tmid);
        //            }
        //            return tmid;
        //        }
        //        else
        //        {
        //            return mTime[this.mTime.Length - 1];
        //        }
        //    }
        //    else
        //    {
        //        return mTime[0];
        //    }
        //}

        //// Compute a subset of curve points according to the specified attribute.
        //// The input 'numPoints' must be two or larger.
        //public void SubdivideByTime(int numPoints, Vector3[] points)
        //{
        //    double delta = (mTime[this.mTime.Length - 1] - mTime[0]) / (double)(numPoints - 1);
        //    for (int i = 0; i < numPoints; ++i)
        //    {
        //        double t = mTime[0] + delta * i;
        //        points[i] = GetPosition(t);
        //    }
        //}

        //public void SubdivideByLength(int numPoints, Vector3[] points)
        //{
        //    double delta = GetTotalLength() / (numPoints - 1);
        //    for (int i = 0; i < numPoints; ++i)
        //    {
        //        double length = delta * i;
        //        double t = GetTime(length);
        //        points[i] = GetPosition(t);
        //    }
        //}
    };
}
