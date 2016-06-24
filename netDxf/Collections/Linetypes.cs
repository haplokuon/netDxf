#region netDxf library, Copyright (C) 2009-2016 Daniel Carvajal (haplokuon@gmail.com)

//                        netDxf library
// Copyright (C) 2009-2016 Daniel Carvajal (haplokuon@gmail.com)
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
    /// Represents a collection of line types.
    /// </summary>
    public sealed class Linetypes :
        TableObjects<Linetype>
    {
        #region constructor

        internal Linetypes(DxfDocument document, string handle = null)
            : this(document, 0, handle)
        {
        }

        internal Linetypes(DxfDocument document, int capacity, string handle = null)
            : base(document,
                new Dictionary<string, Linetype>(capacity, StringComparer.OrdinalIgnoreCase),
                new Dictionary<string, List<DxfObject>>(capacity, StringComparer.OrdinalIgnoreCase),
                DxfObjectCode.LinetypeTable,
                handle)
        {
            this.MaxCapacity = short.MaxValue;
        }

        #endregion

        #region override methods

        /// <summary>
        /// Adds a line type to the list.
        /// </summary>
        /// <param name="linetype"><see cref="Linetype">Linetype</see> to add to the list.</param>
        /// <param name="assignHandle">Specifies if a handle needs to be generated for the line type parameter.</param>
        /// <returns>
        /// If a line type already exists with the same name as the instance that is being added the method returns the existing line type,
        /// if not it will return the new line type.
        /// </returns>
        internal override Linetype Add(Linetype linetype, bool assignHandle)
        {
            if (this.list.Count >= this.MaxCapacity)
                throw new OverflowException(string.Format("Table overflow. The maximum number of elements the table {0} can have is {1}", this.CodeName, this.MaxCapacity));
            if (linetype == null)
                throw new ArgumentNullException(nameof(linetype));

            Linetype add;

            if (this.list.TryGetValue(linetype.Name, out add))
                return add;

            if (assignHandle || string.IsNullOrEmpty(linetype.Handle))
                this.Owner.NumHandles = linetype.AsignHandle(this.Owner.NumHandles);

            this.list.Add(linetype.Name, linetype);
            this.Owner.AddedObjects.Add(linetype.Handle, linetype);
            this.references.Add(linetype.Name, new List<DxfObject>());

            linetype.Owner = this;

            linetype.NameChanged += this.Item_NameChanged;

            return linetype;
        }

        /// <summary>
        /// Removes a line type.
        /// </summary>
        /// <param name="name"><see cref="Linetype">Linetype</see> name to remove from the document.</param>
        /// <returns>True if the line type has been successfully removed, or false otherwise.</returns>
        /// <remarks>Reserved line types or any other referenced by objects cannot be removed.</remarks>
        public override bool Remove(string name)
        {
            return this.Remove(this[name]);
        }

        /// <summary>
        /// Removes a line type.
        /// </summary>
        /// <param name="item"><see cref="Linetype">Linetype</see> to remove from the document.</param>
        /// <returns>True if the line type has been successfully removed, or false otherwise.</returns>
        /// <remarks>Reserved line types or any other referenced by objects cannot be removed.</remarks>
        public override bool Remove(Linetype item)
        {
            if (item == null)
                return false;

            if (!this.Contains(item))
                return false;

            if (item.IsReserved)
                return false;

            if (this.references[item.Name].Count != 0)
                return false;

            this.Owner.AddedObjects.Remove(item.Handle);
            this.references.Remove(item.Name);
            this.list.Remove(item.Name);

            item.Handle = null;
            item.Owner = null;

            item.NameChanged -= this.Item_NameChanged;

            return true;
        }

        #endregion

        #region Linetype events

        private void Item_NameChanged(TableObject sender, TableObjectChangedEventArgs<string> e)
        {
            if (this.Contains(e.NewValue))
                throw new ArgumentException("There is already another line type with the same name.");

            this.list.Remove(sender.Name);
            this.list.Add(e.NewValue, (Linetype) sender);

            List<DxfObject> refs = this.references[sender.Name];
            this.references.Remove(sender.Name);
            this.references.Add(e.NewValue, refs);
        }

        #endregion
    }
}