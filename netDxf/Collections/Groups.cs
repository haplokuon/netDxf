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
using netDxf.Entities;
using netDxf.Objects;

namespace netDxf.Collections
{
    /// <summary>
    /// Represents a collection of groups.
    /// </summary>
    /// <remarks>The Groups collection method GetReferences will always return an empty list since there are no DxfObjects that references them.</remarks>
    public sealed class Groups :
        TableObjects<Group>
    {

        #region constructor

        internal Groups(DxfDocument document, string handle = null)
            : base(document,
            new Dictionary<string, Group>(StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, List<DxfObject>>(StringComparer.OrdinalIgnoreCase),
            StringCode.GroupDictionary,
            handle)
        {
        }

        #endregion

        #region override methods

        /// <summary>
        /// Adds a group to the list.
        /// </summary>
        /// <param name="group"><see cref="Group">Group</see> to add to the list.</param>
        /// <returns>
        /// If a group already exists with the same name as the instance that is being added the method returns the existing group,
        /// if not it will return the new user coordinate system.<br />
        /// The methods will automatically add the grouped entities to the document, if they have not been added previously.
        /// </returns>
        internal override Group Add(Group group, bool assignHandle)
        {
            // if no name has been given to the group a generic name will be created
            if (group.IsUnnamed && string.IsNullOrEmpty(group.Name))
                group.Name = "*A" + ++this.document.GroupNamesGenerated;

            Group add;
            if (this.list.TryGetValue(group.Name, out add))
                return add;

            if (assignHandle)
                this.document.NumHandles = group.AsignHandle(this.document.NumHandles);

            this.document.AddedObjects.Add(group.Handle, group);
            this.list.Add(group.Name, group);
            this.references.Add(group.Name, new List<DxfObject>());
            foreach (EntityObject entity in group.Entities)
            {
                this.document.AddEntity(entity);
                this.references[group.Name].Add(entity);
            }
            group.Owner = this;
            return group;
        }

        /// <summary>
        /// Deletes a group but keeps the grouped entities in the document.
        /// </summary>
        /// <param name="name"><see cref="Group">Group</see> name to remove from the document.</param>
        /// <returns>True is the group has been successfully removed, or false otherwise.</returns>
        /// <remarks>
        /// Reserved group or any other referenced by objects cannot be removed.</remarks>
        public bool Ungroup(string name)
        {
            return Ungroup(this[name]);
        }

        /// <summary>
        /// Deletes a group but keeps the grouped entities in the document.
        /// </summary>
        /// <param name="group"><see cref="Group">Group</see> to remove from the document.</param>
        /// <returns>True is the group has been successfully removed, or false otherwise.</returns>
        /// <remarks>Reserved groups or any other referenced by objects cannot be removed.</remarks>
        public bool Ungroup(Group group)
        {
            if (group == null)
                return false;

            if (!this.Contains(group))
                return false;

            if (group.IsReserved)
                return false;

            if (this.references[group.Name].Count != 0)
                return false;

            group.Owner = null;
            this.document.AddedObjects.Remove(group.Handle);
            this.references.Remove(group.Name);
            this.list.Remove(group.Name);

            return true;

        }

        /// <summary>
        /// Deletes a group and removes the grouped entities from the document.
        /// </summary>
        /// <param name="name"><see cref="Group">Group</see> name to remove from the document.</param>
        /// <returns>True is the group has been successfully removed, or false otherwise.</returns>
        /// <remarks>Reserved groups or any other referenced by objects cannot be removed.</remarks>
        public override bool Remove(string name)
        {
            return Remove(this[name]);
        }

        /// <summary>
        /// Deletes a group and removes the grouped entities from the document.
        /// </summary>
        /// <param name="group"><see cref="Group">Group</see> to remove from the document.</param>
        /// <returns>True is the group has been successfully removed, or false otherwise.</returns>
        /// <remarks>Reserved groups or any other referenced by objects cannot be removed.</remarks>
        public override bool Remove(Group group)
        {
            if (Ungroup(group))
            {
                foreach (EntityObject entity in group.Entities)
                {
                    this.document.RemoveEntity(entity);
                }
                return true;
            }
            return false;
        }

        #endregion

    }
}