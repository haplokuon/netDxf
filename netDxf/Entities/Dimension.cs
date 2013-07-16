#region netDxf, Copyright(C) 2013 Daniel Carvajal, Licensed under LGPL.

//                        netDxf library
// Copyright (C) 2013 Daniel Carvajal (haplokuon@gmail.com)
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
using System.Globalization;
using netDxf.Blocks;
using netDxf.Tables;

namespace netDxf.Entities
{
    /// <summary>
    /// Represents the base class for a dimension <see cref="EntityObject">entity</see>.
    /// </summary>
    /// <reamarks>
    /// Once a dimension is added to the dxf document, its properties should not be modified or the changes will not be reflected in the saved dxf file.
    /// </reamarks>
    public abstract class Dimension:
        EntityObject
    {
        #region protected fields

        protected Vector3 definitionPoint;
        protected Vector3 midTextPoint;
        protected DimensionStyle style;
        protected DimensionType dimensionType;
        protected MTextAttachmentPoint attachmentPoint;
        protected MTextLineSpacingStyle lineSpacingStyle;
        protected Block block;
        protected double lineSpacing;

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
            set { this.style = value; }
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
                    throw new ArgumentOutOfRangeException("value", value, "The line spacing factor valid values range from 0.25 to 4.00");
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
        /// <returns>The formatted text.</returns>
        internal virtual string FormatDimensionText(double dimValue)
        {
            string format;
            string text;
            NumberFormatInfo numberFormat = new NumberFormatInfo { NumberDecimalSeparator = this.style.DIMDSEP };
            if (this is Angular3PointDimension || this is Angular2LineDimension)
            {
                format = "F" + this.style.DIMADEC;
                text = dimValue.ToString(format, numberFormat) + Symbols.Degree;
            }
            else
            {
                format = "F" + this.style.DIMDEC;
                text = dimValue.ToString(format, numberFormat);
            }
            text = this.style.DIMPOST.Replace("<>", text);
            return text;
        }
        
        #endregion

    }
}
