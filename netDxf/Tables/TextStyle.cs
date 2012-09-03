#region netDxf, Copyright(C) 2009 Daniel Carvajal, Licensed under LGPL.

//                        netDxf library
// Copyright (C) 2009 Daniel Carvajal (haplokuon@gmail.com)
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

namespace netDxf.Tables
{
    /// <summary>
    /// Represents a text style.
    /// </summary>
    /// <remarks>
    /// AutoCad12 does not support true type fonts.
    /// </remarks>
    public class TextStyle :
        DxfObject,
        ITableObject
    {
        #region private fields

        private readonly string font;
        private readonly string name;
        private double height;
        private bool isBackward;
        private bool isUpsideDown;
        private bool isVertical;
        private double obliqueAngle;
        private double widthFactor;

        #endregion

        #region constants

        /// <summary>
        /// Gets the default text style.
        /// </summary>
        public static TextStyle Default
        {
            get { return new TextStyle("Standard", "simplex"); }
        }

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>TextStyle</c> class.
        /// </summary>
        /// <param name="name">Text style name.</param>
        /// <param name="font">Text style font name.</param>
        public TextStyle(string name, string font)
            : base(DxfObjectCode.TextStyle)
        {
            if (string.IsNullOrEmpty(name))
                throw (new ArgumentNullException("name"));
            this.name = name;
            if (string.IsNullOrEmpty(font))
                font = "simplex";
            this.font = font;
            this.widthFactor = 1.0f;
            this.obliqueAngle = 0.0f;
            this.height = 0.0f;
            this.isVertical = false;
            this.isBackward = false;
            this.isUpsideDown = false;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the text style font name.
        /// </summary>
        public string Font
        {
            get{return this.font;}
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
                    throw (new ArgumentOutOfRangeException("value", value,"The height can not be less than zero."));
                this.height = value;
            }
        }

        /// <summary>
        /// Gets or sets the width factor.
        /// </summary>
        public double WidthFactor
        {
            get { return this.widthFactor; }
            set
            {
                if (value <= 0)
                    throw (new ArgumentOutOfRangeException("value", value, "The width factor should be greater than zero."));
                this.widthFactor = value;
            }
        }

        /// <summary>
        /// Gets or sets the font oblique angle.
        /// </summary>
        public double ObliqueAngle
        {
            get { return this.obliqueAngle; }
            set { this.obliqueAngle = value; }
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

        #endregion

        #region ITableObject Members

        /// <summary>
        /// Gets the table name.
        /// </summary>
        public string Name
        {
            get { return this.name; }
        }

        #endregion

        #region overrides

        /// <summary>
        /// Converts the value of this instance to its equivalent string representation.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            return this.name;
        }

        #endregion
    }
}