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

using System.Collections.Generic;
using netDxf.Tables;

namespace netDxf.Entities
{
    /// <summary>
    /// Represents a DXF Polyline.
    /// </summary>
    /// <remarks>
    /// Under the POLYLINE data the DXF stores information about smoothed Polylines2D (non-smoothed Polylines2D are stored as LWPOLYLINE,
    /// Polylines3D (smoothed and non-smoothed), and PolyfaceMeshes.<br />
    /// For internal use only.
    /// </remarks>
    internal class Polyline :
        DxfObject
    {
        #region private fields

        private string subclassMarker;
        private Layer layer;
        private double thickness;
        private double elevation;
        private Vector3 normal;
        private AciColor color;
        private EndSequence endSequence;
        private List<Vertex> vertexes;
        private PolylineTypeFlags flags;
        private PolylineSmoothType smoothType;

        // polygon mesh
        private short m;
        private short n;
        private short densityM;
        private short densityN;

        #endregion

        #region constructor

        /// <summary>
        /// Initializes a new instance of the <c>Polyline</c> class.
        /// </summary>
        public Polyline()
            : base(DxfObjectCode.Polyline)
        {
            this.subclassMarker = netDxf.SubclassMarker.Polyline;
        }

        #endregion

        #region properties

        /// <summary>
        /// Gets or sets the polyline subclass marker.
        /// </summary>
        public string SubclassMarker
        {
            get { return this.subclassMarker; }
            set { this.subclassMarker = value; }
        }

        /// <summary>
        /// Gets or sets the polyline layer.
        /// </summary>
        public Layer Layer
        {
            get { return this.layer; }
            set { this.layer = value; }
        }

        /// <summary>
        /// Gets or sets the polyline thickness.
        /// </summary>
        public double Thickness
        {
            get { return this.thickness; }
            set { this.thickness = value; }
        }

        /// <summary>
        /// Gets or sets the polyline elevation.
        /// </summary>
        public double Elevation
        {
            get { return this.elevation; }
            set { this.elevation = value; }
        }

        /// <summary>
        /// Gets or sets the polyline normal.
        /// </summary>
        public Vector3 Normal
        {
            get { return this.normal; }
            set { this.normal = value; }
        }

        /// <summary>
        /// Gets or sets the polyline color.
        /// </summary>
        public AciColor Color
        {
            get { return this.color; }
            set { this.color = value; }
        }

        /// <summary>
        /// Gets or sets the polyline EndSequence object.
        /// </summary>
        public EndSequence EndSequence
        {
            get { return this.endSequence; }
            set { this.endSequence = value; }
        }

        /// <summary>
        /// Gets or sets the polyline vertexes list.
        /// </summary>
        public List<Vertex> Vertexes
        {
            get { return this.vertexes; }
            set { this.vertexes = value; }
        }

        /// <summary>
        /// Gets or sets the polyline flags.
        /// </summary>
        public PolylineTypeFlags Flags
        {
            get { return this.flags; }
            set { this.flags = value; }
        }

        /// <summary>
        /// Gets or sets the polyline smooth type.
        /// </summary>
        public PolylineSmoothType SmoothType
        {
            get { return this.smoothType; }
            set { this.smoothType = value; }
        }

        /// <summary>
        /// Polygon mesh M vertex count.
        /// </summary>
        public short M
        {
            get { return this.m; }
            set { this.m = value; }
        }

        /// <summary>
        /// Polygon mesh N vertex count.
        /// </summary>
        public short N
        {
            get { return this.n; }
            set { this.n = value; }
        }

        /// <summary>
        /// Polygon mesh smooth surface M density.
        /// </summary>
        public short DensityM
        {
            get { return this.densityM; }
            set { this.densityM = value; }
        }

        /// <summary>
        /// Polygon mesh smooth surface N density.
        /// </summary>
        public short DensityN
        {
            get { return this.densityN; }
            set { this.densityN = value; }
        }

        #endregion
    }
}
