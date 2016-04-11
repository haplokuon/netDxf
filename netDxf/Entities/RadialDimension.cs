#region netDxf, Copyright(C) 2016 Daniel Carvajal, Licensed under LGPL.
// 
//                         netDxf library
//  Copyright (C) 2009-2016 Daniel Carvajal (haplokuon@gmail.com)
//  
//  This library is free software; you can redistribute it and/or
//  modify it under the terms of the GNU Lesser General Public
//  License as published by the Free Software Foundation; either
//  version 2.1 of the License, or (at your option) any later version.
//  
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
//  
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
//  FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
//  COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
//  IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
//  CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using netDxf.Blocks;
using netDxf.Tables;

namespace netDxf.Entities
{
    /// <summary>
    /// Represents a radial dimension <see cref="EntityObject">entity</see>.
    /// </summary>
    public class RadialDimension :
        Dimension
    {
        #region private fields

        private Vector2 center;
        private Vector2 refPoint;
        private double offset;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>RadialDimension</c> class.
        /// </summary>
        public RadialDimension()
            : this(Vector2.Zero, Vector2.UnitX, 0.0, DimensionStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>RadialDimension</c> class.
        /// </summary>
        /// <param name="arc"><see cref="Arc">Arc</see> to measure.</param>
        /// <param name="rotation">Rotation in degrees of the dimension line.</param>
        /// <param name="offset">Distance between the reference point and the dimension text</param>
        /// <remarks>The center point and the definition point define the distance to be measure.</remarks>
        public RadialDimension(Arc arc, double rotation, double offset)
            : this(arc, rotation, offset, DimensionStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>RadialDimension</c> class.
        /// </summary>
        /// <param name="arc"><see cref="Arc">Arc</see> to measure.</param>
        /// <param name="rotation">Rotation in degrees of the dimension line.</param>
        /// <param name="offset">Distance between the reference point and the dimension text</param>
        /// <param name="style">The <see cref="DimensionStyle">style</see> to use with the dimension.</param>
        /// <remarks>The center point and the definition point define the distance to be measure.</remarks>
        public RadialDimension(Arc arc, double rotation, double offset, DimensionStyle style)
            : base(DimensionType.Radius)
        {
            Vector3 ocsCenter = MathHelper.Transform(arc.Center, arc.Normal, CoordinateSystem.World, CoordinateSystem.Object);
            this.center = new Vector2(ocsCenter.X, ocsCenter.Y);
            this.elevation = ocsCenter.Z;
            this.refPoint = Vector2.Polar(this.center, arc.Radius, rotation * MathHelper.DegToRad);
            this.offset = offset;
            this.normal = arc.Normal;
            if (style == null)
                throw new ArgumentNullException(nameof(style), "The Dimension style cannot be null.");
            this.style = style;
        }

        /// <summary>
        /// Initializes a new instance of the <c>RadialDimension</c> class.
        /// </summary>
        /// <param name="circle"><see cref="Circle">Circle</see> to measure.</param>
        /// <param name="rotation">Rotation in degrees of the dimension line.</param>
        /// <param name="offset">Distance between the reference point and the dimension text</param>
        /// <remarks>The center point and the definition point define the distance to be measure.</remarks>
        public RadialDimension(Circle circle, double rotation, double offset)
            : this(circle, rotation, offset, DimensionStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>RadialDimension</c> class.
        /// </summary>
        /// <param name="circle"><see cref="Circle">Circle</see> to measure.</param>
        /// <param name="rotation">Rotation in degrees of the dimension line.</param>
        /// <param name="offset">Distance between the reference point and the dimension text</param>
        /// <param name="style">The <see cref="DimensionStyle">style</see> to use with the dimension.</param>
        /// <remarks>The center point and the definition point define the distance to be measure.</remarks>
        public RadialDimension(Circle circle, double rotation, double offset, DimensionStyle style)
            : base(DimensionType.Radius)
        {
            Vector3 ocsCenter = MathHelper.Transform(circle.Center, circle.Normal, CoordinateSystem.World, CoordinateSystem.Object);
            this.center = new Vector2(ocsCenter.X, ocsCenter.Y);
            this.elevation = ocsCenter.Z;
            this.refPoint = Vector2.Polar(this.center, circle.Radius, rotation * MathHelper.DegToRad);
            if (offset < 0.0)
                throw new ArgumentOutOfRangeException(nameof(offset), "The offset value cannot be negative.");
            this.offset = offset;
            this.normal = circle.Normal;
            if (style == null)
                throw new ArgumentNullException(nameof(style), "The Dimension style cannot be null.");
            this.style = style;
        }

        /// <summary>
        /// Initializes a new instance of the <c>RadialDimension</c> class.
        /// </summary>
        /// <param name="centerPoint">Center <see cref="Vector2">point</see> of the circumference.</param>
        /// <param name="referencePoint"><see cref="Vector2">Point</see> on circle or arc.</param>
        /// <param name="offset">Distance between the reference point and the dimension text</param>
        /// <remarks>The center point and the definition point define the distance to be measure.</remarks>
        public RadialDimension(Vector2 centerPoint, Vector2 referencePoint, double offset)
            : this(centerPoint, referencePoint, offset, DimensionStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>RadialDimension</c> class.
        /// </summary>
        /// <param name="centerPoint">Center <see cref="Vector2">point</see> of the circumference.</param>
        /// <param name="referencePoint"><see cref="Vector2">Point</see> on circle or arc.</param>
        /// <param name="offset">Distance between the reference point and the dimension text</param>
        /// <param name="style">The <see cref="DimensionStyle">style</see> to use with the dimension.</param>
        /// <remarks>The center point and the definition point define the distance to be measure.</remarks>
        public RadialDimension(Vector2 centerPoint, Vector2 referencePoint, double offset, DimensionStyle style)
            : base(DimensionType.Radius)
        {
            this.center = centerPoint;
            this.refPoint = referencePoint;

            if (offset < 0.0)
                throw new ArgumentOutOfRangeException(nameof(offset), "The offset value cannot be negative.");
            this.offset = offset;

            if (style == null)
                throw new ArgumentNullException(nameof(style), "The Dimension style cannot be null.");
            this.style = style;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the center <see cref="Vector2">point</see> of the circumference in OCS (object coordinate system).
        /// </summary>
        public Vector2 CenterPoint
        {
            get { return this.center; }
            set { this.center = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="Vector2">point</see> on circumference or arc in OCS (object coordinate system).
        /// </summary>
        public Vector2 ReferencePoint
        {
            get { return this.refPoint; }
            set { this.refPoint = value; }
        }

        /// <summary>
        /// Gets or sets the distance between the center point and the dimension text.
        /// </summary>
        /// <remarks>
        /// If the value is close to the radius, the offset will be 2*DIMASZ+DIMGAP*DIMSCALE.
        /// If the value is set to zero the dimension will be shown at the center, both arrowheads will be shown and no center mark will be drawn.
        /// </remarks>
        public double Offset
        {
            get { return this.offset; }
            set
            {
                if (value < 0.0)
                    throw new ArgumentOutOfRangeException(nameof(value), "The offset value cannot be negative.");
                this.offset = value;
            }
        }

        /// <summary>
        /// Actual measurement.
        /// </summary>
        public override double Measurement
        {
            get { return Vector2.Distance(this.center, this.refPoint); }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Calculates the reference point and dimension offset from a point along the dimension line.
        /// </summary>
        /// <param name="point">Point along the dimension line.</param>
        public void SetDimensionLinePosition(Vector2 point)
        {
            double radius = Vector2.Distance(this.center, this.refPoint);
            double rotation = Vector2.Angle(this.center, point);
            this.refPoint = Vector2.Polar(this.center, radius, rotation);
            this.offset = Vector2.Distance(this.center, point);
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
        /// Creates a new RadialDimension that is a copy of the current instance.
        /// </summary>
        /// <returns>A new RadialDimension that is a copy of this instance.</returns>
        public override object Clone()
        {
            RadialDimension entity = new RadialDimension
            {
                //EntityObject properties
                Layer = (Layer)this.layer.Clone(),
                LineType = (LineType)this.lineType.Clone(),
                Color = (AciColor)this.color.Clone(),
                Lineweight = (Lineweight)this.lineweight.Clone(),
                Transparency = (Transparency)this.transparency.Clone(),
                LineTypeScale = this.lineTypeScale,
                Normal = this.normal,
                //Dimension properties
                Style = (DimensionStyle)this.style.Clone(),
                AttachmentPoint = this.attachmentPoint,
                LineSpacingStyle = this.lineSpacingStyle,
                LineSpacingFactor = this.lineSpacing,
                //RadialDimension properties
                CenterPoint = this.center,
                ReferencePoint = this.refPoint,
                Offset = this.offset,
                Elevation = this.elevation
            };

            foreach (XData data in this.XData.Values)
                entity.XData.Add((XData)data.Clone());

            return entity;
        }

        #endregion
    }
}