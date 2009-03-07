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
using System.Collections.Generic;

namespace netDxf.Tables
{
    /// <summary>
    /// Represents a line type.
    /// </summary>
    public class LineType :
        DxfObject,
        ITableObject
    {
        #region private fields

        private readonly string name;
        private string description;
        private List<float> segments;

        #endregion

        #region constants

        /// <summary>
        /// Gets the ByLayer line type.
        /// </summary>
        public static LineType ByLayer
        {
            get { return new LineType("ByLayer"); }
        }

        /// <summary>
        /// Gets the ByBlock line type.
        /// </summary>
        public static LineType ByBlock
        {
            get { return new LineType("ByBlock"); }
        }

        /// <summary>
        /// Gets a predefined continuous line.
        /// </summary>
        public static LineType Continuous
        {
            get
            {
                var result = new LineType("Continuous")
                                 {
                                     Description = "Solid line"
                                 };
                return result;
            }
        }

        /// <summary>
        /// Gets a predefined center line.
        /// </summary>
        public static LineType Center
        {
            get
            {
                var result = new LineType("Center")
                                 {
                                     Description = "Center, ____ _ ____ _ ____ _ ____ _ ____ _ ____"
                                 };
                result.Segments.AddRange(new[] {1.25f, -0.25f, 0.25f, -0.25f});
                return result;
            }
        }

        /// <summary>
        /// Gets a predefined dash dot line.
        /// </summary>
        public static LineType DashDot
        {
            get
            {
                var result = new LineType("Dashdot")
                                 {
                                     Description = "Dash dot, __ . __ . __ . __ . __ . __ . __ . __"
                                 };
                result.Segments.AddRange(new[] {0.5f, -0.25f, 0.0f, -0.25f});
                return result;
            }
        }

        /// <summary>
        /// Gets a predefined dashed line
        /// </summary>
        public static LineType Dashed
        {
            get
            {
                var result = new LineType("Dashed")
                                 {
                                     Description = "Dashed, __ __ __ __ __ __ __ __ __ __ __ __ __ _"
                                 };
                result.Segments.AddRange(new[] {0.5f, -0.25f});
                return result;
            }
        }

        /// <summary>
        /// Gets a predefined dot line
        /// </summary>
        public static LineType Dot
        {
            get
            {
                var result = new LineType("Dot")
                                 {
                                     Description = "Dot, . . . . . . . . . . . . . . . . . . . . . . . ."
                                 };
                result.Segments.AddRange(new[] {0.0f, - 0.25f});
                return result;
            }
        }

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>LineType</c> class.
        /// </summary>
        /// <param name="name">Line type name.</param>
        public LineType(string name)
            : base(DxfObjectCode.LineType)
        {
            if (string.IsNullOrEmpty(name))
                throw (new ArgumentNullException("name"));
            this.name = name;
            this.description = string.Empty;
            this.segments = new List<float>();
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the line type description.
        /// </summary>
        public string Description
        {
            get { return this.description; }
            set { this.description = value; }
        }

        /// <summary>
        /// Gets or stes the list of line type segments.
        /// </summary>
        /// <remarks>
        /// Positive values means solid segments and negative values means spaces (one entry per element)
        /// </remarks>
        public List<float> Segments
        {
            get { return this.segments; }
            set
            {
                if (value == null)
                    throw new NullReferenceException("value");
                this.segments = value;
            }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Gets the total length of the line type.
        /// </summary>
        public float Legth()
        {
            float result = 0;
            foreach (float s in this.segments)
            {
                result += Math.Abs(s);
            }
            return result;
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

        /// <summary>
        /// Converts the value of this instance to its equivalent string representation.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            return this.name;
        }

        #endregion
    }
}