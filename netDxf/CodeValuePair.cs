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
    public struct CodeValuePair
    {
        private readonly int code;
        private readonly string value;

        public CodeValuePair(int code, string value)
        {
            this.code = code;
            this.value = value;
        }

        public int Code
        {
            get { return this.code; }
        }

        public string Value
        {
            get { return this.value; }
        }

        //public static void Write(TextWriter writer, int codigo, object valor)
        //{
        //    CultureInfo cInfo = CultureInfo.CurrentCulture;
        //    Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        //    writer.WriteLine(codigo);
        //    writer.WriteLine(valor);
        //    Thread.CurrentThread.CurrentCulture = cInfo;
        //}

        //public static CodePair Read<T>(TextReader reader)
        //{
        //    CultureInfo cInfo = CultureInfo.CurrentCulture;
        //    Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        //    int code;
        //    string readCode = reader.ReadLine();
        //    if (!int.TryParse(readCode, out code))
        //    {
        //        throw (new DxfException("Invalid group code " + readCode));
        //    }
        //    Type type = typeof (T);
            
        //    //if (type is short)

        //    string value = reader.ReadLine();

            
        //    Thread.CurrentThread.CurrentCulture = cInfo;
        //    return new CodePair(code, value);
        //}

    }
}