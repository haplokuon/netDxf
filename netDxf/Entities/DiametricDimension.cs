#region netDxf library licensed under the MIT License
// 
//                       netDxf library
// Copyright (c) Daniel Carvajal (haplokuon@gmail.com)
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
using System.Diagnostics;
using netDxf.Blocks;
using netDxf.Tables;

namespace netDxf.Entities
{
    /// <summary>
    /// Represents a diametric dimension <see cref="EntityObject">entity</see>.
    /// </summary>
    public class DiametricDimension :
        Dimension
    {
        #region private fields

        private Vector2 center;
        private Vector2 refPoint;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>DiametricDimension</c> class.
        /// </summary>
        public DiametricDimension()
            : this(Vector2.Zero, Vector2.UnitX, DimensionStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>DiametricDimension</c> class.
        /// </summary>
        /// <param name="arc"><see cref="Arc">Arc</see> to measure.</param>
        /// <param name="rotation">Rotation in degrees of the dimension line.</param>
        /// <remarks>The center point and the definition point define the distance to be measure.</remarks>
        public DiametricDimension(Arc arc, double rotation)
            : this(arc, rotation, DimensionStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>DiametricDimension</c> class.
        /// </summary>
        /// <param name="arc"><see cref="Arc">Arc</see> to measure.</param>
        /// <param name="rotation">Rotation in degrees of the dimension line.</param>
        /// <param name="style">The <see cref="DimensionStyle">style</see> to use with the dimension.</param>
        /// <remarks>The center point and the definition point define the distance to be measure.</remarks>
        public DiametricDimension(Arc arc, double rotation, DimensionStyle style)
            : base(DimensionType.Diameter)
        {
            if (arc == null)
                throw new ArgumentNullException(nameof(arc));

            Vector3 ocsCenter = MathHelper.Transform(arc.Center, arc.Normal, CoordinateSystem.World, CoordinateSystem.Object);
            this.center = new Vector2(ocsCenter.X, ocsCenter.Y);
            this.refPoint = Vector2.Polar(this.center, arc.Radius, rotation*MathHelper.DegToRad);

            this.Style = style ?? throw new ArgumentNullException(nameof(style));
            this.Normal = arc.Normal;
            this.Elevation = ocsCenter.Z;
            this.Update();
        }

        /// <summary>
        /// Initializes a new instance of the <c>DiametricDimension</c> class.
        /// </summary>
        /// <param name="circle"><see cref="Circle">Circle</see> to measure.</param>
        /// <param name="rotation">Rotation in degrees of the dimension line.</param>
        /// <remarks>The center point and the definition point define the distance to be measure.</remarks>
        public DiametricDimension(Circle circle, double rotation)
            : this(circle, rotation, DimensionStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>DiametricDimension</c> class.
        /// </summary>
        /// <param name="circle"><see cref="Circle">Circle</see> to measure.</param>
        /// <param name="rotation">Rotation in degrees of the dimension line.</param>
        /// <param name="style">The <see cref="DimensionStyle">style</see> to use with the dimension.</param>
        /// <remarks>The center point and the definition point define the distance to be measure.</remarks>
        public DiametricDimension(Circle circle, double rotation, DimensionStyle style)
            : base(DimensionType.Diameter)
        {
            if (circle == null)
                throw new ArgumentNullException(nameof(circle));

            Vector3 ocsCenter = MathHelper.Transform(circle.Center, circle.Normal, CoordinateSystem.World, CoordinateSystem.Object);
            this.center = new Vector2(ocsCenter.X, ocsCenter.Y);
            this.refPoint = Vector2.Polar(this.center, circle.Radius, rotation*MathHelper.DegToRad);

            this.Style = style ?? throw new ArgumentNullException(nameof(style));
            this.Normal = circle.Normal;
            this.Elevation = ocsCenter.Z;
            this.Update();
        }

        /// <summary>
        /// Initializes a new instance of the <c>DiametricDimension</c> class.
        /// </summary>
        /// <param name="centerPoint">Center <see cref="Vector2">point</see> of the circumference.</param>
        /// <param name="referencePoint"><see cref="Vector2">Point</see> on circle or arc.</param>
        /// <remarks>The center point and the definition point define the distance to be measure.</remarks>
        public DiametricDimension(Vector2 centerPoint, Vector2 referencePoint)
            : this(centerPoint, referencePoint, DimensionStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>DiametricDimension</c> class.
        /// </summary>
        /// <param name="centerPoint">Center <see cref="Vector2">point</see> of the circumference.</param>
        /// <param name="referencePoint"><see cref="Vector2">Point</see> on circle or arc.</param>
        /// <param name="style">The <see cref="DimensionStyle">style</see> to use with the dimension.</param>
        /// <remarks>The center point and the definition point define the distance to be measure.</remarks>
        public DiametricDimension(Vector2 centerPoint, Vector2 referencePoint, DimensionStyle style)
            : base(DimensionType.Diameter)
        {
            if(Vector2.Equals(centerPoint, referencePoint))
                throw new ArgumentException("The center and the reference point cannot be the same");
            this.center = centerPoint;
            this.refPoint = referencePoint;

            this.Style = style ?? throw new ArgumentNullException(nameof(style));

            this.Update();
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
        /// Actual measurement.
        /// </summary>
        public override double Measurement
        {
            get { return 2*Vector2.Distance(this.center, this.refPoint); }
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

            this.defPoint = Vector2.Polar(this.center, -radius, rotation);
            this.refPoint = Vector2.Polar(this.center, radius, rotation);

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
                double arrowSize = this.Style.ArrowSize;
                if (this.StyleOverrides.TryGetValue(DimensionStyleOverrideType.ArrowSize, out styleOverride))
                {
                    arrowSize = (double) styleOverride.Value;
                }

                Vector2 vec = Vector2.Normalize(this.refPoint - this.center);
                double minOffset = (2 * arrowSize + textGap) * scale;
                this.textRefPoint = this.refPoint + minOffset * vec;
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
        /// The transformation will not be applied if the resulting center and reference points are the same.<br />
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

            Vector3 v = transOW * new Vector3(this.CenterPoint.X, this.CenterPoint.Y, this.Elevation);
            v = transformation * v + translation;
            v = transWO * v;
            Vector2 newCenter = new Vector2(v.X, v.Y);
            double newElevation = v.Z;

            v = transOW * new Vector3(this.ReferencePoint.X, this.ReferencePoint.Y, this.Elevation);
            v = transformation * v + translation;
            v = transWO * v;
            Vector2 newRefPoint = new Vector2(v.X, v.Y);

            if(Vector2.Equals(newCenter, newRefPoint))
            {
                Debug.Assert(false, "The transformation cannot be applied, the resulting center and reference points are the same.");
                return;
            }

            v = transOW * new Vector3(this.textRefPoint.X, this.textRefPoint.Y, this.Elevation);
            v = transformation * v + translation;
            v = transWO * v;
            this.textRefPoint = new Vector2(v.X, v.Y);

            v = transOW * new Vector3(this.defPoint.X, this.defPoint.Y, this.Elevation);
            v = transformation * v + translation;
            v = transWO * v;
            this.defPoint = new Vector2(v.X, v.Y);

            this.CenterPoint = newCenter;
            this.ReferencePoint = newRefPoint;
            this.Elevation = newElevation;
            this.Normal = newNormal;
        }

        /// <summary>
        /// Calculate the dimension reference points.
        /// </summary>
        protected override void CalculateReferencePoints()
        {
            if (Vector2.Equals(this.center, this.refPoint))
            {
                throw new ArgumentException("The center and the reference point cannot be the same");
            }

            double measure = this.Measurement;
            Vector2 centerRef = this.center;
            Vector2 ref1 = this.refPoint;

            double angleRef = Vector2.Angle(centerRef, ref1);

            this.defPoint = Vector2.Polar(ref1, -measure, angleRef);

            if (this.TextPositionManuallySet)
            {
                this.SetDimensionLinePosition(this.textRefPoint);
            }
            else
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
                double arrowSize = this.Style.ArrowSize;
                if (this.StyleOverrides.TryGetValue(DimensionStyleOverrideType.ArrowSize, out styleOverride))
                {
                    arrowSize = (double) styleOverride.Value;
                }

                Vector2 vec = Vector2.Normalize(this.refPoint - this.center);
                double minOffset = (2 * arrowSize + textGap) * scale;
                this.textRefPoint = this.refPoint + minOffset * vec;
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
        /// Creates a new DiametricDimension that is a copy of the current instance.
        /// </summary>
        /// <returns>A new DiametricDimension that is a copy of this instance.</returns>
        public override object Clone()
        {
            DiametricDimension entity = new DiametricDimension
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
                //DiametricDimension properties
                CenterPoint = this.center,
                ReferencePoint = this.refPoint
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