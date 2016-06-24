#region netDxf library, Copyright (C) 2009-2016 Daniel Carvajal (haplokuon@gmail.com)

//                        netDxf library
// Copyright (C) 2009-2016 Daniel Carvajal (haplokuon@gmail.com)
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
using System.Globalization;
using System.IO;
using System.Windows.Media;
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

        private readonly string font;
        private double height;
        private bool isBackward;
        private bool isUpsideDown;
        private bool isVertical;
        private double obliqueAngle;
        private double widthFactor;
        private readonly GlyphTypeface glyphTypeface;
        private readonly string fontFamilyName;

        #endregion

        #region constants

        /// <summary>
        /// Default text style name.
        /// </summary>
        public const string DefaultName = "Standard";

        /// <summary>
        /// Gets the default text style.
        /// </summary>
        public static TextStyle Default
        {
            get { return new TextStyle(DefaultName, "simplex.shx"); }
        }

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>TextStyle</c> class. The font file name, without the extension, will be used as the TextStyle name.
        /// </summary>
        /// <param name="font">Text style font file name with full or relative path.</param>
        /// <remarks>If the font file is a true type and is not found in the specified path, the constructor will try to find it in the system font folder.</remarks>
        public TextStyle(string font)
            : this(Path.GetFileNameWithoutExtension(font), font)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>TextStyle</c> class.
        /// </summary>
        /// <param name="name">Text style name.</param>
        /// <param name="font">Text style font file name with full or relative path.</param>
        /// <remarks>If the font file is a true type and is not found in the specified path, the constructor will try to find it in the system font folder.</remarks>
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
        /// <remarks>If the font file is a true type and is not found in the specified path, the constructor will try to find it in the system font folder.</remarks>
        internal TextStyle(string name, string font, bool checkName)
            : base(name, DxfObjectCode.TextStyle, checkName)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name), "The text style name should be at least one character long.");

            if (string.IsNullOrEmpty(font))
                throw new ArgumentNullException(nameof(font));

            this.IsReserved = name.Equals(DefaultName, StringComparison.OrdinalIgnoreCase);
            this.font = font;
            this.widthFactor = 1.0;
            this.obliqueAngle = 0.0;
            this.height = 0.0;
            this.isVertical = false;
            this.isBackward = false;
            this.isUpsideDown = false;
            this.glyphTypeface = null;
            this.fontFamilyName = Path.GetFileNameWithoutExtension(font);

            // the following information is only applied to ttf not shx fonts
            if (!Path.GetExtension(font).Equals(".ttf", StringComparison.OrdinalIgnoreCase))
                return;

            // try to find the file in the specified directory, if not try it in the fonts system folder
            string fontFile;
            if (File.Exists(font))
                fontFile = Path.GetFullPath(font);
            else
            {
                string file = Path.GetFileName(font);
                fontFile = string.Format("{0}{1}{2}", Environment.GetFolderPath(Environment.SpecialFolder.Fonts), Path.DirectorySeparatorChar, file);
                // if the ttf does not even exist in the font system folder 
                if (!File.Exists(fontFile))
                    return;
                this.font = file;
            }
            this.glyphTypeface = new GlyphTypeface(new Uri(fontFile));
            this.fontFamilyName = this.glyphTypeface.FamilyNames[CultureInfo.GetCultureInfo(1033)];
            if (string.IsNullOrEmpty(this.fontFamilyName))
            {
                ICollection<string> names = this.glyphTypeface.FamilyNames.Values;
                IEnumerator<string> enumerator = names.GetEnumerator();
                enumerator.MoveNext();
                this.fontFamilyName = enumerator.Current;
            }
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the text style font file name.
        /// </summary>
        public string FontFile
        {
            get { return this.font; }
        }

        /// <summary>
        /// Gets the style font family name.
        /// </summary>
        public string FontFamilyName
        {
            get { return this.fontFamilyName; }
        }

        /// <summary>
        /// Gets the GliphTypface associated with the font file, this is only applicable to true type fonts.
        /// </summary>
        public GlyphTypeface GlyphTypeface
        {
            get { return this.glyphTypeface; }
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
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The TextStyle height must be equals or greater than zero.");
                this.height = value;
            }
        }

        /// <summary>
        /// Gets or sets the width factor.
        /// </summary>
        /// <remarks>Valid values range from 0.01 to 100. Default: 1.0.</remarks>
        public double WidthFactor
        {
            get { return this.widthFactor; }
            set
            {
                if (value < 0.01 || value > 100.0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The TextStyle width factor valid values range from 0.01 to 100.");
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
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The TextStyle oblique angle valid values range from -85 to 85.");
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
        /// Gets the owner of the actual dxf object.
        /// </summary>
        public new TextStyles Owner
        {
            get { return (TextStyles) base.Owner; }
            internal set { base.Owner = value; }
        }

        #endregion

        #region overrides

        /// <summary>
        /// Creates a new TextStyle that is a copy of the current instance.
        /// </summary>
        /// <param name="newName">TextStyle name of the copy.</param>
        /// <returns>A new TextStyle that is a copy of this instance.</returns>
        public override TableObject Clone(string newName)
        {
            return new TextStyle(newName, this.font)
            {
                Height = this.height,
                IsBackward = this.isBackward,
                IsUpsideDown = this.isUpsideDown,
                IsVertical = this.isVertical,
                ObliqueAngle = this.obliqueAngle,
                WidthFactor = this.widthFactor
            };
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