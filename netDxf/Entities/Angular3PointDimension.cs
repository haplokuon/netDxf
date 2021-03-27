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
using System.Diagnostics;
using netDxf.Blocks;
using netDxf.Tables;

namespace netDxf.Entities
{
    /// <summary>
    /// Represents a 3 point angular dimension <see cref="EntityObject">entity</see>.
    /// </summary>
    public class Angular3PointDimension :
        Dimension
    {
        #region private fields

        private double offset;
        private Vector2 center;
        private Vector2 start;
        private Vector2 end;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>Angular3PointDimension</c> class.
        /// </summary>
        public Angular3PointDimension()
            : this(Vector2.Zero, Vector2.UnitX, Vector2.UnitY, 0.1)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Angular3PointDimension</c> class.
        /// </summary>
        /// <param name="arc"><see cref="Arc">Arc</see> to measure.</param>
        /// <param name="offset">Distance between the center of the arc and the dimension line.</param>
        public Angular3PointDimension(Arc arc, double offset)
            : this(arc, offset, DimensionStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Angular3PointDimension</c> class.
        /// </summary>
        /// <param name="arc">Angle <see cref="Arc">arc</see> to measure.</param>
        /// <param name="offset">Distance between the center of the arc and the dimension line.</param>
        /// <param name="style">The <see cref="DimensionStyle">style</see> to use with the dimension.</param>
        public Angular3PointDimension(Arc arc, double offset, DimensionStyle style)
            : base(DimensionType.Angular3Point)
        {
            if (arc == null)
            {
                throw new ArgumentNullException(nameof(arc));
            }

            Vector3 refPoint = MathHelper.Transform(arc.Center, arc.Normal, CoordinateSystem.World, CoordinateSystem.Object);
            this.center = new Vector2(refPoint.X, refPoint.Y);
            this.start = Vector2.Polar(this.center, arc.Radius, arc.StartAngle*MathHelper.DegToRad);
            this.end = Vector2.Polar(this.center, arc.Radius, arc.EndAngle*MathHelper.DegToRad);

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), "The offset value must be equal or greater than zero.");
            }
            this.offset = offset;

