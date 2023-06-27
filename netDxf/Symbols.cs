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

namespace netDxf
{
    /// <summary>
    /// Symbols for dxf text strings.
    /// </summary>
    /// <remarks>
    /// These special strings translates to symbols in AutoCad. They are obsolete since Unicode characters are supported.
    /// </remarks>
    public static class Symbols
    {
        /// <summary>
        /// Text string that shows as a diameter 'Ø' character.
        /// </summary>
        public const string Diameter = "%%c";

        /// <summary>
        /// Text string that shows as a degree '°' character.
        /// </summary>
        public const string Degree = "%%d";

        /// <summary>
        /// Text string that shows as a plus-minus '±' character.
        /// </summary>
        public const string PlusMinus = "%%p";
    }
}