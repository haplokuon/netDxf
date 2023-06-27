#region netDxf library licensed under the MIT License
// 
//                       netDxf library
// Copyright (c) Daniel Carvajal (haplokuon@gmail.com)
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
using netDxf.Collections;
using netDxf.Tables;

namespace netDxf.Entities
{
    /// <summary>
    /// Represents a hatch <see cref="EntityObject">entity</see>.
    /// </summary>
    public class Hatch :
        EntityObject
    {
        #region delegates and events

        public delegate void HatchBoundaryPathAddedEventHandler(Hatch sender, ObservableCollectionEventArgs<HatchBoundaryPath> e);
        public event HatchBoundaryPathAddedEventHandler HatchBoundaryPathAdded;
        protected virtual void OnHatchBoundaryPathAddedEvent(HatchBoundaryPath item)
        {
            HatchBoundaryPathAddedEventHandler ae = this.HatchBoundaryPathAdded;
            if (ae != null)
            {
                ae(this, new ObservableCollectionEventArgs<HatchBoundaryPath>(item));
            }
        }

        public delegate void HatchBoundaryPathRemovedEventHandler(Hatch sender, ObservableCollectionEventArgs<HatchBoundaryPath> e);
        public event HatchBoundaryPathRemovedEventHandler HatchBoundaryPathRemoved;
        protected virtual void OnHatchBoundaryPathRemovedEvent(HatchBoundaryPath item)
        {
            HatchBoundaryPathRemovedEventHandler ae = this.HatchBoundaryPathRemoved;
            if (ae != null)
            {
                ae(this, new ObservableCollectionEventArgs<HatchBoundaryPath>(item));
            }
        }

        #endregion

        #region private fields

        private readonly ObservableCollection<HatchBoundaryPath> boundaryPaths;
        private HatchPattern pattern;
        private double elevation;
        private bool associative;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>Hatch</c> class.
        /// </summary>
        /// <remarks>
        /// This constructor is initialized with an empty list of boundary paths, remember a hatch without boundaries will be discarded when saving the file.<br/>
        /// When creating an associative hatch do not add the entities that make the boundary to the document, it will be done automatically. Doing so will throw an exception.<br/>
        /// The hatch boundary paths must be on the same plane as the hatch.
        /// The normal and the elevation of the boundary paths will be omitted (the hatch elevation and normal will be used instead).
        /// Only the x and y coordinates for the center of the line, ellipse, circle and arc will be used.
        /// </remarks>
        /// <param name="pattern"><see cref="HatchPattern">Hatch pattern</see>.</param>
        /// <param name="associative">Defines if the hatch is associative or not.</param>
        public Hatch(HatchPattern pattern, bool associative)
            : base(EntityType.Hatch, DxfObjectCode.Hatch)
        {
            this.pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
            this.boundaryPaths = new ObservableCollection<HatchBoundaryPath>();
            this.boundaryPaths.BeforeAddItem += this.BoundaryPaths_BeforeAddItem;
            this.boundaryPaths.AddItem += this.BoundaryPaths_AddItem;
            this.boundaryPaths.BeforeRemoveItem += this.BoundaryPaths_BeforeRemoveItem;
            this.boundaryPaths.RemoveItem += this.BoundaryPaths_RemoveItem;
            this.associative = associative;
        }

        /// <summary>
        /// Initializes a new instance of the <c>Hatch</c> class.
        /// </summary>
        /// <remarks>
        /// The hatch boundary paths must be on the same plane as the hatch.
        /// The normal and the elevation of the boundary paths will be omitted (the hatch elevation and normal will be used instead).
        /// Only the x and y coordinates for the center of the line, ellipse, circle and arc will be used.
        /// </remarks>
        /// <param name="pattern"><see cref="HatchPattern">Hatch pattern</see>.</param>
        /// <param name="paths">A list of <see cref="HatchBoundaryPath">boundary paths</see>.</param>
        /// <param name="associative">Defines if the hatch is associative or not.</param>
        public Hatch(HatchPattern pattern, IEnumerable<HatchBoundaryPath> paths, bool associative)
            : base(EntityType.Hatch, DxfObjectCode.Hatch)
        {
            this.pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));

