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
using System.Collections.Generic;
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
        TableObject,
        IComparable<Layout>
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
        private short tabOrder;
        private Viewport viewport;
        private readonly bool isPaperSpace;
        private Block associatedBlock;

        #endregion

        #region constants

        /// <summary>
        /// Layout ModelSpace name.
        /// </summary>
        public const string ModelSpaceName = "Model";

        /// <summary>
        /// Gets the ModelSpace layout.
        /// </summary>
        /// <remarks>
        /// There can be only one model space layout and it is always called "Model".
        /// </remarks>
        public static Layout ModelSpace
        {
            get { return new Layout(ModelSpaceName, Block.ModelSpace, new PlotSettings()); }
        }

        #endregion

        #region constructor

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
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name), "The layout name should be at least one character long.");
            }

            if (name.Equals(ModelSpaceName, StringComparison.OrdinalIgnoreCase))
            {
                this.IsReserved = true;
                this.isPaperSpace = false;
                this.viewport = null;
                plotSettings.Flags = PlotFlags.Initializing | PlotFlags.UpdatePaper | PlotFlags.ModelType | PlotFlags.DrawViewportsFirst | PlotFlags.PrintLineweights | PlotFlags.PlotPlotStyles | PlotFlags.UseStandardScale;
            }
            else
            {
                this.IsReserved = false;
                this.isPaperSpace = true;
                this.viewport = new Viewport(1)
                {
                    ViewCenter = new Vector2(50.0, 100.0),
                    Status = ViewportStatusFlags.AdaptiveGridDisplay |
                             ViewportStatusFlags.DisplayGridBeyondDrawingLimits |
                             ViewportStatusFlags.CurrentlyAlwaysEnabled |
                             ViewportStatusFlags.UcsIconVisibility
                };
            }

            this.tabOrder = 0;
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
        /// This number is an ordinal indicating this layout's ordering in the tab control that is
        /// attached to the AutoCAD drawing frame window. Note that the "Model" tab always appears
        /// as the first tab regardless of its tab order (always zero).
        /// </remarks>
        public short TabOrder
        {
            get { return this.tabOrder; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("The tab order index must be greater than zero.", nameof(value));
                }
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
        /// Gets the viewport associated with this layout. This is the viewport with Id 1 that represents the paper space itself,
        /// it has no graphical representation, and does not show the model.
        /// </summary>
        /// <remarks>The ModelSpace layout does not require a viewport and it will always return null.</remarks>
        public Viewport Viewport
        {
            get { return this.viewport; }
            internal set { this.viewport = value; }
        }

        /// <summary>
        /// Gets the owner of the actual layout.
        /// </summary>
        public new Layouts Owner
        {
            get { return (Layouts) base.Owner; }
            internal set { base.Owner = value; }
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
        /// Checks if this instance has been referenced by other DxfObjects. 
        /// </summary>
        /// <returns>
        /// Returns true if this instance has been referenced by other DxfObjects, false otherwise.
        /// It will always return false if this instance does not belong to a document.
        /// </returns>
        /// <remarks>
        /// This method returns the same value as the HasReferences method that can be found in the TableObjects class.
        /// </remarks>
        public override bool HasReferences()
        {
            return this.Owner != null && this.Owner.HasReferences(this.Name);
        }

        /// <summary>
        /// Gets the list of DxfObjects referenced by this instance.
        /// </summary>
        /// <returns>
        /// A list of DxfObjectReference that contains the DxfObject referenced by this instance and the number of times it does.
        /// It will return null if this instance does not belong to a document.
        /// </returns>
        /// <remarks>
        /// This method returns the same list as the GetReferences method that can be found in the TableObjects class.
        /// </remarks>
        public override List<DxfObjectReference> GetReferences()
        {
            return this.Owner?.GetReferences(this.Name);
        }

        /// <summary>
        /// Creates a new Layout that is a copy of the current instance.
        /// </summary>
        /// <param name="newName">Layout name of the copy.</param>
        /// <returns>A new Layout that is a copy of this instance.</returns>
        /// <remarks>
        /// The Model Layout cannot be cloned.<br />
        /// When cloning a PaperSpace layout the contents of the layout will not be cloned.
        /// </remarks>
        public override TableObject Clone(string newName)
        {
            if (!this.IsPaperSpace)
            {
                throw new NotSupportedException("The Model layout cannot be cloned.");
            }

            if (string.Equals(newName, ModelSpaceName, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("The layout name \"Model\" is reserved for the ModelSpace.");
            }

            Layout copy = new Layout(newName, null, (PlotSettings) this.plot.Clone())
            {
                TabOrder = this.tabOrder,
                MinLimit = this.minLimit,
                MaxLimit = this.maxLimit,
                BasePoint = this.basePoint,
                MinExtents = this.minExtents,
                MaxExtents = this.maxExtents,
                Elevation = this.elevation,
                UcsOrigin = this.origin,
                UcsXAxis = this.xAxis,
                UcsYAxis = this.yAxis,
                Viewport = (Viewport) this.viewport.Clone()
            };

            foreach (XData data in this.XData.Values)
            {
                copy.XData.Add((XData)data.Clone());
            }

            return copy;
        }

        /// <summary>
        /// Creates a new Layout that is a copy of the current instance.
        /// </summary>
        /// <returns>A new Layout that is a copy of this instance.</returns>
        /// <remarks>
        /// The Model Layout cannot be cloned.<br />
        /// When cloning a PaperSpace layout the contents of the layout will not be cloned.
        /// </remarks>
        public override object Clone()
        {
            return this.Clone(this.Name);
        }

        /// <summary>
        /// Assigns a handle to the object based in a integer counter.
        /// </summary>
        /// <param name="entityNumber">Number to assign.</param>
        /// <returns>Next available entity number.</returns>
        /// <remarks>
        /// Some objects might consume more than one, is, for example, the case of polylines that will assign
        /// automatically a handle to its vertexes. The entity number will be converted to an hexadecimal number.
        /// </remarks>
        internal override long AssignHandle(long entityNumber)
        {
            entityNumber = this.Owner.AssignHandle(entityNumber);
            if (this.isPaperSpace)
            {
                entityNumber = this.viewport.AssignHandle(entityNumber);
            }
            return base.AssignHandle(entityNumber);
        }

        #endregion

        #region implements IComparable

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared.
        /// The return value has the following meanings: Value Meaning Less than zero This object is less than the other parameter.
        /// Zero This object is equal to other. Greater than zero This object is greater than other.
        /// </returns>
        public int CompareTo(Layout other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            return this.tabOrder.CompareTo(other.tabOrder);
        }

        #endregion
    }
}