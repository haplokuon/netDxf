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
    [Flags]
    public enum ViewModeFlags
    {
        /// <summary>
        /// Turned off.
        /// </summary>
        Off = 0,

        /// <summary>
        /// Perspective view active.
        /// </summary>
        Perspective = 1,

        /// <summary>
        /// Front clipping on.
        /// </summary>
        FrontClippingPlane = 2,

        /// <summary>
        /// Back clipping on.
        /// </summary>
        BackClippingPlane = 4,

        /// <summary>
        /// UCS Follow mode on.
        /// </summary>
        UCSFollow = 8,

        /// <summary>
        /// Front clip not at eye. If on, the front clip distance (FRONTZ) determines the front clipping plane.
        /// If off, FRONTZ is ignored, and the front clipping plane is set to pass through the camera point (vectors behind the camera are not displayed).
        /// This flag is ignored if the front-clipping bit (2) is off.
        /// </summary>
        FrontClipNotAtEye = 16
    }
}