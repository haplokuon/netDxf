#region netDxf, Copyright(C) 2014 Daniel Carvajal, Licensed under LGPL.

//                        netDxf library
// Copyright (C) 2013 Daniel Carvajal (haplokuon@gmail.com)
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
    /// Represents a Text <see cref="EntityObject">entity</see>.
    /// </summary>
    public class Text :
        EntityObject
    {
        #region private fields

        private TextAlignment alignment;
        private Vector3 position;
        private double obliqueAngle;
        private TextStyle style;
        private string value;
        private double height;
        private double widthFactor;
        private double rotation;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>Text</c> class.
        /// </summary>
        public Text() 
            : this(string.Empty, Vector3.Zero, 1.0, TextStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Text</c> class.
        /// </summary>
        /// <param name="text">Text string.</param>
        /// <param name="position">Text <see cref="Vector2">position</see> in world coordinates.</param>
        /// <param name="height">Text height.</param>
        public Text(string text, Vector2 position, double height)
            : this(text, new Vector3(position.X, position.Y, 0.0), height, TextStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Text</c> class.
        /// </summary>
        /// <param name="text">Text string.</param>
        /// <param name="position">Text <see cref="Vector2">position</see> in world coordinates.</param>
        /// <param name="height">Text height.</param>
        public Text(string text, Vector3 position, double height)
            : this(text, position, height, TextStyle.Default)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <c>Text</c> class.
        /// </summary>
        /// <param name="text">Text string.</param>
        /// <param name="position">Text <see cref="Vector2">position</see> in world coordinates.</param>
        /// <param name="height">Text height.</param>
        /// <param name="style">Text <see cref="TextStyle">style</see>.</param>
        public Text(string text, Vector2 position, double height, TextStyle style)
            : this(text, new Vector3(position.X, position.Y, 0.0), height, style)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Text</c> class.
        /// </summary>
        /// <param name="text">Text string.</param>
        /// <param name="position">Text <see cref="Vector2">position</see> in world coordinates.</param>
        /// <param name="height">Text height.</param>
        /// <param name="style">Text <see cref="TextStyle">style</see>.</param>
        public Text(string text, Vector3 position, double height, TextStyle style)
            : base(EntityType.Text, DxfObjectCode.Text)
        {
            this.value = text;
            this.position = position;
            this.alignment = TextAlignment.BaselineLeft;
            this.normal = Vector3.UnitZ;
            if (style == null)
                throw new ArgumentNullException("style", "The Text style cannot be null.");
            this.style = style;
            if (height <= 0)
                throw (new ArgumentOutOfRangeException("height", value, "The Text Height can not be zero or less."));
            this.height = height;
            this.widthFactor = style.WidthFactor;
            this.obliqueAngle = style.ObliqueAngle;
            this.rotation = 0.0;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets Text <see cref="Vector2">position</see> in world coordinates.
        /// </summary>
        public Vector3 Position
        {
            get { return this.position; }
            set { this.position = value; }
        }

        /// <summary>
        /// Gets or sets the text rotation in degrees.
        /// </summary>
        public double Rotation
        {
            get { return this.rotation; }
            set { this.rotation = MathHelper.NormalizeAngle(value); }
        }

        /// <summary>
        /// Gets or sets the text height.
        /// </summary>
        public double Height
        {
            get { return this.height; }
            set
            {
                if (value <= 0)
                    throw (new ArgumentOutOfRangeException("value", value, "The Text Height must be greater than zero."));
                this.height = value;
            }
        }

        /// <summary>
        /// Gets or sets the width factor.
        /// </summary>
        /// <remarks>Valid values range from 0.01 to 100. Default: 1.0.</remarks>
        public double WidthFactor
        {
            get { return this.widthFactor; }
            set
            {
                if (value < 0.01 || value > 100.0)
                    throw (new ArgumentOutOfRangeException("value", value, "The Text WidthFactor valid values range from 0.01 to 100."));
                this.widthFactor = value;
            }
        }

        /// <summary>
        /// Gets or sets the font oblique angle.
        /// </summary>
        /// <remarks>Valid values range from -85 to 85. Default: 0.0.</remarks>
        public double ObliqueAngle
        {
            get { return this.obliqueAngle; }
            set
            {
                if (value < -85.0 || value > 85.0)
                    throw (new ArgumentOutOfRangeException("value", value, "The Text ObliqueAngle valid values range from -85 to 85."));
                this.obliqueAngle = value;
            }
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
        /// Gets or sets the <see cref="TextStyle">text style</see>.
        /// </summary>
        public TextStyle Style
        {
            get { return this.style; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value", "The Text Style cannot be null.");
                this.style = value;
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

        #region overrides

        /// <summary>
        /// Creates a new Text that is a copy of the current instance.
        /// </summary>
        /// <returns>A new Text that is a copy of this instance.</returns>
        public override object Clone()
        {
            Dictionary<string, XData> copyXData = null;
            if (this.xData != null)
            {
                copyXData = new Dictionary<string, XData>();
                foreach (KeyValuePair<string, XData> data in this.xData)
                {
                    copyXData.Add(data.Key, (XData)data.Value.Clone());
                }
            }

            return new Text
            {
                //EntityObject properties
                Layer = (Layer)this.layer.Clone(),
                LineType = (LineType)this.lineType.Clone(),
                Color = (AciColor)this.color.Clone(),
                Lineweight = (Lineweight)this.lineweight.Clone(),
                Transparency = (Transparency)this.transparency.Clone(),
                LineTypeScale = this.lineTypeScale,
                Normal = this.normal,
                XData = copyXData,
                //Text properties
                Position = this.position,
                Rotation = this.rotation,
                Height = this.height,
                WidthFactor = this.widthFactor,
                ObliqueAngle = this.obliqueAngle,
                Alignment = this.alignment,
                Style = this.style,
                Value = this.value
            };
        }

        #endregion

    }
}