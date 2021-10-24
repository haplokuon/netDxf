#region netDxf library licensed under the MIT License
// 
//                       netDxf library
// Copyright (c) 2019-2021 Daniel Carvajal (haplokuon@gmail.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// 
#endregion

using System;
using System.Collections.Generic;
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
        private readonly List<Vector3> vertexes;
        private PolylineTypeFlags flags;
        private PolylineSmoothType smoothType;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>Polyline3d</c> class.
        /// </summary>
        public Polyline()
            : this(new List<Vector3>(), false)
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

            this.vertexes = new List<Vector3>(vertexes);
            this.flags = isClosed ? PolylineTypeFlags.ClosedPolylineOrClosedPolygonMeshInM | PolylineTypeFlags.Polyline3D : PolylineTypeFlags.Polyline3D;
            this.smoothType = PolylineSmoothType.NoSmooth;
            this.endSequence = new EndSequence(this);
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the polyline <see cref="Vector3">vertex</see> list.
        /// </summary>
        public List<Vector3> Vertexes
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

        /// <summary>
        /// Gets or sets the curve smooth type.
        /// </summary>
        /// <remarks>
        /// The additional polyline vertexes corresponding to the SplineFit will be created when writing the DXF file.
        /// It is not recommended to use any kind of smoothness in polylines other than NoSmooth. Use a Spline entity instead.
        /// </remarks>
        public PolylineSmoothType SmoothType
        {
            get { return this.smoothType; }
            set
            {
                if (value == PolylineSmoothType.NoSmooth)
                {
                    this.flags &= ~PolylineTypeFlags.SplineFit;
                }
                else
                {
                    this.flags |= PolylineTypeFlags.SplineFit;
                }
                this.smoothType = value;
            }
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
            foreach (Vector3 vertex in this.Vertexes)
            {
                Vector3 start;
                Vector3 end;

                if (index == this.Vertexes.Count - 1)
                {
                    if (!this.IsClosed)
                    {
                        break;
                    }
                    start = vertex;
                    end = this.vertexes[0];
                }
                else
                {
                    start = vertex;
                    end = this.vertexes[index + 1];
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

        #region private methods

        /// <summary>
        /// Converts the polyline in a list of vertexes.
        /// </summary>
        /// <param name="precision">Number of vertexes generated, only applicable for smoothed polylines.</param>
        /// <returns>A list vertexes that represents the polyline.</returns>
        public List<Vector3> PolygonalVertexes(int precision)
        {
            int degree;
            if (this.smoothType == PolylineSmoothType.Quadratic)
            {
                degree = 2;
            }
            else if (this.smoothType == PolylineSmoothType.Cubic)
            {
                degree = 3;
            }
            else
            {
                List<Vector3> points = new List<Vector3>(this.vertexes);
                return points;
            }

            List<SplineVertex> ctrlPoints = new List<SplineVertex>();
            foreach (Vector3 vertex in this.vertexes)
            {
                ctrlPoints.Add(new SplineVertex(vertex));
            }

            int replicate = this.IsClosed ? degree : 0;
            for (int i = 0; i < replicate; i++)
            {
                ctrlPoints.Add(ctrlPoints[i]);
            }

            return Spline.NurbsEvaluator(ctrlPoints, null, degree, this.IsClosed, this.IsClosed, Math.Abs(precision));
        }

        /// <summary>
        /// Converts the actual 3d polyline in a 2d polyline (LwPolyline).
        /// </summary>
        /// <param name="precision">Number of vertexes generated, only applicable for smoothed polylines.</param>
        /// <returns>A LwPolyline that represents the polyline.</returns>
        /// <remarks>
        /// The resulting LwPolyline will be a projection of the actual polyline into the plane defined by its normal vector.
        /// </remarks>
        public LwPolyline ToLwPolyline(int precision)
        {
            List<Vector3> vertexes3D = this.PolygonalVertexes(precision);
            List<Vector2> vertexes2D = MathHelper.Transform(vertexes3D, this.Normal, out double _);
            LwPolyline lwPolyline = new LwPolyline(vertexes2D)
            {
                Layer = (Layer) this.Layer.Clone(),
                Linetype = (Linetype) this.Linetype.Clone(),
                Color = (AciColor) this.Color.Clone(),
                Lineweight = this.Lineweight,
                Transparency = (Transparency) this.Transparency.Clone(),
                LinetypeScale = this.LinetypeScale,
                Normal = this.Normal,
                IsClosed = this.IsClosed
            };

            return lwPolyline;
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
            for (int i = 0; i < this.vertexes.Count; i++)
            {
                this.vertexes[i] = transformation * this.vertexes[i] + translation;
            }

            Vector3 newNormal = transformation * this.Normal;
            if (Vector3.Equals(Vector3.Zero, newNormal))
            {
                newNormal = this.Normal;
            }
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
            entityNumber = this.endSequence.AssignHandle(entityNumber);
            return base.AssignHandle(entityNumber);
        }

        /// <summary>
        /// Creates a new Polyline that is a copy of the current instance.
        /// </summary>
        /// <returns>A new Polyline that is a copy of this instance.</returns>
        public override object Clone()
        {
            Polyline entity = new Polyline(this.vertexes)
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

            foreach (XData data in this.XData.Values)
            {
                entity.XData.Add((XData) data.Clone());
            }

            return entity;
        }

        #endregion
    }
}