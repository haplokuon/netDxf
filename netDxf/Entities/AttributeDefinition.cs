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
    ///<summary>Attribute flags.</summary>
    [Flags]
    public enum AttributeFlags
    {
        /// <summary>
        /// Attribute is visible 
        /// </summary>
        Visible = 0,
        /// <summary>
        /// Attribute is invisible (does not appear)
        /// </summary>
        Hidden = 1,
        /// <summary>
        /// This is a constant attribute
        /// </summary>
        Constant = 2,
        /// <summary>
        /// Verification is required on input of this attribute
        /// </summary>
        Verify = 4,
        /// <summary>
        /// Attribute is preset (no prompt during insertion)
        /// </summary>
        Predefined = 8
    }

    /// <summary>
    /// Represents a attribute entity.
    /// </summary>
    public class AttributeDefinition :
        IEntityObject
    {
        #region private fields

        private const string DXF_NAME = DxfEntityCode.AttributeDefinition;
        private const EntityType TYPE = EntityType.AttributeDefinition;
        private readonly string id;
        private string text;
        private object value;
        private TextStyle style;
        private AciColor color;
        private Vector3 basePoint;
        private Layer layer;
        private LineType lineType;
        private AttributeFlags flags;
        private TextAlignment alignment;
        private float height;
        private float widthFactor;
        private float rotation;
        private Vector3 normal;
        private readonly List<XData> xData;

        #endregion

        #region constructor

        /// <summary>
        /// Intitializes a new attribute entity.
        /// </summary>
        /// <param name="id">Attribute identifier.</param>
        /// <remarks>The id cannot contains spaces.</remarks>
        public AttributeDefinition(string id)
        {
            this.flags = AttributeFlags.Visible;
            this.id = id;
            this.text = string.Empty;
            this.value = 0;
            this.basePoint = Vector3.Zero;
            this.layer = Layer.Default;
            this.color = AciColor.Bylayer;
            this.lineType = LineType.ByLayer;
            this.style = TextStyle.Default;
            this.alignment = TextAlignment.BaselineLeft;
            this.height = this.style.Height;
            this.widthFactor = this.style.WidthFactor;
            this.rotation = 0.0f;
            this.normal = Vector3.UnitZ;
            this.xData = new List<XData>();
        }

        #endregion

        #region public property

        /// <summary>
        /// Gets the attribute identifier.
        /// </summary>
        /// <remarks>The id cannot contains spaces.</remarks>
        public string Id
        {
            get { return this.id; }
        }

        /// <summary>
        /// Gets or sets the attribute information text.
        /// </summary>
        public string Text
        {
            get { return this.text; }
            set { this.text = value; }
        }

        public TextAlignment Alignment
        {
            get { return this.alignment; }
            set { this.alignment = value; }
        }

        public float Height
        {
            get { return this.height; }
            set
            {
                if (value <= 0)
                    throw (new ArgumentOutOfRangeException("value", value, "The height should be greater than zero."));
                this.height = value;
            }
        }

        public float WidthFactor
        {
            get { return this.widthFactor; }
            set
            {
                if (value <= 0)
                    throw (new ArgumentOutOfRangeException("value", value, "The width factor should be greater than zero."));
                this.widthFactor = value;
            }
        }

        public float Rotation
        {
            get { return this.rotation; }
            set { this.rotation = value; }
        }

        /// <summary>
        /// Gets or sets the attribute layer.
        /// </summary>
        public object Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        /// <summary>
        /// Gets or sets  the attribute text style.
        /// </summary>
        public TextStyle Style
        {
            get { return this.style; }
            set
            {
                if (value == null)
                    throw new NullReferenceException("value");
                this.style = value;
            }
        }

        /// <summary>
        /// Gets or sets the attribute insertion point.
        /// </summary>
        public Vector3 BasePoint
        {
            get { return this.basePoint; }
            set { this.basePoint = value; }
        }

        /// <summary>
        /// Gets or sets the attribute flags.
        /// </summary>
        public AttributeFlags Flags
        {
            get { return this.flags; }
            set { this.flags = value; }
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