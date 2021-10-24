#region netDxf library licensed under the MIT License
// 
//                       netDxf library
// Copyright (c) 2019-2021 Daniel Carvajal (haplokuon@gmail.com)
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

namespace netDxf.Tables
{
    /// <summary>
    /// Represents the way tolerances are formatted in dimension entities
    /// </summary>
    public class DimensionStyleTolerances :
        ICloneable
    {
        #region private fields

        private DimensionStyleTolerancesDisplayMethod dimtol;
        private double dimtp;
        private double dimtm;
        private DimensionStyleTolerancesVerticalPlacement dimtolj;
        private short dimtdec;
        private bool suppressLinearLeadingZeros;
        private bool suppressLinearTrailingZeros;
        private bool suppressZeroFeet;
        private bool suppressZeroInches;
        private short dimalttd;
        private bool altSuppressLinearLeadingZeros;
        private bool altSuppressLinearTrailingZeros;
        private bool altSuppressZeroFeet;
        private bool altSuppressZeroInches;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>TolerancesFormat</c> class.
        /// </summary>
        public DimensionStyleTolerances()
        {
            this.dimtol = DimensionStyleTolerancesDisplayMethod.None;
            this.dimtm = 0.0;
            this.dimtp = 0.0;
            this.dimtolj = DimensionStyleTolerancesVerticalPlacement.Middle;
            this.dimtdec = 4;
            this.suppressLinearLeadingZeros = false;
            this.suppressLinearTrailingZeros = false;
            this.suppressZeroFeet = true;
            this.suppressZeroInches = true;
            this.dimalttd = 2;
            this.altSuppressLinearLeadingZeros = false;
            this.altSuppressLinearTrailingZeros = false;
            this.altSuppressZeroFeet = true;
            this.altSuppressZeroInches = true;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the method for calculating the tolerance. (DIMTOL)
        /// </summary>
        /// <remarks>
        /// Default: None
        /// </remarks>
        public DimensionStyleTolerancesDisplayMethod DisplayMethod
        {
            get { return this.dimtol; }
            set { this.dimtol = value; }
        }

        /// <summary>
        /// Gets or sets the maximum or upper tolerance value. When you select Symmetrical in DisplayMethod, this value is used for the tolerance. (DIMTP)
        /// </summary>
        /// <remarks>
        /// Default: 0.0
        /// </remarks>
        public double UpperLimit
        {
            get { return this.dimtp; }
            set { this.dimtp = value; }
        }

        /// <summary>
        /// Gets or sets the minimum or lower tolerance value. (DIMTM)
        /// </summary>
        /// <remarks>
        /// Default: 0.0
        /// </remarks>
        public double LowerLimit
        {
            get { return this.dimtm; }
            set { this.dimtm = value; }
        }

        /// <summary>
        /// Gets or sets the text vertical placement for symmetrical and deviation tolerances. (DIMTOLJ)
        /// </summary>
        /// <remarks>
        /// Default: Middle
        /// </remarks>
        public DimensionStyleTolerancesVerticalPlacement VerticalPlacement
        {
            get { return this.dimtolj; }
            set { this.dimtolj = value; }
        }

        /// <summary>
        /// Gets or sets the number of decimal places. (DIMTDEC)
        /// </summary>
        /// <remarks>
        /// Default: 4<br/>
        /// It is recommended to use values in the range 0 to 8.
        /// </remarks>
        public short Precision
        {
            get { return this.dimtdec; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The tolerance precision must be equals or greater than zero.");
                }
                this.dimtdec = value;
            }
        }

        /// <summary>
        /// Suppresses leading zeros in linear decimal tolerance units. (DIMTZIN)
        /// </summary>
        /// <remarks>
        /// This value is part of the DIMTZIN variable.
        /// </remarks>
        public bool SuppressLinearLeadingZeros
        {
            get { return this.suppressLinearLeadingZeros; }
            set { this.suppressLinearLeadingZeros = value; }
        }

        /// <summary>
        /// Suppresses trailing zeros in linear decimal tolerance units. (DIMTZIN)
        /// </summary>
        /// <remarks>
        /// This value is part of the DIMTZIN variable.
        /// </remarks>
        public bool SuppressLinearTrailingZeros
        {
            get { return this.suppressLinearTrailingZeros; }
            set { this.suppressLinearTrailingZeros = value; }
        }

        /// <summary>
        /// Suppresses zero feet in architectural tolerance units. (DIMTZIN)
        /// </summary>
        /// <remarks>
        /// This value is part of the DIMTZIN variable.
        /// </remarks>
        public bool SuppressZeroFeet
        {
            get { return this.suppressZeroFeet; }
            set { this.suppressZeroFeet = value; }
        }

        /// <summary>
        /// Suppresses zero inches in architectural tolerance units. (DIMTZIN)
        /// </summary>
        /// <remarks>
        /// This value is part of the DIMTZIN variable.
        /// </remarks>
        public bool SuppressZeroInches
        {
            get { return this.suppressZeroInches; }
            set { this.suppressZeroInches = value; }
        }

        /// <summary>
        /// Gets or sets the number of decimal places of the tolerance alternate units. (DIMALTTD)
        /// </summary>
        /// <remarks>
        /// Default: 2<br/>
        /// It is recommended to use values in the range 0 to 8.
        /// </remarks>
        public short AlternatePrecision
        {
            get { return this.dimalttd; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The alternate precision must be equals or greater than zero.");
                }
                this.dimalttd = value;
            }
        }

