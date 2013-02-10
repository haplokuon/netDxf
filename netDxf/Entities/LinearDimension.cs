#region netDxf, Copyright(C) 2012 Daniel Carvajal, Licensed under LGPL.

//                        netDxf library
// Copyright (C) 2012 Daniel Carvajal (haplokuon@gmail.com)
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
    /// Represents a linear or rotated dimension <see cref="EntityObject">entity</see>.
    /// </summary>
    public class LinearDimension :
        Dimension
    {
        #region private fields

        private Vector3 firstPoint;
        private Vector3 secondPoint;
        private double offset;
        private double rotation;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>LinearDimension</c> class.
        /// </summary>
        /// <param name="referenceLine">Reference <see cref="Line">line</see> of the dimension.</param>
        /// <param name="offset">Distance between the reference line and the dimension line.</param>
        /// <param name="rotation">Rotation in degrees of the dimension line.</param>
        /// <remarks>The reference points define the distance to be measure.</remarks>
        public LinearDimension(Line referenceLine, double offset, double rotation)
            : this(referenceLine, offset, rotation, DimensionStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>LinearDimension</c> class.
        /// </summary>
        /// <param name="referenceLine">Reference <see cref="Line">line</see> of the dimension.</param>
        /// <param name="offset">Distance between the reference line and the dimension line.</param>
        /// <param name="rotation">Rotation in degrees of the dimension line.</param>
        /// <param name="style">The <see cref="DimensionStyle">style</see> to use with the dimension.</param>
        /// <remarks>The reference line define the distance to be measure.</remarks>
        public LinearDimension(Line referenceLine, double offset, double rotation, DimensionStyle style)
            : this(referenceLine.StartPoint, referenceLine.EndPoint, offset, rotation, style)
        {
            this.normal = referenceLine.Normal;
        }

        /// <summary>
        /// Initializes a new instance of the <c>LinearDimension</c> class.
        /// </summary>
        /// <param name="firstPoint">First reference <see cref="Vector3">point</see> of the dimension.</param>
        /// <param name="secondPoint">Second reference <see cref="Vector3">point</see> of the dimension.</param>
        /// <param name="offset">Distance between the mid point reference line and the dimension line.</param>
        /// <param name="rotation">Rotation in degrees of the dimension line.</param>
        /// <remarks>The reference points define the distance to be measure.</remarks>
        public LinearDimension(Vector3 firstPoint, Vector3 secondPoint, double offset, double rotation)
            : this(firstPoint, secondPoint, offset, rotation, DimensionStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>LinearDimension</c> class.
        /// </summary>
        /// <param name="firstPoint">First reference <see cref="Vector3">point</see> of the dimension.</param>
        /// <param name="secondPoint">Second reference <see cref="Vector3">point</see> of the dimension.</param>
        /// <param name="offset">Distance between the mid point reference line and the dimension line.</param>
        /// <param name="rotation">Rotation in degrees of the dimension line.</param>
        /// <param name="style">The <see cref="DimensionStyle">style</see> to use with the dimension.</param>
        /// <remarks>The reference points define the distance to be measure.</remarks>
        public LinearDimension(Vector3 firstPoint, Vector3 secondPoint, double offset, double rotation, DimensionStyle style)
            : base(DimensionType.Linear)
        {
            this.firstPoint = firstPoint;
            this.secondPoint = secondPoint;
            this.offset = offset;
            this.style = style;
            this.rotation = rotation;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the first definition point of the dimension.
        /// </summary>
        public Vector3 FirstReferencePoint
        {
            get { return firstPoint; }
            set { this.firstPoint = value; }
        }

        /// <summary>
        /// Gets or sets the second definition point of the dimension.
        /// </summary>
        public Vector3 SecondReferencePoint
        {
            get { return secondPoint; }
            set { this.secondPoint = value; }
        }

        /// <summary>
        /// Gets or sets the distance between the mid point of the reference line and the dimension line.
        /// </summary>
        public double Offset
        {
            get { return offset; }
            set { offset = value; }
        }

        /// <summary>
        /// Gets or sets the rotation of the dimension line.
        /// </summary>
        public double Rotation
        {
            get { return rotation; }
            set { this.rotation = value; }
        }

        /// <summary>
        /// Gets the actual measurement.
        /// </summary>
        public override double Value
        {
            get
            {
                Vector3 refPoint = MathHelper.Transform(this.firstPoint, this.normal,
                                                        MathHelper.CoordinateSystem.World,
                                                        MathHelper.CoordinateSystem.Object);

                Vector2 firstRef = new Vector2(refPoint.X, refPoint.Y);
                refPoint = MathHelper.Transform(this.secondPoint, this.normal,
                                                MathHelper.CoordinateSystem.World,
                                                MathHelper.CoordinateSystem.Object);

                Vector2 secondRef = new Vector2(refPoint.X, refPoint.Y);

                double refRot = Vector2.Angle(firstRef, secondRef);
                double dimRot = this.rotation * MathHelper.DegToRad;
                return Math.Abs(Vector2.Distance(firstRef, secondRef) * Math.Cos(dimRot - refRot));
            }
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
            Vector3 refPoint = MathHelper.Transform(this.firstPoint, this.normal,
                                                    MathHelper.CoordinateSystem.World,
                                                    MathHelper.CoordinateSystem.Object);

            Vector2 firstRef = new Vector2(refPoint.X, refPoint.Y);
            refPoint = MathHelper.Transform(this.secondPoint, this.normal,
                                            MathHelper.CoordinateSystem.World,
                                            MathHelper.CoordinateSystem.Object);

            Vector2 secondRef = new Vector2(refPoint.X, refPoint.Y);

            double measurement = this.Value;
            double dimRot = this.rotation * MathHelper.DegToRad;
            double elev = refPoint.Z;

            Vector2 midRef = Vector2.MidPoint(firstRef, secondRef);

            Vector2 midDimLine = Vector2.Polar(midRef, this.offset, dimRot + MathHelper.HalfPI);


            Vector2 startDimLine = Vector2.Polar(midDimLine, measurement*0.5, dimRot + MathHelper.PI);
            Vector2 endDimLine = Vector2.Polar(midDimLine, measurement*0.5, dimRot);

            // reference points
            Layer defPoints = new Layer("Defpoints") { Plot = false };
            Point startRef = new Point(firstRef) { Layer = defPoints };
            Point endRef = new Point(secondRef) { Layer = defPoints };
            Point defPoint = new Point(endDimLine) { Layer = defPoints };

            // dimension lines
            Line startBorder = new Line(Vector2.Polar(firstRef, this.style.DIMEXO, dimRot + MathHelper.HalfPI),
                                        Vector2.Polar(startDimLine, this.style.DIMEXE, dimRot + MathHelper.HalfPI));

            Line endBorder = new Line(Vector2.Polar(secondRef, this.style.DIMEXO, dimRot + MathHelper.HalfPI),
                                      Vector2.Polar(endDimLine, this.style.DIMEXE, dimRot + MathHelper.HalfPI));

            Line dimLine = new Line(startDimLine, endDimLine);

            this.definitionPoint = MathHelper.Transform(new Vector3(endDimLine.X, endDimLine.Y, elev), this.normal,
                                                        MathHelper.CoordinateSystem.Object,
                                                        MathHelper.CoordinateSystem.World);


            // dimension arrows
            Vector2 arrowRefBegin = Vector2.Polar(startDimLine, this.style.DIMASZ, dimRot);
            Solid arrowBegin = new Solid(startDimLine,
                                         Vector2.Polar(arrowRefBegin, -this.style.DIMASZ/6, dimRot + MathHelper.HalfPI),
                                         Vector2.Polar(arrowRefBegin, this.style.DIMASZ/6, dimRot + MathHelper.HalfPI),
                                         startDimLine);

            Vector2 arrowRefEnd = Vector2.Polar(endDimLine, -this.style.DIMASZ, dimRot);
            Solid arrowEnd = new Solid(endDimLine,
                                       Vector2.Polar(arrowRefEnd, this.style.DIMASZ/6, dimRot + MathHelper.HalfPI),
                                       Vector2.Polar(arrowRefEnd, -this.style.DIMASZ/6, dimRot + MathHelper.HalfPI),
                                       endDimLine);

            // dimension text
            this.midTextPoint = new Vector3(midDimLine.X, midDimLine.Y, elev); // this value is in OCS
            MText text = new MText(this.FormatDimensionText(this.Value),
                                   Vector2.Polar(midDimLine, this.style.DIMGAP, dimRot + MathHelper.HalfPI),
                                   this.style.DIMTXT, 0.0, this.style.TextStyle)
                             {
                                 AttachmentPoint = MTextAttachmentPoint.BottomCenter,
                                 Rotation = this.rotation
                             };

            // drawing block
            Block dim = new Block(name);
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

        #endregion
    }
}