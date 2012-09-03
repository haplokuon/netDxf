#region netDxf, Copyright(C) 2009 Daniel Carvajal, Licensed under LGPL.

//                        netDxf library
// Copyright (C) 2009 Daniel Carvajal (haplokuon@gmail.com)
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 

#endregion

using System;

namespace netDxf
{
    /// <summary>
    /// Represent a three component vector of simple precision.
    /// </summary>
    public struct Vector3f
    {
        #region private fields

        private float x;
        private float y;
        private float z;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of Vector3f.
        /// </summary>
        /// <param name="x">X component.</param>
        /// <param name="y">Y component.</param>
        /// <param name="z">Z component.</param>
        public Vector3f(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// Initializes a new instance of Vector3f.
        /// </summary>
        /// <param name="array">Array of three elements that represents the vector.</param>
        public Vector3f(float[] array)
        {
            if (array.Length != 3)
                throw new ArgumentOutOfRangeException("array", array.Length, "The dimension of the array must be three.");
            this.x = array[0];
            this.y = array[1];
            this.z = array[2];
        }

        #endregion

        #region constants

        /// <summary>
        /// Zero vector.
        /// </summary>
        public static Vector3f Zero
        {
            get { return new Vector3f(0, 0, 0); }
        }

        /// <summary>
        /// Unit X vector.
        /// </summary>
        public static Vector3f UnitX
        {
            get { return new Vector3f(1, 0, 0); }
        }

        /// <summary>
        /// Unit Y vector.
        /// </summary>
        public static Vector3f UnitY
        {
            get { return new Vector3f(0, 1, 0); }
        }

        /// <summary>
        /// Unit Z vector.
        /// </summary>
        public static Vector3f UnitZ
        {
            get { return new Vector3f(0, 0, 1); }
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the X component.
        /// </summary>
        public float X
        {
            get { return this.x; }
            set { this.x = value; }
        }

        /// <summary>
        /// Gets or sets the Y component.
        /// </summary>
        public float Y
        {
            get { return this.y; }
            set { this.y = value; }
        }

        /// <summary>
        /// Gets or sets the Z component.
        /// </summary>
        public float Z
        {
            get { return this.z; }
            set { this.z = value; }
        }

        /// <summary>
        /// Gets or sets a vector element defined by its index.
        /// </summary>
        /// <param name="index">Index of the element.</param>
        public float this[int index]
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
                    default:
                        throw (new ArgumentOutOfRangeException("index"));
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
                    default:
                        throw (new ArgumentOutOfRangeException("index"));
                }
            }
        }

        #endregion

        #region static methods

        /// <summary>
        /// Obtains the dot product of two vectors.
        /// </summary>
        /// <param name="u">Vector3f.</param>
        /// <param name="v">Vector3f.</param>
        /// <returns>The dot product.</returns>
        public static float DotProduct(Vector3f u, Vector3f v)
        {
            return (u.X*v.X) + (u.Y*v.Y) + (u.Z*v.Z);
        }

        /// <summary>
        /// Obtains the cross product of two vectors.
        /// </summary>
        /// <param name="u">Vector3f.</param>
        /// <param name="v">Vector3f.</param>
        /// <returns>Vector3f.</returns>
        public static Vector3f CrossProduct(Vector3f u, Vector3f v)
        {
            float a = u.Y*v.Z - u.Z*v.Y;
            float b = u.Z*v.X - u.X*v.Z;
            float c = u.X*v.Y - u.Y*v.X;
            return new Vector3f(a, b, c);
        }

        /// <summary>
        /// Obtains the distance between two points.
        /// </summary>
        /// <param name="u">Vector3f.</param>
        /// <param name="v">Vector3f.</param>
        /// <returns>Distancie.</returns>
        public static float Distance(Vector3f u, Vector3f v)
        {
            return (float) (Math.Sqrt((u.X - v.X)*(u.X - v.X) + (u.Y - v.Y)*(u.Y - v.Y) + (u.Z - v.Z)*(u.Z - v.Z)));
        }

