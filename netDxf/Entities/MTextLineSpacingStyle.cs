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
    /// MText line spacing style.
    /// </summary>
    public enum MTextLineSpacingStyle
    {
        /// <summary>
        /// Default (only applicable in MTextParagraphOptions).
        /// </summary>
        /// <remarks>
        /// The default value defined by the MText property will be applied.
        /// </remarks>
        Default = 0,

        /// <summary>
        /// At least (taller characters will override)
        /// </summary>
        /// <remarks>
        /// Takes both the user specified arbitrary value and the text height to determine spacing.
        /// </remarks>
        AtLeast = 1,

        /// <summary>
        /// Exact (taller characters will not override)
        /// </summary>
        /// <remarks>
        /// Defines the space with an arbitrary unit value the user specifies. Changing text height will not affect line spacing.
        /// </remarks>
        Exact = 2,

        /// <summary>
        /// Multiple (only applicable in MTextParagraphOptions).
        /// </summary>
        /// <remarks>
        /// Instead of assigning a value to line spacing, you specify spacing according to text height.
        /// </remarks>
        Multiple = 3
    }
}