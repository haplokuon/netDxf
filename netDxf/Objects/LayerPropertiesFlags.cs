#region netDxf library licensed under the MIT License, Copyright © 2009-2021 Daniel Carvajal (haplokuon@gmail.com)
// 
//                        netDxf library
// Copyright © 2021 Daniel Carvajal (haplokuon@gmail.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software
// and associated documentation files (the “Software”), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;

namespace netDxf.Objects
{
    /// <summary>
    /// Layer properties flags.
    /// </summary>
    [Flags]
    public enum LayerPropertiesFlags
    {
        /// <summary>
        /// No flags equivalent to layer On / Thawed / Unlocked / No Plot / No NewVpFreeze / No VpFreeze.
        /// </summary>
        None = 0,

        /// <summary>
        /// Layer visibility flag On/Off.
        /// </summary>
        Hidden = 1,

        /// <summary>
        /// Layer freeze flag Frozen/Thawed.
        /// </summary>
        Frozen = 2,

        /// <summary>
        /// Layer lock flag Locked/Unlocked.
        /// </summary>
        Locked = 4,

        /// <summary>
        /// Layer plot flag Plot/NoPlot.
        /// </summary>
        Plot = 8,

        /// <summary>
        /// Freeze layer in newly created viewports.
        /// </summary>
        /// <remarks>
        /// Not currently implemented. To freeze a layer in a viewport add it to its FrozenLayers list.
        /// </remarks>
        NewVpFrozen = 16,

        /// <summary>
        /// Freeze layer in current viewport.
        /// </summary>
        /// <remarks>
        /// Not currently implemented. To freeze a layer in a viewport add it to its FrozenLayers list.
        /// </remarks>
        VpFrozen = 32
    }
}