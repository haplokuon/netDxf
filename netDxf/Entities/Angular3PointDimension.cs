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
    /// Represents a 3 point angular dimension <see cref="EntityObject">entity</see>.
    /// </summary>
    public class Angular3PointDimension :
        Dimension
    {
        #region private fields

        private double offset;
        private Vector3 center;
        private Vector3 start;
        private Vector3 end;
        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>Angular3PointDimension</c> class.
        /// </summary>
        public Angular3PointDimension()
            : this(Vector3.Zero, Vector3.UnitX, Vector3.UnitY, 0.1)
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
            this.center = arc.Center;

            Vector3 refPoint = MathHelper.Transform(arc.Center, arc.Normal,
                                                    MathHelper.CoordinateSystem.World,
                                                    MathHelper.CoordinateSystem.Object);

            Vector2 centerRef = new Vector2(refPoint.X, refPoint.Y);
            double elev = refPoint.Z;

            Vector2 firstRef = Vector2.Polar(centerRef, arc.Radius, arc.StartAngle * MathHelper.DegToRad);
            this.start = MathHelper.Transform(new Vector3(firstRef.X, firstRef.Y, elev), arc.Normal,
                                                    MathHelper.CoordinateSystem.Object,
                                                    MathHelper.CoordinateSystem.World);

            Vector2 secondRef = Vector2.Polar(centerRef, arc.Radius, arc.EndAngle * MathHelper.DegToRad);
            this.end = MathHelper.Transform(new Vector3(secondRef.X, secondRef.Y, elev), arc.Normal,
                                                    MathHelper.CoordinateSystem.Object,
                                                    MathHelper.CoordinateSystem.World);

            this.normal = arc.Normal;
            this.offset = offset;
            if (style == null)
                throw new ArgumentNullException("style", "The Dimension style cannot be null.");
            this.style = style;      
        }

        /// <summary>
        /// Initializes a new instance of the <c>Angular3PointDimension</c> class.
        /// </summary>
        /// <param name="centerPoint">Center of the angle arc to mesaure.</param>
        /// <param name="startPoint">Angle start point.</param>
        /// <param name="endPoint">Angle end point.</param>
        /// <param name="offset">Distance between the center point and the dimension line.</param>
        public Angular3PointDimension(Vector2 centerPoint, Vector2 startPoint, Vector2 endPoint, double offset)
            : this(new Vector3(centerPoint.X, centerPoint.Y, 0.0), new Vector3(startPoint.X, startPoint.Y, 0.0), new Vector3(endPoint.X, endPoint.Y, 0.0), offset, DimensionStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Angular3PointDimension</c> class.
        /// </summary>
        /// <param name="centerPoint">Center of the angle arc to mesaure.</param>
        /// <param name="startPoint">Angle start point.</param>
        /// <param name="endPoint">Angle end point.</param>
        /// <param name="offset">Distance between the center point and the dimension line.</param>
        public Angular3PointDimension(Vector3 centerPoint, Vector3 startPoint, Vector3 endPoint, double offset)
            : this(centerPoint, startPoint, endPoint, offset, DimensionStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Angular3PointDimension</c> class.
        /// </summary>
        /// <param name="centerPoint">Center of the angle arc to mesaure.</param>
        /// <param name="startPoint">Angle start point.</param>
        /// <param name="endPoint">Angle end point.</param>
        /// <param name="offset">Distance between the center point and the dimension line.</param>
        /// <param name="style">The <see cref="DimensionStyle">style</see> to use with the dimension.</param>
        public Angular3PointDimension(Vector2 centerPoint, Vector2 startPoint, Vector2 endPoint, double offset, DimensionStyle style)
            : this(new Vector3(centerPoint.X, centerPoint.Y, 0.0), new Vector3(startPoint.X, startPoint.Y, 0.0), new Vector3(endPoint.X, endPoint.Y, 0.0), offset, style)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Angular3PointDimension</c> class.
        /// </summary>
        /// <param name="centerPoint">Center of the angle arc to mesaure.</param>
        /// <param name="startPoint">Angle start point.</param>
        /// <param name="endPoint">Angle end point.</param>
        /// <param name="offset">Distance between the center point and the dimension line.</param>
        /// <param name="style">The <see cref="DimensionStyle">style</see> to use with the dimension.</param>
        public Angular3PointDimension(Vector3 centerPoint, Vector3 startPoint, Vector3 endPoint, double offset, DimensionStyle style)
            : base(DimensionType.Angular3Point)
        {
            this.center = centerPoint;

            Vector3 dir1 = startPoint - centerPoint;
            Vector3 dir2 = endPoint - centerPoint;
            this.normal = Vector3.CrossProduct(dir1, dir2);
            this.start = startPoint;
            this.end = endPoint;
            this.offset = offset;
            this.style = style;
        }
        #endregion

        #region internal properties

        /// <summary>
        /// Definition point for linear and angular dimensions (in WCS).
        /// </summary>
        internal Vector3 FirstPoint
        {
            get { return this.start; }
            set { this.start = value; }
        }

        /// <summary>
        /// Definition point for linear and angular dimensions (in WCS).
        /// </summary>
        internal Vector3 SecondPoint
        {
            get { return this.end; }
            set { this.end = value; }
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the center <see cref="Vector3">point</see> of the arc.
        /// </summary>
        public Vector3 CenterPoint
        {
            get { return this.center; }
            set { this.center = value; }
        }

        /// <summary>
        /// Gets or sets the angle start point of the dimension.
        /// </summary>
        public Vector3 StartPoint
        {
            get { return this.start; }
            set { this.start = value; }
        }

        /// <summary>
        /// Gets or sets the angle end point of the dimension.
        /// </summary>
        public Vector3 EndPoint
        {
            get { return this.end; }
            set { this.end = value; }
        }

        /// <summary>
        /// Gets or sets the distance between the center point and the dimension line.
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
            get
            {
                Vector3 localPoint = MathHelper.Transform(this.center, this.normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
                Vector2 refCenter = new Vector2(localPoint.X, localPoint.Y);

                localPoint = MathHelper.Transform(this.start, this.normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
                Vector2 refStart = new Vector2(localPoint.X, localPoint.Y);

                localPoint = MathHelper.Transform(this.end, this.normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
                Vector2 refEnd = new Vector2(localPoint.X, localPoint.Y);

                double rotation = Vector2.Angle(refCenter, refStart);
                double angle = (Vector2.Angle(refCenter, refEnd)  - rotation) * MathHelper.RadToDeg;
                return angle < 0 ? 360 + angle : angle;
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
            Vector3 localPoint = MathHelper.Transform(this.center, this.normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            Vector2 refCenter = new Vector2(localPoint.X, localPoint.Y);

            localPoint = MathHelper.Transform(this.start, this.normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            Vector2 refStart = new Vector2(localPoint.X, localPoint.Y);

            localPoint = MathHelper.Transform(this.end, this.normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            Vector2 refEnd = new Vector2(localPoint.X, localPoint.Y);

            double elev = localPoint.Z;

            // reference points
            Layer defPoints = new Layer("Defpoints") { Plot = false };
            Point startRef = new Point(refStart) { Layer = defPoints };
            Point endRef = new Point(refEnd) { Layer = defPoints };
            Point centerPoint = new Point(refCenter) { Layer = defPoints };

            // dimension lines
            double startAngle = Vector2.Angle(refCenter, refStart);
            double endAngle = Vector2.Angle(refCenter, refEnd);

            Vector2 startArc = Vector2.Polar(refCenter, this.offset, startAngle);
            Line startBorder = new Line(Vector2.Polar(refStart, this.style.DIMEXO, startAngle),
                                        Vector2.Polar(startArc, this.style.DIMEXE, startAngle));

            Vector2 endArc = Vector2.Polar(refCenter, this.offset, endAngle);
            Line endBorder = new Line(Vector2.Polar(refEnd, this.style.DIMEXO, endAngle),
                                      Vector2.Polar(endArc, this.style.DIMEXE, endAngle));

            Arc dimArc = new Arc(refCenter, this.offset, startAngle * MathHelper.RadToDeg, endAngle * MathHelper.RadToDeg);


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
            Vector2 midText = Vector2.Polar(refCenter, this.offset + this.style.DIMGAP, startAngle + aperture * MathHelper.DegToRad * 0.5);
            this.definitionPoint = MathHelper.Transform(new Vector3(midText.X, midText.Y, elev), this.normal,
                                                    MathHelper.CoordinateSystem.Object,
                                                    MathHelper.CoordinateSystem.World);

            this.midTextPoint = new Vector3(midText.X, midText.Y, elev); // this value is in OCS
            
            MText text = new MText(this.FormatDimensionText(aperture),
                                   this.midTextPoint,
                                   this.style.DIMTXT, 0.0, this.style.TextStyle)
            {
                AttachmentPoint = MTextAttachmentPoint.BottomCenter,
                Rotation = rotText * MathHelper.RadToDeg
            };

            // drawing block
            Block dim = new Block(name, false);
            dim.Entities.Add(startRef);
            dim.Entities.Add(endRef);
            dim.Entities.Add(centerPoint);
            dim.Entities.Add(startBorder);
            dim.Entities.Add(endBorder);
            dim.Entities.Add(dimArc);
            dim.Entities.Add(arrowBegin);
            dim.Entities.Add(arrowEnd);
            dim.Entities.Add(text);
            this.block = dim;
            return dim;
        }

        /// <summary>
        /// Creates a new Angular3PointDimension that is a copy of the current instance.
        /// </summary>
        /// <returns>A new Angular3PointDimension that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new Angular3PointDimension
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
                //Angular3PointDimension properties
                CenterPoint = this.center,
                StartPoint = this.start,
                EndPoint = this.end,
                Offset = this.offset
            };
        }

        #endregion

    }
}
