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

using netDxf.Blocks;
using netDxf.Tables;

namespace netDxf.Entities
{
    /// <summary>
    /// Represents an ordinate dimension <see cref="EntityObject">entity</see>.
    /// </summary>
    public class OrdinateDimension
        : Dimension
    {
        
        #region private fields

        private Vector3 origin;
        private Vector2 referencePoint;
        private double length;
        private double rotation;
        private OrdinateDimensionAxis axis;
        private Vector3 firstPoint;
        private Vector3 secondPoint;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>OrdinateDimension</c> class.
        /// </summary>
        public OrdinateDimension()
            :this(Vector3.Zero, Vector2.UnitX, 0.1, 0.0, OrdinateDimensionAxis.Y)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>OrdinateDimension</c> class.
        /// </summary>
        /// <param name="origin">Origin <see cref="Vector3">point</see> of the ordinate dimension.</param>
        /// <param name="referencePoint">Base location <see cref="Vector3">point</see> in local coordinates of the ordinate dimension.</param>
        /// <param name="length">Length of the dimension line.</param>
        /// <param name="axis">Length of the dimension line.</param>
        /// <param name="rotation">Angle of rotation in degrees of the dimension lines.</param>
        /// <remarks>The local coordinate system of the dimension is defined by the dimension normal and the rotation value.</remarks>
        public OrdinateDimension(Vector3 origin, Vector2 referencePoint, double length, double rotation, OrdinateDimensionAxis axis)
            : this(origin, referencePoint, length, axis, rotation, DimensionStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>OrdinateDimension</c> class.
        /// </summary>
        /// <param name="origin">Origin <see cref="Vector3">point</see> in world coordinates of the ordinate dimension.</param>
        /// <param name="referencePoint">Base location <see cref="Vector3">point</see> in local coordinates of the ordinate dimension.</param>
        /// <param name="length">Length of the dimension line.</param>
        /// <param name="axis">Local axis that measures the ordinate dimension.</param>
        /// <param name="rotation">Angle of rotation in degrees of the dimension lines.</param>
        /// <param name="style">The <see cref="DimensionStyle">style</see> to use with the dimension.</param>
        /// <remarks>The local coordinate system of the dimension is defined by the dimension normal and the rotation value.</remarks>
        public OrdinateDimension(Vector3 origin, Vector2 referencePoint, double length, OrdinateDimensionAxis axis, double rotation, DimensionStyle style)
            : base(DimensionType.Ordinate)
        {
            this.origin = origin;
            this.rotation = MathHelper.NormalizeAngle(rotation);
            this.length = length;
            this.referencePoint = referencePoint;
            this.axis = axis;
            this.style = style;
        }
          
        #endregion

        #region internal properties

        internal Vector3 FirstPoint
        {
            get { return firstPoint; }
            set { firstPoint = value; }
        }

        internal Vector3 SecondPoint
        {
            get { return secondPoint; }
            set { secondPoint = value; }
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the origin <see cref="Vector3">point</see> in world coordinates of the ordinate dimension.
        /// </summary>
        public Vector3 Origin
        {
            get { return origin; }
            set { origin = value; }
        }

        /// <summary>
        /// Gets or sets the base location <see cref="Vector3">point</see> in local coordinates of the ordinate dimension.
        /// </summary>
        public Vector2 ReferencePoint
        {
            get { return referencePoint; }
            set { referencePoint = value; }
        }

        /// <summary>
        /// Gets or sets the angle of rotation in degrees of the ordinate dimension local coordinate system.
        /// </summary>
        public double Rotation
        {
            get { return rotation; }
            set { MathHelper.NormalizeAngle(rotation = value); }
        }

        /// <summary>
        /// Gets or sets the length of the dimension line.
        /// </summary>
        public double Length
        {
            get { return length; }
            set { length = value; }
        }

        /// <summary>
        /// Gest or sets the local axis that measures the ordinate dimension.
        /// </summary>
        public OrdinateDimensionAxis Axis
        {
            get { return axis; }
            set { axis = value; }
        }

        /// <summary>
        /// Actual measurement.
        /// </summary>
        public override double Value
        {
            get { return this.axis == OrdinateDimensionAxis.X ? referencePoint.X : referencePoint.Y; }
        }
        #endregion

        #region overrides

        internal override Block BuildBlock(string name)
        {
            this.definitionPoint = this.origin;
            double angle = this.rotation * MathHelper.DegToRad;

            Vector3 localPoint = MathHelper.Transform(this.origin, this.normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            Vector2 refCenter = new Vector2(localPoint.X, localPoint.Y);

            double elev = localPoint.Z;
            
            Vector2 startPoint = refCenter + MathHelper.Transform(this.referencePoint, angle, MathHelper.CoordinateSystem.Object, MathHelper.CoordinateSystem.World);
            this.firstPoint = MathHelper.Transform(new Vector3(startPoint.X, startPoint.Y, elev), this.normal, MathHelper.CoordinateSystem.Object, MathHelper.CoordinateSystem.World);

            if (this.axis == OrdinateDimensionAxis.X)
                angle += MathHelper.HalfPI;
            Vector2 endPoint = Vector2.Polar(startPoint, this.length, angle);
            this.secondPoint = MathHelper.Transform(new Vector3(endPoint.X, endPoint.Y, elev), this.normal, MathHelper.CoordinateSystem.Object, MathHelper.CoordinateSystem.World);

            // reference points
            Layer defPoints = new Layer("Defpoints") { Plot = false };
            Point startRef = new Point(startPoint) { Layer = defPoints };
            Point endRef = new Point(endPoint) { Layer = defPoints };

            // dimension lines
            Line dimLine = new Line(Vector2.Polar(startPoint, this.style.DIMEXO, angle), endPoint);

            // dimension text
            Vector2 midText = Vector2.Polar(startPoint, this.length + this.style.DIMGAP, angle);
            this.midTextPoint = new Vector3(midText.X, midText.Y, elev); // this value is in OCS
            
            MText text = new MText(this.FormatDimensionText(this.Value),
                                   midText,
                                   this.style.DIMTXT, 0.0, this.style.TextStyle)
            {
                AttachmentPoint = MTextAttachmentPoint.MiddleLeft,
                Rotation = angle * MathHelper.RadToDeg
            };

            // drawing block
            Block dim = new Block(name, false);
            dim.Entities.Add(startRef);
            dim.Entities.Add(endRef);
            dim.Entities.Add(dimLine);
            dim.Entities.Add(text);
            this.block = dim;
            return dim;
        }

        /// <summary>
        /// Creates a new OrdinateDimension that is a copy of the current instance.
        /// </summary>
        /// <returns>A new OrdinateDimension that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new OrdinateDimension
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
                //OrdinateDimension properties
                Origin = this.origin,
                ReferencePoint = this.referencePoint,
                Rotation = this.rotation,
                Length = this.length,
                Axis = this.axis
            };
        }

        #endregion
        
    }
}
