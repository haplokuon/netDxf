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
    /// Represents a light weight polyline <see cref="IEntityObject">entity</see>.
    /// </summary>
    /// <remarks>
    /// Light weight polylines are bidimensional polylines that can hold information about the width of the lines and arcs that compose them.
    /// </remarks>
    public class LwPolyline :
        DxfObject,
        IEntityObject
    {
        #region private fields

        private const EntityType TYPE = EntityType.LightWeightPolyline;
        private List<LwPolylineVertex> vertexes;
        private bool isClosed;
        private PolylineTypeFlags flags;
        private Layer layer;
        private AciColor color;
        private LineType lineType;
        private Vector3 normal;
        private double elevation;
        private double thickness;
        private Dictionary<ApplicationRegistry, XData> xData;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>LwPolyline</c> class.
        /// </summary>
        /// <param name="vertexes">LwPolyline <see cref="LwPolylineVertex">vertex</see> list in object coordinates.</param>
        /// <param name="isClosed">Sets if the polyline is closed</param>
        public LwPolyline(List<LwPolylineVertex> vertexes, bool isClosed = false)
            : base(DxfObjectCode.LightWeightPolyline)
        {
            this.vertexes = vertexes;
            this.isClosed = isClosed;
            this.layer = Layer.Default;
            this.color = AciColor.ByLayer;
            this.lineType = LineType.ByLayer;
            this.normal = Vector3.UnitZ;
            this.elevation = 0.0;
            this.thickness = 0.0;
            this.flags = isClosed ? PolylineTypeFlags.ClosedPolylineOrClosedPolygonMeshInM : PolylineTypeFlags.OpenPolyline;
        }

        /// <summary>
        /// Initializes a new instance of the <c>LwPolyline</c> class.
        /// </summary>
        public LwPolyline()
            : this(new List<LwPolylineVertex>())
        {
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
        public virtual bool IsClosed
        {
            get { return this.isClosed; }
            set
            {
                this.flags = value ? PolylineTypeFlags.ClosedPolylineOrClosedPolygonMeshInM : PolylineTypeFlags.OpenPolyline;
                this.isClosed = value;
            }
        }

        /// <summary>
        /// Gets or sets the light weight polyline <see cref="Vector3">normal</see>.
        /// </summary>
        public Vector3 Normal
        {
            get { return this.normal; }
            set
            {
                if (Vector3.Zero == value)
                    throw new ArgumentNullException("value", "The normal can not be the zero vector");
                value.Normalize();
                this.normal = value;
            }
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

        /// <summary>
        /// Gets the light weight polyline type.
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
        /// <remarks>
        /// Makes the opposite function as the Join() method.
        /// </remarks>
        /// <returns>A list of <see cref="Line">lines</see>see> and <see cref="Arc">arcs</see> that made up the polyline.</returns>
        public List<IEntityObject> Explode()
        {
            List<IEntityObject> entities = new List<IEntityObject>();
            int index = 0;
            foreach (LwPolylineVertex vertex in this.Vertexes)
            {
                double bulge = vertex.Bulge;
                Vector2 p1;
                Vector2 p2;

                if (index == this.Vertexes.Count - 1)
                {
                    if (!this.isClosed) break;
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

                    entities.Add(new Line(start, end)
                    {
                        Color = this.color,
                        Layer = this.layer,
                        LineType = this.lineType,
                        Normal = this.normal,
                        Thickness = this.thickness,
                        XData = this.xData
                    });
                }
                else
                {
                    // the polyline edge is an arc
                    double theta = 4 * Math.Atan(Math.Abs(bulge));
                    double c = Vector2.Distance(p1, p2);
                    double r = (c / 2) / Math.Sin(theta / 2);
                    double gamma = (Math.PI - theta) / 2;
                    double phi = Vector2.AngleBetween(p1, p2) + Math.Sign(bulge) * gamma;
                    Vector2 center = new Vector2(p1.X + r * Math.Cos(phi), p1.Y + r * Math.Sin(phi));
                    double startAngle;
                    double endAngle;
                    if (bulge > 0)
                    {
                        startAngle = MathHelper.RadToDeg * Vector2.AngleBetween(Vector2.UnitX, p1 - center);
                        endAngle = startAngle + MathHelper.RadToDeg * theta;
                    }
                    else
                    {
                        endAngle = MathHelper.RadToDeg * Vector2.AngleBetween(Vector2.UnitX, p1 - center);
                        startAngle = endAngle - MathHelper.RadToDeg * theta;
                    }
                    entities.Add(new Arc(new Vector3(center.X, center.Y, this.elevation), r, startAngle, endAngle)
                    {
                        Color = this.color,
                        Layer = this.layer,
                        LineType = this.lineType,
                        Normal = this.normal,
                        Thickness = this.thickness,
                        XData = this.xData
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
        public List<Vector2> PoligonalVertexes(int bulgePrecision, double weldThreshold, double bulgeThreshold)
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
                            double phi = Vector2.AngleBetween(p1, p2) + Math.Sign(bulge) * gamma;
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