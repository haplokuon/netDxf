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
using System.Collections.ObjectModel;

namespace netDxf.Entities
{
    /// <summary>
    /// Defines the boundary path type of the hatch.
    /// </summary>
    /// <remarks>Bit flag.</remarks>
    [Flags]
    public enum BoundaryPathTypeFlag
    {
        Default = 0,
        External = 1,
        Polyline = 2,
        Derived = 4,
        Textbox = 8,
        Outermost = 16
    }

    /// <summary>
    /// Represent a loop of a hatch boundary path.
    /// The entities that make a loop can be any combination of lines, polylines, circles, arcs and ellipses.
    /// </summary>
    /// <remarks>
    /// The entities that define a loop must define a closed path and they have to be on the same plane as the hatch, 
    /// if these conditions are not met the result will be unpredictable.
    /// The normal and the elevation will be omited (the hatch elevation and normal will be used instead).
    /// Only the x and y coordinates for the center of the line, ellipse and the circle will be used.
    /// Circles, full ellipses and closed polylines are closed paths so only one must exist in the data list.
    /// Lines, arcs, ellipse arcs and open polylines are open paths so more enties must exist to make a closed loop.
    /// </remarks>
    public class HatchBoundaryPath
    {
        #region private fields

        private readonly List<IEntityObject> data;
        private BoundaryPathTypeFlag pathTypeFlag;
        private int numberOfEdges;
        private bool isPolyline;

        #endregion

        #region constructor
        /// <summary>
        /// Initializes a new instance of the <c>Hatch</c> class.
        /// </summary>
        /// <param name="data">list of entities that makes a loop for the hatch boundary paths.</param>
        public HatchBoundaryPath(List<IEntityObject> data)
        {
            if (data == null)
                    throw new ArgumentNullException("data");
            this.data = data;
            SetInternalInfo();
        }
        #endregion

        #region public properties
        /// <summary>
        /// Gets the list of entities that makes a loop for the hatch boundary paths.
        /// </summary>
        public ReadOnlyCollection<IEntityObject> Data
        {
            get { return data.AsReadOnly(); }
        }

        /// <summary>
        /// Gets the boundary path type flag.
        /// </summary>
        public BoundaryPathTypeFlag PathTypeFlag
        {
            get { return pathTypeFlag; }
        }

        /// <summary>
        /// Gets the number of edges that make up the boundary path.
        /// </summary>
        public int NumberOfEdges
        {
            get { return numberOfEdges; }
        }

        /// <summary>
        /// Defines if the boundary is a closed polyline.
        /// </summary>
        public bool IsPolyline
        {
            get { return isPolyline; }
        }

        #endregion

        #region private methods
        private void SetInternalInfo()
        {
            numberOfEdges = 0;
            foreach (IEntityObject entity in data)
            {
                switch (entity.Type)
                {
                    case EntityType.Arc:
                        if (data.Count <= 1) throw new ArgumentException("Only an arc does not make closed loop.");
                        pathTypeFlag = BoundaryPathTypeFlag.Derived | BoundaryPathTypeFlag.External;
                        numberOfEdges += 1;
                        isPolyline = false;
                        break;
                    case EntityType.Circle:
                        // a circle is a closed loop
                        if (data.Count>1) throw new ArgumentException("A circle is a closed loop, there can be only per path.");
                        pathTypeFlag = BoundaryPathTypeFlag.Derived | BoundaryPathTypeFlag.External;
                        numberOfEdges += 1;
                        isPolyline = false;
                        break;
                    case EntityType.Ellipse:
                        // a full ellipse is a closed loop
                        if (((Ellipse)entity).IsFullEllipse && data.Count > 1) throw new ArgumentException("A full ellipse is a closed loop, there can be only per path.");
                        pathTypeFlag = BoundaryPathTypeFlag.Derived | BoundaryPathTypeFlag.External;
                        numberOfEdges += 1;
                        isPolyline = false;
                        break;
                    case EntityType.Line:
                        if (data.Count <= 1) throw new ArgumentException("Only a line does not make closed loop.");
                        pathTypeFlag = BoundaryPathTypeFlag.Derived | BoundaryPathTypeFlag.External;
                        numberOfEdges += 1;
                        isPolyline = false;
                        break;
                    case EntityType.Polyline:
                        
                        if (((Polyline)entity).IsClosed)
                        {
                            if (data.Count > 1) throw new ArgumentException("A closed polyline is a closed loop, there can be only per path.");
                            // for a closed polyline the number of edges is equal the number of vertexes
                            pathTypeFlag = BoundaryPathTypeFlag.Derived | BoundaryPathTypeFlag.External | BoundaryPathTypeFlag.Polyline;
                            numberOfEdges += ((Polyline)entity).Vertexes.Count;
                            isPolyline = true;
                        }
                        else
                        {
                            throw new NotImplementedException("A polyline path can not be combined with any other type of entity." + "To combine polylines with arcs or ellipses first the polylines must be decomposed in its internal components: lines and arcs.");
                        }
                        break;
                }
            }

        }
        #endregion

    }
}