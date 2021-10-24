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

namespace netDxf.Entities
{
    /// <summary>
    /// Tolerance Material Condition.
    /// </summary>
    public enum ToleranceMaterialCondition
    {
        /// <summary>
        /// No material condition.
        /// </summary>
        None,

        /// <summary>
        /// Maximum Material Condition (MMC). A feature contains the maximum amount of material stated in the limits. 
        /// </summary>
        Maximum,

        /// <summary>
        /// Least Material Condition (LMC). A feature contains the minimum amount of material stated in the limits. 
        /// </summary>
        Least,

        /// <summary>
        /// Regardless of feature size. A feature can be any size within the stated limits. 
        /// </summary>
        Regardless
    }
}