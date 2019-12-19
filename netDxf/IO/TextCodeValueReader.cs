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
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace netDxf.IO
{
    internal class TextCodeValueReader :
        ICodeValueReader
    {
        #region constructors

        public TextCodeValueReader(TextReader reader)
        {
            this.reader = reader;
            code = 0;
            value = null;
            currentPosition = 0;
        }

        #endregion

        #region private fields

        private readonly TextReader reader;
        private short code;
        private object value;
        private long currentPosition;

        #endregion

        #region public properties

        public short Code => code;

        public object Value => value;

        public long CurrentPosition => currentPosition;

        #endregion

        #region public methods

        public void Next()
        {
            var readCode = reader.ReadLine();
            currentPosition += 1;

            if (!short.TryParse(readCode, NumberStyles.Integer, CultureInfo.InvariantCulture, out code))
            {
                throw new Exception(string.Format("Code {0} not valid at line {1}", code, currentPosition));
            }

            value = ReadValue(reader.ReadLine());
            currentPosition += 1;
        }

        public byte ReadByte()
        {
            return (byte)value;
        }

        public byte[] ReadBytes()
        {
            return (byte[])value;
        }

        public short ReadShort()
        {
            return (short)value;
        }

        public int ReadInt()
        {
            return (int)value;
        }

        public long ReadLong()
        {
            return (long)value;
        }

        public bool ReadBool()
        {
            return (bool) value;
        }

        public double ReadDouble()
        {
            return (double)value;
        }

        public string ReadString()
        {
            return (string)value;
        }

        public string ReadHex()
        {
            return (string)value;
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", code, value);
        }

        #endregion

        #region private methods

        private object ReadValue(string valueString)
        {
            if (code >= 0 && code <= 9) // string
            {
                return ReadString(valueString);
            }
            if (code >= 10 && code <= 39) // double precision 3D point value
            {
                return ReadDouble(valueString);
            }
            if (code >= 40 && code <= 59) // double precision floating point value
            {
                return ReadDouble(valueString);
            }
            if (code >= 60 && code <= 79) // 16-bit integer value
            {
                return ReadShort(valueString);
            }
            if (code >= 90 && code <= 99) // 32-bit integer value
            {
                return ReadInt(valueString);
            }
            if (code == 100) // string (255-character maximum; less for Unicode strings)
            {
                return ReadString(valueString);
            }
            if (code == 101) // string (255-character maximum; less for Unicode strings). This code is undocumented and seems to affect only the AcdsData in dxf version 2013
            {
                return ReadString(valueString);
            }
            if (code == 102) // string (255-character maximum; less for Unicode strings)
            {
                return ReadString(valueString);
            }
            if (code == 105) // string representing hexadecimal (hex) handle value
            {
                return ReadHex(valueString);
            }
            if (code >= 110 && code <= 119) // double precision floating point value
            {
                return ReadDouble(valueString);
            }
            if (code >= 120 && code <= 129) // double precision floating point value
            {
                return ReadDouble(valueString);
            }
            if (code >= 130 && code <= 139) // double precision floating point value
            {
                return ReadDouble(valueString);
            }
            if (code >= 140 && code <= 149) // double precision scalar floating-point value
            {
                return ReadDouble(valueString);
            }
            if (code >= 160 && code <= 169) // 64-bit integer value
            {
                return ReadLong(valueString);
            }
            if (code >= 170 && code <= 179) // 16-bit integer value
            {
                return ReadShort(valueString);
            }
            if (code >= 210 && code <= 239) // double precision scalar floating-point value
            {
                return ReadDouble(valueString);
            }
            if (code >= 270 && code <= 279) // 16-bit integer value
            {
                return ReadShort(valueString);
            }
            if (code >= 280 && code <= 289) // 16-bit integer value
            {
                return ReadShort(valueString);
            }
            if (code >= 290 && code <= 299) // byte (boolean flag value)
            {
                return ReadBool(valueString);
            }
            if (code >= 300 && code <= 309) // arbitrary text string
            {
                return ReadString(valueString);
            }
            if (code >= 310 && code <= 319) // string representing hex value of binary chunk
            {
                return ReadBytes(valueString);
            }
            if (code >= 320 && code <= 329) // string representing hex handle value
            {
                return ReadHex(valueString);
            }
            if (code >= 330 && code <= 369) // string representing hex object IDs
            {
                return ReadHex(valueString);
            }
            if (code >= 370 && code <= 379) // 16-bit integer value
            {
                return ReadShort(valueString);
            }
            if (code >= 380 && code <= 389) // 16-bit integer value
            {
                return ReadShort(valueString);
            }
            if (code >= 390 && code <= 399) // string representing hex handle value
            {
                return ReadHex(valueString);
            }
            if (code >= 400 && code <= 409) // 16-bit integer value
            {
                return ReadShort(valueString);
            }
            if (code >= 410 && code <= 419) // string
            {
                return ReadString(valueString);
            }
            if (code >= 420 && code <= 429) // 32-bit integer value
            {
                return ReadInt(valueString);
            }
            if (code >= 430 && code <= 439) // string
            {
                return ReadString(valueString);
            }
            if (code >= 440 && code <= 449) // 32-bit integer value
            {
                return ReadInt(valueString);
            }
            if (code >= 450 && code <= 459) // 32-bit integer value
            {
                return ReadInt(valueString);
            }
            if (code >= 460 && code <= 469) // double-precision floating-point value
            {
                return ReadDouble(valueString);
            }
            if (code >= 470 && code <= 479) // string
            {
                return ReadString(valueString);
            }
            if (code >= 480 && code <= 481) // string representing hex handle value
            {
                return ReadHex(valueString);
            }
            if (code == 999) // comment (string)
            {
                return ReadString(valueString);
            }
            if (code >= 1010 && code <= 1059) // double-precision floating-point value
            {
                return ReadDouble(valueString);
            }
            if (code >= 1000 && code <= 1003) // string (same limits as indicated with 0-9 code range)
            {
                return ReadString(valueString);
            }
            if (code == 1004) // string representing hex value of binary chunk
            {
                return ReadBytes(valueString);
            }
            if (code >= 1005 && code <= 1009) // string (same limits as indicated with 0-9 code range)
            {
                return ReadString(valueString);
            }
            if (code >= 1060 && code <= 1070) // 16-bit integer value
            {
                return ReadShort(valueString);
            }
            if (code == 1071) // 32-bit integer value
            {
                return ReadInt(valueString);
            }

            throw new Exception(string.Format("Code {0} not valid at line {1}", code, currentPosition));
        }

        private byte ReadByte(string valueString)
        {
            byte result;

            if (byte.TryParse(valueString, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite,
                CultureInfo.InvariantCulture, out result))
            {
                return result;
            }

            throw new Exception(string.Format("Value {0} not valid at line {1}", valueString, this.currentPosition));
        }

        private byte[] ReadBytes(string valueString)
        {
            var bytes = new List<byte>();

            for (var i = 0; i < valueString.Length; i++)
            {
                var hex = string.Concat(valueString[i], valueString[++i]);
                byte result;

                if (byte.TryParse(hex,
                    NumberStyles.AllowHexSpecifier | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite,
                    CultureInfo.InvariantCulture, out result))
                {
                    bytes.Add(result);
                }
                else
                {
                    throw new Exception(string.Format("Value {0} not valid at line {1}", hex, this.currentPosition));
                }
            }

            return bytes.ToArray();
        }

        private short ReadShort(string valueString)
        {
            short result;

            if (short.TryParse(valueString, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
            {
                return result;
            }

            throw new Exception(string.Format("Value {0} not valid at line {1}", valueString, this.currentPosition));
        }

        private int ReadInt(string valueString)
        {
            int result;

            if (int.TryParse(valueString, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
            {
                return result;
            }

            throw new Exception(string.Format("Value {0} not valid at line {1}", valueString, this.currentPosition));
        }

        private long ReadLong(string valueString)
        {
            long result;

            if (long.TryParse(valueString, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
            {
                return result;
            }

            throw new Exception(string.Format("Value {0} not valid at line {1}", valueString, this.currentPosition));
        }

        private bool ReadBool(string valueString)
        {
            byte result = this.ReadByte(valueString);
            return result > 0;
        }

        private double ReadDouble(string valueString)
        {
            double result;

            if (double.TryParse(valueString, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
            {
                return result;
            }

            throw new Exception(string.Format("Value {0} not valid at line {1}", valueString, this.currentPosition));
        }

        private string ReadString(string valueString)
        {
            return valueString;
        }

        private string ReadHex(string valueString)
        {
            long test;

            if (long.TryParse(valueString, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out test))
            {
                return test.ToString("X");
            }

            throw new Exception(string.Format("Value {0} not valid at line {1}", valueString, this.currentPosition));
        }

        #endregion
    }
}