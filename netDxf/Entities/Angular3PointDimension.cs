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
    /// Represents a 3 point angular dimension <see cref="EntityObject">entity</see>.
    /// </summary>
    public class Angular3PointDimension :
        Dimension
    {
        #region private fields

        private double offset;
        private Vector3 centerPoint;
        private Vector3 firstPoint;
        private Vector3 secondPoint;
        #endregion

        #region constructors

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
            this.centerPoint = arc.Center;

            Vector3 refPoint = MathHelper.Transform(arc.Center, arc.Normal,
                                                    MathHelper.CoordinateSystem.World,
                                                    MathHelper.CoordinateSystem.Object);

            Vector2 centerRef = new Vector2(refPoint.X, refPoint.Y);
            double elev = refPoint.Z;

            Vector2 firstRef = Vector2.Polar(centerRef, arc.Radius, arc.StartAngle * MathHelper.DegToRad);
            this.firstPoint = MathHelper.Transform(new Vector3(firstRef.X, firstRef.Y, elev), arc.Normal,
                                                    MathHelper.CoordinateSystem.Object,
                                                    MathHelper.CoordinateSystem.World);

            Vector2 secondRef = Vector2.Polar(centerRef, arc.Radius, arc.EndAngle * MathHelper.DegToRad);
            this.secondPoint = MathHelper.Transform(new Vector3(secondRef.X, secondRef.Y, elev), arc.Normal,
                                                    MathHelper.CoordinateSystem.Object,
                                                    MathHelper.CoordinateSystem.World);

            this.normal = arc.Normal;
            this.offset = offset;
            this.style = style;      
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
        /// <param name="firstPoint">Angle start point.</param>
        /// <param name="secondPoint">Angle end point.</param>
        /// <param name="offset">Distance between the center point and the dimension line.</param>
        /// <param name="style">The <see cref="DimensionStyle">style</see> to use with the dimension.</param>
        public Angular3PointDimension(Vector3 centerPoint, Vector3 firstPoint, Vector3 secondPoint, double offset, DimensionStyle style)
            : base(DimensionType.Angular3Point)
        {
            this.centerPoint = centerPoint;

            Vector3 dir1 = firstPoint - centerPoint;
            Vector3 dir2 = secondPoint - centerPoint;
            this.normal = Vector3.CrossProduct(dir1, dir2);
            this.firstPoint = firstPoint;
            this.secondPoint = secondPoint;
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
            get { return firstPoint; }
            set { firstPoint = value; }
        }

        /// <summary>
        /// Definition point for linear and angular dimensions (in WCS).
        /// </summary>
        internal Vector3 SecondPoint
        {
            get { return secondPoint; }
            set { secondPoint = value; }
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the center <see cref="Vector3">point</see> of the arc.
        /// </summary>
        public Vector3 CenterPoint
        {
            get { return centerPoint; }
            set { centerPoint = value; }
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
                Vector3 localPoint = MathHelper.Transform(centerPoint, this.normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
                Vector2 refCenter = new Vector2(localPoint.X, localPoint.Y);

                localPoint = MathHelper.Transform(firstPoint, this.normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
                Vector2 refStart = new Vector2(localPoint.X, localPoint.Y);

                localPoint = MathHelper.Transform(secondPoint, this.normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
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
            Vector3 localPoint = MathHelper.Transform(centerPoint, this.normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            Vector2 refCenter = new Vector2(localPoint.X, localPoint.Y);

            localPoint = MathHelper.Transform(firstPoint, this.normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            Vector2 refStart = new Vector2(localPoint.X, localPoint.Y);

            localPoint = MathHelper.Transform(secondPoint, this.normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            Vector2 refEnd = new Vector2(localPoint.X, localPoint.Y);

            double elev = localPoint.Z;

            // reference points
            Layer defPoints = new Layer("Defpoints") { Plot = false };
            Point startRef = new Point(refStart) { Layer = defPoints };
            Point endRef = new Point(refEnd) { Layer = defPoints };
            Point center = new Point(refCenter) { Layer = defPoints };

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
                                   midTextPoint,
                                   this.style.DIMTXT, 0.0, this.style.TextStyle)
            {
                AttachmentPoint = MTextAttachmentPoint.BottomCenter,
                Rotation = rotText * MathHelper.RadToDeg
            };

            // drawing block
            Block dim = new Block(name);
            dim.Entities.Add(startRef);
            dim.Entities.Add(endRef);
            dim.Entities.Add(center);
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
