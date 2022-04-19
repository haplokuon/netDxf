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
    /// Represents a two dimensional polyline <see cref="EntityObject">entity</see>.
    /// </summary>
    /// <remarks>
    /// Two dimensional polylines can hold information about the width of the lines and arcs that compose them.
    /// </remarks>
    public class Polyline2D :
        EntityObject
    {
        #region private fields

        private static short defaultSplineSegs = 8;
        private readonly List<Polyline2DVertex> vertexes;
        private PolylineTypeFlags flags;
        private double elevation;
        private double thickness;
        private PolylineSmoothType smoothType;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>Polyline2D</c> class.
        /// </summary>
        public Polyline2D()
            : this(new List<Polyline2DVertex>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Polyline2D</c> class.
        /// </summary>
        /// <param name="vertexes">Polyline2D <see cref="Vector2">vertex</see> list in object coordinates.</param>
        public Polyline2D(IEnumerable<Vector2> vertexes)
            : this(vertexes, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Polyline2D</c> class.
        /// </summary>
        /// <param name="vertexes">Polyline2D <see cref="Vector2">vertex</see> list in object coordinates.</param>
        /// <param name="isClosed">Sets if the polyline is closed, by default it will create an open polyline.</param>
        public Polyline2D(IEnumerable<Vector2> vertexes, bool isClosed)
            : base(EntityType.Polyline2D, DxfObjectCode.LwPolyline)
        {
            if (vertexes == null)
            {
                throw new ArgumentNullException(nameof(vertexes));
            }

            this.vertexes = new List<Polyline2DVertex>();
            foreach (Vector2 vertex in vertexes)
            {
                this.vertexes.Add(new Polyline2DVertex(vertex));
            }

            this.elevation = 0.0;
            this.thickness = 0.0;
            this.flags = isClosed ? PolylineTypeFlags.ClosedPolylineOrClosedPolygonMeshInM : PolylineTypeFlags.OpenPolyline;
            this.smoothType = PolylineSmoothType.NoSmooth;
        }

        /// <summary>
        /// Initializes a new instance of the <c>Polyline2D</c> class.
        /// </summary>
        /// <param name="vertexes">Polyline2D <see cref="Polyline2DVertex">vertex</see> list in object coordinates.</param>
        public Polyline2D(IEnumerable<Polyline2DVertex> vertexes)
            : this(vertexes, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Polyline2D</c> class.
        /// </summary>
        /// <param name="vertexes">Polyline2D <see cref="Polyline2DVertex">vertex</see> list in object coordinates.</param>
        /// <param name="isClosed">Sets if the polyline is closed (default: false).</param>
        public Polyline2D(IEnumerable<Polyline2DVertex> vertexes, bool isClosed)
            : base(EntityType.Polyline2D, DxfObjectCode.LwPolyline)
        {
            if (vertexes == null)
            {
                throw new ArgumentNullException(nameof(vertexes));
            }

            this.vertexes = new List<Polyline2DVertex>(vertexes);
            this.elevation = 0.0;
            this.thickness = 0.0;
            this.flags = isClosed ? PolylineTypeFlags.ClosedPolylineOrClosedPolygonMeshInM : PolylineTypeFlags.OpenPolyline;
            this.smoothType = PolylineSmoothType.NoSmooth;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets if the default SplineSegs value.
        /// </summary>
        /// <remarks>
        /// This value is used by the Explode method when the current Polyline2D does not belong to a DXF document.
        /// </remarks>
        public static short DefaultSplineSegs
        {
            get { return defaultSplineSegs; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Values must be greater than 0.");
                }
                defaultSplineSegs = value;
            }
        }

        /// <summary>
        /// Gets or sets the polyline <see cref="Polyline2DVertex">vertex</see> list.
        /// </summary>
        public List<Polyline2DVertex> Vertexes
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
        /// Gets or sets the polyline thickness.
        /// </summary>
        public double Thickness
        {
            get { return this.thickness; }
            set { this.thickness = value; }
        }

        /// <summary>
        /// Gets or sets the polyline elevation.
        /// </summary>
        /// <remarks>This is the distance from the origin to the plane of the light weight polyline.</remarks>
        public double Elevation
        {
            get { return this.elevation; }
            set { this.elevation = value; }
        }

        /// <summary>
        /// Enable or disable if the linetype pattern is generated continuously around the vertexes of the polyline.
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
        /// Gets or sets the polyline smooth type.
        /// </summary>
        /// <remarks>
        /// The additional polyline vertexes corresponding to the SplineFit will be created when writing the DXF file.
        /// </remarks>
        public PolylineSmoothType SmoothType
        {
            get { return this.smoothType; }
            set
            {
                if (value == PolylineSmoothType.NoSmooth)
                {
                    this.CodeName = DxfObjectCode.LwPolyline;
                    this.flags &= ~PolylineTypeFlags.SplineFit;
                }
                else
                {
                    this.CodeName = DxfObjectCode.Polyline;
                    this.flags |= PolylineTypeFlags.SplineFit;
                }
                this.smoothType = value;
            }
        }

        #endregion

        #region internal properties

        /// <summary>
        /// Gets the polyline flags.
        /// </summary>
        internal PolylineTypeFlags Flags
        {
            get { return this.flags; }
            set { this.flags = value; }
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

            double firstBulge = this.vertexes[0].Bulge;
       
            for (int i = 0; i < this.vertexes.Count - 1; i++)
            {
                this.vertexes[i].Bulge = -this.vertexes[i + 1].Bulge;
            }

            this.vertexes[this.vertexes.Count - 1].Bulge = -firstBulge;
        }

        /// <summary>
        /// Sets a constant width for all the polyline segments.
        /// </summary>
        /// <param name="width">Polyline width.</param>
        /// <remarks>
        /// Smoothed polylines can only have a constant width, the start width of the first vertex will be used.
        /// </remarks>
        public void SetConstantWidth(double width)
        {
            foreach (Polyline2DVertex v in this.vertexes)
            {
                v.StartWidth = width;
                v.EndWidth = width;
            }
        }

        /// <summary>
        /// Decompose the actual polyline in its internal entities, <see cref="Line">lines</see> and <see cref="Arc">arcs</see>.
        /// </summary>
        /// <returns>A list of <see cref="Line">lines</see> and <see cref="Arc">arcs</see> that made up the polyline.</returns>
        public List<EntityObject> Explode()
        {
            List<EntityObject> entities = new List<EntityObject>();

            if (this.smoothType == PolylineSmoothType.NoSmooth)
            {
                int index = 0;
                foreach (Polyline2DVertex vertex in this.Vertexes)
                {
                    double bulge = vertex.Bulge;
                    Vector2 p1;
                    Vector2 p2;

                    if (index == this.Vertexes.Count - 1)
                    {
                        if (!this.IsClosed)
                        {
                            break;
                        }
                        p1 = new Vector2(vertex.Position.X, vertex.Position.Y);
                        p2 = new Vector2(this.vertexes[0].Position.X, this.vertexes[0].Position.Y);
                    }
                    else
                    {
                        p1 = new Vector2(vertex.Position.X, vertex.Position.Y);
                        p2 = new Vector2(this.vertexes[index + 1].Position.X, this.vertexes[index + 1].Position.Y);
                    }

                    if (MathHelper.IsZero(bulge))
                    {
                        // the polyline edge is a line
                        Vector3 start = MathHelper.Transform(new Vector3(p1.X, p1.Y, this.elevation), this.Normal, CoordinateSystem.Object, CoordinateSystem.World);
                        Vector3 end = MathHelper.Transform(new Vector3(p2.X, p2.Y, this.elevation), this.Normal, CoordinateSystem.Object, CoordinateSystem.World);

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
                            Thickness = this.Thickness
                        });
                    }
                    else
                    {
                        // the polyline edge is an arc
                        double theta = 4 * Math.Atan(Math.Abs(bulge));
                        double c = Vector2.Distance(p1, p2) / 2.0;
                        double r = c / Math.Sin(theta / 2.0);

                        // avoid arcs with very small radius, draw a line instead
                        if (MathHelper.IsZero(r))
                        {
                            // the polyline edge is a line
                            List<Vector3> points = MathHelper.Transform(
                                new []
                                {
                                    new Vector3(p1.X, p1.Y, this.elevation),
                                    new Vector3(p2.X, p2.Y, this.elevation)
                                },
                                this.Normal,
                                CoordinateSystem.Object, CoordinateSystem.World);

                            entities.Add(new Line
                            {
                                Layer = (Layer)this.Layer.Clone(),
                                Linetype = (Linetype)this.Linetype.Clone(),
                                Color = (AciColor)this.Color.Clone(),
                                Lineweight = this.Lineweight,
                                Transparency = (Transparency)this.Transparency.Clone(),
                                LinetypeScale = this.LinetypeScale,
                                Normal = this.Normal,
                                StartPoint = points[0],
                                EndPoint = points[1],
                                Thickness = this.Thickness,
                            });
                        }
                        else
                        {
                            double gamma = (Math.PI - theta) / 2;
                            double phi = Vector2.Angle(p1, p2) + Math.Sign(bulge) * gamma;
                            Vector2 center = new Vector2(p1.X + r * Math.Cos(phi), p1.Y + r * Math.Sin(phi));
                            double startAngle;
                            double endAngle;
                            if (bulge > 0)
                            {
                                startAngle = MathHelper.RadToDeg*Vector2.Angle(p1 - center);
                                endAngle = startAngle + MathHelper.RadToDeg*theta;
                            }
                            else
                            {
                                endAngle = MathHelper.RadToDeg*Vector2.Angle(p1 - center);
                                startAngle = endAngle - MathHelper.RadToDeg*theta;
                            }
                            Vector3 point = MathHelper.Transform(new Vector3(center.X, center.Y,
                                this.elevation),
                                this.Normal,
                                CoordinateSystem.Object,
                                CoordinateSystem.World);

                            entities.Add(new Arc
                            {
                                Layer = (Layer) this.Layer.Clone(),
                                Linetype = (Linetype) this.Linetype.Clone(),
                                Color = (AciColor) this.Color.Clone(),
                                Lineweight = this.Lineweight,
                                Transparency = (Transparency) this.Transparency.Clone(),
                                LinetypeScale = this.LinetypeScale,
                                Normal = this.Normal,
                                Center = point,
                                Radius = r,
                                StartAngle = startAngle,
                                EndAngle = endAngle,
                                Thickness = this.Thickness,
                            });
                        }
                    }
                    index++;
                }
                return entities;
            }

            Vector3[] wcsVertexes = new Vector3[this.vertexes.Count];
            Matrix3 trans = MathHelper.ArbitraryAxis(this.Normal);
            for (int i = 0; i < this.vertexes.Count; i++)
            {
                Vector3 wcsVertex = trans * new Vector3(this.vertexes[i].Position.X, this.vertexes[i].Position.Y, this.elevation);
                wcsVertexes[i] = wcsVertex;
            }

            int degree = this.smoothType == PolylineSmoothType.Quadratic ? 2 : 3;
            int splineSegs = this.Owner == null ? DefaultSplineSegs : this.Owner.Record.Owner.Owner.DrawingVariables.SplineSegs;
            int precision = this.IsClosed ? splineSegs * this.Vertexes.Count : splineSegs * (this.Vertexes.Count - 1);
            List<Vector3> splinePoints = Spline.NurbsEvaluator(wcsVertexes, null, null, degree, false, this.IsClosed, precision);

            for (int i = 1; i < splinePoints.Count; i++)
            {
                Vector3 start = splinePoints[i - 1];
                Vector3 end = splinePoints[i];
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
                    Thickness = this.Thickness
                });
            }

            if (this.IsClosed)
            {
                entities.Add(new Line
                {
                    Layer = (Layer) this.Layer.Clone(),
                    Linetype = (Linetype) this.Linetype.Clone(),
                    Color = (AciColor) this.Color.Clone(),
                    Lineweight = this.Lineweight,
                    Transparency = (Transparency) this.Transparency.Clone(),
                    LinetypeScale = this.LinetypeScale,
                    Normal = this.Normal,
                    StartPoint = splinePoints[splinePoints.Count - 1],
                    EndPoint = splinePoints[0],
                    Thickness = this.Thickness
                });
            }

            return entities;
        }

        /// <summary>
        /// Obtains a list of vertexes that represent the polyline approximating the curve segments as necessary.
        /// </summary>
        /// <param name="precision">The number of vertexes created for curve segments.</param>
        /// <returns>A list of vertexes expressed in object coordinate system.</returns>
        /// <remarks>
        /// For vertexes with bulge values different than zero a precision of zero means that no approximation will be made.
        /// For smoothed polylines the minimum number of vertexes generated is 2.
        /// </remarks>
        public List<Vector2> PolygonalVertexes(int precision)
        {
            return this.PolygonalVertexes(precision, MathHelper.Epsilon, MathHelper.Epsilon);
        }

        /// <summary>
        /// Obtains a list of vertexes that represent the polyline approximating the curve segments as necessary.
        /// </summary>
        /// <param name="precision">The number of vertexes created for curve segments.</param>
        /// <param name="weldThreshold">Tolerance to consider if two new generated vertexes are equal.</param>
        /// <param name="bulgeThreshold">Minimum distance from which approximate curved segments of the polyline.</param>
        /// <returns>A list of vertexes expressed in object coordinate system.</returns>
        /// <remarks>
        /// For vertexes with bulge values different than zero a precision of zero means that no approximation will be made.
        /// For smoothed polylines the minimum number of vertexes generated is 2.
        /// </remarks>
        public List<Vector2> PolygonalVertexes(int precision, double weldThreshold, double bulgeThreshold)
        {
            if (precision < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(precision), precision, "The bulgePrecision must be equal or greater than zero.");
            }

            List<Vector2> ocsVertexes = new List<Vector2>();
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
                int index = 0;

                foreach (Polyline2DVertex vertex in this.Vertexes)
                {
                    double bulge = vertex.Bulge;
                    Vector2 p1;
                    Vector2 p2;

                    if (index == this.Vertexes.Count - 1)
                    {
                        p1 = new Vector2(vertex.Position.X, vertex.Position.Y);
                        if (!this.IsClosed)
                        {
                            ocsVertexes.Add(p1);
                            continue;
                        }
                        p2 = new Vector2(this.vertexes[0].Position.X, this.vertexes[0].Position.Y);
                    }
                    else
                    {
                        p1 = new Vector2(vertex.Position.X, vertex.Position.Y);
                        p2 = new Vector2(this.vertexes[index + 1].Position.X, this.vertexes[index + 1].Position.Y);
                    }

                    if (!p1.Equals(p2, weldThreshold))
                    {
                        if (MathHelper.IsZero(bulge) || precision == 0)
                        {
                            ocsVertexes.Add(p1);
                        }
                        else
                        {
                            double c = Vector2.Distance(p1, p2) * 0.5;
                            if (c >= bulgeThreshold)
                            {
                                double s = c * Math.Abs(bulge);
                                double r = (c * c + s * s) / (2.0 * s);
                                double theta = 4 * Math.Atan(Math.Abs(bulge));
                                double gamma = (Math.PI - theta) * 0.5;
                                double phi = Vector2.Angle(p1, p2) + Math.Sign(bulge) * gamma;
                                Vector2 center = new Vector2(p1.X + r * Math.Cos(phi), p1.Y + r * Math.Sin(phi));
                                Vector2 a1 = p1 - center;
                                double angle = Math.Sign(bulge) * theta / (precision + 1);
                                ocsVertexes.Add(p1);
                                Vector2 prevCurvePoint = p1;
                                for (int i = 1; i <= precision; i++)
                                {
                                    Vector2 curvePoint = new Vector2
                                    {
                                        X = center.X + Math.Cos(i * angle) * a1.X - Math.Sin(i * angle) * a1.Y,
                                        Y = center.Y + Math.Sin(i * angle) * a1.X + Math.Cos(i * angle) * a1.Y
                                    };

                                    if (!curvePoint.Equals(prevCurvePoint, weldThreshold) && !curvePoint.Equals(p2, weldThreshold))
                                    {
                                        ocsVertexes.Add(curvePoint);
                                        prevCurvePoint = curvePoint;
                                    }
                                }
                            }
                            else
                            {
                                ocsVertexes.Add(p1);
                            }
                        }
                    }
                    index++;
                }

                return ocsVertexes;
            }

            // the minimum number of vertexes generated for smoothed polylines is 2
            if (precision < 2)
            {
                precision = 2;
            }

            Vector3[] ctrlPoints = new Vector3[this.vertexes.Count];
            for (int i = 0; i < this.vertexes.Count; i++)
            {
                Vector2 position = this.vertexes[i].Position;
                ctrlPoints[i] = new Vector3(position.X, position.Y, 0.0);
            }

            // closed polylines will be considered as closed and periodic
            List<Vector3> points = Spline.NurbsEvaluator(ctrlPoints, null, null, degree, false, this.IsClosed, precision);
            foreach (Vector3 point in points)
            {
                ocsVertexes.Add(new Vector2(point.X, point.Y));
            }

            return ocsVertexes;
        }

        #endregion

        #region overrides

        /// <summary>
        /// Moves, scales, and/or rotates the current entity given a 3x3 transformation matrix and a translation vector.
        /// </summary>
        /// <param name="transformation">Transformation matrix.</param>
        /// <param name="translation">Translation vector.</param>
        /// <remarks>
        /// Non-uniform scaling is not supported if a bulge different than zero is applied to any of the Polyline2D vertexes,
        /// a non-uniform scaling cannot be applied to the arc segments. Explode the entity and convert the arcs into ellipse arcs and transform them instead.<br />
        /// Matrix3 adopts the convention of using column vectors to represent a transformation matrix.
        /// </remarks>
        public override void TransformBy(Matrix3 transformation, Vector3 translation)
        {
            double newElevation = this.Elevation;
            Vector3 newNormal = transformation * this.Normal;
            if (Vector3.Equals(Vector3.Zero, newNormal))
            {
                newNormal = this.Normal;
            }

            Matrix3 transOW = MathHelper.ArbitraryAxis(this.Normal);
            Matrix3 transWO = MathHelper.ArbitraryAxis(newNormal).Transpose();

            foreach (Polyline2DVertex vertex in this.Vertexes)
            {
                Vector3 v = transOW * new Vector3(vertex.Position.X, vertex.Position.Y, this.Elevation);
                v = transformation * v + translation;
                v = transWO * v;
                vertex.Position = new Vector2(v.X, v.Y);
                newElevation = v.Z;
            }
            this.Elevation = newElevation;
            this.Normal = newNormal;
        }

        /// <summary>
        /// Creates a new Polyline2D that is a copy of the current instance.
        /// </summary>
        /// <returns>A new Polyline2D that is a copy of this instance.</returns>
        public override object Clone()
        {
            Polyline2D entity = new Polyline2D
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
                //LwPolyline properties
                Elevation = this.elevation,
                Thickness = this.thickness,
                Flags = this.flags
            };

            foreach (Polyline2DVertex vertex in this.vertexes)
            {
                entity.Vertexes.Add((Polyline2DVertex) vertex.Clone());
            }

            foreach (XData data in this.XData.Values)
            {
                entity.XData.Add((XData) data.Clone());
            }

            return entity;
        }

        #endregion
    }
}