#region netDxf library, Copyright (C) 2009-2019 Daniel Carvajal (haplokuon@gmail.com)

//                        netDxf library
// Copyright (C) 2009-2019 Daniel Carvajal (haplokuon@gmail.com)
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

namespace netDxf.Entities
{
    /// <summary>
    /// Gradient pattern types.
    /// </summary>
    public enum HatchGradientPatternType
    {
        /// <summary>
        /// Linear.
        /// </summary>
        [StringValue("LINEAR")]
        Linear = 0,

        /// <summary>
        /// Cylinder.
        /// </summary>
        [StringValue("CYLINDER")]
        Cylinder = 1,

        /// <summary>
        /// Inverse cylinder.
        /// </summary>
        [StringValue("INVCYLINDER")]
        InvCylinder = 2,

        /// <summary>
        /// Spherical.
        /// </summary>
        [StringValue("SPHERICAL")]
        Spherical = 3,

        /// <summary>
        /// Inverse spherical.
        /// </summary>
        [StringValue("INVSPHERICAL")]
        InvSpherical = 4,

        /// <summary>
        /// Hemispherical.
        /// </summary>
        [StringValue("HEMISPHERICAL")]
        Hemispherical = 5,

        /// <summary>
        /// Inverse hemispherical.
        /// </summary>
        [StringValue("INVHEMISPHERICAL")]
        InvHemispherical = 6,

        /// <summary>
        /// Curved.
        /// </summary>
        [StringValue("CURVED")]
        Curved = 7,

        /// <summary>
        /// Inverse curved.
        /// </summary>
        [StringValue("INVCURVED")]
        InvCurved = 8
    }
}