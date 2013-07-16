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

namespace netDxf.Entities
{
    /// <summary>
    /// Represents a solid <see cref="EntityObject">entity</see>.
    /// </summary>
    public class Solid :
        EntityObject
    {
        #region private fields

        private Vector3 firstVertex;
        private Vector3 secondVertex;
        private Vector3 thirdVertex;
        private Vector3 fourthVertex;
        private double thickness;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>Solid</c> class.
        /// </summary>
        /// <param name="firstVertex">Solid <see cref="Vector3">first vertex</see>.</param>
        /// <param name="secondVertex">Solid <see cref="Vector3">second vertex</see>.</param>
        /// <param name="thirdVertex">Solid <see cref="Vector3">third vertex</see>.</param>
        /// <param name="fourthVertex">Solid <see cref="Vector3">fourth vertex</see>.</param>
        public Solid(Vector3 firstVertex, Vector3 secondVertex, Vector3 thirdVertex, Vector3 fourthVertex)
            : base(EntityType.Solid, DxfObjectCode.Solid)
        {
            this.firstVertex = firstVertex;
            this.secondVertex = secondVertex;
            this.thirdVertex = thirdVertex;
            this.fourthVertex = fourthVertex;
            this.thickness = 0.0;
        }

        /// <summary>
        /// Initializes a new instance of the <c>Solid</c> class.
        /// </summary>
        /// <param name="firstVertex">Solid <see cref="Vector2">first vertex</see>.</param>
        /// <param name="secondVertex">Solid <see cref="Vector2">second vertex</see>.</param>
        /// <param name="thirdVertex">Solid <see cref="Vector2">third vertex</see>.</param>
        /// <param name="fourthVertex">Solid <see cref="Vector2">fourth vertex</see>.</param>
        public Solid(Vector2 firstVertex, Vector2 secondVertex, Vector2 thirdVertex, Vector2 fourthVertex)
            : this(new Vector3(firstVertex.X, firstVertex.Y, 0.0),
                   new Vector3(secondVertex.X, secondVertex.Y, 0.0),
                   new Vector3(thirdVertex.X, thirdVertex.Y, 0.0),
                   new Vector3(fourthVertex.X, fourthVertex.Y, 0.0))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Solid</c> class.
        /// </summary>
        public Solid()
            : this(Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero)
        {
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the first solid <see cref="Vector3">vertex</see>.
        /// </summary>
        public Vector3 FirstVertex
        {
            get { return this.firstVertex; }
            set { this.firstVertex = value; }
        }

        /// <summary>
        /// Gets or sets the second solid <see cref="Vector3">vertex</see>.
        /// </summary>
        public Vector3 SecondVertex
        {
            get { return this.secondVertex; }
            set { this.secondVertex = value; }
        }

        /// <summary>
        /// Gets or sets the third solid <see cref="Vector3">vertex</see>.
        /// </summary>
        public Vector3 ThirdVertex
        {
            get { return this.thirdVertex; }
            set { this.thirdVertex = value; }
        }

        /// <summary>
        /// Gets or sets the fourth solid <see cref="Vector3">vertex</see>.
        /// </summary>
        public Vector3 FourthVertex
        {
            get { return this.fourthVertex; }
            set { this.fourthVertex = value; }
        }

        /// <summary>
        /// Gets or sets the thickness of the solid.
        /// </summary>
        public double Thickness
        {
            get { return this.thickness; }
            set { this.thickness = value; }
        }

        #endregion

        #region overrides

        /// <summary>
        /// Creates a new Solid that is a copy of the current instance.
        /// </summary>
        /// <returns>A new Solid that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new Solid
            {
                //EntityObject properties
                Color = this.color,
                Layer = this.layer,
                LineType = this.lineType,
                Lineweight = this.lineweight,
                LineTypeScale = this.lineTypeScale,
                Normal = this.normal,
                XData = this.xData,
                //Solid properties
                FirstVertex = this.firstVertex,
                SecondVertex = this.secondVertex,
                ThirdVertex = this.thirdVertex,
                FourthVertex = this.fourthVertex,
                Thickness = this.thickness
            };
        }

        #endregion

    }
}