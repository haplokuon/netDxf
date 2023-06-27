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

using netDxf.Tables;

namespace netDxf.Entities
{
    /// <summary>
    /// Represents a point <see cref="EntityObject">entity</see>.
    /// </summary>
    public class Point :
        EntityObject
    {
        #region private fields

        private Vector3 position;
        private double thickness;
        private double rotation;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>Point</c> class.
        /// </summary>
        /// <param name="position">Point <see cref="Vector3">position</see>.</param>
        public Point(Vector3 position)
            : base(EntityType.Point, DxfObjectCode.Point)
        {
            this.position = position;
            this.thickness = 0.0f;
            this.rotation = 0.0;
        }

        /// <summary>
        /// Initializes a new instance of the <c>Point</c> class.
        /// </summary>
        /// <param name="position">Point <see cref="Vector2">position</see>.</param>
        public Point(Vector2 position)
            : this(new Vector3(position.X, position.Y, 0.0))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Point</c> class.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="z">Z coordinate.</param>
        public Point(double x, double y, double z)
            : this(new Vector3(x, y, z))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Point</c> class.
        /// </summary>
        public Point()
            : this(Vector3.Zero)
        {
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the point <see cref="Vector3">position</see>.
        /// </summary>
        public Vector3 Position
        {
            get { return this.position; }
            set { this.position = value; }
        }

        /// <summary>
        /// Gets or sets the point thickness.
        /// </summary>
        public double Thickness
        {
            get { return this.thickness; }
            set { this.thickness = value; }
        }

        /// <summary>
        /// Gets or sets the point local rotation in degrees along its normal.
        /// </summary>
        public double Rotation
        {
            get { return this.rotation; }
            set { this.rotation = MathHelper.NormalizeAngle(value); }
        }

        #endregion

        #region overrides

        /// <summary>
        /// Moves, scales, and/or rotates the current entity given a 3x3 transformation matrix and a translation vector.
        /// </summary>
        /// <param name="transformation">Transformation matrix.</param>
        /// <param name="translation">Translation vector.</param>
        /// <remarks>Matrix3 adopts the convention of using column vectors to represent a transformation matrix.</remarks>
        public override void TransformBy(Matrix3 transformation, Vector3 translation)
        {
            Vector3 newPosition = transformation * this.Position + translation;
            Vector3 newNormal = transformation * this.Normal;
            if (Vector3.Equals(Vector3.Zero, newNormal))
            {
                newNormal = this.Normal;
            }

            Matrix3 transOW = MathHelper.ArbitraryAxis(this.Normal);
            Matrix3 transWO = MathHelper.ArbitraryAxis(newNormal).Transpose();

            Vector2 refAxis = Vector2.Rotate(Vector2.UnitX, this.Rotation * MathHelper.DegToRad);
            Vector3 v = transOW * new Vector3(refAxis.X, refAxis.Y, 0.0);
            v = transformation * v;
            v = transWO * v;
            double newRotation = Vector2.Angle(new Vector2(v.X, v.Y)) * MathHelper.RadToDeg;

            this.Position = newPosition;
            this.Rotation = newRotation;
            this.Normal = newNormal;
        }

        /// <summary>
        /// Creates a new Point that is a copy of the current instance.
        /// </summary>
        /// <returns>A new Point that is a copy of this instance.</returns>
        public override object Clone()
        {
            Point entity = new Point
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
                //Point properties
                Position = this.position,
                Rotation = this.rotation,
                Thickness = this.thickness
            };

            foreach (XData data in this.XData.Values)
            {
                entity.XData.Add((XData) data.Clone());
            }

            return entity;
        }

        #endregion
    }
}