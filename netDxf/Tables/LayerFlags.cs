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

namespace netDxf.Tables
{
    /// <summary>
    /// Standard layer flags (bit-coded values).
    /// </summary>
    [Flags]
    internal enum LayerFlags
    {
        /// <summary>
        /// Default.
        /// </summary>
        None = 0,

        /// <summary>
        /// Layer is frozen; otherwise layer is thawed.
        /// </summary>
        Frozen = 1,

        /// <summary>
        /// Layer is frozen by default in new viewports.
        /// </summary>
        FrozenNewViewports = 2,

        /// <summary>
        /// Layer is locked.
        /// </summary>
        Locked = 4,

        /// <summary>
        /// If set, table entry is externally dependent on an xRef.
        /// </summary>
        XrefDependent = 16,

        /// <summary>
        /// If both this bit and bit 16 are set, the externally dependent xRef has been successfully resolved.
        /// </summary>
        XrefResolved = 32,

        /// <summary>
        /// If set, the table entry was referenced by at least one entity in the drawing the last time the 
        /// drawing was edited. (This flag is for the benefit of AutoCAD commands. It can be ignored by 
        /// most programs that read DXF files and need not be set by programs that write DXF files)
        /// </summary>
        Referenced = 64
    }
}