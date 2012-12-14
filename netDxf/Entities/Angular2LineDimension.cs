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

using netDxf.Blocks;
using netDxf.Tables;

namespace netDxf.Entities
{
    /// <summary>
    /// Represents a 3 point angular dimension <see cref="IEntityObject">entity</see>.
    /// </summary>
    public class Angular2LineDimension :
        Dimension
    {
        #region private fields

        private double offset;
        private Vector3 startFirstLine;
        private Vector3 endFirstLine;
        private Vector3 startSecondLine;
        private Vector3 endSecondLine;
        private Vector3 arcDefinitionPoint;

        #endregion

        #region constructors
        /// <summary>
        /// Initializes a new instance of the <c>Angular2LineDimension</c> class.
        /// </summary>
        /// <param name="firstLine">First <see cref="Line">line</see> that defines the angle to measure.</param>
        /// <param name="secondLine">Second <see cref="Line">line</see> that defines the angle to measure.</param>
        /// <param name="offset">Distance between the center point and the dimension line.</param>
        public Angular2LineDimension(Line firstLine, Line secondLine, double offset)
            : this(firstLine, secondLine, offset, DimensionStyle.Default)
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
            : this(firstLine.StartPoint, firstLine.EndPoint, secondLine.StartPoint, secondLine.EndPoint, offset, style)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <c>Angular2LineDimension</c> class.
        /// </summary>
        /// <param name="startFirstLine">Start <see cref="Vector3">point</see> of the first line that defines de angle to measure.</param>
        /// <param name="endFirstLine">End <see cref="Vector3">point</see> of the first line that defines de angle to measure.</param>
        /// <param name="startSecondLine">Start <see cref="Vector3">point</see> of the second line that defines de angle to measure.</param>
        /// <param name="endSecondLine">End <see cref="Vector3">point</see> of the second line that defines de angle to measure.</param>
        /// <param name="offset">Distance between the center point and the dimension line.</param>
        public Angular2LineDimension(Vector3 startFirstLine, Vector3 endFirstLine, Vector3 startSecondLine, Vector3 endSecondLine, double offset)
            : this(startFirstLine, endFirstLine, startSecondLine, endSecondLine, offset, DimensionStyle.Default)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <c>Angular2LineDimension</c> class.
        /// </summary>
        /// <param name="startFirstLine">Start <see cref="Vector3">point</see> of the first line that defines de angle to measure.</param>
        /// <param name="endFirstLine">End <see cref="Vector3">point</see> of the first line that defines de angle to measure.</param>
        /// <param name="startSecondLine">Start <see cref="Vector3">point</see> of the second line that defines de angle to measure.</param>
        /// <param name="endSecondLine">End <see cref="Vector3">point</see> of the second line that defines de angle to measure.</param>
        /// <param name="offset">Distance between the center point and the dimension line.</param>
        /// <param name="style">The <see cref="DimensionStyle">style</see> to use with the dimension.</param>
        public Angular2LineDimension(Vector3 startFirstLine, Vector3 endFirstLine, Vector3 startSecondLine, Vector3 endSecondLine, double offset, DimensionStyle style)
            : base(DimensionType.Angular)
        {
            this.startFirstLine = startFirstLine;
            this.endFirstLine = endFirstLine;
            this.startSecondLine = startSecondLine;
            this.endSecondLine = endSecondLine;
            this.offset = offset;
            this.style = style;
            this.normal = CaluculateNormal();
        }

        #endregion

        #region internal properties

        internal Vector3 ArcDefinitionPoint
        {
            get { return arcDefinitionPoint; }
            set { arcDefinitionPoint = value; }
        }

        #endregion

        #region public properties

        /// <summary>
        /// Start <see cref="Vector3">point</see> of the first line that defines de angle to measure.
        /// </summary>
        public Vector3 StartFirstLine
        {
            get { return startFirstLine; }
            set
            {
                startFirstLine = value;
                this.normal = CaluculateNormal();
            }
        }

        /// <summary>
        /// End <see cref="Vector3">point</see> of the first line that defines de angle to measure.
        /// </summary>
        public Vector3 EndFirstLine
        {
            get { return endFirstLine; }
            set
            {
                endFirstLine = value;
                this.normal = CaluculateNormal();
            }
        }

        /// <summary>
        /// Start <see cref="Vector3">point</see> of the second line that defines de angle to measure.
        /// </summary>
        public Vector3 StartSecondLine
        {
            get { return startSecondLine; }
            set
            {
                startSecondLine = value;
                this.normal = CaluculateNormal();
            }
        }

        /// <summary>
        /// End <see cref="Vector3">point</see> of the second line that defines de angle to measure.
        /// </summary>
        public Vector3 EndSecondLine
        {
            get { return endSecondLine; }
            set
            {
                endSecondLine = value;
                this.normal = CaluculateNormal();
            }
        }

        /// <summary>
        /// Gets or sets the distance between the center point and the dimension line.
        /// </summary>
        public double Offset
        {
            get { return offset; }
            set { offset = value; }
        }

        /// <summary>
        /// Actual measurement.
        /// </summary>
        public override double Value
        {
            get
            {
                Vector3 dir1 = endFirstLine - startFirstLine;
                Vector3 dir2 = endSecondLine - startSecondLine;
                double angle = Vector3.AngleBetween(dir1, dir2) * MathHelper.RadToDeg;
                return angle < 0 ? 360 + angle : angle;
            }
        }

        #endregion

        #region private methods

        private Vector3 CaluculateNormal()
        {
            Vector3 dir1 = this.endFirstLine - this.startFirstLine;
            Vector3 dir2 = this.endSecondLine - this.startSecondLine;
            return Vector3.CrossProduct(dir1, dir2);
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
            Vector3 refPoint = MathHelper.Transform(this.startFirstLine, this.normal,
                                                    MathHelper.CoordinateSystem.World,
                                                    MathHelper.CoordinateSystem.Object);
            Vector2 refStartFirst = new Vector2(refPoint.X, refPoint.Y);
            double elev = refPoint.Z;

            refPoint = MathHelper.Transform(this.endFirstLine, this.normal,
                                                    MathHelper.CoordinateSystem.World,
                                                    MathHelper.CoordinateSystem.Object);
            Vector2 refEndFirst = new Vector2(refPoint.X, refPoint.Y);

            refPoint = MathHelper.Transform(this.startSecondLine, this.normal,
                                                    MathHelper.CoordinateSystem.World,
                                                    MathHelper.CoordinateSystem.Object);
            Vector2 refStartSecond = new Vector2(refPoint.X, refPoint.Y);

            refPoint = MathHelper.Transform(this.endSecondLine, this.normal,
                                                    MathHelper.CoordinateSystem.World,
                                                    MathHelper.CoordinateSystem.Object);
            Vector2 refEndSecond = new Vector2(refPoint.X, refPoint.Y);

            double startAngle = Vector2.Angle(refStartFirst, refEndFirst);
            double endAngle = Vector2.Angle(refStartSecond, refEndSecond);
            Vector2 centerRef;
            MathHelper.FindIntersection(refStartFirst, refEndFirst - refStartFirst, refStartSecond, refEndSecond - refStartSecond, out centerRef);

            // reference points
            Layer defPoints = new Layer("Defpoints");
            Point startFirstPoint = new Point(refStartFirst) { Layer = defPoints };
            Point endFirstPoint = new Point(refEndFirst) { Layer = defPoints };
            Point startSecondPoint = new Point(refStartSecond) { Layer = defPoints };
            Point endSecondPoint = new Point(refEndSecond) { Layer = defPoints };

            // dimension lines
            Vector2 startArc = Vector2.Polar(centerRef, this.offset, startAngle);
            Line startBorder = new Line(Vector2.Polar(refEndFirst, this.style.DIMEXO, startAngle),
                                        Vector2.Polar(startArc, this.style.DIMEXE, startAngle));

            Vector2 endArc = Vector2.Polar(centerRef, this.offset, endAngle);
            Line endBorder = new Line(Vector2.Polar(refEndSecond, this.style.DIMEXO, endAngle),
                                      Vector2.Polar(endArc, this.style.DIMEXE, endAngle));

            Arc dimArc = new Arc(centerRef, this.offset, startAngle * MathHelper.RadToDeg, endAngle * MathHelper.RadToDeg);

            // dimension arrows
            Vector2 arrowRefBegin = Vector2.Polar(startArc, this.style.DIMASZ, startAngle + MathHelper.HalfPI);
            Solid arrowBegin = new Solid(startArc,
                                         Vector2.Polar(arrowRefBegin, -this.style.DIMASZ / 6, startAngle),
                                         Vector2.Polar(arrowRefBegin, this.style.DIMASZ / 6, startAngle),
                                         startArc);

            Vector2 arrowRefEnd = Vector2.Polar(endArc, -this.style.DIMASZ, endAngle + MathHelper.HalfPI);
            Solid arrowEnd = new Solid(endArc,
                                       Vector2.Polar(arrowRefEnd, this.style.DIMASZ / 6, endAngle),
                                       Vector2.Polar(arrowRefEnd, -this.style.DIMASZ / 6, endAngle),
                                       endArc);

            // dimension text
            double aperture = this.Value;
            double rotText = Vector2.Angle(endArc, startArc);
            Vector2 midText = Vector2.Polar(centerRef, this.offset + this.style.DIMGAP, startAngle + aperture * MathHelper.DegToRad * 0.5);

            this.definitionPoint = this.endSecondLine;
            this.midTextPoint = new Vector3(midText.X, midText.Y, elev); // this value is in OCS
            this.arcDefinitionPoint = midTextPoint; // this value is in OCS

            MText text = new MText(this.FormatDimensionText(aperture),
                                   midTextPoint,
                                   this.style.DIMTXT, 0.0, this.style.TextStyle)
            {
                AttachmentPoint = MTextAttachmentPoint.BottomCenter,
                Rotation = rotText * MathHelper.RadToDeg
            };

            // drawing block
            Block dim = new Block(name);
            dim.Entities.Add(startFirstPoint);
            dim.Entities.Add(endFirstPoint);
            dim.Entities.Add(startSecondPoint);
            dim.Entities.Add(endSecondPoint);
            dim.Entities.Add(startBorder);
            dim.Entities.Add(endBorder);
            dim.Entities.Add(dimArc);
            dim.Entities.Add(arrowBegin);
            dim.Entities.Add(arrowEnd);
            dim.Entities.Add(text);
            this.block = dim;
            return dim;
        }

        #endregion

    }
}
