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
    public class BandedMatrix
    {
        private readonly int size;
        private readonly double[] dBand;
        private readonly double[][] lBands, uBands;

        // Construction and destruction.
        public BandedMatrix(int size, int numLBands, int numUBands)
        {
            this.size = size;

            if (size > 0 && 0 <= numLBands && numLBands < size && 0 <= numUBands && numUBands < size)
            {
                this.dBand = new double[size];
                int numElements;

                if (numLBands > 0)
                {
                    this.lBands = new double[numLBands][];
                    numElements = size - 1;
                    for (int i = 0; i < numLBands; i++)
                    {
                        this.lBands[i] = new double[numElements--];
                    }
                }

                if (numUBands > 0)
                {
                    this.uBands = new double[numUBands][];
                    numElements = size - 1;
                    for (int i = 0; i < numUBands; i++)
                    {
                        this.uBands[i] = new double[numElements--];
                    }
                }
            }
            else
            {
                // Invalid argument to BandedMatrix constructor.
               this.size = 0;
            }
        }

        // Member access.
        public int Size
        {
            get { return this.size; }
        }

        public double[] DBand
        {
            get { return this.dBand; }
        }

        public double[][] LBands
        {
            get { return this.lBands; }
        }

        public double[][] UBands
        {
            get { return this.uBands; }
        }

        public double this[int r, int c]
        {
            get
            {
                if (0 <= r && r < this.size && 0 <= c && c < this.size)
                {
                    int band = c - r;
                    if (band > 0)
                    {
                        int numUBands = this.uBands.Length;
                        if (--band < numUBands && r < this.size - 1 - band)
                        {
                            return this.uBands[band][r];
                        }
                    }
                    else if (band < 0)
                    {
                        band = -band;
                        int numLBands = this.lBands.Length;
                        if (--band < numLBands && c < this.size - 1 - band)
                        {
                            return this.lBands[band][c];
                        }
                    }
                    else
                    {
                        return this.dBand[r];
                    }
                }

                // else invalid index
                //Debug.Assert(false, "Invalid index.");
                return 0.0;
            }
            set
            {
                if (0 <= r && r < this.size && 0 <= c && c < this.size)
                {
                    int band = c - r;
                    if (band > 0)
                    {
                        int numUBands = this.uBands.Length;
                        if (--band < numUBands && r < this.size - 1 - band)
                        {
                            this.uBands[band][r] = value;
                        }
                    }
                    else if (band < 0)
                    {
                        band = -band;
                        int numLBands = this.lBands.Length;
                        if (--band < numLBands && c < this.size - 1 - band)
                        {
                            this.lBands[band][c] = value;
                        }
                    }
                    else
                    {
                        this.dBand[r] = value;
                    }
                }
                else
                {
                    // invalid index
                    //Debug.Assert(false, "Invalid index.");
                }
            }
        }

        // Factor the square banded matrix A into A = L*L^T, where L is a
        // lower-triangular matrix (L^T is an upper-triangular matrix).  This
        // is an LU decomposition that allows for stable inversion of A to
        // solve A*X = B.  The return value is 'true' iff the factorizing is
        // successful (L is invertible).  If successful, A contains the
        // Cholesky factorization: L in the lower-triangular part of A and
        // L^T in the upper-triangular part of A.
        public bool CholeskyFactor()
        {
            if (this.dBand.Length == 0 || this.lBands.Length != this.uBands.Length)
            {
                // Invalid number of bands.
                return false;
            }

            int sizeM1 = this.size - 1;
            int numBands = this.lBands.Length;

            for (int i = 0; i < this.size; i++)
            {
                int jMin = i - numBands;
                if (jMin < 0)
                {
                    jMin = 0;
                }

                int j, k, kMax;
                for (j = jMin; j < i; j++)
                {
                    kMax = j + numBands;
                    if (kMax > sizeM1)
                    {
                        kMax = sizeM1;
                    }

                    for (k = i; k <= kMax; k++)
                    {
                        this[k, i] -= this[i, j] * this[k, j];
                    }
                }

                kMax = j + numBands;
                if (kMax > sizeM1)
                {
                    kMax = sizeM1;
                }

                for (k = 0; k < i; k++)
                {
                    this[k, i] = this[i, k];
                }

                double diagonal = this[i, i];
                if (diagonal <= 0.0)
                {
                    return false;
                }
                double invSqrt = 1.0 / Math.Sqrt(diagonal);
                for (k = i; k <= kMax; k++)
                {
                    this[k, i] *= invSqrt;
                }
            }

            return true;
        }

        // Solve the linear system A*X = B, where A is an NxN banded matrix
        // and B is an Nx1 vector.  The unknown X is also Nx1.  The input to
        // this function is B.  The output X is computed and stored in B.  The
        // return value is 'true' iff the system has a solution.  The matrix A
        // and the vector B are both modified by this function.  If
        // successful, A contains the Cholesky factorization: L in the
        // lower-triangular part of A and L^T in the upper-triangular part
        // of A.
        public bool SolveSystem(ref double[] bVector)
        {
            return this.CholeskyFactor() && this.SolveLower(ref bVector) && this.SolveUpper(ref bVector);
        }

        // Solve the linear system A*X = B, where A is an NxN banded matrix
        // and B is an NxM matrix.  The unknown X is also NxM.  The input to
        // this function is B.  The output X is computed and stored in B.  The
        // return value is 'true' iff the system has a solution.  The matrix A
        // and the vector B are both modified by this function.  If
        // successful, A contains the Cholesky factorization: L in the
        // lower-triangular part of A and L^T in the upper-triangular part
        // of A.
        //
        // 'bMatrix' must have the storage order specified by the template
        // parameter.
        public bool SolveSystem(ref double[] bMatrix, int numBColumns)
        {
            return this.CholeskyFactor() && this.SolveLower(ref bMatrix, numBColumns) && this.SolveUpper(ref bMatrix, numBColumns);
        }

        // Compute the inverse of the banded matrix.  The return value is
        // 'true' when the matrix is invertible, in which case the 'inverse'
        // output is valid.  The return value is 'false' when the matrix is
        // not invertible, in which case 'inverse' is invalid and should not
        // be used.  The input matrix 'inverse' must be the same size as
        // 'this'.
        //
        // 'bMatrix' must have the storage order specified by the template
        // parameter.
        public bool ComputeInverse(double[] inverse)
        {
            LexicoArray2 invA = new LexicoArray2(this.size, this.size, inverse);

            BandedMatrix tmpA = this;
            for (int row = 0; row < this.size; row++)
            {
                for (int col = 0; col < this.size; col++)
                {
                    if (row != col)
                    {
                        invA[row, col] = 0.0;
                    }
                    else
                    {
                        invA[row, row] = 1.0;
                    }
                }
            }

            // Forward elimination.
            for (int row = 0; row < this.size; row++)
            {
                // The pivot must be nonzero in order to proceed.
                double diag = tmpA[row, row];
                if (Math.Abs(diag) < double.Epsilon)
                {
                    return false;
                }

                double invDiag = 1.0 / diag;
                tmpA[row, row] = 1.0;

                // Multiply the row to be consistent with diagonal term of 1.
                int colMin = row + 1;
                int colMax = colMin + this.uBands.Length;
                if (colMax > this.size)
                {
                    colMax = this.size;
                }

                int c;
                for (c = colMin; c < colMax; c++)
                {
                    tmpA[row, c] *= invDiag;
                }
                for (c = 0; c <= row; c++)
                {
                    invA[row, c] *= invDiag;
                }

                // Reduce the remaining rows.
                int rowMin = row + 1;
                int rowMax = rowMin + this.lBands.Length;
                if (rowMax > this.size)
                {
                    rowMax = this.size;
                }

                for (int r = rowMin; r < rowMax; r++)
                {
                    double mult = tmpA[r, row];
                    tmpA[r, row] = 0.0;
                    for (c = colMin; c < colMax; c++)
                    {
                        tmpA[r, c] -= mult * tmpA[row, c];
                    }
                    for (c = 0; c <= row; c++)
                    {
                        invA[r, c] -= mult * invA[row, c];
                    }
                }
            }

            // Backward elimination.
            for (int row = this.size - 1; row >= 1; row--)
            {
                int rowMax = row - 1;
                int rowMin = row - this.uBands.Length;
                if (rowMin < 0)
                {
                    rowMin = 0;
                }

                for (int r = rowMax; r >= rowMin; r--)
                {
                    double mult = tmpA[r, row];
                    tmpA[r, row] = 0.0;
                    for (int c = 0; c < this.size; c++)
                    {
                        invA[r, c] -= mult * invA[row, c];
                    }
                }
            }

            return true;
        }

        // The linear system is L*U*X = B, where A = L*U and U = L^T,  Reduce
        // this to U*X = L^{-1}*B.  The return value is 'true' iff the
        // operation is successful.
        private bool SolveLower(ref double[] dataVector)
        {
            int dBandSize = this.dBand.Length;
            for (int r = 0; r < dBandSize; r++)
            {
                double lowerRR = this[r, r];
                if (lowerRR > 0.0)
                {
                    for (int c = 0; c < r; c++)
                    {
                        double lowerRC = this[r, c];
                        dataVector[r] -= lowerRC * dataVector[c];
                    }
                    dataVector[r] /= lowerRR;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        // The linear system is U*X = L^{-1}*B.  Reduce this to
        // X = U^{-1}*L^{-1}*B.  The return value is 'true' iff the operation
        // is successful.
        private bool SolveUpper(ref double[] dataVector)
        {
            int dBandSize = this.dBand.Length;
            for (int r = this.size - 1; r >= 0; r--)
            {
                double upperRR = this[r, r];
                if (upperRR > 0.0)
                {
                    for (int c = r + 1; c < dBandSize; c++)
                    {
                        double upperRC = this[r, c];
                        dataVector[r] -= upperRC * dataVector[c];
                    }

                    dataVector[r] /= upperRR;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        // The linear system is L*U*X = B, where A = L*U and U = L^T,  Reduce
        // this to U*X = L^{-1}*B.  The return value is 'true' iff the
        // operation is successful.  See the comments for
        // SolveSystem(double*,int) about the storage for dataMatrix.
        private bool SolveLower(ref double[] dataMatrix, int numColumns)
        {
            LexicoArray2 data = new LexicoArray2(this.size, numColumns, dataMatrix);

            for (int r = 0; r < this.size; r++)
            {
                double lowerRR = this[r, r];
                if (lowerRR > 0.0)
                {
                    for (int c = 0; c < r; c++)
                    {
                        double lowerRC = this[r, c];
                        for (int bCol = 0; bCol < numColumns; bCol++)
                        {
                            data[r, bCol] -= lowerRC * data[c, bCol];
                        }
                    }

                    double inverse = 1.0 / lowerRR;
                    for (int bCol = 0; bCol < numColumns; bCol++)
                    {
                        data[r, bCol] *= inverse;
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        // The linear system is U*X = L^{-1}*B.  Reduce this to
        // X = U^{-1}*L^{-1}*B.  The return value is 'true' iff the operation
        // is successful.  See the comments for SolveSystem(double*,int) about
        // the storage for dataMatrix.
        private bool SolveUpper(ref double[] dataMatrix, int numColumns)
        {
            LexicoArray2 data = new LexicoArray2(this.size, numColumns, dataMatrix);

            for (int r = this.size - 1; r >= 0; r--)
            {
                double upperRR = this[r, r];
                if (upperRR > 0.0)
                {
                    for (int c = r + 1; c < this.size; c++)
                    {
                        double upperRC = this[r, c];
                        for (int bCol = 0; bCol < numColumns; bCol++)
                        {
                            data[r, bCol] -= upperRC * data[c, bCol];
                        }
                    }

                    double inverse = 1.0 / upperRR;
                    for (int bCol = 0; bCol < numColumns; bCol++)
                    {
                        data[r, bCol] *= inverse;
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
    };
}
