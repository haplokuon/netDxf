#region netDxf library, Copyright (C) 2009-2019 Daniel Carvajal (haplokuon@gmail.com)

//                        netDxf library
// Copyright (C) 2009-2019 Daniel Carvajal (haplokuon@gmail.com)
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
        private readonly ObservableCollection<PolylineVertex> vertexes;
        private PolylineTypeFlags flags;
        private PolylineSmoothType smoothType;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>Polyline3d</c> class.
        /// </summary>
        public Polyline()
            : this(new List<PolylineVertex>(), false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Polyline3d</c> class.
        /// </summary>
        /// <param name="vertexes">3d polyline <see cref="Vector3">vertex</see> list.</param>
        public Polyline(IEnumerable<Vector3> vertexes)
            : this(vertexes, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Polyline3d</c> class.
        /// </summary>
        /// <param name="vertexes">3d polyline <see cref="Vector3">vertex</see> list.</param>
        /// <param name="isClosed">Sets if the polyline is closed, by default it will create an open polyline.</param>
        public Polyline(IEnumerable<Vector3> vertexes, bool isClosed)
            : base(EntityType.Polyline, DxfObjectCode.Polyline)
        {
            if (vertexes == null)
            {
                throw new ArgumentNullException(nameof(vertexes));
            }

            this.vertexes = new ObservableCollection<PolylineVertex>();
            this.vertexes.BeforeAddItem += this.Vertexes_BeforeAddItem;
            this.vertexes.AddItem += this.Vertexes_AddItem;
            this.vertexes.BeforeRemoveItem += this.Vertexes_BeforeRemoveItem;
            this.vertexes.RemoveItem += this.Vertexes_RemoveItem;

            foreach (Vector3 vertex in vertexes)
            {
                this.vertexes.Add(new PolylineVertex(vertex));
            }

            this.flags = isClosed ? PolylineTypeFlags.ClosedPolylineOrClosedPolygonMeshInM | PolylineTypeFlags.Polyline3D : PolylineTypeFlags.Polyline3D;
            this.smoothType = PolylineSmoothType.NoSmooth;
            this.endSequence = new EndSequence(this);
        }


        /// <summary>
        /// Initializes a new instance of the <c>Polyline3d</c> class.
        /// </summary>
        /// <param name="vertexes">3d polyline <see cref="PolylineVertex">vertex</see> list.</param>
        public Polyline(IEnumerable<PolylineVertex> vertexes)
            : this(vertexes, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Polyline3d</c> class.
        /// </summary>
        /// <param name="vertexes">3d polyline <see cref="PolylineVertex">vertex</see> list.</param>
        /// <param name="isClosed">Sets if the polyline is closed  (default: false).</param>
        public Polyline(IEnumerable<PolylineVertex> vertexes, bool isClosed)
            : base(EntityType.Polyline, DxfObjectCode.Polyline)
        {
            if (vertexes == null)
            {
                throw new ArgumentNullException(nameof(vertexes));
            }

            this.vertexes = new ObservableCollection<PolylineVertex>();
            this.vertexes.BeforeAddItem += this.Vertexes_BeforeAddItem;
            this.vertexes.AddItem += this.Vertexes_AddItem;
            this.vertexes.BeforeRemoveItem += this.Vertexes_BeforeRemoveItem;
            this.vertexes.RemoveItem += this.Vertexes_RemoveItem;

            this.vertexes.AddRange(vertexes);

            this.flags = isClosed ? PolylineTypeFlags.ClosedPolylineOrClosedPolygonMeshInM | PolylineTypeFlags.Polyline3D : PolylineTypeFlags.Polyline3D;
            this.smoothType = PolylineSmoothType.NoSmooth;
            this.endSequence = new EndSequence(this);
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the polyline <see cref="PolylineVertex">vertex</see> list.
        /// </summary>
        public ObservableCollection<PolylineVertex> Vertexes
        {
            get { return this.vertexes; }
        }

        /// <summary>
        /// Gets or sets if the polyline is closed.
        /// </summary>
        public bool IsClosed
        {
            get { return this.flags.HasFlag(PolylineTypeFlags.ClosedPolylineOrClosedPolygonMeshInM); }
            set
            {
                if (value)
                {
                    this.flags |= PolylineTypeFlags.ClosedPolylineOrClosedPolygonMeshInM;
                }
                else
                {
                    this.flags &= ~PolylineTypeFlags.ClosedPolylineOrClosedPolygonMeshInM;
                }
            }
        }

        /// <summary>
        /// Enable or disable if the line type pattern is generated continuously around the vertexes of the polyline.
        /// </summary>
        public bool LinetypeGeneration
        {
            get { return this.flags.HasFlag(PolylineTypeFlags.ContinuousLinetypePattern); }
            set
            {
                if (value)
                {
                    this.flags |= PolylineTypeFlags.ContinuousLinetypePattern;
                }
                else
                {
                    this.flags &= ~PolylineTypeFlags.ContinuousLinetypePattern;
                }
            }
        }

        #endregion

        #region internal properties

        /// <summary>
        /// Gets or sets the curve smooth type.
        /// </summary>
        internal PolylineSmoothType SmoothType
        {
            get { return this.smoothType; }
            set { this.smoothType = value; }
        }

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
        /// Switch the polyline direction.
        /// </summary>
        public void Reverse()
        {
            if (this.vertexes.Count < 2)
            {
                return;
            }

            this.vertexes.Reverse();
        }

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
                    if (!this.IsClosed)
                    {
                        break;
                    }
                    start = vertex.Position;
                    end = this.vertexes[0].Position;
                }
                else
                {
                    start = vertex.Position;
                    end = this.vertexes[index + 1].Position;
                }

                entities.Add(new Line
                {
                    Layer = (Layer) this.Layer.Clone(),
                    Linetype = (Linetype) this.Linetype.Clone(),
                    Color = (AciColor) this.Color.Clone(),
                    Lineweight = this.Lineweight,
                    Transparency = (Transparency) this.Transparency.Clone(),
                    LinetypeScale = this.LinetypeScale,
                    Normal = this.Normal,
                    StartPoint = start,
                    EndPoint = end,
                });

                index++;
            }

            return entities;
        }

        #endregion

        #region overrides

        /// <summary>
        /// Moves, scales, and/or rotates the current entity given a 3x3 transformation matrix and a translation vector.
        /// </summary>
        /// <param name="transformation">Transformation matrix.</param>
        /// <param name="translation">Translation vector.</param>
        /// <remarks>Matrix3 adopts the convention of using column vectors to represent a transformation matrix.</remarks>
        public override void TransformBy(Matrix3 transformation, Vector3 translation)
        {
            foreach (PolylineVertex point in this.Vertexes)
            {
                point.Position = transformation * point.Position + translation;
            }

            Vector3 newNormal = transformation * this.Normal;
            if (Vector3.Equals(Vector3.Zero, newNormal)) newNormal = this.Normal;
            this.Normal = newNormal;
        }

        /// <summary>
        /// Assigns a handle to the object based in a integer counter.
        /// </summary>
        /// <param name="entityNumber">Number to assign.</param>
        /// <returns>Next available entity number.</returns>
        /// <remarks>
        /// Some objects might consume more than one, is, for example, the case of polylines that will assign
        /// automatically a handle to its vertexes. The entity number will be converted to an hexadecimal number.
        /// </remarks>
        internal override long AssignHandle(long entityNumber)
        {
            foreach (PolylineVertex v in this.vertexes)
            {
                entityNumber = v.AssignHandle(entityNumber);
            }
            entityNumber = this.endSequence.AssignHandle(entityNumber);

            return base.AssignHandle(entityNumber);
        }

        /// <summary>
        /// Creates a new Polyline that is a copy of the current instance.
        /// </summary>
        /// <returns>A new Polyline that is a copy of this instance.</returns>
        public override object Clone()
        {
            Polyline entity = new Polyline
            {
                //EntityObject properties
                Layer = (Layer) this.Layer.Clone(),
                Linetype = (Linetype) this.Linetype.Clone(),
                Color = (AciColor) this.Color.Clone(),
                Lineweight = this.Lineweight,
                Transparency = (Transparency) this.Transparency.Clone(),
                LinetypeScale = this.LinetypeScale,
                Normal = this.Normal,
                IsVisible = this.IsVisible,
                //Polyline properties
                Flags = this.flags
            };

            foreach (PolylineVertex vertex in this.vertexes)
            {
                entity.Vertexes.Add((PolylineVertex) vertex.Clone());
            }

            foreach (XData data in this.XData.Values)
            {
                entity.XData.Add((XData) data.Clone());
            }

            return entity;
        }

        #endregion

        #region Entities collection events

        private void Vertexes_BeforeAddItem(ObservableCollection<PolylineVertex> sender, ObservableCollectionEventArgs<PolylineVertex> e)
        {
            // null items and vertexes that belong to another polyline are not allowed.
            if (e.Item == null)
            {
                e.Cancel = true;
            }
            else if (e.Item.Owner != null)
            {
                e.Cancel = true;
            }
            else
            {
                e.Cancel = false;
            }
        }

        private void Vertexes_AddItem(ObservableCollection<PolylineVertex> sender, ObservableCollectionEventArgs<PolylineVertex> e)
        {
            // if the polyline already belongs to a document
            if (this.Owner != null)
            {
                // get the document
                DxfDocument doc = this.Owner.Record.Owner.Owner;
                doc.NumHandles = e.Item.AssignHandle(doc.NumHandles);
            }
            e.Item.Owner = this;
        }

        private void Vertexes_BeforeRemoveItem(ObservableCollection<PolylineVertex> sender, ObservableCollectionEventArgs<PolylineVertex> e)
        {
        }

        private void Vertexes_RemoveItem(ObservableCollection<PolylineVertex> sender, ObservableCollectionEventArgs<PolylineVertex> e)
        {
            e.Item.Handle = null;
            e.Item.Owner = null;
        }

        #endregion
    }
}