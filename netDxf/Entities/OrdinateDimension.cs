#region netDxf library, Copyright (C) 2009-2016 Daniel Carvajal (haplokuon@gmail.com)

//                        netDxf library
// Copyright (C) 2009-2016 Daniel Carvajal (haplokuon@gmail.com)
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
using netDxf.Blocks;
using netDxf.Tables;

namespace netDxf.Entities
{
    /// <summary>
    /// Represents an ordinate dimension <see cref="EntityObject">entity</see>.
    /// </summary>
    public class OrdinateDimension
        : Dimension
    {
        #region private fields

        private Vector2 origin;
        private Vector2 referencePoint;
        private double length;
        private double rotation;
        private OrdinateDimensionAxis axis;
        private Vector3 firstPoint;
        private Vector3 secondPoint;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>OrdinateDimension</c> class.
        /// </summary>
        public OrdinateDimension()
            : this(Vector2.Zero, Vector2.UnitX, 0.1, OrdinateDimensionAxis.Y, 0.0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>OrdinateDimension</c> class.
        /// </summary>
        /// <param name="origin">Origin <see cref="Vector2">point</see> of the ordinate dimension.</param>
        /// <param name="referencePoint">Base location <see cref="Vector3">point</see> in local coordinates of the ordinate dimension.</param>
        /// <param name="length">Length of the dimension line.</param>
        /// <param name="axis">Length of the dimension line.</param>
        /// <remarks>The local coordinate system of the dimension is defined by the dimension normal and the rotation value.</remarks>
        public OrdinateDimension(Vector2 origin, Vector2 referencePoint, double length, OrdinateDimensionAxis axis)
            : this(origin, referencePoint, length, axis, 0.0, DimensionStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>OrdinateDimension</c> class.
        /// </summary>
        /// <param name="origin">Origin <see cref="Vector2">point</see> of the ordinate dimension.</param>
        /// <param name="referencePoint">Base location <see cref="Vector3">point</see> in local coordinates of the ordinate dimension.</param>
        /// <param name="length">Length of the dimension line.</param>
        /// <param name="axis">Length of the dimension line.</param>
        /// <param name="style">The <see cref="DimensionStyle">style</see> to use with the dimension.</param>
        /// <remarks>The local coordinate system of the dimension is defined by the dimension normal and the rotation value.</remarks>
        public OrdinateDimension(Vector2 origin, Vector2 referencePoint, double length, OrdinateDimensionAxis axis, DimensionStyle style)
            : this(origin, referencePoint, length, axis, 0.0, style)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>OrdinateDimension</c> class.
        /// </summary>
        /// <param name="origin">Origin <see cref="Vector2">point</see> of the ordinate dimension.</param>
        /// <param name="referencePoint">Base location <see cref="Vector3">point</see> in local coordinates of the ordinate dimension.</param>
        /// <param name="length">Length of the dimension line.</param>
        /// <param name="rotation">Angle of rotation in degrees of the dimension lines.</param>
        /// <param name="axis">Length of the dimension line.</param>
        /// <remarks>The local coordinate system of the dimension is defined by the dimension normal and the rotation value.</remarks>
        public OrdinateDimension(Vector2 origin, Vector2 referencePoint, double length, OrdinateDimensionAxis axis, double rotation)
            : this(origin, referencePoint, length, axis, rotation, DimensionStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>OrdinateDimension</c> class.
        /// </summary>
        /// <param name="origin">Origin <see cref="Vector3">point</see> in world coordinates of the ordinate dimension.</param>
        /// <param name="referencePoint">Base location <see cref="Vector3">point</see> in local coordinates of the ordinate dimension.</param>
        /// <param name="length">Length of the dimension line.</param>
        /// <param name="axis">Local axis that measures the ordinate dimension.</param>
        /// <param name="rotation">Angle of rotation in degrees of the dimension lines.</param>
        /// <param name="style">The <see cref="DimensionStyle">style</see> to use with the dimension.</param>
        /// <remarks>The local coordinate system of the dimension is defined by the dimension normal and the rotation value.</remarks>
        public OrdinateDimension(Vector2 origin, Vector2 referencePoint, double length, OrdinateDimensionAxis axis, double rotation, DimensionStyle style)
            : base(DimensionType.Ordinate)
        {
            this.origin = origin;
            this.rotation = MathHelper.NormalizeAngle(rotation);
            this.length = length;
            this.referencePoint = referencePoint;
            this.axis = axis;

            if (style == null)
                throw new ArgumentNullException(nameof(style));
            this.Style = style;
        }

        #endregion

        #region internal properties

        internal Vector3 FirstPoint
        {
            get { return this.firstPoint; }
            set { this.firstPoint = value; }
        }

        internal Vector3 SecondPoint
        {
            get { return this.secondPoint; }
            set { this.secondPoint = value; }
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the origin <see cref="Vector2">point</see> in OCS (object coordinate system).
        /// </summary>
        public Vector2 Origin
        {
            get { return this.origin; }
            set { this.origin = value; }
        }

        /// <summary>
        /// Gets or sets the base location <see cref="Vector2">point</see> in OCS (object coordinate system).
        /// </summary>
        public Vector2 ReferencePoint
        {
            get { return this.referencePoint; }
            set { this.referencePoint = value; }
        }

        /// <summary>
        /// Gets or sets the angle of rotation in degrees of the ordinate dimension local coordinate system.
        /// </summary>
        public double Rotation
        {
            get { return this.rotation; }
            set { MathHelper.NormalizeAngle(this.rotation = value); }
        }

        /// <summary>
        /// Gets or sets the length of the dimension line.
        /// </summary>
        public double Length
        {
            get { return this.length; }
            set { this.length = value; }
        }

        /// <summary>
        /// Gets or sets the local axis that measures the ordinate dimension.
        /// </summary>
        public OrdinateDimensionAxis Axis
        {
            get { return this.axis; }
            set { this.axis = value; }
        }

        /// <summary>
        /// Actual measurement.
        /// </summary>
        public override double Measurement
        {
            get { return this.axis == OrdinateDimensionAxis.X ? this.referencePoint.X : this.referencePoint.Y; }
        }

        #endregion

        #region overrides

        /// <summary>
        /// Gets the block that contains the entities that make up the dimension picture.
        /// </summary>
        /// <param name="name">Name to be assigned to the generated block.</param>
        /// <returns>The block that represents the actual dimension.</returns>
        internal override Block BuildBlock(string name)
        {
            return DimensionBlock.Build(this, name);
        }

        /// <summary>
        /// Creates a new OrdinateDimension that is a copy of the current instance.
        /// </summary>
        /// <returns>A new OrdinateDimension that is a copy of this instance.</returns>
        public override object Clone()
        {
            OrdinateDimension entity = new OrdinateDimension
            {
                //EntityObject properties
                Layer = (Layer) this.Layer.Clone(),
                Linetype = (Linetype) this.Linetype.Clone(),
                Color = (AciColor) this.Color.Clone(),
                Lineweight = this.Lineweight,
                Transparency = (Transparency) this.Transparency.Clone(),
                LinetypeScale = this.LinetypeScale,
                Normal = this.Normal,
                IsVisible = this.IsVisible,
                //Dimension properties
                Style = (DimensionStyle) this.Style.Clone(),
                AttachmentPoint = this.AttachmentPoint,
                LineSpacingStyle = this.LineSpacingStyle,
                LineSpacingFactor = this.LineSpacingFactor,
                //OrdinateDimension properties
                Origin = this.origin,
                ReferencePoint = this.referencePoint,
                Rotation = this.rotation,
                Length = this.length,
                Axis = this.axis,
                Elevation = this.Elevation
            };

            foreach (XData data in this.XData.Values)
                entity.XData.Add((XData) data.Clone());

            return entity;
        }

        #endregion
    }
}