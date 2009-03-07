#region netDxf, Copyright(C) 2009 Daniel Carvajal, Licensed under LGPL.

//                        netDxf library
// Copyright (C) 2009 Daniel Carvajal (haplokuon@gmail.com)
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
    /// Represents a nurbs curve vertex.
    /// </summary>
    public class NurbsVertex
    {
        #region private fields

        private Vector2f location;
        private float weight;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>NurbsVertex</c> class.
        /// </summary>
        public NurbsVertex()
        {
            this.location = Vector2f.Zero;
            this.weight = 1;
        }

        /// <summary>
        /// Initializes a new instance of the <c>NurbsVertex</c> class.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        public NurbsVertex(float x, float y)
        {
            this.location = new Vector2f(x, y);
            this.weight = 1;
        }

        /// <summary>
        /// Initializes a new instance of the <c>NurbsVertex</c> class.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="weight">Nurbs vertex weight.</param>
        public NurbsVertex(float x, float y, float weight)
        {
            this.location = new Vector2f(x, y);
            this.weight = weight;
        }

        /// <summary>
        /// Initializes a new instance of the <c>NurbsVertex</c> class.
        /// </summary>
        /// <param name="location">Nurbs vertex <see cref="Vector2f">location</see>.
        public NurbsVertex(Vector2f location)
        {
            this.location = location;
            this.weight = 1;
        }

        /// <summary>
        /// Initializes a new instance of the <c>NurbsVertex</c> class.
        /// </summary>
        /// <param name="location">Nurbs vertex <see cref="Vector2f">location</see>.
        /// <param name="weight">Nurbs vertex weight.</param>
        public NurbsVertex(Vector2f location, float weight)
        {
            this.location = location;
            this.weight = weight;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the vertex <see cref="netDxf.Vector2f">location</see>.
        /// </summary>
        public Vector2f Location
        {
            get { return this.location; }
            set { this.location = value; }
        }

        /// <summary>
        /// Gets or sets the vertex weight.
        /// </summary>
        public float Weight
        {
            get { return this.weight; }
            set { this.weight = value; }
        }

        #endregion
    }
}