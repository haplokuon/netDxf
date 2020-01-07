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

namespace netDxf.Units
{
    /// <summary>
    /// Represents the parameters to convert linear and angular units to its string representation.
    /// </summary>
    public class UnitStyleFormat
    {
        #region private fields

        private short linearDecimalPlaces;
        private short angularDecimalPlaces;
        private string decimalSeparator;
        private string feetInchesSeparator;
        private string degreesSymbol;
        private string minutesSymbol;
        private string secondsSymbol;
        private string radiansSymbol;
        private string gradiansSymbol;
        private string feetSymbol;
        private string inchesSymbol;
        private double fractionHeightScale;
        private FractionFormatType fractionType;
        private bool suppressLinearLeadingZeros;
        private bool suppressLinearTrailingZeros;
        private bool suppressAngularLeadingZeros;
        private bool suppressAngularTrailingZeros;
        private bool suppressZeroFeet;
        private bool suppressZeroInches;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>UnitStyleFormat</c> class.
        /// </summary>
        public UnitStyleFormat()
        {
            this.linearDecimalPlaces = 2;
            this.angularDecimalPlaces = 0;
            this.decimalSeparator = ".";
            this.feetInchesSeparator = "-";
            this.degreesSymbol = "°";
            this.minutesSymbol = "\'";
            this.secondsSymbol = "\"";
            this.radiansSymbol = "r";
            this.gradiansSymbol = "g";
            this.feetSymbol = "\'";
            this.inchesSymbol = "\"";
            this.fractionHeightScale = 1.0;
            this.fractionType = FractionFormatType.Horizontal;
            this.suppressLinearLeadingZeros = false;
            this.suppressLinearTrailingZeros = false;
            this.suppressAngularLeadingZeros = false;
            this.suppressAngularTrailingZeros = false;
            this.suppressZeroFeet = true;
            this.suppressZeroInches = true;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the number of decimal places for linear units.
        /// </summary>
        /// <remarks>
        /// For architectural and fractional the precision used for the minimum fraction is 1/2^LinearDecimalPlaces.
        /// </remarks>
        public short LinearDecimalPlaces
        {
            get { return this.linearDecimalPlaces; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The number of decimal places must be equals or greater than zero.");
                this.linearDecimalPlaces = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of decimal places for angular units.
        /// </summary>
        public short AngularDecimalPlaces
        {
            get { return this.angularDecimalPlaces; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The number of decimal places must be equals or greater than zero.");
                this.angularDecimalPlaces = value;
            }
        }

        /// <summary>
        /// Gets or set the decimal separator.
        /// </summary>
        public string DecimalSeparator
        {
            get { return this.decimalSeparator; }
            set { this.decimalSeparator = value; }
        }

        /// <summary>
        /// Gets or sets the separator between feet and inches.
        /// </summary>
        public string FeetInchesSeparator
        {
            get { return this.feetInchesSeparator; }
            set { this.feetInchesSeparator = value; }
        }

        /// <summary>
        /// Gets or set the angle degrees symbol.
        /// </summary>
        public string DegreesSymbol
        {
            get { return this.degreesSymbol; }
            set { this.degreesSymbol = value; }
        }

        /// <summary>
        /// Gets or set the angle minutes symbol.
        /// </summary>
        public string MinutesSymbol
        {
            get { return this.minutesSymbol; }
            set { this.minutesSymbol = value; }
        }

        /// <summary>
        /// Gets or set the angle seconds symbol.
        /// </summary>
        public string SecondsSymbol
        {
            get { return this.secondsSymbol; }
            set { this.secondsSymbol = value; }
        }

        /// <summary>
        /// Gets or set the angle radians symbol.
        /// </summary>
        public string RadiansSymbol
        {
            get { return this.radiansSymbol; }
            set { this.radiansSymbol = value; }
        }

        /// <summary>
        /// Gets or set the angle gradians symbol.
        /// </summary>
        public string GradiansSymbol
        {
            get { return this.gradiansSymbol; }
            set { this.gradiansSymbol = value; }
        }

        /// <summary>
        /// Gets or set the feet symbol.
        /// </summary>
        public string FeetSymbol
        {
            get { return this.feetSymbol; }
            set { this.feetSymbol = value; }
        }

        /// <summary>
        /// Gets or set the inches symbol.
        /// </summary>
        public string InchesSymbol
        {
            get { return this.inchesSymbol; }
            set { this.inchesSymbol = value; }
        }

        /// <summary>
        /// Gets or sets the scale of fractions relative to dimension text height.
        /// </summary>
        public double FractionHeightScale
        {
            get { return this.fractionHeightScale; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The fraction height scale must be greater than zero.");
                this.fractionHeightScale = value;
            }
        }

        /// <summary>
        /// Gets or sets the fraction format for architectural or fractional units.
        /// </summary>
        /// <remarks>
        /// Horizontal stacking<br/>
        /// Diagonal stacking<br/>
        /// Not stacked (for example, 1/2)
        /// </remarks>
        public FractionFormatType FractionType
        {
            get { return this.fractionType; }
            set { this.fractionType = value; }
        }

        /// <summary>
        /// Suppresses leading zeros in linear decimal dimensions (for example, 0.5000 becomes .5000).
        /// </summary>
        public bool SuppressLinearLeadingZeros
        {
            get { return this.suppressLinearLeadingZeros; }
            set { this.suppressLinearLeadingZeros = value; }
        }

        /// <summary>
        /// Suppresses trailing zeros in linear decimal dimensions (for example, 12.5000 becomes 12.5).
        /// </summary>
        public bool SuppressLinearTrailingZeros
        {
            get { return this.suppressLinearTrailingZeros; }
            set { this.suppressLinearTrailingZeros = value; }
        }

        /// <summary>
        /// Suppresses leading zeros in angular decimal dimensions (for example, 0.5000 becomes .5000).
        /// </summary>
        public bool SuppressAngularLeadingZeros
        {
            get { return this.suppressAngularLeadingZeros; }
            set { this.suppressAngularLeadingZeros = value; }
        }

        /// <summary>
        /// Suppresses trailing zeros in angular decimal dimensions (for example, 12.5000 becomes 12.5).
        /// </summary>
        public bool SuppressAngularTrailingZeros
        {
            get { return this.suppressAngularTrailingZeros; }
            set { this.suppressAngularTrailingZeros = value; }
        }

        /// <summary>
        /// Suppresses zero feet in architectural dimensions.
        /// </summary>
        public bool SuppressZeroFeet
        {
            get { return this.suppressZeroFeet; }
            set { this.suppressZeroFeet = value; }
        }

        /// <summary>
        /// Suppresses zero inches in architectural dimensions.
        /// </summary>
        public bool SuppressZeroInches
        {
            get { return this.suppressZeroInches; }
            set { this.suppressZeroInches = value; }
        }

        #endregion
    }
}