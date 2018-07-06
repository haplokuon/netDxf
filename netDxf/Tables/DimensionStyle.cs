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
using netDxf.Blocks;
using netDxf.Collections;
using netDxf.Units;

namespace netDxf.Tables
{
    /// <summary>
    /// Represents a dimension style.
    /// </summary>
    public class DimensionStyle :
        TableObject
    {
        #region nested classes

        /// <summary>
        /// Represents the way alternate units are formatted in dimension entities.
        /// </summary>
        /// <remarks>Alternative units are not applicable for angular dimensions.</remarks>
        public class AlternateUnitsFormat :
            ICloneable
        {
            #region private fields

            private bool dimalt;
            private LinearUnitType dimaltu;
            private bool stackedUnits;
            private short dimaltd;
            private double dimaltf;
            private double dimaltrnd;
            private string dimPrefix;
            private string dimSuffix;
            private bool suppressLinearLeadingZeros;
            private bool suppressLinearTrailingZeros;
            private bool suppressZeroFeet;
            private bool suppressZeroInches;

            #endregion

            #region constructors

            /// <summary>
            /// Initializes a new instance of the <c>DimensionStyleUnitsFormat</c> class.
            /// </summary>
            public AlternateUnitsFormat()
            {
                this.dimalt = false;
                this.dimaltd = 2;
                this.dimPrefix = string.Empty;
                this.dimSuffix = string.Empty;
                this.dimaltf = 25.4;
                this.dimaltu = LinearUnitType.Decimal;
                this.stackedUnits = false;
                this.suppressLinearLeadingZeros = false;
                this.suppressLinearTrailingZeros = false;
                this.suppressZeroFeet = true;
                this.suppressZeroInches = true;
                this.dimaltrnd = 0.0;
            }

            #endregion

            #region public properties

            /// <summary>
            /// Gets or sets if the alternate measurement units are added to the dimension text.
            /// </summary>
            public bool Enabled
            {
                get { return this.dimalt; }
                set { this.dimalt = value; }
            }

            /// <summary>
            /// Sets the number of decimal places displayed for the alternate units of a dimension.
            /// </summary>
            /// <remarks>
            /// Default: 4<br/>
            /// It is recommended to use values in the range 0 to 8.<br/>
            /// For architectural and fractional the precision used for the minimum fraction is 1/2^LinearDecimalPlaces.
            /// </remarks>
            public short LengthPrecision
            {
                get { return this.dimaltd; }
                set
                {
                    if (value < 0)
                        throw new ArgumentOutOfRangeException(nameof(value), value, "The length precision must be equals or greater than zero.");
                    this.dimaltd = value;
                }
            }

            /// <summary>
            /// Specifies the text prefix for the dimension.
            /// </summary>
            public string Prefix
            {
                get { return this.dimPrefix; }
                set { this.dimPrefix = value ?? string.Empty; }
            }

            /// <summary>
            /// Specifies the text suffix for the dimension.
            /// </summary>
            public string Suffix
            {
                get { return this.dimSuffix; }
                set { this.dimSuffix = value ?? string.Empty; }
            }

            /// <summary>
            /// Gets or sets the multiplier used as the conversion factor between primary and alternate units.
            /// </summary>
            /// <remarks>
            /// to convert inches to millimeters, enter 25.4.
            /// The value has no effect on angular dimensions, and it is not applied to the rounding value or the plus or minus tolerance values. 
            /// </remarks>
            public double Multiplier
            {
                get { return this.dimaltf; }
                set
                {
                    if (value <= 0.0)
                        throw new ArgumentOutOfRangeException(nameof(value), value, "The multiplier for alternate units must be greater than zero0.");
                    this.dimaltf = value;
                }
            }

            /// <summary>
            /// Gets or sets the alternate units for all dimension types except angular.
            /// </summary>
            /// <remarks>
            /// Scientific<br/>
            /// Decimal<br/>
            /// Engineering<br/>
            /// Architectural<br/>
            /// Fractional
            /// </remarks>
            public LinearUnitType LengthUnits
            {
                get { return this.dimaltu; }
                set { this.dimaltu = value; }
            }

            /// <summary>
            /// Gets or set if the Architectural or Fractional linear units will be shown stacked or not.
            /// </summary>
            /// <remarks>
            /// This value only is applicable if the <c>DimLengthUnits</c> property has been set to Architectural or Fractional,
            /// for any other value this parameter is not applicable.
            /// </remarks>
            public bool StackUnits
            {
                get { return this.stackedUnits; }
                set { this.stackedUnits = value; }
            }

            /// <summary>
            /// Suppresses leading zeros in linear decimal alternate units (for example, 0.5000 becomes .5000).
            /// </summary>
            /// <remarks>This value is part of the DIMALTZ variable.</remarks>
            public bool SuppressLinearLeadingZeros
            {
                get { return this.suppressLinearLeadingZeros; }
                set { this.suppressLinearLeadingZeros = value; }
            }

            /// <summary>
            /// Suppresses trailing zeros in linear decimal alternate units (for example, 12.5000 becomes 12.5).
            /// </summary>
            /// <remarks>This value is part of the DIMALTZ variable.</remarks>
            public bool SuppressLinearTrailingZeros
            {
                get { return this.suppressLinearTrailingZeros; }
                set { this.suppressLinearTrailingZeros = value; }
            }

            /// <summary>
            /// Suppresses zero feet in architectural alternate units.
            /// </summary>
            /// <remarks>This value is part of the DIMALTZ variable.</remarks>
            public bool SuppressZeroFeet
            {
                get { return this.suppressZeroFeet; }
                set { this.suppressZeroFeet = value; }
            }

            /// <summary>
            /// Suppresses zero inches in architectural alternate units.
            /// </summary>
            /// <remarks>This value is part of the DIMALTZ variable.</remarks>
            public bool SuppressZeroInches
            {
                get { return this.suppressZeroInches; }
                set { this.suppressZeroInches = value; }
            }

            /// <summary>
            /// Gets or sets the value to round all dimensioning distances.
            /// </summary>
            /// <remarks>
            /// Default: 0 (no rounding off).<br/>
            /// If DIMRND is set to 0.25, all distances round to the nearest 0.25 unit.
            /// If you set DIMRND to 1.0, all distances round to the nearest integer.
            /// Note that the number of digits edited after the decimal point depends on the precision set by DIMDEC.
            /// DIMRND does not apply to angular dimensions.
            /// </remarks>
            public double Roundoff
            {
                get { return this.dimaltrnd; }
                set
                {
                    if (value < 0.000001 && !MathHelper.IsZero(value, double.Epsilon))
                        throw new ArgumentOutOfRangeException(nameof(value), value, "The nearest value to round all distances must be equal or greater than 0.000001 or zero (no rounding off).");
                    this.dimaltrnd = value;
                }
            }

            #endregion

            #region implements ICloneable

            /// <summary>
            /// Creates a new <c>DimensionStyle.AlternateUnitsFormat</c> that is a copy of the current instance.
            /// </summary>
            /// <returns>A new <c>DimensionStyle.AlternateUnitsFormat</c> that is a copy of this instance.</returns>
            public object Clone()
            {
                AlternateUnitsFormat copy = new AlternateUnitsFormat()
                {
                    Enabled = this.dimalt,
                    LengthUnits = this.dimaltu,
                    StackUnits = this.stackedUnits,
                    LengthPrecision = this.dimaltd,
                    Multiplier = this.dimaltf,
                    Roundoff = this.dimaltrnd,
                    Prefix = this.dimPrefix,
                    Suffix = this.dimSuffix,
                    SuppressLinearLeadingZeros = this.suppressLinearLeadingZeros,
                    SuppressLinearTrailingZeros = this.suppressLinearTrailingZeros,
                    SuppressZeroFeet = this.suppressZeroFeet,
                    SuppressZeroInches = this.suppressZeroInches
                };

                return copy;
            }

            #endregion
        }

        /// <summary>
        /// Defines the method for calculating the tolerance.
        /// </summary>
        /// <remarks>
        /// The Basic method for displaying tolerances in dimensions is not available,
        /// use a negative number for the <c>TextOffet</c> of the dimension style. The result is exactly the same.
        /// </remarks>
        public enum TolerancesDisplayMethod
        {
            /// <summary>
            /// Does not add a tolerance.
            /// </summary>
            None,

            /// <summary>
            /// Adds a plus/minus expression of tolerance in which a single value of variation is applied to the dimension measurement.
            /// </summary>
            Symmetrical,

            /// <summary>
            /// Adds a plus/minus tolerance expression. Different plus and minus values of variation are applied to the dimension measurement.
            /// </summary>
            Deviation,

            /// <summary>
            /// Creates a limit dimension. A maximum and a minimum value are displayed, one over the other.
            /// </summary>
            Limits,
        }

        /// <summary>
        /// Controls text justification for symmetrical and deviation tolerances.
        /// </summary>
        public enum TolerancesVerticalPlacement
        {
            /// <summary>
            /// Aligns the tolerance text with the bottom of the main dimension text.
            /// </summary>
            Bottom = 0,

            /// <summary>
            /// Aligns the tolerance text with the middle of the main dimension text.
            /// </summary>
            Middle = 1,

            /// <summary>
            /// Aligns the tolerance text with the top of the main dimension text.
            /// </summary>
            Top = 2
        }

        /// <summary>
        /// Represents the way tolerances are formated in dimension entities
        /// </summary>
        public class TolerancesFormat :
            ICloneable
        {
            #region private fields

            private TolerancesDisplayMethod dimtol;
            private double dimtp;
            private double dimtm;
            private TolerancesVerticalPlacement dimtolj;
            private short dimtdec;
            private bool suppressLinearLeadingZeros;
            private bool suppressLinearTrailingZeros;
            private bool suppressZeroFeet;
            private bool suppressZeroInches;
            private double dimtfac;
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
            public TolerancesFormat()
            {
                this.dimtol = TolerancesDisplayMethod.None;
                this.dimtm = 0.0;
                this.dimtp = 0.0;
                this.dimtolj = TolerancesVerticalPlacement.Middle;
                this.dimtdec = 4;
                this.suppressLinearLeadingZeros = false;
                this.suppressLinearTrailingZeros = false;
                this.suppressZeroFeet = true;
                this.suppressZeroInches = true;
                this.dimtfac = 1.0;
                this.dimalttd = 2;
                this.altSuppressLinearLeadingZeros = false;
                this.altSuppressLinearTrailingZeros = false;
                this.altSuppressZeroFeet = true;
                this.altSuppressZeroInches = true;
            }

            #endregion

            #region public properties

            /// <summary>
            /// Gets or sets the method for calculating the tolerance.
            /// </summary>
            /// <remarks>
            /// Default: None
            /// </remarks>
            public TolerancesDisplayMethod DisplayMethod
            {
                get { return this.dimtol; }
                set { this.dimtol = value; }
            }

            /// <summary>
            /// Gets or sets the maximum or upper tolerance value. When you select Symmetrical in DisplayMethod, this value is used for the tolerance.
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
            /// Gets or sets the minimum or lower tolerance value.
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
            /// Gets or sets the text vertical placement for symmetrical and deviation tolerances.
            /// </summary>
            /// <remarks>
            /// Default: Middle
            /// </remarks>
            public TolerancesVerticalPlacement VerticalPlacement
            {
                get { return this.dimtolj; }
                set { this.dimtolj = value; }
            }

            /// <summary>
            /// Gets or sets the number of decimal places.
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
                        throw new ArgumentOutOfRangeException(nameof(value), value, "The tolerance precision must be equals or greater than zero.");
                    this.dimtdec = value;
                }
            }

