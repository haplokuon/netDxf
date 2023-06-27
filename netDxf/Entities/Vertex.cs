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
    /// Represents a DXF Vertex.
    /// </summary>
    /// <remarks>
    /// Under the VERTEX data the DXF stores information about the vertexes of smoothed Polylines2D (non-smoothed Polylines2D are stored as LWPOLYLINE,
    /// Polylines3D (smoothed and non-smoothed), and PolyfaceMeshes.<br />
    /// For internal use only.
    /// </remarks>
    internal class Vertex :
        DxfObject
    {
        #region private fields

        private VertexTypeFlags flags;
        private Vector3 position;
        private short[] vertexIndexes;
        private double startWidth;
        private double endWidth;
        private double bulge;
        private AciColor color;
        private Layer layer;
        private Linetype linetype;
        private string subclassMarker;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>Vertex</c> class.
        /// </summary>
        public Vertex()
            : base(DxfObjectCode.Vertex)
        {
            this.flags = VertexTypeFlags.Polyline2DVertex;
            this.position = Vector3.Zero;
            this.layer = null;
            this.color = null;
            this.linetype = null;
            this.bulge = 0.0;
            this.startWidth = 0.0;
            this.endWidth = 0.0;
            this.vertexIndexes = null;
            this.subclassMarker = netDxf.SubclassMarker.Polyline2DVertex;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the polyline vertex <see cref="Vector3">location</see>.
        /// </summary>
        public Vector3 Position
        {
            get { return this.position; }
            set { this.position = value; }
        }

        /// <summary>
        /// Gets or sets the indexes, only applicable when the vertex represents a face of a polyface mesh.
        /// </summary>
        public short[] VertexIndexes
        {
            get { return this.vertexIndexes; }
            set { this.vertexIndexes = value; }
        }

        /// <summary>
        /// Gets or sets the light weight polyline start segment width.
        /// </summary>
        public double StartWidth
        {
            get { return this.startWidth; }
            set { this.startWidth = value; }
        }

        /// <summary>
        /// Gets or sets the light weight polyline end segment width.
        /// </summary>
        public double EndWidth
        {
            get { return this.endWidth; }
            set { this.endWidth = value; }
        }

        /// <summary>
        /// Gets or set the light weight polyline bulge.Accepted values range from 0 to 1.
        /// </summary>
        public double Bulge
        {
            get { return this.bulge; }
            set { this.bulge = value; }
        }

        /// <summary>
        /// Gets or sets the vertex type.
        /// </summary>
        public VertexTypeFlags Flags
        {
            get { return this.flags; }
            set { this.flags = value; }
        }

        /// <summary>
        /// Gets or sets the entity color.
        /// </summary>
        public AciColor Color
        {
            get { return this.color; }
            set { this.color = value; }
        }

        /// <summary>
        /// Gets or sets the entity layer.
        /// </summary>
        public Layer Layer
        {
            get { return this.layer; }
            set { this.layer = value; }
        }

        /// <summary>
        /// Gets or sets the entity line type.
        /// </summary>
        public Linetype Linetype
        {
            get { return this.linetype; }
            set { this.linetype = value; }
        }

        /// <summary>
        /// Gets or sets the subclass marker
        /// </summary>
        public string SubclassMarker
        {
            get { return this.subclassMarker; }
            set { this.subclassMarker = value; }
        }

        #endregion
    }
}