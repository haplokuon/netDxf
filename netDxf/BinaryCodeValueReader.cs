#region netDxf, Copyright(C) 2014 Daniel Carvajal, Licensed under LGPL.

//                        netDxf library
// Copyright (C) 2014 Daniel Carvajal (haplokuon@gmail.com)
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
using System.IO;
using System.Text;

namespace netDxf
{
    internal class BinaryCodeValueReader :
        ICodeValueReader
    {
        private readonly BinaryReader reader;
        private short code;
        private object value;

        public BinaryCodeValueReader(BinaryReader reader)
        {
            this.reader = reader;
            byte[] sentinel = this.reader.ReadBytes(22);
            StringBuilder sb = new StringBuilder(18);
            for (int i = 0; i < 18; i++)
            {
                sb.Append((char)sentinel[i]);
            }
            if (sb.ToString() != "AutoCAD Binary DXF")
                throw new ArgumentException("Not a valid binary dxf.");

            this.code = 0;
            this.value = null;
        }

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
            get { return this.reader.BaseStream.Position; }
        }

        public void Next()
        {
            code = this.reader.ReadInt16();
            if (code >= 0 && code <= 9) // string
                value = this.NullTerminatedString();
            else if (code >= 10 && code <= 39) // double precision 3D point value
                value = this.reader.ReadDouble();
            else if (code >= 40 && code <= 59) // double precision floating point value
                value = this.reader.ReadDouble();
            else if (code >= 60 && code <= 79) // 16-bit integer value
                value = this.reader.ReadInt16();
            else if (code >= 90 && code <= 99) // 32-bit integer value
                value = this.reader.ReadInt32();
            else if (code == 100) // string (255-character maximum; less for Unicode strings)
                value = this.NullTerminatedString();
            else if (code == 101) // string (255-character maximum; less for Unicode strings). This code is undocummented and seems to affect only the AcdsData in dxf version 2013
                value = this.NullTerminatedString();
            else if (code == 102) // string (255-character maximum; less for Unicode strings)
                value = this.NullTerminatedString();
            else if (code == 105) // string representing hexadecimal (hex) handle value
                value = this.NullTerminatedString();
            else if (code >= 110 && code <= 119) // double precision floating point value
                value = this.reader.ReadDouble();
            else if (code >= 120 && code <= 129) // double precision floating point value
                value = this.reader.ReadDouble();
            else if (code >= 130 && code <= 139) // double precision floating point value
                value = this.reader.ReadDouble();
            else if (code >= 140 && code <= 149) // double precision scalar floating-point value
                value = this.reader.ReadDouble();
            else if (code >= 160 && code <= 169) // 64-bit integer value
                value = this.reader.ReadInt64();
            else if (code >= 170 && code <= 179) // 16-bit integer value
                value = this.reader.ReadInt16();
            else if (code >= 210 && code <= 239) // double precision scalar floating-point value
                value = this.reader.ReadDouble();
            else if (code >= 270 && code <= 279) // 16-bit integer value
                value = this.reader.ReadInt16();
            else if (code >= 280 && code <= 289) // 16-bit integer value
                value = this.reader.ReadInt16();
            else if (code >= 290 && code <= 299) // byte (boolean flag value)
                value = this.reader.ReadByte();
            else if (code >= 300 && code <= 309) // arbitrary text string
                value = this.NullTerminatedString();
            else if (code >= 310 && code <= 319) // string representing hex value of binary chunk
                value = this.ReadBinaryData();
            else if (code >= 320 && code <= 329) // string representing hex handle value
                value = this.NullTerminatedString();
            else if (code >= 330 && code <= 369) // string representing hex object IDs
                value = this.NullTerminatedString();
            else if (code >= 370 && code <= 379) // 16-bit integer value
                value = this.reader.ReadInt16();
            else if (code >= 380 && code <= 389) // 16-bit integer value
                value = this.reader.ReadInt16();
            else if (code >= 390 && code <= 399) // string representing hex handle value
                value = this.NullTerminatedString();
            else if (code >= 400 && code <= 409) // 16-bit integer value
                value = this.reader.ReadInt16();
            else if (code >= 410 && code <= 419) // string
                value = this.NullTerminatedString();
            else if (code >= 420 && code <= 429) // 32-bit integer value
                value = this.reader.ReadInt32();
            else if (code >= 430 && code <= 439) // string
                value = this.NullTerminatedString();
            else if (code >= 440 && code <= 449) // 32-bit integer value
                value = this.reader.ReadInt32();
            else if (code >= 450 && code <= 459) // 32-bit integer value
                value = this.reader.ReadInt32();
            else if (code >= 460 && code <= 469) // double-precision floating-point value
                value = this.reader.ReadDouble();
            else if (code >= 470 && code <= 479) // string
                value = this.NullTerminatedString();
            else if (code >= 480 && code <= 481) // string representing hex handle value
                value = this.NullTerminatedString();
            else if (code == 999) // comment (string)
                throw new Exception(string.Format("The comment group, 999, is not used in binary DXF files at byte address {0}", this.reader.BaseStream.Position));
            else if (code >= 1010 && code <= 1059) // double-precision floating-point value
                value = this.reader.ReadDouble();
            else if (code >= 1000 && code <= 1003) // string (same limits as indicated with 0-9 code range)
                value = this.NullTerminatedString();
            else if (code == 1004) // string representing hex value of binary chunk
                value = this.ReadBinaryData();
            else if (code >= 1005 && code <= 1009) // string (same limits as indicated with 0-9 code range)
                value = this.NullTerminatedString();
            else if (code >= 1060 && code <= 1070) // 16-bit integer value
                value = this.reader.ReadInt16();
            else if (code == 1071) // 32-bit integer value
                value = this.reader.ReadInt32();
            else
                throw new Exception(string.Format("Code {0} not valid at byte address {1}", this.code, this.reader.BaseStream.Position));
        }

        private byte[] ReadBinaryData()
        {
            byte length = this.reader.ReadByte();
            return this.reader.ReadBytes(length);
        }

        private string NullTerminatedString()
        {
            byte c = this.reader.ReadByte();
            List<byte> bytes = new List<byte>();
            while (c != 0) // strings always end with a 0 byte (char NULL)
            {
                bytes.Add(c);
                c = this.reader.ReadByte();
            }
            return Encoding.UTF8.GetString(bytes.ToArray(), 0, bytes.Count);
        }

        public byte ReadByte()
        {
            return (byte)this.value;
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
            return (int)this.value;
        }

        public long ReadLong()
        {
            return (long) this.value;
        }

        public bool ReadBool()
        {
            byte result = ReadByte();
            return result > 0;
        }

        public double ReadDouble()
        {
            return (double)this.value;
        }

        public string ReadString()
        {
            return (string)this.value;
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", this.code, this.value);
        }
    }
}
