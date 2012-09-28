#region netDxf, Copyright(C) 2012 Daniel Carvajal, Licensed under LGPL.

//                        netDxf library
// Copyright (C) 2012 Daniel Carvajal (haplokuon@gmail.com)
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

namespace netDxf.Header
{
    /// <summary>
    /// The AutoCAD drawing database version number.
    /// </summary>
    public enum DxfVersion
    {
        /// <summary>
        /// AutoCAD 12 DXF file.
        /// </summary>
        [StringValue("AC1009")] AutoCad12,
        /// <summary>
        /// AutoCAD 2000 DXF file.
        /// </summary>
        [StringValue("AC1015")] AutoCad2000,
        /// <summary>
        /// AutoCAD 2004 DXF file.
        /// </summary>
        [StringValue("AC1018")] AutoCad2004,
        /// <summary>
        /// AutoCAD 2007 DXF file.
        /// </summary>
        [StringValue("AC1021")] AutoCad2007,
        /// <summary>
        /// AutoCAD 2010 DXF file.
        /// </summary>
        [StringValue("AC1024")] AutoCad2010
    }
}