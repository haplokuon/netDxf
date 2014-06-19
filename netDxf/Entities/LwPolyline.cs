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
    /// Represents a light weight polyline <see cref="EntityObject">entity</see>.
    /// </summary>
    /// <remarks>
    /// Light weight polylines are bidimensional polylines that can hold information about the width of the lines and arcs that compose them.
    /// </remarks>
    public class LwPolyline :
        EntityObject
    {
        #region private fields

        private List<LwPolylineVertex> vertexes;
        private PolylineTypeFlags flags;
        private double elevation;
        private double thickness;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>LwPolyline</c> class.
        /// </summary>
        public LwPolyline()
            : this(new List<LwPolylineVertex>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>LwPolyline</c> class.
        /// </summary>
        /// <param name="vertexes">LwPolyline <see cref="LwPolylineVertex">vertex</see> list in object coordinates.</param>
        /// <param name="isClosed">Sets if the polyline is closed.</param>
        public LwPolyline(List<LwPolylineVertex> vertexes, bool isClosed = false)
            : base(EntityType.LightWeightPolyline, DxfObjectCode.LightWeightPolyline)
        {
            if (vertexes == null)
                throw new ArgumentNullException("vertexes");
            this.vertexes = vertexes;
            this.elevation = 0.0;
            this.thickness = 0.0;
            this.flags = isClosed ? PolylineTypeFlags.ClosedPolylineOrClosedPolygonMeshInM : PolylineTypeFlags.OpenPolyline;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the light weight polyline <see cref="LwPolylineVertex">vertex</see> list.
        /// </summary>
        public List<LwPolylineVertex> Vertexes
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
        /// Gets or sets if the light weight polyline is closed.
        /// </summary>
        public bool IsClosed
        {
            get { return (this.flags & PolylineTypeFlags.ClosedPolylineOrClosedPolygonMeshInM) == PolylineTypeFlags.ClosedPolylineOrClosedPolygonMeshInM; }
            set { this.flags = value ? PolylineTypeFlags.ClosedPolylineOrClosedPolygonMeshInM : PolylineTypeFlags.OpenPolyline; }
        }

        /// <summary>
        /// Gets or sets the light weight polyline thickness.
        /// </summary>
        public double Thickness
        {
            get { return this.thickness; }
            set { this.thickness = value; }
        }

        /// <summary>
        /// Gets or sets the light weight polyline elevation.
        /// </summary>
        public double Elevation
        {
            get { return this.elevation; }
            set { this.elevation = value; }
        }

        #endregion

        #region internal properties

        /// <summary>
        /// Gets the light weight polyline type.
        /// </summary>
        internal PolylineTypeFlags Flags
        {
            get { return this.flags; }
            set { this.flags = value; }
        }   

        #endregion

        #region public methods

        /// <summary>
        /// Sets a constant width for all the polyline segments.
        /// </summary>
        /// <param name="width">Polyline width.</param>
        public void SetConstantWidth(double width)
        {
            foreach (LwPolylineVertex v in this.vertexes)
            {
                v.BeginWidth = width;
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
            int index = 0;
            foreach (LwPolylineVertex vertex in this.Vertexes)
            {
                double bulge = vertex.Bulge;
                Vector2 p1;
                Vector2 p2;

                if (index == this.Vertexes.Count - 1)
                {
                    if (!this.IsClosed) break;
                    p1 = new Vector2(vertex.Location.X, vertex.Location.Y);
                    p2 = new Vector2(this.vertexes[0].Location.X, this.vertexes[0].Location.Y);
                }
                else
                {
                    p1 = new Vector2(vertex.Location.X, vertex.Location.Y);
                    p2 = new Vector2(this.vertexes[index + 1].Location.X, this.vertexes[index + 1].Location.Y);
                }
                if (MathHelper.IsZero(bulge))
                {
                    // the polyline edge is a line
                    Vector3 start = MathHelper.Transform(new Vector3(p1.X, p1.Y, this.elevation), this.normal,
                                                            MathHelper.CoordinateSystem.Object,
                                                            MathHelper.CoordinateSystem.World);
                    Vector3 end = MathHelper.Transform(new Vector3(p2.X, p2.Y, this.elevation), this.normal,
                                                        MathHelper.CoordinateSystem.Object,
                                                        MathHelper.CoordinateSystem.World);

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
                        Normal = this.Normal,
                        Thickness = this.Thickness,
                        XData = this.XData
                    });
                }
                else
                {
                    // the polyline edge is an arc
                    double theta = 4 * Math.Atan(Math.Abs(bulge));
                    double c = Vector2.Distance(p1, p2);
                    double r = (c / 2) / Math.Sin(theta / 2);
                    double gamma = (Math.PI - theta) / 2;
                    double phi = Vector2.Angle(p1, p2) + Math.Sign(bulge) * gamma;
                    Vector2 center = new Vector2(p1.X + r * Math.Cos(phi), p1.Y + r * Math.Sin(phi));
                    double startAngle;
                    double endAngle;
                    if (bulge > 0)
                    {
                        startAngle = MathHelper.RadToDeg * Vector2.Angle(p1 - center);
                        endAngle = startAngle + MathHelper.RadToDeg * theta;
                    }
                    else
                    {
                        endAngle = MathHelper.RadToDeg * Vector2.Angle(p1 - center);
                        startAngle = endAngle - MathHelper.RadToDeg * theta;
                    }
                    Vector3 point = MathHelper.Transform(new Vector3(center.X, center.Y, this.elevation), this.normal,
                                                            MathHelper.CoordinateSystem.Object,
                                                            MathHelper.CoordinateSystem.World);
                    entities.Add(new Arc
                                     {
                                         Center = point,
                                         Radius = r,
                                         StartAngle = startAngle,
                                         EndAngle = endAngle,
                                         Color = this.Color,
                                         IsVisible = this.IsVisible,
                                         Layer = this.Layer,
                                         LineType = this.LineType,
                                         LineTypeScale = this.LineTypeScale,
                                         Lineweight = this.Lineweight,
                                         Normal = this.Normal,
                                         Thickness = this.Thickness,
                                         XData = this.XData
                                     });
                }
                index++;
            }

            return entities;
        }

        /// <summary>
        /// Obtains a list of vertexes that represent the polyline approximating the curve segments as necessary.
        /// </summary>
        /// <param name="bulgePrecision">Curve segments precision (a value of zero means that no approximation will be made).</param>
        /// <param name="weldThreshold">Tolerance to consider if two new generated vertexes are equal.</param>
        /// <param name="bulgeThreshold">Minimun distance from which approximate curved segments of the polyline.</param>
        /// <returns>A list of vertexes expresed in object coordinate system.</returns>
        public IList<Vector2> PoligonalVertexes(int bulgePrecision, double weldThreshold, double bulgeThreshold)
        {
            List<Vector2> ocsVertexes = new List<Vector2>();

            int index = 0;

            foreach (LwPolylineVertex vertex in this.Vertexes)
            {
                double bulge = vertex.Bulge;
                Vector2 p1;
                Vector2 p2;

                if (index == this.Vertexes.Count - 1)
                {
                    p1 = new Vector2(vertex.Location.X, vertex.Location.Y);
                    p2 = new Vector2(this.vertexes[0].Location.X, this.vertexes[0].Location.Y);
                }
                else
                {
                    p1 = new Vector2(vertex.Location.X, vertex.Location.Y);
                    p2 = new Vector2(this.vertexes[index + 1].Location.X, this.vertexes[index + 1].Location.Y);
                }

                if (!p1.Equals(p2, weldThreshold))
                {
                    if (MathHelper.IsZero(bulge) || bulgePrecision == 0)
                    {
                        ocsVertexes.Add(p1);
                    }
                    else
                    {
                        double c = Vector2.Distance(p1, p2);
                        if (c >= bulgeThreshold)
                        {
                            double s = (c / 2) * Math.Abs(bulge);
                            double r = ((c / 2) * (c / 2) + s * s) / (2 * s);
                            double theta = 4 * Math.Atan(Math.Abs(bulge));
                            double gamma = (Math.PI - theta) / 2;
                            double phi = Vector2.Angle(p1, p2) + Math.Sign(bulge) * gamma;
                            Vector2 center = new Vector2(p1.X + r * Math.Cos(phi), p1.Y + r * Math.Sin(phi));
                            Vector2 a1 = p1 - center;
                            double angle = Math.Sign(bulge) * theta / (bulgePrecision + 1);
                            ocsVertexes.Add(p1);
                            for (int i = 1; i <= bulgePrecision; i++)
                            {
                                Vector2 curvePoint = new Vector2();
                                Vector2 prevCurvePoint = new Vector2(this.vertexes[this.vertexes.Count - 1].Location.X, this.vertexes[this.vertexes.Count - 1].Location.Y);
                                curvePoint.X = center.X + Math.Cos(i*angle)*a1.X - Math.Sin(i*angle)*a1.Y;
                                curvePoint.Y = center.Y + Math.Sin(i*angle)*a1.X + Math.Cos(i*angle)*a1.Y;

                                if (!curvePoint.Equals(prevCurvePoint, weldThreshold) && !curvePoint.Equals(p2, weldThreshold))
                                {
                                    ocsVertexes.Add(curvePoint);
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

        #endregion

        #region overrides

        /// <summary>
        /// Creates a new LwPolyline that is a copy of the current instance.
        /// </summary>
        /// <returns>A new LwPolyline that is a copy of this instance.</returns>
        public override object Clone()
        {
            List<LwPolylineVertex> copyVertexes = new List<LwPolylineVertex>();
            foreach (LwPolylineVertex vertex in this.vertexes)
            {
                copyVertexes.Add((LwPolylineVertex) vertex.Clone());
            }
            return new LwPolyline
                {
                    //EntityObject properties
                    Color = this.color,
                    Layer = this.layer,
                    LineType = this.lineType,
                    Lineweight = this.lineweight,
                    LineTypeScale = this.lineTypeScale,
                    Normal = this.normal,
                    XData = this.xData,
                    //LwPolyline properties
                    Vertexes = copyVertexes,
                    Elevation = this.elevation,
                    Thickness = this.thickness,
                    Flags = this.flags
                };
        }

        #endregion


    }
}