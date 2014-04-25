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

using System;
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

        private double diameter;
        private double rotation;
        private Vector3 center;
        private Vector3 circunferenceRef;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>RadialDimension</c> class.
        /// </summary>
        public DiametricDimension()
            : this(Vector3.Zero, 1.0, 0.0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>RadialDimension</c> class.
        /// </summary>
        /// <param name="circle"><see cref="Circle">Circle</see> to measure.</param>
        /// <param name="rotation">Rotation in degrees of the dimension line.</param>
        /// <remarks>The center point and the definition point define the distance to be measure.</remarks>
        public DiametricDimension(Circle circle, double rotation)
            : this(circle, rotation, DimensionStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>RadialDimension</c> class.
        /// </summary>
        /// <param name="circle"><see cref="Circle">Circle</see> to measure.</param>
        /// <param name="rotation">Rotation in degrees of the dimension line.</param>
        /// <param name="style">The <see cref="DimensionStyle">style</see> to use with the dimension.</param>
        /// <remarks>The center point and the definition point define the distance to be measure.</remarks>
        public DiametricDimension(Circle circle, double rotation, DimensionStyle style)
            : this(circle.Center, 2*circle.Radius, rotation, style )
        {
            this.normal = circle.Normal;
        }

        /// <summary>
        /// Initializes a new instance of the <c>RadialDimension</c> class.
        /// </summary>
        /// <param name="centerPoint">Center <see cref="Vector3">point</see> of the circunference.</param>
        /// <param name="diameter">Diameter value.</param>
        /// <param name="rotation">Rotation in degrees of the dimension line.</param>
        /// <remarks>The center point and the definition point define the distance to be measure.</remarks>
        public DiametricDimension(Vector2 centerPoint, double diameter, double rotation)
            : this(new Vector3(centerPoint.X, centerPoint.Y, 0.0), diameter, rotation, DimensionStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>RadialDimension</c> class.
        /// </summary>
        /// <param name="centerPoint">Center <see cref="Vector3">point</see> of the circunference.</param>
        /// <param name="diameter">Diameter value.</param>
        /// <param name="rotation">Rotation in degrees of the dimension line.</param>
        /// <remarks>The center point and the definition point define the distance to be measure.</remarks>
        public DiametricDimension(Vector3 centerPoint, double diameter, double rotation)
            : this(centerPoint, diameter, rotation, DimensionStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>RadialDimension</c> class.
        /// </summary>
        /// <param name="centerPoint">Center <see cref="Vector3">point</see> of the circunference.</param>
        /// <param name="diameter">Diameter value.</param>
        /// <param name="rotation">Rotation in degrees of the dimension line.</param>
        /// <param name="style">The <see cref="DimensionStyle">style</see> to use with the dimension.</param>
        /// <remarks>The center point and the definition point define the distance to be measure.</remarks>
        public DiametricDimension(Vector2 centerPoint, double diameter, double rotation, DimensionStyle style)
            : this(new Vector3(centerPoint.X, centerPoint.Y, 0.0), diameter, rotation, style)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>RadialDimension</c> class.
        /// </summary>
        /// <param name="centerPoint">Center <see cref="Vector3">point</see> of the circunference.</param>
        /// <param name="diameter">Diameter value.</param>
        /// <param name="rotation">Rotation in degrees of the dimension line.</param>
        /// <param name="style">The <see cref="DimensionStyle">style</see> to use with the dimension.</param>
        /// <remarks>The center point and the definition point define the distance to be measure.</remarks>
        public DiametricDimension(Vector3 centerPoint, double diameter, double rotation, DimensionStyle style)
            : base(DimensionType.Diameter)
        {
            this.center = centerPoint;
            this.diameter = diameter;
            this.rotation = MathHelper.NormalizeAngle(rotation);
            if (style == null)
                throw new ArgumentNullException("style", "The Dimension style cannot be null.");
            this.style = style;
        }

        #endregion

        #region internal properties

        /// <summary>
        /// Definition point for diameter, radius, and angular dimensions (in WCS).
        /// </summary>
        internal Vector3 CircunferencePoint
        {
            get { return circunferenceRef; }
            set { circunferenceRef = value; }
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the center <see cref="Vector3">point</see> of the circunference.
        /// </summary>
        public Vector3 CenterPoint
        {
            get { return this.center; }
            set { this.center = value; }
        }

        /// <summary>
        /// Gets or sets the diameter of the circunference.
        /// </summary>
        public double Diameter
        {
            get { return this.diameter; }
            set { this.diameter = value; }
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
        /// Actual measurement.
        /// </summary>
        public override double Value
        {
            get { return this.diameter; }
        }

        #endregion

        #region overrides

        /// <summary>
        /// Format the value for the dimension text.
        /// </summary>
        /// <param name="dimValue">Dimension value.</param>
        /// <returns>The formated text.</returns>
        internal override string FormatDimensionText(double dimValue)
        {
            return Symbols.Diameter + base.FormatDimensionText(dimValue);
        }

        /// <summary>
        /// Gets the the block that contains the entities that make up the dimension picture.
        /// </summary>
        /// <param name="name">Name to be asigned to the generated block.</param>
        /// <returns>The block that represents the actual dimension.</returns>
        internal override Block BuildBlock(string name)
        {
            // we will build the dimension block in object coordinates with normal the dimension normal
            Vector3 refPoint = MathHelper.Transform(this.center, this.normal,
                                                    MathHelper.CoordinateSystem.World,
                                                    MathHelper.CoordinateSystem.Object);

            Vector2 centerRef = new Vector2(refPoint.X, refPoint.Y);

            double elev = refPoint.Z;
            double refRotation = this.rotation*MathHelper.DegToRad;

            Vector2 firstRef = Vector2.Polar(centerRef, this.diameter * 0.5, refRotation);
            this.definitionPoint = MathHelper.Transform(new Vector3(firstRef.X, firstRef.Y, elev), this.normal,
                                                           MathHelper.CoordinateSystem.Object,
                                                           MathHelper.CoordinateSystem.World);

            Vector2 secondRef = Vector2.Polar(centerRef, -this.diameter * 0.5, refRotation);
            this.circunferenceRef = MathHelper.Transform(new Vector3(secondRef.X, secondRef.Y, elev), this.normal,
                                                           MathHelper.CoordinateSystem.Object,
                                                           MathHelper.CoordinateSystem.World);
            // reference points
            Layer defPoints = new Layer("Defpoints") { Plot = false };
            Point startRef = new Point(firstRef) { Layer = defPoints };
            Point endRef = new Point(secondRef) { Layer = defPoints };

            // dimension lines
            Line dimLine = new Line(firstRef, secondRef);

            // center cross
            double dist = Math.Abs(this.style.DIMCEN);
            Vector2 c1 = new Vector2(0, -dist) + centerRef;
            Vector2 c2 = new Vector2(0, dist) + centerRef;
            Line crossLine1 = new Line(c1, c2);
            c1 = new Vector2(-dist, 0) + centerRef;
            c2 = new Vector2(dist, 0) + centerRef;
            Line crossLine2 = new Line(c1, c2);

            // dimension arrows
            Vector2 arrowRef1 = Vector2.Polar(secondRef, this.style.DIMASZ, refRotation);
            Solid arrow1 = new Solid(secondRef,
                                    Vector2.Polar(arrowRef1, -this.style.DIMASZ / 6, refRotation + MathHelper.HalfPI),
                                    Vector2.Polar(arrowRef1, this.style.DIMASZ / 6, refRotation + MathHelper.HalfPI),
                                    secondRef);
            Vector2 arrowRef2 = Vector2.Polar(firstRef, -this.style.DIMASZ, refRotation);
            Solid arrow2 = new Solid(firstRef,
                                    Vector2.Polar(arrowRef2, this.style.DIMASZ / 6, refRotation + MathHelper.HalfPI),
                                    Vector2.Polar(arrowRef2, -this.style.DIMASZ / 6, refRotation + MathHelper.HalfPI),
                                    firstRef);

            // dimension text
            this.midTextPoint = new Vector3(centerRef.X, centerRef.Y, elev); // this value is in OCS
            MText text = new MText(this.FormatDimensionText(this.Value),
                                   Vector2.Polar(centerRef, this.style.DIMGAP, refRotation + MathHelper.HalfPI),
                                   this.style.DIMTXT, 0.0, this.style.TextStyle)
                             {
                                 AttachmentPoint = MTextAttachmentPoint.BottomCenter,
                                 Rotation = refRotation*MathHelper.RadToDeg
                             };

            Block dim = new Block(name, false);
            dim.Entities.Add(startRef);
            dim.Entities.Add(endRef);
            dim.Entities.Add(dimLine);
            dim.Entities.Add(crossLine1);
            dim.Entities.Add(crossLine2);
            dim.Entities.Add(arrow1);
            dim.Entities.Add(arrow2);
            dim.Entities.Add(text);
            this.block = dim;
            return dim;
        }

        /// <summary>
        /// Creates a new DiametricDimension that is a copy of the current instance.
        /// </summary>
        /// <returns>A new DiametricDimension that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new DiametricDimension
            {
                //EntityObject properties
                Color = this.color,
                Layer = this.layer,
                LineType = this.lineType,
                Lineweight = this.lineweight,
                LineTypeScale = this.lineTypeScale,
                Normal = this.normal,
                XData = this.xData,
                //Dimension properties
                Style = this.style,
                AttachmentPoint = this.attachmentPoint,
                LineSpacingStyle = this.lineSpacingStyle,
                LineSpacingFactor = this.lineSpacing,
                //DiametricDimension properties
                CenterPoint = this.center,
                Diameter = this.diameter,
                Rotation = this.rotation
            };
        }

        #endregion

    }
}