            this.Style = style ?? throw new ArgumentNullException(nameof(style));
            this.Normal = arc.Normal;
            this.Elevation = refPoint.Z;
            this.Update();
        }

        /// <summary>
        /// Initializes a new instance of the <c>Angular3PointDimension</c> class.
        /// </summary>
        /// <param name="centerPoint">Center of the angle arc to measure.</param>
        /// <param name="startPoint">Angle start point.</param>
        /// <param name="endPoint">Angle end point.</param>
        /// <param name="offset">Distance between the center point and the dimension line.</param>
        public Angular3PointDimension(Vector2 centerPoint, Vector2 startPoint, Vector2 endPoint, double offset)
            : this(centerPoint, startPoint, endPoint, offset, DimensionStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Angular3PointDimension</c> class.
        /// </summary>
        /// <param name="centerPoint">Center of the angle arc to measure.</param>
        /// <param name="startPoint">Angle start point.</param>
        /// <param name="endPoint">Angle end point.</param>
        /// <param name="offset">Distance between the center point and the dimension line.</param>
        /// <param name="style">The <see cref="DimensionStyle">style</see> to use with the dimension.</param>
        public Angular3PointDimension(Vector2 centerPoint, Vector2 startPoint, Vector2 endPoint, double offset, DimensionStyle style)
            : base(DimensionType.Angular3Point)
        {
            Vector2 dir1 = startPoint - centerPoint;
            Vector2 dir2 = endPoint - centerPoint;
            if (Vector2.AreParallel(dir1, dir2))
            {
                throw new ArgumentException("The two lines that define the dimension are parallel.");
            }
            this.center = centerPoint;
            this.start = startPoint;
            this.end = endPoint;
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), "The offset value must be equal or greater than zero.");
            }
            this.offset = offset;
            this.Style = style ?? throw new ArgumentNullException(nameof(style));
            this.Update();
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the center <see cref="Vector2">point</see> of the arc in OCS (object coordinate system).
        /// </summary>
        public Vector2 CenterPoint
        {
            get { return this.center; }
            set { this.center = value; }
        }

        /// <summary>
        /// Gets or sets the angle start <see cref="Vector2">point</see> of the dimension in OCS (object coordinate system).
        /// </summary>
        public Vector2 StartPoint
        {
            get { return this.start; }
            set { this.start = value; }
        }

        /// <summary>
        /// Gets or sets the angle end <see cref="Vector2">point</see> of the dimension in OCS (object coordinate system).
        /// </summary>
        public Vector2 EndPoint
        {
            get { return this.end; }
            set { this.end = value; }
        }

        /// <summary>
        /// Gets the location of the dimension line arc.
        /// </summary>
        public Vector2 ArcDefinitionPoint
        {
            get { return this.defPoint; }
        }

        /// <summary>
        /// Gets or sets the distance between the center point and the dimension line.
        /// </summary>
        /// <remarks>
        /// Offset values cannot be negative and, even thought, zero values are allowed, they are not recommended.
        /// </remarks>
        public double Offset
        {
            get { return this.offset; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "The offset value must be equal or greater than zero.");
                }
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
                double angle = (Vector2.Angle(this.center, this.end) - Vector2.Angle(this.center, this.start))*MathHelper.RadToDeg;
                return MathHelper.NormalizeAngle(angle);
            }
        }

        #endregion

        #region public method

        /// <summary>
        /// Calculates the dimension offset from a point along the dimension line.
        /// </summary>
        /// <param name="point">Point along the dimension line.</param>
        /// <remarks>
        /// The start and end points of the reference lines will be modified,
        /// the angle measurement is always made from the direction of the center-first point line to the direction of the center-second point line.
        /// </remarks>
        public void SetDimensionLinePosition(Vector2 point)
        {
            this.SetDimensionLinePosition(point, true);
        }

        #endregion

        #region private methods

        private void SetDimensionLinePosition(Vector2 point, bool updateRefs)
        {
            if (updateRefs)
            {
                Vector2 refStart = this.start;
                Vector2 refEnd = this.end;
                Vector2 dirStart = refStart - this.center;
                Vector2 dirEnd = refEnd - this.center;
                Vector2 dirPoint = point - this.center;
                double cross = Vector2.CrossProduct(dirStart, dirEnd);
                double cross1 = Vector2.CrossProduct(dirStart, dirPoint);
                double cross2 = Vector2.CrossProduct(dirEnd, dirPoint);
                if (cross >= 0)
                {
                    if (!(cross1 >= 0 && cross2 < 0))
                    {
                        this.start = refEnd;
                        this.end = refStart;
                    }
                }              
                else if (cross1 < 0 && cross2 >= 0)
                {
                    this.start = refEnd;
                    this.end = refStart;
                }
            }
            
            double newOffset = Vector2.Distance(this.center, point);
            this.offset = MathHelper.IsZero(newOffset) ? MathHelper.Epsilon : newOffset;

            double startAngle = Vector2.Angle(this.center, this.start);
            double midRot = startAngle + this.Measurement * MathHelper.DegToRad * 0.5;
            Vector2 midDim = Vector2.Polar(this.center, this.offset, midRot);
            this.defPoint = midDim;

            if (!this.TextPositionManuallySet)
            {
                DimensionStyleOverride styleOverride;
                double textGap = this.Style.TextOffset;
                if (this.StyleOverrides.TryGetValue(DimensionStyleOverrideType.TextOffset, out styleOverride))
                {
                    textGap = (double) styleOverride.Value;
                }
                double scale = this.Style.DimScaleOverall;
                if (this.StyleOverrides.TryGetValue(DimensionStyleOverrideType.DimScaleOverall, out styleOverride))
                {
                    scale = (double) styleOverride.Value;
                }

                double gap = textGap * scale;
                this.textRefPoint = midDim + gap * Vector2.Normalize(midDim - this.center);
            }
        }

        #endregion

        #region overrides

        /// <summary>
        /// Moves, scales, and/or rotates the current entity given a 3x3 transformation matrix and a translation vector.
        /// </summary>
        /// <param name="transformation">Transformation matrix.</param>
        /// <param name="translation">Translation vector.</param>
        /// <remarks>
        /// Non-uniform and zero scaling local to the dimension entity are not supported.<br />
        /// The transformation will not be applied if the resulting reference lines are parallel.<br />
        /// Matrix3 adopts the convention of using column vectors to represent a transformation matrix.
        /// </remarks>
        public override void TransformBy(Matrix3 transformation, Vector3 translation)
        {
            Vector3 newNormal = transformation * this.Normal;
            if (Vector3.Equals(Vector3.Zero, newNormal)) newNormal = this.Normal;

            Matrix3 transOW = MathHelper.ArbitraryAxis(this.Normal);
            Matrix3 transWO = MathHelper.ArbitraryAxis(newNormal).Transpose();

            Vector3 v = transOW * new Vector3(this.StartPoint.X, this.StartPoint.Y, this.Elevation);
            v = transformation * v + translation;
            v = transWO * v;
            Vector2 newStart = new Vector2(v.X, v.Y);
            double newElevation = v.Z;

            v = transOW * new Vector3(this.EndPoint.X, this.EndPoint.Y, this.Elevation);
            v = transformation * v + translation;
            v = transWO * v;
            Vector2 newEnd = new Vector2(v.X, v.Y);

            v = transOW * new Vector3(this.CenterPoint.X, this.CenterPoint.Y, this.Elevation);
            v = transformation * v + translation;
            v = transWO * v;
            Vector2 newCenter = new Vector2(v.X, v.Y);

            Vector2 dir1 = newStart - newCenter;
            Vector2 dir2 = newEnd - newCenter;
            if (Vector2.AreParallel(dir1, dir2))
            {
                Debug.Assert(false, "The transformation cannot be applied, the resulting reference lines are parallel.");
                return;
            }

            if (this.TextPositionManuallySet)
            {
                v = transOW * new Vector3(this.textRefPoint.X, this.textRefPoint.Y, this.Elevation);
                v = transformation * v + translation;
                v = transWO * v;
                this.textRefPoint = new Vector2(v.X, v.Y);
            }

            v = transOW * new Vector3(this.defPoint.X, this.defPoint.Y, this.Elevation);
            v = transformation * v + translation;
            v = transWO * v;
            this.defPoint = new Vector2(v.X, v.Y);

            this.StartPoint = newStart;
            this.EndPoint = newEnd;
            this.CenterPoint = newCenter;
            this.Elevation = newElevation;
            this.Normal = newNormal;

            this.SetDimensionLinePosition(this.defPoint);
        }

        /// <summary>
        /// Calculate the dimension reference points.
        /// </summary>
        protected override void CalculateReferencePoints()
        {
            Vector2 dir1 = this.start - this.center;
            Vector2 dir2 = this.end - this.center;
            if (Vector2.AreParallel(dir1, dir2))
            {
                throw new ArgumentException("The two lines that define the dimension are parallel.");
            }

            DimensionStyleOverride styleOverride;

            double measure = this.Measurement;
            double startAngle = Vector2.Angle(this.center, this.start);
            double midRot = startAngle + measure * MathHelper.DegToRad * 0.5;
            Vector2 midDim = Vector2.Polar(this.center, this.offset, midRot);

            this.defPoint = midDim;

            if (this.TextPositionManuallySet)
            {
                DimensionStyleFitTextMove moveText = this.Style.FitTextMove;
                if (this.StyleOverrides.TryGetValue(DimensionStyleOverrideType.FitTextMove, out styleOverride))
                {
                    moveText = (DimensionStyleFitTextMove)styleOverride.Value;
                }

                if (moveText == DimensionStyleFitTextMove.BesideDimLine)
                {
                    this.SetDimensionLinePosition(this.textRefPoint, false);
                }
            }
            else
            {
                double textGap = this.Style.TextOffset;
                if (this.StyleOverrides.TryGetValue(DimensionStyleOverrideType.TextOffset, out styleOverride))
                {
                    textGap = (double)styleOverride.Value;
                }
                double scale = this.Style.DimScaleOverall;
                if (this.StyleOverrides.TryGetValue(DimensionStyleOverrideType.DimScaleOverall, out styleOverride))
                {
                    scale = (double)styleOverride.Value;
                }

                double gap = textGap * scale;
                this.textRefPoint = midDim + gap * Vector2.Normalize(midDim - this.center);
            }
        }

        /// <summary>
        /// Gets the block that contains the entities that make up the dimension picture.
        /// </summary>
        /// <param name="name">Name to be assigned to the generated block.</param>
        /// <returns>The block that represents the actual dimension.</returns>
        protected override Block BuildBlock(string name)
        {
            return DimensionBlock.Build(this, name);
        }

        /// <summary>
        /// Creates a new Angular3PointDimension that is a copy of the current instance.
        /// </summary>
        /// <returns>A new Angular3PointDimension that is a copy of this instance.</returns>
        public override object Clone()
        {
            Angular3PointDimension entity = new Angular3PointDimension
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
                DefinitionPoint = this.DefinitionPoint,
                TextReferencePoint = this.TextReferencePoint,
                TextPositionManuallySet = this.TextPositionManuallySet,
                TextRotation = this.TextRotation,
                AttachmentPoint = this.AttachmentPoint,
                LineSpacingStyle = this.LineSpacingStyle,
                LineSpacingFactor = this.LineSpacingFactor,
                UserText = this.UserText,
                Elevation = this.Elevation,
                //Angular3PointDimension properties
                CenterPoint = this.center,
                StartPoint = this.start,
                EndPoint = this.end,
                Offset = this.offset
            };

            foreach (DimensionStyleOverride styleOverride in this.StyleOverrides.Values)
            {
                object copy = styleOverride.Value is ICloneable value ? value.Clone() : styleOverride.Value;
                entity.StyleOverrides.Add(new DimensionStyleOverride(styleOverride.Type, copy));
            }

            foreach (XData data in this.XData.Values)
            {
                entity.XData.Add((XData) data.Clone());
            }

            return entity;
        }

        #endregion
    }
}