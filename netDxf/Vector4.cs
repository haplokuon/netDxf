#region netDxf library licensed under the MIT License
// 
//                       netDxf library
// Copyright (c) 2019-2023 Daniel Carvajal (haplokuon@gmail.com)
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

using System;
using System.Threading;

namespace netDxf
{
    /// <summary>
    /// Represent a four component vector of double precision.
    /// </summary>
    public struct Vector4 :
        IEquatable<Vector4>
    {
        #region private fields

        private double x;
        private double y;
        private double z;
        private double w;

        private bool isNormalized;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of Vector4.
        /// </summary>
        /// <param name="x">X component.</param>
        /// <param name="y">Y component.</param>
        /// <param name="z">Z component.</param>
        /// <param name="w">W component.</param>
        public Vector4(double x, double y, double z, double w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
            this.isNormalized = false;
        }

        /// <summary>
        /// Initializes a new instance of Vector4.
        /// </summary>
        /// <param name="array">Array of four elements that represents the vector.</param>
        public Vector4(double[] array)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (array.Length != 4)
            {
                throw new ArgumentOutOfRangeException(nameof(array), array.Length, "The dimension of the array must be four.");
            }

            this.x = array[0];
            this.y = array[1];
            this.z = array[2];
            this.w = array[3];
            this.isNormalized = false;
        }

        #endregion

        #region constants

        /// <summary>
        /// Zero vector.
        /// </summary>
        public static Vector4 Zero
        {
            get { return new Vector4(0.0, 0.0, 0.0, 0.0); }
        }

        /// <summary>
        /// Unit X vector.
        /// </summary>
        public static Vector4 UnitX
        {
            get { return new Vector4(1.0, 0.0, 0.0, 0.0) {isNormalized = true}; }
        }

        /// <summary>
        /// Unit Y vector.
        /// </summary>
        public static Vector4 UnitY
        {
            get { return new Vector4(0.0, 1.0, 0.0, 0.0) {isNormalized = true}; }
        }

        /// <summary>
        /// Unit Z vector.
        /// </summary>
        public static Vector4 UnitZ
        {
            get { return new Vector4(0, 0, 1, 0) {isNormalized = true}; }
        }

        /// <summary>
        /// Unit W vector.
        /// </summary>
        public static Vector4 UnitW
        {
            get { return new Vector4(0.0, 0.0, 0.0, 1.0) {isNormalized = true}; }
        }

