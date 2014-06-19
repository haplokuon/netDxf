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
using System.Collections.Generic;
using netDxf.Collections;
using netDxf.Tables;

namespace netDxf.Objects
{
    /// <summary>
    /// Represents as MLine style.
    /// </summary>
    public class MLineStyle :
        TableObject
    {

        #region private fields

        private MLineStyleFlags flags;
        private string description;
        private AciColor fillColor;
        private double startAngle;
        private double endAngle;
        private List<MLineStyleElement> elements;

        private static readonly MLineStyle standard;
        #endregion

        #region constants

        /// <summary>
        /// Gets the default MLine style.
        /// </summary>
        public static MLineStyle Default
        {
            get { return standard; }
        }

        #endregion

        #region constructors

        static MLineStyle()
        {
            standard = new MLineStyle("Standard");
        }

        /// <summary>
        /// Initializes a new instance of the <c>MLineStyle</c> class.
        /// </summary>
        /// <param name="name">MLine style name.</param>
        /// <param name="description">MLine style description (optional).</param>
        /// <remarks>By default the multiline style has to elements with offsets 0.5 y -0.5.</remarks>
        public MLineStyle(string name, string description = null)
            : base(name, DxfObjectCode.MLineStyle, true)
        {
            this.reserved = name.Equals("Standard", StringComparison.OrdinalIgnoreCase);
            this.flags = MLineStyleFlags.None;

            this.description = string.IsNullOrEmpty(description) ? string.Empty : description;
            this.fillColor = AciColor.ByLayer;
            this.startAngle = 90.0;
            this.endAngle = 90.0;
            this.elements = new List<MLineStyleElement>
                                {
                                    new MLineStyleElement(0.5),
                                    new MLineStyleElement(-0.5)
                                };
        }

        /// <summary>
        /// Initializes a new instance of the <c>MLineStyle</c> class.
        /// </summary>
        /// <param name="name">MLine style name.</param>
        /// <param name="elements">Elements of the multiline.</param>
        /// <param name="description">MLine style description (optional).</param>
        public MLineStyle(string name, List<MLineStyleElement> elements, string description = null)
            : base(name, DxfObjectCode.MLineStyle, true)
        {
            
            if (elements == null)
                throw new ArgumentNullException("elements");
            if (elements.Count < 1)
                throw new ArgumentOutOfRangeException("elements", elements.Count, "The elements list must have at least one element.");
            // the elements list must be ordered
            elements.Sort();

            this.elements = elements;
            this.flags = MLineStyleFlags.None;
            this.description = string.IsNullOrEmpty(description) ? string.Empty : description;
            this.fillColor = AciColor.ByLayer;
            this.startAngle = 90.0;
            this.endAngle = 90.0;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the MLine style flags.
        /// </summary>
        public MLineStyleFlags Flags
        {
            get { return this.flags; }
            set { this.flags = value; }
        }

        /// <summary>
        /// Gets or sets the line type description (optional).
        /// </summary>
        public string Description
        {
            get { return this.description; }
            set { this.description = string.IsNullOrEmpty(value) ? string.Empty : value; }
        }

        /// <summary>
        /// Gets or sets the MLine fill color.
        /// </summary>
        /// <remarks>
        /// AutoCad2000 dxf version does not support true colors for MLineStyle fill color.
        /// </remarks>
        public AciColor FillColor
        {
            get { return this.fillColor; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                this.fillColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the MLine start angle in degrees.
        /// </summary>
        public double StartAngle
        {
            get { return this.startAngle; }
            set
            {
                if (value < 10.0 || value > 170.0)
                    throw new ArgumentOutOfRangeException("value", value, "The MLine style start angle valid values range from 10 to 170 degrees.");
                this.startAngle = value;
            }
        }

        /// <summary>
        /// Gets or sets the MLine end angle in degrees.
        /// </summary>
        public double EndAngle
        {
            get { return this.endAngle; }
            set
            {
                if (value < 10.0 || value > 170.0)
                    throw new ArgumentOutOfRangeException("value", value, "The MLine style start angle valid values range from 10 to 170 degrees.");
                this.endAngle = value;
            }
        }

        /// <summary>
        /// Gets or sets the list of elements that make up the multiline.
        /// </summary>
        /// <remarks>
        /// The elements list must be ordered, this will be done automatically,
        /// but if new elements are added individually to the list it will have to be sorted manually calling the Sort() method.
        /// </remarks>
        public List<MLineStyleElement> Elements
        {
            get { return this.elements; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                if (value.Count < 1)
                    throw new ArgumentOutOfRangeException("value", value.Count, "The vertex list must have at least one element.");
                value.Sort();
                this.elements = value;
            }
        }

        /// <summary>
        /// Gets the owner of the actual dxf object.
        /// </summary>
        public new MLineStyles Owner
        {
            get { return (MLineStyles)this.owner; }
            internal set { this.owner = value; }
        }

        #endregion

    }
}