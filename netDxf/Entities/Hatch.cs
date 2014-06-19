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
    /// Represents a hatch <see cref="EntityObject">entity</see>.
    /// </summary>
    public class Hatch :
        EntityObject
    {
        #region private fields

        private List<HatchBoundaryPath> boundaryPaths;
        private HatchPattern pattern;
        private double elevation;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>Hatch</c> class.
        /// </summary>
        /// <remarks>
        /// The hatch boundary paths must be on the same plane as the hatch.
        /// The normal and the elevation of the boundary paths will be omited (the hatch elevation and normal will be used instead).
        /// Only the x and y coordinates for the center of the line, ellipse, circle and arc will be used.
        /// </remarks>
        /// <param name="pattern"><see cref="HatchPattern">Hatch pattern</see>.</param>
        /// <param name="paths">A list of <see cref="HatchBoundaryPath">boundary paths</see>.</param>
        public Hatch(HatchPattern pattern, List<HatchBoundaryPath> paths)
            : base(EntityType.Hatch, DxfObjectCode.Hatch)
        {
            if (pattern == null)
                throw new ArgumentNullException("pattern");
            if (paths == null)
                throw new ArgumentNullException("paths");
            if(paths.Count == 0)
                throw new ArgumentException("The hatch boundary must contain at least one valid path.", "paths");
            this.boundaryPaths = paths;
            this.pattern = pattern;
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
                if (value == null)
                    throw new ArgumentNullException("value");
                this.pattern = value;
            }
        }

        /// <summary>
        /// Gets or sets the hatch boundary paths.
        /// </summary>
        /// <remarks>
        /// The hatch must contain at least on valid boundary path to be able to add it to the DxfDocument, otherwise it will be rejected.
        /// </remarks>
        public List<HatchBoundaryPath> BoundaryPaths
        {
            get { return this.boundaryPaths; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                if (value.Count == 0)
                    throw new ArgumentException("The hatch boundary must contain at least one valid path.", "value");
                this.boundaryPaths = value;
            }
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
        /// Creates a list of entities that represents the boundary of the hatch in world coordinates.
        /// </summary>
        /// <returns>A list of entities that makes the boundary of the hatch in world coordinates.</returns>
        /// <remarks>
        /// The generated list can be used to directly draw the hatch boundary given the normal and elevation of the hatch.
        /// All entities are in world coordinates except the LwPolyline boundary path since by definition its vertexes are expressed in object coordinates.
        /// This list differs in that the hatch entities list are in local coordinates of the hatch
        /// while with this method the entities are transformed by the normal and elevetation of it.
        /// </remarks>
        public List<EntityObject> CreateWCSBoundary()
        {
            List<EntityObject> wcsBoundary = new List<EntityObject>();
            Matrix3 trans = MathHelper.ArbitraryAxis(this.normal);
            Vector3 pos = trans*new Vector3(0.0, 0.0, this.elevation);
            foreach (HatchBoundaryPath path in this.boundaryPaths)
            {
                foreach (HatchBoundaryPath.Edge edge in path.Edges)
                {
                    EntityObject entity = edge.ConvertTo();
                    switch (entity.Type)
                    {
                        case (EntityType.Arc):
                            wcsBoundary.Add(ProcessArc((Arc) entity, trans, pos));
                            break;
                        case (EntityType.Circle):
                            wcsBoundary.Add(ProcessCircle((Circle) entity, trans, pos));
                            break;
                        case (EntityType.Ellipse):
                            wcsBoundary.Add(ProcessEllipse((Ellipse) entity, trans, pos));
                            break;
                        case (EntityType.Line):
                            wcsBoundary.Add(ProcessLine((Line) entity, trans, pos));
                            break;
                        case (EntityType.LightWeightPolyline):
                            // LwPolylines need an special threatement since their vertexes are expressed in object coordinates.
                            wcsBoundary.Add(ProcessLwPolyline((LwPolyline) entity, this.normal, this.elevation));
                            break;
                        case (EntityType.Spline):
                            wcsBoundary.Add(ProcessSpline((Spline) entity, trans, pos));
                            break;
                    }
                }
            }
            return wcsBoundary;
        }

        #endregion

        #region private methods

        private static EntityObject ProcessArc(Arc arc, Matrix3 trans, Vector3 pos)
        {
            arc.Center = trans*arc.Center + pos;
            arc.Normal = trans*arc.Normal;
            return arc;
        }

        private static EntityObject ProcessCircle(Circle circle, Matrix3 trans, Vector3 pos)
        {
            circle.Center = trans*circle.Center + pos;
            circle.Normal = trans*circle.Normal;
            return circle;
        }

        private static Ellipse ProcessEllipse(Ellipse ellipse, Matrix3 trans, Vector3 pos)
        {
            ellipse.Center = trans*ellipse.Center + pos;
            ellipse.Normal = trans*ellipse.Normal;
            return ellipse;
        }

        private static Line ProcessLine(Line line, Matrix3 trans, Vector3 pos)
        {
            line.StartPoint = trans*line.StartPoint + pos;
            line.EndPoint = trans*line.EndPoint + pos;
            line.Normal = trans*line.Normal;
            return line;
        }

        private static LwPolyline ProcessLwPolyline(LwPolyline polyline, Vector3 normal, double elevation)
        {
            polyline.Elevation = elevation;
            polyline.Normal = normal;
            return polyline;
        }

        private static Spline ProcessSpline(Spline spline, Matrix3 trans, Vector3 pos)
        {
            foreach (SplineVertex vertex in spline.ControlPoints)
                vertex.Location = trans*vertex.Location + pos;

            spline.Normal = trans*spline.Normal;
            return spline;
        }

        #endregion

        #region overrides

        /// <summary>
        /// Creates a new Hatch that is a copy of the current instance.
        /// </summary>
        /// <returns>A new Hatch that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new Hatch(this.pattern, this.boundaryPaths)
                {
                    //EntityObject properties
                    Color = this.color,
                    Layer = this.layer,
                    LineType = this.lineType,
                    Lineweight = this.lineweight,
                    LineTypeScale = this.lineTypeScale,
                    Normal = this.normal,
                    XData = this.xData,
                    //Hatch properties
                    Elevation = this.elevation
                };
        }

        #endregion

    }
}