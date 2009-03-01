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

    /// <summary>
    /// Represents a polyface mesh <see cref="netDxf.Entities.IEntityObject">entity.
    /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the <c>PolyfaceMesh</c> class.
        /// </summary>
        /// <param name="vertexes">Polyface mesh <see cref="PolyfaceMeshVertex">vertex</see> list.</param>
        /// <param name="faces">Polyface mesh <see cref="PolyfaceMeshFace">faces</see> list.</param>
        public PolyfaceMesh(List<PolyfaceMeshVertex> vertexes, List<PolyfaceMeshFace> faces)
        {
            this.flags = PolylineTypeFlags.PolyfaceMesh;
            this.vertexes = vertexes;
            this.faces = faces;
            this.layer = Layer.Default;
            this.color = AciColor.ByLayer;
            this.lineType = LineType.ByLayer;
            this.xData = new List<XData>();

        }

        /// <summary>
        /// Initializes a new instance of the <c>PolyfaceMesh</c> class.
        /// </summary>
        public PolyfaceMesh()
        {
            this.flags = PolylineTypeFlags.PolyfaceMesh;
            this.faces = new List<PolyfaceMeshFace>();
            this.layer = Layer.Default;
            this.color = AciColor.ByLayer;
            this.lineType = LineType.ByLayer;
            this.xData = new List<XData>();

        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the polyface mesh <see cref="netDxf.Entities.PolyfaceMeshVertex">vertexes</see>.
        /// </summary>
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
        /// Gets or sets the polyface mesh <see cref="netDxf.Entities.PolyfaceMeshFace">faces</see>.
        /// </summary>
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

        /// <summary>
        /// Gets the polyline type.
        /// </summary>
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
        /// Gets the entity <see cref="netDxf.Entities.EntityType">type</see>.
        /// </summary>
        public EntityType Type
        {
            get { return TYPE; }
        }

        /// <summary>
        /// Gets or sets the entity <see cref="netDxf.AciColor">color</see>.
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
        /// Gets or sets the entity <see cref="netDxf.Tables.Layer">layer</see>.
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
        /// Gets or sets the entity <see cref="netDxf.Tables.LineType">line type</see>.
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
        /// Gets or sets the entity <see cref="netDxf.XData">extende data</see>.
        /// </summary>
        public List<XData> XData
        {
            get { return this.xData; }
        }

        #endregion

        #region overrides

        /// <summary>
        /// Converts the value of this instance to its equivalent string representation.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            return TYPE.ToString();
        }

        #endregion
    }
}