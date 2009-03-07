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
    /// Represents a Text <see cref="IEntityObject">entity</see>.
    /// </summary>
    public class Text :
        DxfObject,
        IEntityObject
    {
        #region private fields

        private const EntityType TYPE = EntityType.Text;
        private TextAlignment alignment;
        private Vector3f basePoint;
        private AciColor color;
        private Layer layer;
        private LineType lineType;
        private Vector3f normal;
        private float obliqueAngle;
        private TextStyle style;
        private string value;
        private float height;
        private float widthFactor;
        private float rotation;
        private Dictionary<ApplicationRegistry, XData> xData;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>Text</c> class.
        /// </summary>
        public Text() 
            : base(DxfObjectCode.Text)
        {
            this.value = string.Empty;
            this.basePoint = Vector3f.Zero;
            this.alignment = TextAlignment.BaselineLeft;
            this.layer = Layer.Default;
            this.color = AciColor.ByLayer;
            this.lineType = LineType.ByLayer;
            this.normal = Vector3f.UnitZ;
            this.style = TextStyle.Default;
            this.rotation = 0.0f;
            this.height = 0.0f;
            this.widthFactor = 1.0f;
            this.obliqueAngle = 0.0f;
        }

        /// <summary>
        /// Initializes a new instance of the <c>Text</c> class.
        /// </summary>
        /// <param name="text">Text string.</param>
        /// <param name="basePoint">Text base <see cref="Vector3f">point</see>.</param>
        /// <param name="height">Text height.</param>
        public Text(string text, Vector3f basePoint, float height)
            : base(DxfObjectCode.Text)
        {
            this.value = text;
            this.basePoint = basePoint;
            this.alignment = TextAlignment.BaselineLeft;
            this.layer = Layer.Default;
            this.color = AciColor.ByLayer;
            this.lineType = LineType.ByLayer;
            this.normal = Vector3f.UnitZ;
            this.style = TextStyle.Default;
            this.rotation = 0.0f;
            this.height = height;
            this.widthFactor = 1.0f;
            this.obliqueAngle = 0.0f;
        }

        /// <summary>
        /// Initializes a new instance of the <c>Text</c> class.
        /// </summary>
        /// <param name="text">Text string.</param>
        /// <param name="basePoint">Text base <see cref="Vector3f">point</see>.</param>
        /// <param name="height">Text height.</param>
        /// <param name="style">Text <see cref="TextStyle">style</see>.</param>
        public Text(string text, Vector3f basePoint, float height, TextStyle style)
            : base(DxfObjectCode.Text)
        {
            this.value = text;
            this.basePoint = basePoint;
            this.alignment = TextAlignment.BaselineLeft;
            this.layer = Layer.Default;
            this.color = AciColor.ByLayer;
            this.lineType = LineType.ByLayer;
            this.normal = Vector3f.UnitZ;
            this.style = style;
            this.height = height;
            this.widthFactor = style.WidthFactor;
            this.obliqueAngle = style.ObliqueAngle;
            this.rotation = 0.0f;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the text rotation.
        /// </summary>
        public float Rotation
        {
            get { return this.rotation; }
            set { this.rotation = value; }
        }

        /// <summary>
        /// Gets or sets the text height.
        /// </summary>
        public float Height
        {
            get { return this.height; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value",value.ToString());
                this.height = value;
            }
        }

        /// <summary>
        /// Gets or sets the width factor.
        /// </summary>
        public float WidthFactor
        {
            get { return this.widthFactor; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("value", value.ToString());
                this.widthFactor = value;
            }
        }

        /// <summary>
        /// Gets or sets the font oblique angle.
        /// </summary>
        public float ObliqueAngle
        {
            get { return this.obliqueAngle; }
            set { this.obliqueAngle = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="netDxf.Tables.TextStyle">text style</see>.
        /// </summary>
        public TextStyle Style
        {
            get { return this.style; }
            set { this.style = value; }
        }

        /// <summary>
        /// Gets or sets the text base <see cref="netDxf.Vector3f">point</see>.
        /// </summary>
        public Vector3f BasePoint
        {
            get { return this.basePoint; }
            set { this.basePoint = value; }
        }

        /// <summary>
        /// Gets or sets the text alignment.
        /// </summary>
        public TextAlignment Alignment
        {
            get { return this.alignment; }
            set { this.alignment = value; }
        }

        /// <summary>
        /// Gets or sets the text <see cref="netDxf.Vector3f">normal</see>.
        /// </summary>
        public Vector3f Normal
        {
            get { return this.normal; }
            set
            {
                value.Normalize();
                this.normal = value;
            }
        }

        /// <summary>
        /// Gets or sets the text string.
        /// </summary>
        public string Value
        {
            get { return this.value; }
            set { this.value = value; }
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
            set { this.xData = value; }
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