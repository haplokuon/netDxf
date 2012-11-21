#region netDxf, Copyright(C) 2012 Daniel Carvajal, Licensed under LGPL.

//                        netDxf library
// Copyright (C) 2012 Daniel Carvajal (haplokuon@gmail.com)
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
using System.Collections.Generic;
using System.Text;
using netDxf.Blocks;
using netDxf.Tables;

namespace netDxf.Entities
{
    /// <summary>
    /// Represents the base class for a dimension <see cref="IEntityObject">entity</see>.
    /// </summary>
    public abstract class Dimension:
        DxfObject,
        IEntityObject
    {
        #region protected fields

        protected const EntityType TYPE = EntityType.Dimension;
        protected Vector3 definitionPoint;
        protected Vector3 midTextPoint;
        protected Vector3 normal;
        protected DimensionStyle style;
        protected DimensionType dimensionType;
        protected MTextAttachmentPoint attachmentPoint;
        protected MTextLineSpacingStyle lineSpacingStyle;
        protected Block block;
        protected double lineSpacing;
        protected AciColor color;
        protected Layer layer;
        protected LineType lineType;
        protected Dictionary<ApplicationRegistry, XData> xData;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>Dimension</c> class.
        /// </summary>
        protected Dimension(DimensionType type) : base(DxfObjectCode.Dimension)
        {
            this.definitionPoint = Vector3.Zero;
            this.midTextPoint = Vector3.Zero;
            this.normal = Vector3.UnitZ;
            this.dimensionType = type;
            this.attachmentPoint = MTextAttachmentPoint.MiddleCenter;
            this.lineSpacingStyle = MTextLineSpacingStyle.AtLeast;
            this.lineSpacing = 1.0;
            this.block = null;
            this.color = AciColor.ByLayer;
            this.layer = Layer.Default;
            this.lineType = LineType.ByLayer;
            this.style = DimensionStyle.Default;
        }
        #endregion

        #region public properties

        /// <summary>
        /// Definition point (in WCS).
        /// </summary>
        internal Vector3 DefinitionPoint
        {
            get { return definitionPoint; }
            set { definitionPoint = value; }
        }

        /// <summary>
        /// Middle point of dimension text (in OCS).
        /// </summary>
        internal Vector3 MidTextPoint
        {
            get { return midTextPoint; }
            set { midTextPoint = value; }
        }
        /// <summary>
        /// Gets or sets the dimension <see cref="Vector3">normal</see>.
        /// </summary>
        public Vector3 Normal
        {
            get { return normal; }
            set
            {
                if (Vector3.Zero == value)
                    throw new ArgumentNullException("value", "The normal can not be the zero vector");
                value.Normalize();
                this.normal = value;
            }
        }

        /// <summary>
        /// Gets or sets the style associated with the dimension.
        /// </summary>
        public DimensionStyle Style
        {
            get { return style; }
            set { style = value; }
        }

        /// <summary>
        /// Dimension type.
        /// </summary>
        public DimensionType DimensionType
        {
            get { return dimensionType; }
        }

        /// <summary>
        /// Actual measurement.
        /// </summary>
        public abstract double Value
        {
            get;
        }

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
            get { return lineSpacingStyle; }
            set { lineSpacingStyle = value; }
        }

        /// <summary>
        /// Gets or sets the dimension text line spacing factor.
        /// </summary>
        /// <remarks>
        /// Percentage of default line spacing to be applied. Valid values range from 0.25 to 4.00, the default value 1.0.
        /// </remarks>
        public double LineSpacingFactor
        {
            get { return lineSpacing; }
            set
            {
                if (value < 0.25 || value > 4.0)
                    throw new ArgumentOutOfRangeException("value", value, "Valid values range from 0.25 to 4.00");
                lineSpacing = value;
            }
        }

        /// <summary>
        /// Gets the block that contains the entities that make up the dimension picture.
        /// </summary>
        public Block Block
        {
            get { return block; }
            internal set { block = value; }
        }

        #endregion

        #region IEntityObject Members

        /// <summary>
        /// Gets the entity <see cref="netDxf.Entities.EntityType">type</see>.
        /// </summary>
        public EntityType Type
        {
            get { return TYPE; }
        }

        /// <summary>
        /// Gets or sets the entity <see cref="netDxf.AciColor">color</see>.
        /// </summary>
        public AciColor Color
        {
            get { return this.color; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                this.color = value;
            }
        }

        /// <summary>
        /// Gets or sets the entity <see cref="netDxf.Tables.Layer">layer</see>.
        /// </summary>
        public Layer Layer
        {
            get { return this.layer; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                this.layer = value;
            }
        }

        /// <summary>
        /// Gets or sets the entity <see cref="netDxf.Tables.LineType">line type</see>.
        /// </summary>
        public LineType LineType
        {
            get { return this.lineType; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                this.lineType = value;
            }
        }

        /// <summary>
        /// Gets or sets the entity <see cref="netDxf.XData">extende data</see>.
        /// </summary>
        public Dictionary<ApplicationRegistry, XData> XData
        {
            get { return this.xData; }
            set { this.xData = value; }
        }

        #endregion

        #region internal methods

        /// <summary>
        /// Gets the the block that contains the entities that make up the dimension picture.
        /// </summary>
        /// <param name="name">Name to be asigned to the generated block.</param>
        /// <returns>The block that represents the actual dimension.</returns>
        internal abstract Block BuildBlock(string name);

        /// <summary>
        /// Format the value for the dimension text.
        /// </summary>
        /// <param name="dimValue">Dimension value.</param>
        /// <returns>The formated text.</returns>
        internal string FormatDimensionText(double dimValue)
        {
            return this.style.DIMPOST.Replace("<>", Math.Round(dimValue, this.style.DIMDEC).ToString());
        }
        #endregion
    }
}
