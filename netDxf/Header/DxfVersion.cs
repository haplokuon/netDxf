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

namespace netDxf.Header
{
    /// <summary>
    /// The AutoCAD drawing database version number.
    /// </summary>
    public enum DxfVersion
    {
        /// <summary>
        /// Unknown AutoCAD DXF file version.
        /// </summary>
        [StringValue("Unknown")]
        Unknown,

        /// <summary>
        /// AutoCAD R1.1 DXF file. AutoCAD release 1.1.
        /// </summary>
        [StringValue("MC0.0")]
        AutoCad1_1,

        /// <summary>
        /// AutoCAD R1.2 DXF file. AutoCAD release 1.2.
        /// </summary>
        [StringValue("AC1.2")]
        AutoCad1_2,

        /// <summary>
        /// AutoCAD R1.4 DXF file. AutoCAD release 1.4.
        /// </summary>
        [StringValue("AC1.4")]
        AutoCad1_4,

        /// <summary>
        /// AutoCAD R2.0 DXF file. AutoCAD release 2.0.
        /// </summary>
        [StringValue("AC1.50")]
        AutoCad2_0,

        /// <summary>
        /// AutoCAD R2.10 DXF file. AutoCAD release 2.10.
        /// </summary>
        [StringValue("AC2.10")]
        AutoCad2_10,

        /// <summary>
        /// AutoCAD R2.5 DXF file. AutoCAD release 2.5.
        /// </summary>
        [StringValue("AC1002")]
        AutoCad2_5,

        /// <summary>
        /// AutoCAD R2.6 DXF file. AutoCAD release 2.6.
        /// </summary>
        [StringValue("AC1003")]
        AutoCad2_6,

        /// <summary>
        /// AutoCAD R9 DXF file. AutoCAD release 9.
        /// </summary>
        [StringValue("AC1004")]
        AutoCad9,

        /// <summary>
        /// AutoCAD R10 DXF file. AutoCAD release 10.
        /// </summary>
        [StringValue("AC1006")]
        AutoCad10,

        /// <summary>
        /// AutoCAD R11 and R12 DXF file. AutoCAD release 11/12 (LT R1/R2).
        /// </summary>
        [StringValue("AC1009")]
        AutoCad12,

        /// <summary>
        /// AutoCAD R13 DXF file. AutoCAD release 13 (LT95).
        /// </summary>
        [StringValue("AC1012")]
        AutoCad13,

        /// <summary>
        /// AutoCAD R14 DXF file. AutoCAD release 14, 14.01 (LT97/LT98).
        /// </summary>
        [StringValue("AC1014")]
        AutoCad14,

        /// <summary>
        /// AutoCAD 2000 DXF file. AutoCAD 2000/2000i/2002.
        /// </summary>
        [StringValue("AC1015")]
        AutoCad2000,

        /// <summary>
        /// AutoCAD 2004 DXF file. AutoCAD 2004/2005/2006.
        /// </summary>
        [StringValue("AC1018")]
        AutoCad2004,

        /// <summary>
        /// AutoCAD 2007 DXF file. AutoCAD 2007/2008/2009.
        /// </summary>
        [StringValue("AC1021")]
        AutoCad2007,

        /// <summary>
        /// AutoCAD 2010 DXF file. AutoCAD 2010/2011/2012.
        /// </summary>
        [StringValue("AC1024")]
        AutoCad2010,

        /// <summary>
        /// AutoCAD 2013 DXF file. AutoCAD 2013/2014/2015/2016/2017.
        /// </summary>
        [StringValue("AC1027")]
        AutoCad2013,

        /// <summary>
        /// AutoCAD 2018 DXF file. AutoCAD 2018/2019/2020/2021/2022/2023.
        /// </summary>
        [StringValue("AC1032")]
        AutoCad2018
    }
}