        /// <summary>
        /// Suppresses leading zeros in linear decimal alternate tolerance units. (DIMALTTZ)
        /// </summary>
        /// <remarks>This value is part of the DIMALTTZ variable.</remarks>
        public bool AlternateSuppressLinearLeadingZeros
        {
            get { return this.altSuppressLinearLeadingZeros; }
            set { this.altSuppressLinearLeadingZeros = value; }
        }

        /// <summary>
        /// Suppresses trailing zeros in linear decimal alternate tolerance units. (DIMALTTZ)
        /// </summary>
        /// <remarks>This value is part of the DIMALTTZ variable.</remarks>
        public bool AlternateSuppressLinearTrailingZeros
        {
            get { return this.altSuppressLinearTrailingZeros; }
            set { this.altSuppressLinearTrailingZeros = value; }
        }

        /// <summary>
        /// Suppresses zero feet in architectural alternate tolerance units. (DIMALTTZ)
        /// </summary>
        /// <remarks>This value is part of the DIMALTTZ variable.</remarks>
        public bool AlternateSuppressZeroFeet
        {
            get { return this.altSuppressZeroFeet; }
            set { this.altSuppressZeroFeet = value; }
        }

        /// <summary>
        /// Suppresses zero inches in architectural alternate tolerance units. (DIMALTTZ)
        /// </summary>
        /// <remarks>This value is part of the DIMALTTZ variable.</remarks>
        public bool AlternateSuppressZeroInches
        {
            get { return this.altSuppressZeroInches; }
            set { this.altSuppressZeroInches = value; }
        }

        #endregion

        #region implements ICloneable

        /// <summary>
        /// Creates a new <c>DimensionStyle.TolerancesFormat</c> that is a copy of the current instance.
        /// </summary>
        /// <returns>A new <c>DimensionStyle.TolerancesFormat</c> that is a copy of this instance.</returns>
        public object Clone()
        {
            return new DimensionStyleTolerances
            {
                DisplayMethod = this.dimtol,
                UpperLimit = this.dimtp,
                LowerLimit = this.dimtm,
                VerticalPlacement = this.dimtolj,
                Precision = this.dimtdec,
                SuppressLinearLeadingZeros = this.suppressLinearLeadingZeros,
                SuppressLinearTrailingZeros = this.suppressLinearTrailingZeros,
                SuppressZeroFeet = this.suppressZeroFeet,
                SuppressZeroInches = this.suppressZeroInches,
                AlternatePrecision = this.dimalttd,
                AlternateSuppressLinearLeadingZeros = this.altSuppressLinearLeadingZeros,
                AlternateSuppressLinearTrailingZeros = this.altSuppressLinearTrailingZeros,
                AlternateSuppressZeroFeet = this.altSuppressZeroFeet,
                AlternateSuppressZeroInches = this.altSuppressZeroInches,
            };
        }

        #endregion
    }
}