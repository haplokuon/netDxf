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
