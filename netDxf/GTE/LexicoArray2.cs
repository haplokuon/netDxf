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
    // A template class to provide 2D array access that conforms to row-major
    // order (RowMajor = true) or column-major order (RowMajor = false).  The

    // The array dimensions are known only at run time.
    public class LexicoArray2
    {
        private readonly int numRows, numCols;
        private readonly double[] matrix;

        public LexicoArray2(int numRows, int numCols, double[] matrix)
        {
            this.numRows = numRows;
            this.numCols = numCols;
            this.matrix = matrix;
        }

        public int NumRows
        {
            get { return this.numRows; }
        }

        public int NumCols
        {
            get { return this.numCols; }
        }

        public double this[int r, int c]
        {
            get { return GTE.UseRowMajor ? this.matrix[c + this.numCols * r] : this.matrix[r + this.numRows * c]; }
            set
            {
                if (GTE.UseRowMajor)
                {
                    this.matrix[c + this.numCols * r] = value;
                }
                else
                {
                    this.matrix[r + this.numRows * c] = value;
                }
            }
        }

        public void CopyTo(double[] array, int index)
        {
            this.matrix.CopyTo(array, 0);
        }
    };
}
