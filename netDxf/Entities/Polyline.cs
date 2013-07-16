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
using System.Collections.Generic;

namespace netDxf.Entities
{
    /// <summary>
    /// Represents a generic polyline <see cref="EntityObject">entity</see>.
    /// </summary>
    public class Polyline :
        EntityObject
    {
        #region private fields

        private readonly EndSequence endSequence;
        private List<PolylineVertex> vertexes;
        private PolylineTypeFlags flags;
        private readonly PolylineSmoothType smoothType;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>Polyline3d</c> class.
        /// </summary>
        /// <param name="vertexes">3d polyline <see cref="PolylineVertex">vertex</see> list.</param>
        /// <param name="isClosed">Sets if the polyline is closed.</param>
        public Polyline(List<PolylineVertex> vertexes, bool isClosed = false) 
            : base (EntityType.Polyline, DxfObjectCode.Polyline)
        {
            if (vertexes == null)
                throw new ArgumentNullException("vertexes");
            this.vertexes = vertexes;
            this.flags = isClosed ? PolylineTypeFlags.ClosedPolylineOrClosedPolygonMeshInM | PolylineTypeFlags.Polyline3D : PolylineTypeFlags.Polyline3D;
            this.smoothType = PolylineSmoothType.NoSmooth;
            this.endSequence = new EndSequence();
        }

        /// <summary>
        /// Initializes a new instance of the <c>Polyline3d</c> class.
        /// </summary>
        public Polyline()
            : this(new List<PolylineVertex>())
        {
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

        /// <summary>
        /// Gets or sets if the polyline is closed.
        /// </summary>
        public bool IsClosed
        {
            get { return (this.flags & PolylineTypeFlags.ClosedPolylineOrClosedPolygonMeshInM) == PolylineTypeFlags.ClosedPolylineOrClosedPolygonMeshInM; }
            set { this.flags = value ? PolylineTypeFlags.ClosedPolylineOrClosedPolygonMeshInM | PolylineTypeFlags.Polyline3D : PolylineTypeFlags.Polyline3D; }
        }

        /// <summary>
        /// Gets the curve smooth type.
        /// </summary>
        public PolylineSmoothType SmoothType
        {
            get { return smoothType; }
        }

        #endregion

        #region internal properties

        /// <summary>
        /// Gets the polyline type.
        /// </summary>
        internal PolylineTypeFlags Flags
        {
            get { return this.flags; }
            set { this.flags = value; }
        }

        /// <summary>
        /// Gets the end vertex sequence.
        /// </summary>
        internal EndSequence EndSequence
        {
            get { return this.endSequence; }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Decompose the actual polyline in a list of <see cref="Line">lines</see>.
        /// </summary>
        /// <returns>A list of <see cref="Line">lines</see> that made up the polyline.</returns>
        public List<EntityObject> Explode()
        {
            List<EntityObject> entities = new List<EntityObject>();
            int index = 0;
            foreach (PolylineVertex vertex in this.Vertexes)
            {
                Vector3 start;
                Vector3 end;

                if (index == this.Vertexes.Count - 1)
                {
                    if (!this.IsClosed) break;
                    start = vertex.Location;
                    end = this.vertexes[0].Location;
                }
                else
                {
                    start = vertex.Location;
                    end = this.vertexes[index + 1].Location;
                }

                entities.Add(new Line
                {
                    StartPoint = start,
                    EndPoint = end,
                    Color = this.Color,
                    IsVisible = this.IsVisible,
                    Layer = this.Layer,
                    LineType = this.LineType,
                    LineTypeScale = this.LineTypeScale,
                    Lineweight = this.Lineweight,
                    XData = this.XData
                });

                index++;
            }

            return entities;
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
        /// Creates a new Polyline that is a copy of the current instance.
        /// </summary>
        /// <returns>A new Polyline that is a copy of this instance.</returns>
        public override object Clone()
        {
            List<PolylineVertex> copyVertexes = new List<PolylineVertex>();
            foreach (PolylineVertex vertex in this.vertexes)
            {
                copyVertexes.Add((PolylineVertex) vertex.Clone());
            }
            return new Polyline
            {
                //EntityObject properties
                Color = this.color,
                Layer = this.layer,
                LineType = this.lineType,
                Lineweight = this.lineweight,
                LineTypeScale = this.lineTypeScale,
                Normal = this.normal,
                XData = this.xData,
                //Polyline properties
                Vertexes = copyVertexes,
                Flags = this.flags
            };
        }

        #endregion

    }
}