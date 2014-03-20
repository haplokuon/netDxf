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
using netDxf.Tables;

namespace netDxf.Entities
{
    /// <summary>
    /// Options for the <see cref="MText">multiline text</see> entity text formatting.
    /// </summary>
    public class MTextFormattingOptions
    {
        /// <summary>
        /// Text alligment options.
        /// </summary>
        public enum TextAligment
        {
            /// <summary>
            /// Bottom.
            /// </summary>
            Bottom = 0,
            /// <summary>
            /// Center.
            /// </summary>
            Center = 1,
            /// <summary>
            /// Top.
            /// </summary>
            Top = 2,
            /// <summary>
            /// Current value (no changes).
            /// </summary>
            Default = 3
        }

        #region private fields

        private bool bold = false;
        private bool italic = false;
        private bool overline = false;
        private bool underline = false;
        private AciColor color = null;
        private string fontName = null;
        private TextAligment aligment = TextAligment.Default;
        private double heightFactor = 1.0;
        private double obliqueAngle = 0.0;
        private double characterSpaceFactor = 1.0;
        private double widthFactor = 1.0;
        private readonly TextStyle style;

        #endregion

        #region constructors
        /// <summary>
        /// Initializes a new instance of the <c>MTextFormattingOptions</c> class
        /// </summary>
        /// <param name="style">Current style of the <see cref="MText">multiline text</see> entity.</param>
        public MTextFormattingOptions(TextStyle style)
        {
            this.bold = false;
            this.italic = false;
            this.overline = false;
            this.underline = false;
            this.color = null;
            this.fontName = null;
            this.aligment = TextAligment.Default;
            this.heightFactor = 1.0;
            this.obliqueAngle = 0.0;
            this.characterSpaceFactor = 1.0;
            this.widthFactor = 1.0;
            this.style = style;
        }
        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets if the text is bold.
        /// </summary>
        /// <remarks>The style font must support bold characters.</remarks>
        public bool Bold
        {
            get { return bold; }
            set { bold = value; }
        }

        /// <summary>
        /// Gets or sets if the text is italic.
        /// </summary>
        /// <remarks>The style font must support italic characters.</remarks>
        public bool Italic
        {
            get { return italic; }
            set { italic = value; }
        }

        /// <summary>
        /// Gets or sets the overline.
        /// </summary>
        public bool Overline
        {
            get { return overline; }
            set { overline = value; }
        }

        /// <summary>
        /// Gets or sets underline.
        /// </summary>
        public bool Underline
        {
            get { return underline; }
            set { underline = value; }
        }

        /// <summary>
        /// Gets or sets the text color.
        /// </summary>
        /// <remarks>Set as null to apply the default color.</remarks>
        public AciColor Color
        {
            get { return color; }
            set { color = value; }
        }

        /// <summary>
        /// Gets or sets the font file name (.ttf fonts without the extension).
        /// </summary>
        /// <remarks>Set as null to apply the default font.</remarks>
        public string FontName
        {
            get { return fontName; }
            set { fontName = value; }
        }

        /// <summary>
        /// Gets or sets the text alignment.
        /// </summary>
        public TextAligment Aligment
        {
            get { return aligment; }
            set { aligment = value; }
        }

        /// <summary>
        /// Gets or sets the text height as a multiple of the current text height.
        /// </summary>
        /// <remarks>Set as 1.0 to apply the default height factor.</remarks>
        public double HeightFactor
        {
            get { return heightFactor; }
            set
            {
                if(value<=0)
                    throw new ArgumentOutOfRangeException("value",value, "The character percentage height must be greater than 0.");
                heightFactor = value;
            }
        }

        /// <summary>
        /// Gets or sets the obliquing angle in degrees.
        /// </summary>
        /// <remarks>Set as 0.0 to apply the default obliquing angle.</remarks>
        public double ObliqueAngle
        {
            get { return obliqueAngle; }
            set { obliqueAngle = MathHelper.NormalizeAngle(value); }
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
            get { return characterSpaceFactor; }
            set
            {
                if(value < 0.75 || value > 4)
                    throw new ArgumentOutOfRangeException("value", value, "The character space valid values range from a minimum of .75 to 4");
                characterSpaceFactor = value;
            }
        }

        /// <summary>
        /// Gets or sets the width factor to produce wide text.
        /// </summary>
        /// <remarks>Set as 1.0 to apply the default width factor.</remarks>
        public double WidthFactor
        {
            get { return widthFactor; }
            set { widthFactor = value; }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Obtains the string that represents the formatted text appling the current options.
        /// </summary>
        /// <param name="text">Text to be formatted.</param>
        /// <returns>The formatted text string.</returns>
        public string FormatText(string text)
        {
            string formattedText = text;
            if(overline)
                formattedText = string.Format("\\O{0}\\o", formattedText);  
            if(underline)
                formattedText = string.Format("\\L{0}\\l", formattedText);  
            if(color != null)
                formattedText = string.Format("\\C{0};{1}", color.Index, formattedText);
            if(fontName != null)
            {
                if (bold && italic)
                    formattedText = string.Format("\\f{0}|b1|i1;{1}", fontName, formattedText);
                else if (bold && !italic)
                    formattedText = string.Format("\\f{0}|b1|i0;{1}", fontName, formattedText);
                else if (!bold && italic)
                    formattedText = string.Format("\\f{0}|i1|b0;{1}", fontName, formattedText);
                else
                    formattedText = string.Format("\\F{0};{1}", fontName, formattedText);
            }
            else
            {
                if (bold && italic)
                    formattedText = string.Format("\\f{0}|b1|i1;{1}", style.FontNameWithoutExtension, formattedText);
                if (bold && !italic)
                    formattedText = string.Format("\\f{0}|b1|i0;{1}", style.FontNameWithoutExtension, formattedText);
                if (!bold && italic)
                    formattedText = string.Format("\\f{0}|i1|b0;{1}", style.FontNameWithoutExtension, formattedText);
            }    
            if(aligment != TextAligment.Default)
                formattedText = string.Format("\\A{0};{1}", (int)aligment, formattedText);
            if(!MathHelper.IsOne(heightFactor))
                formattedText = string.Format("\\H{0}x;{1}", heightFactor, formattedText);
            if (!MathHelper.IsZero(obliqueAngle))
                formattedText = string.Format("\\Q{0};{1}", obliqueAngle, formattedText);
            if (!MathHelper.IsOne(characterSpaceFactor))
                formattedText = string.Format("\\T{0};{1}", characterSpaceFactor, formattedText);
            if (!MathHelper.IsOne(widthFactor))
                formattedText = string.Format("\\W{0};{1}", widthFactor, formattedText);
            return "{" + formattedText + "}";
        }

        #endregion

    }
}
