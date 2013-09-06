#region netDxf, Copyright(C) 2013 Daniel Carvajal, Licensed under LGPL.

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
    /// Represents a dimension <see cref="EntityObject">entity</see> that is aligned the reference line.
    /// </summary>
    public class AlignedDimension :
        Dimension
    {
        #region private fields

        private Vector3 firstRefPoint;
        private Vector3 secondRefPoint;
        private double offset;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>AlignedDimension</c> class.
        /// </summary>
        public AlignedDimension()
            : this(Vector3.Zero, Vector3.UnitX, 0.1)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>AlignedDimension</c> class.
        /// </summary>
        /// <param name="referenceLine">Reference <see cref="Line">line</see> of the dimension.</param>
        /// <param name="offset">Distance between the reference line and the dimension line.</param>
        /// <remarks>The reference points define the distance to be measure.</remarks>
        public AlignedDimension(Line referenceLine, double offset)
            : this(referenceLine, offset, DimensionStyle.Default)
        {         
        }

        /// <summary>
        /// Initializes a new instance of the <c>AlignedDimension</c> class.
        /// </summary>
        /// <param name="referenceLine">Reference <see cref="Line">line</see> of the dimension.</param>
        /// <param name="offset">Distance between the reference line and the dimension line.</param>
        /// <param name="style">The <see cref="DimensionStyle">style</see> to use with the dimension.</param>
        /// <remarks>The reference line define the distance to be measure.</remarks>
        public AlignedDimension(Line referenceLine, double offset, DimensionStyle style)
            : this(referenceLine.StartPoint, referenceLine.EndPoint, offset, style)
        {
            this.normal = referenceLine.Normal;
        }

        /// <summary>
        /// Initializes a new instance of the <c>AlignedDimension</c> class.
        /// </summary>
        /// <param name="firstPoint">First reference <see cref="Vector2">point</see> of the dimension.</param>
        /// <param name="secondPoint">Second reference <see cref="Vector2">point</see> of the dimension.</param>
        /// <param name="offset">Distance between the reference line and the dimension line.</param>
        /// <remarks>The reference points define the distance to be measure.</remarks>
        public AlignedDimension(Vector2 firstPoint, Vector2 secondPoint, double offset)
            : this(new Vector3(firstPoint.X, firstPoint.Y, 0.0), new Vector3(secondPoint.X, secondPoint.Y, 0.0), offset, DimensionStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>AlignedDimension</c> class.
        /// </summary>
        /// <param name="firstPoint">First reference <see cref="Vector3">point</see> of the dimension.</param>
        /// <param name="secondPoint">Second reference <see cref="Vector3">point</see> of the dimension.</param>
        /// <param name="offset">Distance between the reference line and the dimension line.</param>
        /// <remarks>The reference points define the distance to be measure.</remarks>
        public AlignedDimension(Vector3 firstPoint, Vector3 secondPoint, double offset)
            : this(firstPoint, secondPoint, offset, DimensionStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>AlignedDimension</c> class.
        /// </summary>
        /// <param name="firstPoint">First reference <see cref="Vector2">point</see> of the dimension.</param>
        /// <param name="secondPoint">Second reference <see cref="Vector2">point</see> of the dimension.</param>
        /// <param name="offset">Distance between the reference line and the dimension line.</param>
        /// <param name="style">The <see cref="DimensionStyle">style</see> to use with the dimension.</param>
        /// <remarks>The reference points define the distance to be measure.</remarks>
        public AlignedDimension(Vector2 firstPoint, Vector2 secondPoint, double offset, DimensionStyle style)
            : this(new Vector3(firstPoint.X, firstPoint.Y, 0.0), new Vector3(secondPoint.X, secondPoint.Y, 0.0), offset, style)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>AlignedDimension</c> class.
        /// </summary>
        /// <param name="firstPoint">First reference <see cref="Vector3">point</see> of the dimension.</param>
        /// <param name="secondPoint">Second reference <see cref="Vector3">point</see> of the dimension.</param>
        /// <param name="offset">Distance between the reference line and the dimension line.</param>
        /// <param name="style">The <see cref="DimensionStyle">style</see> to use with the dimension.</param>
        /// <remarks>The reference points define the distance to be measure.</remarks>
        public AlignedDimension(Vector3 firstPoint, Vector3 secondPoint, double offset, DimensionStyle style)
            : base(DimensionType.Aligned)
        {
            this.firstRefPoint = firstPoint;
            this.secondRefPoint = secondPoint;
            this.offset = offset;
            if (style == null)
                throw new ArgumentNullException("style", "The Dimension style cannot be null.");
            this.style = style;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the first definition point of the dimension.
        /// </summary>
        public Vector3 FirstReferencePoint
        {
            get { return this.firstRefPoint; }
            set { this.firstRefPoint = value; }
        }

        /// <summary>
        /// Gets or sets the second definition point of the dimension.
        /// </summary>
        public Vector3 SecondReferencePoint
        {
            get { return this.secondRefPoint; }
            set { this.secondRefPoint = value; }
        }

        /// <summary>
        /// Gets or sets the distance between the reference line and the dimension line.
        /// </summary>
        public double Offset
        {
            get { return this.offset; }
            set { this.offset = value; }
        }

        /// <summary>
        /// Actual measurement.
        /// </summary>
        public override double Value
        {
            get { return Vector3.Distance(this.firstRefPoint, this.secondRefPoint); }
        }

        #endregion

        #region overrides

        /// <summary>
        /// Gets the the block that contains the entities that make up the dimension picture.
        /// </summary>
        /// <param name="name">Name to be asigned to the generated block.</param>
        /// <returns>The block that represents the actual dimension.</returns>
        internal override Block BuildBlock(string name)
        {
            // we will build the dimension block in object coordinates with normal the dimension normal
            Vector3 refPoint = MathHelper.Transform(this.firstRefPoint, this.normal,
                                                    MathHelper.CoordinateSystem.World,
                                                    MathHelper.CoordinateSystem.Object);

            Vector2 firstRef = new Vector2(refPoint.X, refPoint.Y);   
            refPoint = MathHelper.Transform(this.secondRefPoint, this.normal,
                                            MathHelper.CoordinateSystem.World,
                                            MathHelper.CoordinateSystem.Object);

            Vector2 secondRef = new Vector2(refPoint.X, refPoint.Y);
            double elev = refPoint.Z;
            double refRotation = Vector2.Angle(firstRef, secondRef);
            
            
            Vector2 startDimLine = Vector2.Polar(firstRef, this.offset, refRotation + MathHelper.HalfPI);
            Vector2 endDimLine = Vector2.Polar(secondRef, this.offset, refRotation + MathHelper.HalfPI);

            

            // reference points
            Layer defPoints = new Layer("Defpoints") {Plot = false};
            Point startRef = new Point(firstRef) { Layer = defPoints };
            Point endRef = new Point(secondRef) { Layer = defPoints };
            Point defPoint = new Point(endDimLine) { Layer = defPoints };

            // dimension lines
            double offsetRot = 0.0;
            if (this.offset < 0)
                offsetRot = MathHelper.PI;  
            Line startBorder = new Line(Vector2.Polar(firstRef, this.style.DIMEXO, offsetRot + refRotation + MathHelper.HalfPI),
                                        Vector2.Polar(startDimLine, this.style.DIMEXE, offsetRot + refRotation + MathHelper.HalfPI));

            Line endBorder = new Line(Vector2.Polar(secondRef, this.style.DIMEXO, offsetRot + refRotation + MathHelper.HalfPI),
                                      Vector2.Polar(endDimLine, this.style.DIMEXE, offsetRot + refRotation + MathHelper.HalfPI));

            Line dimLine = new Line(startDimLine, endDimLine);

            this.definitionPoint = MathHelper.Transform(new Vector3(endDimLine.X, endDimLine.Y, elev), this.normal,
                                                    MathHelper.CoordinateSystem.Object,
                                                    MathHelper.CoordinateSystem.World);

            Vector2 midDimLine = Vector2.MidPoint(startDimLine, endDimLine);
            
            // dimension arrows
            Vector2 arrowRefBegin = Vector2.Polar(startDimLine, this.style.DIMASZ, refRotation);
            Vector2 arrowRefEnd = Vector2.Polar(endDimLine, -this.style.DIMASZ, refRotation);

            Solid arrowBegin = new Solid(startDimLine,
                                         Vector2.Polar(arrowRefBegin, -this.style.DIMASZ/6, refRotation + MathHelper.HalfPI),
                                         Vector2.Polar(arrowRefBegin, this.style.DIMASZ/6, refRotation + MathHelper.HalfPI),
                                         startDimLine);

            Solid arrowEnd = new Solid(endDimLine,
                                       Vector2.Polar(arrowRefEnd, this.style.DIMASZ/6, refRotation + MathHelper.HalfPI),
                                       Vector2.Polar(arrowRefEnd, -this.style.DIMASZ/6, refRotation + MathHelper.HalfPI),
                                       endDimLine);

            // dimension text
            this.midTextPoint = new Vector3(midDimLine.X, midDimLine.Y, elev); // this value is in OCS
            MText text = new MText(this.FormatDimensionText(this.Value),
                                   Vector2.Polar(midDimLine, this.style.DIMGAP, refRotation + MathHelper.HalfPI),
                                   this.style.DIMTXT, 0.0, this.style.TextStyle)
                             {
                                 AttachmentPoint = MTextAttachmentPoint.BottomCenter,
                                 Rotation = refRotation*MathHelper.RadToDeg
                             };

            // drawing block
            Block dim = new Block(name, false);
            dim.Entities.Add(startRef);
            dim.Entities.Add(endRef);
            dim.Entities.Add(defPoint);
            dim.Entities.Add(startBorder);
            dim.Entities.Add(endBorder);
            dim.Entities.Add(dimLine);
            dim.Entities.Add(arrowBegin);
            dim.Entities.Add(arrowEnd);
            dim.Entities.Add(text);
            this.block = dim;
            return dim;
        }

        /// <summary>
        /// Creates a new AlignedDimension that is a copy of the current instance.
        /// </summary>
        /// <returns>A new AlignedDimension that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new AlignedDimension
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
                    //AlignedDimension properties
                    FirstReferencePoint = this.firstRefPoint,
                    SecondReferencePoint = this.secondRefPoint,
                    Offset = this.offset
                };
        }

        #endregion

    }
}