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
using System.Globalization;
using System.IO;
using System.Text;

namespace netDxf.IO
{
    internal static class EncodingType
    {
        public static Encoding GetType(Stream fs)
        {
            byte[] unicode = {0xFF, 0xFE, 0x41};
            byte[] unicodeBig = {0xFE, 0xFF, 0x00};
            byte[] utf8 = {0xEF, 0xBB, 0xBF}; //with BOM
            Encoding reVal = Encoding.ASCII; //.Default;

            BinaryReader r = new BinaryReader(fs, Encoding.Default);
            if (!int.TryParse(fs.Length.ToString(CultureInfo.InvariantCulture), out int i))
            {
                return null;
            }

            byte[] ss = r.ReadBytes(i);
            if (IsUTF8Bytes(ss) || (ss[0] == utf8[0] && ss[1] == utf8[1] && ss[2] == utf8[2]))
            {
                reVal = Encoding.UTF8;
            }
            else if (ss[0] == unicodeBig[0] && ss[1] == unicodeBig[1] && ss[2] == unicodeBig[2])
            {
                reVal = Encoding.BigEndianUnicode;
            }
            else if (ss[0] == unicode[0] && ss[1] == unicode[1] && ss[2] == unicode[2])
            {
                reVal = Encoding.Unicode;
            }
            return reVal;
        }

        private static bool IsUTF8Bytes(byte[] data)
        {
            int charByteCounter = 1;
            foreach (byte t in data)
            {
                byte curByte = t;
                if (charByteCounter == 1)
                {
                    if (curByte >= 0x80)
                    {
                        while (((curByte <<= 1) & 0x80) != 0)
                            charByteCounter++;

                        if (charByteCounter == 1 || charByteCounter > 6)
                            return false;
                    }
                }
                else
                {
                    if ((curByte & 0xC0) != 0x80)
                    {
                        return false;
                    }
                    charByteCounter--;
                }
            }

            if (charByteCounter > 1)
            {
                throw new Exception("Error byte format.");
            }

            return true;
        }
    }
}