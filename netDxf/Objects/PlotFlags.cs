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

namespace netDxf.Objects
{
    /// <summary>
    /// Defines the plot settings flag.
    /// </summary>
    /// <remarks>Bit flag.</remarks>
    [Flags]
    public enum PlotFlags
    {
        /// <summary>
        /// Plot viewport borders.
        /// </summary>
        PlotViewportBorders = 1,

        /// <summary>
        /// Show plot styles.
        /// </summary>
        ShowPlotStyles = 2,

        /// <summary>
        /// Plot centered.
        /// </summary>
        PlotCentered = 4,

        /// <summary>
        /// Plot hidden.
        /// </summary>
        PlotHidden = 8,

        /// <summary>
        /// Use standard scale.
        /// </summary>
        UseStandardScale = 16,

        /// <summary>
        /// Plot styles.
        /// </summary>
        PlotPlotStyles = 32,

        /// <summary>
        /// Scale line weights.
        /// </summary>
        ScaleLineweights = 64,

        /// <summary>
        /// Print line weights.
        /// </summary>
        PrintLineweights = 128,

        /// <summary>
        /// Draw viewports first.
        /// </summary>
        DrawViewportsFirst = 512,

        /// <summary>
        /// Model type.
        /// </summary>
        ModelType = 1024,

        /// <summary>
        /// Update paper.
        /// </summary>
        UpdatePaper = 2048,

        /// <summary>
        /// Soon to paper on update.
        /// </summary>
        ZoomToPaperOnUpdate = 4096,

        /// <summary>
        /// Initializing.
        /// </summary>
        Initializing = 8192,

        /// <summary>
        /// Preview plot initialization.
        /// </summary>
        PrevPlotInit = 16384
    }
}