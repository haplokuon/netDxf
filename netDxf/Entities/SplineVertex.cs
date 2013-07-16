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
    /// Represents a control point of a <see cref="Spline">spline</see>.
    /// </summary>
    public class SplineVertex :
        ICloneable

    {
        #region private fields

        private Vector3 location;
        private double weigth;

        #endregion

        #region contructors

        /// <summary>
        /// Initializes a new instance of the <c>SplineVertex</c> class.
        /// </summary>
        /// <param name="x">Control point x coordiante.</param>
        /// <param name="y">Control point y coordiante.</param>
        /// <param name="z">Control point z coordiante.</param>
        /// <param name="w">Control point weight (default 1.0).</param>
        public SplineVertex(double x, double y, double z, double w = 1.0)
            : this(new Vector3(x, y, z), w)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>SplineVertex</c> class.
        /// </summary>
        /// <param name="location">Spline control point <see cref="Vector2">vertex</see> coordinates.</param>
        /// <param name="weigth">Weigth of the spline control point (default 1.0).</param>
        public SplineVertex(Vector2 location, double weigth = 1.0)
            : this(new Vector3(location.X, location.Y, 0.0), weigth)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>SplineVertex</c> class.
        /// </summary>
        /// <param name="location">Spline control point <see cref="Vector3">vertex</see> coordinates.</param>
        /// <param name="weigth">Weigth of the spline control point (default 1.0).</param>
        public SplineVertex(Vector3 location, double weigth = 1.0)
        {
            if (weigth <= 0)
                throw new ArgumentException("The spline vertex weight must be greater than zero.");
            this.location = location;
            this.weigth = weigth;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Get or sets the spline control point <see cref="Vector3">vertex</see> coordinates.
        /// </summary>
        public Vector3 Location
        {
            get { return this.location; }
            set { this.location = value; }
        }

        /// <summary>
        /// Gets or sets the weigth of the spline control point.
        /// </summary>
        public double Weigth
        {
            get { return this.weigth; }
            set { this.weigth = value; }
        }

        #endregion

        #region overrides

        /// <summary>
        /// Obtains a string that represents the spline vertex.
        /// </summary>
        /// <returns>A string text.</returns>
        public override string ToString()
        {
            return string.Format("{0}: ({1}) w={2}", "SplineVertex", this.Location, this.Weigth);
        }

        /// <summary>
        /// Creates a new Spline that is a copy of the current instance.
        /// </summary>
        /// <returns>A new Spline that is a copy of this instance.</returns>
        public object Clone()
        {
            return new SplineVertex(this.location, this.weigth);
        }

        /// <summary>
        /// Obtains a string that represents the spline vertex.
        /// </summary>
        /// <param name="provider">An IFormatProvider interface implementation that supplies culture-specific formatting information. </param>
        /// <returns>A string text.</returns>
        public string ToString(IFormatProvider provider)
        {
            return string.Format("{0}: ({1}) w={2}", "SplineVertex", this.Location.ToString(provider), this.Weigth.ToString(provider));
        }

        #endregion

    }
}
