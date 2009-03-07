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

namespace netDxf.Tables
{
    internal class ViewPort :
        DxfObject,
        ITableObject
    {
        #region private fields

        private readonly string name;
        private Vector2f lowerLeftCorner = Vector2f.Zero;
        private Vector2f upperRightCorner = new Vector2f(1, 1);
        private Vector2f snapBasePoint = Vector2f.Zero;
        private Vector2f snapSpacing = new Vector2f(0.5f, 0.5f);
        private Vector2f gridSpacing = new Vector2f(10, 10);
        private Vector3f target = Vector3f.Zero;
        private Vector3f camera = Vector3f.UnitZ;

        #endregion

        #region constants

        internal static ViewPort Active
        {
            get { return new ViewPort("*Active"); }
        }

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>ViewPort</c> class.
        /// </summary>
        public ViewPort(string name)
            : base(DxfObjectCode.ViewPort)
        {
            this.name = name;
        }

        #endregion

        #region public properties

        public Vector2f LowerLeftCorner
        {
            get { return this.lowerLeftCorner; }
            set { this.lowerLeftCorner = value; }
        }

        public Vector2f UpperRightCorner
        {
            get { return this.upperRightCorner; }
            set { this.upperRightCorner = value; }
        }

        public Vector2f SnapBasePoint
        {
            get { return this.snapBasePoint; }
            set { this.snapBasePoint = value; }
        }

        public Vector2f SnapSpacing
        {
            get { return this.snapSpacing; }
            set { this.snapSpacing = value; }
        }

        public Vector2f GridSpacing
        {
            get { return this.gridSpacing; }
            set { this.gridSpacing = value; }
        }

        public Vector3f Target
        {
            get { return this.target; }
            set { this.target = value; }
        }

        public Vector3f Camera
        {
            get { return this.camera; }
            set { this.camera = value; }
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