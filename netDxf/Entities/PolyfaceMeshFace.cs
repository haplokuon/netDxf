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
    /// Represents a polyface mesh face. 
    /// </summary>
    /// <remarks>
    /// The way the vertex indexes for a polyfacemesh are defined follows the dxf documentation.
    /// The values of the vertex indexes specify one of the previously defined vertexes by the index in the list plus one.
    /// If the index is negative, the edge that begins with that vertex is invisible.
    /// For example if the vertex index in the list is 0 the vertex index for the face will be 1, and
    /// if the edge between the vertexes 0 and 1 is hidden the vertex index for the face will be -1.
    /// The maximum number of vertex indexes in a face is 4.
    /// </remarks>
    public class PolyfaceMeshFace :
        DxfObject
    {
        #region private fields
        private VertexTypeFlags flags;
        private int[] vertexIndexes;
        private Dictionary<ApplicationRegistry, XData> xData;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>PolyfaceMeshFace</c> class.
        /// </summary>
        /// <remarks>
        /// By default the face is made up of three vertexes.
        /// </remarks>
        public PolyfaceMeshFace()
            : this(new int[3])
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>PolyfaceMeshFace</c> class.
        /// </summary>
        /// <param name="vertexIndexes">Array of indexes to the vertex list of a polyface mesh that makes up the face.</param>
        public PolyfaceMeshFace(int[] vertexIndexes)
            : base(DxfObjectCode.Vertex)
        {
            if (vertexIndexes == null)
                throw new ArgumentNullException("vertexIndexes");
            if (vertexIndexes.Length>4)
                throw new ArgumentOutOfRangeException("vertexIndexes", vertexIndexes.Length, "The maximun number of index vertexes in a face is 4");
            
            this.flags = VertexTypeFlags.PolyfaceMeshVertex;
            this.vertexIndexes = vertexIndexes;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the array of indexes to the vertex list of a polyface mesh that makes up the face.
        /// </summary>
        public int[] VertexIndexes
        {
            get { return this.vertexIndexes; }
            set
            {
                if (value == null) 
                    throw new ArgumentNullException("value");
                if (value.Length > 4)
                    throw new ArgumentOutOfRangeException("value", this.vertexIndexes.Length, "The maximun number of index vertexes in a face is 4");

                this.vertexIndexes = value;
            }
        }

        /// <summary>
        /// Gets the vertex type.
        /// </summary>
        internal VertexTypeFlags Flags
        {
            get { return this.flags; }
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

        #region overrides

        /// <summary>
        /// Converts the value of this instance to its equivalent string representation.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            return "PolyfaceMeshFace";
        }

        #endregion
    }
}