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

namespace netDxf.IO
{
    internal interface ICodeValueWriter
    {
        /// <summary>
        /// Gets the DXF code.
        /// </summary>
        short Code { get; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        object Value { get; }

        /// <summary>
        /// Gets the line that has been written.
        /// </summary>
        long CurrentPosition { get; }

        void Write(short code, object value);
        void WriteByte(byte value);
        void WriteBytes(byte[] value);
        void WriteShort(short value);
        void WriteInt(int value);
        void WriteLong(long value);
        void WriteBool(bool value);
        void WriteDouble(double value);
        void WriteString(string value);
        void Flush();
    }
}