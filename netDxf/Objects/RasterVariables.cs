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

using netDxf.Units;

namespace netDxf.Objects
{
    /// <summary>
    /// Represents the variables applied to bitmaps.
    /// </summary>
    public class RasterVariables :
        DxfObject
    {
        #region private fields

        private bool displayFrame;
        private ImageDisplayQuality quality;
        private ImageUnits units;

        #endregion

        #region constructors

        internal RasterVariables(DxfDocument document)
            : base(DxfObjectCode.RasterVariables)
        {
            this.displayFrame = true;
            this.quality = ImageDisplayQuality.High;
            this.units = ImageUnits.Unitless;
            this.Owner = document;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets if the image frame is shown.
        /// </summary>
        public bool DisplayFrame
        {
            get { return this.displayFrame; }
            set { this.displayFrame = value; }
        }

        /// <summary>
        /// Gets or sets the image display quality (screen only).
        /// </summary>
        public ImageDisplayQuality DisplayQuality
        {
            get { return this.quality; }
            set { this.quality = value; }
        }

        /// <summary>
        /// Gets or sets the AutoCAD units for inserting images.
        /// </summary>
        /// <remarks>
        /// Default: None<br />
        /// This is what one AutoCAD unit is equal to for the purpose of inserting and scaling images with an associated resolution.
        /// It is recommended to use the same units as the header variable InsUnits, or just use none to avoid any unwanted scaling when inserting images into the drawing.
        /// </remarks>
        public ImageUnits Units
        {
            get { return this.units; }
            set { this.units = value; }
        }

        #endregion
    }
}