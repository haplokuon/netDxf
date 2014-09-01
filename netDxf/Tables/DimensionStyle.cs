#region netDxf, Copyright(C) 2014 Daniel Carvajal, Licensed under LGPL.

//                        netDxf library
// Copyright (C) 2014 Daniel Carvajal (haplokuon@gmail.com)
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
using netDxf.Collections;

namespace netDxf.Tables
{
    /// <summary>
    /// Represents as dimension style.
    /// </summary>
    public class DimensionStyle :
        TableObject
    {
        #region private fields

        private TextStyle textStyle;

        // lines
        private double dimexo;
        private double dimexe;

        // symbols and arrows
        private double dimasz;
        private double dimcen;

        // text
        private double dimtxt = 0.18;
        private short dimjust;
        private short dimtad;
        private double dimgap;
        private short dimadec;
        private short dimdec;
        private string dimpost;
        private short dimtih;
        private short dimtoh;
        private char dimdsep;
        private short dimaunit;

        #endregion

        #region constants

        /// <summary>
        /// Gets the default dimension style.
        /// </summary>
        public static DimensionStyle Default
        {
            get { return new DimensionStyle("Standard"); }
        }

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>DimensionStyle</c> class.
        /// </summary>
        /// <param name="name">The dimension style name.</param>
        public DimensionStyle(string name)
            : base(name, DxfObjectCode.DimStyle, true)
        {
            this.reserved = name.Equals("Standard", StringComparison.OrdinalIgnoreCase);
            this.textStyle = TextStyle.Default;

            this.dimexo = 0.0625;
            this.dimexe = 0.18;

            // symbols and arrows
            this.dimasz = 0.18;
            this.dimcen = 0.09;

            // text
            this.dimtxt = 0.18;
            this.dimjust = 0;
            this.dimtad = 1;
            this.dimgap = 0.09;
            this.dimadec = 0;
            this.dimdec = 2;
            this.dimpost = "<>";
            this.dimtih = 0;
            this.dimtoh = 0;
            this.dimdsep = '.';
            this.dimaunit = 0;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the text style of the dimension.
        /// </summary>
        public TextStyle TextStyle
        {
            get { return this.textStyle; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                this.textStyle = value;
            }
        }

        /// <summary>
        /// Sets the distance around the dimension text when the dimension line breaks to accommodate dimension text.
        /// </summary>
        public double DIMGAP
        {
            get { return this.dimgap; }
            set { this.dimgap = value; }
        }

        /// <summary>
        /// Specifies how far extension lines are offset from origin points.
        /// </summary>
        public double DIMEXO
        {
            get { return this.dimexo; }
            set { this.dimexo = value; }
        }

        /// <summary>
        /// Specifies how far to extend the extension line beyond the dimension line.
        /// </summary>
        public double DIMEXE
        {
            get { return this.dimexe; }
            set { this.dimexe = value; }
        }

        /// <summary>
        /// Controls the size of dimension line and leader line arrowheads. Also controls the size of hook lines.
        /// </summary>
        public double DIMASZ
        {
            get { return this.dimasz; }
            set { this.dimasz = value; }
        }

        /// <summary>
        /// Specifies the height of dimension text, unless the current text style has a fixed height.
        /// </summary>
        public double DIMTXT
        {
            get { return this.dimtxt; }
            set { this.dimtxt = value; }
        }

        /// <summary>
        /// Controls the horizontal positioning of dimension text.
        /// </summary>
        public short DIMJUST
        {
            get { return this.dimjust; }
            set { this.dimjust = value; }
        }

        /// <summary>
        /// Controls the vertical position of text in relation to the dimension line.
        /// </summary>
        public short DIMTAD
        {
            get { return this.dimtad; }
            set { this.dimtad = value; }
        }

        /// <summary>
        /// Sets the number of decimal places displayed for the primary units of a dimension.
        /// </summary>
        public short DIMDEC
        {
            get { return this.dimdec; }
            set { this.dimdec = value; }
        }

        /// <summary>
        /// Controls drawing of circle or arc center marks and centerlines.
        /// </summary>
        /// <remarks>
        /// 0 - No center marks or lines are drawn.<br />
        /// greater than 0 - Centerlines are drawn.<br />
        /// lower than 0 - Center marks are drawn.<br />
        /// The absolute value specifies the size of the center mark or centerline. 
        /// The size of the centerline is the length of the centerline segment that extends outside the circle or arc.
        /// It is also the size of the gap between the center mark and the start of the centerline. 
        /// The size of the center mark is the distance from the center of the circle or arc to the end of the center mark. 
        /// </remarks>
        public double DIMCEN
        {
            get { return this.dimcen; }
            set { this.dimcen = value; }
        }

        /// <summary>
        /// Controls the number of precision places displayed in angular dimensions.
        /// </summary>
        public short DIMADEC
        {
            get { return this.dimadec; }
            set { this.dimadec = value; }
        }

        /// <summary>
        /// Specifies a text prefix or suffix (or both) to the dimension measurement.
        /// </summary>
        /// <remarks>
        /// Use "&lt;&gt;" to indicate placement of the text in relation to the dimension value. 
        /// For example, enter "&lt;&gt;mm" to display a 5.0 millimeter radial dimension as "5.0mm". 
        /// If you entered "mm &lt;&gt;", the dimension would be displayed as "mm 5.0".
        /// Use the "&lt;&gt;" mechanism for angular dimensions.
        /// </remarks>
        public string DIMPOST
        {
            get { return this.dimpost; }
            set { this.dimpost = value; }
        }

        /// <summary>
        /// Controls the position of dimension text inside the extension lines for all dimension types except Ordinate.
        /// </summary>
        public short DIMTIH
        {
            get { return this.dimtih; }
            set { this.dimtih = value; }
        }

        /// <summary>
        /// Controls the position of dimension text outside the extension lines.
        /// </summary>
        public short DIMTOH
        {
            get { return this.dimtoh; }
            set { this.dimtoh = value; }
        }

        /// <summary>
        /// Specifies a single-character decimal separator to use when creating dimensions whose unit format is decimal.
        /// </summary>
        public char DIMDSEP
        {
            get { return this.dimdsep; }
            set { this.dimdsep = value; }
        }

        /// <summary>
        /// Gets the units format for angular dimensions.
        /// </summary>
        /// <remarks>
        /// 0 Decimal degrees
        /// 1 Degrees/minutes/seconds
        /// 2 Gradians
        /// 3 Radians
        /// </remarks>
        public short DIMAUNIT
        {
            get { return this.dimaunit; }
            set { this.dimaunit = value; }
        }

        /// <summary>
        /// Gets the owner of the actual dxf object.
        /// </summary>
        public new DimensionStyles Owner
        {
            get { return (DimensionStyles) this.owner; }
            internal set { this.owner = value; }
        }

        #endregion

        #region overrides

        /// <summary>
        /// Creates a new DimensionStyle that is a copy of the current instance.
        /// </summary>
        /// <param name="newName">DimensionStyle name of the copy.</param>
        /// <returns>A new DimensionStyle that is a copy of this instance.</returns>
        public override TableObject Clone(string newName)
        {
            return new DimensionStyle(newName)
            {
                TextStyle = (TextStyle)this.textStyle.Clone(),
                DIMEXO = this.dimexo,
                DIMEXE = this.dimexe,

                // symbols and arrows
                DIMASZ = this.dimasz,
                DIMCEN = this.dimcen,

                // text
                DIMTXT = this.dimtxt,
                DIMJUST = this.dimjust,
                DIMTAD = this.dimtad,
                DIMGAP = this.dimgap,
                DIMADEC = this.dimadec,
                DIMDEC = this.dimdec,
                DIMPOST = this.dimpost,
                DIMTIH = this.dimtih,
                DIMTOH = this.dimtoh,
                DIMDSEP = this.dimdsep,
                DIMAUNIT = this.dimaunit
            };

        }

        /// <summary>
        /// Creates a new DimensionStyle that is a copy of the current instance.
        /// </summary>
        /// <returns>A new DimensionStyle that is a copy of this instance.</returns>
        public override object Clone()
        {
            return Clone(this.name);
        }

        #endregion
    }
}