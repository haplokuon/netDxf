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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace netDxf.IO
{
    internal class TextCodeValueReader :
        ICodeValueReader
    {
        #region private fields

        private readonly TextReader reader;
        private short code;
        private object value;
        private long currentPosition;

        #endregion

        #region constructors

        public TextCodeValueReader(TextReader reader)
        {
            this.reader = reader;
            this.code = 0;
            this.value = null;
            this.currentPosition = 0;
        }

        #endregion

        #region public properties

        public short Code
        {
            get { return this.code; }
        }

        public object Value
        {
            get { return this.value; }
        }

        public long CurrentPosition
        {
            get { return this.currentPosition; }
        }

        #endregion

        #region public methods

        public void Next()
        {
            string readCode = this.reader.ReadLine();
            if (readCode == null)
            {
                this.code = 0;
                this.value = DxfObjectCode.EndOfFile;
            }
            else
            {
                this.currentPosition += 1;
                if (!short.TryParse(readCode, NumberStyles.Integer, CultureInfo.InvariantCulture, out this.code))
                {
                    throw new Exception(string.Format("Code {0} not valid at line {1}", this.code, this.currentPosition));
                }
                this.value = this.ReadValue(this.reader.ReadLine());
                this.currentPosition += 1;
            }
        }

        public byte ReadByte()
        {
            return (byte) this.value;
        }

        public byte[] ReadBytes()
        {
            return (byte[]) this.value;
        }

        public short ReadShort()
        {
            return (short) this.value;
        }

        public int ReadInt()
        {
            return (int) this.value;
        }

        public long ReadLong()
        {
            return (long) this.value;
        }

        public bool ReadBool()
        {
            return (bool) this.value;
        }

        public double ReadDouble()
        {
            return (double) this.value;
        }

        public string ReadString()
        {
            return (string) this.value;
        }

        public string ReadHex()
        {
            return (string) this.value;
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", this.code, this.value);
        }

        #endregion

        #region private methods

        private object ReadValue(string valueString)
        {
            if (this.code >= 0 && this.code <= 9) // string
            {
                return this.ReadString(valueString);
            }
            if (this.code >= 10 && this.code <= 39) // double precision 3D point value
            {
                return this.ReadDouble(valueString);
            }
            if (this.code >= 40 && this.code <= 59) // double precision floating point value
            {
                return this.ReadDouble(valueString);
            }
            if (this.code >= 60 && this.code <= 79) // 16-bit integer value
            {
                return this.ReadShort(valueString);
            }
            if (this.code >= 90 && this.code <= 99) // 32-bit integer value
            {
                return this.ReadInt(valueString);
            }
            if (this.code == 100) // string (255-character maximum; less for Unicode strings)
            {
                return this.ReadString(valueString);
            }
            if (this.code == 101) // string (255-character maximum; less for Unicode strings). This code is undocumented and seems to affect only the AcdsData in dxf version 2013
            {
                return this.ReadString(valueString);
            }
            if (this.code == 102) // string (255-character maximum; less for Unicode strings)
            {
                return this.ReadString(valueString);
            }
            if (this.code == 105) // string representing hexadecimal (hex) handle value
            {
                return this.ReadHex(valueString);
            }
            if (this.code >= 110 && this.code <= 119) // double precision floating point value
            {
                return this.ReadDouble(valueString);
            }
            if (this.code >= 120 && this.code <= 129) // double precision floating point value
            {
                return this.ReadDouble(valueString);
            }
            if (this.code >= 130 && this.code <= 139) // double precision floating point value
            {
                return this.ReadDouble(valueString);
            }
            if (this.code >= 140 && this.code <= 149) // double precision scalar floating-point value
            {
                return this.ReadDouble(valueString);
            }
            if (this.code >= 160 && this.code <= 169) // 64-bit integer value
            {
                return this.ReadLong(valueString);
            }
            if (this.code >= 170 && this.code <= 179) // 16-bit integer value
            {
                return this.ReadShort(valueString);
            }
            if (this.code >= 210 && this.code <= 239) // double precision scalar floating-point value
            {
                return this.ReadDouble(valueString);
            }
            if (this.code >= 270 && this.code <= 279) // 16-bit integer value
            {
                return this.ReadShort(valueString);
            }
            if (this.code >= 280 && this.code <= 289) // 16-bit integer value
            {
                return this.ReadShort(valueString);
            }
            if (this.code >= 290 && this.code <= 299) // byte (boolean flag value)
            {
                return this.ReadBool(valueString);
            }
            if (this.code >= 300 && this.code <= 309) // arbitrary text string
            {
                return this.ReadString(valueString);
            }
            if (this.code >= 310 && this.code <= 319) // string representing hex value of binary chunk
            {
                return this.ReadBytes(valueString);
            }
            if (this.code >= 320 && this.code <= 329) // string representing hex handle value
            {
                return this.ReadHex(valueString);
            }
            if (this.code >= 330 && this.code <= 369) // string representing hex object IDs
            {
                return this.ReadHex(valueString);
            }
            if (this.code >= 370 && this.code <= 379) // 16-bit integer value
            {
                return this.ReadShort(valueString);
            }
            if (this.code >= 380 && this.code <= 389) // 16-bit integer value
            {
                return this.ReadShort(valueString);
            }
            if (this.code >= 390 && this.code <= 399) // string representing hex handle value
            {
                return this.ReadHex(valueString);
            }
            if (this.code >= 400 && this.code <= 409) // 16-bit integer value
            {
                return this.ReadShort(valueString);
            }
            if (this.code >= 410 && this.code <= 419) // string
            {
                return this.ReadString(valueString);
            }
            if (this.code >= 420 && this.code <= 429) // 32-bit integer value
            {
                return this.ReadInt(valueString);
            }
            if (this.code >= 430 && this.code <= 439) // string
            {
                return this.ReadString(valueString);
            }
            if (this.code >= 440 && this.code <= 449) // 32-bit integer value
            {
                return this.ReadInt(valueString);
            }
            if (this.code >= 450 && this.code <= 459) // 32-bit integer value
            {
                return this.ReadInt(valueString);
            }
            if (this.code >= 460 && this.code <= 469) // double-precision floating-point value
            {
                return this.ReadDouble(valueString);
            }
            if (this.code >= 470 && this.code <= 479) // string
            {
                return this.ReadString(valueString);
            }
            if (this.code >= 480 && this.code <= 481) // string representing hex handle value
            {
                return this.ReadHex(valueString);
            }
            if (this.code == 999) // comment (string)
            {
                return this.ReadString(valueString);
            }
            if (this.code >= 1010 && this.code <= 1059) // double-precision floating-point value
            {
                return this.ReadDouble(valueString);
            }
            if (this.code >= 1000 && this.code <= 1003) // string (same limits as indicated with 0-9 code range)
            {
                return this.ReadString(valueString);
            }
            if (this.code == 1004) // string representing hex value of binary chunk
            {
                return this.ReadBytes(valueString);
            }
            if (this.code >= 1005 && this.code <= 1009) // string (same limits as indicated with 0-9 code range)
            {
                return this.ReadString(valueString);
            }
            if (this.code >= 1060 && this.code <= 1070) // 16-bit integer value
            {
                return this.ReadShort(valueString);
            }
            if (this.code == 1071) // 32-bit integer value
            {
                return this.ReadInt(valueString);
            }

            throw new Exception(string.Format("Code \"{0}\" not valid at line {1}", this.code, this.currentPosition));
        }

        //private byte ReadByte(string valueString)
        //{
        //    if (byte.TryParse(valueString, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture, out byte result))
        //    {
        //        return result;
        //    }

        //    Debug.Assert(false, string.Format("Value \"{0}\" not valid at line {1}", valueString, this.currentPosition));

        //    return 0;
        //}

        private byte[] ReadBytes(string valueString)
        {
            List<byte> bytes = new List<byte>();
            for (int i = 0; i < valueString.Length; i++)
            {
                string hex = string.Concat(valueString[i], valueString[++i]);
                if (byte.TryParse(hex, NumberStyles.AllowHexSpecifier | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture, out byte result))
                {
                    bytes.Add(result);
                }
                else
                {
                    Debug.Assert(false, string.Format("Value \"{0}\" not valid at line {1}", valueString, this.currentPosition));

                    return new byte[0];
                }
            }

            return bytes.ToArray();
        }

        private short ReadShort(string valueString)
        {
            if (short.TryParse(valueString, NumberStyles.Integer, CultureInfo.InvariantCulture, out short result))
            {
                return result;
            }

            Debug.Assert(false, string.Format("Value \"{0}\" not valid at line {1}", valueString, this.currentPosition));

            return 0;
        }

        private int ReadInt(string valueString)
        {
            if (int.TryParse(valueString, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
            {
                return result;
            }

            Debug.Assert(false, string.Format("Value \"{0}\" not valid at line {1}", valueString, this.currentPosition));

            return 0;
        }

        private long ReadLong(string valueString)
        {
            if (long.TryParse(valueString, NumberStyles.Integer, CultureInfo.InvariantCulture, out long result))
            {
                return result;
            }

            Debug.Assert(false, string.Format("Value \"{0}\" not valid at line {1}", valueString, this.currentPosition));

            return 0;
        }

        private bool ReadBool(string valueString)
        {
            if (byte.TryParse(valueString, NumberStyles.Integer, CultureInfo.InvariantCulture, out byte result))
            {
                return result > 0;
            }

            Debug.Assert(false, string.Format("Value \"{0}\" not valid at line {1}", valueString, this.currentPosition));

            return false;
        }

        private double ReadDouble(string valueString)
        {
            if (double.TryParse(valueString, NumberStyles.Float, CultureInfo.InvariantCulture, out double result))
            {
                return result;
            }

            Debug.Assert(false, string.Format("Value \"{0}\" not valid at line {1}", valueString, this.currentPosition));

            return 0.0;
        }

        private string ReadString(string valueString)
        {
            return valueString;
        }

        private string ReadHex(string valueString)
        {
            if (long.TryParse(valueString, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out long result))
            {
                return result.ToString("X");
            }

            Debug.Assert(false, string.Format("Value \"{0}\" not valid at line {1}", valueString, this.currentPosition));

            return string.Empty;
        }

        #endregion
    }
}