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
using netDxf.Blocks;
using netDxf.Tables;

namespace netDxf.Entities
{
    public class Insert :
        IEntityObject
    {
        #region private fields

        private const string DXF_NAME = DxfEntityCode.Insert;
        private const EntityType TYPE = EntityType.Insert;
        private AciColor color;
        private Layer layer;
        private LineType lineType;
        private Block block;
        private Vector3 insertionPoint;
        private Vector3 scale;
        private float rotation;
        private Vector3 normal;
        private readonly List<Attribute> attributes;
        private readonly List<XData> xData;

        #endregion

        #region constructors

        public Insert(Block block, Vector3 insertionPoint)
        {
            if (block == null)
                throw new ArgumentNullException("block");

            this.block = block;
            this.insertionPoint = insertionPoint;
            this.scale = new Vector3(1.0f, 1.0f, 1.0f);
            this.rotation = 0.0f;
            this.normal = Vector3.UnitZ;
            this.layer = Layer.Default;
            this.color = AciColor.Bylayer;
            this.lineType = LineType.ByLayer;
            this.attributes = new List<Attribute>();
            foreach (AttributeDefinition attdef in block.Attributes.Values)
            {
                this.attributes.Add(new Attribute(attdef));
            }
            this.xData = new List<XData>();
        }

        public Insert(Block block)
        {
            if (block == null)
                throw new ArgumentNullException("block");

            this.block = block;
            this.insertionPoint = Vector3.Zero;
            this.scale = new Vector3(1.0f, 1.0f, 1.0f);
            this.rotation = 0.0f;
            this.normal = Vector3.UnitZ;
            this.layer = Layer.Default;
            this.color = AciColor.Bylayer;
            this.lineType = LineType.ByLayer;
            this.attributes = new List<Attribute>();
            foreach (AttributeDefinition attdef in block.Attributes.Values)
            {
                this.attributes.Add(new Attribute(attdef));
            }
            this.xData = new List<XData>();
        }

        #endregion

        #region public properties

        public List<Attribute> Attributes
        {
            get { return this.attributes; }
        }

        public Block Block
        {
            get { return this.block; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                this.block = value;
            }
        }

        public Vector3 InsertionPoint
        {
            get { return this.insertionPoint; }
            set { this.insertionPoint = value; }
        }

        public Vector3 Scale
        {
            get { return this.scale; }
            set { this.scale = value; }
        }

        public float Rotation
        {
            get { return this.rotation; }
            set { this.rotation = value; }
        }

        public Vector3 Normal
        {
            get { return this.normal; }
            set
            {
                if (Vector3.Zero == value)
                    throw new ArgumentNullException("value", "The normal can not be the zero vector");
                value.Normalize();
                this.normal = value;
            }
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