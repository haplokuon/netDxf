#region netDxf library licensed under the MIT License
// 
//                       netDxf library
// Copyright (c) 2019-2021 Daniel Carvajal (haplokuon@gmail.com)
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
using System.Text;
using System.Threading;

namespace netDxf
{
    /// <summary>
    /// Represents a 4x4 double precision matrix.
    /// </summary>
    public struct Matrix4 :
        IEquatable<Matrix4>
    {
        #region private fields

        private double m11;
        private double m12;
        private double m13;
        private double m14;
        private double m21;
        private double m22;
        private double m23;
        private double m24;
        private double m31;
        private double m32;
        private double m33;
        private double m34;
        private double m41;
        private double m42;
        private double m43;
        private double m44;

        private bool dirty;
        private bool isIdentity;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of Matrix4.
        /// </summary>
        /// <param name="m11">Element [0,0].</param>
        /// <param name="m12">Element [0,1].</param>
        /// <param name="m13">Element [0,2].</param>
        /// <param name="m14">Element [0,3].</param>
        /// <param name="m21">Element [1,0].</param>
        /// <param name="m22">Element [1,1].</param>
        /// <param name="m23">Element [1,2].</param>
        /// <param name="m24">Element [1,3].</param>
        /// <param name="m31">Element [2,0].</param>
        /// <param name="m32">Element [2,1].</param>
        /// <param name="m33">Element [2,2].</param>
        /// <param name="m34">Element [2,3].</param>
        /// <param name="m41">Element [3,0].</param>
        /// <param name="m42">Element [3,1].</param>
        /// <param name="m43">Element [3,2].</param>
        /// <param name="m44">Element [3,3].</param>
        public Matrix4(double m11, double m12, double m13, double m14,
                       double m21, double m22, double m23, double m24,
                       double m31, double m32, double m33, double m34,
                       double m41, double m42, double m43, double m44)
        {
            this.m11 = m11;
            this.m12 = m12;
            this.m13 = m13;
            this.m14 = m14;

            this.m21 = m21;
            this.m22 = m22;
            this.m23 = m23;
            this.m24 = m24;

            this.m31 = m31;
            this.m32 = m32;
            this.m33 = m33;
            this.m34 = m34;

            this.m41 = m41;
            this.m42 = m42;
            this.m43 = m43;
            this.m44 = m44;

            this.dirty = true;
            this.isIdentity = false;
        }

        #endregion

        #region constants

        /// <summary>
        /// Gets the zero matrix.
        /// </summary>
        public static Matrix4 Zero
        {
            get
            {
                return new Matrix4(0.0, 0.0, 0.0, 0.0,
                                   0.0, 0.0, 0.0, 0.0,
                                   0.0, 0.0, 0.0, 0.0,
                                   0.0, 0.0, 0.0, 0.0) {dirty = false, isIdentity = false};
            }
        }

        /// <summary>
        /// Gets the identity matrix.
        /// </summary>
        public static Matrix4 Identity
        {
            get
            {
                return new Matrix4(1.0, 0.0, 0.0, 0.0,
                                   0.0, 1.0, 0.0, 0.0,
                                   0.0, 0.0, 1.0, 0.0,
                                   0.0, 0.0, 0.0, 1.0) {dirty = false, isIdentity = true};
            }
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the matrix element [0,0].
        /// </summary>
        public double M11
        {
            get { return this.m11; }
            set
            {
                this.m11 = value;
                this.dirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the matrix element [0,1].
        /// </summary>
        public double M12
        {
            get { return this.m12; }
            set
            {
                this.m12 = value;
                this.dirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the matrix element [0,2].
        /// </summary>
        public double M13
        {
            get { return this.m13; }
            set
            {
                this.m13 = value;
                this.dirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the matrix element [0,3].
        /// </summary>
        public double M14
        {
            get { return this.m14; }
            set
            {
                this.m14 = value;
                this.dirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the matrix element [1,0].
        /// </summary>
        public double M21
        {
            get { return this.m21; }
            set
            {
                this.m21 = value;
                this.dirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the matrix element [1,1].
        /// </summary>
        public double M22
        {
            get { return this.m22; }
            set
            {
                this.m22 = value;
                this.dirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the matrix element [1,2].
        /// </summary>
        public double M23
        {
            get { return this.m23; }
            set
            {
                this.m23 = value;
                this.dirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the matrix element [1,3].
        /// </summary>
        public double M24
        {
            get { return this.m24; }
            set
            {
                this.m24 = value;
                this.dirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the matrix element [2,0].
        /// </summary>
        public double M31
        {
            get { return this.m31; }
            set
            {
                this.m31 = value;
                this.dirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the matrix element [2,1].
        /// </summary>
        public double M32
        {
            get { return this.m32; }
            set
            {
                this.m32 = value;
                this.dirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the matrix element [2,2].
        /// </summary>
        public double M33
        {
            get { return this.m33; }
            set
            {
                this.m33 = value;
                this.dirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the matrix element [2,3].
        /// </summary>
        public double M34
        {
            get { return this.m34; }
            set
            {
                this.m34 = value;
                this.dirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the matrix element [3,0].
        /// </summary>
        public double M41
        {
            get { return this.m41; }
            set
            {
                this.m41 = value;
                this.dirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the matrix element [3,1].
        /// </summary>
        public double M42
        {
            get { return this.m42; }
            set
            {
                this.m42 = value;
                this.dirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the matrix element [3,2].
        /// </summary>
        public double M43
        {
            get { return this.m43; }
            set
            {
                this.m43 = value;
                this.dirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the matrix element [3,3].
        /// </summary>
        public double M44
        {
            get { return this.m44; }
            set
            {
                this.m44 = value;
                this.dirty = true;
            }
        }

        /// <summary>Gets or sets the component at the given row and column index in the matrix.</summary>
        /// <param name="row">The row index of the matrix.</param>
        /// <param name="column">The column index of the matrix.</param>
        /// <returns>The component at the given row and column index in the matrix.</returns>
        public double this[int row, int column]
        {
            get
            {
                switch (row)
                {
                    case 0:
                        switch (column)
                        {
                            case 0:
                                return this.m11;
                            case 1:
                                return this.m12;
                            case 2:
                                return this.m13;
                            case 3:
                                return this.m14;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(column));
                        }
                    case 1:
                        switch (column)
                        {
                            case 0:
                                return this.m21;
                            case 1:
                                return this.m22;
                            case 2:
                                return this.m23;
                            case 3:
                                return this.m24;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(column));
                        }

                    case 2:
                        switch (column)
                        {
                            case 0:
                                return this.m31;
                            case 1:
                                return this.m32;
                            case 2:
                                return this.m33;
                            case 3:
                                return this.m34;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(column));
                        }

                    case 3:
                        switch (column)
                        {
                            case 0:
                                return this.m41;
                            case 1:
                                return this.m42;
                            case 2:
                                return this.m43;
                            case 3:
                                return this.m44;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(column));
                        }

                    default:
                        throw new ArgumentOutOfRangeException(nameof(row));
                }
            }
            set
            {
                switch (row)
                {
                    case 0:
                        switch (column)
                        {
                            case 0:
                                this.m11 = value;
                                break;
                            case 1:
                                this.m12 = value;
                                break;
                            case 2:
                                this.m13 = value;
                                break;
                            case 3:
                                this.m14 = value;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(column));
                        }
                        break;

                    case 1:
                        switch (column)
                        {
                            case 0:
                                this.m21 = value;
                                break;
                            case 1:
                                this.m22 = value;
                                break;
                            case 2:
                                this.m23 = value;
                                break;
                            case 3:
                                this.m24 = value;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(column));
                        }
                        break;

                    case 2:
                        switch (column)
                        {
                            case 0:
                                this.m31 = value;
                                break;
                            case 1:
                                this.m32 = value;
                                break;
                            case 2:
                                this.m33 = value;
                                break;
                            case 3:
                                this.m34 = value;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(column));
                        }
                        break;

                    case 3:
                        switch (column)
                        {
                            case 0:
                                this.m41 = value;
                                break;
                            case 1:
                                this.m42 = value;
                                break;
                            case 2:
                                this.m43 = value;
                                break;
                            case 3:
                                this.m44 = value;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(column));
                        }
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(row));
                }
                this.dirty = true;
            }
        }

        /// <summary>
        /// Gets if the actual matrix is the identity.
        /// </summary>
        /// <remarks>
        /// The checks to see if the matrix is the identity uses the MathHelper.Epsilon as a the threshold for testing values close to one and zero.
        /// </remarks>
        public bool IsIdentity
        {
            get
            {
                if (this.dirty)
                {
                    this.dirty = false;

                    // row 1
                    if (!MathHelper.IsOne(this.M11))
                    {
                        this.isIdentity = false;
                        return this.isIdentity;
                    }
                    if (!MathHelper.IsZero(this.M12))
                    {
                        this.isIdentity = false;
                        return this.isIdentity;
                    }
                    if (!MathHelper.IsZero(this.M13))
                    {
                        this.isIdentity = false;
                        return this.isIdentity;
                    }
                    if (!MathHelper.IsZero(this.M14))
                    {
                        this.isIdentity = false;
                        return this.isIdentity;
                    }

                    // row 2
                    if (!MathHelper.IsZero(this.M21))
                    {
                        this.isIdentity = false;
                        return this.isIdentity;
                    }
                    if (!MathHelper.IsOne(this.M22))
                    {
                        this.isIdentity = false;
                        return this.isIdentity;
                    }
                    if (!MathHelper.IsZero(this.M23))
                    {
                        this.isIdentity = false;
                        return this.isIdentity;
                    }
                    if (!MathHelper.IsZero(this.M24))
                    {
                        this.isIdentity = false;
                        return this.isIdentity;
                    }

                    // row 3
                    if (!MathHelper.IsZero(this.M31))
                    {
                        this.isIdentity = false;
                        return this.isIdentity;
                    }
                    if (!MathHelper.IsZero(this.M32))
                    {
                        this.isIdentity = false;
                        return this.isIdentity;
                    }
                    if (!MathHelper.IsOne(this.M33))
                    {
                        this.isIdentity = false;
                        return this.isIdentity;
                    }
                    if (!MathHelper.IsZero(this.M34))
                    {
                        this.isIdentity = false;
                        return this.isIdentity;
                    }

                    // row 4
                    if (!MathHelper.IsZero(this.M41))
                    {
                        this.isIdentity = false;
                        return this.isIdentity;
                    }
                    if (!MathHelper.IsZero(this.M42))
                    {
                        this.isIdentity = false;
                        return this.isIdentity;
                    }
                    if (!MathHelper.IsZero(this.M43))
                    {
                        this.isIdentity = false;
                        return this.isIdentity;
                    }
                    if (!MathHelper.IsOne(this.M44))
                    {
                        this.isIdentity = false;
                        return this.isIdentity;
                    }

                    this.isIdentity = true;
                    return this.isIdentity;
                }

                return this.isIdentity;
            }
        }

        #endregion

        #region overloaded operators

        /// <summary>
        /// Matrix addition.
        /// </summary>
        /// <param name="a">Matrix4.</param>
        /// <param name="b">Matrix4.</param>
        /// <returns>Matrix4.</returns>
        public static Matrix4 operator +(Matrix4 a, Matrix4 b)
        {
            return new Matrix4(a.M11 + b.M11, a.M12 + b.M12, a.M13 + b.M13, a.m14 + b.m14,
                               a.M21 + b.M21, a.M22 + b.M22, a.M23 + b.M23, a.m24 + b.m24,
                               a.M31 + b.M31, a.M32 + b.M32, a.M33 + b.M33, a.m34 + b.m34,
                               a.M41 + b.M41, a.M42 + b.M42, a.M43 + b.M43, a.m44 + b.m44);
        }

        /// <summary>
        /// Matrix addition.
        /// </summary>
        /// <param name="a">Matrix4.</param>
        /// <param name="b">Matrix4.</param>
        /// <returns>Matrix4.</returns>
        public static Matrix4 Add(Matrix4 a, Matrix4 b)
        {
            return new Matrix4(a.M11 + b.M11, a.M12 + b.M12, a.M13 + b.M13, a.m14 + b.m14,
                               a.M21 + b.M21, a.M22 + b.M22, a.M23 + b.M23, a.m24 + b.m24,
                               a.M31 + b.M31, a.M32 + b.M32, a.M33 + b.M33, a.m34 + b.m34,
                               a.M41 + b.M41, a.M42 + b.M42, a.M43 + b.M43, a.m44 + b.m44);
        }

        /// <summary>
        /// Matrix subtraction.
        /// </summary>
        /// <param name="a">Matrix4.</param>
        /// <param name="b">Matrix4.</param>
        /// <returns>Matrix4.</returns>
        public static Matrix4 operator -(Matrix4 a, Matrix4 b)
        {
            return new Matrix4(a.M11 - b.M11, a.M12 - b.M12, a.M13 - b.M13, a.m14 - b.m14,
                               a.M21 - b.M21, a.M22 - b.M22, a.M23 - b.M23, a.m24 - b.m24,
                               a.M31 - b.M31, a.M32 - b.M32, a.M33 - b.M33, a.m34 - b.m34,
                               a.M41 - b.M41, a.M42 - b.M42, a.M43 - b.M43, a.m44 - b.m44);
        }

        /// <summary>
        /// Matrix subtraction.
        /// </summary>
        /// <param name="a">Matrix4.</param>
        /// <param name="b">Matrix4.</param>
        /// <returns>Matrix4.</returns>
        public static Matrix4 Subtract(Matrix4 a, Matrix4 b)
        {
            return new Matrix4(a.M11 - b.M11, a.M12 - b.M12, a.M13 - b.M13, a.m14 - b.m14,
                               a.M21 - b.M21, a.M22 - b.M22, a.M23 - b.M23, a.m24 - b.m24,
                               a.M31 - b.M31, a.M32 - b.M32, a.M33 - b.M33, a.m34 - b.m34,
                               a.M41 - b.M41, a.M42 - b.M42, a.M43 - b.M43, a.m44 - b.m44);
        }

        /// <summary>
        /// Product of two matrices.
        /// </summary>
        /// <param name="a">Matrix4.</param>
        /// <param name="b">Matrix4.</param>
        /// <returns>Matrix4.</returns>
        public static Matrix4 operator *(Matrix4 a, Matrix4 b)
        {
            if (a.IsIdentity)
            {
                return b;
            }

            if (b.IsIdentity)
            {
                return a;
            }

            return new Matrix4(a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31 + a.M14 * b.M41, a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32 + a.M14 * b.M42, a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33 + a.M14 * b.M43, a.M11 * b.M14 + a.M12 * b.M24 + a.M13 * b.M34 + a.M14 * b.M44,
                               a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31 + a.M24 * b.M41, a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32 + a.M24 * b.M42, a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33 + a.M24 * b.M43, a.M21 * b.M14 + a.M22 * b.M24 + a.M23 * b.M34 + a.M24 * b.M44,
                               a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31 + a.M34 * b.M41, a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32 + a.M34 * b.M42, a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33 + a.M34 * b.M43, a.M31 * b.M14 + a.M32 * b.M24 + a.M33 * b.M34 + a.M34 * b.M44,
                               a.M41 * b.M11 + a.M42 * b.M21 + a.M43 * b.M31 + a.M44 * b.M41, a.M41 * b.M12 + a.M42 * b.M22 + a.M43 * b.M32 + a.M44 * b.M42, a.M41 * b.M13 + a.M42 * b.M23 + a.M43 * b.M33 + a.M44 * b.M43, a.M41 * b.M14 + a.M42 * b.M24 + a.M43 * b.M34 + a.M44 * b.M44);
        }

        /// <summary>
        /// Product of two matrices.
        /// </summary>
        /// <param name="a">Matrix4.</param>
        /// <param name="b">Matrix4.</param>
        /// <returns>Matrix4.</returns>
        public static Matrix4 Multiply(Matrix4 a, Matrix4 b)
        {
            if (a.IsIdentity)
            {
                return b;
            }

            if (b.IsIdentity)
            {
                return a;
            }

            return new Matrix4(a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31 + a.M14 * b.M41, a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32 + a.M14 * b.M42, a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33 + a.M14 * b.M43, a.M11 * b.M14 + a.M12 * b.M24 + a.M13 * b.M34 + a.M14 * b.M44,
                               a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31 + a.M24 * b.M41, a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32 + a.M24 * b.M42, a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33 + a.M24 * b.M43, a.M21 * b.M14 + a.M22 * b.M24 + a.M23 * b.M34 + a.M24 * b.M44,
                               a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31 + a.M34 * b.M41, a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32 + a.M34 * b.M42, a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33 + a.M34 * b.M43, a.M31 * b.M14 + a.M32 * b.M24 + a.M33 * b.M34 + a.M34 * b.M44,
                               a.M41 * b.M11 + a.M42 * b.M21 + a.M43 * b.M31 + a.M44 * b.M41, a.M41 * b.M12 + a.M42 * b.M22 + a.M43 * b.M32 + a.M44 * b.M42, a.M41 * b.M13 + a.M42 * b.M23 + a.M43 * b.M33 + a.M44 * b.M43, a.M41 * b.M14 + a.M42 * b.M24 + a.M43 * b.M34 + a.M44 * b.M44);
        }

        /// <summary>
        /// Product of a matrix with a vector.
        /// </summary>
        /// <param name="a">Matrix4.</param>
        /// <param name="u">Vector4.</param>
        /// <returns>Matrix4.</returns>
        /// <remarks>Matrix4 adopts the convention of using column vectors.</remarks>
        public static Vector4 operator *(Matrix4 a, Vector4 u)
        {
            return a.IsIdentity ? u : new Vector4(a.M11 * u.X + a.M12 * u.Y + a.M13 * u.Z + a.M14 * u.W,
                                                  a.M21 * u.X + a.M22 * u.Y + a.M23 * u.Z + a.M24 * u.W,
                                                  a.M31 * u.X + a.M32 * u.Y + a.M33 * u.Z + a.M34 * u.W,
                                                  a.M41 * u.X + a.M42 * u.Y + a.M43 * u.Z + a.M44 * u.W);
        }

        /// <summary>
        /// Product of a matrix with a vector.
        /// </summary>
        /// <param name="a">Matrix4.</param>
        /// <param name="u">Vector4.</param>
        /// <returns>Matrix4.</returns>
        /// <remarks>Matrix4 adopts the convention of using column vectors to represent three dimensional points.</remarks>
        public static Vector4 Multiply(Matrix4 a, Vector4 u)
        {
            return a.IsIdentity ? u : new Vector4(a.M11 * u.X + a.M12 * u.Y + a.M13 * u.Z + a.M14 * u.W,
                                                  a.M21 * u.X + a.M22 * u.Y + a.M23 * u.Z + a.M24 * u.W,
                                                  a.M31 * u.X + a.M32 * u.Y + a.M33 * u.Z + a.M34 * u.W,
                                                  a.M41 * u.X + a.M42 * u.Y + a.M43 * u.Z + a.M44 * u.W);
        }

        /// <summary>
        /// Product of a matrix with a scalar.
        /// </summary>
        /// <param name="m">Matrix4.</param>
        /// <param name="a">Scalar.</param>
        /// <returns>Matrix4.</returns>
        public static Matrix4 operator *(Matrix4 m, double a)
        {
            return new Matrix4(m.M11 * a, m.M12 * a, m.M13 * a, m.M14 * a,
                               m.M21 * a, m.M22 * a, m.M23 * a, m.M24 * a,
                               m.M31 * a, m.M32 * a, m.M33 * a, m.M34 * a,
                               m.M41 * a, m.M42 * a, m.M43 * a, m.M44 * a);
        }

        /// <summary>
        /// Product of a matrix with a scalar.
        /// </summary>
        /// <param name="m">Matrix4.</param>
        /// <param name="a">Scalar.</param>
        /// <returns>Matrix4.</returns>
        public static Matrix4 Multiply(Matrix4 m, double a)
        {
            return new Matrix4(m.M11 * a, m.M12 * a, m.M13 * a, m.M14 * a,
                               m.M21 * a, m.M22 * a, m.M23 * a, m.M24 * a,
                               m.M31 * a, m.M32 * a, m.M33 * a, m.M34 * a,
                               m.M41 * a, m.M42 * a, m.M43 * a, m.M44 * a);
        }

        /// <summary>
        /// Check if the components of two matrices are equal.
        /// </summary>
        /// <param name="u">Matrix4.</param>
        /// <param name="v">Matrix4.</param>
        /// <returns>True if the matrix components are equal or false in any other case.</returns>
        public static bool operator ==(Matrix4 u, Matrix4 v)
        {
            return Equals(u, v);
        }

        /// <summary>
        /// Check if the components of two matrices are different.
        /// </summary>
        /// <param name="u">Matrix4.</param>
        /// <param name="v">Matrix4.</param>
        /// <returns>True if the matrix components are different or false in any other case.</returns>
        public static bool operator !=(Matrix4 u, Matrix4 v)
        {
            return !Equals(u, v);
        }

        #endregion

        #region public methods

        /// <summary>
        /// Calculate the determinant of the actual matrix.
        /// </summary>
        /// <returns>Determinant.</returns>
        public double Determinant()
        {
            return this.IsIdentity
                ? 1.0
                : this.m11 * (this.m22 * (this.m33 * this.m44 - this.m34 * this.m43) - this.m23 * (this.m32 * this.m44 - this.m34 * this.m42) + this.m24 * (this.m32 * this.m43 - this.m33 * this.m42)) -
                  this.m12 * (this.m21 * (this.m33 * this.m44 - this.m34 * this.m43) - this.m23 * (this.m31 * this.m44 - this.m34 * this.m41) + this.m24 * (this.m31 * this.m43 - this.m33 * this.m41)) +
                  this.m13 * (this.m21 * (this.m32 * this.m44 - this.m34 * this.m42) - this.m22 * (this.m31 * this.m44 - this.m34 * this.m41) + this.m24 * (this.m31 * this.m42 - this.m32 * this.m41)) -
                  this.m14 * (this.m21 * (this.m32 * this.m43 - this.m33 * this.m42) - this.m22 * (this.m31 * this.m43 - this.m33 * this.m41) + this.m23 * (this.m31 * this.m42 - this.m32 * this.m41));
        }

        /// <summary>
        /// Calculates the inverse matrix.
        /// </summary>
        /// <returns>Inverse Matrix4.</returns>
        public Matrix4 Inverse()
        {
            if (this.IsIdentity)
            {
                return Identity;
            }

            double det = this.Determinant();
            if (MathHelper.IsZero(det))
            {
                throw new ArithmeticException("The matrix is not invertible.");
            }

            det = 1 / det;

            return new Matrix4(
                det * (this.m22 * (this.m33 * this.m44 - this.m34 * this.m43) - this.m23 * (this.m32 * this.m44 - this.m34 * this.m42) + this.m24 * (this.m32 * this.m43 - this.m33 * this.m42)),
                det * -(this.m12 * (this.m33 * this.m44 - this.m34 * this.m43) - this.m13 * (this.m32 * this.m44 - this.m34 * this.m42) + this.m14 * (this.m32 * this.m43 - this.m33 * this.m42)),
                det * (this.m12 * (this.m23 * this.m44 - this.m24 * this.m43) - this.m13 * (this.m22 * this.m44 - this.m24 * this.m42) + this.m14 * (this.m22 * this.m43 - this.m23 * this.m42)),
                det * -(this.m12 * (this.m23 * this.m34 - this.m24 * this.m33) - this.m13 * (this.m22 * this.m34 - this.m24 * this.m32) + this.m14 * (this.m22 * this.m33 - this.m23 * this.m32)),
                det * -(this.m21 * (this.m33 * this.m44 - this.m34 * this.m43) - this.m23 * (this.m31 * this.m44 - this.m34 * this.m41) + this.m24 * (this.m31 * this.m43 - this.m33 * this.m41)),
                det * (this.m11 * (this.m33 * this.m44 - this.m34 * this.m43) - this.m13 * (this.m31 * this.m44 - this.m34 * this.m41) + this.m14 * (this.m31 * this.m43 - this.m33 * this.m41)),
                det * -(this.m11 * (this.m23 * this.m44 - this.m24 * this.m43) - this.m13 * (this.m21 * this.m44 - this.m24 * this.m41) + this.m14 * (this.m21 * this.m43 - this.m23 * this.m41)),
                det * (this.m11 * (this.m23 * this.m34 - this.m24 * this.m33) - this.m13 * (this.m21 * this.m34 - this.m24 * this.m31) + this.m14 * (this.m21 * this.m33 - this.m23 * this.m31)),
                det * (this.m21 * (this.m32 * this.m44 - this.m34 * this.m42) - this.m22 * (this.m31 * this.m44 - this.m34 * this.m41) + this.m24 * (this.m31 * this.m42 - this.m32 * this.m41)),
                det * -(this.m11 * (this.m32 * this.m44 - this.m34 * this.m42) - this.m12 * (this.m31 * this.m44 - this.m34 * this.m41) + this.m14 * (this.m31 * this.m42 - this.m32 * this.m41)),
                det * (this.m11 * (this.m22 * this.m44 - this.m24 * this.m42) - this.m12 * (this.m21 * this.m44 - this.m24 * this.m41) + this.m14 * (this.m21 * this.m42 - this.m22 * this.m41)),
                det * -(this.m11 * (this.m22 * this.m34 - this.m24 * this.m32) - this.m12 * (this.m21 * this.m34 - this.m24 * this.m31) + this.m14 * (this.m21 * this.m32 - this.m22 * this.m31)),
                det * -(this.m21 * (this.m32 * this.m43 - this.m33 * this.m42) - this.m22 * (this.m31 * this.m43 - this.m33 * this.m41) + this.m23 * (this.m31 * this.m42 - this.m32 * this.m41)),
                det * (this.m11 * (this.m32 * this.m43 - this.m33 * this.m42) - this.m12 * (this.m31 * this.m43 - this.m33 * this.m41) + this.m13 * (this.m31 * this.m42 - this.m32 * this.m41)),
                det * -(this.m11 * (this.m22 * this.m43 - this.m23 * this.m42) - this.m12 * (this.m21 * this.m43 - this.m23 * this.m41) + this.m13 * (this.m21 * this.m42 - this.m22 * this.m41)),
                det * (this.m11 * (this.m22 * this.m33 - this.m23 * this.m32) - this.m12 * (this.m21 * this.m33 - this.m23 * this.m31) + this.m13 * (this.m21 * this.m32 - this.m22 * this.m31)));
        }

        /// <summary>
        /// Obtains the transpose matrix.
        /// </summary>
        /// <returns>Transpose matrix.</returns>
        public Matrix4 Transpose()
        {
            return this.IsIdentity ? Identity : new Matrix4(this.m11, this.m21, this.m31, this.m41,
                                                            this.m12, this.m22, this.m32, this.m42,
                                                            this.m13, this.m23, this.m33, this.m43,
                                                            this.m14, this.m24, this.m34, this.m44);
        }

        #endregion

        #region static methods

        /// <summary>
        /// Builds a rotation matrix for a rotation around the x-axis.
        /// </summary>
        /// <param name="angle">The counter-clockwise angle in radians.</param>
        /// <returns>The resulting Matrix4 instance.</returns>
        /// <remarks>Matrix4 adopts the convention of using column vectors to represent a transformation matrix.</remarks>
        public static Matrix4 RotationX(double angle)
        {
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);
            return new Matrix4(1.0, 0.0, 0.0, 0.0,
                               0.0, cos, -sin, 0.0,
                               0.0, sin, cos, 0.0,
                               0.0, 0.0, 0.0, 1.0);
        }

        /// <summary>
        /// Builds a rotation matrix for a rotation around the y-axis.
        /// </summary>
        /// <param name="angle">The counter-clockwise angle in radians.</param>
        /// <returns>The resulting Matrix4 instance.</returns>
        /// <remarks>Matrix4 adopts the convention of using column vectors to represent a transformation matrix.</remarks>
        public static Matrix4 RotationY(double angle)
        {
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);
            return new Matrix4(cos, 0.0, sin, 0.0,
                               0.0, 1.0, 0.0, 0.0,
                               -sin, 0.0, cos, 0.0,
                               0.0, 0.0, 0.0, 1.0);
        }

        /// <summary>
        /// Builds a rotation matrix for a rotation around the z-axis.
        /// </summary>
        /// <param name="angle">The counter-clockwise angle in radians.</param>
        /// <returns>The resulting Matrix4 instance.</returns>
        /// <remarks>Matrix4 adopts the convention of using column vectors to represent a transformation matrix.</remarks>
        public static Matrix4 RotationZ(double angle)
        {
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);
            return new Matrix4(cos, -sin, 0.0, 0.0,
                               sin, cos, 0.0, 0.0,
                               0.0, 0.0, 1.0, 0.0,
                               0.0, 0.0, 0.0, 1.0);
        }

        /// <summary>
        /// Build a scaling matrix.
        /// </summary>
        /// <param name="value">Single scale factor for x, y, and z axis.</param>
        /// <returns>A scaling matrix.</returns>
        public static Matrix4 Scale(double value)
        {
            return Scale(value, value, value);
        }

        /// <summary>
        /// Build a scaling matrix.
        /// </summary>
        /// <param name="value">Scale factors for x, y, and z axis.</param>
        /// <returns>A scaling matrix.</returns>
        public static Matrix4 Scale(Vector3 value)
        {
            return Scale(value.X, value.Y, value.Z);
        }

        /// <summary>
        /// Build a scaling matrix.
        /// </summary>
        /// <param name="x">Scale factor for x-axis.</param>
        /// <param name="y">Scale factor for y-axis.</param>
        /// <param name="z">Scale factor for z-axis.</param>
        /// <returns>A scaling matrix.</returns>
        public static Matrix4 Scale(double x, double y, double z)
        {
            return new Matrix4(x, 0.0, 0.0, 0.0,
                               0.0, y, 0.0, 0.0,
                               0.0, 0.0, z, 0.0,
                               0.0, 0.0, 0.0, 1.0);
        }
		
		/// <summary>
		/// Build a translation matrix.
		/// </summary>
		/// <param name="vector">Translation vector along the X, Y, and Z axis.</param>
		/// <returns>A translation matrix.</returns>
		/// <remarks>Matrix4 adopts the convention of using column vectors to represent a transformation matrix.</remarks>
		public static Matrix4 Translation(Vector3 vector)
		{
			return new Matrix4(1.0, 0.0, 0.0, vector.X,
                               0.0, 1.0, 0.0, vector.Y,
                               0.0, 0.0, 1.0, vector.Z,
                               0.0, 0.0, 0.0, 1.0);
		}
		
		/// <summary>
		/// Build a translation matrix.
		/// </summary>
		/// <param name="x">Translation along the X axis.</param>
		/// <param name="y">Translation along the Y axis.</param>
		/// <param name="z">Translation along the Z axis.</param>
		/// <returns>A translation matrix.</returns>
		/// <remarks>Matrix4 adopts the convention of using column vectors to represent a transformation matrix.</remarks>
		public static Matrix4 Translation(double x, double y, double z)
		{
			return new Matrix4(1.0, 0.0, 0.0, x,
                               0.0, 1.0, 0.0, y,
                               0.0, 0.0, 1.0, z,
                               0.0, 0.0, 0.0, 1.0);
		}

        /// <summary>
        /// Build the reflection matrix of a mirror plane that passes through a point.
        /// </summary>
        /// <param name="normal">Mirror plane normal vector.</param>
        /// <param name="point">A point on the mirror plane.</param>
        /// <returns>A mirror plane reflection matrix that passes through a point.</returns>
        public static Matrix4 Reflection(Vector3 normal, Vector3 point)
        {
            // plane equation that passes through a point ax + by + cz + d = 0 where d = -point·normal
            Vector3 n = Vector3.Normalize(normal);
            double a = n.X;
            double b = n.Y;
            double c = n.Z;
            double d = -Vector3.DotProduct(point, n);
            return new Matrix4(1 - 2 * a * a, -2 * a * b, -2 * a * c, -2 * a * d,
                               -2 * a * b, 1 - 2 * b * b, -2 * b * c, -2 * b * d,
                               -2 * a * c, -2 * b * c, 1 - 2 * c * c, -2 * c * d,
                               0, 0, 0, 1.0);
        }

        #endregion

        #region comparison methods

        /// <summary>
        /// Check if the components of two matrices are approximate equal.
        /// </summary>
        /// <param name="a">Matrix4.</param>
        /// <param name="b">Matrix4.</param>
        /// <returns>True if the matrix components are almost equal or false in any other case.</returns>
        public static bool Equals(Matrix4 a, Matrix4 b)
        {
            return a.Equals(b, MathHelper.Epsilon);
        }

        /// <summary>
        /// Check if the components of two matrices are approximate equal.
        /// </summary>
        /// <param name="a">Matrix4.</param>
        /// <param name="b">Matrix4.</param>
        /// <param name="threshold">Maximum tolerance.</param>
        /// <returns>True if the matrix components are almost equal or false in any other case.</returns>
        public static bool Equals(Matrix4 a, Matrix4 b, double threshold)
        {
            return a.Equals(b, threshold);
        }

        /// <summary>
        /// Check if the components of two matrices are approximate equal.
        /// </summary>
        /// <param name="other">Matrix4.</param>
        /// <returns>True if the matrix components are almost equal or false in any other case.</returns>
        public bool Equals(Matrix4 other)
        {
            return this.Equals(other, MathHelper.Epsilon);
        }

        /// <summary>
        /// Check if the components of two matrices are approximate equal.
        /// </summary>
        /// <param name="obj">Matrix4.</param>
        /// <param name="threshold">Maximum tolerance.</param>
        /// <returns>True if the matrix components are almost equal or false in any other case.</returns>
        public bool Equals(Matrix4 obj, double threshold)
        {
            return
                MathHelper.IsEqual(obj.M11, this.M11, threshold) &&
                MathHelper.IsEqual(obj.M12, this.M12, threshold) &&
                MathHelper.IsEqual(obj.M13, this.M13, threshold) &&
                MathHelper.IsEqual(obj.M14, this.M14, threshold) &&

                MathHelper.IsEqual(obj.M21, this.M21, threshold) &&
                MathHelper.IsEqual(obj.M22, this.M22, threshold) &&
                MathHelper.IsEqual(obj.M23, this.M23, threshold) &&
                MathHelper.IsEqual(obj.M24, this.M24, threshold) &&

                MathHelper.IsEqual(obj.M31, this.M31, threshold) &&
                MathHelper.IsEqual(obj.M32, this.M32, threshold) &&
                MathHelper.IsEqual(obj.M33, this.M33, threshold) &&
                MathHelper.IsEqual(obj.M34, this.M34, threshold) &&
                
                MathHelper.IsEqual(obj.M41, this.M41, threshold) &&
                MathHelper.IsEqual(obj.M42, this.M42, threshold) &&
                MathHelper.IsEqual(obj.M43, this.M43, threshold) &&
                MathHelper.IsEqual(obj.M44, this.M44, threshold);
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">Another object to compare to.</param>
        /// <returns>True if obj and this instance are the same type and represent the same value; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (obj is Matrix4 matrix)
            {
                return this.Equals(matrix);
            }
            return false;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return
                this.M11.GetHashCode() ^ this.M12.GetHashCode() ^ this.M13.GetHashCode() ^ this.M14.GetHashCode() ^
                this.M21.GetHashCode() ^ this.M22.GetHashCode() ^ this.M23.GetHashCode() ^ this.M24.GetHashCode() ^
                this.M31.GetHashCode() ^ this.M32.GetHashCode() ^ this.M33.GetHashCode() ^ this.M34.GetHashCode() ^
                this.M41.GetHashCode() ^ this.M42.GetHashCode() ^ this.M43.GetHashCode() ^ this.M44.GetHashCode();
        }

        #endregion

        #region overrides

        /// <summary>
        /// Obtains a string that represents the matrix.
        /// </summary>
        /// <returns>A string text.</returns>
        public override string ToString()
        {
            string separator = Thread.CurrentThread.CurrentCulture.TextInfo.ListSeparator;
            StringBuilder s = new StringBuilder();
            s.Append(string.Format("|{0}{4} {1}{4} {2}{4} {3}|" + Environment.NewLine, this.m11, this.m12, this.m13, this.m14, separator));
            s.Append(string.Format("|{0}{4} {1}{4} {2}{4} {3}|" + Environment.NewLine, this.m21, this.m22, this.m23, this.m24, separator));
            s.Append(string.Format("|{0}{4} {1}{4} {2}{4} {3}|" + Environment.NewLine, this.m31, this.m32, this.m33, this.m34, separator));
            s.Append(string.Format("|{0}{4} {1}{4} {2}{4} {3}|", this.m41, this.m42, this.m43, this.m44, separator));
            return s.ToString();
        }

        /// <summary>
        /// Obtains a string that represents the matrix.
        /// </summary>
        /// <param name="provider">An IFormatProvider interface implementation that supplies culture-specific formatting information. </param>
        /// <returns>A string text.</returns>
        public string ToString(IFormatProvider provider)
        {
            string separator = Thread.CurrentThread.CurrentCulture.TextInfo.ListSeparator;
            StringBuilder s = new StringBuilder();
            s.Append(string.Format("|{0}{4} {1}{4} {2}{4} {3}|" + Environment.NewLine, this.m11.ToString(provider), this.m12.ToString(provider), this.m13.ToString(provider), this.m14.ToString(provider), separator));
            s.Append(string.Format("|{0}{4} {1}{4} {2}{4} {3}|" + Environment.NewLine, this.m21.ToString(provider), this.m22.ToString(provider), this.m23.ToString(provider), this.m24.ToString(provider), separator));
            s.Append(string.Format("|{0}{4} {1}{4} {2}{4} {3}|" + Environment.NewLine, this.m31.ToString(provider), this.m32.ToString(provider), this.m33.ToString(provider), this.m34.ToString(provider), separator));
            s.Append(string.Format("|{0}{4} {1}{4} {2}{4} {3}|", this.m41.ToString(provider), this.m42.ToString(provider), this.m43.ToString(provider), this.m44.ToString(provider), separator));
            return s.ToString();
        }

        #endregion
    }
}