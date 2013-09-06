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

using System.Collections.Generic;
using netDxf.Tables;

namespace netDxf.Collections
{
    /// <summary>
    /// Represents a collection of user coordinate systems.
    /// </summary>
    /// <remarks>The UCSs collection method GetReferences will always return an empty list since there are no DxfObjects that references them.</remarks>
    public sealed class UCSs :
        TableObjects<UCS>
    {

        #region constructor

        internal UCSs(DxfDocument document)
            : base(document)
        {
        }

        internal UCSs(DxfDocument document, Dictionary<string, UCS> list, Dictionary<string, List<DxfObject>> references)
            : base( document, list, references)
        {
        }

        #endregion

        #region override methods

        /// <summary>
        /// Adds a user coordinate system to the list.
        /// </summary>
        /// <param name="ucs"><see cref="UCS">User coordinate system</see> to add to the list.</param>
        /// <returns>
        /// If a user coordinate system already exists with the same name as the instance that is being added the method returns the existing user coordinate system,
        /// if not it will return the new user coordinate system.
        /// </returns>
        public override UCS Add(UCS ucs)
        {
            UCS add;
            if (this.list.TryGetValue(ucs.Name, out add))
                return add;

            this.document.NumHandles = ucs.AsignHandle(this.document.NumHandles);
            this.list.Add(ucs.Name, ucs);
            this.references.Add(ucs.Name, new List<DxfObject>());

            return ucs;
        }

        /// <summary>
        /// Removes a user coordinate system.
        /// </summary>
        /// <param name="name"><see cref="UCS">User coordinate system</see> name to remove from the document.</param>
        /// <returns>True is the user coordinate system has been successfully removed, or false otherwise.</returns>
        /// <remarks>Reserved user coordinate system or any other referenced by objects cannot be removed.</remarks>
        public override bool Remove(string name)
        {
            UCS ucs = this[name];

            if (ucs == null)
                return false;

            if (ucs.IsReserved)
                return false;

            if (this.references[ucs.Name].Count != 0)
                return false;

            this.references.Remove(ucs.Name);
            return this.list.Remove(ucs.Name);
        }

        /// <summary>
        /// Removes a user coordinate system.
        /// </summary>
        /// <param name="ucs"><see cref="UCS">User coordinate system</see> to remove from the document.</param>
        /// <returns>True is the user coordinate system has been successfully removed, or false otherwise.</returns>
        /// <remarks>Reserved user coordinate system or any other referenced by objects cannot be removed.</remarks>
        public override bool Remove(UCS ucs)
        {
            return Remove(ucs.Name);
        }

        #endregion

    }
}