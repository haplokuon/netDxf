#region netDxf library licensed under the MIT License, Copyright © 2009-2021 Daniel Carvajal (haplokuon@gmail.com)
// 
//                        netDxf library
// Copyright © 2021 Daniel Carvajal (haplokuon@gmail.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software
// and associated documentation files (the “Software”), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;

namespace netDxf.Entities
{
    /// <summary>
    /// Represents an entry in a tolerance entity.
    /// </summary>
    /// <remarks>
    /// Each entry can be made of up to two tolerance values and three datum references, plus a symbol that represents the geometric characteristics.
    /// </remarks>
    public class ToleranceEntry :
        ICloneable
    {
        #region private fields

        private ToleranceGeometricSymbol geometricSymbol;
        private ToleranceValue tolerance1;
        private ToleranceValue tolerance2;
        private DatumReferenceValue datum1;
        private DatumReferenceValue datum2;
        private DatumReferenceValue datum3;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>ToleranceEntry</c> class.
        /// </summary>
        public ToleranceEntry()
        {
            this.geometricSymbol = ToleranceGeometricSymbol.None;
            this.tolerance1 = null;
            this.tolerance2 = null;
            this.datum1 = null;
            this.datum2 = null;
            this.datum3 = null;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the geometric characteristics symbol.
        /// </summary>
        public ToleranceGeometricSymbol GeometricSymbol
        {
            get { return this.geometricSymbol; }
            set { this.geometricSymbol = value; }
        }

        /// <summary>
        /// Gets or sets the first tolerance value.
        /// </summary>
        public ToleranceValue Tolerance1
        {
            get { return this.tolerance1; }
            set { this.tolerance1 = value; }
        }

        /// <summary>
        /// Gets or sets the second tolerance value.
        /// </summary>
        public ToleranceValue Tolerance2
        {
            get { return this.tolerance2; }
            set { this.tolerance2 = value; }
        }

        /// <summary>
        /// Gets or sets the first datum reference value.
        /// </summary>
        public DatumReferenceValue Datum1
        {
            get { return this.datum1; }
            set { this.datum1 = value; }
        }

        /// <summary>
        /// Gets or sets the second datum reference value.
        /// </summary>
        public DatumReferenceValue Datum2
        {
            get { return this.datum2; }
            set { this.datum2 = value; }
        }

        /// <summary>
        /// Gets or sets the third datum reference value.
        /// </summary>
        public DatumReferenceValue Datum3
        {
            get { return this.datum3; }
            set { this.datum3 = value; }
        }

        #endregion

        #region ICloneable

        /// <summary>
        /// Creates a new ToleranceEntry that is a copy of the current instance.
        /// </summary>
        /// <returns>A new ToleranceEntry that is a copy of this instance.</returns>
        public object Clone()
        {
            return new ToleranceEntry
            {
                GeometricSymbol = this.geometricSymbol,
                Tolerance1 = (ToleranceValue) this.tolerance1?.Clone(),
                Tolerance2 = (ToleranceValue) this.tolerance2?.Clone(),
                Datum1 = (DatumReferenceValue) this.datum1?.Clone(),
                Datum2 = (DatumReferenceValue) this.datum2?.Clone(),
                Datum3 = (DatumReferenceValue) this.datum3?.Clone(),
            };
        }

        #endregion
    }
}