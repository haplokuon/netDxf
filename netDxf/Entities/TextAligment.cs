#region netDxf library licensed under the MIT License
// 
//                       netDxf library
// Copyright (c) Daniel Carvajal (haplokuon@gmail.com)
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
    /// Defines the text alignment.
    /// </summary>
    public enum TextAlignment
    {
        /// <summary>
        /// Top left.
        /// </summary>
        TopLeft,

        /// <summary>
        /// Top center.
        /// </summary>
        TopCenter,

        /// <summary>
        /// Top right.
        /// </summary>
        TopRight,

        /// <summary>
        /// Middle left.
        /// </summary>
        MiddleLeft,

        /// <summary>
        /// Middle center (uses the center of the text as uppercase characters).
        /// </summary>
        MiddleCenter,

        /// <summary>
        /// Middle right.
        /// </summary>
        MiddleRight,

        /// <summary>
        /// Bottom left.
        /// </summary>
        BottomLeft,

        /// <summary>
        /// Bottom center.
        /// </summary>
        BottomCenter,

        /// <summary>
        /// Bottom right.
        /// </summary>
        BottomRight,

        /// <summary>
        /// Baseline left.
        /// </summary>
        BaselineLeft,

        /// <summary>
        /// Baseline center.
        /// </summary>
        BaselineCenter,

        /// <summary>
        /// Baseline right.
        /// </summary>
        BaselineRight,

        /// <summary>
        /// Aligned.
        /// </summary>
        /// <remarks>The text height will be automatically adjusted so the text will fit in the specified width.</remarks>
        Aligned,

        /// <summary>
        /// Middle (uses the center of the text including descenders).
        /// </summary>
        Middle,

        /// <summary>
        /// Fit.
        /// </summary>
        /// <remarks>The text width factor will be automatically adjusted so the text will fit in the specified width.</remarks>
        Fit
    }
}