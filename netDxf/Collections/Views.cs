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
    /// Represents a collection of views.
    /// </summary>
    public sealed class Views :
        TableObjects<View>
    {

        #region constructor

        internal Views(DxfDocument document, string handle = null)
            : base(document,
            new Dictionary<string, View>(StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, List<DxfObject>>(StringComparer.OrdinalIgnoreCase),
            StringCode.ViewTable,
            handle)
        {
        }

        #endregion

        #region override methods

        /// <summary>
        /// Adds a view to the list.
        /// </summary>
        /// <param name="view"><see cref="View">View</see> to add to the list.</param>
        /// <returns>
        /// If a view already exists with the same name as the instance that is being added the method returns the existing view,
        /// if not it will return the new view.
        /// </returns>
        internal override View Add(View view, bool assignHandle)
        {
            View add;
            if (this.list.TryGetValue(view.Name, out add))
                return add;

            if (assignHandle)
                this.document.NumHandles = view.AsignHandle(this.document.NumHandles);

            this.document.AddedObjects.Add(view.Handle, view);
            this.list.Add(view.Name, view);
            this.references.Add(view.Name, new List<DxfObject>());
            view.Owner = this;
            return view;
        }

        /// <summary>
        /// Removes view.
        /// </summary>
        /// <param name="name"><see cref="View">View</see> name to remove from the document.</param>
        /// <returns>True is the view has been successfully removed, or false otherwise.</returns>
        /// <remarks>Reserved views or any other referenced by objects cannot be removed.</remarks>
        public override bool Remove(string name)
        {
            return Remove(this[name]);
        }

        /// <summary>
        /// Removes a view.
        /// </summary>
        /// <param name="view"><see cref="View">View</see> to remove from the document.</param>
        /// <returns>True is the view has been successfully removed, or false otherwise.</returns>
        /// <remarks>Reserved views or any other referenced by objects cannot be removed.</remarks>
        public override bool Remove(View view)
        {
            if (view == null)
                return false;

            if (!this.Contains(view))
                return false;

            if (view.IsReserved)
                return false;

            if (this.references[view.Name].Count != 0)
                return false;

            view.Owner = null;
            this.document.AddedObjects.Remove(view.Handle);
            this.references.Remove(view.Name);
            this.list.Remove(view.Name);

            return true;
        }

        #endregion

    }
}