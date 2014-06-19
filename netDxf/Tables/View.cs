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

using netDxf.Collections;

namespace netDxf.Tables
{

    public enum Viewmode
    {
        /// <summary>
        /// Turned off.
        /// </summary>
        Off = 0,
        /// <summary>
        /// Perspective view active.
        /// </summary>
        Perspective = 1,
        /// <summary>
        /// Front clipping on.
        /// </summary>
        FrontClippingPlane = 2,
        /// <summary>
        /// Back clipping on.
        /// </summary>
        BackClippingPlane = 4,
        /// <summary>
        /// UCS Follow mode on.
        /// </summary>
        UCSFollow = 8,
        /// <summary>
        /// Front clip not at eye. If on, the front clip distance (FRONTZ) determines the front clipping plane.
        /// If off, FRONTZ is ignored, and the front clipping plane is set to pass through the camera point (vectors behind the camera are not displayed).
        /// This flag is ignored if the front-clipping bit (2) is off.
        /// </summary>
        FrontClipNotAtEye = 16
    }

    public class View :
        TableObject
    {
        #region private fields

        private Vector3 target = Vector3.Zero;
        private Vector3 camera = Vector3.UnitZ;
        private double height = 1.0;
        private double width = 1.0;
        private double rotation = 0.0;
        private Viewmode viewmode = Viewmode.Off;
        private double fov = 40.0;
        private double frontClippingPlane = 0.0;
        private double backClippingPlane = 0.0;

        #endregion

        #region constants

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>View</c> class.
        /// </summary>
        public View(string name)
            : base(name, DxfObjectCode.View, true)
        {
            this.reserved = false;
        }

        #endregion

        #region public properties

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

        public double Height
        {
            get { return this.height; }
            set { this.height = value; }
        }

        public double Width
        {
            get { return this.width; }
            set { this.width = value; }
        }

        public double Rotation
        {
            get { return this.rotation; }
            set { this.rotation = value; }
        }

        public Viewmode Viewmode
        {
            get { return this.viewmode; }
            set { this.viewmode = value; }
        }

        public double Fov
        {
            get { return this.fov; }
            set { this.fov = value; }
        }

        public double FrontClippingPlane
        {
            get { return this.frontClippingPlane; }
            set { this.frontClippingPlane = value; }
        }

        public double BackClippingPlane
        {
            get { return this.backClippingPlane; }
            set { this.backClippingPlane = value; }
        }

        /// <summary>
        /// Gets the owner of the actual dxf object.
        /// </summary>
        public new Views Owner
        {
            get { return (Views)this.owner; }
            internal set { this.owner = value; }
        }

        #endregion

    }
}