            if (paths == null)
            {
                throw new ArgumentNullException(nameof(paths));
            }
            this.boundaryPaths = new ObservableCollection<HatchBoundaryPath>();
            this.boundaryPaths.BeforeAddItem += this.BoundaryPaths_BeforeAddItem;
            this.boundaryPaths.AddItem += this.BoundaryPaths_AddItem;
            this.boundaryPaths.BeforeRemoveItem += this.BoundaryPaths_BeforeRemoveItem;
            this.boundaryPaths.RemoveItem += this.BoundaryPaths_RemoveItem;
            this.associative = associative;

            foreach (HatchBoundaryPath path in paths)
            {
                if (associative)
                {
                    // create the entities that make the path if it has been defined as associative but the edges do not have an associated entity
                    if (path.Entities.Count == 0)
                    {
                        foreach (HatchBoundaryPath.Edge edge in path.Edges)
                        {
                            path.AddContour(edge.ConvertTo());
                        }
                    }
                }
                else
                {
                    path.ClearContour();
                }

                this.boundaryPaths.Add(path);
            }
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the hatch pattern.
        /// </summary>
        public HatchPattern Pattern
        {
            get { return this.pattern; }
            set
            {
                this.pattern = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        /// <summary>
        /// Gets the hatch boundary paths.
        /// </summary>
        /// <remarks>
        /// The hatch must contain at least on valid boundary path to be able to add it to the DxfDocument, otherwise it will be rejected.
        /// </remarks>
        public ObservableCollection<HatchBoundaryPath> BoundaryPaths
        {
            get { return this.boundaryPaths; }
        }

        /// <summary>
        /// Gets if the hatch is associative or not, which means if the hatch object is associated with the hatch boundary entities.
        /// </summary>
        public bool Associative
        {
            get { return this.associative; }
        }

        /// <summary>
        /// Gets or sets the hatch elevation, its position along its normal.
        /// </summary>
        public double Elevation
        {
            get { return this.elevation; }
            set { this.elevation = value; }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Unlinks the boundary from the hatch, turning the associative property to false.
        /// </summary>
        /// <returns>The list of unlinked entities from the boundary of the hatch.</returns>
        /// <remarks>The entities that make the hatch boundaries will not be deleted from the document if they already belong to one.</remarks>
        public List<EntityObject> UnLinkBoundary()
        {
            List<EntityObject> boundary = new List<EntityObject>();
            this.associative = false;
            foreach (HatchBoundaryPath path in this.boundaryPaths)
            {
                foreach (EntityObject entity in path.Entities)
                {
                    entity.RemoveReactor(this);
                    boundary.Add(entity);
                }
                path.ClearContour();
            }
            return boundary;
        }

        /// <summary>
        /// Creates a list of entities that represents the boundary of the hatch and optionally associates to it.
        /// </summary>
        /// <param name="linkBoundary">Indicates if the new boundary will be associated with the hatch, turning the associative property to true.</param>
        /// <returns>A list of entities that makes the boundary of the hatch.</returns>
        /// <remarks>
        /// If the actual hatch is already associative, the old boundary entities will be unlinked, but not deleted from the hatch document.
        /// If linkBoundary is true, the new boundary entities will be added to the same layout and document as the hatch, in case it belongs to one,
        /// so, in this case, if you also try to add the return list to the document it will cause an error.<br/>
        /// All entities are in world coordinates except the Polyline2D boundary path since by definition its vertexes are expressed in object coordinates.
        /// </remarks>
        public List<EntityObject> CreateBoundary(bool linkBoundary)
        {
            if (this.associative)
            {
                this.UnLinkBoundary();
            }

            this.associative = linkBoundary;
            List<EntityObject> boundary = new List<EntityObject>();
            Matrix3 trans = MathHelper.ArbitraryAxis(this.Normal);
            Vector3 pos = trans * new Vector3(0.0, 0.0, this.elevation);
            foreach (HatchBoundaryPath path in this.boundaryPaths)
            {
                foreach (HatchBoundaryPath.Edge edge in path.Edges)
                {
                    EntityObject entity = edge.ConvertTo();
                    switch (entity.Type)
                    {
                        case EntityType.Arc:
                            boundary.Add(ProcessArc((Arc) entity, trans, pos));
                            break;
                        case EntityType.Circle:
                            boundary.Add(ProcessCircle((Circle) entity, trans, pos));
                            break;
                        case EntityType.Ellipse:
                            boundary.Add(ProcessEllipse((Ellipse) entity, trans, pos));
                            break;
                        case EntityType.Line:
                            boundary.Add(ProcessLine((Line) entity, trans, pos));
                            break;
                        case EntityType.Polyline2D:
                            // LwPolylines need an special treatment since their vertexes are expressed in object coordinates.
                            boundary.Add(ProcessLwPolyline((Polyline2D) entity, this.Normal, this.elevation));
                            break;
                        case EntityType.Spline:
                            boundary.Add(ProcessSpline((Spline) entity, trans, pos));
                            break;
                    }

                    if (this.associative)
                    {
                        path.AddContour(entity);
                        entity.AddReactor(this);
                        this.OnHatchBoundaryPathAddedEvent(path);
                    }
                }
            }
            return boundary;
        }

        #endregion

        #region private methods

        private static EntityObject ProcessArc(Arc arc, Matrix3 trans, Vector3 pos)
        {
            arc.Center = trans * arc.Center + pos;
            arc.Normal = trans * arc.Normal;
            return arc;
        }

        private static EntityObject ProcessCircle(Circle circle, Matrix3 trans, Vector3 pos)
        {
            circle.Center = trans * circle.Center + pos;
            circle.Normal = trans * circle.Normal;
            return circle;
        }

        private static Ellipse ProcessEllipse(Ellipse ellipse, Matrix3 trans, Vector3 pos)
        {
            ellipse.Center = trans * ellipse.Center + pos;
            ellipse.Normal = trans * ellipse.Normal;
            return ellipse;
        }

        private static Line ProcessLine(Line line, Matrix3 trans, Vector3 pos)
        {
            line.StartPoint = trans * line.StartPoint + pos;
            line.EndPoint = trans * line.EndPoint + pos;
            line.Normal = trans * line.Normal;
            return line;
        }

        private static Polyline2D ProcessLwPolyline(Polyline2D polyline2D, Vector3 normal, double elevation)
        {
            polyline2D.Elevation = elevation;
            polyline2D.Normal = normal;
            return polyline2D;
        }

        private static Spline ProcessSpline(Spline spline, Matrix3 trans, Vector3 pos)
        {
            for (int i = 0; i < spline.ControlPoints.Length; i++)
            {
                spline.ControlPoints[i] = trans * spline.ControlPoints[i] + pos;
            }
            spline.Normal = trans * spline.Normal;
            return spline;
        }

        #endregion

        #region overrides

        // TODO: apply the transformation directly to edges
        //public void TransformBy2(Matrix3 transformation, Vector3 translation)
        //{
        //    if (this.associative)
        //    {
        //        this.UnLinkBoundary();
        //    }

        //    Vector3 newNormal = transformation * this.Normal;
        //    if (Vector3.Equals(Vector3.Zero, newNormal))
        //    {
        //        newNormal = this.Normal;
        //    }

        //    Matrix3 transOW = MathHelper.ArbitraryAxis(this.Normal);
        //    Matrix3 transWO = MathHelper.ArbitraryAxis(newNormal).Transpose();

        //    Vector3 position = transOW * new Vector3(0.0, 0.0, this.Elevation);

        //    foreach (HatchBoundaryPath path in this.BoundaryPaths)
        //    {
        //        foreach (HatchBoundaryPath.Edge edge in path.Edges)
        //        {
        //            switch (edge.Type)
        //            {
        //                case HatchBoundaryPath.EdgeType.Arc:
        //                    break;

        //                case HatchBoundaryPath.EdgeType.Ellipse:
        //                    break;

        //                case HatchBoundaryPath.EdgeType.Line:
        //                    HatchBoundaryPath.Line line = (HatchBoundaryPath.Line)edge;
        //                    Vector3 start = new Vector3(line.Start.X, line.Start.Y, 0.0);
        //                    Vector3 end = new Vector3(line.End.X, line.End.Y, 0.0);

        //                    // to world coordinates
        //                    start = transOW * start + position;
        //                    end = transOW * end + position;

        //                    // transformation
        //                    start = transformation * start + translation;
        //                    end = transformation * end + translation;

        //                    Vector3 point;
        //                    point = transWO * start;
        //                    line.Start = new Vector2(point.X, point.Y);
        //                    point = transWO * end;
        //                    line.End = new Vector2(point.X, point.Y);
        //                    break;

        //                case HatchBoundaryPath.EdgeType.Polyline:
        //                    break;

        //                case HatchBoundaryPath.EdgeType.Spline:
        //                    break;
        //            }
        //        }
        //    }

        //    position = transformation * position + translation;
        //    position = transWO * position;

        //    Vector2 refAxis = Vector2.Rotate(Vector2.UnitX, this.Pattern.Angle * MathHelper.DegToRad);
        //    refAxis = this.Pattern.Scale * refAxis;
        //    Vector3 v = transOW * new Vector3(refAxis.X, refAxis.Y, 0.0);
        //    v = transformation * v;
        //    v = transWO * v;
        //    Vector2 axis = new Vector2(v.X, v.Y);
        //    double newAngle = Vector2.Angle(axis) * MathHelper.RadToDeg;

        //    double newScale = axis.Modulus();
        //    newScale = MathHelper.IsZero(newScale) ? MathHelper.Epsilon : newScale;

        //    this.Pattern.Scale = newScale;
        //    this.Pattern.Angle = newAngle;
        //    this.Elevation = position.Z;
        //    this.Normal = newNormal;
        //}

        /// <summary>
        /// Moves, scales, and/or rotates the current entity given a 3x3 transformation matrix and a translation vector.
        /// </summary>
        /// <param name="transformation">Transformation matrix.</param>
        /// <param name="translation">Translation vector.</param>
        /// <remarks>Matrix3 adopts the convention of using column vectors to represent a transformation matrix.</remarks>
        public override void TransformBy(Matrix3 transformation, Vector3 translation)
        {
            if (this.associative)
            {
                this.UnLinkBoundary();
            }

            Vector3 newNormal = transformation * this.Normal;
            if (Vector3.Equals(Vector3.Zero, newNormal))
            {
                newNormal = this.Normal;
            }

            Matrix3 transOW = MathHelper.ArbitraryAxis(this.Normal);
            Matrix3 transWO = MathHelper.ArbitraryAxis(newNormal).Transpose();

            Vector3 position = transOW * new Vector3(0.0, 0.0, this.Elevation);

            List<HatchBoundaryPath> paths = new List<HatchBoundaryPath>();

            foreach (HatchBoundaryPath path in this.BoundaryPaths)
            {
                List<EntityObject> data = new List<EntityObject>();

                foreach (HatchBoundaryPath.Edge edge in path.Edges)
                {
                    EntityObject entity = edge.ConvertTo();

                    switch (entity.Type)
                    {
                        case EntityType.Arc:
                            entity = ProcessArc((Arc) entity, transOW, position);
                            break;
                        case EntityType.Circle:
                            entity = ProcessCircle((Circle) entity, transOW, position);
                            break;
                        case EntityType.Ellipse:
                            entity = ProcessEllipse((Ellipse) entity, transOW, position);
                            break;
                        case EntityType.Line:
                            entity = ProcessLine((Line) entity, transOW, position);
                            break;
                        case EntityType.Polyline2D:
                            entity = ProcessLwPolyline((Polyline2D) entity, this.Normal, this.Elevation);
                            break;
                        case EntityType.Spline:
                            entity = ProcessSpline((Spline) entity, transOW, position);
                            break;
                    }
                    entity.TransformBy(transformation, translation);
                    data.Add(entity);
                }
                paths.Add(new HatchBoundaryPath(data));
            }

            position = transformation * position + translation;
            position = transWO * position;

            Vector2 refAxis = Vector2.Rotate(Vector2.UnitX, this.Pattern.Angle * MathHelper.DegToRad);
            refAxis = this.Pattern.Scale * refAxis;
            Vector3 v = transOW * new Vector3(refAxis.X, refAxis.Y, 0.0);
            v = transformation * v;
            v = transWO * v;
            Vector2 axis = new Vector2(v.X, v.Y);
            double newAngle = Vector2.Angle(axis) * MathHelper.RadToDeg;

            double newScale = axis.Modulus();
            newScale = MathHelper.IsZero(newScale) ? MathHelper.Epsilon : newScale;

            this.Pattern.Scale = newScale;
            this.Pattern.Angle = newAngle;
            this.Elevation = position.Z;
         
            this.Normal = newNormal;
            this.BoundaryPaths.Clear();
            this.BoundaryPaths.AddRange(paths);
        }

        /// <summary>
        /// Creates a new Hatch that is a copy of the current instance.
        /// </summary>
        /// <returns>A new Hatch that is a copy of this instance.</returns>
        /// <remarks>
        /// If the hatch is associative the referenced boundary entities will not be automatically cloned, its associative property will be set to false.
        /// Use CreateBoundary if required after cloning.
        /// </remarks>
        public override object Clone()
        {
            Hatch entity = new Hatch((HatchPattern) this.pattern.Clone(), false)
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
                //Hatch properties
                Elevation = this.elevation
            };

            foreach (HatchBoundaryPath path in this.boundaryPaths)
            {
                entity.boundaryPaths.Add((HatchBoundaryPath) path.Clone());
            }

            foreach (XData data in this.XData.Values)
            {
                entity.XData.Add((XData) data.Clone());
            }

            return entity;
        }

        #endregion

        #region HatchBoundaryPath collection events

        private void BoundaryPaths_BeforeAddItem(ObservableCollection<HatchBoundaryPath> sender, ObservableCollectionEventArgs<HatchBoundaryPath> e)
        {
            // null items are not allowed in the list.
            if (e.Item == null)
            {
                e.Cancel = true;
            }
            e.Cancel = false;
        }

        private void BoundaryPaths_AddItem(ObservableCollection<HatchBoundaryPath> sender, ObservableCollectionEventArgs<HatchBoundaryPath> e)
        {
            if (this.associative)
            {
                foreach (EntityObject entity in e.Item.Entities)
                {
                    entity.AddReactor(this);
                }
            }
            else
            {
                e.Item.ClearContour();
            }

            this.OnHatchBoundaryPathAddedEvent(e.Item);
        }

        private void BoundaryPaths_BeforeRemoveItem(ObservableCollection<HatchBoundaryPath> sender, ObservableCollectionEventArgs<HatchBoundaryPath> e)
        {
        }

        private void BoundaryPaths_RemoveItem(ObservableCollection<HatchBoundaryPath> sender, ObservableCollectionEventArgs<HatchBoundaryPath> e)
        {
            if (this.associative)
            {
                foreach (EntityObject entity in e.Item.Entities)
                {
                    entity.RemoveReactor(this);
                }
            }

            this.OnHatchBoundaryPathRemovedEvent(e.Item);
        }

        #endregion
    }
}