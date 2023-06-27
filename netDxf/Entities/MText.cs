#region netDxf library licensed under the MIT License
// 
//                       netDxf library
// Copyright (c) Daniel Carvajal (haplokuon@gmail.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// 
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using netDxf.Tables;
using netDxf.Units;

namespace netDxf.Entities
{
    /// <summary>
    /// Represents a multiline text <see cref="EntityObject">entity</see>.
    /// </summary>
    /// <remarks>
    /// Formatting codes for MText, you can use them directly while setting the text value or use the Write() and EndParagraph() methods.<br />
    /// \L Start underline<br />
    /// \l Stop underline<br />
    /// \O Start overstrike<br />
    /// \o Stop overstrike<br />
    /// \K Start strike-through<br />
    /// \k Stop strike-through<br />
    /// \P New paragraph (new line)<br />
    /// \pxi Control codes for bullets, numbered paragraphs and columns<br />
    /// \X Paragraph wrap on the dimension line (only in dimensions)<br />
    /// \Q Slanting (obliquing) text by angle - e.g. \Q30;<br />
    /// \H Text height - e.g. \H3x;<br />
    /// \W Text width - e.g. \W0.8x;<br />
    /// \F Font selection<br />
    /// <br />
    /// e.g. \Fgdt;o - GDT-tolerance<br />
    /// e.g. \Fkroeger|b0|i0|c238|p10; - font Kroeger, non-bold, non-italic, code page 238, pitch 10<br />
    /// <br />
    /// \S Stacking, fractions<br />
    /// <br />
    /// e.g. \SA^B;<br />
    /// A<br />
    /// B<br />
    /// e.g. \SX/Y<br />
    /// X<br />
    /// -<br />
    /// Y<br />
    /// e.g. \S1#4;<br />
    /// 1/4<br />
    /// <br />
    /// \A Alignment<br />
    /// \A0; = bottom<br />
    /// \A1; = center<br />
    /// \A2; = top<br />
    /// <br />
    /// \C Color change<br />
    /// \C1; = red<br />
    /// \C2; = yellow<br />
    /// \C3; = green<br />
    /// \C4; = cyan<br />
    /// \C5; = blue<br />
    /// \C6; = magenta<br />
    /// \C7; = white<br />
    /// <br />
    /// \T Tracking, char.spacing - e.g. \T2;<br />
    /// \~ Non-wrapping space, hard space<br />
    /// {} Braces - define the text area influenced by the code<br />
    /// \ Escape character - e.g. \\ = "\", \{ = "{"<br />
    /// <br />
    /// Codes and braces can be nested up to 8 levels deep.<br />
    /// </remarks>
    public class MText :
        EntityObject
    {
        #region delegates and events

        public delegate void TextStyleChangedEventHandler(MText sender, TableObjectChangedEventArgs<TextStyle> e);
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

        private Vector3 position;
        private double rectangleWidth;
        private double height;
        private double rotation;
        private double lineSpacing;
        private MTextLineSpacingStyle lineSpacingStyle;
        private MTextDrawingDirection drawingDirection;
        private MTextAttachmentPoint attachmentPoint;
        private TextStyle style;
        private string text;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>MText</c> class.
        /// </summary>
        public MText()
            : this(string.Empty, Vector3.Zero, 1.0, 0.0, TextStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>MText</c> class.
        /// </summary>
        /// <param name="text">Text string.</param>
        public MText(string text)
            : this(text, Vector3.Zero, 1.0, 0.0, TextStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>MText</c> class.
        /// </summary>
        /// <param name="position">Text <see cref="Vector2">position</see> in world coordinates.</param>
        /// <param name="height">Text height.</param>
        public MText(Vector2 position, double height)
            : this(string.Empty, position, height, 0.0, TextStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>MText</c> class.
        /// </summary>
        /// <param name="position">Text <see cref="Vector2">position</see> in world coordinates.</param>
        /// <param name="height">Text height.</param>
        public MText(Vector3 position, double height)
            : this(string.Empty, position, height, 0.0, TextStyle.Default)
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
        public MText(string text, Vector2 position, double height)
            : this(text, new Vector3(position.X, position.Y, 0.0), height, 0.0, TextStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>MText</c> class.
        /// </summary>
        /// <param name="text">Text string.</param>
        /// <param name="position">Text <see cref="Vector2">position</see> in world coordinates.</param>
        /// <param name="height">Text height.</param>
        public MText(string text, Vector3 position, double height)
            : this(text, position, height, 0.0, TextStyle.Default)
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
        /// <param name="style">Text <see cref="TextStyle">style</see>.</param>
        public MText(string text, Vector3 position, double height, double rectangleWidth, TextStyle style)
            : base(EntityType.MText, DxfObjectCode.MText)
        {
            this.text = text;
            this.position = position;
            this.attachmentPoint = MTextAttachmentPoint.TopLeft;
            this.style = style ?? throw new ArgumentNullException(nameof(style));
            this.rectangleWidth = rectangleWidth;
            if (height <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), this.text, "The MText height must be greater than zero.");
            }
            this.height = height;
            this.lineSpacing = 1.0;
            this.lineSpacingStyle = MTextLineSpacingStyle.AtLeast;
            this.drawingDirection = MTextDrawingDirection.ByStyle;
            this.rotation = 0.0;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets if the text will be mirrored when a symmetry is performed, when the current MText entity does not belong to a DXF document.
        /// </summary>
        public static bool DefaultMirrText
        {
            get;
            set;
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
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The MText height must be greater than zero.");
                }
                this.height = value;
            }
        }

        /// <summary>
        /// Gets or sets the line spacing factor.
        /// </summary>
        /// <remarks>
        /// Percentage of default line spacing to be applied. Valid values range from 0.25 to 4.0, the default value 1.0.
        /// </remarks>
        public double LineSpacingFactor
        {
            get { return this.lineSpacing; }
            set
            {
                if (value < 0.25 || value > 4.0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The MText LineSpacingFactor valid values range from 0.25 to 4.0");
                }
                this.lineSpacing = value;
            }
        }


        /// <summary>
        /// Get or sets the <see cref="MTextLineSpacingStyle">line spacing style</see>.
        /// </summary>
        /// <remarks>
        /// The only available options are AtLeast and Exact, Default and Multiple are only applicable to MTextParagraphOptions objects.
        /// </remarks>
        public MTextLineSpacingStyle LineSpacingStyle
        {
            get { return this.lineSpacingStyle; }
            set
            {
                if (value == MTextLineSpacingStyle.Default || value == MTextLineSpacingStyle.Multiple)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The Default and Multiple options are only applicable to MTextParagraphOptions objects.");
                }
                this.lineSpacingStyle = value;
            }
        }

        /// <summary>
        /// Get or sets the <see cref="MTextDrawingDirection">text drawing direction</see>.
        /// </summary>
        public MTextDrawingDirection DrawingDirection
        {
            get { return this.drawingDirection; }
            set { this.drawingDirection = value; }
        }

        /// <summary>
        /// Gets or sets the text reference rectangle width.
        /// </summary>
        /// <remarks>
        /// This value defines the width of the box where the text will fit.<br/>
        /// If a paragraph width is longer than the rectangle width it will be broken in several lines, using the word spaces as breaking points.<br/>
        /// If you specify a width of 0, word wrap is turned off and the width of the multiline text object is as wide as the longest line of text.
        ///  </remarks>
        public double RectangleWidth
        {
            get { return this.rectangleWidth; }
            set
            {
                if (value < 0.0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The MText rectangle width must be equals or greater than zero.");
                }
                this.rectangleWidth = value;
            }
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
        /// Gets or sets the Text <see cref="Vector3">position</see> in world coordinates.
        /// </summary>
        public Vector3 Position
        {
            get { return this.position; }
            set { this.position = value; }
        }

        /// <summary>
        /// Gets or sets the raw text string.
        /// </summary>
        public string Value
        {
            get { return this.text; }
            set { this.text = value; }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Formats a text string to represent a fraction.
        /// </summary>
        /// <param name="numerator">Fraction numerator.</param>
        /// <param name="denominator">Fraction denominator.</param>
        /// <param name="fractionType">Style of the fraction.</param>
        /// <returns>A text string that represents the fraction.</returns>
        /// <remarks>
        /// In fractions the characters '/' and '#' are reserved if you need to write them you must write "\/" and "\#", respectively.
        /// </remarks>
        public void WriteFraction(string numerator, string denominator, FractionFormatType fractionType)
        {
           this.WriteFraction(numerator, denominator, fractionType, null);
        }

        /// <summary>
        /// Formats a text string to represent a fraction.
        /// </summary>
        /// <param name="numerator">Fraction numerator.</param>
        /// <param name="denominator">Fraction denominator.</param>
        /// <param name="fractionType">Style of the fraction.</param>
        /// <param name="options">Text formatting options.</param>
        /// <returns>A text string that represents the fraction.</returns>
        /// <remarks>
        /// In fractions the characters '/' and '#' are reserved if you need to write them you must write "\/" and "\#", respectively.<br />
        /// Do not combine fractions with super or subscript options, it is not supported (stacking commands cannot be nested).
        /// </remarks>
        public void WriteFraction(string numerator, string denominator, FractionFormatType fractionType, MTextFormattingOptions options)
        {
            string txt = string.Empty;
            switch (fractionType)
            {
                case FractionFormatType.Diagonal:
                    txt = "\\S" + numerator + "#" + denominator + ";";
                    break;
                case FractionFormatType.Horizontal:
                    txt = "\\S" + numerator + "/" + denominator + ";";
                    break;
                case FractionFormatType.NotStacked:
                    txt = numerator + "/" + denominator;
                    break;
            }

            this.Write(txt, options);
        }

        /// <summary>
        /// Adds the text to the current paragraph. 
        /// </summary>
        /// <param name="txt">Text string.</param>
        public void Write(string txt)
        {
            this.Write(txt, null);
        }

        /// <summary>
        /// Adds the text to the current paragraph. 
        /// </summary>
        /// <param name="txt">Text string.</param>
        /// <param name="options">Text formatting options.</param>
        public void Write(string txt, MTextFormattingOptions options)
        {
            if (options == null)
            {
                this.text += txt;
                return;
            }

            string formattedText = txt;
            double baseHeightFactor = options.HeightFactor;

            if (options.Superscript)
            {
                formattedText = string.Format("\\S{0}^ ;", formattedText);
                baseHeightFactor *= options.SuperSubScriptHeightFactor;
            }
            if (options.Subscript)
            {
                formattedText = string.Format("\\S^ {0};", formattedText);
                baseHeightFactor *= options.SuperSubScriptHeightFactor;
            }

            string f;
            if (string.IsNullOrEmpty(options.FontName))
            {
                f = string.IsNullOrEmpty(this.style.FontFamilyName) ? this.style.FontFile : this.style.FontFamilyName;
            }
            else
            {
                f = options.FontName;
            }

            if (options.Bold && options.Italic)
            {
                formattedText = string.Format("\\F{0}|b1|i1;{1}", f, formattedText);
            }
            else if (options.Bold && !options.Italic)
            {
                formattedText = string.Format("\\F{0}|b1|i0;{1}", f, formattedText);
            }
            else if (!options.Bold && options.Italic)
            {
                formattedText = string.Format("\\F{0}|i1|b0;{1}", f, formattedText);
            }
            else
            {
                formattedText = string.Format("\\F{0}|b0|i0;{1}", f, formattedText);
            }

            if (options.Overline)
            {
                formattedText = string.Format("\\O{0}\\o", formattedText);
            }

            if (options.Underline)
            {
                formattedText = string.Format("\\L{0}\\l", formattedText);
            }

            if (options.StrikeThrough)
            {
                formattedText = string.Format("\\K{0}\\k", formattedText);
            }

            if (options.Color != null)
            {
                // The DXF is not consistent in the way it converts a true color to its 24-bit representation,
                // while stuff like layer colors it follows BGR order, when formatting text it uses RGB order.
                formattedText = options.Color.UseTrueColor
                    ? string.Format("\\C{0};\\c{1};{2}", options.Color.Index, BitConverter.ToInt32(new byte[] { options.Color.R, options.Color.G, options.Color.B, 0 }, 0), formattedText)
                    : string.Format("\\C{0};{1}", options.Color.Index, formattedText);
            }

            if (!MathHelper.IsOne(baseHeightFactor))
            {
                formattedText = string.Format("\\H{0}x;{1}", baseHeightFactor.ToString(CultureInfo.InvariantCulture), formattedText);
            }

            if (!MathHelper.IsZero(options.ObliqueAngle))
            {
                formattedText = string.Format("\\Q{0};{1}", options.ObliqueAngle.ToString(CultureInfo.InvariantCulture), formattedText);
            }

            if (!MathHelper.IsOne(options.CharacterSpaceFactor))
            {
                formattedText = string.Format("\\T{0};{1}", options.CharacterSpaceFactor.ToString(CultureInfo.InvariantCulture), formattedText);
            }

            if (!MathHelper.IsOne(options.WidthFactor))
            {
                formattedText = string.Format("\\W{0};{1}", options.WidthFactor.ToString(CultureInfo.InvariantCulture), formattedText);
            }

            this.text += "{" + formattedText + "}";
        }

        /// <summary>
        /// Ends the current paragraph. 
        /// </summary>
        public void EndParagraph()
        {
            this.text += "\\P";
        }

        /// <summary>
        /// Starts a new paragraph. 
        /// </summary>
        /// <remarks>
        /// When no paragraph options are used, strictly speaking, there is no need to call this method, the previous paragraph options will be inherited.<br />
        /// When there is no need to change the paragraph options from the previous, it is no necessary to pass again the same instance,
        /// the paragraph characteristics are inherited from the previous one.
        /// This way no codes needs to be written and it will save some characters in the final string.
        /// </remarks>
        public void StartParagraph()
        {
            this.StartParagraph(null);

        }
        /// <summary>
        /// Starts a new paragraph. 
        /// </summary>
        /// <param name="options">Paragraph options.</param>
        /// <remarks>
        /// When no paragraph options are used, strictly speaking, there is no need to call this method, the previous paragraph options will be inherited.<br />
        /// When there is no need to change the paragraph options from the previous, it is no necessary to pass again the same instance,
        /// the paragraph characteristics are inherited from the previous one.
        /// This way no codes needs to be written and it will save some characters in the final string.
        /// </remarks>
        public void StartParagraph(MTextParagraphOptions options)
        {
            if (options == null)
            {
                this.text += "\\A1;";
                return;
            }

            string codes = string.Empty;
            
            switch (options.Alignment)
            {
                case MTextParagraphAlignment.Left:
                    codes = string.Format("\\pql;{0}", codes);
                    break;
                case MTextParagraphAlignment.Center:
                    codes = string.Format("\\pqc;{0}", codes);
                    break;
                case MTextParagraphAlignment.Right:
                    codes = string.Format("\\pqr;{0}", codes);
                    break;
                case MTextParagraphAlignment.Justified:
                    codes = string.Format("\\pqj;{0}", codes);
                    break;
                case MTextParagraphAlignment.Distribute:
                    codes = string.Format("\\pqd;{0}", codes);
                    break;
            }

            // when the first line indent is negative, it cannot be lower than the available space left by the left indent
            double fli = options.FirstLineIndent;
            if (fli < 0.0 && Math.Abs(fli) > options.LeftIndent)
            {
                fli = -options.LeftIndent;
            }

            codes = string.Format("\\pi{0},l{1},r{2},b{3},a{4};{5}",
                fli.ToString(CultureInfo.InvariantCulture),
                options.LeftIndent.ToString(CultureInfo.InvariantCulture),
                options.RightIndent.ToString(CultureInfo.InvariantCulture),
                options.SpacingBefore.ToString(CultureInfo.InvariantCulture),
                options.SpacingAfter.ToString(CultureInfo.InvariantCulture),
                codes);

            switch (options.LineSpacingStyle)
            {
                case MTextLineSpacingStyle.Default:
                    codes = string.Format("\\ps*;{0}", codes);
                    break;
                case MTextLineSpacingStyle.AtLeast:
                    codes = string.Format("\\psa{0};{1}", options.LineSpacingFactor.ToString(CultureInfo.InvariantCulture), codes);
                    break;
                case MTextLineSpacingStyle.Exact:
                    codes = string.Format("\\pse{0};{1}", options.LineSpacingFactor.ToString(CultureInfo.InvariantCulture), codes);
                    break;
                case MTextLineSpacingStyle.Multiple:
                    codes = string.Format("\\psm{0};{1}", options.LineSpacingFactor.ToString(CultureInfo.InvariantCulture), codes);
                    break;
            }

            codes = string.Format("\\A{0};\\H{1}x;{2}", (int)options.VerticalAlignment, options.HeightFactor.ToString(CultureInfo.InvariantCulture), codes);

            this.text += codes;
        }

        /// <summary>
        /// Obtains the MText text value without the formatting codes, control characters like tab '\t' will be preserved in the result,
        /// the new paragraph command "\P" will be converted to new line feed '\r\n'.
        /// </summary>
        /// <returns>MText text value without the formatting codes.</returns>
        public string PlainText()
        {
            if (string.IsNullOrEmpty(this.text))
                return string.Empty;

            string txt = this.text;

            //text = text.Replace("%%c", "Ø");
            //text = text.Replace("%%d", "°");
            //text = text.Replace("%%p", "±");

            StringBuilder rawText = new StringBuilder();

            using (CharEnumerator chars = txt.GetEnumerator())
            {
                while (chars.MoveNext())
                {
                    char token = chars.Current;
                    if (token == '\\') // is a formatting command
                    {
                        if (chars.MoveNext())
                            token = chars.Current;
                        else
                            return rawText.ToString(); // premature end of text

                        if (token == '\\' | token == '{' | token == '}') // escape chars
                            rawText.Append(token);
                        else if (token == 'L' | token == 'l' | token == 'O' | token == 'o' | token == 'K' | token == 'k')
                        {
                            /* discard one char commands */
                        }
                        else if (token == 'P' | token == 'X')
                            rawText.Append(Environment.NewLine); // replace the end paragraph command with the standard new line, carriage return code "\r\n".
                        else if (token == 'S')
                        {
                            if (chars.MoveNext())
                                token = chars.Current;
                            else
                                return rawText.ToString(); // premature end of text

                            // we want to preserve the text under the stacking command
                            StringBuilder data = new StringBuilder();

                            while (token != ';')
                            {
                                if (token == '\\')
                                {
                                    if (chars.MoveNext())
                                        token = chars.Current;
                                    else
                                        return rawText.ToString(); // premature end of text

                                    data.Append(token);
                                }
                                else if (token == '^')
                                {
                                    if (chars.MoveNext())
                                        token = chars.Current;
                                    else
                                        return rawText.ToString(); // premature end of text

                                    // discard the code "^ " that defines super and subscript texts
                                    if (token != ' ') data.Append("^" + token);
                                }
                                else
                                {
                                    // replace the '#' stacking command by '/'
                                    // non command characters '#' are written as '\#'
                                    data.Append(token == '#' ? '/' : token);
                                }

                                if (chars.MoveNext())
                                    token = chars.Current;
                                else
                                    return rawText.ToString(); // premature end of text
                            }

                            rawText.Append(data);
                        }
                        else
                        {
                            // formatting commands of more than one character always terminate in ';'
                            // discard all information
                            while (token != ';')
                            {
                                if (chars.MoveNext())
                                    token = chars.Current;
                                else
                                    return rawText.ToString(); // premature end of text
                            }
                        }
                    }
                    else if (token == '{' | token == '}')
                    {
                        /* discard group markers */
                    }
                    else // char is what it is, a character
                        rawText.Append(token);
                }
            }

            return rawText.ToString();
        }

        #endregion

        #region overrides

        /// <summary>
        /// Moves, scales, and/or rotates the current entity given a 3x3 transformation matrix and a translation vector.
        /// </summary>
        /// <param name="transformation">Transformation matrix.</param>
        /// <param name="translation">Translation vector.</param>
        /// <remarks>
        /// Non-uniform scaling is not supported, it would require to decompose each line into independent Text entities.
        /// When the current Text entity does not belong to a DXF document, the text will use the DefaultMirrText when a symmetry is performed;
        /// otherwise, the drawing variable MirrText will be used.<br />
        /// Matrix3 adopts the convention of using column vectors to represent a transformation matrix.
        /// </remarks>
        public override void TransformBy(Matrix3 transformation, Vector3 translation)
        {
            bool mirrText = this.Owner == null ? DefaultMirrText : this.Owner.Record.Owner.Owner.DrawingVariables.MirrText;

            Vector3 newPosition = transformation * this.Position + translation;
            Vector3 newNormal = transformation * this.Normal;
            if (Vector3.Equals(Vector3.Zero, newNormal))
            {
                newNormal = this.Normal;
            }

            Matrix3 transOW = MathHelper.ArbitraryAxis(this.Normal);

            Matrix3 transWO = MathHelper.ArbitraryAxis(newNormal);
            transWO = transWO.Transpose();

            List<Vector2> uv = MathHelper.Transform(
                new[]
                {
                    Vector2.UnitX, Vector2.UnitY
                }, 
                this.Rotation * MathHelper.DegToRad,
                CoordinateSystem.Object, CoordinateSystem.World);

            Vector3 v;
            v = transOW * new Vector3(uv[0].X, uv[0].Y, 0.0);
            v = transformation * v;
            v = transWO * v;
            Vector2 newUvector = new Vector2(v.X, v.Y);

            // the MText entity does not support non-uniform scaling
            double scale = newUvector.Modulus();

            v = transOW * new Vector3(uv[1].X, uv[1].Y, 0.0);
            v = transformation * v;
            v = transWO * v;
            Vector2 newVvector = new Vector2(v.X, v.Y);

            double newRotation = Vector2.Angle(newUvector) * MathHelper.RadToDeg;

            if (mirrText)
            {
                if (Vector2.CrossProduct(newUvector, newVvector) < 0.0)
                {
                    newRotation += 180;
                    newNormal = -newNormal; 
                }
            }
            else
            {
                if (Vector2.CrossProduct(newUvector, newVvector) < 0.0)
                {
                    if (Vector2.DotProduct(newUvector, uv[0]) < 0.0)
                    {
                        newRotation += 180;

                        switch (this.AttachmentPoint)
                        {
                            case MTextAttachmentPoint.TopLeft:
                                this.AttachmentPoint = MTextAttachmentPoint.TopRight;
                                break;
                            case MTextAttachmentPoint.TopRight:
                                this.AttachmentPoint = MTextAttachmentPoint.TopLeft;
                                break;
                            case MTextAttachmentPoint.MiddleLeft:
                                this.AttachmentPoint = MTextAttachmentPoint.MiddleRight;
                                break;
                            case MTextAttachmentPoint.MiddleRight:
                                this.AttachmentPoint = MTextAttachmentPoint.MiddleLeft;
                                break;
                            case MTextAttachmentPoint.BottomLeft:
                                this.AttachmentPoint = MTextAttachmentPoint.BottomRight;
                                break;
                            case MTextAttachmentPoint.BottomRight:
                                this.AttachmentPoint = MTextAttachmentPoint.BottomLeft;
                                break;
                        }
                    }
                    else
                    {
                        switch (this.AttachmentPoint)
                        {
                            case MTextAttachmentPoint.TopLeft:
                                this.AttachmentPoint = MTextAttachmentPoint.BottomLeft;
                                break;
                            case MTextAttachmentPoint.TopCenter:
                                this.attachmentPoint = MTextAttachmentPoint.BottomCenter;
                                break;
                            case MTextAttachmentPoint.TopRight:
                                this.AttachmentPoint = MTextAttachmentPoint.BottomRight;
                                break;
                            case MTextAttachmentPoint.BottomLeft:
                                this.AttachmentPoint = MTextAttachmentPoint.TopLeft;
                                break;
                            case MTextAttachmentPoint.BottomCenter:
                                this.attachmentPoint = MTextAttachmentPoint.TopCenter;
                                break;
                            case MTextAttachmentPoint.BottomRight:
                                this.AttachmentPoint = MTextAttachmentPoint.TopRight;
                                break;
                        }
                    }
                }
            }

            double newHeight = this.Height * scale;
            newHeight = MathHelper.IsZero(newHeight) ? MathHelper.Epsilon : newHeight;

            this.Position = newPosition;
            this.Normal = newNormal;
            this.Rotation = newRotation;
            this.Height = newHeight;
            this.RectangleWidth *= scale;

        }

        /// <summary>
        /// Creates a new MText that is a copy of the current instance.
        /// </summary>
        /// <returns>A new MText that is a copy of this instance.</returns>
        public override object Clone()
        {
            MText entity = new MText
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
                //MText properties
                Position = this.position,
                Rotation = this.rotation,
                Height = this.height,
                LineSpacingFactor = this.lineSpacing,
                LineSpacingStyle = this.lineSpacingStyle,
                RectangleWidth = this.rectangleWidth,
                AttachmentPoint = this.attachmentPoint,
                Style = (TextStyle) this.style.Clone(),
                Value = this.text
            };

            foreach (XData data in this.XData.Values)
            {
                entity.XData.Add((XData) data.Clone());
            }

            return entity;
        }

        #endregion
    }
}