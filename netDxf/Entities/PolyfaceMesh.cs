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
    /// Represents a polyface mesh <see cref="EntityObject">entity</see>.
    /// </summary>
    public class PolyfaceMesh :
        EntityObject
    {
        #region private fields

        private readonly EndSequence endSequence;
        private List<PolyfaceMeshFace> faces;
        private List<PolyfaceMeshVertex> vertexes;
        private readonly PolylineTypeFlags flags;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>PolyfaceMesh</c> class.
        /// </summary>
        public PolyfaceMesh()
            : this(new List<PolyfaceMeshVertex>(), new List<PolyfaceMeshFace>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>PolyfaceMesh</c> class.
        /// </summary>
        /// <param name="vertexes">Polyface mesh <see cref="PolyfaceMeshVertex">vertex</see> list.</param>
        /// <param name="faces">Polyface mesh <see cref="PolyfaceMeshFace">faces</see> list.</param>
        public PolyfaceMesh(List<PolyfaceMeshVertex> vertexes, List<PolyfaceMeshFace> faces)
            : base(EntityType.PolyfaceMesh, DxfObjectCode.Polyline)
        {
            this.flags = PolylineTypeFlags.PolyfaceMesh;
            this.vertexes = vertexes;
            this.faces = faces;
            this.endSequence = new EndSequence();
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the polyface mesh <see cref="PolyfaceMeshVertex">vertexes</see>.
        /// </summary>
        public List<PolyfaceMeshVertex> Vertexes
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
        /// Gets or sets the polyface mesh <see cref="PolyfaceMeshFace">faces</see>.
        /// </summary>
        public List<PolyfaceMeshFace> Faces
        {
            get { return this.faces; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                this.faces = value;
            }
        }

        #endregion

        #region internal properties

        /// <summary>
        /// Gets the polyface mesh flag type.
        /// </summary>
        internal PolylineTypeFlags Flags
        {
            get { return this.flags; }
        }

        /// <summary>
        /// Gets the end vertex sequence.
        /// </summary>
        internal EndSequence EndSequence
        {
            get { return this.endSequence; }
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
        internal override long AsignHandle(long entityNumber)
        {
            entityNumber = this.endSequence.AsignHandle(entityNumber);
            foreach (PolyfaceMeshVertex v in this.vertexes)
            {
                entityNumber = v.AsignHandle(entityNumber);
            }
            foreach(PolyfaceMeshFace f in this.faces)
            {
                entityNumber = f.AsignHandle(entityNumber);
            }
            return base.AsignHandle(entityNumber);
        }

        #endregion

        #region public methods

        /// <summary>
        /// Decompose the actual polyface mesh faces in <see cref="Point">points</see> (one vertex polyface mesh face),
        /// <see cref="Line">lines</see> (two vertexes polyface mesh face) and <see cref="Face3d">3d faces</see> (three or four vertexes polyface mesh face).
        /// </summary>
        /// <returns>A list of <see cref="Face3d">3d faces</see> that made up the polyface mesh.</returns>
        public List<EntityObject> Explode()
        {
            List<EntityObject> entities = new List<EntityObject>();

            foreach (PolyfaceMeshFace face in this.Faces)
            {
                if (face.VertexIndexes.Length == 1)
                {
                    Point point = new Point
                                      {
                                          Location = this.Vertexes[Math.Abs(face.VertexIndexes[0]) - 1].Location,
                                          Color = this.Color,
                                          IsVisible = this.IsVisible,
                                          Layer = this.Layer,
                                          LineType = this.LineType,
                                          LineTypeScale = this.LineTypeScale,
                                          Lineweight = this.Lineweight,
                                          XData = this.XData
                                      };
                    entities.Add(point);
                    continue;
                }
                if (face.VertexIndexes.Length == 2)
                {
                    Line line = new Line
                    {
                        StartPoint = this.Vertexes[Math.Abs(face.VertexIndexes[0]) - 1].Location,
                        EndPoint = this.Vertexes[Math.Abs(face.VertexIndexes[1]) - 1].Location,
                        Color = this.Color,
                        IsVisible = this.IsVisible,
                        Layer = this.Layer,
                        LineType = this.LineType,
                        LineTypeScale = this.LineTypeScale,
                        Lineweight = this.Lineweight,
                        XData = this.XData
                    };
                    entities.Add(line);
                    continue;
                }

                EdgeFlags edgeVisibility = EdgeFlags.Visibles;

                int indexV1 = face.VertexIndexes[0];
                int indexV2 = face.VertexIndexes[1];
                int indexV3 = face.VertexIndexes[2];
                // Polyface mesh faces are made of 3 or 4 vertexes, we will repeat the third vertex if the face vertexes is three
                int indexV4 = face.VertexIndexes.Length == 3 ? face.VertexIndexes[2] : face.VertexIndexes[3];

                if (indexV1 < 0)
                    edgeVisibility = edgeVisibility | EdgeFlags.First;
                if (indexV2 < 0)
                    edgeVisibility = edgeVisibility | EdgeFlags.Second;
                if (indexV3 < 0)
                    edgeVisibility = edgeVisibility | EdgeFlags.Third;
                if (indexV4 < 0)
                    edgeVisibility = edgeVisibility | EdgeFlags.Fourth;

                Vector3 v1 = this.Vertexes[Math.Abs(indexV1) - 1].Location;
                Vector3 v2 = this.Vertexes[Math.Abs(indexV2) - 1].Location;
                Vector3 v3 = this.Vertexes[Math.Abs(indexV3) - 1].Location;
                Vector3 v4 = this.Vertexes[Math.Abs(indexV4) - 1].Location;

                Face3d face3d = new Face3d
                {
                    FirstVertex = v1,
                    SecondVertex = v2,
                    ThirdVertex = v3,
                    FourthVertex = v4,
                    EdgeFlags = edgeVisibility,
                    Color = this.Color,
                    IsVisible = this.IsVisible,
                    Layer = this.Layer,
                    LineType = this.LineType,
                    LineTypeScale = this.LineTypeScale,
                    Lineweight = this.Lineweight,
                    XData = this.XData
                };

                entities.Add(face3d);
            }
            return entities;
        }

        #endregion

        #region overrides

        /// <summary>
        /// Creates a new PolyfaceMesh that is a copy of the current instance.
        /// </summary>
        /// <returns>A new PolyfaceMesh that is a copy of this instance.</returns>
        public override object Clone()
        {
            List<PolyfaceMeshVertex> copyVertexes = new List<PolyfaceMeshVertex>();
            foreach (PolyfaceMeshVertex vertex in this.vertexes)
            {
                copyVertexes.Add((PolyfaceMeshVertex) vertex.Clone());
            }
            List<PolyfaceMeshFace> copyFaces = new List<PolyfaceMeshFace>();
            foreach (PolyfaceMeshFace face in this.faces)
            {
                copyFaces.Add((PolyfaceMeshFace) face.Clone());
            }
            return new PolyfaceMesh
            {
                //EntityObject properties
                Color = this.color,
                Layer = this.layer,
                LineType = this.lineType,
                Lineweight = this.lineweight,
                LineTypeScale = this.lineTypeScale,
                Normal = this.normal,
                XData = this.xData,
                //PolyfaceMesh properties
                Vertexes = copyVertexes,
                Faces = copyFaces
            };
        }

        #endregion

    }
}