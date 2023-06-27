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
using System.IO;
using netDxf.Collections;

namespace netDxf.Tables
{
    /// <summary>
    /// Represents a text style.
    /// </summary>
    public class TextStyle :
        TableObject
    {
        #region private fields

        private string file;
        private string bigFont;
        private double height;
        private bool isBackward;
        private bool isUpsideDown;
        private bool isVertical;
        private double obliqueAngle;
        private double widthFactor;
        private FontStyle fontStyle;
        private string fontFamilyName;

        #endregion

        #region constants

        /// <summary>
        /// Default text style name.
        /// </summary>
        public const string DefaultName = "Standard";

        /// <summary>
        /// Default text style font.
        /// </summary>
        public const string DefaultFont = "simplex.shx";

        /// <summary>
        /// Gets the default text style.
        /// </summary>
        public static TextStyle Default
        {
            get { return new TextStyle(DefaultName, DefaultFont); }
        }

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>TextStyle</c> class.
        /// </summary>
        /// <param name="name">Text style name.</param>
        /// <param name="font">Text style font file name with full or relative path.</param>
        public TextStyle(string name, string font)
            : this(name, font, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>TextStyle</c> class.
        /// </summary>
        /// <param name="name">Text style name.</param>
        /// <param name="font">Text style font file name with full or relative path.</param>
        /// <param name="checkName">Specifies if the style name has to be checked.</param>
        internal TextStyle(string name, string font, bool checkName)
            : base(name, DxfObjectCode.TextStyle, checkName)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name), "The text style name should be at least one character long.");
            }

            if (string.IsNullOrEmpty(font))
            {
                throw new ArgumentNullException(nameof(font));
            }

            if (!Path.GetExtension(font).Equals(".TTF", StringComparison.OrdinalIgnoreCase) &&
                !Path.GetExtension(font).Equals(".SHX", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Only true type TTF fonts and ACAD compiled shape SHX fonts are allowed.");
            }

            this.IsReserved = name.Equals(DefaultName, StringComparison.OrdinalIgnoreCase);
            this.file = font;
            this.bigFont = string.Empty;
            this.widthFactor = 1.0;
            this.obliqueAngle = 0.0;
            this.height = 0.0;
            this.isVertical = false;
            this.isBackward = false;
            this.isUpsideDown = false;
            this.fontFamilyName = string.Empty;
            this.fontStyle = FontStyle.Regular;
        }

        /// <summary>
        /// Initializes a new instance of the <c>TextStyle</c> class exclusively to be used with true type fonts.
        /// </summary>
        /// <param name="name">Text style name.</param>
        /// <param name="fontFamily">True type font family name.</param>
        /// <param name="fontStyle">True type font style.</param>
        /// <remarks>This constructor is to be use only with true type fonts.</remarks>
        public TextStyle(string name, string fontFamily, FontStyle fontStyle)
            : this(name, fontFamily, fontStyle, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>TextStyle</c> class exclusively to be used with true type fonts.
        /// </summary>
        /// <param name="name">Text style name.</param>
        /// <param name="fontFamily">True type font family name.</param>
        /// <param name="fontStyle">True type font style</param>
        /// <param name="checkName">Specifies if the style name has to be checked.</param>
        /// <remarks>This constructor is to be use only with true type fonts.</remarks>
        internal TextStyle(string name, string fontFamily, FontStyle fontStyle, bool checkName)
            : base(name, DxfObjectCode.TextStyle, checkName)
        {
            this.file = string.Empty;
            this.bigFont = string.Empty;
            this.widthFactor = 1.0;
            this.obliqueAngle = 0.0;
            this.height = 0.0;
            this.isVertical = false;
            this.isBackward = false;
            this.isUpsideDown = false;
            if (string.IsNullOrEmpty(fontFamily))
            {
                throw new ArgumentNullException(nameof(fontFamily));
            }
            this.fontFamilyName = fontFamily;
            this.fontStyle = fontStyle;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the text style font file name.
        /// </summary>
        /// <remarks>
        /// When this value is used for true type fonts should be present in the Font system folder.<br />
        /// When the style does not contain any information for the file the font information will be saved in the extended data when saved to a DXF,
        /// this is only applicable for true type fonts.
        /// </remarks>
        public string FontFile
        {
            get { return this.file; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException(nameof(value));
                }

                if (!Path.GetExtension(value).Equals(".TTF", StringComparison.OrdinalIgnoreCase) &&
                    !Path.GetExtension(value).Equals(".SHX", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException("Only true type TTF fonts and ACAD compiled shape SHX fonts are allowed.");
                }

                this.fontFamilyName = string.Empty;
                this.bigFont = string.Empty;
                this.fontStyle = FontStyle.Regular;
                this.file = value;
            }
        }

        /// <summary>
        /// Gets or sets an Asian-language Big Font file.
        /// </summary>
        /// <remarks>Only ACAD compiled shape SHX fonts are valid for creating Big Fonts.</remarks>
        public string BigFont
        {
            get { return this.bigFont; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.bigFont = string.Empty;
                }
                else
                {
                    if (string.IsNullOrEmpty(this.file))
                    {
                        throw new NullReferenceException("The Big Font is only applicable for SHX Asian fonts.");
                    }

                    if (!Path.GetExtension(this.file).Equals(".SHX", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new NullReferenceException("The Big Font is only applicable for SHX Asian fonts.");
                    }

                    if (!Path.GetExtension(value).Equals(".SHX", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new ArgumentException("The Big Font is only applicable for SHX Asian fonts.", nameof(value));
                    }

                    this.bigFont = value;
                }               
            }
        }

        /// <summary>
        /// Gets or sets the true type font family name.
        /// </summary>
        /// <remarks>
        /// When the font family name is manually specified the file font will not be used and it will be set to empty,
        /// the font style will also we set to FontStyle.Regular.
        /// In this case the font information will be stored in the style extended data when saved to a DXF.<br />
        /// This value is only applicable for true type fonts.
        /// </remarks>
        public string FontFamilyName
        {
            get { return this.fontFamilyName; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException(nameof(value));
                }
                this.file = string.Empty;
                this.bigFont = string.Empty;
                this.fontStyle = FontStyle.Regular;
                this.fontFamilyName = value;
            }
        }

        /// <summary>
        /// Gets or sets the true type font style.
        /// </summary>
        /// <remarks>
        /// The font style value is ignored and will always return FontStyle.Regular when a font file has been specified.<br />
        /// All styles may not be available for the current font family.
        /// </remarks>
        public FontStyle FontStyle
        {
            get { return this.fontStyle; }
            set
            {
                if (string.IsNullOrEmpty(this.file))
                {
                    this.fontStyle = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the text height.
        /// </summary>
        /// <remarks>Fixed text height; 0 if not fixed.</remarks>
        public double Height
        {
            get { return this.height; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The TextStyle height must be equals or greater than zero.");
                }
                this.height = value;
            }
        }

        /// <summary>
        /// Gets or sets the text width factor.
        /// </summary>
        /// <remarks>Valid values range from 0.01 to 100. Default: 1.0.</remarks>
        public double WidthFactor
        {
            get { return this.widthFactor; }
            set
            {
                if (value < 0.01 || value > 100.0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The TextStyle width factor valid values range from 0.01 to 100.");
                }
                this.widthFactor = value;
            }
        }

        /// <summary>
        /// Gets or sets the font oblique angle in degrees.
        /// </summary>
        /// <remarks>Valid values range from -85 to 85. Default: 0.0.</remarks>
        public double ObliqueAngle
        {
            get { return this.obliqueAngle; }
            set
            {
                if (value < -85.0 || value > 85.0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The TextStyle oblique angle valid values range from -85 to 85.");
                }
                this.obliqueAngle = value;
            }
        }

        /// <summary>
        /// Gets or sets the text is vertical.
        /// </summary>
        public bool IsVertical
        {
            get { return this.isVertical; }
            set { this.isVertical = value; }
        }

        /// <summary>
        /// Gets or sets if the text is backward (mirrored in X).
        /// </summary>
        public bool IsBackward
        {
            get { return this.isBackward; }
            set { this.isBackward = value; }
        }

        /// <summary>
        /// Gets or sets if the text is upside down (mirrored in Y).
        /// </summary>
        public bool IsUpsideDown
        {
            get { return this.isUpsideDown; }
            set { this.isUpsideDown = value; }
        }

        /// <summary>
        /// Gets the owner of the actual text style.
        /// </summary>
        public new TextStyles Owner
        {
            get { return (TextStyles) base.Owner; }
            internal set { base.Owner = value; }
        }

        #endregion

        #region public methods

#if NET4X

        /// <summary>
        /// Find the font family name of an specified TTF font file. Only available for Net Framework 4.5 builds.
        /// </summary>
        /// <param name="ttfFont">TTF font file.</param>
        /// <returns>The font family name of the specified TTF font file.</returns>
        /// <remarks>This method will return an empty string if the specified font is not found in its path or the system font folder or if it is not a valid TTF font.</remarks>
        public static string TrueTypeFontFamilyName(string ttfFont)
        {
            if (string.IsNullOrEmpty(ttfFont))
            {
                throw new ArgumentNullException(nameof(ttfFont));
            }

            // the following information is only applied to TTF not SHX fonts
            if (!Path.GetExtension(ttfFont).Equals(".TTF", StringComparison.InvariantCultureIgnoreCase))
            {
                return string.Empty;
            }

            // try to find the file in the specified directory, if not try it in the fonts system folder
            string fontFile = File.Exists(ttfFont) ?
                Path.GetFullPath(ttfFont) :
                string.Format("{0}{1}{2}", Environment.GetFolderPath(Environment.SpecialFolder.Fonts), Path.DirectorySeparatorChar, Path.GetFileName(ttfFont));

            System.Drawing.Text.PrivateFontCollection fontCollection = new System.Drawing.Text.PrivateFontCollection();
            try
            {
                fontCollection.AddFontFile(fontFile);
                return fontCollection.Families[0].Name;
            }
            catch (FileNotFoundException)
            {
                return string.Empty;
            }
            finally
            {
                fontCollection.Dispose();
            }
        }

#endif

        #endregion

        #region overrides

        /// <summary>
        /// Checks if this instance has been referenced by other DxfObjects. 
        /// </summary>
        /// <returns>
        /// Returns true if this instance has been referenced by other DxfObjects, false otherwise.
        /// It will always return false if this instance does not belong to a document.
        /// </returns>
        /// <remarks>
        /// This method returns the same value as the HasReferences method that can be found in the TableObjects class.
        /// </remarks>
        public override bool HasReferences()
        {
            return this.Owner != null && this.Owner.HasReferences(this.Name);
        }

        /// <summary>
        /// Gets the list of DxfObjects referenced by this instance.
        /// </summary>
        /// <returns>
        /// A list of DxfObjectReference that contains the DxfObject referenced by this instance and the number of times it does.
        /// It will return null if this instance does not belong to a document.
        /// </returns>
        /// <remarks>
        /// This method returns the same list as the GetReferences method that can be found in the TableObjects class.
        /// </remarks>
        public override List<DxfObjectReference> GetReferences()
        {
            return this.Owner?.GetReferences(this.Name);
        }

        /// <summary>
        /// Creates a new TextStyle that is a copy of the current instance.
        /// </summary>
        /// <param name="newName">TextStyle name of the copy.</param>
        /// <returns>A new TextStyle that is a copy of this instance.</returns>
        public override TableObject Clone(string newName)
        {
            TextStyle copy;

            if (string.IsNullOrEmpty(this.FontFamilyName))
            {
                copy = new TextStyle(newName, this.file)
                {
                    Height = this.height,
                    IsBackward = this.isBackward,
                    IsUpsideDown = this.isUpsideDown,
                    IsVertical = this.isVertical,
                    ObliqueAngle = this.obliqueAngle,
                    WidthFactor = this.widthFactor
                };
            }
            else
            {
                copy = new TextStyle(newName, this.fontFamilyName, this.fontStyle)
                {
                    Height = this.height,
                    IsBackward = this.isBackward,
                    IsUpsideDown = this.isUpsideDown,
                    IsVertical = this.isVertical,
                    ObliqueAngle = this.obliqueAngle,
                    WidthFactor = this.widthFactor
                };
            }

            foreach (XData data in this.XData.Values)
            {
                copy.XData.Add((XData)data.Clone());
            }

            return copy;
        }

        /// <summary>
        /// Creates a new TextStyle that is a copy of the current instance.
        /// </summary>
        /// <returns>A new TextStyle that is a copy of this instance.</returns>
        public override object Clone()
        {
            return this.Clone(this.Name);
        }

        #endregion
    }
}
