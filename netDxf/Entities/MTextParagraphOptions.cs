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

namespace netDxf.Entities
{
    /// <summary>
    /// Options for the <see cref="MText">multiline text</see> entity paragraph formatting.
    /// </summary>
    /// <remarks>Old DXF versions might not support all available formatting codes.</remarks>
    public class MTextParagraphOptions
    {
        #region private fields

        private double heightFactor;
        private MTextParagraphAlignment alignment;
        private MTextParagraphVerticalAlignment verticalAlignment;
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
            this.verticalAlignment = MTextParagraphVerticalAlignment.Center;
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
        /// Gets or sets the paragraph justification (text horizontal alignment).
        /// </summary>
        public MTextParagraphAlignment Alignment
        {
            get { return this.alignment; }
            set { this.alignment = value; }
        }

        /// <summary>
        /// Gets or sets the paragraph line vertical alignment.
        /// </summary>
        /// <remarks>
        /// The vertical alignment affects how fractions, superscripts, subscripts, and characters of different heights are placed in a paragraph line.
        /// By default the paragraph vertical alignment is Center.
        /// </remarks>
        public MTextParagraphVerticalAlignment VerticalAlignment
        {
            get { return this.verticalAlignment; }
            set { this.verticalAlignment = value; }
        }

        /// <summary>
        /// Specifies the spacing before the paragraphs.
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
        /// Specifies the spacing before or after the paragraph.
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
        /// Gets or sets the indent value for the first line of the paragraph.
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
        /// Gets or sets the right indent value of the paragraphs.
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
        /// Gets or sets the paragraph line spacing factor.
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
        /// Get or sets the paragraph <see cref="MTextLineSpacingStyle">line spacing style</see>.
        /// </summary>
        public MTextLineSpacingStyle LineSpacingStyle
        {
            get { return this.lineSpacingStyle; }
            set { this.lineSpacingStyle = value; }
        }

        #endregion
    }
}