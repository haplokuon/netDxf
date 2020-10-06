#region netDxf library, Copyright (C) 2009-2020 Daniel Carvajal (haplokuon@gmail.com)

//                        netDxf library
// Copyright (C) 2009-2020 Daniel Carvajal (haplokuon@gmail.com)
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

namespace netDxf.Objects
{
    internal sealed class XRecordEntry
    {
        private readonly int code;
        private readonly object value;

        public XRecordEntry(int code, object value)
        {
            // Autodesk doesn't know how to keep its own crap together. This is what the OFFICIAL documentation says about the code entries in a XRecord
            // quote "1-369 (except 5 and 105). These values can be used by an application in any way.
            // Xrecord objects are used to store and manage arbitrary data.
            // They are composed of DXF group codes with “normal object” groups (that is, non-xdata group codes),
            // ranging from 1 through 369 for supported ranges.
            // This object is similar in concept to xdata but is not limited by size or order." end quote
            // but in the case of layer states which information is saved as XRecords it allows for codes outside this range, for example
            // code 440 for the transparency and code 370 for the lineweight

            //if (code < 1 || code > 369 || code == 5 || code == 105)
            //{
            //    throw new ArgumentException("Valid XRecord entry codes range from 1 to 369 except 5 and 105", nameof(code));
            //}

            //if (!CheckCodeValuePair(code, value))
            //{
            //    throw new ArgumentException("Incorrect value type for the specified code", nameof(value));
            //}

            this.code = code;
            this.value = value;
        }

        public int Code
        {
            get { return this.code; }
        }

        public object Value
        {
            get { return this.value; }
        }

        //private static bool CheckCodeValuePair(int code, object value)
        //{
        //    if (code >= 1 && code <= 9) // string
        //    {
        //        if (!(value is string))
        //        {
        //            return false;
        //        }
        //    }
        //    else if (code >= 10 && code <= 39) // double precision 3D point value
        //    {
        //        if (!(value is double))
        //        {
        //            return false;
        //        }
        //    }
        //    else if (code >= 40 && code <= 59) // double precision floating point value
        //    {
        //        if (!(value is double))
        //        {
        //            return false;
        //        }
        //    }
        //    else if (code >= 60 && code <= 79) // 16-bit integer value
        //    {
        //        if (!(value is short))
        //        {
        //            return false;
        //        }
        //    }
        //    else if (code >= 90 && code <= 99) // 32-bit integer value
        //    {
        //        if (!(value is int))
        //        {
        //            return false;
        //        }
        //    }
        //    else if (code == 100) // string (255-character maximum; less for Unicode strings)
        //    {
        //        if (!(value is string))
        //        {
        //            return false;
        //        }
        //    }
        //    else if (code == 101) // string (255-character maximum; less for Unicode strings). This code is undocumented and seems to affect only the AcdsData in dxf version 2013
        //    {
        //        if (!(value is string))
        //        {
        //            return false;
        //        }
        //    }
        //    else if (code == 102) // string (255-character maximum; less for Unicode strings)
        //    {
        //        if (!(value is string))
        //        {
        //            return false;
        //        }
        //    }
        //    else if (code >= 110 && code <= 119) // double precision floating point value
        //    {
        //        if (!(value is double))
        //        {
        //            return false;
        //        }
        //    }
        //    else if (code >= 120 && code <= 129) // double precision floating point value
        //    {
        //        if (!(value is double))
        //        {
        //            return false;
        //        }
        //    }
        //    else if (code >= 130 && code <= 139) // double precision floating point value
        //    {
        //        if (!(value is double))
        //        {
        //            return false;
        //        }
        //    }
        //    else if (code >= 140 && code <= 149) // double precision scalar floating-point value
        //    {
        //        if (!(value is double))
        //        {
        //            return false;
        //        }
        //    }
        //    else if (code >= 160 && code <= 169) // 64-bit integer value
        //    {
        //        if (!(value is long))
        //        {
        //            return false;
        //        }
        //    }

        //    return true;
        //}
    }
}