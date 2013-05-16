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
        /// <param name="boundaryPaths">A list of <see cref="HatchBoundaryPath">boundary paths</see>.</param>
        public Hatch(HatchPattern pattern, List<HatchBoundaryPath> boundaryPaths)
            : base(EntityType.Hatch, DxfObjectCode.Hatch)
        {
            this.pattern = pattern;
            this.boundaryPaths = boundaryPaths;
            if (pattern.GetType() == typeof (HatchGradientPattern))
                ((HatchGradientPattern) pattern).GradientColorAciXData(this.XData);
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the hatch pattern name.
        /// </summary>
        public HatchPattern Pattern
        {
            get { return pattern; }
            set
            {
                if (value.GetType() == typeof(HatchGradientPattern))
                    ((HatchGradientPattern)value).GradientColorAciXData(this.XData);
                pattern = value;
            }
        }

        /// <summary>
        /// Gets or sets the hatch boundary paths.
        /// </summary>
        /// <remarks>
        /// If the hatch is associative the boundary paths will be also added to the document.
        /// </remarks>
        public List<HatchBoundaryPath> BoundaryPaths
        {
            get { return boundaryPaths; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                boundaryPaths = value;
            }
        }

        /// <summary>
        /// Gets or sets the hatch elevation.
        /// </summary>
        public double Elevation
        {
            get { return elevation; }
            set { elevation = value; }
        }

        #endregion

    }
}
