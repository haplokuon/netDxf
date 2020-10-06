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

using System;

namespace netDxf.Objects
{
    /// <summary>
    /// Layer properties to update when restoring the LayerState to the Layers list and vice versa.
    /// </summary>
    [Flags]
    public enum LayerPropertiesRestoreFlags
    {
        /// <summary>
        /// No flags, do not restore anything.
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
        /// Not implemented. To freeze a layer in a viewport add it to its FrozenLayers list.
        /// </remarks>
        NewVpFrozen = 16,

        /// <summary>
        /// Freeze layer in current viewport.
        /// </summary>
        /// <remarks>
        /// Not implemented. To freeze a layer in a viewport add it to its FrozenLayers list.
        /// </remarks>
        VpFrozen = 32,

        /// <summary>
        /// Layer color.
        /// </summary>
        Color = 64,

        /// <summary>
        /// Layer linetype.
        /// </summary>
        Linetype = 128,

        /// <summary>
        /// Layer lineweight.
        /// </summary>
        Lineweight = 256,

        /// <summary>
        /// Layer transparency.
        /// </summary>
        Transparency = 512,

        /// <summary>
        /// Layer plot style.
        /// </summary>
        /// <remarks>Not implemented.</remarks>
        PlotStyle = 1024,

        /// <summary>
        /// All flags, restore all layer properties.
        /// </summary>
        All = Hidden | Frozen | Locked | Plot | NewVpFrozen | VpFrozen | Color | Linetype | Lineweight | Transparency | PlotStyle
    }
}