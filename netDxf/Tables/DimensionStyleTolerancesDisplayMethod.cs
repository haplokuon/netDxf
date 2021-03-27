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

namespace netDxf.Tables
{
    /// <summary>
    /// Defines the method for calculating the tolerance.
    /// </summary>
    /// <remarks>
    /// The Basic method for displaying tolerances in dimensions is not available,
    /// use a negative number for the <c>TextOffset</c> of the dimension style. The result is exactly the same.
    /// </remarks>
    public enum DimensionStyleTolerancesDisplayMethod
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
}