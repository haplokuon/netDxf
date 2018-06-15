#region netDxf library, Copyright (C) 2009-2016 Daniel Carvajal (haplokuon@gmail.com)

//                        netDxf library
// Copyright (C) 2009-2016 Daniel Carvajal (haplokuon@gmail.com)
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
using netDxf.Tables;

namespace netDxf.Entities
{
    /// <summary>
    /// Represents the base class for a dimension <see cref="EntityObject">entity</see>.
    /// </summary>
    /// <reamarks>
    /// Once a dimension is added to the dxf document, its properties should not be modified or the changes will not be reflected in the saved dxf file.
    /// </reamarks>
    public abstract class Dimension :
        EntityObject
    {
        #region delegates and events

        public delegate void DimensionStyleChangedEventHandler(Dimension sender, TableObjectChangedEventArgs<DimensionStyle> e);

        public event DimensionStyleChangedEventHandler DimensionStyleChanged;

        protected virtual DimensionStyle OnDimensionStyleChangedEvent(DimensionStyle oldStyle, DimensionStyle newStyle)
        {
            DimensionStyleChangedEventHandler ae = this.DimensionStyleChanged;
            if (ae != null)
            {
                TableObjectChangedEventArgs<DimensionStyle> eventArgs = new TableObjectChangedEventArgs<DimensionStyle>(oldStyle, newStyle);
                ae(this, eventArgs);
                return eventArgs.NewValue;
            }
            return newStyle;
        }

        public delegate void DimensionBlockChangedEventHandler(Dimension sender, TableObjectChangedEventArgs<Block> e);

        public event DimensionBlockChangedEventHandler DimensionBlockChanged;

        protected virtual Block OnDimensionBlockChangedEvent(Block oldBlock, Block newBlock)
        {
            DimensionBlockChangedEventHandler ae = this.DimensionBlockChanged;
            if (ae != null)
            {
                TableObjectChangedEventArgs<Block> eventArgs = new TableObjectChangedEventArgs<Block>(oldBlock, newBlock);
                ae(this, eventArgs);
                return eventArgs.NewValue;
            }
            return newBlock;
        }

        #endregion

        #region delegates and events for style overrides

        public delegate void DimensionStyleOverrideAddedEventHandler(Dimension sender, DimensionStyleOverrideChangeEventArgs e);

        public event DimensionStyleOverrideAddedEventHandler DimensionStyleOverrideAdded;

        protected virtual void OnDimensionStyleOverrideAddedEvent(DimensionStyleOverride item)
        {
            DimensionStyleOverrideAddedEventHandler ae = this.DimensionStyleOverrideAdded;
            if (ae != null)
                ae(this, new DimensionStyleOverrideChangeEventArgs(item));
        }

        public delegate void DimensionStyleOverrideRemovedEventHandler(Dimension sender, DimensionStyleOverrideChangeEventArgs e);

        public event DimensionStyleOverrideRemovedEventHandler DimensionStyleOverrideRemoved;

        protected virtual void OnDimensionStyleOverrideRemovedEvent(DimensionStyleOverride item)
        {
            DimensionStyleOverrideRemovedEventHandler ae = this.DimensionStyleOverrideRemoved;
            if (ae != null)
                ae(this, new DimensionStyleOverrideChangeEventArgs(item));
        }

        #endregion

        #region protected fields

        private Vector3 definitionPoint;
        private Vector3 midTextPoint;
        private DimensionStyle style;
        private readonly DimensionType dimensionType;
        private MTextAttachmentPoint attachmentPoint;
        private MTextLineSpacingStyle lineSpacingStyle;
        private Block block;
        private string userText;
        private double lineSpacing;
        private double elevation;
        private readonly DimensionStyleOverrideDictionary styleOverrides;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>Dimension</c> class.
        /// </summary>
        protected Dimension(DimensionType type)
            : base(EntityType.Dimension, DxfObjectCode.Dimension)
        {
            this.definitionPoint = Vector3.Zero;
            this.midTextPoint = Vector3.Zero;
            this.dimensionType = type;
            this.attachmentPoint = MTextAttachmentPoint.MiddleCenter;
            this.lineSpacingStyle = MTextLineSpacingStyle.AtLeast;
            this.lineSpacing = 1.0;
            this.block = null;
            this.style = DimensionStyle.Default;
            this.userText = null;
            this.elevation = 0.0;
            this.styleOverrides = new DimensionStyleOverrideDictionary();
            this.styleOverrides.BeforeAddItem += this.StyleOverrides_BeforeAddItem;
            this.styleOverrides.AddItem += this.StyleOverrides_AddItem;
            this.styleOverrides.BeforeRemoveItem += this.StyleOverrides_BeforeRemoveItem;
            this.styleOverrides.RemoveItem += this.StyleOverrides_RemoveItem;
        }

        #endregion

        #region internal properties

        /// <summary>
        /// Definition point (in WCS).
        /// </summary>
        internal Vector3 DefinitionPoint
        {
            get { return this.definitionPoint; }
            set { this.definitionPoint = value; }
        }

        /// <summary>
        /// Middle point of dimension text (in OCS).
        /// </summary>
        internal Vector3 MidTextPoint
        {
            get { return this.midTextPoint; }
            set { this.midTextPoint = value; }
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the style associated with the dimension.
        /// </summary>
        public DimensionStyle Style
        {
            get { return this.style; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                this.style = this.OnDimensionStyleChangedEvent(this.style, value);
            }
        }

        /// <summary>
        /// Gets the dimension style overrides list.
        /// </summary>
        /// <remarks>Any dimension style value stored in this list will override its corresponding value in the assigned style to the dimension.</remarks>
        public DimensionStyleOverrideDictionary StyleOverrides
        {
            get { return this.styleOverrides; }
        }

        /// <summary>
        /// Gets the dimension type.
        /// </summary>
        public DimensionType DimensionType
        {
            get { return this.dimensionType; }
        }

        /// <summary>
        /// Gets the actual measurement.
        /// </summary>
        public abstract double Measurement { get; }

        /// <summary>
        /// Gets or sets the dimension text attachment point.
        /// </summary>
        public MTextAttachmentPoint AttachmentPoint
        {
            get { return this.attachmentPoint; }
            set { this.attachmentPoint = value; }
        }

        /// <summary>
        /// Get or sets the dimension text <see cref="MTextLineSpacingStyle">line spacing style</see>.
        /// </summary>
        public MTextLineSpacingStyle LineSpacingStyle
        {
            get { return this.lineSpacingStyle; }
            set { this.lineSpacingStyle = value; }
        }

        /// <summary>
        /// Gets or sets the dimension text line spacing factor.
        /// </summary>
        /// <remarks>
        /// Percentage of default line spacing to be applied. Valid values range from 0.25 to 4.00, the default value 1.0.
        /// </remarks>
        public double LineSpacingFactor
        {
            get { return this.lineSpacing; }
            set
            {
                if (value < 0.25 || value > 4.0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The line spacing factor valid values range from 0.25 to 4.00");
                this.lineSpacing = value;
            }
        }

        /// <summary>
        /// Gets the block that contains the entities that make up the dimension picture.
        /// </summary>
        /// <remarks>
        /// This value will be null until the dimension entity is added to the document.
        /// </remarks>
        public Block Block
        {
            get { return this.block; }
            internal set { this.block = value; }
        }

        /// <summary>
        /// Gets or sets the dimension text explicitly.
        /// </summary>
        /// <remarks>
        /// Dimension text explicitly entered by the user. Optional; default is the measurement.
        /// If null or "&lt;&gt;", the dimension measurement is drawn as the text,
        /// if " " (one blank space), the text is suppressed. Anything else is drawn as the text.
        /// </remarks>
        public string UserText
        {
            get { return this.userText; }
            set { this.userText = value; }
        }

        /// <summary>
        /// Gets or sets the dimension elevation, its position along its normal.
        /// </summary>
        public double Elevation
        {
            get { return this.elevation; }
            set { this.elevation = value; }
        }

        #endregion

        #region internal methods

        /// <summary>
        /// Gets the block that contains the entities that make up the dimension picture.
        /// </summary>
        /// <param name="name">Name to be assigned to the generated block.</param>
        /// <returns> The block that represents the actual dimension.</returns>
        internal abstract Block BuildBlock(string name);

        #endregion

        #region public methods

        /// <summary>
        /// Rebuilds the block definition of the actual dimension.
        /// </summary>
        /// <remarks>
        /// This method needs to be manually called to reflect any change made to the dimension style.<br />
        /// The dimension must belong to a document.
        /// </remarks>
        public void Update()
        {
            //DxfDocument doc = this.Owner.Owner.Owner.Owner as DxfDocument;
            //if (doc == null)
            //    throw new NullReferenceException("The dimension entity must belong to a document to be able to update or create the block that represent the actual object.");

            //string name;
            //if (this.block == null)
            //    name = "*D" + ++doc.DimensionBlocksIndex;
            //else
            //    name = this.block.Name;

            //Block newBlock = this.BuildBlock(name);
            //this.block = this.OnDimensionBlockChangedEvent(this.block, newBlock);

            if (this.block == null)
                return;
            Block newBlock = this.BuildBlock(this.block.Name);
            this.block = this.OnDimensionBlockChangedEvent(this.block, newBlock);

        }

        /// <summary>
        /// Clears the reference to the block associated with the dimension.
        /// </summary>
        /// <remarks>
        /// This method only deletes the reference not the actual block from the document.
        /// </remarks>
        //public void ClearBlockReference()
        //{
        //    this.block = this.OnDimensionBlockChangedEvent(this.block, null);
        //    //this.block = null;
        //}

        #endregion

        #region Dimension style overrides events

        private void StyleOverrides_BeforeAddItem(DimensionStyleOverrideDictionary sender, DimensionStyleOverrideDictionaryEventArgs e)
        {
            DimensionStyleOverride old;
            if (sender.TryGetValue(e.Item.Type, out old))
                if (ReferenceEquals(old.Value, e.Item.Value))
                    e.Cancel = true;
        }

        private void StyleOverrides_AddItem(DimensionStyleOverrideDictionary sender, DimensionStyleOverrideDictionaryEventArgs e)
        {
            this.OnDimensionStyleOverrideAddedEvent(e.Item);
        }

        private void StyleOverrides_BeforeRemoveItem(DimensionStyleOverrideDictionary sender, DimensionStyleOverrideDictionaryEventArgs e)
        {
        }

        private void StyleOverrides_RemoveItem(DimensionStyleOverrideDictionary sender, DimensionStyleOverrideDictionaryEventArgs e)
        {
            this.OnDimensionStyleOverrideRemovedEvent(e.Item);
        }

        #endregion
    }
}