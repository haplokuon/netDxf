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

using System;
using System.Globalization;

namespace netDxf
{
    /// <summary>
    /// Represents an entry in the extended data of an entity.
    /// </summary>
    public class XDataRecord
    {
        #region private fields

        private readonly XDataCode code;
        private readonly object value;

        #endregion

        #region constants

        /// <summary>
        /// An extended data control string can be either "{"or "}".
        /// These braces enable applications to organize their data by subdividing the data into lists.
        /// The left brace begins a list, and the right brace terminates the most recent list. Lists can be nested.
        /// </summary>
        public static XDataRecord OpenControlString
        {
            get { return new XDataRecord(XDataCode.ControlString, "{"); }
        }

        /// <summary>
        /// An extended data control string can be either "{" or "}".
        /// These braces enable applications to organize their data by subdividing the data into lists.
        /// The left brace begins a list, and the right brace terminates the most recent list. Lists can be nested.
        /// </summary>
        public static XDataRecord CloseControlString
        {
            get { return new XDataRecord(XDataCode.ControlString, "}"); }
        }

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new XDataRecord.
        /// </summary>
        /// <param name="code">XData code.</param>
        /// <param name="value">XData value.</param>
        public XDataRecord(XDataCode code, object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            switch (code)
            {
                case XDataCode.AppReg:
                    throw new ArgumentException("An application registry cannot be an extended data record.", nameof(value));
                case XDataCode.BinaryData:
                    if (!(value is byte[] bytes))
                    {
                        throw new ArgumentException("The value of a XDataCode.BinaryData must be a byte array.", nameof(value));
                    }
                    if (bytes.Length > 127)
                    {
                        throw new ArgumentOutOfRangeException(nameof(value), value, "The maximum length of a XDataCode.BinaryData is 127, if larger divide the data into multiple XDataCode.BinaryData records.");
                    }
                    break;
                case XDataCode.ControlString:
                    if (!(value is string v))
                    {
                        throw new ArgumentException("The value of a XDataCode.ControlString must be a string.", nameof(value));
                    }
                    if (!string.Equals(v, "{") && !string.Equals(v, "}"))
                    {
                        throw new ArgumentException("The only valid values of a XDataCode.ControlString are { or }.", nameof(value));
                    }
                    break;
                case XDataCode.DatabaseHandle:
                    if (!(value is string handle))
                    {
                        throw new ArgumentException("The value of a XDataCode.DatabaseHandle must be an hexadecimal number.", nameof(value));
                    }
                    if (!long.TryParse(handle, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out long _))
                    {
                        throw new ArgumentException("The value of a XDataCode.DatabaseHandle must be an hexadecimal number.", nameof(value));
                    }
                    break;
                case XDataCode.Distance:
                case XDataCode.Real:
                case XDataCode.RealX:
                case XDataCode.RealY:
                case XDataCode.RealZ:
                case XDataCode.WorldSpacePositionX:
                case XDataCode.WorldSpacePositionY:
                case XDataCode.WorldSpacePositionZ:
                case XDataCode.ScaleFactor:
                case XDataCode.WorldDirectionX:
                case XDataCode.WorldDirectionY:
                case XDataCode.WorldDirectionZ:
                case XDataCode.WorldSpaceDisplacementX:
                case XDataCode.WorldSpaceDisplacementY:
                case XDataCode.WorldSpaceDisplacementZ:
                    if (!(value is double))
                    {
                        throw new ArgumentException(string.Format("The value of a XDataCode.{0} must be a {1}.", code, typeof (double)), nameof(value));
                    }
                    break;
                case XDataCode.Int16:
                    if (!(value is short))
                    {
                        throw new ArgumentException(string.Format("The value of a XDataCode.{0} must be a {1}.", code, typeof (short)), nameof(value));
                    }
                    break;
                case XDataCode.Int32:
                    if (!(value is int))
                    {
                        throw new ArgumentException(string.Format("The value of a XDataCode.{0} must be an {1}.", code, typeof (int)), nameof(value));
                    }
                    break;
                case XDataCode.LayerName:
                case XDataCode.String:
                    if (!(value is string text))
                    {
                        throw new ArgumentException(string.Format("The value of a XDataCode.{0} must be a {1}.", code, typeof (string)), nameof(value));
                    }
                    if (text.Length > 255)
                    {
                        throw new ArgumentOutOfRangeException(nameof(value), value, "The maximum length of a XDataCode.String is 255, if larger divide the data into multiple XDataCode.String records.");
                    }

                    break;
            }
            this.code = code;
            this.value = value;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the XData code.
        /// </summary>
        /// <remarks>The only valid values are the ones defined in the <see cref="XDataCode">XDataCode</see> class.</remarks>
        public XDataCode Code
        {
            get { return this.code; }
        }

        /// <summary>
        /// Gets the XData value.
        /// </summary>
        public object Value
        {
            get { return this.value; }
        }

        #endregion

        #region overrides

        /// <summary>
        /// Obtains a string that represents the XDataRecord.
        /// </summary>
        /// <returns>A string text.</returns>
        public override string ToString()
        {
            return string.Format("{0} - {1}", this.code, this.value);
        }

        #endregion
    }
}