            /// <summary>
            /// Suppresses leading zeros in linear decimal tolerance units (for example, 0.5000 becomes .5000).
            /// </summary>
            /// <remarks>This value is part of the DIMTZIN variable.</remarks>
            public bool SuppressLinearLeadingZeros
            {
                get { return this.suppressLinearLeadingZeros; }
                set { this.suppressLinearLeadingZeros = value; }
            }

            /// <summary>
            /// Suppresses trailing zeros in linear decimal tolerance units (for example, 12.5000 becomes 12.5).
            /// </summary>
            /// <remarks>This value is part of the DIMTZIN variable.</remarks>
            public bool SuppressLinearTrailingZeros
            {
                get { return this.suppressLinearTrailingZeros; }
                set { this.suppressLinearTrailingZeros = value; }
            }

            /// <summary>
            /// Suppresses zero feet in architectural tolerance units.
            /// </summary>
            /// <remarks>This value is part of the DIMTZIN variable.</remarks>
            public bool SuppressZeroFeet
            {
                get { return this.suppressZeroFeet; }
                set { this.suppressZeroFeet = value; }
            }

            /// <summary>
            /// Suppresses zero inches in architectural tolerance units.
            /// </summary>
            /// <remarks>This value is part of the DIMTZIN variable.</remarks>
            public bool SuppressZeroInches
            {
                get { return this.suppressZeroInches; }
                set { this.suppressZeroInches = value; }
            }