        /// <summary>
        /// Obtains the square distance between two points.
        /// </summary>
        /// <param name="u">Vector3f.</param>
        /// <param name="v">Vector3f.</param>
        /// <returns>Square distance.</returns>
        public static float SquareDistance(Vector3f u, Vector3f v)
        {
            return (u.X - v.X)*(u.X - v.X) + (u.Y - v.Y)*(u.Y - v.Y) + (u.Z - v.Z)*(u.Z - v.Z);
        }

        /// <summary>
        /// Obtains the angle between two vectors.
        /// </summary>
        /// <param name="u">Vector3f.</param>
        /// <param name="v">Vector3f.</param>
        /// <returns>Angle in radians.</returns>
        public static float AngleBetween(Vector3f u, Vector3f v)
        {
            float cos = DotProduct(u, v)/(u.Modulus()*v.Modulus());
            if (MathHelper.IsOne(cos))
            {
                return 0;
            }
            if (MathHelper.IsOne(-cos))
            {
                return (float) Math.PI;
            }
            return (float) Math.Acos(cos);
        }

        /// <summary>
        /// Obtains the midpoint.
        /// </summary>
        /// <param name="u">Vector3f.</param>
        /// <param name="v">Vector3f.</param>
        /// <returns>Vector3f.</returns>
        public static Vector3f MidPoint(Vector3f u, Vector3f v)
        {
            return new Vector3f((v.X + u.X)*0.5F, (v.Y + u.Y)*0.5F, (v.Z + u.Z)*0.5F);
        }

        /// <summary>
        /// Checks if two vectors are perpendicular.
        /// </summary>
        /// <param name="u">Vector3f.</param>
        /// <param name="v">Vector3f.</param>
        /// <param name="threshold">Tolerance used.</param>
        /// <returns>True if are penpendicular or false in anyother case.</returns>
        public static bool ArePerpendicular(Vector3f u, Vector3f v, float threshold)
        {
            return MathHelper.IsZero(DotProduct(u, v), threshold);
        }

