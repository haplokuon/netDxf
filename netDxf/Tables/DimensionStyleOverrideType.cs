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

namespace netDxf.Tables
{
    /// <summary>
    /// Dimension style override types.
    /// </summary>
    /// <remarks>
    /// There is one dimension style override type for each property of the <see cref="netDxf.Tables.DimensionStyle">DimensionStyle</see> class.
    /// The dimension style properties DIMBLK and DIMSAH are not available.
    /// The overrides always make use of the DIMBLK1 and DIMBLK2 setting the DIMSAH to true even when both arrow ends are the same.
    /// </remarks>
    public enum DimensionStyleOverrideType
    {
        /// <summary>
        /// Assigns colors to dimension lines, arrowheads, and dimension leader lines.
        /// </summary>
        DimLineColor,

        /// <summary>
        /// Linetype of the dimension line.
        /// </summary>
        DimLineLinetype,

        /// <summary>
        /// Lineweight to dimension lines.
        /// </summary>
        DimLineLineweight,

        /// <summary>
        /// Distance the dimension line extends beyond the extension line when oblique, architectural tick, integral, or no marks are drawn for arrowheads.
        /// </summary>
        DimLineExtend,

        /// <summary>
        /// Colors to extension lines, center marks, and centerlines.
        /// </summary>
        ExtLineColor,

        /// <summary>
        /// Linetype of the first extension line.
        /// </summary>
        ExtLine1Linetype,

        /// <summary>
        /// Linetype of the second extension line.
        /// </summary>
        ExtLine2Linetype,

        /// <summary>
        /// Lineweight to extension lines.
        /// </summary>
        ExtLineLineweight,

        /// <summary>
        /// Suppresses display of the first extension line.
        /// </summary>
        ExtLine1,

        /// <summary>
        /// Suppresses display of the second extension line.
        /// </summary>
        ExtLine2,

        /// <summary>
        /// Specifies how far extension lines are offset from origin points.
        /// </summary>
        ExtLineOffset,

        /// <summary>
        /// Specifies how far to extend the extension line beyond the dimension line.
        /// </summary>
        ExtLineExtend,

        /// <summary>
        /// Size of dimension line and leader line arrowheads. Also controls the size of hook lines.
        /// </summary>
        ArrowSize,

        /// <summary>
        /// Drawing of circle or arc center marks and centerlines.
        /// </summary>
        CenterMarkSize,

        /// <summary>
        /// Arrowhead block for leaders.
        /// </summary>
        LeaderArrow,

        /// <summary>
        /// Arrowhead block for the first end of the dimension line.
        /// </summary>
        DimArrow1,

        /// <summary>
        /// Arrowhead block for the second end of the dimension line.
        /// </summary>
        DimArrow2,

        /// <summary>
        /// Text style of the dimension.
        /// </summary>
        TextStyle,

        /// <summary>
        /// Color of dimension text.
        /// </summary>
        TextColor,

        /// <summary>
        /// Height of dimension text, unless the current text style has a fixed height.
        /// </summary>
        TextHeight,

        ///// <summary>
        ///// Horizontal positioning of dimension text.
        ///// </summary>
        //DIMJUST,

        ///// <summary>
        ///// Vertical position of text in relation to the dimension line.
        ///// </summary>
        //DIMTAD,

        /// <summary>
        /// Distance around the dimension text when the dimension line breaks to accommodate dimension text.
        /// </summary>
        TextOffset,

        /// <summary>
        /// Overall scale factor applied to dimensioning variables that specify sizes, distances, or offsets.
        /// </summary>
        DimScaleOverall,

        ///// <summary>
        ///// Position of dimension text inside the extension lines for all dimension types except Ordinate.
        ///// </summary>
        //DIMTIH,

        ///// <summary>
        ///// Position of dimension text outside the extension lines.
        ///// </summary>
        //DIMTOH,

        /// <summary>
        /// Number of precision places displayed in angular dimensions.
        /// </summary>
        AngularPrecision,

        /// <summary>
        /// Number of decimal places displayed for the primary units of a dimension.
        /// </summary>
        LengthPrecision,

        ///// <summary>
        ///// Text prefix or suffix (or both) to the dimension measurement.
        ///// </summary>
        //DIMPOST,

        /// <summary>
        /// Specifies the text prefix for the dimension.
        /// </summary>
        DimPrefix,

        /// <summary>
        /// Specifies the text suffix for the dimension.
        /// </summary>
        DimSuffix,

        /// <summary>
        /// Single-character decimal separator to use when creating dimensions whose unit format is decimal.
        /// </summary>
        DecimalSeparator,

        /// <summary>
        /// Scale factor for linear dimension measurements
        /// </summary>
        DimScaleLinear,

        /// <summary>
        /// Units for all dimension types except angular.
        /// </summary>
        DimLengthUnits,

        /// <summary>
        /// Units format for angular dimensions.
        /// </summary>
        DimAngularUnits,

        /// <summary>
        /// Fraction format when DIMLUNIT is set to Architectural or Fractional.
        /// </summary>
        FractionalType,

        /// <summary>
        /// Suppresses leading zeros in linear decimal dimensions (for example, 0.5000 becomes .5000).
        /// </summary>
        SuppressLinearLeadingZeros,

        /// <summary>
        /// Suppresses trailing zeros in linear decimal dimensions (for example, 12.5000 becomes 12.5).
        /// </summary>
        SuppressLinearTrailingZeros,

        /// <summary>
        /// Suppresses leading zeros in angular decimal dimensions (for example, 0.5000 becomes .5000).
        /// </summary>
        SuppressAngularLeadingZeros,

        /// <summary>
        /// Suppresses trailing zeros in angular decimal dimensions (for example, 12.5000 becomes 12.5).
        /// </summary>
        SuppressAngularTrailingZeros,

        /// <summary>
        /// Suppresses zero feet in architectural dimensions.
        /// </summary>
        SuppressZeroFeet,

        /// <summary>
        /// Suppresses zero inches in architectural dimensions.
        /// </summary>
        SuppressZeroInches,

        /// <summary>
        /// Value to round all dimensioning distances.
        /// </summary>
        DimRoundoff
    }
}