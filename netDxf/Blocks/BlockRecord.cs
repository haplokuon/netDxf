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
using netDxf.Collections;
using netDxf.Objects;
using netDxf.Units;

namespace netDxf.Blocks
{
    /// <summary>
    /// Represent the record of a block in the tables section.
    /// </summary>
    public class BlockRecord :
        DxfObject
    {
        #region private fields

        private string name;
        private Layout layout;
        private static DrawingUnits defaultUnits = DrawingUnits.Unitless;
        private DrawingUnits units;
        private bool allowExploding;
        private bool scaleUniformly;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>BlockRecord</c> class.
        /// </summary>
        /// <param name="name">Block definition name.</param>
        internal BlockRecord(string name)
            : base(DxfObjectCode.BlockRecord)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }
            this.name = name;
            this.layout = null;
            this.units = DefaultUnits;
            this.allowExploding = true;
            this.scaleUniformly = false;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the name of the block record.
        /// </summary>
        /// <remarks>
        /// Block record names are case insensitive.<br />
        /// The block which name starts with "*" are for internal purpose only.
        /// </remarks>
        public string Name
        {
            get { return this.name; }
            internal set { this.name = value; }
        }

        /// <summary>
        /// Gets the associated Layout.
        /// </summary>
        public Layout Layout
        {
            get { return this.layout; }
            internal set { this.layout = value; }
        }

        /// <summary>
        /// Gets or sets the block insertion units.
        /// </summary>
        public DrawingUnits Units
        {
            get { return this.units; }
            set { this.units = value; }
        }

        /// <summary>
        /// Gets or sets the default block units.
        /// </summary>
        /// <remarks>These are the units that all new blocks will use as default.</remarks>
        public static DrawingUnits DefaultUnits
        {
            get { return defaultUnits; }
            set { defaultUnits = value; }
        }

        /// <summary>
        /// Gets or sets if the block can be exploded.
        /// </summary>
        /// <remarks>
        /// This property is only compatible with DXF version AutoCad2007 and upwards.
        /// </remarks>
        public bool AllowExploding
        {
            get { return this.allowExploding; }
            set { this.allowExploding = value; }
        }

        /// <summary>
        /// Gets or sets if the block must be scaled uniformly.
        /// </summary>
        /// <remarks>
        /// This property is only compatible with DXF version AutoCad2007 and upwards.
        /// </remarks>
        public bool ScaleUniformly
        {
            get { return this.scaleUniformly; }
            set { this.scaleUniformly = value; }
        }

        /// <summary>
        /// Gets the owner of the actual DXF object.
        /// </summary>
        public new BlockRecords Owner
        {
            get { return (BlockRecords) base.Owner; }
            internal set { base.Owner = value; }
        }

        /// <summary>
        /// Gets if the block record is for internal use only.
        /// </summary>
        /// <remarks>
        /// All blocks which name starts with "*" are for internal use and should not be modified.
        /// </remarks>
        public bool IsForInternalUseOnly
        {
            get { return this.name.StartsWith("*"); }
        }

        #endregion

        #region overrides

        /// <summary>
        /// Converts the value of this instance to its equivalent string representation.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion
    }
}