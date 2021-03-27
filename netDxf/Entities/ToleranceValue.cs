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
    /// Represents a tolerance, indicates the amount by which the geometric characteristic can deviate from a perfect form.
    /// </summary>
    public class ToleranceValue :
        ICloneable
    {
        #region private fields

        private bool showDiameterSymbol;
        private string tolerance;
        private ToleranceMaterialCondition materialCondition;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>ToleranceValue</c> class.
        /// </summary>
        public ToleranceValue()
        {
            this.showDiameterSymbol = false;
            this.tolerance = string.Empty;
            this.materialCondition = ToleranceMaterialCondition.None;
        }

        /// <summary>
        /// Initializes a new instance of the <c>ToleranceValue</c> class.
        /// </summary>
        /// <param name="showDiameterSymbol">Show a diameter symbol before the tolerance value.</param>
        /// <param name="value">Tolerance value.</param>
        /// <param name="materialCondition">Tolerance material condition.</param>
        public ToleranceValue(bool showDiameterSymbol, string value, ToleranceMaterialCondition materialCondition)
        {
            this.showDiameterSymbol = showDiameterSymbol;
            this.tolerance = string.IsNullOrEmpty(value) ? string.Empty : value;
            this.materialCondition = materialCondition;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets if the tolerance diameter symbol will be shown.
        /// </summary>
        public bool ShowDiameterSymbol
        {
            get { return this.showDiameterSymbol; }
            set { this.showDiameterSymbol = value; }
        }

        /// <summary>
        /// Gets or sets the tolerance value.
        /// </summary>
        public string Value
        {
            get { return this.tolerance; }
            set { this.tolerance = string.IsNullOrEmpty(value) ? string.Empty : value; ; }
        }

        /// <summary>
        /// Gets or sets the tolerance material condition.
        /// </summary>
        public ToleranceMaterialCondition MaterialCondition
        {
            get { return this.materialCondition; }
            set { this.materialCondition = value; }
        }

        #endregion

        #region ICloneable

        /// <summary>
        /// Creates a new ToleranceValue that is a copy of the current instance.
        /// </summary>
        /// <returns>A new ToleranceValue that is a copy of this instance.</returns>
        public object Clone()
        {
            return new ToleranceValue
            {
                ShowDiameterSymbol = this.showDiameterSymbol,
                Value = this.tolerance,
                MaterialCondition = this.materialCondition
            };
        }

        #endregion
    }
}