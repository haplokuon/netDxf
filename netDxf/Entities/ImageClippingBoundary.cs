#region netDxf, Copyright(C) 2013 Daniel Carvajal, Licensed under LGPL.

//                        netDxf library
// Copyright (C) 2013 Daniel Carvajal (haplokuon@gmail.com)
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
using System.Collections.Generic;

namespace netDxf.Entities
{
    /// <summary>
    /// Image clipping boundary type.
    /// </summary>
    public enum ImageClippingBoundaryType
    {
        /// <summary>
        /// Rectangular.
        /// </summary>
        Rectangular = 1,
        /// <summary>
        /// Polygonal
        /// </summary>
        Polygonal = 2
    }

    /// <summary>
    /// Represent a clipping boundary to display specific portions of an image.
    /// </summary>
    public class ImageClippingBoundary
    {
        #region private fields

        private ImageClippingBoundaryType type;
        private List<Vector2> vertexes;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>ImageClippingBoundary</c> class as a rectangular clipping boundary.
        /// </summary>
        /// <param name="x">Rectangle x-coordinate of the top-left corner in image local coordinates.</param>
        /// <param name="y">Rectangle y-coordinate of the top-left corner in image local coordinates.</param>
        /// <param name="width">Rectangle width in image local coordinates.</param>
        /// <param name="height">Rectangle height in image local coordinates.</param>
        public ImageClippingBoundary(double x, double y, double width, double height)
        {
            this.type = ImageClippingBoundaryType.Rectangular;
            this.vertexes = new List<Vector2> { new Vector2(x, y), new Vector2(x + width, y + height) };
        }

        /// <summary>
        /// Initializes a new instance of the <c>ImageClippingBoundary</c> class as a rectangular clipping boundary.
        /// </summary>
        /// <param name="topLeftCorner">Rectangle top-left corner in image local coordinates.</param>
        /// <param name="bottomRightCorner">Rectangle bottom-right corner in image local coordinates.</param>
        public ImageClippingBoundary(Vector2 topLeftCorner, Vector2 bottomRightCorner)
        {
            this.type = ImageClippingBoundaryType.Rectangular;
            this.vertexes = new List<Vector2> { topLeftCorner, bottomRightCorner };
        }

        /// <summary>
        /// Initializes a new instance of the <c>ImageClippingBoundary</c> class as a polygonal clipping boundary.
        /// </summary>
        /// <param name="vertexes">The list of vertexes of the polygonal boundary.</param>
        public ImageClippingBoundary(List<Vector2> vertexes)
        {
            if (vertexes.Count < 3)
                throw new ArgumentException("The number of vertexes for the polygonal clipping boundary must be equal or greater than three.", "vertexes");

            this.type = ImageClippingBoundaryType.Polygonal;
            this.vertexes = vertexes;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the clipping boundary type, rectangular or polygonal.
        /// </summary>
        public ImageClippingBoundaryType Type
        {
            get { return type; }
        }

        /// <summary>
        /// Gets the list of vertexes of the polygonal boundary.
        /// </summary>
        public List<Vector2> Vertexes
        {
            get { return vertexes; }
        }

        #endregion
    }
}
