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
using System.Diagnostics;

namespace netDxf.GTE
{
    // The input matrix M must be NxN.  The storage convention for element lookup
    // is determined by GTE_USE_ROW_MAJOR or GTE_USE_COL_MAJOR, whichever is
    // active.  If you want the inverse of M, pass a nonnull pointer inverseM;
    // this matrix must also be NxN and use the same storage convention as M.  If
    // you do not want the inverse of M, pass a nullptr for inverseM.  If you want
    // to solve M*X = B for X, where X and B are Nx1, pass nonnull pointers for B
    // and X.  If you want to solve M*Y = C for Y, where X and C are NxK, pass
    // nonnull pointers for C and Y and pass K to numCols.  In all cases, pass
    // N to numRows.

    public static class GaussianElimination
    {
        public static bool Solve(int numRows, double[] M, double[] inverseM, out double determinant, double[] B, double[] X, double[] C, int numCols, double[] Y)
        {
            if (numRows <= 0 || M == null || ((B != null) != (X != null)) || ((C != null) != (Y != null)) || (C != null && numCols < 1))
            {
                Debug.Assert(false, "Invalid input.");
            }

            int numElements = numRows * numRows;
            bool wantInverse = inverseM != null;
            //if (!wantInverse)
            //{
            //    inverseM = new double[numElements];
            //}
            Set(numElements, M, out inverseM);

            if (B != null)
            {
                Set(numRows, B, out X);
            }

            if (C != null)
            {
                Set(numRows * numCols, C, out Y);
            }

            LexicoArray2 matInvM = new LexicoArray2(numRows, numRows, inverseM);
            LexicoArray2 matY = new LexicoArray2(numRows, numCols, Y);

            int[] colIndex = new int[numRows];
            int[] rowIndex = new int[numRows];
            bool[] pivoted = new bool[numRows];

            const double zero = 0.0;
            const double one = 1.0;
            bool odd = false;
            determinant = one;

            // Elimination by full pivoting.
            int i1, i2, row = 0, col = 0;
            for (int i0 = 0; i0 < numRows; i0++)
            {
                // Search matrix (excluding pivoted rows) for maximum absolute entry.
                double maxValue = zero;
                for (i1 = 0; i1 < numRows; i1++)
                {
                    if (!pivoted[i1])
                    {
                        for (i2 = 0; i2 < numRows; i2++)
                        {
                            if (!pivoted[i2])
                            {
                                double value = matInvM[i1, i2];
                                double absValue = (value >= zero ? value : -value);
                                if (absValue > maxValue)
                                {
                                    maxValue = absValue;
                                    row = i1;
                                    col = i2;
                                }
                            }
                        }
                    }
                }

                if (Math.Abs(maxValue - zero) < double.Epsilon)
                {
                    // The matrix is not invertible.
                    if (wantInverse)
                    {
                        Set(numElements, null, out inverseM);
                    }
                    determinant = zero;

                    if (B != null)
                    {
                        Set(numRows, null, out X);
                    }

                    if (C != null)
                    {
                        Set(numRows * numCols, null, out Y);
                    }
                    return false;
                }

                pivoted[col] = true;

                // Swap rows so that the pivot entry is in row 'col'.
                if (row != col)
                {
                    odd = !odd;
                    for (int i = 0; i < numRows; i++)
                    {
                        double tmp = matInvM[row, i];
                        matInvM[row, i] = matInvM[col, i];
                        matInvM[col, i] = tmp;
                    }

                    if (B != null)
                    {
                        double tmp = X[row];
                        X[row] = X[col];
                        X[col] = tmp;
                    }

                    if (C != null)
                    {
                        for (int i = 0; i < numCols; i++)
                        {
                            double tmp = matY[row, i];
                            matY[row, i] = matY[col, i];
                            matY[col, i] = tmp;
                        }
                    }
                }

                // Keep track of the permutations of the rows.
                rowIndex[i0] = row;
                colIndex[i0] = col;

                // Scale the row so that the pivot entry is 1.
                double diagonal = matInvM[col, col];
                determinant *= diagonal;
                double inv = one / diagonal;
                matInvM[col, col] = one;
                for (i2 = 0; i2 < numRows; i2++)
                {
                    matInvM[col, i2] *= inv;
                }

                if (B != null)
                {
                    X[col] *= inv;
                }

                if (C != null)
                {
                    for (i2 = 0; i2 < numCols; i2++)
                    {
                        matY[col, i2] *= inv;
                    }
                }

                // Zero out the pivot column locations in the other rows.
                for (i1 = 0; i1 < numRows; i1++)
                {
                    if (i1 != col)
                    {
                        double save = matInvM[i1, col];
                        matInvM[i1, col] = zero;
                        for (i2 = 0; i2 < numRows; i2++)
                        {
                            matInvM[i1, i2] -= matInvM[col, i2] * save;
                        }

                        if (B != null)
                        {
                            X[i1] -= X[col] * save;
                        }

                        if (C != null)
                        {
                            for (i2 = 0; i2 < numCols; i2++)
                            {
                                matY[i1, i2] -= matY[col, i2] * save;
                            }
                        }
                    }
                }
            }

            if (wantInverse)
            {
                // Reorder rows to undo any permutations in Gaussian elimination.
                for (i1 = numRows - 1; i1 >= 0; i1--)
                {
                    if (rowIndex[i1] != colIndex[i1])
                    {
                        for (i2 = 0; i2 < numRows; i2++)
                        {
                            double tmp = matInvM[i2, rowIndex[i1]];
                            matInvM[i2, rowIndex[i1]] = matInvM[i2, colIndex[i1]];
                            matInvM[i2, colIndex[i1]] = tmp;
                        }
                    }
                }
            }

            if (odd)
            {
                determinant = -determinant;
            }

            return true;
        }

        // Support for copying source to target or to set target to zero.  If
        // source is nullptr, then target is set to zero; otherwise source is
        // copied to target.  This function hides the type traits used to
        // determine whether double is native floating-point or otherwise (such
        // as BSNumber or BSRational).
        private static void Set(int numElements, double[] source, out double[] target)
        {
            target = new double[numElements]; 
            if (source != null)
            {
                source.CopyTo(target, 0);
            }
        }
    };
}
