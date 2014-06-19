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
using netDxf.Blocks;
using netDxf.Entities;
using netDxf.Objects;

namespace netDxf.Collections
{
    /// <summary>
    /// Represents a collection of layouts.
    /// </summary>
    public sealed class Layouts :
        TableObjects<Layout>
    {

        #region constructor

        internal Layouts(DxfDocument document, string handle = null)
            : base(document,
            new Dictionary<string, Layout>(StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, List<DxfObject>>(StringComparer.OrdinalIgnoreCase),
            StringCode.LayoutDictionary,
            handle)
        {
        }

        #endregion

        #region override methods

        /// <summary>
        /// Adds a layout to the list.
        /// </summary>
        /// <param name="layout"><see cref="Layout">Layout</see> to add to the list.</param>
        /// <returns>
        /// If a layout already exists with the same name as the instance that is being added the method returns the existing layout.
        /// </returns>
        internal override Layout Add(Layout layout, bool assignHandle)
        {
            Layout add;
            if (this.list.TryGetValue(layout.Name, out add))
                return add;

            Block associatadBlock = layout.AssociatedBlock;

            // create and add the corresponding PaperSpace block
            if (layout.IsPaperSpace && associatadBlock == null)
            {
                int index = this.list.Count - 2;
                string spaceName = index < 0 ? Block.PaperSpace.Name : string.Concat(Block.PaperSpace.Name, index);
                associatadBlock = new Block(spaceName, false);
            }

            associatadBlock = this.document.Blocks.Add(associatadBlock);
            layout.AssociatedBlock = associatadBlock;
            associatadBlock.Record.Layout = layout;
            layout.Owner = this;

            if (layout.Viewport != null) layout.Viewport.Owner = associatadBlock;

            if (assignHandle)
                this.document.NumHandles = layout.AsignHandle(this.document.NumHandles);

            this.document.AddedObjects.Add(layout.Handle, layout);

            this.list.Add(layout.Name, layout);

            this.references.Add(layout.Name, new List<DxfObject>());
            return layout;
        }

        /// <summary>
        /// Deletes a layout and removes the layout entities from the document.
        /// </summary>
        /// <param name="name"><see cref="Layout">Layout</see> name to remove from the document.</param>
        /// <returns>True is the layout has been successfully removed, or false otherwise.</returns>
        /// <remarks>
        /// The ModelSpace layout cannot be remove. If all PaperSpace layouts have been removed a default PaperSpace will be created since it is required by the dxf implementation.<br />
        /// When a Layout is deleted all entities that has been added to it will also be removed.<br />
        /// Removing a Layout will rebuild the PaperSpace block names, to follow the naming rule: Paper_Space, Paper_Space0, Paper_Space1, ...
        /// </remarks>
        public override bool Remove(string name)
        {
            return Remove(this[name]);
        }

        /// <summary>
        /// Deletes a layout and removes the layout entities from the document.
        /// </summary>
        /// <param name="layout"><see cref="Layout">Layout</see> to remove from the document.</param>
        /// <returns>True is the layout has been successfully removed, or false otherwise.</returns>
        /// <remarks>Reserved layouts or any other referenced by objects cannot be removed.</remarks>
        public override bool Remove(Layout layout)
        {
            if (layout == null)
                return false;

            if (!this.Contains(layout))
                return false;

            if (layout.IsReserved)
                return false;

            List<DxfObject> refObjects = this.references[layout.Name];
            if (refObjects.Count != 0)
            {
                DxfObject[] entities = new DxfObject[refObjects.Count];
                refObjects.CopyTo(entities);
                foreach (DxfObject e in entities)
                {
                    this.document.RemoveEntity(e as EntityObject);
                }
            }

            // When a layout is removed we need to rebuild the PaperSpace block names, to follow the naming Paper_Space, Paper_Space0, Paper_Space1, ...
            foreach (Layout l in this.list.Values)
            {
                // The ModelSpace block cannot be removed. 
                if (l.IsPaperSpace)
                {
                    this.document.Blocks.Remove(l.AssociatedBlock);
                    l.AssociatedBlock = null;
                }
            }

            layout.Owner = null;
            layout.Viewport.Owner = null;
            this.document.AddedObjects.Remove(layout.Handle);
            this.references.Remove(layout.Name);
            this.list.Remove(layout.Name);

            // When a layout is removed we need to rebuild the PaperSpace block names, to follow the naming Paper_Space, Paper_Space0, Paper_Space1, ...
            foreach (Layout l in this.list.Values)
            {
                // Create and add the corresponding PaperSpace block
                if (l.IsPaperSpace)
                {
                    int index = this.list.Count - 3;
                    string spaceName = index < 0 ? Block.PaperSpace.Name : string.Concat(Block.PaperSpace.Name, index);
                    Block associatadBlock = new Block(spaceName, false);
                    associatadBlock = this.document.Blocks.Add(associatadBlock);
                    l.AssociatedBlock = associatadBlock;
                    associatadBlock.Record.Layout = l;
                }
            }

            return true;

        }

        #endregion

    }
}