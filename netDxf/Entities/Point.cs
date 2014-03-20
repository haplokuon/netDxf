#region netDxf, Copyright(C) 2014 Daniel Carvajal, Licensed under LGPL.

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

namespace netDxf.Entities
{
    /// <summary>
    /// Represents a point <see cref="EntityObject">entity</see>.
    /// </summary>
    public class Point :
        EntityObject
    {
        #region private fields

        private Vector3 location;
        private double thickness;
        private double rotation;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>Point</c> class.
        /// </summary>
        /// <param name="location">Point <see cref="Vector3">location</see>.</param>
        public Point(Vector3 location)
            : base(EntityType.Point, DxfObjectCode.Point)
        {
            this.location = location;
            this.thickness = 0.0f;
            this.rotation = 0.0;
        }

        /// <summary>
        /// Initializes a new instance of the <c>Point</c> class.
        /// </summary>
        /// <param name="location">Point <see cref="Vector2">location</see>.</param>
        public Point(Vector2 location)
            : this(new Vector3(location.X, location.Y, 0.0))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Point</c> class.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="z">Z coordinate.</param>
        public Point(double x, double y, double z)
            : this(new Vector3(x, y, z))
        {

        }

        /// <summary>
        /// Initializes a new instance of the <c>Point</c> class.
        /// </summary>
        public Point()
            : this(Vector3.Zero)
        {
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the point <see cref="netDxf.Vector3">location</see>.
        /// </summary>
        public Vector3 Location
        {
            get { return this.location; }
            set { this.location = value; }
        }

        /// <summary>
        /// Gets or sets the point thickness.
        /// </summary>
        public double Thickness
        {
            get { return this.thickness; }
            set { this.thickness = value; }
        }

        /// <summary>
        /// Gets or sets the point local rotation in degrees along its normal.
        /// </summary>
        public double Rotation
        {
            get { return this.rotation; }
            set { this.rotation = MathHelper.NormalizeAngle(value); }
        }

        #endregion

        #region overrides

        /// <summary>
        /// Creates a new Point that is a copy of the current instance.
        /// </summary>
        /// <returns>A new Point that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new Point
            {
                //EntityObject properties
                Color = this.color,
                Layer = this.layer,
                LineType = this.lineType,
                Lineweight = this.lineweight,
                LineTypeScale = this.lineTypeScale,
                Normal = this.normal,
                XData = this.xData,
                //Point properties
                Location = this.location,
                Rotation = this.rotation,
                Thickness = this.thickness
            };
        }

        #endregion

    }
}