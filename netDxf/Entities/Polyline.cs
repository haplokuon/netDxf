#region netDxf, Copyright(C) 2012 Daniel Carvajal, Licensed under LGPL.

//                        netDxf library
// Copyright (C) 2012 Daniel Carvajal (haplokuon@gmail.com)
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
    /// Defines the polyline type.
    /// </summary>
    /// <remarks>Bit flag.</remarks>
    [Flags]
    public enum PolylineTypeFlags
    {
        /// <summary>
        /// Default, open polyline.
        /// </summary>
        OpenPolyline = 0,
        /// <summary>
        /// This is a closed polyline (or a polygon mesh closed in the M direction).
        /// </summary>
        ClosedPolylineOrClosedPolygonMeshInM = 1,
        /// <summary>
        /// Curve-fit vertices have been added.
        /// </summary>
        CurveFit = 2,
        /// <summary>
        /// Spline-fit vertices have been added.
        /// </summary>
        SplineFit = 4,
        /// <summary>
        /// This is a 3D polyline.
        /// </summary>
        Polyline3D = 8,
        /// <summary>
        /// This is a 3D polygon mesh.
        /// </summary>
        PolygonMesh = 16,
        /// <summary>
        /// The polygon mesh is closed in the N direction.
        /// </summary>
        ClosedPolygonMeshInN = 32,
        /// <summary>
        /// The polyline is a polyface mesh.
        /// </summary>
        PolyfaceMesh = 64,
        /// <summary>
        /// The linetype pattern is generated continuously around the vertices of this polyline.
        /// </summary>
        ContinuousLineTypePatter = 128
    }

    /// <summary>
    /// Defines the curves and smooth surface type.
    /// </summary>
    public enum SmoothType
    {
        /// <summary>
        /// No smooth surface fitted
        /// </summary>
        NoSmooth = 0,
        /// <summary>
        /// Quadratic B-spline surface
        /// </summary>
        Quadratic = 5,
        /// <summary>
        /// Cubic B-spline surface
        /// </summary>
        Cubic = 6,
        /// <summary>
        /// Bezier surface
        /// </summary>
        Bezier = 8
    }

    /// <summary>
    /// Represents a generic polyline <see cref="IEntityObject">entity</see>.
    /// </summary>
    public class Polyline :
        DxfObject,
        IEntityObject
    {
        #region private fields

        private readonly EndSequence endSequence;
        protected const EntityType TYPE = EntityType.Polyline3d;
        protected List<PolylineVertex> vertexes;
        protected PolylineTypeFlags flags;
        protected Layer layer;
        protected AciColor color;
        protected LineType lineType;
        protected Dictionary<ApplicationRegistry, XData> xData;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>Polyline3d</c> class.
        /// </summary>
        /// <param name="vertexes">3d polyline <see cref="PolylineVertex">vertex</see> list.</param>
        /// <param name="isClosed">Sets if the polyline is closed</param>
        public Polyline(List<PolylineVertex> vertexes, bool isClosed = false) 
            : base (DxfObjectCode.Polyline)
        {
            this.vertexes = vertexes;
            this.layer = Layer.Default;
            this.color = AciColor.ByLayer;
            this.lineType = LineType.ByLayer;
            this.flags = isClosed ? PolylineTypeFlags.ClosedPolylineOrClosedPolygonMeshInM | PolylineTypeFlags.Polyline3D : PolylineTypeFlags.Polyline3D;
            this.endSequence = new EndSequence();
        }

        /// <summary>
        /// Initializes a new instance of the <c>Polyline3d</c> class.
        /// </summary>
        public Polyline()
            : base(DxfObjectCode.Polyline)
        {
            this.flags = PolylineTypeFlags.Polyline3D;
            this.vertexes = new List<PolylineVertex>();
            this.layer = Layer.Default;
            this.color = AciColor.ByLayer;
            this.lineType = LineType.ByLayer;
            this.endSequence = new EndSequence();
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the polyline <see cref="PolylineVertex">vertex</see> list.
        /// </summary>
        public List<PolylineVertex> Vertexes
        {
            get { return this.vertexes; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value"); 
                this.vertexes = value;
            }
        }

        internal EndSequence EndSequence
        {
            get { return this.endSequence; }
        }

        /// <summary>
        /// Gets the polyline type.
        /// </summary>
        public PolylineTypeFlags Flags
        {
            get { return this.flags; }
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
        /// Asigns a handle to the object based in a integer counter.
        /// </summary>
        /// <param name="entityNumber">Number to asign.</param>
        /// <returns>Next avaliable entity number.</returns>
        /// <remarks>
        /// Some objects might consume more than one, is, for example, the case of polylines that will asign
        /// automatically a handle to its vertexes. The entity number will be converted to an hexadecimal number.
        /// </remarks>
        internal override int AsignHandle(int entityNumber)
        {
            foreach( PolylineVertex v in this.vertexes )
            {
                entityNumber = v.AsignHandle(entityNumber);
            }
            entityNumber = this.endSequence.AsignHandle(entityNumber);

            return base.AsignHandle(entityNumber);
        }

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