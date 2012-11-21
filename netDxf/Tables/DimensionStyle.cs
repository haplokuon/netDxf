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

namespace netDxf.Tables
{
    /// <summary>
    /// Represents as dimension style.
    /// </summary>
    public class DimensionStyle :
        DxfObject,
        ITableObject
    {

        #region private fields

        private readonly string name;
        private TextStyle textStyle;

        // lines
        private double dimexo = 0.0625;
        private double dimexe = 0.18;

        // symbols and arrows
        private double dimasz = 0.18;

        // text
        private double dimtxt = 0.18;
        private int dimjust = 0;
        private int dimtad = 1;
        private double dimgap = 0.09;
        private int dimdec = 2;
        private string dimpost = "<>";
        private int dimtih = 0;
        private int dimtoh = 0;
        #endregion

        #region constants

        internal static DimensionStyle Default
        {
            get { return new DimensionStyle("Standard"); }
        }

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>DimensionStyle</c> class.
        /// </summary>
        public DimensionStyle(string name)
            : base(DxfObjectCode.DimStyle)
        {
            this.name = name;
            this.textStyle = TextStyle.Default;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the text style of the dimension.
        /// </summary>
        public TextStyle TextStyle
        {
            get { return textStyle; }
            set { textStyle = value; }
        }

        /// <summary>
        /// Sets the distance around the dimension text when the dimension line breaks to accommodate dimension text.
        /// </summary>
        public double DIMGAP
        {
            get { return dimgap; }
            set { dimgap = value; }
        }

        /// <summary>
        /// Specifies how far extension lines are offset from origin points.
        /// </summary>
        public double DIMEXO
        {
            get { return dimexo; }
            set { dimexo = value; }
        }

        /// <summary>
        /// Specifies how far to extend the extension line beyond the dimension line.
        /// </summary>
        public double DIMEXE
        {
            get { return dimexe; }
            set { dimexe = value; }
        }

        /// <summary>
        /// Controls the size of dimension line and leader line arrowheads. Also controls the size of hook lines.
        /// </summary>
        public double DIMASZ
        {
            get { return dimasz; }
            set { dimasz = value; }
        }

        /// <summary>
        /// Specifies the height of dimension text, unless the current text style has a fixed height.
        /// </summary>
        public double DIMTXT
        {
            get { return dimtxt; }
            set { dimtxt = value; }
        }

        /// <summary>
        /// Controls the horizontal positioning of dimension text.
        /// </summary>
        public int DIMJUST
        {
            get { return dimjust; }
            set { dimjust = value; }
        }

        /// <summary>
        /// Controls the vertical position of text in relation to the dimension line.
        /// </summary>
        public int DIMTAD
        {
            get { return dimtad; }
            set { dimtad = value; }
        }

        /// <summary>
        /// Sets the number of decimal places displayed for the primary units of a dimension.
        /// </summary>
        public int DIMDEC
        {
            get { return dimdec; }
            set { dimdec = value; }
        }

        /// <summary>
        /// Specifies a text prefix or suffix (or both) to the dimension measurement.
        /// </summary>
        /// <remarks>
        /// Use <> to indicate placement of the text in relation to the dimension value. 
        /// For example, enter <> mm to display a 5.0 millimeter radial dimension as "5.0mm". 
        /// If you entered mm <>, the dimension would be displayed as "mm 5.0". Use the <> mechanism for angular dimensions.
        /// </remarks>
        public string DIMPOST
        {
            get { return dimpost; }
            set { dimpost = value; }
        }

        /// <summary>
        /// Controls the position of dimension text inside the extension lines for all dimension types except Ordinate.
        /// </summary>
        public int DIMTIH
        {
            get { return dimtih; }
            set { dimtih = value; }
        }

        /// <summary>
        /// Controls the position of dimension text outside the extension lines.
        /// </summary>
        public int DIMTOH
        {
            get { return dimtoh; }
            set { dimtoh = value; }
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