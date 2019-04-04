#region netDxf library, Copyright (C) 2009-2016 Daniel Carvajal (haplokuon@gmail.com)

//                        netDxf library
// Copyright (C) 2009-2016 Daniel Carvajal (haplokuon@gmail.com)
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
        /// <remarks>The text width factor will be automatically adjusted so the text will fit in the specified width.</remarks>
        Aligned,

        /// <summary>
        /// Middle (uses the center of the text including descenders).
        /// </summary>
        Middle,

        /// <summary>
        /// Fit.
        /// </summary>
        /// <remarks>The text height will be automatically adjusted so the text will fit in the specified width.</remarks>
        Fit
    }
}