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

namespace netDxf.Objects
{
    /// <summary>
    /// Duplicate record cloning flag (determines how to merge duplicate entries).
    /// </summary>
    public enum DictionaryCloningFlags
    {
        /// <summary>
        /// Not applicable.
        /// </summary>
        NotApplicable = 0,

        /// <summary>
        /// Keep existing.
        /// </summary>
        KeepExisting = 1,

        /// <summary>
        /// Use clone.
        /// </summary>
        UseClone = 2,

        /// <summary>
        /// External reference name.
        /// </summary>
        XrefName = 3,

        /// <summary>
        /// Name.
        /// </summary>
        Name = 4,

        /// <summary>
        /// Unmangle name.
        /// </summary>
        UnmangleName = 5
    }
}