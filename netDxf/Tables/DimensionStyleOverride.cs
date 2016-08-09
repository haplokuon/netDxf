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
using netDxf.Blocks;
using netDxf.Units;

namespace netDxf.Tables
{
    /// <summary>
    /// Represents a dimension style value that overrides a property of the style associated with a dimension.
    /// </summary>
    public class DimensionStyleOverride
    {
        #region private fields

        private readonly DimensionStyleOverrideType type;
        private readonly object value;

        #endregion

        #region constructor

        /// <summary>
        /// Initializes a new instance of <c>DimensionStyleOverride</c>.
        /// </summary>
        /// <param name="type">Type of the dimension style to override.</param>
        /// <param name="value">Value of the dimension style to override.</param>
        public DimensionStyleOverride(DimensionStyleOverrideType type, object value)
        {
            // check if the value is valid for its assigned type
            switch (type)
            {
                case DimensionStyleOverrideType.DimLineColor:
                    if (!(value is AciColor))
                        throw new ArgumentException(string.Format("The DimensionStyleOverrideType.{0} dimension style override must be a valid {1}", type, typeof (AciColor)), nameof(value));
                    break;
                case DimensionStyleOverrideType.DimLineLinetype:
                    if (!(value is Linetype))
                        throw new ArgumentException(string.Format("The DimensionStyleOverrideType.{0} dimension style override must be a valid {1}", type, typeof (Linetype)), nameof(value));
                    break;
                case DimensionStyleOverrideType.DimLineLineweight:
                    if (!(value is Lineweight))
                        throw new ArgumentException(string.Format("The DimensionStyleOverrideType.{0} dimension style override must be a valid {1}", type, typeof (Lineweight)), nameof(value));
                    break;
                case DimensionStyleOverrideType.DimLineExtend:
                    if (!(value is double))
                        throw new ArgumentException(string.Format("The DimensionStyleOverrideType.{0} dimension style override must be a valid {1}", type, typeof (double)), nameof(value));
                    if ((double) value < 0)
                        throw new ArgumentOutOfRangeException(nameof(value), value, string.Format("The DimensionStyleOverrideType.{0} dimension style override must be equals or greater than zero.", type));
                    break;
                case DimensionStyleOverrideType.ExtLineColor:
                    if (!(value is AciColor))
                        throw new ArgumentException(string.Format("The DimensionStyleOverrideType.{0} dimension style override must be a valid {1}", type, typeof (AciColor)), nameof(value));
                    break;
                case DimensionStyleOverrideType.ExtLine1Linetype:
                    if (!(value is Linetype))
                        throw new ArgumentException(string.Format("The DimensionStyleOverrideType.{0} dimension style override must be a valid {1}", type, typeof (Linetype)), nameof(value));
                    break;
                case DimensionStyleOverrideType.ExtLine2Linetype:
                    if (!(value is Linetype))
                        throw new ArgumentException(string.Format("The DimensionStyleOverrideType.{0} dimension style override must be a valid {1}", type, typeof (Linetype)), nameof(value));
                    break;
                case DimensionStyleOverrideType.ExtLineLineweight:
                    if (!(value is Lineweight))
                        throw new ArgumentException(string.Format("The DimensionStyleOverrideType.{0} dimension style override must be a valid {1}", type, typeof (Lineweight)), nameof(value));
                    break;
                case DimensionStyleOverrideType.ExtLine1:
                    if (!(value is bool))
                        throw new ArgumentException(string.Format("The DimensionStyleOverrideType.{0} dimension style override must be a valid {1}", type, typeof (bool)), nameof(value));
                    break;
                case DimensionStyleOverrideType.ExtLine2:
                    if (!(value is bool))
                        throw new ArgumentException(string.Format("The DimensionStyleOverrideType.{0} dimension style override must be a valid {1}", type, typeof (bool)), nameof(value));
                    break;
                case DimensionStyleOverrideType.ExtLineOffset:
                    if (!(value is double))
                        throw new ArgumentException(string.Format("The DimensionStyleOverrideType.{0} dimension style override must be a valid {1}", type, typeof (double)), nameof(value));
                    if ((double) value < 0)
                        throw new ArgumentOutOfRangeException(nameof(value), value, string.Format("The DimensionStyleOverrideType.{0} dimension style override must be equals or greater than zero.", type));
                    break;
                case DimensionStyleOverrideType.ExtLineExtend:
                    if (!(value is double))
                        throw new ArgumentException(string.Format("The DimensionStyleOverrideType.{0} dimension style override must be a valid {1}", type, typeof (double)), nameof(value));
                    if ((double) value < 0)
                        throw new ArgumentOutOfRangeException(nameof(value), value, string.Format("The DimensionStyleOverrideType.{0} dimension style override must be equals or greater than zero.", type));
                    break;
                case DimensionStyleOverrideType.ArrowSize:
                    if (!(value is double))
                        throw new ArgumentException(string.Format("The DimensionStyleOverrideType.{0} dimension style override must be a valid {1}", type, typeof (double)), nameof(value));
                    if ((double) value < 0)
                        throw new ArgumentOutOfRangeException(nameof(value), value, string.Format("The DimensionStyleOverrideType.{0} dimension style override must be equals or greater than zero.", type));
                    break;
                case DimensionStyleOverrideType.CenterMarkSize:
                    if (!(value is double))
                        throw new ArgumentException(string.Format("The DimensionStyleOverrideType.{0} dimension style override must be a valid {1}", type, typeof (double)), nameof(value));
                    break;
                case DimensionStyleOverrideType.LeaderArrow:
                    if (value == null)
                        break;
                    if (!(value is Block))
                        throw new ArgumentException(string.Format("The DimensionStyleOverrideType.{0} dimension style override must be a valid {1}", type, typeof (Block)), nameof(value));
                    break;
                case DimensionStyleOverrideType.DimArrow1:
                    if (value == null)
                        break;
                    if (!(value is Block))
                        throw new ArgumentException(string.Format("The DimensionStyleOverrideType.{0} dimension style override must be a valid {1}", type, typeof (Block)), nameof(value));
                    break;
                case DimensionStyleOverrideType.DimArrow2:
                    if (value == null)
                        break;
                    if (!(value is Block))
                        throw new ArgumentException(string.Format("The DimensionStyleOverrideType.{0} dimension style override must be a valid {1}", type, typeof (Block)), nameof(value));
                    break;
                case DimensionStyleOverrideType.TextStyle:
                    if (!(value is TextStyle))
                        throw new ArgumentException(string.Format("The DimensionStyleOverrideType.{0} dimension style override must be a valid {1}", type, typeof (TextStyle)), nameof(value));
                    break;
                case DimensionStyleOverrideType.TextColor:
                    if (!(value is AciColor))
                        throw new ArgumentException(string.Format("The DimensionStyleOverrideType.{0} dimension style override must be a valid {1}", type, typeof (AciColor)), nameof(value));
                    break;
                case DimensionStyleOverrideType.TextHeight:
                    if (!(value is double))
                        throw new ArgumentException(string.Format("The DimensionStyleOverrideType.{0} dimension style override must be a valid {1}", type, typeof (double)), nameof(value));
                    if ((double) value <= 0)
                        throw new ArgumentOutOfRangeException(nameof(value), value, string.Format("The {0} dimension style override must be greater than zero.", type));
                    break;
                case DimensionStyleOverrideType.TextOffset:
                    if (!(value is double))
                        throw new ArgumentException(string.Format("The DimensionStyleOverrideType.{0} dimension style override must be a valid {1}", type, typeof (double)), nameof(value));
                    if ((double) value < 0)
                        throw new ArgumentOutOfRangeException(nameof(value), value, string.Format("The {0} dimension style override must be equals or greater than zero.", type));
                    break;
                case DimensionStyleOverrideType.DimScaleOverall:
                    if (!(value is double))
                        throw new ArgumentException(string.Format("The DimensionStyleOverrideType.{0} dimension style override must be a valid {1}", type, typeof (double)), nameof(value));
                    if ((double) value < 0)
                        throw new ArgumentOutOfRangeException(nameof(value), value, string.Format("The DimensionStyleOverrideType.{0} dimension style override must be equals or greater than zero.", type));
                    break;
                case DimensionStyleOverrideType.AngularPrecision:
                    if (!(value is short))
                        throw new ArgumentException(string.Format("The DimensionStyleOverrideType.{0} dimension style override must be a valid {1}", type, typeof (short)), nameof(value));
                    if ((short) value < -1)
                        throw new ArgumentOutOfRangeException(nameof(value), value, string.Format("The {0} dimension style override must be greater than -1.", type));
                    break;
                case DimensionStyleOverrideType.LengthPrecision:
                    if (!(value is short))
                        throw new ArgumentException(string.Format("The DimensionStyleOverrideType.{0} dimension style override must be a valid {1}", type, typeof (short)), nameof(value));
                    if ((short) value < 0)
                        throw new ArgumentOutOfRangeException(nameof(value), value, string.Format("The {0} dimension style override must be equals or greater than zero.", type));
                    break;
                case DimensionStyleOverrideType.DimPrefix:
                    if (!(value is string))
                        throw new ArgumentException(string.Format("The DimensionStyleOverrideType.{0} dimension style override must be a valid {1}", type, typeof (string)), nameof(value));
                    break;
                case DimensionStyleOverrideType.DimSuffix:
                    if (!(value is string))
                        throw new ArgumentException(string.Format("The DimensionStyleOverrideType.{0} dimension style override must be a valid {1}", type, typeof (string)), nameof(value));
                    break;
                case DimensionStyleOverrideType.DecimalSeparator:
                    if (!(value is char))
                        throw new ArgumentException(string.Format("The DimensionStyleOverrideType.{0} dimension style override must be a valid {1}", type, typeof (char)), nameof(value));
                    break;
                case DimensionStyleOverrideType.DimScaleLinear:
                    if (!(value is double))
                        throw new ArgumentException(string.Format("The DimensionStyleOverrideType.{0} dimension style override must be a valid {1}", type, typeof (double)), nameof(value));
                    if (MathHelper.IsZero((double) value))
                        throw new ArgumentOutOfRangeException(nameof(value), value, string.Format("The DimensionStyleOverrideType.{0} dimension style override cannot be zero.", type));
                    break;
                case DimensionStyleOverrideType.DimLengthUnits:
                    if (!(value is LinearUnitType))
                        throw new ArgumentException(string.Format("The DimensionStyleOverrideType.{0} dimension style override must be a valid {1}", type, typeof (LinearUnitType)), nameof(value));
                    break;
                case DimensionStyleOverrideType.DimAngularUnits:
                    if (!(value is AngleUnitType))
                        throw new ArgumentException(string.Format("The DimensionStyleOverrideType.{0} dimension style override must be a valid {1}", type, typeof (AngleUnitType)), nameof(value));
                    break;
                case DimensionStyleOverrideType.FractionalType:
                    if (!(value is FractionFormatType))
                        throw new ArgumentException(string.Format("The DimensionStyleOverrideType.{0} dimension style override must be a valid {1}", type, typeof (FractionFormatType)), nameof(value));
                    break;
                case DimensionStyleOverrideType.SuppressLinearLeadingZeros:
                    if (!(value is bool))
                        throw new ArgumentException(string.Format("The DimensionStyleOverrideType.{0} dimension style override must be a valid {1}", type, typeof (bool)), nameof(value));
                    break;
                case DimensionStyleOverrideType.SuppressLinearTrailingZeros:
                    if (!(value is bool))
                        throw new ArgumentException(string.Format("The DimensionStyleOverrideType.{0} dimension style override must be a valid {1}", type, typeof (bool)), nameof(value));
                    break;
                case DimensionStyleOverrideType.SuppressAngularLeadingZeros:
                    if (!(value is bool))
                        throw new ArgumentException(string.Format("The DimensionStyleOverrideType.{0} dimension style override must be a valid {1}", type, typeof (bool)), nameof(value));
                    break;
                case DimensionStyleOverrideType.SuppressAngularTrailingZeros:
                    if (!(value is bool))
                        throw new ArgumentException(string.Format("The DimensionStyleOverrideType.{0} dimension style override must be a valid {1}", type, typeof (bool)), nameof(value));
                    break;
                case DimensionStyleOverrideType.SuppressZeroFeet:
                    if (!(value is bool))
                        throw new ArgumentException(string.Format("The DimensionStyleOverrideType.{0} dimension style override must be a valid {1}", type, typeof (bool)), nameof(value));
                    break;
                case DimensionStyleOverrideType.SuppressZeroInches:
                    if (!(value is bool))
                        throw new ArgumentException(string.Format("The DimensionStyleOverrideType.{0} dimension style override must be a valid {1}", type, typeof (bool)), nameof(value));
                    break;
                case DimensionStyleOverrideType.DimRoundoff:
                    if (!(value is double))
                        throw new ArgumentException(string.Format("The DimensionStyleOverrideType.{0} dimension style override must be a valid {1}", type, typeof (double)), nameof(value));
                    if ((double) value < 0)
                        throw new ArgumentOutOfRangeException(nameof(value), value, string.Format("The {0} dimension style override must be equals or greater than zero.", type));
                    break;
            }
            this.type = type;
            this.value = value;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the type of the dimension style to override.
        /// </summary>
        public DimensionStyleOverrideType Type
        {
            get { return this.type; }
        }

        /// <summary>
        /// Gets the value of the dimension style to override.
        /// </summary>
        public object Value
        {
            get { return this.value; }
        }

        #endregion

        #region overrides

        /// <summary>
        /// Obtains a string that represents the actual dimension style override.
        /// </summary>
        /// <returns>A string text.</returns>
        public override string ToString()
        {
            return string.Format("{0} : {1}", this.type, this.value);
        }

        #endregion
    }
}