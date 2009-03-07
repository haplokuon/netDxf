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
using netDxf.Tables;

namespace netDxf.Entities
{
    /// <summary>
    /// Defines which edges are hidden.
    /// </summary>
    [Flags]
    public enum EdgeFlags
    {
        /// <summary>
        /// All edges as visibles (default).
        /// </summary>
        Visibles = 0,
        /// <summary>
        /// First edge is invisible.
        /// </summary>
        First = 1,
        /// <summary>
        /// Second edge is invisible.
        /// </summary>
        Second = 2,
        /// <summary>
        /// Third edge is invisible.
        /// </summary>
        Third = 4,
        /// <summary>
        /// Fourth edge is invisible.
        /// </summary>
        Fourth = 8
    }

    /// <summary>
    /// Represents a 3DFace <see cref="netDxf.Entities.IEntityObject">entity</see>.
    /// </summary>
    public class Face3d :
        DxfObject,
        IEntityObject
    {
        #region private fields

        private const EntityType TYPE = EntityType.Face3D;
        private Vector3f firstVertex;
        private Vector3f secondVertex;
        private Vector3f thirdVertex;
        private Vector3f fourthVertex;
        private EdgeFlags edgeFlags;
        private Layer layer;
        private AciColor color;
        private LineType lineType;
        private Dictionary<ApplicationRegistry, XData> xData;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>Face3D</c> class.
        /// </summary>
        /// <param name="firstVertex">3d face <see cref="Vector3f">first vertex</see>.</param>
        /// <param name="secondVertex">3d face <see cref="Vector3f">second vertex</see>.</param>
        /// <param name="thirdVertex">3d face <see cref="Vector3f">third vertex</see>.</param>
        /// <param name="fourthVertex">3d face <see cref="Vector3f">fourth vertex</see>.</param>
        public Face3d(Vector3f firstVertex, Vector3f secondVertex, Vector3f thirdVertex, Vector3f fourthVertex)
            : base(DxfObjectCode.Face3D)
        {
            this.firstVertex = firstVertex;
            this.secondVertex = secondVertex;
            this.thirdVertex = thirdVertex;
            this.fourthVertex = fourthVertex;
            this.edgeFlags = EdgeFlags.Visibles;
            this.layer = Layer.Default;
            this.color = AciColor.ByLayer;
            this.lineType = LineType.ByLayer;
            this.xData = new Dictionary<ApplicationRegistry, XData>();
        }

        /// <summary>
        /// Initializes a new instance of the <c>Face3D</c> class.
        /// </summary>
        public Face3d()
            : base(DxfObjectCode.Face3D)
        {
            this.firstVertex = Vector3f.Zero;
            this.secondVertex = Vector3f.Zero;
            this.thirdVertex = Vector3f.Zero;
            this.fourthVertex = Vector3f.Zero;
            this.edgeFlags = EdgeFlags.Visibles;
            this.layer = Layer.Default;
            this.color = AciColor.ByLayer;
            this.lineType = LineType.ByLayer;
            this.xData = new Dictionary<ApplicationRegistry, XData>();
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the first 3d face <see cref="netDxf.Vector3f">vertex</see>.
        /// </summary>
        public Vector3f FirstVertex
        {
            get { return this.firstVertex; }
            set { this.firstVertex = value; }
        }

        /// <summary>
        /// Gets or sets the second 3d face <see cref="netDxf.Vector3f">vertex</see>.
        /// </summary>
        public Vector3f SecondVertex
        {
            get { return this.secondVertex; }
            set { this.secondVertex = value; }
        }

        /// <summary>
        /// Gets or sets the third 3d face <see cref="netDxf.Vector3f">vertex</see>.
        /// </summary>
        public Vector3f ThirdVertex
        {
            get { return this.thirdVertex; }
            set { this.thirdVertex = value; }
        }

        /// <summary>
        /// Gets or sets the fourth 3d face <see cref="netDxf.Vector3f">vertex</see>.
        /// </summary>
        public Vector3f FourthVertex
        {
            get { return this.fourthVertex; }
            set { this.fourthVertex = value; }
        }

        /// <summary>
        /// Gets or set the 3d face edge visibility.
        /// </summary>
        public EdgeFlags EdgeFlags
        {
            get { return this.edgeFlags; }
            set { this.edgeFlags = value; }
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

        #region overrides

        /// <summary>
        /// Converts the value of this instance to its equivalent string representation.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            return TYPE.ToString();
        }

        #endregion
    }
}