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
using System.Collections.Generic;
using System.Text;
using netDxf.Blocks;
using netDxf.Tables;

namespace netDxf.Entities
{
    /// <summary>
    /// Represents a linear or rotated dimension <see cref="IEntityObject">entity</see>.
    /// </summary>
    public class LinearDimension:
        Dimension
    {

        #region private fields

        private Vector3 firstPoint;
        private Vector3 secondPoint;

        #endregion

        #region constructors
        /// <summary>
        /// Initializes a new instance of the <c>LinearDimension</c> class.
        /// </summary>
        /// <param name="firstPoint">First reference <see cref="Vector3">point</see> of the dimension.</param>
        /// <param name="secondPoint">Second reference <see cref="Vector3">point</see> of the dimension.</param>
        /// <param name="offset">Distance between the reference line and the dimension line.</param>
        /// <remarks>The reference points define the distance to be measure.</remarks>
        public LinearDimension(Vector3 firstPoint, Vector3 secondPoint, double offset)
            : base(DimensionType.Linear)
        {
            this.firstPoint = firstPoint;
            this.secondPoint = secondPoint;
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
        /// Actual measurement.
        /// </summary>
        public override double Value
        {
            get { return Vector3.Distance(this.firstPoint, this.secondPoint); }
        }

        #endregion

        /// <summary>
        /// Gets the the block that contains the entities that make up the dimension picture.
        /// </summary>
        /// <param name="name">Name to be asigned to the generated block.</param>
        /// <returns>The block that represents the actual dimension.</returns>
        internal override Block BuildBlock(string name)
        {
            Layer defPoints = new Layer("Defpoints");
            Point firstRef = new Point(firstPoint) { Layer = defPoints };
            Point secondRef = new Point(secondPoint) { Layer = defPoints };
            Point thirdRef = new Point(definitionPoint) { Layer = defPoints };

            Vector3 dir = this.secondPoint - this.firstPoint;
            dir.Normalize();
            Vector3 perp = Vector3.CrossProduct(this.normal, dir);

            return new Block("*D1");
        }

    }

}
