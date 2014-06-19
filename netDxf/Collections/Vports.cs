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
    /// Represents a collection of vports.
    /// </summary>
    public sealed class VPorts :
        TableObjects<VPort>
    {

        #region constructor

        internal VPorts(DxfDocument document, string handle = null)
            : base(document,
            new Dictionary<string, VPort>(StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, List<DxfObject>>(StringComparer.OrdinalIgnoreCase),
            StringCode.VportTable,
            handle)
        {
        }

        #endregion

        #region override methods

        /// <summary>
        /// Adds an vport to the list.
        /// </summary>
        /// <param name="vport"><see cref="VPort">VPort</see> to add to the list.</param>
        /// <returns>
        /// If a vport already exists with the same name as the instance that is being added the method returns the existing vport,
        /// if not it will return the new vport.
        /// </returns>
        internal override VPort Add(VPort vport, bool assignHandle)
        {
            VPort add;
            if (this.list.TryGetValue(vport.Name, out add))
                return add;

            if (assignHandle)
                this.document.NumHandles = vport.AsignHandle(this.document.NumHandles);

            this.document.AddedObjects.Add(vport.Handle, vport);
            this.list.Add(vport.Name, vport);
            this.references.Add(vport.Name, new List<DxfObject>());
            vport.Owner = this;
            return vport;
        }

        /// <summary>
        /// Removes a vport.
        /// </summary>
        /// <param name="name"><see cref="VPort">VPort</see> name to remove from the document.</param>
        /// <returns>True is the vport has been successfully removed, or false otherwise.</returns>
        /// <remarks>Reserved vports or any other referenced by objects cannot be removed.</remarks>
        public override bool Remove(string name)
        {
            return Remove(this[name]);
        }

        /// <summary>
        /// Removes a vport.
        /// </summary>
        /// <param name="vport"><see cref="VPort">VPort</see> to remove from the document.</param>
        /// <returns>True is the vport has been successfully removed, or false otherwise.</returns>
        /// <remarks>Reserved vports or any other referenced by objects cannot be removed.</remarks>
        public override bool Remove(VPort vport)
        {
            if (vport == null)
                return false;

            if (!this.Contains(vport))
                return false;

            if (vport.IsReserved)
                return false;

            if (this.references[vport.Name].Count != 0)
                return false;

            vport.Owner = null;
            this.document.AddedObjects.Remove(vport.Handle);
            this.references.Remove(vport.Name);
            this.list.Remove(vport.Name);

            return true;
        }

        #endregion

    }
}