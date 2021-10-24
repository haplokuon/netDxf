#region netDxf library licensed under the MIT License
// 
//                       netDxf library
// Copyright (c) 2019-2021 Daniel Carvajal (haplokuon@gmail.com)
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
using netDxf.Tables;

namespace netDxf.Collections
{
    /// <summary>
    /// Represents a collection of viewports.
    /// </summary>
    /// <remarks>
    /// Multiple Model viewports are not supported, there can be only one called "*Active".
    /// </remarks>
    public sealed class VPorts :
        TableObjects<VPort>
    {
        #region constructor

        internal VPorts(DxfDocument document)
            : this(document, null)
        {
        }

        internal VPorts(DxfDocument document, string handle)
            : base(document, DxfObjectCode.VportTable, handle)
        {
            // add the current document viewport, it is always present
            VPort active = VPort.Active;
            this.Owner.NumHandles = active.AssignHandle(this.Owner.NumHandles);

            this.Owner.AddedObjects.Add(active.Handle, active);
            this.list.Add(active.Name, active);
            this.references.Add(active.Name, new List<DxfObject>());
            active.Owner = this;
        }

        #endregion

        #region override methods

        /// <summary>
        /// Adds an viewports to the list.
        /// </summary>
        /// <param name="vport"><see cref="VPort">VPort</see> to add to the list.</param>
        /// <param name="assignHandle">Specifies if a handle needs to be generated for the viewport parameter.</param>
        /// <returns>
        /// If a viewports already exists with the same name as the instance that is being added the method returns the existing viewports,
        /// if not it will return the new viewports.
        /// </returns>
        internal override VPort Add(VPort vport, bool assignHandle)
        {
            throw new ArgumentException("VPorts cannot be added to the collection. There is only one VPort in the list the \"*Active\".", nameof(vport));
        }

        /// <summary>
        /// Removes a viewports.
        /// </summary>
        /// <param name="name"><see cref="VPort">VPort</see> name to remove from the document.</param>
        /// <returns>True if the viewports has been successfully removed, or false otherwise.</returns>
        /// <remarks>Reserved viewports or any other referenced by objects cannot be removed.</remarks>
        public override bool Remove(string name)
        {
            throw new ArgumentException("VPorts cannot be removed from the collection.", nameof(name));
        }

        /// <summary>
        /// Removes a viewports.
        /// </summary>
        /// <param name="item"><see cref="VPort">VPort</see> to remove from the document.</param>
        /// <returns>True if the viewports has been successfully removed, or false otherwise.</returns>
        /// <remarks>Reserved viewports or any other referenced by objects cannot be removed.</remarks>
        public override bool Remove(VPort item)
        {
            throw new ArgumentException("VPorts cannot be removed from the collection.", nameof(item));
        }

        #endregion
    }
}