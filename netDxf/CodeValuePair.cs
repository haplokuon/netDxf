#region netDxf, Copyright(C) 2009 Daniel Carvajal, Licensed under LGPL.

//                        netDxf library
// Copyright (C) 2009 Daniel Carvajal (haplokuon@gmail.com)
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

namespace netDxf
{
    /// <summary>
    /// Represents the minimun information element in a dxf file.
    /// </summary>
    internal struct CodeValuePair
    {
        private readonly int code;
        private readonly string value;

        /// <summary>
        /// Initalizes a new instance of the <c>CodeValuePair</c> class.
        /// </summary>
        /// <param name="code">Dxf code.</param>
        /// <param name="value">Value for the specified code.</param>
        public CodeValuePair(int code, string value)
        {
            this.code = code;
            this.value = value;
        }

        /// <summary>
        /// Gets the dxf code.
        /// </summary>
        public int Code
        {
            get { return this.code; }
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        public string Value
        {
            get { return this.value; }
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", this.code, this.value);
        }
    }
}