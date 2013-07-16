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

namespace netDxf.Tables
{
    /// <summary>
    /// Represents a layer.
    /// </summary>
    public class Layer :
        TableObject
    {
        #region private fields

        private AciColor color;
        private bool isVisible;
        private bool plot;
        private LineType lineType;
        private Lineweight lineweight;

        #endregion

        #region constants

        /// <summary>
        /// Gets the default Layer 0.
        /// </summary>
        public static Layer Default
        {
            get { return new Layer("0"); }
        }

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>Layer</c> class.
        /// </summary>
        /// <param name="name">Layer name.</param>
        public Layer(string name)
            : base(name, DxfObjectCode.Layer)
        {
            this.reserved = name.Equals("0", StringComparison.InvariantCultureIgnoreCase);

            this.color = AciColor.Default;
            this.lineType = LineType.Continuous;
            this.isVisible = true;
            this.plot = true;
            this.lineweight = Lineweight.Default;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the layer <see cref="LineType">line type</see>.
        /// </summary>
        public LineType LineType
        {
            get { return this.lineType; }
            set
            {
                if (value == null)
                    throw new NullReferenceException("value"); 
                this.lineType = value;
            }
        }

        /// <summary>
        /// Gets or sets the layer <see cref="AciColor">color</see>.
        /// </summary>
        public AciColor Color
        {
            get { return this.color; }
            set
            {
                if (value == null)
                    throw new NullReferenceException("value"); 
                this.color = value;
            }
        }

        /// <summary>
        /// Gets or sets the layer visibility.
        /// </summary>
        public bool IsVisible
        {
            get { return this.isVisible; }
            set { this.isVisible = value; }
        }

        /// <summary>
        /// Gets or sets if the plotting flag.
        /// </summary>
        /// <remarks>If set to false, do not plot this layer.</remarks>
        public bool Plot
        {
            get { return plot; }
            set { plot = value; }
        }

        /// <summary>
        /// Gets or sets the entity line weight, one unit is always 1/100 mm (default = Default).
        /// </summary>
        public Lineweight Lineweight
        {
            get { return this.lineweight; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                this.lineweight = value;
            }
        }

        #endregion

    }
}