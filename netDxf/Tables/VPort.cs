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
using netDxf.Collections;

namespace netDxf.Tables
{
    public class VPort :
        TableObject
    {
        #region private fields

        private Vector2 lowerLeftCorner;
        private Vector2 upperRightCorner;
        private Vector2 snapBasePoint;
        private Vector2 snapSpacing;
        private Vector2 gridSpacing;
        private Vector3 target;
        private Vector3 camera;

        #endregion

        #region constants

        public static VPort Active
        {
            get { return new VPort("*Active", false); }
        }

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>Viewport</c> class.
        /// </summary>
        public VPort(string name)
            : this(name, true)
        {
        }

        internal VPort(string name, bool checkName)
            : base(name, DxfObjectCode.VPort, checkName)
        {
            this.reserved = name.Equals("*Active", StringComparison.OrdinalIgnoreCase);
            this.lowerLeftCorner = Vector2.Zero;
            this.upperRightCorner = new Vector2(1, 1);
            this.snapBasePoint = Vector2.Zero;
            this.snapSpacing = new Vector2(0.5, 0.5);
            this.gridSpacing = new Vector2(10, 10);
            this.target = Vector3.Zero;
            this.camera = Vector3.UnitZ;
        }

        #endregion

        #region public properties

        public Vector2 LowerLeftCorner
        {
            get { return this.lowerLeftCorner; }
            set { this.lowerLeftCorner = value; }
        }

        public Vector2 UpperRightCorner
        {
            get { return this.upperRightCorner; }
            set { this.upperRightCorner = value; }
        }

        public Vector2 SnapBasePoint
        {
            get { return this.snapBasePoint; }
            set { this.snapBasePoint = value; }
        }

        public Vector2 SnapSpacing
        {
            get { return this.snapSpacing; }
            set { this.snapSpacing = value; }
        }

        public Vector2 GridSpacing
        {
            get { return this.gridSpacing; }
            set { this.gridSpacing = value; }
        }

        public Vector3 Target
        {
            get { return this.target; }
            set { this.target = value; }
        }

        public Vector3 Camera
        {
            get { return this.camera; }
            set { this.camera = value; }
        }

        /// <summary>
        /// Gets the owner of the actual dxf object.
        /// </summary>
        public new VPorts Owner
        {
            get { return (VPorts) this.owner; }
            internal set { this.owner = value; }
        }

        #endregion

        #region overrides

        /// <summary>
        /// Creates a new VPort that is a copy of the current instance.
        /// </summary>
        /// <param name="newName">VPort name of the copy.</param>
        /// <returns>A new VPort that is a copy of this instance.</returns>
        public override TableObject Clone(string newName)
        {
            return new VPort(newName)
            {
                LowerLeftCorner = this.lowerLeftCorner,
                UpperRightCorner = this.upperRightCorner,
                SnapBasePoint = this.snapBasePoint,
                SnapSpacing = this.snapSpacing,
                GridSpacing = this.gridSpacing,
                Target = this.target,
                Camera = this.camera
            };
        }

        /// <summary>
        /// Creates a new VPort that is a copy of the current instance.
        /// </summary>
        /// <returns>A new VPort that is a copy of this instance.</returns>
        public override object Clone()
        {
            return Clone(this.name);
        }

        #endregion
    }
}