            /// <summary>
            /// Gets or sets the height factor applied to the tolerance text in relation with the dimension text height.
            /// </summary>
            /// <remarks>
            /// Default: 1.0
            /// </remarks>
            public double TextHeightFactor
            {
                get { return this.dimtfac; }
                set
                {
                    if (value <= 0)
                        throw new ArgumentOutOfRangeException(nameof(value), value, "The tolerance text height factor must be greater than zero.");
                    this.dimtfac = value;
                }
            }

            /// <summary>
            /// Gets or sets the number of decimal places of the tolerance alternate units.
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
                        throw new ArgumentOutOfRangeException(nameof(value), value, "The alternate precision must be equals or greater than zero.");
                    this.dimalttd = value;
                }
            }

            /// <summary>
            /// Suppresses leading zeros in linear decimal alternate tolerance units (for example, 0.5000 becomes .5000).
            /// </summary>
            /// <remarks>This value is part of the DIMALTTZ variable.</remarks>
            public bool AlternateSuppressLinearLeadingZeros
            {
                get { return this.altSuppressLinearLeadingZeros; }
                set { this.altSuppressLinearLeadingZeros = value; }
            }

            /// <summary>
            /// Suppresses trailing zeros in linear decimal alternate tolerance units (for example, 12.5000 becomes 12.5).
            /// </summary>
            /// <remarks>This value is part of the DIMALTTZ variable.</remarks>
            public bool AlternateSuppressLinearTrailingZeros
            {
                get { return this.altSuppressLinearTrailingZeros; }
                set { this.altSuppressLinearTrailingZeros = value; }
            }

            /// <summary>
            /// Suppresses zero feet in architectural alternate tolerance units.
            /// </summary>
            /// <remarks>This value is part of the DIMALTTZ variable.</remarks>
            public bool AlternateSuppressZeroFeet
            {
                get { return this.altSuppressZeroFeet; }
                set { this.altSuppressZeroFeet = value; }
            }

            /// <summary>
            /// Suppresses zero inches in architectural alternate tolerance units.
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
            /// Creates a new <c>DimensionStyle.TolernacesFormat</c> that is a copy of the current instance.
            /// </summary>
            /// <returns>A new <c>DimensionStyle.TolernacesFormat</c> that is a copy of this instance.</returns>
            public object Clone()
            {
                return new TolerancesFormat
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
                    TextHeightFactor = this.dimtfac,
                    AlternatePrecision = this.dimalttd,
                    AlternateSuppressLinearLeadingZeros = this.altSuppressLinearLeadingZeros,
                    AlternateSuppressLinearTrailingZeros = this.altSuppressLinearTrailingZeros,
                    AlternateSuppressZeroFeet = this.altSuppressZeroFeet,
                    AlternateSuppressZeroInches = this.altSuppressZeroInches,
                };
            }

            #endregion
        }

        #endregion

        #region delegates and events

        public delegate void LinetypeChangedEventHandler(TableObject sender, TableObjectChangedEventArgs<Linetype> e);

        public event LinetypeChangedEventHandler LinetypeChanged;

        protected virtual Linetype OnLinetypeChangedEvent(Linetype oldLinetype, Linetype newLinetype)
        {
            LinetypeChangedEventHandler ae = this.LinetypeChanged;
            if (ae != null)
            {
                TableObjectChangedEventArgs<Linetype> eventArgs = new TableObjectChangedEventArgs<Linetype>(oldLinetype, newLinetype);
                ae(this, eventArgs);
                return eventArgs.NewValue;
            }
            return newLinetype;
        }

        public delegate void TextStyleChangedEventHandler(TableObject sender, TableObjectChangedEventArgs<TextStyle> e);

        public event TextStyleChangedEventHandler TextStyleChanged;

        protected virtual TextStyle OnTextStyleChangedEvent(TextStyle oldTextStyle, TextStyle newTextStyle)
        {
            TextStyleChangedEventHandler ae = this.TextStyleChanged;
            if (ae != null)
            {
                TableObjectChangedEventArgs<TextStyle> eventArgs = new TableObjectChangedEventArgs<TextStyle>(oldTextStyle, newTextStyle);
                ae(this, eventArgs);
                return eventArgs.NewValue;
            }
            return newTextStyle;
        }

        public delegate void BlockChangedEventHandler(TableObject sender, TableObjectChangedEventArgs<Block> e);

        public event BlockChangedEventHandler BlockChanged;

        protected virtual Block OnBlockChangedEvent(Block oldBlock, Block newBlock)
        {
            BlockChangedEventHandler ae = this.BlockChanged;
            if (ae != null)
            {
                TableObjectChangedEventArgs<Block> eventArgs = new TableObjectChangedEventArgs<Block>(oldBlock, newBlock);
                ae(this, eventArgs);
                return eventArgs.NewValue;
            }
            return newBlock;
        }

        #endregion

        #region private fields

        // dimension and extension lines
        private AciColor dimclrd;
        private Linetype dimltype;
        private Lineweight dimlwd;
        private bool dimsd1;
        private bool dimsd2;
        private double dimdle;
        private double dimdli;

        private AciColor dimclre;
        private Linetype dimltex1;
        private Linetype dimltex2;
        private Lineweight dimlwe;
        private bool dimse1;
        private bool dimse2;
        private double dimexo;
        private double dimexe;
        private bool dimfxlon;
        private double dimfxl;

        // symbols and arrows
        private double dimasz;
        private double dimcen;
        private Block dimldrblk;
        private Block dimblk1;
        private Block dimblk2;

        // text
        private TextStyle dimtxsty;
        private AciColor dimclrt;
        private AciColor dimtfillclr;
        private double dimtxt;
        private DimensionStyleTextHorizontalPlacement dimjust;
        private DimensionStyleTextVerticalPlacement dimtad;
        private double dimgap;
        private bool dimtih;
        private bool dimtoh;

        // fit
        private bool dimtofl;
        private bool dimsoxd;
        private double dimscale;
        private DimensionStyleFitOptions dimatfit;
        private bool dimtix;
        private DimensionStyleFitTextMove dimtmove;

        // primary units
        private short dimadec;
        private short dimdec;
        private string dimPrefix;
        private string dimSuffix;
        private char dimdsep;
        private double dimlfac;
        private LinearUnitType dimlunit;
        private AngleUnitType dimaunit;
        private FractionFormatType dimfrac;

        private bool suppressLinearLeadingZeros;
        private bool suppressLinearTrailingZeros;
        private bool suppressZeroFeet;
        private bool suppressZeroInches;
        private bool suppressAngularLeadingZeros;
        private bool suppressAngularTrailingZeros;

        private double dimrnd;

        // alternate units
        private AlternateUnitsFormat alternateUnits;

        // tolerances
        private TolerancesFormat tolerances;

        #endregion

        #region constants

        /// <summary>
        /// Default dimension style name.
        /// </summary>
        public const string DefaultName = "Standard";

        /// <summary>
        /// Gets the default dimension style.
        /// </summary>
        public static DimensionStyle Default
        {
            get { return new DimensionStyle(DefaultName); }
        }

        /// <summary>
        /// Gets the ISO-25 dimension style as defined in AutoCad.
        /// </summary>
        public static DimensionStyle Iso25
        {
            get
            {
                DimensionStyle style = new DimensionStyle("ISO-25")
                {
                    DimBaselineSpacing = 3.75,
                    ExtLineExtend = 1.25,
                    ExtLineOffset = 0.625,
                    ArrowSize = 2.5,
                    CenterMarkSize = 2.5,
                    TextHeight = 2.5,
                    TextOffset = 0.625,
                    TextOutsideAlign = true,
                    TextInsideAlign = true,
                    TextVerticalPlacement = DimensionStyleTextVerticalPlacement.Above,
                    FitDimLineForce = true,
                    DecimalSeparator = ',',
                    LengthPrecision = 2,
                    SuppressLinearTrailingZeros = true,
                    AlternateUnits =
                    {
                        LengthPrecision = 3,
                        Multiplier = 0.0394
                    },
                    Tolerances =
                    {
                        VerticalPlacement = TolerancesVerticalPlacement.Bottom,
                        Precision = 2,
                        SuppressLinearTrailingZeros = true,
                        AlternatePrecision = 3
                    }
                };
                return style;
            }
        }

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>DimensionStyle</c> class.
        /// </summary>
        /// <param name="name">The dimension style name.</param>
        public DimensionStyle(string name)
            : this(name, true)
        {
        }

        internal DimensionStyle(string name, bool checkName)
            : base(name, DxfObjectCode.DimStyle, checkName)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name), "The dimension style name should be at least one character long.");

            this.IsReserved = name.Equals(DefaultName, StringComparison.OrdinalIgnoreCase);

            // dimension and extension lines
            this.dimclrd = AciColor.ByBlock;
            this.dimltype = Linetype.ByBlock;
            this.dimlwd = Lineweight.ByBlock;
            this.dimdle = 0.0;
            this.dimdli = 0.38;
            this.dimsd1 = false;
            this.dimsd2 = false;

            this.dimclre = AciColor.ByBlock;
            this.dimltex1 = Linetype.ByBlock;
            this.dimltex2 = Linetype.ByBlock;
            this.dimlwe = Lineweight.ByBlock;
            this.dimse1 = false;
            this.dimse2 = false;
            this.dimexo = 0.0625;
            this.dimexe = 0.18;
            this.dimfxlon = false;
            this.dimfxl = 1.0;

            // symbols and arrows
            this.dimldrblk = null;
            this.dimblk1 = null;
            this.dimblk2 = null;
            this.dimasz = 0.18;
            this.dimcen = 0.09;

            // text
            this.dimtxsty = TextStyle.Default;
            this.dimclrt = AciColor.ByBlock;
            this.dimtfillclr = null;
            this.dimtxt = 0.18;
            this.dimtad = DimensionStyleTextVerticalPlacement.Centered;
            this.dimjust = DimensionStyleTextHorizontalPlacement.Centered;
            this.dimgap = 0.09;
            this.dimtih = false;
            this.dimtoh = false;

            // fit
            this.dimtofl = false;
            this.dimsoxd = true;
            this.dimscale = 1.0;
            this.dimatfit = DimensionStyleFitOptions.BestFit;
            this.dimtix = false;
            this.dimtmove = DimensionStyleFitTextMove.BesideDimLine;

            // primary units
            this.dimdec = 4;
            this.dimadec = 0;
            this.dimPrefix = string.Empty;
            this.dimSuffix = string.Empty;
            this.dimdsep = '.';
            this.dimlfac = 1.0;
            this.dimaunit = AngleUnitType.DecimalDegrees;
            this.dimlunit = LinearUnitType.Decimal;
            this.dimfrac = FractionFormatType.Horizontal;
            this.suppressLinearLeadingZeros = false;
            this.suppressLinearTrailingZeros = false;
            this.suppressZeroFeet = true;
            this.suppressZeroInches = true;
            this.suppressAngularLeadingZeros = false;
            this.suppressAngularTrailingZeros = false;
            this.dimrnd = 0.0;

            // alternate units
            this.alternateUnits = new AlternateUnitsFormat();

            // tolerances
            this.tolerances = new TolerancesFormat();
        }

        #endregion

        #region public properties

        #region dimension and extension lines

        /// <summary>
        /// Assigns colors to dimension lines, arrowheads, and dimension leader lines.
        /// </summary>
        /// <remarks>
        /// Default: ByBlock<br />
        /// Only indexed AciColors are supported.
        /// </remarks>
        public AciColor DimLineColor
        {
            get { return this.dimclrd; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                this.dimclrd = value;
            }
        }

        /// <summary>
        /// Sets the line type of the dimension line.
        /// </summary>
        /// <remarks>Default: ByBlock</remarks>
        public Linetype DimLineLinetype
        {
            get { return this.dimltype; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                this.dimltype = this.OnLinetypeChangedEvent(this.dimltype, value);
            }
        }

        /// <summary>
        /// Assigns line weight to dimension lines.
        /// </summary>
        /// <remarks>Default: ByBlock</remarks>
        public Lineweight DimLineLineweight
        {
            get { return this.dimlwd; }
            set { this.dimlwd = value; }
        }

        /// <summary>
        /// Suppresses display of the first dimension line.
        /// </summary>
        /// <remarks>
        /// Default: false <br />
        /// To completely suppress the dimension line set both <c>DimLine1Off</c> and <c>DimLine2Off</c> to false.
        /// </remarks>
        public bool DimLine1Off
        {
            get { return this.dimsd1; }
            set { this.dimsd1 = value; }
        }

        /// <summary>
        /// Suppresses display of the second dimension line.
        /// </summary>
        /// <remarks>
        /// Default: false <br />
        /// To completely suppress the dimension line set both <c>DimLine1Off</c> and <c>DimLine2Off</c> to false.
        /// </remarks>
        public bool DimLine2Off
        {
            get { return this.dimsd2; }
            set { this.dimsd2 = value; }
        }

        /// <summary>
        /// Sets the distance the dimension line extends beyond the extension line when oblique, architectural tick, integral, or no marks are drawn for arrowheads.
        /// </summary>
        /// <remarks>Default: 0.0</remarks>
        public double DimLineExtend
        {
            get { return this.dimdle; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The DIMDLE must be equals or greater than zero.");
                this.dimdle = value;
            }
        }

        /// <summary>
        /// Controls the spacing of the dimension lines in baseline dimensions.
        /// </summary>
        /// <remarks>
        /// Default: 0.38<br />
        /// This value is stored only for information purposes.
        /// Base dimensions are a compound entity made of several dimensions, there is no actual dxf entity that represents that.
        /// </remarks>
        public double DimBaselineSpacing
        {
            get { return this.dimdli; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The DIMDLI must be equals or greater than zero.");
                this.dimdli = value;
            }
        }

        /// <summary>
        /// Assigns colors to extension lines, center marks, and centerlines.
        /// </summary>
        /// <remarks>
        /// Default: ByBlock<br />
        /// Only indexed AciColors are supported.
        /// </remarks>
        public AciColor ExtLineColor
        {
            get { return this.dimclre; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                this.dimclre = value;
            }
        }

        /// <summary>
        /// Sets the line type of the first extension line.
        /// </summary>
        /// <remarks>Default: ByBlock</remarks>
        public Linetype ExtLine1Linetype
        {
            get { return this.dimltex1; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                this.dimltex1 = this.OnLinetypeChangedEvent(this.dimltex1, value);
            }
        }

        /// <summary>
        /// Sets the line type of the second extension line.
        /// </summary>
        /// <remarks>Default: ByBlock</remarks>
        public Linetype ExtLine2Linetype
        {
            get { return this.dimltex2; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                this.dimltex2 = this.OnLinetypeChangedEvent(this.dimltex2, value);
            }
        }

        /// <summary>
        /// Assigns line weight to extension lines.
        /// </summary>
        /// <remarks>Default: ByBlock</remarks>
        public Lineweight ExtLineLineweight
        {
            get { return this.dimlwe; }
            set { this.dimlwe = value; }
        }

        /// <summary>
        /// Suppresses display of the first extension line.
        /// </summary>
        /// <remarks>Default: false</remarks>
        public bool ExtLine1Off
        {
            get { return this.dimse1; }
            set { this.dimse1 = value; }
        }

        /// <summary>
        /// Suppresses display of the second extension line.
        /// </summary>
        /// <remarks>Default: false</remarks>
        public bool ExtLine2Off
        {
            get { return this.dimse2; }
            set { this.dimse2 = value; }
        }

        /// <summary>
        /// Specifies how far extension lines are offset from origin points.
        /// </summary>
        /// <remarks>Default: 0.0625</remarks>
        public double ExtLineOffset
        {
            get { return this.dimexo; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The DIMEXO must be equals or greater than zero.");
                this.dimexo = value;
            }
        }

        /// <summary>
        /// Specifies how far to extend the extension line beyond the dimension line.
        /// </summary>
        /// <remarks>Default: 0.18</remarks>
        public double ExtLineExtend
        {
            get { return this.dimexe; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The DIMEXE must be equals or greater than zero.");
                this.dimexe = value;
            }
        }

        /// <summary>
        /// Enables fixed length extension lines.
        /// </summary>
        public bool ExtLineFixed
        {
            get { return this.dimfxlon; }
            set { this.dimfxlon = value; }
        }

        /// <summary>
        /// Gets or sets the total length of the extension lines starting from the dimension line toward the dimension origin. 
        /// </summary>
        public double ExtLineFixedLength
        {
            get { return this.dimfxl; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The DIMFXL must be equals or greater than zero.");
                this.dimfxl = value;
            }
        }

        #endregion

        #region symbols and arrows

        /// <summary>
        /// Gets or sets the arrowhead block for the first end of the dimension line.
        /// </summary>
        /// <remarks>Default: null. Closed filled.</remarks>
        public Block DimArrow1
        {
            get { return this.dimblk1; }
            set { this.dimblk1 = value == null ? null : this.OnBlockChangedEvent(this.dimblk1, value); }
        }

        /// <summary>
        /// Gets or sets the arrowhead block for the second end of the dimension line.
        /// </summary>
        /// <remarks>Default: null. Closed filled.</remarks>
        public Block DimArrow2
        {
            get { return this.dimblk2; }
            set { this.dimblk2 = value == null ? null : this.OnBlockChangedEvent(this.dimblk2, value); }
        }

        /// <summary>
        /// Gets or sets the arrowhead block for leaders.
        /// </summary>
        /// <remarks>Default: null. Closed filled.</remarks>
        public Block LeaderArrow
        {
            get { return this.dimldrblk; }
            set { this.dimldrblk = value == null ? null : this.OnBlockChangedEvent(this.dimldrblk, value); }
        }

        /// <summary>
        /// Controls the size of dimension line and leader line arrowheads. Also controls the size of hook lines.
        /// </summary>
        /// <remarks>Default: 0.18</remarks>
        public double ArrowSize
        {
            get { return this.dimasz; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The DIMASZ must be equals or greater than zero.");
                this.dimasz = value;
            }
        }

        /// <summary>
        /// Controls drawing of circle or arc center marks and centerlines.
        /// </summary>
        /// <remarks>
        /// 0 - No center marks or lines are drawn.<br />
        /// greater than 0 - Center marks are drawn.<br />
        /// lower than 0 - Center marks and centerlines are drawn.<br />
        /// The absolute value specifies the size of the center mark or centerline. 
        /// The size of the centerline is the length of the centerline segment that extends outside the circle or arc.
        /// It is also the size of the gap between the center mark and the start of the centerline. 
        /// The size of the center mark is the distance from the center of the circle or arc to the end of the center mark.<br/>
        /// Default: 0.09
        /// </remarks>
        public double CenterMarkSize
        {
            get { return this.dimcen; }
            set { this.dimcen = value; }
        }

        #endregion

        #region text appearance

        /// <summary>
        /// Gets or sets the text style of the dimension.
        /// </summary>
        /// <remarks>Default: Standard</remarks>
        public TextStyle TextStyle
        {
            get { return this.dimtxsty; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                this.dimtxsty = this.OnTextStyleChangedEvent(this.dimtxsty, value);
            }
        }

        /// <summary>
        /// Gets or set the color of dimension text.
        /// </summary>
        /// <remarks>
        /// Default: ByBlock<br />
        /// Only indexed AciColors are supported.
        /// </remarks>
        public AciColor TextColor
        {
            get { return this.dimclrt; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                this.dimclrt = value;
            }
        }

        /// <summary>
        /// Gets or set the background color of dimension text. Set to null to specify no color.
        /// </summary>
        /// <remarks>
        /// Default: null<br />
        /// Only indexed AciColors are supported.
        /// </remarks>
        public AciColor TextFillColor
        {
            get { return this.dimtfillclr; }
            set { this.dimtfillclr = value; }
        }

        /// <summary>
        /// Specifies the height of dimension text, unless the current text style has a fixed height.
        /// </summary>
        /// <remarks>Default: 0.18</remarks>
        public double TextHeight
        {
            get { return this.dimtxt; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The DIMTXT must be greater than zero.");
                this.dimtxt = value;
            }
        }

        /// <summary>
        /// Controls the horizontal positioning of dimension text.
        /// </summary>
        /// <remarks>
        /// Default: Centered <br/>
        /// Not implemented in the dimension drawing.
        /// </remarks>
        public DimensionStyleTextHorizontalPlacement TextHorizontalPlacement
        {
            get { return this.dimjust; }
            set { this.dimjust = value; }
        }

        /// <summary>
        /// Controls the vertical position of text in relation to the dimension line.
        /// </summary>
        /// <remarks>
        /// Default: Above<br/>
        /// Not implemented in the dimension drawing.
        /// </remarks>
        public DimensionStyleTextVerticalPlacement TextVerticalPlacement
        {
            get { return this.dimtad; }
            set { this.dimtad = value; }
        }

        /// <summary>
        /// Gets or sets the distance around the dimension text when the dimension line breaks to accommodate dimension text.
        /// </summary>
        /// <remarks>
        /// Default: 0.09<br />
        /// Displays a rectangular frame around the dimension text when negative values are used.
        /// </remarks>
        public double TextOffset
        {
            get { return this.dimgap; }
            set { this.dimgap = value; }
        }

        /// <summary>
        /// Gets or sets the positioning of the dimension text inside extension lines.
        /// </summary>
        /// <remarks>
        /// Default: false<br/>
        /// Not implemented in the dimension drawing.
        /// </remarks>
        public bool TextInsideAlign
        {
            get { return this.dimtih; }
            set { this.dimtih = value; }
        }

        /// <summary>
        /// Gets or sets the positioning of the dimension text outside extension lines.
        /// </summary>
        /// <remarks>
        /// Default: false<br/>
        /// Not implemented in the dimension drawing.
        /// </remarks>
        public bool TextOutsideAlign
        {
            get { return this.dimtoh; }
            set { this.dimtoh = value; }
        }

        #endregion

        #region fit

        /// <summary>
        /// Gets or sets the drawing of the dimension lines even when the text are placed outside the extension lines. 
        /// </summary>
        /// <remarks>
        /// Default: false <br/>
        /// Not implemented in the dimension drawing.
        /// </remarks>
        public bool FitDimLineForce
        {
            get { return this.dimtofl; }
            set { this.dimtofl = value; }
        }

        /// <summary>
        /// Gets or sets the drawing of dimension lines outside extension lines.
        /// </summary>
        /// <remarks>
        /// Default: true <br/>
        /// Not implemented in the dimension drawing.
        /// </remarks>
        public bool FitDimLineInside
        {
            get { return this.dimsoxd; }
            set { this.dimsoxd = value; }
        }

        /// <summary>
        /// Get or set the overall scale factor applied to dimensioning variables that specify sizes, distances, or offsets.
        /// </summary>
        /// <remarks>
        /// DIMSCALE does not affect measured lengths, coordinates, or angles.<br/>
        /// DIMSCALE values of zero are not supported, any imported drawing with a zero value will set the scale to the default 1.0.<br/>
        /// Default: 1.0
        /// </remarks>
        public double DimScaleOverall
        {
            get { return this.dimscale; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The DIMSCALE must be greater than zero.");
                this.dimscale = value;
            }
        }

        /// <summary>
        /// Gets or sets the placement of text and arrowheads based on the space available between the extension lines.
        /// </summary>
        /// <remarks>
        /// Default: BestFit <br/>
        /// Not implemented in the dimension drawing.
        /// </remarks>
        public DimensionStyleFitOptions FitOptions
        {
            get { return this.dimatfit; }
            set { this.dimatfit = value; }
        }

        /// <summary>
        /// Gets or sets the drawing of text between the extension lines.
        /// </summary>
        /// <remarks>
        /// Default: false <br/>
        /// Not implemented in the dimension drawing.
        /// </remarks>
        public bool FitTextInside
        {
            get { return this.dimtix; }
            set { this.dimtix = value; }
        }

        /// <summary>
        /// Gets or sets the position of the text when it's moved either manually or automatically.
        /// </summary>
        /// <remarks>
        /// Default: BesideDimLine <br/>
        /// Not implemented in the dimension drawing.
        /// </remarks>
        public DimensionStyleFitTextMove FitTextMove
        {
            get { return this.dimtmove; }
            set { this.dimtmove = value; }
        }

        #endregion

        #region primary units

        /// <summary>
        /// Controls the number of precision places displayed in angular dimensions.
        /// </summary>
        /// <remarks>
        /// Default: 0<br/>
        /// If set to -1 angular dimensions display the number of decimal places specified by DIMDEC.
        /// It is recommended to use values in the range 0 to 8.
        /// </remarks>
        public short AngularPrecision
        {
            get { return this.dimadec; }
            set
            {
                if (value < -1)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The DIMADEC must be greater than -1.");
                this.dimadec = value;
            }
        }

        /// <summary>
        /// Sets the number of decimal places displayed for the primary units of a dimension.
        /// </summary>
        /// <remarks>
        /// Default: 2<br/>
        /// It is recommended to use values in the range 0 to 8.<br/>
        /// For architectural and fractional the precision used for the minimum fraction is 1/2^LinearDecimalPlaces.
        /// </remarks>
        public short LengthPrecision
        {
            get { return this.dimdec; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The DIMDEC must be equals or greater than zero.");
                this.dimdec = value;
            }
        }

        /// <summary>
        /// Specifies the text prefix for the dimension.
        /// </summary>
        public string DimPrefix
        {
            get { return this.dimPrefix; }
            set { this.dimPrefix = value ?? string.Empty; }
        }

        /// <summary>
        /// Specifies the text suffix for the dimension.
        /// </summary>
        public string DimSuffix
        {
            get { return this.dimSuffix; }
            set { this.dimSuffix = value ?? string.Empty; }
        }

        /// <summary>
        /// Specifies a single-character decimal separator to use when creating dimensions whose unit format is decimal.
        /// </summary>
        /// <remarks>Default: "."</remarks>
        public char DecimalSeparator
        {
            get { return this.dimdsep; }
            set { this.dimdsep = value; }
        }

        /// <summary>
        /// Gets or sets a scale factor for linear dimension measurements
        /// </summary>
        /// <remarks>
        /// All linear dimension distances, including radii, diameters, and coordinates, are multiplied by DIMLFAC before being converted to dimension text.<br />
        /// Positive values of DIMLFAC are applied to dimensions in both model space and paper space; negative values are applied to paper space only.<br />
        /// DIMLFAC has no effect on angular dimensions.
        /// </remarks>
        public double DimScaleLinear
        {
            get { return this.dimlfac; }
            set
            {
                if (MathHelper.IsZero(value))
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The scale factor cannot be zero.");
                this.dimlfac = value;
            }
        }

        /// <summary>
        /// Gets or sets the units for all dimension types except angular.
        /// </summary>
        /// <remarks>
        /// Scientific<br/>
        /// Decimal<br/>
        /// Engineering<br/>
        /// Architectural<br/>
        /// Fractional
        /// </remarks>
        public LinearUnitType DimLengthUnits
        {
            get { return this.dimlunit; }
            set { this.dimlunit = value; }
        }

        /// <summary>
        /// Gets or sets the units format for angular dimensions.
        /// </summary>
        /// <remarks>
        /// Decimal degrees<br/>
        /// Degrees/minutes/seconds<br/>
        /// Gradians<br/>
        /// Radians
        /// </remarks>
        public AngleUnitType DimAngularUnits
        {
            get { return this.dimaunit; }
            set
            {
                if (value == AngleUnitType.SurveyorUnits)
                    throw new ArgumentException("Surveyor's units are not applicable in angular dimensions.");
                this.dimaunit = value;
            }
        }

        /// <summary>
        /// Gets or sets the fraction format when DIMLUNIT is set to Architectural or Fractional.
        /// </summary>
        /// <remarks>
        /// Horizontal stacking<br/>
        /// Diagonal stacking<br/>
        /// Not stacked (for example, 1/2)
        /// </remarks>
        public FractionFormatType FractionalType
        {
            get { return this.dimfrac; }
            set { this.dimfrac = value; }
        }

        /// <summary>
        /// Suppresses leading zeros in linear decimal dimensions (for example, 0.5000 becomes .5000).
        /// </summary>
        /// <remarks>This value is part of the DIMZIN variable.</remarks>
        public bool SuppressLinearLeadingZeros
        {
            get { return this.suppressLinearLeadingZeros; }
            set { this.suppressLinearLeadingZeros = value; }
        }

        /// <summary>
        /// Suppresses trailing zeros in linear decimal dimensions (for example, 12.5000 becomes 12.5).
        /// </summary>
        /// <remarks>This value is part of the DIMZIN variable.</remarks>
        public bool SuppressLinearTrailingZeros
        {
            get { return this.suppressLinearTrailingZeros; }
            set { this.suppressLinearTrailingZeros = value; }
        }

        /// <summary>
        /// Suppresses zero feet in architectural dimensions.
        /// </summary>
        /// <remarks>This value is part of the DIMZIN variable.</remarks>
        public bool SuppressZeroFeet
        {
            get { return this.suppressZeroFeet; }
            set { this.suppressZeroFeet = value; }
        }

        /// <summary>
        /// Suppresses zero inches in architectural dimensions.
        /// </summary>
        /// <remarks>This value is part of the DIMZIN variable.</remarks>
        public bool SuppressZeroInches
        {
            get { return this.suppressZeroInches; }
            set { this.suppressZeroInches = value; }
        }

        /// <summary>
        /// Suppresses leading zeros in angular decimal dimensions (for example, 0.5000 becomes .5000).
        /// </summary>
        /// <remarks>This value is part of the DIMAZIN variable.</remarks>
        public bool SuppressAngularLeadingZeros
        {
            get { return this.suppressAngularLeadingZeros; }
            set { this.suppressAngularLeadingZeros = value; }
        }

        /// <summary>
        /// Suppresses trailing zeros in angular decimal dimensions (for example, 12.5000 becomes 12.5).
        /// </summary>
        /// <remarks>This value is part of the DIMAZIN variable.</remarks>
        public bool SuppressAngularTrailingZeros
        {
            get { return this.suppressAngularTrailingZeros; }
            set { this.suppressAngularTrailingZeros = value; }
        }

        /// <summary>
        /// Gets or sets the value to round all dimensioning distances.
        /// </summary>
        /// <remarks>
        /// Default: 0 (no rounding off).<br/>
        /// If DIMRND is set to 0.25, all distances round to the nearest 0.25 unit.
        /// If you set DIMRND to 1.0, all distances round to the nearest integer.
        /// Note that the number of digits edited after the decimal point depends on the precision set by DIMDEC.
        /// DIMRND does not apply to angular dimensions.
        /// </remarks>
        public double DimRoundoff
        {
            get { return this.dimrnd; }
            set
            {
                if (value < 0.000001 && !MathHelper.IsZero(value, double.Epsilon))
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The nearest value to round all distances must be equal or greater than 0.000001 or zero (no rounding off).");
                this.dimrnd = value;
            }
        }

        #endregion

        #region alternate units

        /// <summary>
        /// Gets the alternate units format for dimensions.
        /// </summary>
        /// <remarks>Alternative units are not applicable for angular dimensions.</remarks>
        public AlternateUnitsFormat AlternateUnits
        {
            get { return this.alternateUnits; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                this.alternateUnits = value;
            }
        }

        #endregion

        #region tolerances

        /// <summary>
        /// Gets the tolerances format for dimensions.
        /// </summary>
        public TolerancesFormat Tolerances
        {
            get { return this.tolerances; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                this.tolerances = value;
            }
        }

        #endregion

        /// <summary>
        /// Gets the owner of the actual dimension style.
        /// </summary>
        public new DimensionStyles Owner
        {
            get { return (DimensionStyles) base.Owner; }
            internal set { base.Owner = value; }
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
            DimensionStyle copy = new DimensionStyle(newName)
            {
                // dimension lines
                DimLineColor = (AciColor) this.dimclrd.Clone(),
                DimLineLinetype = (Linetype) this.dimltype.Clone(),
                DimLineLineweight = this.dimlwd,
                DimLine1Off = this.dimsd1,
                DimLine2Off = this.dimsd2,
                DimBaselineSpacing = this.dimdli,
                DimLineExtend = this.dimdle,

                // extension lines
                ExtLineColor = (AciColor) this.dimclre.Clone(),
                ExtLine1Linetype = (Linetype) this.dimltex1.Clone(),
                ExtLine2Linetype = (Linetype) this.dimltex2.Clone(),
                ExtLineLineweight = this.dimlwe,
                ExtLine1Off = this.dimse1,
                ExtLine2Off = this.dimse2,
                ExtLineOffset = this.dimexo,
                ExtLineExtend = this.dimexe,

                // symbols and arrows
                ArrowSize = this.dimasz,
                CenterMarkSize = this.dimcen,

                // text appearance
                TextStyle = (TextStyle) this.dimtxsty.Clone(),
                TextColor = (AciColor) this.dimclrt.Clone(),
                TextFillColor = (AciColor) this.dimtfillclr?.Clone(),
                TextHeight = this.dimtxt,
                TextHorizontalPlacement = this.dimjust,
                TextVerticalPlacement = this.dimtad,
                TextOffset = this.dimgap,

                // fit
                FitDimLineForce = this.dimtofl,
                FitDimLineInside = this.dimsoxd,
                DimScaleOverall = this.dimscale,
                FitOptions = this.dimatfit,
                FitTextInside = this.dimtix,
                FitTextMove = this.dimtmove,

                // primary units
                AngularPrecision = this.dimadec,
                LengthPrecision = this.dimdec,
                DimPrefix = this.dimPrefix,
                DimSuffix = this.dimSuffix,
                DecimalSeparator = this.dimdsep,
                DimScaleLinear = this.dimlfac,
                DimLengthUnits = this.dimlunit,
                DimAngularUnits = this.dimaunit,
                FractionalType = this.dimfrac,
                SuppressLinearLeadingZeros = this.suppressLinearLeadingZeros,
                SuppressLinearTrailingZeros = this.suppressLinearTrailingZeros,
                SuppressZeroFeet = this.suppressZeroFeet,
                SuppressZeroInches = this.suppressZeroInches,
                SuppressAngularLeadingZeros = this.suppressAngularLeadingZeros,
                SuppressAngularTrailingZeros = this.suppressAngularTrailingZeros,
                DimRoundoff = this.dimrnd,

                // alternate units
                AlternateUnits = (AlternateUnitsFormat) this.alternateUnits.Clone(),

                // tolerances
                Tolerances = (TolerancesFormat) this.tolerances.Clone()
            };


            if (this.dimldrblk != null) copy.LeaderArrow = (Block) this.dimldrblk.Clone();
            if (this.dimblk1 != null) copy.DimArrow1 = (Block) this.dimblk1.Clone();
            if (this.dimblk2 != null) copy.DimArrow2 = (Block) this.dimblk2.Clone();

            foreach (XData data in this.XData.Values)
                copy.XData.Add((XData)data.Clone());

            return copy;
        }

        /// <summary>
        /// Creates a new DimensionStyle that is a copy of the current instance.
        /// </summary>
        /// <returns>A new DimensionStyle that is a copy of this instance.</returns>
        public override object Clone()
        {
            return this.Clone(this.Name);
        }

        #endregion
    }
}