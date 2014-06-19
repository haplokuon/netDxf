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
using System.Collections.Generic;

namespace netDxf.Entities
{
    /// <summary>
    /// Represent a loop of a <see cref="Hatch">hatch</see> boundaries.
    /// </summary>
    /// <remarks>
    /// The entities that make a loop can be any combination of lines, polylines, circles, arcs, ellipses, and splines.<br />
    /// The entities that define a loop must define a closed path and they have to be on the same plane as the hatch, 
    /// if these conditions are not met the result will be unpredictable.<br />
    /// The entitiy normal and the elevation will be ignored. Only the x and y coordinates of the line, ellipse, the circle, and spline will be used.
    /// Circles, full ellipses, closed polylines, closed splines are closed paths so only one should exist in the edges list.
    /// Lines, arcs, ellipse arcs, open polylines, and open splines are open paths so more enties should exist to make a closed loop.
    /// </remarks>
    public class HatchBoundaryPath
    {
        public enum EdgeType
        {
            Polyline = 0,
            Line = 1,
            Arc = 2,
            Ellipse = 3,
            Spline = 4
        }

        public abstract class Edge
        {
            public readonly EdgeType Type;

            protected Edge(EdgeType type)
            {
                this.Type = type;
            }

            public abstract EntityObject ConvertTo();

        }

        public class Polyline :
            Edge
        {
            public Vector3[] Vertexes; // location: (x, y) bulge: z
            public bool IsClosed;

            public Polyline()
                : base(EdgeType.Polyline)
            {
            }

            public Polyline(EntityObject entity)
                : base(EdgeType.Polyline)
            {
                if (entity == null) throw new ArgumentNullException("entity");

                if (entity is Entities.LwPolyline)
                {
                    LwPolyline poly = (LwPolyline) entity;
                    if (!poly.IsClosed) throw new ArgumentException("Only closed polyline are supported as hatch boundary edges.", "entity");

                    this.Vertexes = new Vector3[poly.Vertexes.Count];
                    for (int i = 0; i < poly.Vertexes.Count; i++)
                    {
                        this.Vertexes[i] = new Vector3(poly.Vertexes[i].Location.X, poly.Vertexes[i].Location.Y, poly.Vertexes[i].Bulge);
                    }
                    this.IsClosed = true;
                }
                else if (entity is Entities.Polyline)
                {
                    Entities.Polyline poly = (Entities.Polyline)entity;
                    if (!poly.IsClosed) throw new ArgumentException("Only closed polyline are supported as hatch boundary edges.", "entity");

                    this.Vertexes = new Vector3[poly.Vertexes.Count];
                    for (int i = 0; i < poly.Vertexes.Count; i++)
                    {
                        this.Vertexes[i] = new Vector3(poly.Vertexes[i].Location.X, poly.Vertexes[i].Location.Y, 0.0);
                    }
                    this.IsClosed = true;
                }
                else
                    throw new ArgumentException("The entity is not a LwPolyline or a Polyline", "entity");               
            }

            public static Polyline ConvertFrom(EntityObject entity)
            {
                return new Polyline(entity);
            }

            public override EntityObject ConvertTo()
            {
                List<LwPolylineVertex> points = new List<LwPolylineVertex>(this.Vertexes.Length);
                foreach (Vector3 point in this.Vertexes)
                {
                    points.Add(new LwPolylineVertex(point.X, point.Y, point.Z));
                }
                return new Entities.LwPolyline(points, this.IsClosed);
            }
        }

        public class Line :
            Edge
        {
            public Vector2 Start;
            public Vector2 End;

            public Line()
                : base(EdgeType.Line)
            {
            }

            public Line(EntityObject entity)
                : base(EdgeType.Line)
            {
                if (entity == null) throw new ArgumentNullException("entity");

                Entities.Line line = entity as Entities.Line;
                if (line == null) throw new ArgumentException("The entity is not a Line", "entity");

                this.Start = new Vector2(line.StartPoint.X, line.StartPoint.Y);
                this.End = new Vector2(line.EndPoint.X, line.EndPoint.Y);
            }

            public static Line ConvertFrom(EntityObject entity)
            {
                return new Line(entity);
            }

            public override EntityObject ConvertTo()
            {
                return new Entities.Line(this.Start, this.End);
            }
        }