        /// <summary>
        /// Represents a vector with not a number components.
        /// </summary>
        public static Vector4 NaN
        {
            get { return new Vector4(double.NaN, double.NaN, double.NaN, double.NaN); }
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the X component.
        /// </summary>
        public double X
        {
            get { return this.x; }
            set
            {
                this.x = value;
                this.isNormalized = false;
            }
        }

        /// <summary>
        /// Gets or sets the Y component.
        /// </summary>
        public double Y
        {
            get { return this.y; }
            set
            {
                this.y = value;
                this.isNormalized = false;
            }
        }

        /// <summary>
        /// Gets or sets the Z component.
        /// </summary>
        public double Z
        {
            get { return this.z; }
            set
            {
                this.z = value;
                this.isNormalized = false;
            }
        }

        /// <summary>
        /// Gets or sets the W component.
        /// </summary>
        public double W
        {
            get { return this.w; }
            set
            {
                this.w = value;
                this.isNormalized = false;
            }
        }

        /// <summary>
        /// Gets or sets a vector element defined by its index.
        /// </summary>
        /// <param name="index">Index of the element.</param>
        public double this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return this.x;
                    case 1:
                        return this.y;
                    case 2:
                        return this.z;
                    case 3:
                        return this.w;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(index));
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        this.x = value;
                        break;
                    case 1:
                        this.y = value;
                        break;
                    case 2:
                        this.z = value;
                        break;
                    case 3:
                        this.w = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(index));
                }

                this.isNormalized = false;
            }
        }

        /// <summary>
        /// Gets if the vector has been normalized.
        /// </summary>
        public bool IsNormalized
        {
            get { return this.isNormalized; }
        }

        #endregion

        #region static methods

        /// <summary>
        ///  Returns a value indicating if any component of the specified vector evaluates to a value that is not a number <see cref="System.Double.NaN"/>.
        /// </summary>
        /// <param name="u">Vector4.</param>
        /// <returns>Returns true if any component of the specified vector evaluates to <see cref="System.Double.NaN"/>; otherwise, false.</returns>
        public static bool IsNaN(Vector4 u)
        {
            return double.IsNaN(u.X) || double.IsNaN(u.Y) || double.IsNaN(u.Z) || double.IsNaN(u.W);
        }

        /// <summary>
        ///  Returns a value indicating if all components of the specified vector evaluates to zero.
        /// </summary>
        /// <param name="u">Vector4.</param>
        /// <returns>Returns true if all components of the specified vector evaluates to zero; otherwise, false.</returns>
        public static bool IsZero(Vector4 u)
        {
            return MathHelper.IsZero(u.X) && MathHelper.IsZero(u.Y) && MathHelper.IsZero(u.Z) && MathHelper.IsZero(u.W);
        }

        /// <summary>
        /// Obtains the dot product of two vectors.
        /// </summary>
        /// <param name="u">Vector4.</param>
        /// <param name="v">Vector4.</param>
        /// <returns>The dot product.</returns>
        public static double DotProduct(Vector4 u, Vector4 v)
        {
            return u.X * v.X + u.Y * v.Y + u.Z * v.Z + u.W * v.W;
        }

        /// <summary>
        /// Obtains the distance between two points.
        /// </summary>
        /// <param name="u">Vector4.</param>
        /// <param name="v">Vector4.</param>
        /// <returns>Distance.</returns>
        public static double Distance(Vector4 u, Vector4 v)
        {
            return Math.Sqrt(SquareDistance(u, v));
        }

        /// <summary>
        /// Obtains the square distance between two points.
        /// </summary>
        /// <param name="u">Vector4.</param>
        /// <param name="v">Vector4.</param>
        /// <returns>Square distance.</returns>
        public static double SquareDistance(Vector4 u, Vector4 v)
        {
            return (u.X - v.X) * (u.X - v.X) + (u.Y - v.Y) * (u.Y - v.Y) + (u.Z - v.Z) * (u.Z - v.Z) + (u.W - v.Z) * (u.W - v.W);
        }

        /// <summary>
        /// Rounds the components of a vector.
        /// </summary>
        /// <param name="u">Vector to round.</param>
        /// <param name="numDigits">Number of decimal places in the return value.</param>
        /// <returns>The rounded vector.</returns>
        public static Vector4 Round(Vector4 u, int numDigits)
        {
            return new Vector4(Math.Round(u.X, numDigits), Math.Round(u.Y, numDigits), Math.Round(u.Z, numDigits), Math.Round(u.W, numDigits));
        }

        /// <summary>
        /// Normalizes the vector.
        /// </summary>
        /// <param name="u">Vector to normalize</param>
        /// <returns>The normalized vector.</returns>
        public static Vector4 Normalize(Vector4 u)
        {
            if (u.isNormalized)
            {
                return u;
            }

            double mod = u.Modulus();
            if (MathHelper.IsZero(mod))
            {
                return Zero;
            }

            double modInv = 1 / mod;
            return new Vector4(u.X * modInv, u.Y * modInv, u.Z * modInv, u.W * modInv) {isNormalized = true};
        }

        #endregion

        #region overloaded operators

        /// <summary>
        /// Check if the components of two vectors are equal.
        /// </summary>
        /// <param name="u">Vector4.</param>
        /// <param name="v">Vector4.</param>
        /// <returns>True if the three components are equal or false in any other case.</returns>
        public static bool operator ==(Vector4 u, Vector4 v)
        {
            return Equals(u, v);
        }

        /// <summary>
        /// Check if the components of two vectors are different.
        /// </summary>
        /// <param name="u">Vector4.</param>
        /// <param name="v">Vector4.</param>
        /// <returns>True if the three components are different or false in any other case.</returns>
        public static bool operator !=(Vector4 u, Vector4 v)
        {
            return !Equals(u, v);
        }

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="u">Vector4.</param>
        /// <param name="v">Vector4.</param>
        /// <returns>The addition of u plus v.</returns>
        public static Vector4 operator +(Vector4 u, Vector4 v)
        {
            return new Vector4(u.X + v.X, u.Y + v.Y, u.Z + v.Z, u.W + v.W);
        }

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="u">Vector4.</param>
        /// <param name="v">Vector4.</param>
        /// <returns>The addition of u plus v.</returns>
        public static Vector4 Add(Vector4 u, Vector4 v)
        {
            return new Vector4(u.X + v.X, u.Y + v.Y, u.Z + v.Z, u.W + v.W);
        }

        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        /// <param name="u">Vector4.</param>
        /// <param name="v">Vector4.</param>
        /// <returns>The subtraction of u minus v.</returns>
        public static Vector4 operator -(Vector4 u, Vector4 v)
        {
            return new Vector4(u.X - v.X, u.Y - v.Y, u.Z - v.Z, u.W - v.W);
        }

        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        /// <param name="u">Vector4.</param>
        /// <param name="v">Vector4.</param>
        /// <returns>The subtraction of u minus v.</returns>
        public static Vector4 Subtract(Vector4 u, Vector4 v)
        {
            return new Vector4(u.X - v.X, u.Y - v.Y, u.Z - v.Z, u.W - v.W);
        }

        /// <summary>
        /// Negates a vector.
        /// </summary>
        /// <param name="u">Vector4.</param>
        /// <returns>The negative vector of u.</returns>
        public static Vector4 operator -(Vector4 u)
        {
            return new Vector4(-u.X, -u.Y, -u.Z, -u.W) {isNormalized = u.IsNormalized};
        }

        /// <summary>
        /// Negates a vector.
        /// </summary>
        /// <param name="u">Vector4.</param>
        /// <returns>The negative vector of u.</returns>
        public static Vector4 Negate(Vector4 u)
        {
            return new Vector4(-u.X, -u.Y, -u.Z, -u.W) {isNormalized = u.IsNormalized};
        }

        /// <summary>
        /// Multiplies a vector with an scalar (same as a*u, commutative property).
        /// </summary>
        /// <param name="u">Vector4.</param>
        /// <param name="a">Scalar.</param>
        /// <returns>The multiplication of u times a.</returns>
        public static Vector4 operator *(Vector4 u, double a)
        {
            return new Vector4(u.X * a, u.Y * a, u.Z * a, u.W * a);
        }

        /// <summary>
        /// Multiplies a vector with an scalar (same as a*u, commutative property).
        /// </summary>
        /// <param name="u">Vector4.</param>
        /// <param name="a">Scalar.</param>
        /// <returns>The multiplication of u times a.</returns>
        public static Vector4 Multiply(Vector4 u, double a)
        {
            return new Vector4(u.X * a, u.Y * a, u.Z * a, u.W * a);
        }

        /// <summary>
        /// Multiplies a scalar with a vector (same as u*a, commutative property).
        /// </summary>
        /// <param name="a">Scalar.</param>
        /// <param name="u">Vector4.</param>
        /// <returns>The multiplication of u times a.</returns>
        public static Vector4 operator *(double a, Vector4 u)
        {
            return new Vector4(u.X * a, u.Y * a, u.Z * a, u.W * a);
        }

        /// <summary>
        /// Multiplies a scalar with a vector (same as u*a, commutative property).
        /// </summary>
        /// <param name="a">Scalar.</param>
        /// <param name="u">Vector4.</param>
        /// <returns>The multiplication of u times a.</returns>
        public static Vector4 Multiply(double a, Vector4 u)
        {
            return new Vector4(u.X * a, u.Y * a, u.Z * a, u.W * a);
        }

        /// <summary>
        /// Multiplies two vectors component by component.
        /// </summary>
        /// <param name="u">Vector4.</param>
        /// <param name="v">Vector4.</param>
        /// <returns>The multiplication of u times v.</returns>
        public static Vector4 operator *(Vector4 u, Vector4 v)
        {
            return new Vector4(u.X * v.X, u.Y * v.Y, u.Z * v.Z, u.W * v.W);
        }

        /// <summary>
        /// Multiplies two vectors component by component.
        /// </summary>
        /// <param name="u">Vector4.</param>
        /// <param name="v">Vector4.</param>
        /// <returns>The multiplication of u times v.</returns>
        public static Vector4 Multiply(Vector4 u, Vector4 v)
        {
            return new Vector4(u.X * v.X, u.Y * v.Y, u.Z * v.Z, u.W * v.W);
        }

        /// <summary>
        /// Divides an scalar with a vector.
        /// </summary>
        /// <param name="a">Scalar.</param>
        /// <param name="u">Vector4.</param>
        /// <returns>The division of u times a.</returns>
        public static Vector4 operator /(Vector4 u, double a)
        {
            double invScalar = 1 / a;
            return new Vector4(u.X * invScalar, u.Y * invScalar, u.Z * invScalar, u.W * invScalar);
        }

        /// <summary>
        /// Divides a vector with an scalar.
        /// </summary>
        /// <param name="u">Vector4.</param>
        /// <param name="a">Scalar.</param>
        /// <returns>The division of u times a.</returns>
        public static Vector4 Divide(Vector4 u, double a)
        {
            double invScalar = 1 / a;
            return new Vector4(u.X * invScalar, u.Y * invScalar, u.Z * invScalar, u.W * invScalar);
        }

        /// <summary>
        /// Divides two vectors component by component.
        /// </summary>
        /// <param name="u">Vector4.</param>
        /// <param name="v">Vector4.</param>
        /// <returns>The multiplication of u times v.</returns>
        public static Vector4 operator /(Vector4 u, Vector4 v)
        {
            return new Vector4(u.X / v.X, u.Y / v.Y, u.Z / v.Z, u.W / v.W);
        }

        /// <summary>
        /// Divides two vectors component by component.
        /// </summary>
        /// <param name="u">Vector4.</param>
        /// <param name="v">Vector4.</param>
        /// <returns>The multiplication of u times v.</returns>
        public static Vector4 Divide(Vector4 u, Vector4 v)
        {
            return new Vector4(u.X / v.X, u.Y / v.Y, u.Z / v.Z, u.W / v.W);
        }

        #endregion

        #region public methods

        /// <summary>
        /// Normalizes the current vector.
        /// </summary>
        public void Normalize()
        {
            if (this.isNormalized)
            {
                return;
            }

            double mod = this.Modulus();
            if (MathHelper.IsZero(mod))
            {
                this = Zero;
                return;
            }

            double modInv = 1 / mod;
            this.x *= modInv;
            this.y *= modInv;
            this.z *= modInv;
            this.w *= modInv;

            this.isNormalized = true;
        }

        /// <summary>
        /// Obtains the modulus of the vector.
        /// </summary>
        /// <returns>Vector modulus.</returns>
        public double Modulus()
        {
            return this.isNormalized ? 1.0 : Math.Sqrt(DotProduct(this, this));
        }

        /// <summary>
        /// Returns an array that represents the vector.
        /// </summary>
        /// <returns>Array.</returns>
        public double[] ToArray()
        {
            return new[] {this.x, this.y, this.z};
        }

        #endregion

        #region comparison methods

        /// <summary>
        /// Check if the components of two vectors are approximate equal.
        /// </summary>
        /// <param name="a">Vector4.</param>
        /// <param name="b">Vector4.</param>
        /// <returns>True if the four components are almost equal or false in any other case.</returns>
        public static bool Equals(Vector4 a, Vector4 b)
        {
            return a.Equals(b, MathHelper.Epsilon);
        }

        /// <summary>
        /// Check if the components of two vectors are approximate equal.
        /// </summary>
        /// <param name="a">Vector4.</param>
        /// <param name="b">Vector4.</param>
        /// <param name="threshold">Maximum tolerance.</param>
        /// <returns>True if the four components are almost equal or false in any other case.</returns>
        public static bool Equals(Vector4 a, Vector4 b, double threshold)
        {
            return a.Equals(b, threshold);
        }

        /// <summary>
        /// Check if the components of two vectors are approximate equal.
        /// </summary>
        /// <param name="other">Vector4.</param>
        /// <returns>True if the four components are almost equal or false in any other case.</returns>
        public bool Equals(Vector4 other)
        {
            return this.Equals(other, MathHelper.Epsilon);
        }

        /// <summary>
        /// Check if the components of two vectors are approximate equal.
        /// </summary>
        /// <param name="other">Vector4.</param>
        /// <param name="threshold">Maximum tolerance.</param>
        /// <returns>True if the four components are almost equal or false in any other case.</returns>
        public bool Equals(Vector4 other, double threshold)
        {
            return MathHelper.IsEqual(other.X, this.X, threshold) &&
                   MathHelper.IsEqual(other.Y, this.Y, threshold) &&
                   MathHelper.IsEqual(other.Z, this.Z, threshold) &&
                   MathHelper.IsEqual(other.W, this.W, threshold);
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="other">Another object to compare to.</param>
        /// <returns>True if obj and this instance are the same type and represent the same value; otherwise, false.</returns>
        public override bool Equals(object other)
        {
            if (other is Vector4 vector)
            {
                return this.Equals(vector);
            }

            return false;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return this.X.GetHashCode() ^ this.Y.GetHashCode() ^ this.Z.GetHashCode() ^ this.W.GetHashCode();
        }

        #endregion

        #region overrides

        /// <summary>
        /// Obtains a string that represents the vector.
        /// </summary>
        /// <returns>A string text.</returns>
        public override string ToString()
        {
            return string.Format("{0}{4} {1}{4} {2}{4} {3}", this.x, this.y, this.z, this.w, Thread.CurrentThread.CurrentCulture.TextInfo.ListSeparator);
        }

        /// <summary>
        /// Obtains a string that represents the vector.
        /// </summary>
        /// <param name="provider">An IFormatProvider interface implementation that supplies culture-specific formatting information. </param>
        /// <returns>A string text.</returns>
        public string ToString(IFormatProvider provider)
        {
            return string.Format("{0}{4} {1}{4} {2}{4} {3}", this.x.ToString(provider), this.y.ToString(provider), this.z.ToString(provider), this.w.ToString(provider), Thread.CurrentThread.CurrentCulture.TextInfo.ListSeparator);
        }

        #endregion
    }
}