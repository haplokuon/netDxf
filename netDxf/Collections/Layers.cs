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
using netDxf.Tables;

namespace netDxf.Collections
{
    /// <summary>
    /// Represents a collection of layers.
    /// </summary>
    public sealed class Layers :
        TableObjects<Layer>
    {

        #region constructor

        internal Layers(DxfDocument document, string handle = null)
            : base(document,
            new Dictionary<string, Layer>(StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, List<DxfObject>>(StringComparer.OrdinalIgnoreCase),
            StringCode.LayerTable,
            handle)
        {
        }

        #endregion

        #region override methods

        /// <summary>
        /// Adds a layer to the list.
        /// </summary>
        /// <param name="layer"><see cref="Layer">Layer</see> to add to the list.</param>
        /// <returns>
        /// If a layer already exists with the same name as the instance that is being added the method returns the existing layer,
        /// if not it will return the new layer.
        /// </returns>
        internal override Layer Add(Layer layer, bool assignHandle)
        {
            Layer add;
            if (this.list.TryGetValue(layer.Name, out add))
                return add;

            if (assignHandle)
                this.document.NumHandles = layer.AsignHandle(this.document.NumHandles);

            this.document.AddedObjects.Add(layer.Handle, layer);
            this.list.Add(layer.Name, layer);
            this.references.Add(layer.Name, new List<DxfObject>());
            layer.LineType = this.document.LineTypes.Add(layer.LineType);
            this.document.LineTypes.References[layer.LineType.Name].Add(layer);
            layer.Owner = this;
            return layer;
        }

        /// <summary>
        /// Removes a layer.
        /// </summary>
        /// <param name="name"><see cref="Layer">Layer</see> name to remove from the document.</param>
        /// <returns>True is the layer has been successfully removed, or false otherwise.</returns>
        /// <remarks>Reserved layers or any other referenced by objects cannot be removed.</remarks>
        public override bool Remove(string name)
        {
            return Remove(this[name]);
        }

        /// <summary>
        /// Removes a layer.
        /// </summary>
        /// <param name="layer"><see cref="Layer">Layer</see> to remove from the document.</param>
        /// <returns>True is the layer has been successfully removed, or false otherwise.</returns>
        /// <remarks>Reserved layers or any other referenced by objects cannot be removed.</remarks>
        public override bool Remove(Layer layer)
        {
            if (layer == null)
                return false;

            if (!this.Contains(layer))
                return false;

            if (layer.IsReserved)
                return false;

            if (this.references[layer.Name].Count != 0)
                return false;

            layer.Owner = null;
            this.document.LineTypes.References[layer.LineType.Name].Remove(layer);
            this.document.AddedObjects.Remove(layer.Handle);
            this.references.Remove(layer.Name);
            this.list.Remove(layer.Name);

            return true;
        }

        #endregion

    }
}