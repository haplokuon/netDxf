#region netDxf library, Copyright (C) 2009-2018 Daniel Carvajal (haplokuon@gmail.com)

//                        netDxf library
// Copyright (C) 2009-2018 Daniel Carvajal (haplokuon@gmail.com)
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
using netDxf.Tables;

namespace netDxf.Entities
{
    /// <summary>
    /// Represents an attribute definition <see cref="EntityObject">entity</see>.
    /// </summary>
    /// <remarks>
    /// AutoCad allows to have duplicate tags in the attribute definitions list, but this library does not.
    /// To have duplicate tags is not recommended in any way, since there will be now way to know which is the definition associated to the insert attribute.
    /// </remarks>
    public class AttributeDefinition :
        EntityObject
    {
        #region delegates and events

        public delegate void TextStyleChangedEventHandler(AttributeDefinition sender, TableObjectChangedEventArgs<TextStyle> e);

        public event TextStyleChangedEventHandler TextStyleChange;

        protected virtual TextStyle OnTextStyleChangedEvent(TextStyle oldTextStyle, TextStyle newTextStyle)
        {
            TextStyleChangedEventHandler ae = this.TextStyleChange;
            if (ae != null)
            {
                TableObjectChangedEventArgs<TextStyle> eventArgs = new TableObjectChangedEventArgs<TextStyle>(oldTextStyle, newTextStyle);
                ae(this, eventArgs);
                return eventArgs.NewValue;
            }
            return newTextStyle;
        }

        #endregion

        #region private fields

        private readonly string tag;
        private string prompt;
        private object attValue;
        private TextStyle style;
        private Vector3 position;
        private AttributeFlags flags;
        private double height;
        private double widthFactor;
        private double obliqueAngle;
        private double rotation;
        private TextAlignment alignment;

        #endregion

        #region constructor

        /// <summary>
        /// Initializes a new instance of the <c>AttributeDefiniton</c> class.
        /// </summary>
        /// <param name="tag">Attribute identifier, the parameter <c>id</c> string cannot contain spaces.</param>
        public AttributeDefinition(string tag)
            : this(tag, TextStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>AttributeDefiniton</c> class.
        /// </summary>
        /// <param name="tag">Attribute identifier.</param>
        /// <param name="style">Attribute <see cref="TextStyle">text style</see>.</param>
        public AttributeDefinition(string tag, TextStyle style)
            : base(EntityType.AttributeDefinition, DxfObjectCode.AttributeDefinition)
        {
            if (string.IsNullOrEmpty(tag))
                throw new ArgumentNullException(nameof(tag));

            this.tag = tag;
            this.flags = AttributeFlags.Visible;
            this.prompt = string.Empty;
            this.attValue = null;
            this.position = Vector3.Zero;
            if (style == null)
                throw new ArgumentNullException(nameof(style));
            this.style = style;
            this.height = MathHelper.IsZero(style.Height) ? 1.0 : style.Height;
            this.widthFactor = style.WidthFactor;
            this.obliqueAngle = style.ObliqueAngle;
            this.rotation = 0.0;
            this.alignment = TextAlignment.BaselineLeft;
        }

        /// <summary>
        /// Initializes a new instance of the <c>AttributeDefiniton</c> class.
        /// </summary>
        /// <param name="tag">Attribute identifier, the parameter <c>id</c> string cannot contain spaces.</param>
        /// <param name="textHeight">Height of the attribute definition text.</param>
        /// <param name="style">Attribute <see cref="TextStyle">text style</see>.</param>
        public AttributeDefinition(string tag, double textHeight, TextStyle style)
            : base(EntityType.AttributeDefinition, DxfObjectCode.AttributeDefinition)
        {
            if (string.IsNullOrEmpty(tag))
                throw new ArgumentNullException(nameof(tag));

            if (tag.Contains(" "))
                throw new ArgumentException("The tag string cannot contain spaces.", nameof(tag));
            this.tag = tag;
            this.flags = AttributeFlags.Visible;
            this.prompt = string.Empty;
            this.attValue = null;
            this.position = Vector3.Zero;
            if (style == null)
                throw new ArgumentNullException(nameof(style));
            this.style = style;
            if (textHeight <= 0.0)
                throw new ArgumentOutOfRangeException(nameof(textHeight), this.attValue, "The attribute definition text height must be greater than zero.");
            this.height = textHeight;
            this.widthFactor = style.WidthFactor;
            this.obliqueAngle = style.ObliqueAngle;
            this.rotation = 0.0;
            this.alignment = TextAlignment.BaselineLeft;
        }

        #endregion

        #region public property

        /// <summary>
        /// Gets the attribute identifier.
        /// </summary>
        public string Tag
        {
            get { return this.tag; }
        }

        /// <summary>
        /// Gets or sets the attribute information text.
        /// </summary>
        /// <remarks>This is the text prompt shown to introduce the attribute value when new Insert entities are inserted into the drawing.</remarks>
        public string Prompt
        {
            get { return this.prompt; }
            set { this.prompt = value; }
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
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The height should be greater than zero.");
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
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The width factor should be greater than zero.");
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
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The oblique angle valid values range from -85 to 85.");
                this.obliqueAngle = value;
            }
        }

        /// <summary>
        /// Gets or sets the attribute default value.
        /// </summary>
        public object Value
        {
            get { return this.attValue; }
            set { this.attValue = value; }
        }

        /// <summary>
        /// Gets or sets  the attribute text style.
        /// </summary>
        /// <remarks>
        /// The <see cref="TextStyle">text style</see> defines the basic properties of the information text.
        /// </remarks>
        public TextStyle Style
        {
            get { return this.style; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                this.style = this.OnTextStyleChangedEvent(this.style, value);
            }
        }

        /// <summary>
        /// Gets or sets the attribute <see cref="Vector3">position</see> in object coordinates.
        /// </summary>
        public Vector3 Position
        {
            get { return this.position; }
            set { this.position = value; }
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
        /// Gets or sets the attribute text rotation in degrees.
        /// </summary>
        public double Rotation
        {
            get { return this.rotation; }
            set { this.rotation = MathHelper.NormalizeAngle(value); }
        }

        /// <summary>
        /// Gets or sets the text alignment.
        /// </summary>
        public TextAlignment Alignment
        {
            get { return this.alignment; }
            set { this.alignment = value; }
        }

        #endregion

        #region overrides

        /// <summary>
        /// Creates a new AttributeDefinition that is a copy of the current instance.
        /// </summary>
        /// <returns>A new AttributeDefinition that is a copy of this instance.</returns>
        public override object Clone()
        {
            AttributeDefinition entity = new AttributeDefinition(this.tag)
            {
                //EntityObject properties
                Layer = (Layer) this.Layer.Clone(),
                Linetype = (Linetype) this.Linetype.Clone(),
                Color = (AciColor) this.Color.Clone(),
                Lineweight = this.Lineweight,
                Transparency = (Transparency) this.Transparency.Clone(),
                LinetypeScale = this.LinetypeScale,
                Normal = this.Normal,
                IsVisible = this.IsVisible,
                //Attribute definition properties
                Prompt = this.prompt,
                Value = this.attValue,
                Height = this.height,
                WidthFactor = this.widthFactor,
                ObliqueAngle = this.obliqueAngle,
                Style = this.style,
                Position = this.position,
                Flags = this.flags,
                Rotation = this.rotation,
                Alignment = this.alignment
            };

            foreach (XData data in this.XData.Values)
                entity.XData.Add((XData) data.Clone());

            return entity;
        }

        #endregion
    }
}