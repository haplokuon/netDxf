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

namespace netDxf.Entities
{
    /// <summary>
    /// Options for the <see cref="MText">multiline text</see> entity text formatting.
    /// </summary>
    /// <remarks>Old DXF versions might not support all available formatting codes.</remarks>
    public class MTextFormattingOptions
    {
        #region private fields

        private double superSubScriptHeightFactor;
        private bool bold;
        private bool italic;
        private bool overline;
        private bool underline;
        private bool strikeThrough;
        private bool superscript;
        private bool subscript;
        private AciColor color;
        private string fontName;
        private double heightFactor;
        private double obliqueAngle;
        private double characterSpaceFactor;
        private double widthFactor;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>MTextFormattingOptions</c> class.
        /// </summary>
        public MTextFormattingOptions()
        {
            this.bold = false;
            this.italic = false;
            this.overline = false;
            this.underline = false;
            this.color = null;
            this.fontName = null;
            this.heightFactor = 1.0;
            this.obliqueAngle = 0.0;
            this.characterSpaceFactor = 1.0;
            this.widthFactor = 1.0;
            this.superSubScriptHeightFactor = 0.7;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets if the text is bold.
        /// </summary>
        /// <remarks>The font style must support bold characters.</remarks>
        public bool Bold
        {
            get { return this.bold; }
            set { this.bold = value; }
        }

        /// <summary>
        /// Gets or sets if the text is italic.
        /// </summary>
        /// <remarks>The font style must support italic characters.</remarks>
        public bool Italic
        {
            get { return this.italic; }
            set { this.italic = value; }
        }

        /// <summary>
        /// Gets or sets the over line.
        /// </summary>
        public bool Overline
        {
            get { return this.overline; }
            set { this.overline = value; }
        }

        /// <summary>
        /// Gets or sets underline.
        /// </summary>
        public bool Underline
        {
            get { return this.underline; }
            set { this.underline = value; }
        }

        /// <summary>
        /// Gets or sets strike-through.
        /// </summary>
        public bool StrikeThrough
        {
            get { return this.strikeThrough; }
            set { this.strikeThrough = value; }
        }

        /// <summary>
        /// Get or set if the text is superscript.
        /// </summary>
        /// <remarks>
        /// The Superscript and subscript properties are mutually exclusive, if it is set to true the Subscript property will be set to false automatically.<br />
        /// Internally, superscripts and subscripts are written as stacking text (like fractions);
        /// therefore the characters '/' and '#' are reserved if you need to write them you must write '\/' and '\#' respectively.
        /// </remarks>
        public bool Superscript
        {
            get { return this.superscript; }
            set
            {
                if (value) this.subscript = false;
                this.superscript = value;
            }
        }

        /// <summary>
        /// Get or set if the text is subscript.
        /// </summary>
        /// <remarks>
        /// The Superscript and Subscript properties are mutually exclusive, if it is set to true the Superscript property will be set to false automatically.<br />
        /// Internally, superscripts and subscripts are written as stacking text (like fractions);
        /// therefore the characters '/' and '#' are reserved if you need to write them you must write '\/' and '\#' respectively.
        /// </remarks>
        public bool Subscript
        {
            get { return this.subscript; }
            set
            {
                if (value) this.superscript = false;
                this.subscript = value;
            }
        }

        /// <summary>
        /// Gets or sets the superscript and subscript text height as a multiple of the current text height.
        /// </summary>
        /// <remarks>By default it is set as 0.7 the current text height.</remarks>
        public double SuperSubScriptHeightFactor
        {
            get { return this.superSubScriptHeightFactor; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The character percentage height must be greater than zero.");
                this.superSubScriptHeightFactor = value;
            }
        }

        /// <summary>
        /// Gets or sets the text color.
        /// </summary>
        /// <remarks>
        /// Set as null to apply the default color defined by the MText entity.
        /// </remarks>
        public AciColor Color
        {
            get { return this.color; }
            set { this.color = value; }
        }

        /// <summary>
        /// Gets or sets the font that will override the default defined in the TextStyle. 
        /// </summary>
        /// <remarks>
        /// Set as null or empty to apply the default font.<br />
        /// When using SHX fonts use the font file with the SHX extension,
        /// when using TTF fonts use the font family name.
        /// </remarks>
        public string FontName
        {
            get { return this.fontName; }
            set { this.fontName = value; }
        }

        /// <summary>
        /// Gets or sets the text height as a multiple of the current text height.
        /// </summary>
        /// <remarks>Set as 1.0 to apply the default height factor.</remarks>
        public double HeightFactor
        {
            get { return this.heightFactor; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The character percentage height must be greater than zero.");
                this.heightFactor = value;
            }
        }

        /// <summary>
        /// Gets or sets the obliquing angle in degrees.
        /// </summary>
        /// <remarks>Set as 0.0 to apply the default obliquing angle.</remarks>
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
        ///  Gets or sets the space between characters as a multiple of the original spacing between characters.
        /// </summary>
        /// <remarks>
        /// Valid values range from a minimum of .75 to 4 times the original spacing between characters.
        /// Set as 1.0 to apply the default character space factor.
        /// </remarks>
        public double CharacterSpaceFactor
        {
            get { return this.characterSpaceFactor; }
            set
            {
                if (value < 0.75 || value > 4)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The character space valid values range from a minimum of .75 to 4");
                this.characterSpaceFactor = value;
            }
        }

        /// <summary>
        /// Gets or sets the width factor to produce wide text.
        /// </summary>
        /// <remarks>Set as 1.0 to apply the default width factor.</remarks>
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

        #endregion
    }
}