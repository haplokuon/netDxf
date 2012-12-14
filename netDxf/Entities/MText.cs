#region netDxf, Copyright(C) 2012 Daniel Carvajal, Licensed under LGPL.

//                        netDxf library
// Copyright (C) 2012 Daniel Carvajal (haplokuon@gmail.com)
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
using System.Threading;
using netDxf.Tables;

namespace netDxf.Entities
{
    /// <summary>
    /// Represents a MText <see cref="IEntityObject">entity</see>.
    /// </summary>
    public class MText :
        DxfObject,
        IEntityObject
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

        private const EntityType TYPE = EntityType.MText;
        private Vector3 insertionPoint;        
        private AciColor color;
        private Layer layer;
        private LineType lineType;
        private Vector3 normal;
        private double rectangleWidth;
        private double height;
        private double rotation;
        private double lineSpacing;
        private double paragraphHeightFactor;
        private MTextLineSpacingStyle lineSpacingStyle;
        private MTextAttachmentPoint attachmentPoint;
        private TextStyle style;
        private string value;
        private Dictionary<ApplicationRegistry, XData> xData;

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
        /// <param name="insertionPoint">Text insertion <see cref="Vector3">point</see>.</param>
        /// <param name="height">Text height.</param>
        /// <param name="rectangleWidth">Reference rectangle width.</param>
        public MText(Vector3 insertionPoint, double height, double rectangleWidth)
            : this(string.Empty, insertionPoint, height, rectangleWidth, TextStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>MText</c> class.
        /// </summary>
        /// <param name="insertionPoint">Text insertion <see cref="Vector2">point</see>.</param>
        /// <param name="height">Text height.</param>
        /// <param name="rectangleWidth">Reference rectangle width.</param>
        public MText(Vector2 insertionPoint, double height, double rectangleWidth)
            : this(string.Empty, new Vector3(insertionPoint.X, insertionPoint.Y, 0.0), height, rectangleWidth, TextStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>MText</c> class.
        /// </summary>
        /// <param name="insertionPoint">Text insertion <see cref="Vector3">point</see>.</param>
        /// <param name="height">Text height.</param>
        /// <param name="rectangleWidth">Reference rectangle width.</param>
        /// <param name="style">Text <see cref="TextStyle">style</see>.</param>
        public MText(Vector3 insertionPoint, double height, double rectangleWidth, TextStyle style)
            : this(string.Empty, insertionPoint, height, rectangleWidth, style)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>MText</c> class.
        /// </summary>
        /// <param name="insertionPoint">Text insertion <see cref="Vector2">point</see>.</param>
        /// <param name="height">Text height.</param>
        /// <param name="rectangleWidth">Reference rectangle width.</param>
        /// <param name="style">Text <see cref="TextStyle">style</see>.</param>
        public MText(Vector2 insertionPoint, double height, double rectangleWidth, TextStyle style)
            : this(string.Empty, new Vector3(insertionPoint.X, insertionPoint.Y, 0.0), height, rectangleWidth, style)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>MText</c> class.
        /// </summary>
        /// <param name="text">Text string.</param>
        /// <param name="insertionPoint">Text insertion <see cref="Vector3">point</see>.</param>
        /// <param name="height">Text height.</param>
        /// <param name="rectangleWidth">Reference rectangle width.</param>
        public MText(string text, Vector3 insertionPoint, double height, double rectangleWidth)
            : this(text, insertionPoint, height, rectangleWidth, TextStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>MText</c> class.
        /// </summary>
        /// <param name="text">Text string.</param>
        /// <param name="insertionPoint">Text insertion <see cref="Vector2">point</see>.</param>
        /// <param name="height">Text height.</param>
        /// <param name="rectangleWidth">Reference rectangle width.</param>
        public MText(string text, Vector2 insertionPoint, double height, double rectangleWidth)
            : this(text, new Vector3(insertionPoint.X, insertionPoint.Y, 0.0), height, rectangleWidth, TextStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>MText</c> class.
        /// </summary>
        /// <param name="text">Text string.</param>
        /// <param name="insertionPoint">Text insertion <see cref="Vector3">point</see>.</param>
        /// <param name="height">Text height.</param>
        /// <param name="rectangleWidth">Reference rectangle width.</param>
        /// <param name="style">Text <see cref="TextStyle">style</see>.</param>
        public MText(string text, Vector3 insertionPoint, double height, double rectangleWidth, TextStyle style)
            : base(DxfObjectCode.MText)
        {
            this.value = text;
            this.insertionPoint = insertionPoint;
            this.attachmentPoint = MTextAttachmentPoint.TopLeft;
            this.layer = Layer.Default;
            this.color = AciColor.ByLayer;
            this.lineType = LineType.ByLayer;
            this.normal = Vector3.UnitZ;
            this.style = style;
            this.rectangleWidth = rectangleWidth;
            this.height = height;
            this.lineSpacing = 1.0;
            this.paragraphHeightFactor = 1.0;
            this.lineSpacingStyle = MTextLineSpacingStyle.AtLeast;
            this.rotation = 0.0;
        }

        /// <summary>
        /// Initializes a new instance of the <c>MText</c> class.
        /// </summary>
        /// <param name="text">Text string.</param>
        /// <param name="insertionPoint">Text insertion <see cref="Vector2">point</see>.</param>
        /// <param name="height">Text height.</param>
        /// <param name="rectangleWidth">Reference rectangle width.</param>
        /// <param name="style">Text <see cref="TextStyle">style</see>.</param>
        public MText(string text, Vector2 insertionPoint, double height, double rectangleWidth, TextStyle style)
            : this(text, new Vector3(insertionPoint.X, insertionPoint.Y, 0.0), height, rectangleWidth, style)
        {
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
        /// Gets or sets the text attachment point.
        /// </summary>
        public MTextAttachmentPoint AttachmentPoint
        {
            get { return this.attachmentPoint; }
            set { this.attachmentPoint = value; }
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
        /// Gets or sets the text base <see cref="netDxf.Vector3">point</see>.
        /// </summary>
        public Vector3 InsertionPoint
        {
            get { return this.insertionPoint; }
            set { this.insertionPoint = value; }
        }

        /// <summary>
        /// Gets or sets the text <see cref="netDxf.Vector3">normal</see>.
        /// </summary>
        public Vector3 Normal
        {
            get { return this.normal; }
            set
            {
                value.Normalize();
                this.normal = value;
            }
        }

        /// <summary>
        /// Gets the text string.
        /// </summary>
        public string Value
        {
            get { return this.value; }
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