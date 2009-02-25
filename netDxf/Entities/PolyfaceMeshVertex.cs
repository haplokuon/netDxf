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

using System;
using System.Collections.Generic;
using netDxf.Tables;

namespace netDxf.Entities
{
    public class PolyfaceMeshVertex :
        IVertex
    {
        #region private fields

        protected const string DXF_NAME = DxfEntityCode.Vertex;
        protected const EntityType TYPE = EntityType.PolyfaceMeshVertex;
        protected VertexTypeFlags flags;
        protected Vector3 location;
        protected AciColor color;
        protected Layer layer;
        protected LineType lineType;
        protected readonly List<XData> xData;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the PolylineVertex class.
        /// </summary>
        public PolyfaceMeshVertex()
        {
            this.flags = VertexTypeFlags.PolyfaceMeshVertex | VertexTypeFlags.Polygon3dMesh;
            this.location = Vector3.Zero;
            this.layer = Layer.Default;
            this.color = AciColor.Bylayer;
            this.lineType = LineType.ByLayer;
            this.xData = new List<XData>();

        }

        /// <summary>
        /// Initializes a new instance of the PolylineVertex class.
        /// </summary>
        /// <param name="location">Polyline vertex coordinates.</param>
        public PolyfaceMeshVertex(Vector3 location)
        {
            this.flags = VertexTypeFlags.PolyfaceMeshVertex | VertexTypeFlags.Polygon3dMesh;
            this.location = location;
            this.layer = Layer.Default;
            this.color = AciColor.Bylayer;
            this.lineType = LineType.ByLayer;
            this.xData = new List<XData>();

        }

        /// <summary>
        /// Initializes a new instance of the PolylineVertex class.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="z">Z coordinate.</param>
        public PolyfaceMeshVertex(float x, float y, float z)
        {
            this.flags = VertexTypeFlags.PolyfaceMeshVertex | VertexTypeFlags.Polygon3dMesh;
            this.location = new Vector3(x, y, z);
            this.layer = Layer.Default;
            this.color = AciColor.Bylayer;
            this.lineType = LineType.ByLayer;
            this.xData = new List<XData>();

        }

        #endregion
        
        #region public properties

        /// <summary>
        /// Gets or sets the polyline vertex coordinates.
        /// </summary>
        public Vector3 Location
        {
            get { return this.location; }
            set { this.location = value; }
        }

        #endregion

        #region IVertex Members

        public VertexTypeFlags Flags
        {
            get { return this.flags; }
        }

        #endregion

        #region IEntityObject Members

        /// <summary>
        /// Gets the dxf code that represents the entity.
        /// </summary>
        public string DxfName
        {
            get { return DXF_NAME; }
        }

        /// <summary>
        /// Gets the entity type.
        /// </summary>
        public EntityType Type
        {
            get { return TYPE; }
        }

        /// <summary>
        /// Gets or sets the entity color.
        /// </summary>
        public AciColor Color
        {
            get { return this.color; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                this.color = value;
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
                if (value == null)
                    throw new ArgumentNullException("value");
                this.layer = value;
            }
        }

        /// <summary>
        /// Gets or sets the entity line type.
        /// </summary>
        public LineType LineType
        {
            get { return this.lineType; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                this.lineType = value;
            }
        }

        /// <summary>
        /// Gets or sets the entity extended data.
        /// </summary>
        public List<XData> XData
        {
            get { return this.xData; }
        }

        #endregion

        #region overrides

        public override string ToString()
        {
            return String.Format("{0} {1}", TYPE, this.location);
        }

        #endregion
    }
}