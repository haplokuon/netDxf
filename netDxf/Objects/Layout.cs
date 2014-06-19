#region netDxf, Copyright(C) 2014 Daniel Carvajal, Licensed under LGPL.

//                        netDxf library
// Copyright (C) 2014 Daniel Carvajal (haplokuon@gmail.com)
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
using netDxf.Blocks;
using netDxf.Collections;
using netDxf.Entities;
using netDxf.Tables;

namespace netDxf.Objects
{
    /// <summary>
    /// Represents a layout.
    /// </summary>
    public class Layout :
        TableObject
    {

        #region private fields

        private PlotSettings plot;
        private Vector2 minLimit;
        private Vector2 maxLimit;
        private Vector3 minExtents;
        private Vector3 maxExtents;
        private Vector3 basePoint;
        private double elevation;
        private Vector3 origin;
        private Vector3 xAxis;
        private Vector3 yAxis;
        private int tabOrder;
        private Viewport viewport;
        private readonly bool isPaperSpace;
        private static readonly Layout modelSpace;
        private Block associatedBlock;
        #endregion

        #region constants

        /// <summary>
        /// Gets the ModelSpace layout.
        /// </summary>
        /// <remarks>
        /// There can be only one model space layout and it is always called "Model".
        /// </remarks>
        public static Layout ModelSpace
        {
            get { return modelSpace; }
        }

        #endregion

        #region constructor

        static Layout()
        {
            modelSpace = new Layout("Model", Block.ModelSpace, new PlotSettings());
        }

        /// <summary>
        /// Initializes a new layout.
        /// </summary>
        /// <param name="name">Layout name.</param>
        public Layout(string name)
            : this(name, null, new PlotSettings())
        {
        }

        private Layout(string name, Block associatedBlock, PlotSettings plotSettings)
            : base(name, DxfObjectCode.Layout, true)
        {
            if (name.Equals("Model", StringComparison.OrdinalIgnoreCase))
            {
                this.reserved = true;
                this.isPaperSpace = false;
                this.tabOrder = 0;
                this.viewport = null;
                plotSettings.Flags = PlotFlags.Initializing | PlotFlags.UpdatePaper | PlotFlags.ModelType | PlotFlags.DrawViewportsFirst | PlotFlags.PrintLineweights | PlotFlags.PlotPlotStyles | PlotFlags.UseStandardScale;
            }
            else
            {
                this.reserved = false;
                this.isPaperSpace = true;
                this.tabOrder = 1;
                this.viewport = new Viewport(1) { ViewCenter = new Vector2(50.0, 100.0) };
            }
            this.associatedBlock = associatedBlock;
            this.plot = plotSettings;
            this.minLimit = new Vector2(-20.0, -7.5);
            this.maxLimit = new Vector2(277.0, 202.5);
            this.basePoint = Vector3.Zero;
            this.minExtents = new Vector3(25.7, 19.5, 0.0);
            this.maxExtents = new Vector3(231.3, 175.5, 0.0);
            this.elevation = 0;
            this.origin = Vector3.Zero;
            this.xAxis = Vector3.UnitX;
            this.yAxis = Vector3.UnitY;
        }
        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the tab order.
        /// </summary>
        /// <remarks>
        /// Tab order. This number is an ordinal indicating this layout's ordering in the tab control that is
        /// attached to the AutoCAD drawing frame window. Note that the "Model" tab always appears
        /// as the first tab regardless of its tab order (always zero).
        /// </remarks>
        public int TabOrder
        {
            get { return this.tabOrder; }
            set
            {
                if (value <= 0)
                    throw new ArgumentException("The tab order index must be greater than zero.", "value");
                this.tabOrder = value;
            }
        }

        /// <summary>
        /// Gets or sets the plot settings
        /// </summary>
        public PlotSettings PlotSettings
        {
            get { return this.plot; }
            set { this.plot = value; }
        }

        /// <summary>
        /// Gets or sets the minimum limits for this layout.
        /// </summary>
        public Vector2 MinLimit
        {
            get { return this.minLimit; }
            set { this.minLimit = value; }
        }

        /// <summary>
        /// Gets or sets the maximum limits for this layout.
        /// </summary>
        public Vector2 MaxLimit
        {
            get { return this.maxLimit; }
            set { this.maxLimit = value; }
        }

        /// <summary>
        /// Gets or sets the maximum extents for this layout.
        /// </summary>
        public Vector3 MinExtents
        {
            get { return this.minExtents; }
            set { this.minExtents = value; }
        }

        /// <summary>
        /// Gets or sets the maximum extents for this layout.
        /// </summary>
        public Vector3 MaxExtents
        {
            get { return this.maxExtents; }
            set { this.maxExtents = value; }
        }

        /// <summary>
        /// Gets or sets the insertion base point for this layout.
        /// </summary>
        public Vector3 BasePoint
        {
            get { return this.basePoint; }
            set { this.basePoint = value; }
        }

        /// <summary>
        /// Gets or sets the elevation.
        /// </summary>
        public double Elevation
        {
            get { return this.elevation; }
            set { this.elevation = value; }
        }

        /// <summary>
        /// Gets or sets the UCS origin.
        /// </summary>
        public Vector3 UcsOrigin
        {
            get { return this.origin; }
            set { this.origin = value; }
        }

        /// <summary>
        /// Gets or sets the UCS X axis.
        /// </summary>
        public Vector3 UcsXAxis
        {
            get { return this.xAxis; }
            set { this.xAxis = value; }
        }

        /// <summary>
        /// Gets or sets the UCS Y axis.
        /// </summary>
        public Vector3 UcsYAxis
        {
            get { return this.yAxis; }
            set { this.yAxis = value; }
        }

        /// <summary>
        /// Defines if this layout is a paper space.
        /// </summary>
        public bool IsPaperSpace
        {
            get { return this.isPaperSpace; }
        }

        /// <summary>
        /// Gets the viewport associated with this layout. This is the viewport with Id 1 that represents the paperspace itself, it has no graphical representation, and does not show the model.
        /// </summary>
        /// <remarks>The ModelSpace layout does not requiere a viewport and it will always return null.</remarks>
        public Viewport Viewport
        {
            get { return this.viewport; }
            internal set { this.viewport = value; }
        }

        /// <summary>
        /// Gets the owner of the actual dxf object.
        /// </summary>
        public new Layouts Owner
        {
            get { return (Layouts)this.owner; }
            internal set { this.owner = value; }
        }

        /// <summary>
        /// Gets the associated ModelSpace or PaperSpace block.
        /// </summary>
        public Block AssociatedBlock
        {
            get { return this.associatedBlock; }
            internal set { this.associatedBlock = value; }
        }

        #endregion

        #region overrides

        /// <summary>
        /// Asigns a handle to the object based in a integer counter.
        /// </summary>
        /// <param name="entityNumber">Number to asign.</param>
        /// <returns>Next avaliable entity number.</returns>
        /// <remarks>
        /// Some objects might consume more than one, is, for example, the case of polylines that will asign
        /// automatically a handle to its vertexes. The entity number will be converted to an hexadecimal number.
        /// </remarks>
        internal override long AsignHandle(long entityNumber)
        {
            entityNumber = this.owner.AsignHandle(entityNumber);
            if(this.isPaperSpace) entityNumber = this.viewport.AsignHandle(entityNumber);
            return base.AsignHandle(entityNumber);
        }

        #endregion


    }
}