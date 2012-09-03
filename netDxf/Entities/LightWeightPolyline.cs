#region netDxf, Copyright(C) 2009 Daniel Carvajal, Licensed under LGPL.

//                        netDxf library
// Copyright (C) 2009 Daniel Carvajal (haplokuon@gmail.com)
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
    /// Represents a polyline <see cref="netDxf.Entities.IEntityObject">entity</see>.
    /// </summary>
    /// <remarks>
    /// The <see cref="netDxf.Entities.LightWeightPolyline">LightWeightPolyline</see> and
    /// the <see cref="netDxf.Entities.Polyline">Polyline</see> are essentially the same entity, they are both here for compatibility reasons.
    /// When a AutoCad12 file is saved all lightweight polylines will be converted to polylines, while for AutoCad2000 and later versions all
    /// polylines will be converted to lightweight polylines.
    /// </remarks>
    public class LightWeightPolyline :
        DxfObject,
        IPolyline
    {
        #region private fields

        private const EntityType TYPE = EntityType.LightWeightPolyline;
        private List<LightWeightPolylineVertex> vertexes;
        private bool isClosed;
        private PolylineTypeFlags flags;
        private Layer layer;
        private AciColor color;
        private LineType lineType;
        private Vector3d normal;
        private double elevation;
        private double thickness;
        private Dictionary<ApplicationRegistry, XData> xData;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>Polyline</c> class.
        /// </summary>
        /// <param name="vertexes">Polyline <see cref="LightWeightPolylineVertex">vertex</see> list in object coordinates.</param>
        /// <param name="isClosed">Sets if the polyline is closed</param>
        public LightWeightPolyline(List<LightWeightPolylineVertex> vertexes, bool isClosed)
            : base(DxfObjectCode.LightWeightPolyline)
        {
            this.vertexes = vertexes;
            this.isClosed = isClosed;
            this.layer = Layer.Default;
            this.color = AciColor.ByLayer;
            this.lineType = LineType.ByLayer;
            this.normal = Vector3d.UnitZ;
            this.elevation = 0.0;
            this.thickness = 0.0;
            this.flags = isClosed ? PolylineTypeFlags.ClosedPolylineOrClosedPolygonMeshInM : PolylineTypeFlags.OpenPolyline;
        }

        /// <summary>
        /// Initializes a new instance of the <c>Polyline</c> class.
        /// </summary>
        /// <param name="vertexes">Polyline <see cref="LightWeightPolylineVertex">vertex</see> list in object coordinates.</param>
        public LightWeightPolyline(List<LightWeightPolylineVertex> vertexes)
            : base(DxfObjectCode.LightWeightPolyline)
        {
            this.vertexes = vertexes;
            this.isClosed = false;
            this.layer = Layer.Default;
            this.color = AciColor.ByLayer;
            this.lineType = LineType.ByLayer;
            this.normal = Vector3d.UnitZ;
            this.elevation = 0.0;
            this.thickness = 0.0;
            this.flags = PolylineTypeFlags.OpenPolyline;
        }

        /// <summary>
        /// Initializes a new instance of the <c>Polyline</c> class.
        /// </summary>
        public LightWeightPolyline()
            : base(DxfObjectCode.LightWeightPolyline)
        {
            this.vertexes = new List<LightWeightPolylineVertex>();
            this.isClosed = false;
            this.layer = Layer.Default;
            this.color = AciColor.ByLayer;
            this.lineType = LineType.ByLayer;
            this.normal = Vector3d.UnitZ;
            this.elevation = 0.0;
            this.thickness = 0.0;
            this.flags = PolylineTypeFlags.OpenPolyline;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the polyline <see cref="netDxf.Entities.PolylineVertex">vertex</see> list.
        /// </summary>
        public List<LightWeightPolylineVertex> Vertexes
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
        public virtual bool IsClosed
        {
            get { return this.isClosed; }
            set
            {
                this.flags |= value ? PolylineTypeFlags.ClosedPolylineOrClosedPolygonMeshInM : PolylineTypeFlags.OpenPolyline;
                this.isClosed = value;
            }
        }

        /// <summary>
        /// Gets or sets the polyline <see cref="netDxf.Vector3d">normal</see>.
        /// </summary>
        public Vector3d Normal
        {
            get { return this.normal; }
            set
            {
                if (Vector3d.Zero == value)
                    throw new ArgumentNullException("value", "The normal can not be the zero vector");
                value.Normalize();
                this.normal = value;
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
        public double Elevation
        {
            get { return this.elevation; }
            set { this.elevation = value; }
        }

        #endregion

        #region IPolyline Members

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

        #region public methods

        /// <summary>
        /// Sets a constant width for all the polyline segments.
        /// </summary>
        /// <param name="width">Polyline width.</param>
        public void SetConstantWidth(double width)
        {
            foreach (LightWeightPolylineVertex v in this.vertexes)
            {
                v.BeginThickness = width;
                v.EndThickness = width;
            }
        }

        /// <summary>
        /// Converts the lightweight polyline in a <see cref="Polyline">Polyline</see>.
        /// </summary>
        /// <returns>A new instance of <see cref="Polyline">Polyline</see> that represents the lightweight polyline.</returns>
        public Polyline ToPolyline()
        {
            List<PolylineVertex> polyVertexes = new List<PolylineVertex>();
            
            foreach (LightWeightPolylineVertex v in this.vertexes)
            {
                polyVertexes.Add(new PolylineVertex(v.Location)
                                     {
                                         BeginThickness = v.BeginThickness,
                                         Bulge = v.Bulge,
                                         Color = this.Color,
                                         EndThickness = v.EndThickness,
                                         Layer = this.Layer,
                                         LineType = this.LineType,
                                     }
                    );
            }

            return new Polyline(polyVertexes, this.isClosed)
                       {
                           Color = this.color,
                           Layer = this.layer,
                           LineType = this.lineType,
                           Normal = this.normal,
                           Elevation = this.elevation,
                           Thickness = this.thickness,
                           XData = this.xData
                       };
        }


        /// <summary>
        /// Obtains a list of vertexes that represent the polyline approximating the curve segments as necessary.
        /// </summary>
        /// <param name="bulgePrecision">Curve segments precision (a value of zero means that no approximation will be made).</param>
        /// <param name="weldThreshold">Tolerance to consider if two new generated vertexes are equal.</param>
        /// <param name="bulgeThreshold">Minimun distance from which approximate curved segments of the polyline.</param>
        /// <returns>The return vertexes are expresed in object coordinate system.</returns>
        public List<Vector2d> PoligonalVertexes(int bulgePrecision, double weldThreshold, double bulgeThreshold)
        {
            List<Vector2d> ocsVertexes = new List<Vector2d>();

            int index = 0;

            foreach (LightWeightPolylineVertex vertex in this.Vertexes)
            {
                double bulge = vertex.Bulge;
                Vector2d p1;
                Vector2d p2;

                if (index == this.Vertexes.Count - 1)
                {
                    p1 = new Vector2d(vertex.Location.X, vertex.Location.Y);
                    p2 = new Vector2d(this.vertexes[0].Location.X, this.vertexes[0].Location.Y);
                }
                else
                {
                    p1 = new Vector2d(vertex.Location.X, vertex.Location.Y);
                    p2 = new Vector2d(this.vertexes[index + 1].Location.X, this.vertexes[index + 1].Location.Y);
                }

                if (!p1.Equals(p2, weldThreshold))
                {
                    if (bulge == 0 || bulgePrecision == 0)
                    {
                        ocsVertexes.Add(p1);
                    }
                    else
                    {
                        double c = Vector2d.Distance(p1, p2);
                        if (c >= bulgeThreshold)
                        {
                            double s = (c / 2) * Math.Abs(bulge);
                            double r = ((c / 2) * (c / 2) + s * s) / (2 * s);
                            double theta = 4 * Math.Atan(Math.Abs(bulge));
                            double gamma = (Math.PI - theta) / 2;
                            double phi;

                            if (bulge > 0)
                            {
                                phi = Vector2d.AngleBetween(Vector2d.UnitX, p2 - p1) + gamma;
                            }
                            else
                            {
                                phi = Vector2d.AngleBetween(Vector2d.UnitX, p2 - p1) - gamma;
                            }

                            Vector2d center = new Vector2d(p1.X + r*Math.Cos(phi), p1.Y + r*Math.Sin(phi));
                            Vector2d a1 = p1 - center;
                            double angle = 4 * (Math.Atan(bulge)) / (bulgePrecision + 1);

                            ocsVertexes.Add(p1);
                            for (int i = 1; i <= bulgePrecision; i++)
                            {
                                Vector2d curvePoint = new Vector2d();
                                Vector2d prevCurvePoint = new Vector2d(this.vertexes[this.vertexes.Count - 1].Location.X, this.vertexes[this.vertexes.Count - 1].Location.Y);
                                curvePoint.X = center.X + Math.Cos(i*angle)*a1.X - Math.Sin(i*angle)*a1.Y;
                                curvePoint.Y = center.Y + Math.Sin(i*angle)*a1.X + Math.Cos(i*angle)*a1.Y;

                                if (!curvePoint.Equals(prevCurvePoint, weldThreshold) &&
                                    !curvePoint.Equals(p2, weldThreshold))
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