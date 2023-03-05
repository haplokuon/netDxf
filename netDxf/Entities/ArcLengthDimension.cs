#region netDxf library licensed under the MIT License
// 
//                       netDxf library
// Copyright (c) 2019-2023 Daniel Carvajal (haplokuon@gmail.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// 
#endregion

using System;
using netDxf.Blocks;
using netDxf.Tables;

namespace netDxf.Entities
{
    /// <summary>
    /// Represents an arc length dimension <see cref="EntityObject">entity</see>.
    /// </summary>
    public class ArcLengthDimension :
        Dimension
    {
        #region private fields

        private double offset;
        private Vector2 center;
        private double radius;
        private double startAngle;
        private double endAngle;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>ArcLengthDimension</c> class.
        /// </summary>
        public ArcLengthDimension()
            : this(Vector2.Zero, 1, 0, 0, 0.1)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>ArcLengthDimension</c> class.
        /// </summary>
        /// <param name="arc"><see cref="Arc">Arc</see> to measure.</param>
        /// <param name="offset">Distance between the measured arc and the dimension line.</param>
        public ArcLengthDimension(Arc arc, double offset)
            : this(arc, offset, DimensionStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>ArcLengthDimension</c> class.
        /// </summary>
        /// <param name="arc">Angle <see cref="Arc">arc</see> to measure.</param>
        /// <param name="offset">Distance between the measured arc and the dimension line.</param>
        /// <param name="style">The <see cref="DimensionStyle">style</see> to use with the dimension.</param>
        public ArcLengthDimension(Arc arc, double offset, DimensionStyle style)
            : base(DimensionType.ArcLength)
        {
            if (arc == null)
            {
                throw new ArgumentNullException(nameof(arc));
            }

            // This is the result of how the ArcLengthDimension is implemented in the DXF, although it is no more than other kind of dimension
            // for some reason it is considered its own entity. Its code 0 instead is "ARC_DIMENSION" instead of "DIMENSION" as it is in the rest of dimension entities.
            this.CodeName = DxfObjectCode.ArcDimension;

            Vector3 refPoint = MathHelper.Transform(arc.Center, arc.Normal, CoordinateSystem.World, CoordinateSystem.Object);
            this.center = new Vector2(refPoint.X, refPoint.Y);
            this.radius = arc.Radius;
            this.startAngle = arc.StartAngle;
            this.endAngle = arc.EndAngle;
            this.offset = offset;
            this.Style = style ?? throw new ArgumentNullException(nameof(style));
            this.Normal = arc.Normal;
            this.Elevation = refPoint.Z;
            this.Update();
        }

        /// <summary>
        /// Initializes a new instance of the <c>ArcLengthDimension</c> class.
        /// </summary>
        /// <param name="startPoint">Arc start point, the start point bulge must be different than zero.</param>
        /// <param name="endPoint">Arc end point.</param>
        /// <param name="offset">Distance between the measured arc and the dimension line.</param>
        public ArcLengthDimension(Polyline2DVertex startPoint, Polyline2DVertex endPoint, double offset)
            : this(startPoint.Position, endPoint.Position, startPoint.Bulge, offset)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <c>ArcLengthDimension</c> class.
        /// </summary>
        /// <param name="startPoint">Arc start point.</param>
        /// <param name="endPoint">Arc end point.</param>
        /// <param name="bulge">Bulge value.</param>
        /// <param name="offset">Distance between the measured arc and the dimension line.</param>
        public ArcLengthDimension(Vector2 startPoint, Vector2 endPoint, double bulge, double offset)
            : this(startPoint, endPoint, bulge, offset, DimensionStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>ArcLengthDimension</c> class.
        /// </summary>
        /// <param name="startPoint">Arc start point.</param>
        /// <param name="endPoint">Arc end point.</param>
        /// <param name="bulge">Bulge value.</param>
        /// <param name="offset">Distance between the measured arc and the dimension line.</param>
        /// <param name="style">The <see cref="DimensionStyle">style</see> to use with the dimension.</param>
        public ArcLengthDimension(Vector2 startPoint, Vector2 endPoint, double bulge, double offset, DimensionStyle style)
            : base(DimensionType.ArcLength)
        {
            // This is the result of how the ArcLengthDimension is implemented in the DXF, although it is no more than other kind of dimension
            // for some reason it is considered its own entity. Its code 0 instead is "ARC_DIMENSION" instead of "DIMENSION" as it is in the rest of dimension entities.
            this.CodeName = DxfObjectCode.ArcDimension;

            Tuple<Vector2, double, double, double> arcData = MathHelper.ArcFromBulge(startPoint, endPoint, bulge);
            this.center = arcData.Item1;
            this.radius = arcData.Item2;
            this.startAngle = arcData.Item3;
            this.endAngle = arcData.Item4;
            this.offset = offset;
            this.Style = style ?? throw new ArgumentNullException(nameof(style));
            this.Update();
        }

        /// <summary>
        /// Initializes a new instance of the <c>ArcLengthDimension</c> class.
        /// </summary>
        /// <param name="center">Center of the angle arc to measure.</param>
        /// <param name="radius">Arc radius.</param>
        /// <param name="startAngle">Arc start angle in degrees.</param>
        /// <param name="endAngle">Arc end angle in degrees.</param>
        /// <param name="offset">Distance between the measured arc and the dimension line.</param>
        public ArcLengthDimension(Vector2 center, double radius, double startAngle, double endAngle, double offset)
            : this(center, radius, startAngle, endAngle, offset, DimensionStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>ArcLengthDimension</c> class.
        /// </summary>
        /// <param name="center">Center of the angle arc to measure.</param>
        /// <param name="radius">Arc radius.</param>
        /// <param name="startAngle">Arc start angle in degrees.</param>
        /// <param name="endAngle">Arc end angle in degrees.</param>
        /// <param name="offset">Distance between the measured arc and the dimension line.</param>
        /// <param name="style">The <see cref="DimensionStyle">style</see> to use with the dimension.</param>
        public ArcLengthDimension(Vector2 center, double radius, double startAngle, double endAngle, double offset, DimensionStyle style)
            : base(DimensionType.ArcLength)
        {
            // This is the result of how the ArcLengthDimension is implemented in the DXF, although it is no more than other kind of dimension
            // for some reason it is considered its own entity. Its code 0 instead is "ARC_DIMENSION" instead of "DIMENSION" as it is in the rest of dimension entities.
            this.CodeName = DxfObjectCode.ArcDimension;

            this.center = center;
            this.radius = radius;
            this.startAngle = MathHelper.NormalizeAngle(startAngle);
            this.endAngle = MathHelper.NormalizeAngle(endAngle);
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
        /// Gets or sets the arc radius.
        /// </summary>
        public double Radius
        {
            get { return this.radius; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The arc radius must be greater than zero.");
                }
                this.radius = value;
            }
        }

        /// <summary>
        /// Gets or sets the arc start angle in degrees.
        /// </summary>
        public double StartAngle
        {
            get { return this.startAngle; }
            set { this.startAngle = MathHelper.NormalizeAngle(value); }
        }

        /// <summary>
        /// Gets or sets the arc end angle in degrees.
        /// </summary>
        public double EndAngle
        {
            get { return this.endAngle; }
            set { this.endAngle = MathHelper.NormalizeAngle(value); }
        }

        /// <summary>
        /// Gets the location of the dimension line arc.
        /// </summary>
        public Vector2 ArcDefinitionPoint
        {
            get { return this.defPoint; }
        }

        /// <summary>
        /// Gets or sets the distance between the center of the measured arc and the dimension line.
        /// </summary>
        /// <remarks>
        /// Positive values will measure the arc length between the start point and the end point while negative values will measure the opposite arc length.
        /// Even thought, zero values are allowed, they are not recommended.
        /// </remarks>
        public double Offset
        {
            get { return this.offset; }
            set { this.offset = value; }
        }

        /// <summary>
        /// Gets the angle of the measured arc in degrees.
        /// </summary>
        public double ArcAngle
        {
            get
            {
                double angle = MathHelper.NormalizeAngle(this.endAngle - this.startAngle);

                if (this.offset < 0)
                {
                    return 360 - angle;
                }
                return angle;
            }
        }

        /// <summary>
        /// Actual measurement.
        /// </summary>
        public override double Measurement
        {
            get { return this.radius * this.ArcAngle * MathHelper.DegToRad; }
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
            double newOffset = Vector2.Distance(this.center, point);

            this.offset = newOffset;
            Vector2 start = Vector2.Polar(this.center, this.radius, this.startAngle * MathHelper.DegToRad);
            Vector2 end = Vector2.Polar(this.center, this.radius, this.endAngle * MathHelper.DegToRad);
            Vector2 dirPoint = point - this.center;
            double cross1 = Vector2.CrossProduct(start - this.center, dirPoint);
            double cross2 = Vector2.CrossProduct(end - this.center, dirPoint);

            if (!(cross1 >= 0) || !(cross2 < 0))
            {
                this.offset *= -1;
            }

            double angle = this.offset >= 0 ? this.startAngle : this.endAngle;
            double midRot = (angle + 0.5 * this.ArcAngle) * MathHelper.DegToRad;
            Vector2 midDim = Vector2.Polar(this.center, Math.Abs(this.offset), midRot);
            this.defPoint = midDim;

            if (!this.TextPositionManuallySet)
            {
                DimensionStyleOverride styleOverride;
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

        #endregion

        #region overrides

        /// <summary>
        /// Moves, scales, and/or rotates the current entity given a 3x3 transformation matrix and a translation vector.
        /// </summary>
        /// <param name="transformation">Transformation matrix.</param>
        /// <param name="translation">Translation vector.</param>
        /// <remarks>
        /// Non-uniform and zero scaling local to the dimension entity is not supported, the measured circular arc will become an elliptical arc.<br />
        /// Matrix3 adopts the convention of using column vectors to represent a transformation matrix.
        /// </remarks>
        public override void TransformBy(Matrix3 transformation, Vector3 translation)
        {
            Vector3 newNormal = transformation * this.Normal;
            if (Vector3.Equals(Vector3.Zero, newNormal))
            {
                newNormal = this.Normal;
            }

            Matrix3 transOW = MathHelper.ArbitraryAxis(this.Normal);
            Matrix3 transWO = MathHelper.ArbitraryAxis(newNormal).Transpose();

            Vector3 newCenter = transOW * new Vector3(this.CenterPoint.X, this.CenterPoint.Y, this.Elevation);
            newCenter = transformation * newCenter + translation;
            newCenter = transWO * newCenter;

            Vector3 axis = transOW * new Vector3(this.Radius, 0.0, 0.0);
            axis = transformation * axis;
            axis = transWO * axis;
            Vector2 axisPoint = new Vector2(axis.X, axis.Y);
            double newRadius = axisPoint.Modulus();
            if (MathHelper.IsZero(newRadius))
            {
                newRadius = MathHelper.Epsilon;
            }

            Vector2 start = Vector2.Rotate(new Vector2(this.Radius, 0.0), this.StartAngle * MathHelper.DegToRad);
            Vector2 end = Vector2.Rotate(new Vector2(this.Radius, 0.0), this.EndAngle * MathHelper.DegToRad);

            Vector3 vStart = transOW * new Vector3(start.X, start.Y, 0.0);
            vStart = transformation * vStart;
            vStart = transWO * vStart;

            Vector3 vEnd = transOW * new Vector3(end.X, end.Y, 0.0);
            vEnd = transformation * vEnd;
            vEnd = transWO * vEnd;

            Vector2 startPoint = new Vector2(vStart.X, vStart.Y);
            Vector2 endPoint = new Vector2(vEnd.X, vEnd.Y);

            this.Normal = newNormal;
            this.CenterPoint = new Vector2(newCenter.X, newCenter.Y);
            this.Radius = newRadius;
            this.Elevation = newCenter.Z;

            if (Math.Sign(transformation.M11 * transformation.M22 * transformation.M33) < 0)
            {
                this.EndAngle = Vector2.Angle(startPoint) * MathHelper.RadToDeg;
                this.StartAngle = Vector2.Angle(endPoint) * MathHelper.RadToDeg;
            }
            else
            {
                this.StartAngle = Vector2.Angle(startPoint) * MathHelper.RadToDeg;
                this.EndAngle = Vector2.Angle(endPoint) * MathHelper.RadToDeg;
            }

            Vector3 v;
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
            
            this.SetDimensionLinePosition(this.defPoint);
        }

        /// <summary>
        /// Calculate the dimension reference points.
        /// </summary>
        protected override void CalculateReferencePoints()
        {
            DimensionStyleOverride styleOverride;
            double start = this.offset >= 0 ? this.startAngle : this.endAngle;
            double midRot = (start + 0.5 * this.ArcAngle) * MathHelper.DegToRad;
            Vector2 midDim = Vector2.Polar(this.center, Math.Abs(this.offset), midRot);

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
                    this.SetDimensionLinePosition(this.textRefPoint);
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
        /// Creates a new ArcLengthDimension that is a copy of the current instance.
        /// </summary>
        /// <returns>A new ArcLengthDimension that is a copy of this instance.</returns>
        public override object Clone()
        {
            ArcLengthDimension entity = new ArcLengthDimension
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
                Radius = this.radius,
                StartAngle = this.startAngle,
                EndAngle = this.endAngle,
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