        public class Arc :
            Edge
        {
            public Vector2 Center;
            public double Radius;
            public double StartAngle;
            public double EndAngle;
            public bool IsCounterclockwise;

            public Arc()
                : base(EdgeType.Arc)
            {
            }

            public Arc(EntityObject entity)
                : base(EdgeType.Arc)
            {
                if (entity == null) throw new ArgumentNullException("entity");

                if (entity is Entities.Arc)
                {
                    Entities.Arc arc = (Entities.Arc) entity;
                    this.Center = new Vector2(arc.Center.X, arc.Center.Y);
                    this.Radius = arc.Radius;
                    this.StartAngle = arc.StartAngle;
                    this.EndAngle = arc.EndAngle;
                    this.IsCounterclockwise = true;
                }
                else if (entity is Entities.Circle)
                {
                    Entities.Circle circle = (Circle) entity;
                    this.Center = new Vector2(circle.Center.X, circle.Center.Y);
                    this.Radius = circle.Radius;
                    this.StartAngle = 0.0;
                    this.EndAngle = 360.0;
                    this.IsCounterclockwise = true;
                }
                else
                    throw new ArgumentException("The entity is not a Circle or an Arc", "entity");
            }

            public static Arc ConvertFrom(EntityObject entity)
            {
                return new Arc(entity);
            }

            public override EntityObject ConvertTo()
            {
                if (MathHelper.IsEqual(this.StartAngle, this.EndAngle))
                    return new Entities.Circle(this.Center, this.Radius);
                if(this.IsCounterclockwise)
                    return new Entities.Arc(this.Center, this.Radius, this.StartAngle, this.EndAngle);

                return new Entities.Arc(this.Center, this.Radius, 360 - this.EndAngle, 360 - this.StartAngle);
            }
        }

        public class Ellipse :
            Edge
        {
            public Vector2 Center;
            public Vector2 EndMajorAxis;
            public double MinorRatio;
            public double StartAngle;
            public double EndAngle;
            public bool IsCounterclockwise;

            public Ellipse()
                : base(EdgeType.Ellipse)
            {
            }

            public Ellipse(EntityObject entity)
                : base(EdgeType.Ellipse)
            {
                if (entity == null) throw new ArgumentNullException("entity");

                Entities.Ellipse ellipse = entity as Entities.Ellipse;
                if (ellipse == null) throw new ArgumentException("The entity is not an Ellipse", "entity");

                this.Center = new Vector2(ellipse.Center.X, ellipse.Center.Y);
                double sine = 0.5 * ellipse.MajorAxis * Math.Sin(ellipse.Rotation * MathHelper.DegToRad);
                double cosine = 0.5 * ellipse.MajorAxis * Math.Cos(ellipse.Rotation * MathHelper.DegToRad);
                this.EndMajorAxis = new Vector2(cosine, sine);
                this.MinorRatio = ellipse.MinorAxis / ellipse.MajorAxis;
                this.StartAngle = ellipse.StartAngle;
                this.EndAngle = ellipse.EndAngle;
                this.IsCounterclockwise = true;
            }

            public static Ellipse ConvertFrom(EntityObject entity)
            {
                return new Ellipse(entity);
            }

            public override EntityObject ConvertTo()
            {
                Vector3 center = new Vector3(this.Center.X, this.Center.Y, 0.0);
                Vector3 axisPoint = new Vector3(this.EndMajorAxis.X, this.EndMajorAxis.Y, 0.0);
                Vector3 ocsAxisPoint = MathHelper.Transform(axisPoint,
                                                            Vector3.UnitZ,
                                                            MathHelper.CoordinateSystem.World,
                                                            MathHelper.CoordinateSystem.Object);
                double rotation = Vector2.Angle(new Vector2(ocsAxisPoint.X, ocsAxisPoint.Y)) * MathHelper.RadToDeg;
                double majorAxis = 2 * axisPoint.Modulus();
                return new Entities.Ellipse
                {
                    MajorAxis = majorAxis,
                    MinorAxis = majorAxis * this.MinorRatio,
                    Rotation = rotation,
                    Center = center,
                    StartAngle = this.IsCounterclockwise ? this.StartAngle : 360 - this.EndAngle,
                    EndAngle = this.IsCounterclockwise ? this.EndAngle : 360 - this.StartAngle,
                    Normal = Vector3.UnitZ
                };
            }
        }

        public class Spline :
            Edge
        {
            public short Degree;
            public bool IsRational;
            public bool IsPeriodic;
            public double[] Knots;
            public Vector3[] ControlPoints; // location: (x, y) weight: z

            public Spline()
                : base(EdgeType.Spline)
            {
            }

            public Spline(EntityObject entity)
                : base(EdgeType.Spline)
            {
                if (entity == null) throw new ArgumentNullException("entity");

                Entities.Spline spline = entity as Entities.Spline;
                if (spline == null) throw new ArgumentException("The entity is not an Spline", "entity");

                this.Degree = spline.Degree;
                this.IsRational = (spline.Flags & SplineTypeFlags.Rational) == SplineTypeFlags.Rational;
                this.IsPeriodic = spline.IsPeriodic;
                this.Knots = new double[spline.Knots.Length];
                for (int i = 0; i < spline.Knots.Length; i++)
                {
                    this.Knots[i] = spline.Knots[i];
                }
                this.ControlPoints = new Vector3[spline.ControlPoints.Count];
                for (int i = 0; i < spline.ControlPoints.Count; i++)
                {
                    this.ControlPoints[i] = new Vector3(spline.ControlPoints[i].Location.X, spline.ControlPoints[i].Location.Y, spline.ControlPoints[i].Weigth);
                }
            }

            public static Spline ConvertFrom(EntityObject entity)
            {
                return new Spline(entity);
            }

            public override EntityObject ConvertTo()
            {
                List<SplineVertex> ctrl = new List<SplineVertex>(this.ControlPoints.Length);
                foreach (Vector3 point in ControlPoints)
                {
                    ctrl.Add(new SplineVertex(point.X, point.Y, point.Z));
                }
                return new Entities.Spline(ctrl, this.Knots, this.Degree);
            }
        }

