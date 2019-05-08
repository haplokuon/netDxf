#region netDxf library, Copyright (C) 2009-2019 Daniel Carvajal (haplokuon@gmail.com)

//                        netDxf library
// Copyright (C) 2009-2019 Daniel Carvajal (haplokuon@gmail.com)
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

using System;

namespace netDxf.Header
{
    /// <summary>
    /// Defines a header variable.
    /// </summary>
    public class HeaderVariable
    {
        #region private fields

        private readonly string name;
        private readonly short groupCode;
        private object variable;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>HeaderVariable</c> class.
        /// </summary>
        /// <param name="name">Header variable name.</param>
        /// <param name="groupCode">Header variable group code.</param>
        /// <param name="value">Header variable value.</param>
        /// <remarks>
        /// It is very important to match the group code with its corresponding value type,
        /// check the DXF documentation for details about what group code correspond to its associated type.
        /// For example, typical groups codes are 70, 40, and 2 that correspond to short, double, and string value types, respectively.<br />
        /// If the header value is a Vector3 use the group code 30, if it is a Vector2 use group code 20,
        /// when the variable is written to the DXF the codes 10, 20, and 30 will be added as necessary.
        /// </remarks>
        public HeaderVariable(string name, short groupCode, object value)
        {
            if(!name.StartsWith("$", StringComparison.InvariantCultureIgnoreCase))
                throw new ArgumentException("Header variable names always starts with '$'", nameof(name));
            this.name = name;
            this.groupCode = groupCode;
            this.variable = value;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the header variable name.
        /// </summary>
        /// <remarks>The header variable name is case insensitive.</remarks>
        public string Name
        {
            get { return this.name; }
        }

        /// <summary>
        /// Gets the header variable group code.
        /// </summary>
        public short GroupCode
        {
            get { return this.groupCode; }
        }

        /// <summary>
        /// Gets the header variable stored value.
        /// </summary>
        /// <remarks>
        /// It is very important to match the group code with its corresponding value type,
        /// check the DXF documentation for details about what group code correspond to its associated type.
        /// For example, typical groups codes are 70, 40, and 2 that correspond to short, double, and string value types, respectively.<br />
        /// If the header value is a Vector3 use the group code 30, if it is a Vector2 use group code 20,
        /// when the variable is written to the DXF the codes 10, 20, and 30 will be added as necessary.
        /// </remarks>
        public object Value
        {
            get { return this.variable; }
            set { this.variable = value; }
        }

        #endregion

        #region overrides

        /// <summary>
        /// Obtains a string that represents the header variable.
        /// </summary>
        /// <returns>A string text.</returns>
        public override string ToString()
        {
            return string.Format("{0}:{1}", this.name, this.variable);
        }

        #endregion
    }
}