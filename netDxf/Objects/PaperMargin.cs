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

namespace netDxf.Objects
{
    /// <summary>
    /// Represents the unprintable margins of a paper. 
    /// </summary>
    public struct PaperMargin
    {
        #region private fields

        private double left;
        private double bottom;
        private double right;
        private double top;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of <c>PaperMargin</c>.
        /// </summary>
        /// <param name="left">Margin on left side of paper.</param>
        /// <param name="bottom">Margin on bottom side of paper.</param>
        /// <param name="right">Margin on right side of paper.</param>
        /// <param name="top">Margin on top side of paper.</param>
        public PaperMargin(double left, double bottom, double right, double top)
        {
            this.left = left;
            this.bottom = bottom;
            this.right = right;
            this.top = top;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or set the size, in millimeters, of unprintable margin on left side of paper.
        /// </summary>
        public double Left
        {
            get { return this.left; }
            set { this.left = value; }
        }

        /// <summary>
        /// Gets or set the size, in millimeters, of unprintable margin on bottom side of paper.
        /// </summary>
        public double Bottom
        {
            get { return this.bottom; }
            set { this.bottom = value; }
        }

        /// <summary>
        /// Gets or set the size, in millimeters, of unprintable margin on right side of paper.
        /// </summary>
        public double Right
        {
            get { return this.right; }
            set { this.right = value; }
        }

        /// <summary>
        /// Gets or set the size, in millimeters, of unprintable margin on top side of paper.
        /// </summary>
        public double Top
        {
            get { return this.top; }
            set { this.top = value; }
        }

        #endregion      
    }
}