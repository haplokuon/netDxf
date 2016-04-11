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
    /// Represents a linear or rotated dimension <see cref="EntityObject">entity</see>.
    /// </summary>
    public class LinearDimension :
        Dimension
    {
        #region private fields

        private Vector2 firstRefPoint;
        private Vector2 secondRefPoint;
        private double offset;
        private double rotation;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>LinearDimension</c> class.
        /// </summary>
        public LinearDimension()
            : this(Vector2.Zero, Vector2.UnitX, 0.1, 0.0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>LinearDimension</c> class.
        /// </summary>
        /// <param name="referenceLine">Reference <see cref="Line">line</see> of the dimension.</param>
        /// <param name="offset">Distance between the reference line and the dimension line.</param>
        /// <param name="rotation">Rotation in degrees of the dimension line.</param>
        /// <remarks>The reference points define the distance to be measure.</remarks>
        public LinearDimension(Line referenceLine, double offset, double rotation)
            : this(referenceLine, offset, rotation, Vector3.UnitZ, DimensionStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>LinearDimension</c> class.
        /// </summary>
        /// <param name="referenceLine">Reference <see cref="Line">line</see> of the dimension.</param>
        /// <param name="offset">Distance between the reference line and the dimension line.</param>
        /// <param name="rotation">Rotation in degrees of the dimension line.</param>
        /// <param name="style">The <see cref="DimensionStyle">style</see> to use with the dimension.</param>
        /// <remarks>The reference points define the distance to be measure.</remarks>
        public LinearDimension(Line referenceLine, double offset, double rotation, DimensionStyle style)
            : this(referenceLine, offset, rotation, Vector3.UnitZ, style)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>LinearDimension</c> class.
        /// </summary>
        /// <param name="referenceLine">Reference <see cref="Line">line</see> of the dimension.</param>
        /// <param name="offset">Distance between the reference line and the dimension line.</param>
        /// <param name="rotation">Rotation in degrees of the dimension line.</param>
        /// <param name="normal">Normal vector of the plane where the dimension is defined.</param>
        /// <remarks>The reference points define the distance to be measure.</remarks>
        public LinearDimension(Line referenceLine, double offset, double rotation, Vector3 normal)
            : this(referenceLine, offset, rotation, normal, DimensionStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>LinearDimension</c> class.
        /// </summary>
        /// <param name="referenceLine">Reference <see cref="Line">line</see> of the dimension.</param>
        /// <param name="offset">Distance between the reference line and the dimension line.</param>
        /// <param name="rotation">Rotation in degrees of the dimension line.</param>
        /// <param name="normal">Normal vector of the plane where the dimension is defined.</param>
        /// <param name="style">The <see cref="DimensionStyle">style</see> to use with the dimension.</param>
        /// <remarks>The reference line define the distance to be measure.</remarks>
        public LinearDimension(Line referenceLine, double offset, double rotation, Vector3 normal, DimensionStyle style)
            : base(DimensionType.Linear)
        {
            Vector3 ocsPoint;
            ocsPoint = MathHelper.Transform(referenceLine.StartPoint, normal, CoordinateSystem.World, CoordinateSystem.Object);
            this.firstRefPoint = new Vector2(ocsPoint.X, ocsPoint.Y);
            ocsPoint = MathHelper.Transform(referenceLine.EndPoint, normal, CoordinateSystem.World, CoordinateSystem.Object);
            this.secondRefPoint = new Vector2(ocsPoint.X, ocsPoint.Y);
            this.offset = offset;
            this.rotation = MathHelper.NormalizeAngle(rotation);
            if (style == null)
                throw new ArgumentNullException(nameof(style), "The Dimension style cannot be null.");
            this.style = style;
            this.normal = normal;
            this.elevation = ocsPoint.Z;
        }

        /// <summary>
        /// Initializes a new instance of the <c>LinearDimension</c> class.
        /// </summary>
        /// <param name="firstPoint">First reference <see cref="Vector2">point</see> of the dimension.</param>
        /// <param name="secondPoint">Second reference <see cref="Vector2">point</see> of the dimension.</param>
        /// <param name="offset">Distance between the mid point reference line and the dimension line.</param>
        /// <param name="rotation">Rotation in degrees of the dimension line.</param>
        /// <remarks>The reference points define the distance to be measure.</remarks>
        public LinearDimension(Vector2 firstPoint, Vector2 secondPoint, double offset, double rotation)
            : this(firstPoint, secondPoint, offset, rotation, DimensionStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>LinearDimension</c> class.
        /// </summary>
        /// <param name="firstPoint">First reference <see cref="Vector2">point</see> of the dimension.</param>
        /// <param name="secondPoint">Second reference <see cref="Vector2">point</see> of the dimension.</param>
        /// <param name="offset">Distance between the mid point reference line and the dimension line.</param>
        /// <param name="rotation">Rotation in degrees of the dimension line.</param>
        /// <param name="style">The <see cref="DimensionStyle">style</see> to use with the dimension.</param>
        /// <remarks>The reference points define the distance to be measure.</remarks>
        public LinearDimension(Vector2 firstPoint, Vector2 secondPoint, double offset, double rotation, DimensionStyle style)
            : base(DimensionType.Linear)
        {
            this.firstRefPoint = firstPoint;
            this.secondRefPoint = secondPoint;
            this.offset = offset;
            if (style == null)
                throw new ArgumentNullException(nameof(style), "The Dimension style cannot be null.");
            this.style = style;
            this.rotation = MathHelper.NormalizeAngle(rotation);
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the first definition point of the dimension in OCS (object coordinate system).
        /// </summary>
        public Vector2 FirstReferencePoint
        {
            get { return this.firstRefPoint; }
            set { this.firstRefPoint = value; }
        }

        /// <summary>
        /// Gets or sets the second definition point of the dimension in OCS (object coordinate system).
        /// </summary>
        public Vector2 SecondReferencePoint
        {
            get { return this.secondRefPoint; }
            set { this.secondRefPoint = value; }
        }

        /// <summary>
        /// Gets or sets the rotation of the dimension line.
        /// </summary>
        public double Rotation
        {
            get { return this.rotation; }
            set { this.rotation = MathHelper.NormalizeAngle(value); }
        }

        /// <summary>
        /// Gets or sets the distance between the mid point of the reference line and the dimension line.
        /// </summary>
        public double Offset
        {
            get { return this.offset; }
            set { this.offset = value; }
        }

        /// <summary>
        /// Gets the actual measurement.
        /// </summary>
        /// <remarks>The dimension is always measured in the plane defined by the normal.</remarks>
        public override double Measurement
        {
            get
            {
                double refRot = Vector2.Angle(this.firstRefPoint, this.secondRefPoint);
                return Math.Abs(Vector2.Distance(this.firstRefPoint, this.secondRefPoint) * Math.Cos(this.rotation * MathHelper.DegToRad - refRot));
            }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Calculates the dimension offset from a point along the dimension line.
        /// </summary>
        /// <param name="point">Point along the dimension line.</param>
        public void SetDimensionLinePosition(Vector2 point)
        {
            Vector2 midRef = Vector2.MidPoint(this.firstRefPoint, this.secondRefPoint);
            double dimRotation = this.rotation * MathHelper.DegToRad;
            Vector2 dimDir = new Vector2(Math.Cos(dimRotation), Math.Sin(dimRotation));
            Vector2 refDir = Vector2.Normalize(this.secondRefPoint - this.firstRefPoint);
            Vector2 offsetDir = point - this.firstRefPoint;
            double cross = Vector2.CrossProduct(refDir, offsetDir);
            this.offset = MathHelper.PointLineDistance(midRef, point, dimDir);
            if (cross < 0)
                this.offset *= -1;
            double relativeAngle = Vector2.AngleBetween(refDir, dimDir);
            if (relativeAngle > MathHelper.HalfPI && relativeAngle <= MathHelper.ThreeHalfPI)
                this.offset *= -1;
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
        /// Creates a new LinearDimension that is a copy of the current instance.
        /// </summary>
        /// <returns>A new LinearDimension that is a copy of this instance.</returns>
        public override object Clone()
        {

            LinearDimension entity = new LinearDimension
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
                //LinearDimension properties
                FirstReferencePoint = this.firstRefPoint,
                SecondReferencePoint = this.secondRefPoint,
                Rotation = this.rotation,
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