#region netDxf, Copyright(C) 2009 Daniel Carvajal, Licensed under LGPL.

//                        netDxf library
// Copyright (C) 2009 Daniel Carvajal (haplokuon@gmail.com)
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
        ITableObject
    {
        #region private fields

        private readonly string name;
        private AciColor color;
        private bool isVisible;
        private LineType lineType;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new layer.
        /// </summary>
        /// <param name="name">Layer name.</param>
        public Layer(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw (new ArgumentNullException("name"));
            this.name = name;
            this.color = AciColor.Default;
            this.lineType = LineType.ByLayer;
            this.isVisible = true;
        }

        #endregion

        #region constants

        /// <summary>
        /// Default Layer.
        /// </summary>
        public static Layer Default
        {
            get { return new Layer("0"); }
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the layer line type.
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
        /// Gets or sets the layer color.
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
        /// Gets or sets if the layer is visible.
        /// </summary>
        public bool IsVisible
        {
            get { return this.isVisible; }
            set { this.isVisible = value; }
        }

        #endregion

        #region ITableObject Members

        /// <summary>
        /// Gets the table name.
        /// </summary>
        public string Name
        {
            get { return this.name; }
        }

        #endregion

        #region overrides

        public override string ToString()
        {
            return this.name;
        }

        #endregion
    }
}