        #region private fields

        private readonly List<Edge> edges;
        private HatchBoundaryPathTypeFlag pathTypeFlag;

        #endregion

        #region constructor

        /// <summary>
        /// Initializes a new instance of the <c>Hatch</c> class.
        /// </summary>
        /// <param name="edges">List of entities that makes a loop for the hatch boundary paths.</param>
        public HatchBoundaryPath(IEnumerable<EntityObject> edges)
        {
            if (edges == null)
                throw new ArgumentNullException("edges");
            this.edges = new List<Edge>();
            this.pathTypeFlag = HatchBoundaryPathTypeFlag.Derived | HatchBoundaryPathTypeFlag.External;
            this.SetInternalInfo(edges);
        }

        internal HatchBoundaryPath(List<Edge> edges)
        {
            if (edges == null)
                throw new ArgumentNullException("edges");
            this.pathTypeFlag = HatchBoundaryPathTypeFlag.Derived | HatchBoundaryPathTypeFlag.External;
            this.edges = edges;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the list of entities that makes a loop for the hatch boundary paths.
        /// </summary>
        public List<Edge> Edges
        {
            get { return this.edges; }
        }

        /// <summary>
        /// Gets the boundary path type flag.
        /// </summary>
        public HatchBoundaryPathTypeFlag PathTypeFlag
        {
            get { return this.pathTypeFlag; }
            internal set { this.pathTypeFlag = value; }
        }

        #endregion

        #region private methods

        private void SetInternalInfo(IEnumerable<EntityObject> entities)
        {
            bool containsClosedPolyline = false;

            foreach (EntityObject entity in entities)
            {
                if ((pathTypeFlag & HatchBoundaryPathTypeFlag.Polyline) == HatchBoundaryPathTypeFlag.Polyline)
                    if (this.edges.Count >= 1) throw new ArgumentException("Closed polylines cannot be combined with other entities to make a hatch boundary path.");

                // it seems that AutoCad does not have problems on creating loops that theorically does not make sense, like, for example an internal loop that is made of a single arc.
                // so if AutoCAD is ok with that I am too, the program that make use of this information will take care of this inconsistencies
                switch (entity.Type)
                {
                    case EntityType.Arc:
                        if (containsClosedPolyline) throw new ArgumentException("Closed polylines cannot be combined with other entities to make a hatch boundary path.");
                        this.edges.Add(Arc.ConvertFrom(entity));
                        break;
                    case EntityType.Circle:
                        if (containsClosedPolyline) throw new ArgumentException("Closed polylines cannot be combined with other entities to make a hatch boundary path.");
                        this.edges.Add(Arc.ConvertFrom(entity));
                        break;
                    case EntityType.Ellipse:
                        if (containsClosedPolyline) throw new ArgumentException("Closed polylines cannot be combined with other entities to make a hatch boundary path.");
                        this.edges.Add(Ellipse.ConvertFrom(entity));
                        break;
                    case EntityType.Line:
                        if (containsClosedPolyline) throw new ArgumentException("Closed polylines cannot be combined with other entities to make a hatch boundary path.");
                        this.edges.Add(Line.ConvertFrom(entity));
                        break;
                    case EntityType.LightWeightPolyline:
                        if (containsClosedPolyline) throw new ArgumentException("Closed polylines cannot be combined with other entities to make a hatch boundary path.");
                        LwPolyline poly = (LwPolyline) entity;
                        if (poly.IsClosed)
                        {
                            this.edges.Add(Polyline.ConvertFrom(entity)); // A polyline HatchBoundaryPath must be closed
                            this.pathTypeFlag |= HatchBoundaryPathTypeFlag.Polyline;
                            containsClosedPolyline = true;
                        }
                        else
                            this.SetInternalInfo(poly.Explode()); // open polylines will always be exploded, only one polyline can be present in a path
                        break;
                    case EntityType.Spline:
                        if (containsClosedPolyline) throw new ArgumentException("Closed polylines cannot be combined with other entities to make a hatch boundary path.");
                        this.edges.Add(Spline.ConvertFrom(entity));
                        break;
                    default:
                        throw new ArgumentException("The entity type " + entity.Type + " unknown or not implemented as part of a hatch boundary.");
                }
            }
        }

        #endregion
    }
}