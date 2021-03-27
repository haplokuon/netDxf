#region netDxf library licensed under the MIT License, Copyright © 2009-2021 Daniel Carvajal (haplokuon@gmail.com)
// 
//                        netDxf library
// Copyright © 2021 Daniel Carvajal (haplokuon@gmail.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software
// and associated documentation files (the “Software”), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;

namespace netDxf.Tables
{
    /// <summary>
    /// Represents a text linetype segment.
    /// </summary>
    public class LinetypeTextSegment :
        LinetypeSegment
    {
        #region delegates and events

        public delegate void TextStyleChangedEventHandler(LinetypeTextSegment sender, TableObjectChangedEventArgs<TextStyle> e);
        public event TextStyleChangedEventHandler TextStyleChanged;
        protected virtual TextStyle OnTextStyleChangedEvent(TextStyle oldTextStyle, TextStyle newTextStyle)
        {
            TextStyleChangedEventHandler ae = this.TextStyleChanged;
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

        private string text;
        private TextStyle style;
        private Vector2 offset;
        private LinetypeSegmentRotationType rotationType;
        private double rotation;
        private double scale;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>LinetypeShapeSegment</c> class.
        /// </summary>
        public LinetypeTextSegment() : this(string.Empty, TextStyle.Default, 0.0, Vector2.Zero, LinetypeSegmentRotationType.Relative, 0.0, 1.0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>LinetypeShapeSegment</c> class.
        /// </summary>
        /// <param name="text">Text to display on the linetype segment.</param>
        /// <param name="style">Name of the TextStyle.</param>
        /// <param name="length">Dash, dot, or space length of the linetype segment.</param>
        public LinetypeTextSegment(string text, TextStyle style, double length) : this(text, style, length, Vector2.Zero, LinetypeSegmentRotationType.Relative, 0.0, 1.0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>LinetypeShapeSegment</c> class.
        /// </summary>
        /// <param name="text">Text to display on the linetype segment.</param>
        /// <param name="style">Name of the TextStyle.</param>
        /// <param name="length">Dash, dot, or space length of the linetype segment.</param>
        /// <param name="offset">Shift of the shape along the line.</param>
        /// <param name="rotationType">Type of rotation defined by the rotation value.</param>
        /// <param name="rotation">Rotation of the text.</param>
        /// <param name="scale">Scale of the text.</param>
        public LinetypeTextSegment(string text, TextStyle style, double length, Vector2 offset, LinetypeSegmentRotationType rotationType, double rotation, double scale) : base(LinetypeSegmentType.Text, length)
        {
            this.text = string.IsNullOrEmpty(text) ? string.Empty : text;
            this.style = style ?? throw new ArgumentNullException(nameof(style), "The style must be a valid TextStyle.");
            this.offset = offset;
            this.rotationType = rotationType;
            this.rotation = MathHelper.NormalizeAngle(rotation);
            this.scale = scale;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the text displayed by the linetype.
        /// </summary>
        public string Text
        {
            get { return this.text; }
            set { this.text = string.IsNullOrEmpty(value) ? string.Empty : value; }
        }

        /// <summary>
        /// Gets or sets the TextStyle of the text to be displayed by the linetype.
        /// </summary>
        public TextStyle Style
        {
            get { return this.style; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                this.style = this.OnTextStyleChangedEvent(this.style, value);
            }
        }

        /// <summary>
        /// Gets or sets the shift of the text along the line.
        /// </summary>
        public Vector2 Offset
        {
            get { return this.offset; }
            set { this.offset = value; }
        }

        /// <summary>
        /// Gets or sets the type of rotation defined by the rotation value upright, relative, or absolute.
        /// </summary>
        public LinetypeSegmentRotationType RotationType
        {
            get { return this.rotationType; }
            set { this.rotationType = value; }
        }

        /// <summary>
        /// Gets or sets the angle in degrees of the text.
        /// </summary>
        public double Rotation
        {
            get { return this.rotation; }
            set { this.rotation = MathHelper.NormalizeAngle(value); }
        }

        /// <summary>
        /// Gets or sets the scale of the text relative to the scale of the linetype.
        /// </summary>
        public double Scale
        {
            get { return this.scale; }
            set { this.scale = value; }
        }

        #endregion

        #region overrides

        /// <summary>
        /// Creates a new <c>LinetypeShapeSegment</c> that is a copy of the current instance.
        /// </summary>
        /// <returns>A new <c>LinetypeShapeSegment</c> that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new LinetypeTextSegment(this.text, (TextStyle) this.style.Clone(), this.Length, this.offset, this.rotationType, this.rotation, this.scale);
        }

        #endregion
    }
}