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
using System.Globalization;

namespace netDxf.Entities
{
    /// <summary>
    /// Options for the <see cref="MText">multiline text</see> entity paragraph formatting.
    /// </summary>
    public class MTextParagraphOptions
    {

        #region private fields

        private double heightFactor;
        private MTextParagraphAlignment alignment;
        private double spaceBefore ;
        private double spaceAfter;
        private double firstLineIndent;
        private double leftIndent;
        private double rightIndent;
        private double lineSpacing;
        private MTextLineSpacingStyle lineSpacingStyle;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>MTextParagraphOptions</c> class.
        /// </summary>
        public MTextParagraphOptions()
        {
            this.heightFactor = 1.0;
            this.alignment = MTextParagraphAlignment.Left;
            this.spaceBefore = 0.0;
            this.spaceAfter = 0.0;
            this.firstLineIndent = 0.0;
            this.leftIndent = 0.0;
            this.rightIndent = 0.0;
            this.lineSpacing = 1.0;
            this.lineSpacingStyle = MTextLineSpacingStyle.Default;
    }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the paragraph height factor.
        /// </summary>
        /// <remarks>
        /// Percentage of default paragraph height factor to be applied. Valid values range from 0.25 to 4.0, the default value 1.0.
        /// </remarks>
        public double HeightFactor
        {
            get { return this.heightFactor; }
            set
            {
                if (value < 0.25 || value > 4.0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The paragraph height factor valid values range from 0.25 to 4.0");
                this.heightFactor = value;
            }
        }

        /// <summary>
        /// Gets or sets the current paragraph justification (text horizontal alignment).
        /// </summary>
        public MTextParagraphAlignment Alignment
        {
            get { return this.alignment; }
            set { this.alignment = value; }
        }

        /// <summary>
        /// Specifies the spacing before the current paragraphs.
        /// </summary>
        /// <remarks>
        /// If set to zero no value will be applied and the default will be inherited. When it is non zero, valid values range from 0.25 to 4.0.<br />
        /// The distance between two paragraphs is determined by the total of the after paragraph spacing value of the upper paragraph
        /// and the before paragraph spacing value of the lower paragraph.
        /// </remarks>
        public double SpacingBefore
        {
            get { return this.spaceBefore; }
            set
            {
                if (MathHelper.IsZero(value))
                {
                    this.spaceBefore = 0.0;
                }
                else
                {
                    if (value < 0.25 || value > 4.0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The paragraph spacing valid values range from 0.25 to 4.0");
                    this.spaceBefore = value;
                }                
            }
        }

        /// <summary>
        /// Specifies the spacing before or after the current paragraph.
        /// </summary>
        /// <remarks>
        /// If set to zero no value will be applied and the default will be inherited. When it is non zero, valid values range from 0.25 to 4.0.<br />
        /// The distance between two paragraphs is determined by the total of the after paragraph spacing value of the upper paragraph
        /// and the before paragraph spacing value of the lower paragraph.
        /// </remarks>
        public double SpacingAfter
        {
            get { return this.spaceAfter; }
            set
            {
                if (MathHelper.IsZero(value))
                {
                    this.spaceAfter = 0.0;
                }
                else
                {
                    if (value < 0.25 || value > 4.0)
                        throw new ArgumentOutOfRangeException(nameof(value), value, "The paragraph spacing valid values range from 0.25 to 4.0");
                    this.spaceAfter = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the indent value for the first line of the current paragraph.
        /// </summary>
        /// <remarks>
        /// Valid values range from -10000.0 to 10000.0, the default value 0.0.<br />
        /// Negative first line indent values are limited by the left indent,
        /// in the case its absolute value is larger than the left indent, when applied to the paragraph it will be automatically adjusted .
        /// </remarks>
        public double FirstLineIndent
        {
            get { return this.firstLineIndent; }
            set
            {
                if (value < -10000.0 || value > 10000.0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The paragraph indent valid values range from -10000.0 to 10000.0");

                this.firstLineIndent = value;
            }
        }

        /// <summary>
        /// Gets or sets the left indent of the current paragraph.
        /// </summary>
        /// <remarks>
        /// Valid values range from 0.0 to 10000.0, the default value 0.0.
        /// </remarks>
        public double LeftIndent
        {
            get { return this.leftIndent; }
            set
            {
                if (value < 0.0 || value > 10000.0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The paragraph indent valid values range from 0.0 to 10000.0");
                this.leftIndent = value;
            }
        }

        /// <summary>
        /// Gets or sets the right indent value of the current paragraphs.
        /// </summary>
        /// <remarks>
        /// Valid values range from 0.0 to 10000.0, the default value 0.0.
        /// </remarks>
        public double RightIndent
        {
            get { return this.rightIndent; }
            set
            {
                if (value < 0.0 || value > 10000.0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The paragraph indent valid values range from 0.0 to 10000.0");
                this.rightIndent = value;
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
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The MText LineSpacingFactor valid values range from 0.25 to 4.0");
                this.lineSpacing = value;
            }
        }


        /// <summary>
        /// Get or sets the <see cref="MTextLineSpacingStyle">line spacing style</see>.
        /// </summary>
        public MTextLineSpacingStyle LineSpacingStyle
        {
            get { return this.lineSpacingStyle; }
            set { this.lineSpacingStyle = value; }
        }
        #endregion

        #region public methods

        /// <summary>
        /// Ends the actual paragraph (adds the end paragraph code and the paragraph height factor). 
        /// </summary>
        /// <param name="paragraph">Paragraph to be formatted.</param>
        /// <returns>The formatted paragraph string.</returns>
        public string FormatParagraph(string paragraph)
        {
            switch (this.alignment)
            {
                case MTextParagraphAlignment.Left:
                    paragraph = string.Format("\\pql;{0}", paragraph);
                    break;
                case MTextParagraphAlignment.Center:
                    paragraph = string.Format("\\pqc;{0}", paragraph);
                    break;
                case MTextParagraphAlignment.Right:
                    paragraph = string.Format("\\pqr;{0}", paragraph);
                    break;
                case MTextParagraphAlignment.Justified:
                    paragraph = string.Format("\\pqj;{0}", paragraph);
                    break;
                case MTextParagraphAlignment.Distribute:
                    paragraph = string.Format("\\pqd;{0}", paragraph);
                    break;
            }

            // when the first line indent is negative, it cannot be lower than the available space left by the left indent
            double fli = this.firstLineIndent;
            if (fli < 0.0 && Math.Abs(fli) > this.leftIndent)
                fli = -this.leftIndent;

            paragraph = string.Format("\\pi{0},l{1},r{2},b{3},a{4};{5}",
                fli.ToString(CultureInfo.InvariantCulture),
                this.leftIndent.ToString(CultureInfo.InvariantCulture),
                this.rightIndent.ToString(CultureInfo.InvariantCulture),
                this.spaceBefore.ToString(CultureInfo.InvariantCulture),
                this.spaceAfter.ToString(CultureInfo.InvariantCulture), 
                paragraph);

            switch (this.lineSpacingStyle)
            {
                case MTextLineSpacingStyle.Default:
                    paragraph = string.Format("\\ps*;{0}", paragraph);
                    break;
                case MTextLineSpacingStyle.AtLeast:
                    paragraph = string.Format("\\psa{0};{1}", this.lineSpacing.ToString(CultureInfo.InvariantCulture), paragraph);
                    break;
                case MTextLineSpacingStyle.Exact:
                    paragraph = string.Format("\\pse{0};{1}", this.lineSpacing.ToString(CultureInfo.InvariantCulture), paragraph);
                    break;
                case MTextLineSpacingStyle.Multiple:
                    paragraph = string.Format("\\psm{0};{1}", this.lineSpacing.ToString(CultureInfo.InvariantCulture), paragraph);
                    break;
            }

            return string.Format("\\A1\\H{0}x;{1}\\P", this.heightFactor.ToString(CultureInfo.InvariantCulture), paragraph);
        }

        #endregion
    }
}
