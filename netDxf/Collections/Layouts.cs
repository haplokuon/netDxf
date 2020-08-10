#region netDxf library, Copyright (C) 2009-2020 Daniel Carvajal (haplokuon@gmail.com)

//                        netDxf library
// Copyright (C) 2009-2020 Daniel Carvajal (haplokuon@gmail.com)
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
using netDxf.Tables;

namespace netDxf.Collections
{
    /// <summary>
    /// Represents a collection of layouts.
    /// </summary>
    /// <remarks>
    /// You can add a maximum of 255 layouts to your drawing, the "Model" layout is always present what limits the maximum number of layouts to 256.
    /// Even though this limit is imposed through the AutoCad UI, it can import larger numbers, but exceeding this limit might make it to crash.
    /// </remarks>
    public sealed class Layouts :
        TableObjects<Layout>
    {

        #region private fields

        public const short MaxCapacity = 256;

        #endregion

        #region constructor

        internal Layouts(DxfDocument document)
            : this(document, null)
        {
        }

        internal Layouts(DxfDocument document, string handle)
            : base(document, DxfObjectCode.LayoutDictionary, handle)
        {
            this.references = null;
        }

        #endregion

        #region override methods

        /// <summary>
        /// Gets the <see cref="DxfObject">dxf objects</see> referenced by a T.
        /// </summary>
        /// <param name="name">Table object name.</param>
        /// <returns>The list of DxfObjects that reference the specified table object.</returns>
        /// <remarks>
        /// If there is no table object with the specified name in the list the method an empty list.<br />
        /// The Groups collection method GetReferences will always return an empty list since there are no DxfObjects that references them.
        /// </remarks>
        public new List<DxfObject> GetReferences(string name)
        {
            if (!this.Contains(name))
                return new List<DxfObject>();
            return this.GetReferences(this[name]);
        }

        /// <summary>
        /// Gets the <see cref="DxfObject">dxf objects</see> referenced by a T.
        /// </summary>
        /// <param name="item">Table object.</param>
        /// <returns>The list of DxfObjects that reference the specified table object.</returns>
        /// <remarks>
        /// If there is no table object with the specified name in the list the method an empty list.<br />
        /// The Groups collection method GetReferences will always return an empty list since there are no DxfObjects that references them.
        /// </remarks>
        public new List<DxfObject> GetReferences(Layout item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            List<DxfObject> refs = new List<DxfObject>();
            refs.AddRange(item.AssociatedBlock.Entities);
            refs.AddRange(item.AssociatedBlock.AttributeDefinitions.Values);
            return refs;
        }

        /// <summary>
        /// Adds a layout to the list.
        /// </summary>
        /// <param name="layout"><see cref="Layout">Layout</see> to add to the list.</param>
        /// <param name="assignHandle">Specifies if a handle needs to be generated for the layout parameter.</param>
        /// <returns>
        /// You can add a maximum of 255 layouts to your drawing, the "Model" layout is always present what limits the maximum number of layouts to 256.
        /// If a layout already exists with the same name as the instance that is being added the method returns the existing layout.
        /// </returns>
        internal override Layout Add(Layout layout, bool assignHandle)
        {
            if (this.list.Count >= MaxCapacity)
            {
                throw new OverflowException(string.Format("Table overflow. The maximum number of elements the table {0} can have is {1}", this.CodeName, MaxCapacity));
            }

            if (layout == null)
            {
                throw new ArgumentNullException(nameof(layout));
            }

            Layout add;

            if (this.list.TryGetValue(layout.Name, out add))
            {
                return add;
            }

            layout.Owner = this;

            Block associatedBlock = layout.AssociatedBlock;

            // create and add the corresponding PaperSpace block
            if (layout.IsPaperSpace && associatedBlock == null)
            {
                // the PaperSpace block names follow the naming Paper_Space, Paper_Space0, Paper_Space1, ...
                string spaceName = this.list.Count == 1 ? Block.DefaultPaperSpaceName : string.Concat(Block.DefaultPaperSpaceName, this.list.Count - 2);
                associatedBlock = new Block(spaceName, null, null, false);
                if (layout.TabOrder == 0)
                {
                    layout.TabOrder = (short) this.list.Count;
                }
            }

            associatedBlock = this.Owner.Blocks.Add(associatedBlock);

            layout.AssociatedBlock = associatedBlock;
            associatedBlock.Record.Layout = layout;
            this.Owner.Blocks.References[associatedBlock.Name].Add(layout);

            if (layout.Viewport != null)
            {
                layout.Viewport.Owner = associatedBlock;
            }

            if (assignHandle || string.IsNullOrEmpty(layout.Handle))
            {
                this.Owner.NumHandles = layout.AssignHandle(this.Owner.NumHandles);
            }

            this.list.Add(layout.Name, layout);

            layout.NameChanged += this.Item_NameChanged;

            this.Owner.AddedObjects.Add(layout.Handle, layout);

            return layout;
        }

        /// <summary>
        /// Deletes a layout and removes the layout entities from the document.
        /// </summary>
        /// <param name="name"><see cref="Layout">Layout</see> name to remove from the document.</param>
        /// <returns>True if the layout has been successfully removed, or false otherwise.</returns>
        /// <remarks>
        /// The ModelSpace layout cannot be removed. If all PaperSpace layouts have been removed a default PaperSpace will be created since it is required by the DXF implementation.<br />
        /// When a Layout is deleted all entities that has been added to it will also be removed.<br />
        /// Removing a Layout will rebuild the PaperSpace block names, to follow the naming rule: Paper_Space, Paper_Space0, Paper_Space1, ...
        /// </remarks>
        public override bool Remove(string name)
        {
            return this.Remove(this[name]);
        }

        /// <summary>
        /// Deletes a layout and removes the layout entities from the document.
        /// </summary>
        /// <param name="item"><see cref="Layout">Layout</see> to remove from the document.</param>
        /// <returns>True if the layout has been successfully removed, or false otherwise.</returns>
        /// <remarks>Reserved layouts or any other referenced by objects cannot be removed.</remarks>
        public override bool Remove(Layout item)
        {
            if (item == null) return false;

            if (!this.Contains(item)) return false;

            if (item.IsReserved) return false;

            // remove the entities of the layout
            List<DxfObject> refObjects = this.GetReferences(item.Name);
            if (refObjects.Count != 0)
            {
                DxfObject[] entities = new DxfObject[refObjects.Count];
                refObjects.CopyTo(entities);
                foreach (DxfObject e in entities)
                {
                    this.Owner.RemoveEntity(e as EntityObject);
                }
            }

            // remove the associated block of the Layout that is being removed
            this.Owner.Blocks.References[item.AssociatedBlock.Name].Remove(item);
            this.Owner.Blocks.Remove(item.AssociatedBlock);
            item.AssociatedBlock = null;

            // remove the layout
            this.Owner.AddedObjects.Remove(item.Handle);
            this.list.Remove(item.Name);

            item.Handle = null;
            item.Owner = null;

            item.NameChanged -= this.Item_NameChanged;

            this.RenameAssociatedBlocks();

            return true;
        }

        #endregion

        #region internal methods

        internal void RenameAssociatedBlocks()
        {
            List<int> names = new List<int>();
            foreach (Layout l in this.list.Values)
            {
                if (l.IsPaperSpace)
                {
                    string blockName = l.AssociatedBlock.Name.Remove(0, Block.PaperSpace.Name.Length);
                    int index;
                    if (int.TryParse(blockName, out index))
                    {
                        names.Add(index);
                    }
                    else
                    {
                        names.Add(-1); // for the *Paper_Space block name
                    }
                }
            }
            names.Sort();

            for (int i = 0; i < names.Count; i++)
            {
                string originalName = names[i] == -1 ? Block.PaperSpace.Name : string.Concat(Block.PaperSpace.Name, names[i]);
                string newName = i == 0 ? Block.PaperSpace.Name : string.Concat(Block.PaperSpace.Name, i - 1);
                this.Owner.Blocks[originalName].SetName(newName, false);
            }
        }

        #endregion

        #region Linetype events

        private void Item_NameChanged(TableObject sender, TableObjectChangedEventArgs<string> e)
        {
            if (this.Contains(e.NewValue))
            {
                throw new ArgumentException("There is already another layout with the same name.");
            }

            this.list.Remove(sender.Name);
            this.list.Add(e.NewValue, (Layout) sender);
        }

        #endregion
    }
}