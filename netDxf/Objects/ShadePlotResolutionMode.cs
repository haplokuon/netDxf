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

namespace netDxf.Objects
{
    /// <summary>
    /// Defines the shade plot resolution mode.
    /// </summary>
    public enum ShadePlotResolutionMode
    {
        /// <summary>
        /// Draft.
        /// </summary>
        Draft = 0,

        /// <summary>
        /// Preview.
        /// </summary>
        Preview = 1,

        /// <summary>
        /// Normal.
        /// </summary>
        Normal = 2,

        /// <summary>
        /// Presentation.
        /// </summary>
        Presentation = 3,

        /// <summary>
        /// Maximum
        /// </summary>
        Maximum = 4,

        /// <summary>
        /// Custom as specified by the shade plot DPI.
        /// </summary>
        Custom = 5
    }
}