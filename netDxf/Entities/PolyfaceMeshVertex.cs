#region netDxf library licensed under the MIT License, Copyright © 2009-2021 Daniel Carvajal (haplokuon@gmail.com)
// 
//                        netDxf library
// Copyright © 2021 Daniel Carvajal (haplokuon@gmail.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software
// and associated documentation files (the “Software”), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;

namespace netDxf.Entities
{
    /// <summary>
    /// Represents a polyface mesh vertex. 
    /// </summary>
    /// <remarks>
    /// The PolyfaceMeshVertex can contain external data (XData) information, but it is not saved in the DXF.
    /// </remarks>
    public class PolyfaceMeshVertex :
        DxfObject, ICloneable
    {
        #region private fields

        private readonly VertexTypeFlags flags;
        private Vector3 position;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>PolylineVertex</c> class.
        /// </summary>
        public PolyfaceMeshVertex()
            : this(Vector3.Zero)
        {
        }

        /// <summary>
        /// Initializes a new instance of the PolylineVertex class.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="z">Z coordinate.</param>
        public PolyfaceMeshVertex(double x, double y, double z)
            : this(new Vector3(x, y, z))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>PolylineVertex</c> class.
        /// </summary>
        /// <param name="location">Polyface mesh vertex <see cref="Vector3">location</see>.</param>
        public PolyfaceMeshVertex(Vector3 location)
            : base(DxfObjectCode.Vertex)
        {
            this.flags = VertexTypeFlags.PolyfaceMeshVertex | VertexTypeFlags.Polygon3dMesh;
            this.position = location;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the polyface mesh vertex <see cref="netDxf.Vector3">position</see>.
        /// </summary>
        public Vector3 Position
        {
            get { return this.position; }
            set { this.position = value; }
        }

        /// <summary>
        /// Gets the vertex type.
        /// </summary>
        internal VertexTypeFlags Flags
        {
            get { return this.flags; }
        }

        #endregion

        #region overrides

        /// <summary>
        /// Converts the value of this instance to its equivalent string representation.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            return string.Format("{0}: {1}", "PolyfaceMeshVertex", this.position);
        }

        /// <summary>
        /// Creates a new PolyfaceMeshVertex that is a copy of the current instance.
        /// </summary>
        /// <returns>A new PolyfaceMeshVertex that is a copy of this instance.</returns>
        public object Clone()
        {
            return new PolyfaceMeshVertex(this.position);
        }

        #endregion
    }
}