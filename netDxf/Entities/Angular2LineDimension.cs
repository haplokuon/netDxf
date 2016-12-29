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
using System.Collections.Generic;
using netDxf.Blocks;
using netDxf.Tables;

namespace netDxf.Entities
{
    /// <summary>
    /// Represents a 3 point angular dimension <see cref="EntityObject">entity</see>.
    /// </summary>
    public class Angular2LineDimension :
        Dimension
    {
        #region private fields

        private double offset;
        private Vector2 startFirstLine;
        private Vector2 endFirstLine;
        private Vector2 startSecondLine;
        private Vector2 endSecondLine;
        private Vector3 arcDefinitionPoint;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>Angular2LineDimension</c> class.
        /// </summary>
        public Angular2LineDimension()
            : this(Vector2.Zero, Vector2.UnitX, Vector2.Zero, Vector2.UnitY, 0.1)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Angular2LineDimension</c> class.
        /// </summary>
        /// <param name="firstLine">First <see cref="Line">line</see> that defines the angle to measure.</param>
        /// <param name="secondLine">Second <see cref="Line">line</see> that defines the angle to measure.</param>
        /// <param name="offset">Distance between the center point and the dimension line.</param>
        public Angular2LineDimension(Line firstLine, Line secondLine, double offset)
            : this(firstLine, secondLine, offset, Vector3.UnitZ, DimensionStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Angular2LineDimension</c> class.
        /// </summary>
        /// <param name="firstLine">First <see cref="Line">line</see> that defines the angle to measure.</param>
        /// <param name="secondLine">Second <see cref="Line">line</see> that defines the angle to measure.</param>
        /// <param name="offset">Distance between the center point and the dimension line.</param>
        /// <param name="normal">Normal vector of the plane where the dimension is defined.</param>
        public Angular2LineDimension(Line firstLine, Line secondLine, double offset, Vector3 normal)
            : this(firstLine, secondLine, offset, normal, DimensionStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Angular2LineDimension</c> class.
        /// </summary>
        /// <param name="firstLine">First <see cref="Line">line</see> that defines the angle to measure.</param>
        /// <param name="secondLine">Second <see cref="Line">line</see> that defines the angle to measure.</param>
        /// <param name="offset">Distance between the center point and the dimension line.</param>
        /// <param name="style">The <see cref="DimensionStyle">style</see> to use with the dimension.</param>
        public Angular2LineDimension(Line firstLine, Line secondLine, double offset, DimensionStyle style)
            : this(firstLine, secondLine, offset, Vector3.UnitZ, style)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Angular2LineDimension</c> class.
        /// </summary>
        /// <param name="firstLine">First <see cref="Line">line</see> that defines the angle to measure.</param>
        /// <param name="secondLine">Second <see cref="Line">line</see> that defines the angle to measure.</param>
        /// <param name="offset">Distance between the center point and the dimension line.</param>
        /// <param name="style">The <see cref="DimensionStyle">style</see> to use with the dimension.</param>
        public Angular2LineDimension(Line firstLine, Line secondLine, double offset, Vector3 normal, DimensionStyle style)
            : base(DimensionType.Angular)
        {
            if (firstLine == null)
                throw new ArgumentNullException(nameof(firstLine));
            if (secondLine == null)
                throw new ArgumentNullException(nameof(secondLine));

            if (Vector3.AreParallel(firstLine.Direction, secondLine.Direction))
                throw new ArgumentException("The two lines that define the dimension are parallel.");

            IList<Vector3> ocsPoints =
                MathHelper.Transform(
                    new[]
                    {
                        firstLine.StartPoint,
                        firstLine.EndPoint,
                        secondLine.StartPoint,
                        secondLine.EndPoint
                    },
                    normal, CoordinateSystem.World, CoordinateSystem.Object);

            this.startFirstLine = new Vector2(ocsPoints[0].X, ocsPoints[0].Y);
            this.endFirstLine = new Vector2(ocsPoints[1].X, ocsPoints[1].Y);
            this.startSecondLine = new Vector2(ocsPoints[2].X, ocsPoints[2].Y);
            this.endSecondLine = new Vector2(ocsPoints[3].X, ocsPoints[3].Y);
            if (MathHelper.IsZero(offset))
                throw new ArgumentOutOfRangeException(nameof(offset), "The offset value cannot be zero.");
            this.offset = offset;

            if (style == null)
                throw new ArgumentNullException(nameof(style));
            this.Style = style;
            this.Normal = normal;
            this.Elevation = ocsPoints[0].Z;
        }

        /// <summary>
        /// Initializes a new instance of the <c>Angular2LineDimension</c> class.
        /// </summary>
        /// <param name="startFirstLine">Start <see cref="Vector2">point</see> of the first line that defines the angle to measure.</param>
        /// <param name="endFirstLine">End <see cref="Vector2">point</see> of the first line that defines the angle to measure.</param>
        /// <param name="startSecondLine">Start <see cref="Vector2">point</see> of the second line that defines the angle to measure.</param>
        /// <param name="endSecondLine">End <see cref="Vector2">point</see> of the second line that defines the angle to measure.</param>
        /// <param name="offset">Distance between the center point and the dimension line.</param>
        public Angular2LineDimension(Vector2 startFirstLine, Vector2 endFirstLine, Vector2 startSecondLine, Vector2 endSecondLine, double offset)
            : this(startFirstLine, endFirstLine, startSecondLine, endSecondLine, offset, DimensionStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Angular2LineDimension</c> class.
        /// </summary>
        /// <param name="startFirstLine">Start <see cref="Vector2">point</see> of the first line that defines the angle to measure.</param>
        /// <param name="endFirstLine">End <see cref="Vector2">point</see> of the first line that defines the angle to measure.</param>
        /// <param name="startSecondLine">Start <see cref="Vector2">point</see> of the second line that defines the angle to measure.</param>
        /// <param name="endSecondLine">End <see cref="Vector2">point</see> of the second line that defines the angle to measure.</param>
        /// <param name="offset">Distance between the center point and the dimension line.</param>
        /// <param name="style">The <see cref="DimensionStyle">style</see> to use with the dimension.</param>
        public Angular2LineDimension(Vector2 startFirstLine, Vector2 endFirstLine, Vector2 startSecondLine, Vector2 endSecondLine, double offset, DimensionStyle style)
            : base(DimensionType.Angular)
        {
            Vector2 dir1 = endFirstLine - startFirstLine;
            Vector2 dir2 = endSecondLine - startSecondLine;
            if (Vector2.AreParallel(dir1, dir2))
                throw new ArgumentException("The two lines that define the dimension are parallel.");

            this.startFirstLine = startFirstLine;
            this.endFirstLine = endFirstLine;
            this.startSecondLine = startSecondLine;
            this.endSecondLine = endSecondLine;

            if (MathHelper.IsZero(offset))
                throw new ArgumentOutOfRangeException(nameof(offset), "The offset value cannot be zero.");
            this.offset = offset;

            if (style == null)
                throw new ArgumentNullException(nameof(style));
            this.Style = style;
        }

        #endregion

        #region internal properties

        internal Vector3 ArcDefinitionPoint
        {
            get { return this.arcDefinitionPoint; }
            set { this.arcDefinitionPoint = value; }
        }

        #endregion

        #region public properties

        /// <summary>
        /// Start <see cref="Vector2">point</see> of the first line that defines the angle to measure in OCS (object coordinate system).
        /// </summary>
        public Vector2 StartFirstLine
        {
            get { return this.startFirstLine; }
            set { this.startFirstLine = value; }
        }

        /// <summary>
        /// End <see cref="Vector2">point</see> of the first line that defines the angle to measure in OCS (object coordinate system).
        /// </summary>
        public Vector2 EndFirstLine
        {
            get { return this.endFirstLine; }
            set { this.endFirstLine = value; }
        }

        /// <summary>
        /// Start <see cref="Vector2">point</see> of the second line that defines the angle to measure in OCS (object coordinate system).
        /// </summary>
        public Vector2 StartSecondLine
        {
            get { return this.startSecondLine; }
            set { this.startSecondLine = value; }
        }

        /// <summary>
        /// End <see cref="Vector2">point</see> of the second line that defines the angle to measure in OCS (object coordinate system).
        /// </summary>
        public Vector2 EndSecondLine
        {
            get { return this.endSecondLine; }
            set { this.endSecondLine = value; }
        }

        /// <summary>
        /// Gets or sets the distance between the center point and the dimension line.
        /// </summary>
        /// <remarks>Zero values are not allowed, and even if negative values are permitted they are not recommended.</remarks>
        public double Offset
        {
            get { return this.offset; }
            set
            {
                if (MathHelper.IsZero(value))
                    throw new ArgumentOutOfRangeException(nameof(value), "The offset value cannot be zero.");
                this.offset = value;
            }
        }

        /// <summary>
        /// Actual measurement.
        /// </summary>
        /// <remarks>The dimension is always measured in the plane defined by the normal.</remarks>
        public override double Measurement
        {
            get
            {
                Vector2 dirRef1 = this.endFirstLine - this.startFirstLine;
                Vector2 dirRef2 = this.endSecondLine - this.startSecondLine;
                return Vector2.AngleBetween(dirRef1, dirRef2)*MathHelper.RadToDeg;
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
            Vector2 dirRef1 = this.endFirstLine - this.startFirstLine;
            Vector2 dirRef2 = this.endSecondLine - this.startSecondLine;
            Vector2 center = MathHelper.FindIntersection(this.startFirstLine, dirRef1, this.startSecondLine, dirRef2);

            if (Vector2.IsNaN(center))
                throw new ArgumentException("The two lines that define the dimension are parallel.");
            Vector2 dirOffset = point - center;

            this.offset = Vector2.Distance(center, point);
            double cross = Vector2.CrossProduct(dirRef1, dirRef2);
            if (cross < 0)
            {
                Vector2 tmp = this.startFirstLine;
                this.startFirstLine = this.endFirstLine;
                this.endFirstLine = tmp;

                tmp = this.startSecondLine;
                this.startSecondLine = this.endSecondLine;
                this.endSecondLine = tmp;
            }

            double crossStart = Vector2.CrossProduct(dirRef1, dirOffset);
            double crossEnd = Vector2.CrossProduct(dirRef2, dirOffset);
            if (crossStart >= 0 && crossEnd < 0)
            {
            }
            if (crossStart >= 0 && crossEnd >= 0)
            {
                Vector2 tmp = this.startFirstLine;
                this.startFirstLine = this.endFirstLine;
                this.endFirstLine = tmp;
            }
            if (crossStart < 0 && crossEnd >= 0)
            {
                Vector2 tmp = this.startFirstLine;
                this.startFirstLine = this.endFirstLine;
                this.endFirstLine = tmp;

                tmp = this.startSecondLine;
                this.startSecondLine = this.endSecondLine;
                this.endSecondLine = tmp;
            }
            if (crossStart < 0 && crossEnd < 0)
            {
                Vector2 tmp = this.startSecondLine;
                this.startSecondLine = this.endSecondLine;
                this.endSecondLine = tmp;
            }
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
        /// Creates a new Angular2LineDimension that is a copy of the current instance.
        /// </summary>
        /// <returns>A new Angular2LineDimension that is a copy of this instance.</returns>
        public override object Clone()
        {
            Angular2LineDimension entity = new Angular2LineDimension
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
                UserText = this.UserText,
                //Angular2LineDimension properties
                StartFirstLine = this.startFirstLine,
                EndFirstLine = this.endFirstLine,
                StartSecondLine = this.startSecondLine,
                EndSecondLine = this.endSecondLine,
                Offset = this.offset,
                Elevation = this.Elevation
            };

            foreach (XData data in this.XData.Values)
                entity.XData.Add((XData) data.Clone());

            return entity;
        }

        #endregion
    }
}