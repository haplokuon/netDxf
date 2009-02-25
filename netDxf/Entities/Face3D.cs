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
    /// Defines which edges are hidden.
    /// </summary>
    [Flags]
    public enum EdgeFlags
    {
        /// <summary>
        /// All edges as visibles (default).
        /// </summary>
        Visibles = 0,
        /// <summary>
        /// First edge is invisible.
        /// </summary>
        First = 1,
        /// <summary>
        /// Second edge is invisible.
        /// </summary>
        Second = 2,
        /// <summary>
        /// Third edge is invisible.
        /// </summary>
        Third = 4,
        /// <summary>
        /// Fourth edge is invisible.
        /// </summary>
        Fourth = 8
    }

    /// <summary>
    /// Represents a 3DFace.
    /// </summary>
    public class Face3D :
        IEntityObject
    {
        #region private fields

        private const string DXF_NAME = DxfEntityCode.Face3D;
        private const EntityType TYPE = EntityType.Face3D;
        private Vector3 firstVertex;
        private Vector3 secondVertex;
        private Vector3 thirdVertex;
        private Vector3 fourthVertex;
        private EdgeFlags edgeFlags;
        private Layer layer;
        private AciColor color;
        private LineType lineType;
        private readonly List<XData> xData;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the Face3D class.
        /// </summary>
        public Face3D(Vector3 firstVertex, Vector3 secondVertex, Vector3 thirdVertex, Vector3 fourthVertex)
        {
            this.firstVertex = firstVertex;
            this.secondVertex = secondVertex;
            this.thirdVertex = thirdVertex;
            this.fourthVertex = fourthVertex;
            this.edgeFlags = EdgeFlags.Visibles;
            this.layer = Layer.Default;
            this.color = AciColor.Bylayer;
            this.lineType = LineType.ByLayer;
            this.xData = new List<XData>();
        }

        /// <summary>
        /// Initializes a new instance of the Face3D class.
        /// </summary>
        public Face3D()
        {
            this.firstVertex = Vector3.Zero;
            this.secondVertex = Vector3.Zero;
            this.thirdVertex = Vector3.Zero;
            this.fourthVertex = Vector3.Zero;
            this.edgeFlags = EdgeFlags.Visibles;
            this.layer = Layer.Default;
            this.color = AciColor.Bylayer;
            this.lineType = LineType.ByLayer;
            this.xData = new List<XData>();
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the first 3D face vertex.
        /// </summary>
        public Vector3 FirstVertex
        {
            get { return this.firstVertex; }
            set { this.firstVertex = value; }
        }

        /// <summary>
        /// Gets or sets the second 3D face vertex.
        /// </summary>
        public Vector3 SecondVertex
        {
            get { return this.secondVertex; }
            set { this.secondVertex = value; }
        }

        /// <summary>
        /// Gets or sets the third 3D face vertex.
        /// </summary>
        public Vector3 ThirdVertex
        {
            get { return this.thirdVertex; }
            set { this.thirdVertex = value; }
        }

        /// <summary>
        /// Gets or sets the fourth 3D face vertex.
        /// </summary>
        public Vector3 FourthVertex
        {
            get { return this.fourthVertex; }
            set { this.fourthVertex = value; }
        }

        /// <summary>
        /// Gets or set the 3d face edge visibility.
        /// </summary>
        public EdgeFlags EdgeFlags
        {
            get { return this.edgeFlags; }
            set { this.edgeFlags = value; }
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