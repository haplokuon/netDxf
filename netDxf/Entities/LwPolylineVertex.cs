#region netDxf, Copyright(C) 2012 Daniel Carvajal, Licensed under LGPL.

//                        netDxf library
// Copyright (C) 2012 Daniel Carvajal (haplokuon@gmail.com)
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

namespace netDxf.Entities
{
    /// <summary>
    /// Represents a <see cref="LwPolyline">LwPolyline</see> vertex.
    /// </summary>
    public class LwPolylineVertex :
        ICloneable
    {
        #region private fields

        private Vector2 location;
        private double beginWidth;
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
        /// <param name="bulge">Vertex bulge.</param>
        public LwPolylineVertex(double x, double y, double bulge = 0.0)
            : this(new Vector2(x, y), bulge)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>LwPolylineVertex</c> class.
        /// </summary>
        /// <param name="location">Lightweight polyline <see cref="Vector2">vertex</see> coordinates.</param>
        /// <param name="bulge">Vertex bulge.</param>
        public LwPolylineVertex(Vector2 location, double bulge = 0.0)
        {
            this.location = location;
            this.bulge = bulge;
            this.beginWidth = 0.0;
            this.endWidth = 0.0;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the light weight polyline vertex <see cref="Vector2">location</see>.
        /// </summary>
        public Vector2 Location
        {
            get { return this.location; }
            set { this.location = value; }
        }

        /// <summary>
        /// Gets or sets the light weight polyline begin width.
        /// </summary>
        public double BeginWidth
        {
            get { return this.beginWidth; }
            set { this.beginWidth = value; }
        }

        /// <summary>
        /// Gets or sets the light weight polyline end width.
        /// </summary>
        public double EndWidth
        {
            get { return this.endWidth; }
            set { this.endWidth = value; }
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
            return String.Format("{0}: ({1})", "LwPolylineVertex", this.location);
        }

        /// <summary>
        /// Creates a new LwPolylineVertex that is a copy of the current instance.
        /// </summary>
        /// <returns>A new LwPolylineVertex that is a copy of this instance.</returns>
        public object Clone()
        {
            return new LwPolylineVertex
                {
                    Location = this.location,
                    Bulge = this.bulge,
                    BeginWidth = this.beginWidth,
                    EndWidth = this.endWidth
                };
        }

        #endregion
    }
}