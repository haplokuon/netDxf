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
    /// Represents a attribute definition <see cref="netDxf.Entities.IEntityObject">entity</see>.
    /// </summary>
    public class AttributeDefinition :
        DxfObject,
        IEntityObject
    {
        #region private fields

        private const EntityType TYPE = EntityType.AttributeDefinition;
        private readonly string id;
        private string text;
        private object value;
        private TextStyle style;
        private AciColor color;
        private Vector3d basePoint;
        private Layer layer;
        private LineType lineType;
        private AttributeFlags flags;
        private TextAlignment alignment;
        private double height;
        private double widthFactor;
        private double rotation;
        private Vector3d normal;
        private Dictionary<ApplicationRegistry, XData> xData;

        #endregion

        #region constructor

        /// <summary>
        /// Intitializes a new instance of the <c>AttributeDefiniton</c> class.
        /// </summary>
        /// <param name="id">Attribute identifier, the parameter <c>id</c> string cannot contain spaces.</param>
        public AttributeDefinition(string id)
            : base(DxfObjectCode.AttributeDefinition)
        {
            if (id.Contains(" "))
                throw new ArgumentException("The id string cannot contain spaces", "id");
            this.id = id;
            this.flags = AttributeFlags.Visible;
            this.text = string.Empty;
            this.value = null;
            this.basePoint = Vector3d.Zero;
            this.layer = Layer.Default;
            this.color = AciColor.ByLayer;
            this.lineType = LineType.ByLayer;
            this.style = TextStyle.Default;
            this.alignment = TextAlignment.BaselineLeft;
            this.height = this.style.Height == 0 ? 1.0f : this.style.Height;
            this.widthFactor = this.style.WidthFactor;
            this.rotation = 0.0f;
            this.normal = Vector3d.UnitZ;
        }

        /// <summary>
        /// Intitializes a new instance of the <c>AttributeDefiniton</c> class.
        /// </summary>
        /// <param name="id">Attribute identifier, the parameter <c>id</c> string cannot contain spaces.</param>
        /// <param name="style">Attribute <see cref="netDxf.Tables.TextStyle">text style.</see></param>
        public AttributeDefinition(string id, TextStyle style)
            : base(DxfObjectCode.AttributeDefinition)
        {
            if (id.Contains(" "))
                throw new ArgumentException("The id string cannot contain spaces", "id");
            this.id = id;
            this.flags = AttributeFlags.Visible;
            this.text = string.Empty;
            this.value = null;
            this.basePoint = Vector3d.Zero;
            this.layer = Layer.Default;
            this.color = AciColor.ByLayer;
            this.lineType = LineType.ByLayer;
            this.style = style;
            this.alignment = TextAlignment.BaselineLeft;
            this.height = style.Height == 0 ? 1.0f : style.Height;
            this.widthFactor = style.WidthFactor;
            this.rotation = 0.0f;
            this.normal = Vector3d.UnitZ;
        }

        #endregion

        #region public property

        /// <summary>
        /// Gets the attribute identifier.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the <see cref="netDxf.TextAlignment">text alignment.</see>
        /// </summary>
        public TextAlignment Alignment
        {
            get { return this.alignment; }
            set { this.alignment = value; }
        }

        /// <summary>
        /// Gets or sets the attribute text height.
        /// </summary>
        public double Height
        {
            get { return this.height; }
            set
            {
                if (value <= 0)
                    throw (new ArgumentOutOfRangeException("value", value, "The height should be greater than zero."));
                this.height = value;
            }
        }

        /// <summary>
        /// Gets or sets the attribute text width factor.
        /// </summary>
        public double WidthFactor
        {
            get { return this.widthFactor; }
            set
            {
                if (value <= 0)
                    throw (new ArgumentOutOfRangeException("value", value, "The width factor should be greater than zero."));
                this.widthFactor = value;
            }
        }

        /// <summary>
        /// Gets or sets the attribute text rotation in degrees.
        /// </summary>
        public double Rotation
        {
            get { return this.rotation; }
            set { this.rotation = value; }
        }

        /// <summary>
        /// Gets or sets the attribute default value.
        /// </summary>
        public object Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        /// <summary>
        /// Gets or sets  the attribute text style.
        /// </summary>
        /// <remarks>
        /// The <see cref="netDxf.Tables.TextStyle">text style</see> defines the basic properties of the information text.
        /// </remarks>
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
        /// Gets or sets the attribute <see cref="netDxf.Vector3d">insertion point</see>.
        /// </summary>
        public Vector3d BasePoint
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

        /// <summary>
        /// Gets or sets the attribute <see cref="netDxf.Vector3d">normal</see>.
        /// </summary>
        public Vector3d Normal
        {
            get { return this.normal; }
            set
            {
                if (Vector3d.Zero == value)
                    throw new ArgumentNullException("value", "The normal can not be the zero vector");
                value.Normalize();
                this.normal = value;
            }
        }

        #endregion

        #region IEntityObject Members

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
        public Dictionary<ApplicationRegistry, XData> XData
        {
            get { return this.xData; }
            set
            {
                throw new ArgumentException("Extended data not avaliable for attribute definitions", "value");
            }
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