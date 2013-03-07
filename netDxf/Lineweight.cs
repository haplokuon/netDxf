#region netDxf, Copyright(C) 2013 Daniel Carvajal, Licensed under LGPL.

//                        netDxf library
// Copyright (C) 2013 Daniel Carvajal (haplokuon@gmail.com)
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
using System.Globalization;

namespace netDxf
{

    /// <summary>
    /// Represents the lineweight of a layer or an entity.
    /// </summary>
    public class Lineweight
        : ICloneable
    {
        #region private fields

        private short weight;

        #endregion

        #region constants

        /// <summary>
        /// Gets the ByLayer lineweight.
        /// </summary>
        public static Lineweight ByLayer
        {
            get { return new Lineweight { weight = -1 }; }
        }

        /// <summary>
        /// Gets the ByBlock lineweight.
        /// </summary>
        public static Lineweight ByBlock
        {
            get { return new Lineweight { weight = -2 }; }
        }

        /// <summary>
        /// Gets the Default lineweight.
        /// </summary>
        public static Lineweight Default
        {
            get { return new Lineweight { weight = -3 }; }
        }

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>Lineweight</c> class.
        /// </summary>
        public Lineweight()
        {
            this.weight = -1;
        }

        /// <summary>
        /// Initializes a new instance of the <c>Lineweight</c> class.
        /// </summary>
        /// <param name="weight">Lineweight value range from 0 to 200.</param>
        /// <remarks>
        /// Accepted lineweight values range from 0 to 200, the reserved values - 1, -2, and -3 represents ByLayer, ByBlock, and Default lineweights.
        /// </remarks>
        public Lineweight(short weight)
        {
            if (weight < 0 || weight > 200)
                throw new ArgumentOutOfRangeException("weight", weight, "Accepted lineweight values range from 0 to 200.");
            this.weight = weight;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the line weight value range from 0 to 200, one unit is always 1/100 mm.
        /// </summary>
        /// <remarks>
        /// Accepted lineweight values range from 0 to 200, the reserved values - 1, -2, and -3 represents ByLayer, ByBlock, and Default lineweights.
        /// </remarks>
        public short Value
        {
            get { return weight; }
            set
            {
                if (value < 0 || value > 200)
                    throw new ArgumentOutOfRangeException("value", value, "Accepted lineweight values range from 0 to 200.");
                this.weight = value;
            }
        }

        #endregion

        #region implements ICloneable

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public Object Clone()
        {
            return new Lineweight
                       {
                           weight = this.weight
                       };
        }

        #endregion

        #region overrides

        /// <summary>
        /// Converts the value of this instance to its equivalent string representation.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            if (this.weight == -3)
                return "Default";
            if (this.weight == -2)
                return "ByBlock";
            if (this.weight == -1)
                return "ByLayer";

            return this.weight.ToString(CultureInfo.InvariantCulture);
        }

        #endregion

        #region internal methods

        internal static Lineweight FromCadIndex(short index)
        {
            Lineweight lineweight;
            switch (index)
            {
                case -3:
                    lineweight = Default;
                    break;
                case -2:
                    lineweight = ByBlock;
                    break;
                case -1:
                    lineweight = ByLayer;
                    break;
                default:
                    lineweight = new Lineweight(index);
                    break;
            }

            return lineweight;
        }
        #endregion
    }
}
