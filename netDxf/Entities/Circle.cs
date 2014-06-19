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
using System.Collections.Generic;

namespace netDxf.Entities
{
    /// <summary>
    /// Represents a circle <see cref="EntityObject">entity</see>.
    /// </summary>
    public class Circle :
        EntityObject
    {
        #region private fields

        private Vector3 center;
        private double radius;
        private double thickness;
        
        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>Circle</c> class.
        /// </summary>
        /// <param name="center">Circle <see cref="Vector3">center</see> in world coordinates.</param>
        /// <param name="radius">Circle radius.</param>
        public Circle(Vector3 center, double radius)
            : base(EntityType.Circle, DxfObjectCode.Circle)
        {
            this.center = center;
            this.radius = radius;
            this.thickness = 0.0;
        }

        /// <summary>
        /// Initializes a new instance of the <c>Circle</c> class.
        /// </summary>
        /// <param name="center">Circle <see cref="Vector2">center</see> in world coordinates.</param>
        /// <param name="radius">Circle radius.</param>
        public Circle(Vector2 center, double radius)
            : this(new Vector3(center.X, center.Y, 0.0), radius)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Circle</c> class.
        /// </summary>
        public Circle() 
            : this(Vector3.Zero, 1.0)
        {
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the circle <see cref="netDxf.Vector3">center</see> in world coordinates.
        /// </summary>
        public Vector3 Center
        {
            get { return this.center; }
            set { this.center = value; }
        }

        /// <summary>
        /// Gets or set the circle radius.
        /// </summary>
        public double Radius
        {
            get { return this.radius; }
            set { this.radius = value; }
        }

        /// <summary>
        /// Gets or sets the circle thickness.
        /// </summary>
        public double Thickness
        {
            get { return this.thickness; }
            set { this.thickness = value; }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Converts the circle in a list of vertexes.
        /// </summary>
        /// <param name="precision">Number of vertexes generated.</param>
        /// <returns>A list vertexes that represents the circle expresed in object coordinate system.</returns>
        public IList<Vector2> PolygonalVertexes(int precision)
        {
            if (precision < 3)
                throw new ArgumentOutOfRangeException("precision", precision, "The circle precision must be greater or equal to three");

            List<Vector2> ocsVertexes = new List<Vector2>();

            double angle = MathHelper.TwoPI / precision;

            for (int i = 0; i < precision; i++)
            {
                double sine = this.radius * Math.Sin(MathHelper.HalfPI + angle * i);
                double cosine = this.radius * Math.Cos(MathHelper.HalfPI + angle * i);
                ocsVertexes.Add(new Vector2(cosine, sine));

            }
            return ocsVertexes;
        }

        /// <summary>
        /// Converts the circle in a Polyline.
        /// </summary>
        /// <param name="precision">Number of vertexes generated.</param>
        /// <returns>A new instance of <see cref="LwPolyline">LightWeightPolyline</see> that represents the circle.</returns>
        public LwPolyline ToPolyline(int precision)
        {
            IEnumerable<Vector2> vertexes = this.PolygonalVertexes(precision);
            Vector3 ocsCenter = MathHelper.Transform(this.Center, this.normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);

            LwPolyline poly = new LwPolyline
                                {
                                    Color = this.color,
                                    IsVisible = this.isVisible,
                                    Layer = this.layer,
                                    LineType = this.lineType,
                                    LineTypeScale = this.lineTypeScale,
                                    Lineweight = this.lineweight,
                                    XData = this.xData,
                                    Normal = this.normal,
                                    Elevation = ocsCenter.Z,
                                    Thickness = this.thickness,
                                    IsClosed = true
                                };
            foreach (Vector2 v in vertexes)
            {
                poly.Vertexes.Add(new LwPolylineVertex(v.X + ocsCenter.X, v.Y + ocsCenter.Y));
            }
            return poly;
        }

        #endregion

        #region overrides

        /// <summary>
        /// Creates a new Circle that is a copy of the current instance.
        /// </summary>
        /// <returns>A new Circle that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new Circle
            {
                //EntityObject properties
                Color = this.color,
                Layer = this.layer,
                LineType = this.lineType,
                Lineweight = this.lineweight,
                LineTypeScale = this.lineTypeScale,
                Normal = this.normal,
                XData = this.xData,
                //Circle properties
                Center = this.center,
                Radius = this.radius,
                Thickness = this.thickness
            };
        }

        #endregion

    }
}