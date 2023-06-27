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
using System.Linq;
using netDxf.Tables;

namespace netDxf.Entities
{
    /// <summary>
    /// Represents a polyface mesh face. 
    /// </summary>
    /// <remarks>
    /// The way the vertex indexes for a polyface mesh are defined follows the DXF documentation.
    /// The values of the vertex indexes specify one of the previously defined vertexes by the index in the list plus one.
    /// If the index is negative, the edge that begins with that vertex is invisible.
    /// For example if the vertex index in the list is 0 the vertex index for the face will be 1, and
    /// if the edge between the vertexes 0 and 1 is hidden the vertex index for the face will be -1.<br/>
    /// The maximum number of vertexes per face is 4.
    /// </remarks>
    public class PolyfaceMeshFace :
        ICloneable
    {
        #region delegates and events

        public delegate void LayerChangedEventHandler(PolyfaceMeshFace sender, TableObjectChangedEventArgs<Layer> e);
        public event LayerChangedEventHandler LayerChanged;
        protected virtual Layer OnLayerChangedEvent(Layer oldLayer, Layer newLayer)
        {
            LayerChangedEventHandler ae = this.LayerChanged;
            if (ae != null)
            {
                TableObjectChangedEventArgs<Layer> eventArgs = new TableObjectChangedEventArgs<Layer>(oldLayer, newLayer);
                ae(this, eventArgs);
                return eventArgs.NewValue;
            }
            return newLayer;
        }

        #endregion

        #region private fields

        private readonly short[] vertexIndexes;
        private AciColor color;
        private Layer layer;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>PolyfaceMeshFace</c> class.
        /// </summary>
        /// <remarks>
        /// By default the face is made up of four vertexes.
        /// </remarks>
        public PolyfaceMeshFace()
            : this(new short[4])
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>PolyfaceMeshFace</c> class.
        /// </summary>
        /// <param name="vertexIndexes">Array of indexes to the vertex list of a polyface mesh that makes up the face.</param>
        public PolyfaceMeshFace(IEnumerable<short> vertexIndexes)
        {
            if (vertexIndexes == null)
            {
                throw new ArgumentNullException(nameof(vertexIndexes));
            }
            this.vertexIndexes = vertexIndexes.ToArray();
            if (this.vertexIndexes.Length < 1 || this.vertexIndexes.Length > 4)
            {
                throw new ArgumentOutOfRangeException(nameof(vertexIndexes), "The number of indexes per faces must be greater than 0, and a maximum of 4.");
            }

            this.color = null;
            this.layer = null;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the list of indexes to the vertex list of a polyface mesh that makes up the face.
        /// </summary>
        public short[] VertexIndexes
        {
            get { return this.vertexIndexes; }
        }

        /// <summary>
        /// Gets or sets the face color. Set to null to inherit from its parent polyface mesh.
        /// </summary>
        public AciColor Color
        {
            get { return this.color; }
            set { this.color = value; }
        }

        /// <summary>
        /// Gets or sets the face layer. Set to null to inherit from its parent polyface mesh.
        /// </summary>
        public Layer Layer
        {
            get { return this.layer; }
            set { this.layer = this.OnLayerChangedEvent(this.layer, value); }
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

        /// <summary>
        /// Creates a new PolyfaceMeshFace that is a copy of the current instance.
        /// </summary>
        /// <returns>A new PolyfaceMeshFace that is a copy of this instance.</returns>
        public object Clone()
        {
            return new PolyfaceMeshFace(this.vertexIndexes)
            {
                Layer = (Layer) this.layer.Clone(),
                Color = (AciColor) this.color.Clone()
                
            };
        }

        #endregion
    }
}