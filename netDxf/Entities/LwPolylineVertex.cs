#region netDxf library licensed under the MIT License, Copyright © 2009-2021 Daniel Carvajal (haplokuon@gmail.com)
// 
//                        netDxf library
// Copyright © 2021 Daniel Carvajal (haplokuon@gmail.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software
// and associated documentation files (the “Software”), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;

namespace netDxf.Entities
{
    /// <summary>
    /// Represents a <see cref="LwPolyline">LwPolyline</see> vertex.
    /// </summary>
    public class LwPolylineVertex :
        ICloneable
    {
        #region private fields

        private Vector2 position;
        private double startWidth;
        private double endWidth;
        private double bulge;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>LwPolylineVertex</c> class.
        /// </summary>
        public LwPolylineVertex()
            : this(Vector2.Zero)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>LwPolylineVertex</c> class.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        public LwPolylineVertex(double x, double y)
            : this(new Vector2(x, y), 0.0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>LwPolylineVertex</c> class.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="bulge">Vertex bulge  (default: 0.0).</param>
        public LwPolylineVertex(double x, double y, double bulge)
            : this(new Vector2(x, y), bulge)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>LwPolylineVertex</c> class.
        /// </summary>
        /// <param name="position">Lightweight polyline <see cref="Vector2">vertex</see> coordinates.</param>
        public LwPolylineVertex(Vector2 position)
            : this(position, 0.0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>LwPolylineVertex</c> class.
        /// </summary>
        /// <param name="position">Lightweight polyline <see cref="Vector2">vertex</see> coordinates.</param>
        /// <param name="bulge">Vertex bulge  (default: 0.0).</param>
        public LwPolylineVertex(Vector2 position, double bulge)
        {
            this.position = position;
            this.bulge = bulge;
            this.startWidth = 0.0;
            this.endWidth = 0.0;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the light weight polyline vertex <see cref="Vector2">position</see>.
        /// </summary>
        public Vector2 Position
        {
            get { return this.position; }
            set { this.position = value; }
        }

        /// <summary>
        /// Gets or sets the light weight polyline start segment width.
        /// </summary>
        /// <remarks>Widths greater than zero produce wide lines.</remarks>
        public double StartWidth
        {
            get { return this.startWidth; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The LwPolylineVertex width must be equals or greater than zero.");
                this.startWidth = value;
            }
        }

        /// <summary>
        /// Gets or sets the light weight polyline end segment width.
        /// </summary>
        /// <remarks>Widths greater than zero produce wide lines.</remarks>
        public double EndWidth
        {
            get { return this.endWidth; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The LwPolylineVertex width must be equals or greater than zero.");
                this.endWidth = value;
            }
        }

        /// <summary>
        /// Gets or set the light weight polyline bulge.
        /// </summary>
        /// <remarks>
        /// The bulge is the tangent of one fourth the included angle for an arc segment, 
        /// made negative if the arc goes clockwise from the start point to the endpoint. 
        /// A bulge of 0 indicates a straight segment, and a bulge of 1 is a semicircle.
        /// </remarks>
        public double Bulge
        {
            get { return this.bulge; }
            set { this.bulge = value; }
        }

        #endregion

        #region overrides

        /// <summary>
        /// Converts the value of this instance to its equivalent string representation.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            return string.Format("{0}: ({1})", "LwPolylineVertex", this.position);
        }

        /// <summary>
        /// Creates a new LwPolylineVertex that is a copy of the current instance.
        /// </summary>
        /// <returns>A new LwPolylineVertex that is a copy of this instance.</returns>
        public object Clone()
        {
            return new LwPolylineVertex
            {
                Position = this.position,
                Bulge = this.bulge,
                StartWidth = this.startWidth,
                EndWidth = this.endWidth
            };
        }

        #endregion
    }
}