        /// <summary>
        /// Checks if two vectors are parallel.
        /// </summary>
        /// <param name="u">Vector3f.</param>
        /// <param name="v">Vector3f.</param>
        /// <param name="threshold">Tolerance used.</param>
        /// <returns>True if are parallel or false in anyother case.</returns>
        public static bool AreParallel(Vector3f u, Vector3f v, float threshold)
        {
            float a = u.Y*v.Z - u.Z*v.Y;
            float b = u.Z*v.X - u.X*v.Z;
            float c = u.X*v.Y - u.Y*v.X;
            if (! MathHelper.IsZero(a, threshold))
            {
                return false;
            }
            if (!MathHelper.IsZero(b, threshold))
            {
                return false;
            }
            if (!MathHelper.IsZero(c, threshold))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Rounds the components of a vector.
        /// </summary>
        /// <param name="u">Vector3f.</param>
        /// <param name="numDigits">Number of significative defcimal digits.</param>
        /// <returns>Vector3F.</returns>
        public static Vector3f Round(Vector3f u, int numDigits)
        {
            return new Vector3f((float) (Math.Round(u.X, numDigits)),
                                (float) (Math.Round(u.Y, numDigits)),
                                (float) (Math.Round(u.Z, numDigits)));
        }

        #endregion

        #region overloaded operators
        public static explicit operator Vector3d(Vector3f u)
        {
           return new Vector3d(u.X,u.Y,u.Z);
        }

        public static bool operator ==(Vector3f u, Vector3f v)
        {
            return ((v.X == u.X) && (v.Y == u.Y) && (v.Z == u.Z));
        }

        public static bool operator !=(Vector3f u, Vector3f v)
        {
            return ((v.X != u.X) || (v.Y != u.Y) || (v.Z != u.Z));
        }


        public static Vector3f operator +(Vector3f u, Vector3f v)
        {
            return new Vector3f(u.X + v.X, u.Y + v.Y, u.Z + v.Z);
        }

        public static Vector3f operator -(Vector3f u, Vector3f v)
        {
            return new Vector3f(u.X - v.X, u.Y - v.Y, u.Z - v.Z);
        }

        public static Vector3f operator -(Vector3f u)
        {
            return new Vector3f(- u.X, - u.Y, - u.Z);
        }

        public static Vector3f operator *(Vector3f u, float a)
        {
            return new Vector3f(u.X*a, u.Y*a, u.Z*a);
        }

        public static Vector3f operator *(float a, Vector3f u)
        {
            return new Vector3f(u.X*a, u.Y*a, u.Z*a);
        }

        public static Vector3f operator /(Vector3f u, float a)
        {
            float invEscalar = 1/a;
            return new Vector3f(u.X*invEscalar, u.Y*invEscalar, u.Z*invEscalar);
        }


        public static Vector3f operator /(float a, Vector3f u)
        {
            float invEscalar = 1/a;
            return new Vector3f(u.X*invEscalar, u.Y*invEscalar, u.Z*invEscalar);
        }

        #endregion

        #region public methods

        /// <summary>
        /// Normalizes the vector.
        /// </summary>
        public void Normalize()
        {
            float mod = this.Modulus();
            if (MathHelper.IsZero(mod))
                throw new ArithmeticException("Cannot normalize a zero vector.");
            float modInv = 1/mod;
            this.x *= modInv;
            this.y *= modInv;
            this.z *= modInv;
        }

        /// <summary>
        /// Obtains the modulus of the vector.
        /// </summary>
        /// <returns>Vector modulus.</returns>
        public float Modulus()
        {
            return (float) Math.Sqrt(DotProduct(this, this));
        }

        /// <summary>
        /// Returns an array that represents the vector.
        /// </summary>
        /// <returns>Array.</returns>
        public float[] ToArray()
        {
            float[] u = new[] {this.x, this.y, this.z};
            return u;
        }
        
        #endregion

        #region comparision methods

        /// <summary>
        /// Check if the components of two vectors are approximate equals.
        /// </summary>
        /// <param name="obj">Vector3f.</param>
        /// <param name="threshold">Maximun tolerance.</param>
        /// <returns>True if the three components are almost equal or false in anyother case.</returns>
        public bool Equals(Vector3f obj, float threshold)
        {
            if (Math.Abs(obj.X - this.x) > threshold)
            {
                return false;
            }
            if (Math.Abs(obj.Y - this.y) > threshold)
            {
                return false;
            }
            if (Math.Abs(obj.Z - this.z) > threshold)
            {
                return false;
            }

            return true;
        }

        public bool Equals(Vector3f obj)
        {
            return obj.x == this.x && obj.y == this.y && obj.z == this.z;
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector3f)
                return this.Equals((Vector3f) obj);
            return false;
        }

        public override int GetHashCode()
        {
            return unchecked(this.x.GetHashCode() ^ this.y.GetHashCode() ^ this.z.GetHashCode());
        }

        #endregion

        #region overrides

        /// <summary>
        /// Obtains a string that represents the vector.
        /// </summary>
        /// <returns>A string text.</returns>
        public override string ToString()
        {
            return string.Format("{0};{1};{2}", this.x, this.y, this.z);
        }

        /// <summary>
        /// Obtains a string that represents the vector.
        /// </summary>
        /// <param name="provider">An IFormatProvider interface implementation that supplies culture-specific formatting information. </param>
        /// <returns>A string text.</returns>
        public string ToString(IFormatProvider provider)
        {
            return string.Format("{0};{1};{2}", this.x.ToString(provider), this.y.ToString(provider), this.z.ToString(provider));
        }

        #endregion

    }
}