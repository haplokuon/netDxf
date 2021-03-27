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
using netDxf.Tables;

namespace netDxf.Entities
{
    /// <summary>
    /// Represents a DXF Vertex.
    /// </summary>
    /// <remarks>
    /// The Vertex class holds all the information read from the DXF file even if its needed or not. For internal use only.
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

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>Vertex</c> class.
        /// </summary>
        public Vertex()
            : this(Vector3.Zero)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Vertex</c> class.
        /// </summary>
        /// <param name="position">Vertex <see cref="Vector2">location</see>.</param>
        public Vertex(Vector2 position)
            : this(new Vector3(position.X, position.Y, 0.0))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Vertex</c> class.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="z">Z coordinate.</param>
        public Vertex(double x, double y, double z)
            : this(new Vector3(x, y, z))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Vertex</c> class.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        public Vertex(double x, double y)
            : this(new Vector3(x, y, 0.0))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Vertex</c> class.
        /// </summary>
        /// <param name="position">Vertex <see cref="Vector3">location</see>.</param>
        public Vertex(Vector3 position)
            : base(DxfObjectCode.Vertex)
        {
            this.flags = VertexTypeFlags.PolylineVertex;
            this.position = position;
            this.layer = Layer.Default;
            this.color = AciColor.ByLayer;
            this.linetype = Linetype.ByLayer;
            this.bulge = 0.0;
            this.startWidth = 0.0;
            this.endWidth = 0.0;
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
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The Vertex width must be equals or greater than zero.");
                this.startWidth = value;
            }
        }

        /// <summary>
        /// Gets or sets the light weight polyline end segment width.
        /// </summary>
        public double EndWidth
        {
            get { return this.endWidth; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The Vertex width must be equals or greater than zero.");
                this.endWidth = value;
            }
        }

        /// <summary>
        /// Gets or set the light weight polyline bulge.Accepted values range from 0 to 1.
        /// </summary>
        /// <remarks>
        /// The bulge is the tangent of one fourth the included angle for an arc segment, 
        /// made negative if the arc goes clockwise from the start point to the endpoint. 
        /// A bulge of 0 indicates a straight segment, and a bulge of 1 is a semicircle.
        /// </remarks>
        public double Bulge
        {
            get { return this.bulge; }
            set
            {
                if (this.bulge < 0.0 || this.bulge > 1.0f)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The bulge must be a value between zero and one");
                this.bulge = value;
            }
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
            set
            {
                this.color = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        /// <summary>
        /// Gets or sets the entity layer.
        /// </summary>
        public Layer Layer
        {
            get { return this.layer; }
            set
            {
                this.layer = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        /// <summary>
        /// Gets or sets the entity line type.
        /// </summary>
        public Linetype Linetype
        {
            get { return this.linetype; }
            set
            {
                this.linetype = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        #endregion

        #region overrides

        public override string ToString()
        {
            return this.CodeName;
        }

        #endregion
    }
}