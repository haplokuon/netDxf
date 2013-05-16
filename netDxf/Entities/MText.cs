#region netDxf, Copyright(C) 2013 Daniel Carvajal, Licensed under LGPL.

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
using System.Threading;
using netDxf.Tables;

namespace netDxf.Entities
{
    /// <summary>
    /// Represents a multiline text <see cref="EntityObject">entity</see>.
    /// </summary>
    public class MText :
        EntityObject
    {
        #region Special string codes
        /// <summary>
        /// Text strings to define special characters..
        /// </summary>
        public struct SpecialCharacters
        {
            /// <summary>
            /// Inserts a nonbreaking space
            /// </summary>
            public const string NonbreakingSpace = "\\~";
            /// <summary>
            /// Inserts a backslash
            /// </summary>
            public const string Backslash = "\\\\";
            /// <summary>
            /// Opening brace
            /// </summary>
            public const string OpeningBrace = "\\{";
            /// <summary>
            /// Closing brace
            /// </summary>
            public const string ClosingBrace = "\\}";
        }
        #endregion

        #region private fields

        private Vector3 position;        
        private double rectangleWidth;
        private double height;
        private double rotation;
        private double lineSpacing;
        private double paragraphHeightFactor;
        private MTextLineSpacingStyle lineSpacingStyle;
        private MTextAttachmentPoint attachmentPoint;
        private TextStyle style;
        private string value;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>MText</c> class.
        /// </summary>
        public MText() 
            : this(string.Empty, Vector3.Zero, 1.0, 1.0, TextStyle.Default)
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <c>MText</c> class.
        /// </summary>
        /// <param name="position">Text <see cref="Vector2">position</see> in world coordinates.</param>
        /// <param name="height">Text height.</param>
        /// <param name="rectangleWidth">Reference rectangle width.</param>
        public MText(Vector3 position, double height, double rectangleWidth)
            : this(string.Empty, position, height, rectangleWidth, TextStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>MText</c> class.
        /// </summary>
        /// <param name="position">Text <see cref="Vector2">position</see> in world coordinates.</param>
        /// <param name="height">Text height.</param>
        /// <param name="rectangleWidth">Reference rectangle width.</param>
        public MText(Vector2 position, double height, double rectangleWidth)
            : this(string.Empty, new Vector3(position.X, position.Y, 0.0), height, rectangleWidth, TextStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>MText</c> class.
        /// </summary>
        /// <param name="position">Text <see cref="Vector2">position</see> in world coordinates.</param>
        /// <param name="height">Text height.</param>
        /// <param name="rectangleWidth">Reference rectangle width.</param>
        /// <param name="style">Text <see cref="TextStyle">style</see>.</param>
        public MText(Vector3 position, double height, double rectangleWidth, TextStyle style)
            : this(string.Empty, position, height, rectangleWidth, style)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>MText</c> class.
        /// </summary>
        /// <param name="position">Text <see cref="Vector2">position</see> in world coordinates.</param>
        /// <param name="height">Text height.</param>
        /// <param name="rectangleWidth">Reference rectangle width.</param>
        /// <param name="style">Text <see cref="TextStyle">style</see>.</param>
        public MText(Vector2 position, double height, double rectangleWidth, TextStyle style)
            : this(string.Empty, new Vector3(position.X, position.Y, 0.0), height, rectangleWidth, style)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>MText</c> class.
        /// </summary>
        /// <param name="text">Text string.</param>
        /// <param name="position">Text <see cref="Vector2">position</see> in world coordinates.</param>
        /// <param name="height">Text height.</param>
        /// <param name="rectangleWidth">Reference rectangle width.</param>
        /// <param name="style">Text <see cref="TextStyle">style</see>.</param>
        public MText(string text, Vector2 position, double height, double rectangleWidth, TextStyle style)
            : this(text, new Vector3(position.X, position.Y, 0.0), height, rectangleWidth, style)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>MText</c> class.
        /// </summary>
        /// <param name="text">Text string.</param>
        /// <param name="position">Text <see cref="Vector2">position</see> in world coordinates.</param>
        /// <param name="height">Text height.</param>
        /// <param name="rectangleWidth">Reference rectangle width.</param>
        public MText(string text, Vector2 position, double height, double rectangleWidth)
            : this(text, new Vector3(position.X, position.Y, 0.0), height, rectangleWidth, TextStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>MText</c> class.
        /// </summary>
        /// <param name="text">Text string.</param>
        /// <param name="position">Text <see cref="Vector2">position</see> in world coordinates.</param>
        /// <param name="height">Text height.</param>
        /// <param name="rectangleWidth">Reference rectangle width.</param>
        public MText(string text, Vector3 position, double height, double rectangleWidth)
            : this(text, position, height, rectangleWidth, TextStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>MText</c> class.
        /// </summary>
        /// <param name="text">Text string.</param>
        /// <param name="position">Text <see cref="Vector2">position</see> in world coordinates.</param>
        /// <param name="height">Text height.</param>
        /// <param name="rectangleWidth">Reference rectangle width.</param>
        /// <param name="style">Text <see cref="TextStyle">style</see>.</param>
        public MText(string text, Vector3 position, double height, double rectangleWidth, TextStyle style)
            : base(EntityType.MText, DxfObjectCode.MText)
        {
            this.value = text;
            this.position = position;
            this.attachmentPoint = MTextAttachmentPoint.TopLeft;
            if (style == null)
                throw new ArgumentNullException("style", "The Text style cannot be null.");
            this.style = style;
            this.rectangleWidth = rectangleWidth;
            this.height = height;
            this.lineSpacing = 1.0;
            this.paragraphHeightFactor = 1.0;
            this.lineSpacingStyle = MTextLineSpacingStyle.AtLeast;
            this.rotation = 0.0;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the text rotation in degrees.
        /// </summary>
        public double Rotation
        {
            get { return this.rotation; }
            set { this.rotation = value; }
        }

        /// <summary>
        /// Gets or sets the text height.
        /// </summary>
        public double Height
        {
            get { return this.height; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value", value.ToString(Thread.CurrentThread.CurrentCulture));
                this.height = value;
            }
        }

        /// <summary>
        /// Gets or sets the line spacing factor.
        /// </summary>
        /// <remarks>
        /// Percentage of default line spacing to be applied. Valid values range from 0.25 to 4.00, the default value 1.0.
        /// </remarks>
        public double LineSpacingFactor
        {
            get { return lineSpacing; }
            set
            {
                if(value<0.25 || value>4.0)
                    throw new ArgumentOutOfRangeException("value", value, "Valid values range from 0.25 to 4.00");
                lineSpacing = value;
            }
        }

        /// <summary>
        /// Gets or sets the paragraph height factor.
        /// </summary>
        /// <remarks>
        /// Percentage of default paragraph height factor to be applied. Valid values range from 0.25 to 4.00, the default value 1.0.
        /// </remarks>
        public double ParagraphHeightFactor
        {
            get { return paragraphHeightFactor; }
            set
            {
                if (value < 0.25 || value > 4.0)
                    throw new ArgumentOutOfRangeException("value", value, "Valid values range from 0.25 to 4.00");
                paragraphHeightFactor = value;
            }
        }

        /// <summary>
        /// Get or sets the <see cref="MTextLineSpacingStyle">line spacing style</see>.
        /// </summary>
        public MTextLineSpacingStyle LineSpacingStyle
        {
            get { return lineSpacingStyle; }
            set { lineSpacingStyle = value; }
        }

        /// <summary>
        /// Gets or sets the text reference rectangle width.
        /// </summary>
        /// <remarks>
        /// This value defines the width of the box where the text will fit.
        /// If a paragraph width is longer than the rectangle width it will be broken in several lines, using the word spaces as breaking points.
        ///  </remarks>
        public double RectangleWidth
        {
            get { return rectangleWidth; }
            set { rectangleWidth = value; }
        }

        /// <summary>
        /// Gets or sets the text <see cref="MTextAttachmentPoint">attachment point</see>.
        /// </summary>
        public MTextAttachmentPoint AttachmentPoint
        {
            get { return this.attachmentPoint; }
            set { this.attachmentPoint = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="TextStyle">text style</see>.
        /// </summary>
        public TextStyle Style
        {
            get { return this.style; }
            set { this.style = value; }
        }

        /// <summary>
        /// Gets or sets the Text <see cref="Vector2">position</see> in world coordinates..
        /// </summary>
        public Vector3 Position
        {
            get { return this.position; }
            set { this.position = value; }
        }

        /// <summary>
        /// Gets the text string.
        /// </summary>
        public string Value
        {
            get { return this.value; }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Adds the text to the existing paragraph. 
        /// </summary>
        /// <param name="text">Text string.</param>
        /// <param name="options">Text formatting options.</param>
        public void Write(string text, MTextFormattingOptions options = null)
        {
            if (options == null)
                this.value += text;
            else
                this.value += options.FormatText(text);
        }
        /// <summary>
        /// Ends the actual paragraph (adds the end paragraph code and the paragraph height factor). 
        /// </summary>
        public void EndParagraph()
        {
            if(!MathHelper.IsOne(paragraphHeightFactor))
                this.value += "{\\H" + paragraphHeightFactor + "x;}\\P";
            else
                this.value += "\\P";
        }
        #endregion

    }
}