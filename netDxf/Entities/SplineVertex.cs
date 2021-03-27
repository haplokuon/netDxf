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
    /// Represents a control point of a <see cref="Spline">spline</see>.
    /// </summary>
    public class SplineVertex :
        ICloneable
    {
        #region private fields

        private Vector3 position;
        private double weight;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>SplineVertex</c> class.
        /// </summary>
        /// <param name="x">Control point x coordinate.</param>
        /// <param name="y">Control point y coordinate.</param>
        /// <param name="z">Control point z coordinate.</param>
        public SplineVertex(double x, double y, double z)
            : this(new Vector3(x, y, z), 1.0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>SplineVertex</c> class.
        /// </summary>
        /// <param name="x">Control point x coordinate.</param>
        /// <param name="y">Control point y coordinate.</param>
        /// <param name="z">Control point z coordinate.</param>
        /// <param name="w">Control point weight (default 1.0).</param>
        public SplineVertex(double x, double y, double z, double w)
            : this(new Vector3(x, y, z), w)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>SplineVertex</c> class.
        /// </summary>
        /// <param name="position">Spline control point <see cref="Vector2">vertex</see> coordinates.</param>
        public SplineVertex(Vector2 position)
            : this(new Vector3(position.X, position.Y, 0.0), 1.0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>SplineVertex</c> class.
        /// </summary>
        /// <param name="position">Spline control point <see cref="Vector2">vertex</see> coordinates.</param>
        /// <param name="weight">Weight of the spline control point (default 1.0).</param>
        public SplineVertex(Vector2 position, double weight)
            : this(new Vector3(position.X, position.Y, 0.0), weight)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>SplineVertex</c> class.
        /// </summary>
        /// <param name="position">Spline control point <see cref="Vector3">vertex</see> coordinates.</param>
        public SplineVertex(Vector3 position)
            : this(position, 1.0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>SplineVertex</c> class.
        /// </summary>
        /// <param name="position">Spline control point <see cref="Vector3">vertex</see> coordinates.</param>
        /// <param name="weight">Weight of the spline control point (default 1.0).</param>
        public SplineVertex(Vector3 position, double weight)
        {
            if (weight <= 0)
                throw new ArgumentOutOfRangeException(nameof(weight), weight, "The spline vertex weight must be greater than zero.");
            this.position = position;
            this.weight = weight;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Get or sets the spline control point <see cref="Vector3">vertex</see> coordinates.
        /// </summary>
        public Vector3 Position
        {
            get { return this.position; }
            set { this.position = value; }
        }

        /// <summary>
        /// Gets or sets the weight of the spline control point.
        /// </summary>
        public double Weight
        {
            get { return this.weight; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The spline vertex weight must be greater than zero.");
                this.weight = value;
            }
        }

        #endregion

        #region overrides

        /// <summary>
        /// Obtains a string that represents the spline vertex.
        /// </summary>
        /// <returns>A string text.</returns>
        public override string ToString()
        {
            return string.Format("{0}: ({1}) w={2}", "SplineVertex", this.Position, this.Weight);
        }

        /// <summary>
        /// Creates a new Spline that is a copy of the current instance.
        /// </summary>
        /// <returns>A new Spline that is a copy of this instance.</returns>
        public object Clone()
        {
            return new SplineVertex(this.position, this.weight);
        }

        /// <summary>
        /// Obtains a string that represents the spline vertex.
        /// </summary>
        /// <param name="provider">An IFormatProvider interface implementation that supplies culture-specific formatting information. </param>
        /// <returns>A string text.</returns>
        public string ToString(IFormatProvider provider)
        {
            return string.Format("{0}: ({1}) w={2}", "SplineVertex", this.Position.ToString(provider), this.Weight.ToString(provider));
        }

        #endregion
    }
}