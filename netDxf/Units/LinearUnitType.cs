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

namespace netDxf.Units
{
    /// <summary>
    /// Linear units format for creating objects.
    /// </summary>
    public enum LinearUnitType
    {
        /// <summary>
        /// Scientific.
        /// </summary>
        Scientific = 1,

        /// <summary>
        /// Decimal.
        /// </summary>
        Decimal = 2,

        /// <summary>
        /// Engineering.
        /// </summary>
        Engineering = 3,

        /// <summary>
        /// Architectural.
        /// </summary>
        Architectural = 4,

        /// <summary>
        /// Fractional.
        /// </summary>
        Fractional = 5,

        /// <summary>
        /// Microsoft Windows Desktop (decimal format using Control Panel settings for decimal separator and number grouping symbols).
        /// </summary>
        WindowsDesktop = 6
    }
}