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
    public class PolyfaceMesh :
        IPolyline
    {
        #region private fields

        private const string DXF_NAME = DxfEntityCode.Polyline;
        protected const EntityType TYPE = EntityType.PolyfaceMesh;
        private List<PolyfaceMeshFace> faces;
        private List<PolyfaceMeshVertex> vertexes;
        protected PolylineTypeFlags flags;
        protected Layer layer;
        protected AciColor color;
        protected LineType lineType;
        protected readonly List<XData> xData;
        #endregion

        #region constructurs

        public PolyfaceMesh(List<PolyfaceMeshVertex> vertexes, List<PolyfaceMeshFace> faces)
        {
            this.flags = PolylineTypeFlags.PolyfaceMesh;
            this.vertexes = vertexes;
            this.faces = faces;
            this.layer = Layer.Default;
            this.color = AciColor.Bylayer;
            this.lineType = LineType.ByLayer;
            this.xData = new List<XData>();

        }

        public PolyfaceMesh()
        {
            this.flags = PolylineTypeFlags.PolyfaceMesh;
            this.faces = new List<PolyfaceMeshFace>();
            this.layer = Layer.Default;
            this.color = AciColor.Bylayer;
            this.lineType = LineType.ByLayer;
            this.xData = new List<XData>();

        }

        #endregion

        #region public properties

        public List<PolyfaceMeshVertex> Vertexes
        {
            get { return this.vertexes; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                this.vertexes = value;
            }
        }

        /// <summary>
        /// List of faces.
        /// </summary>
        /// <remarks>The list of faces </remarks>
        public List<PolyfaceMeshFace> Faces
        {
            get { return this.faces; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                this.faces = value;
            }
        }

        #endregion

        #region IPolyline Members

        public PolylineTypeFlags Flags
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
            return TYPE.ToString();
        }

        #endregion
    }
}