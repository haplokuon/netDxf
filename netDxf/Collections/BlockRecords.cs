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
using System.Diagnostics;
using netDxf.Blocks;
using netDxf.Entities;
using netDxf.Tables;

namespace netDxf.Collections
{
    /// <summary>
    /// Represents a collection of blocks.
    /// </summary>
    public sealed class BlockRecords :
        TableObjects<Block>
    {
        #region constructor

        internal BlockRecords(DxfDocument document)
            : this(document, null)
        {
        }

        internal BlockRecords(DxfDocument document, string handle)
            : base(document, DxfObjectCode.BlockRecordTable, handle)
        {
        }

        #endregion

        #region override methods

        /// <summary>
        /// Adds a block to the list.
        /// </summary>
        /// <param name="block"><see cref="Block">Block</see> to add to the list.</param>
        /// <param name="assignHandle">Specifies if a handle needs to be generated for the block parameter.</param>
        /// <returns>
        /// If a block already exists with the same name as the instance that is being added the method returns the existing block,
        /// if not it will return the new block.
        /// </returns>
        internal override Block Add(Block block, bool assignHandle)
        {
            if (block == null)
            {
                throw new ArgumentNullException(nameof(block));
            }

            if (this.List.TryGetValue(block.Name, out Block add))
            {
                return add;
            }

            if (assignHandle || string.IsNullOrEmpty(block.Handle))
            {
                this.Owner.NumHandles = block.AssignHandle(this.Owner.NumHandles);
            }

            this.List.Add(block.Name, block);
            this.References.Add(block.Name, new DxfObjectReferences());

            block.Layer = this.Owner.Layers.Add(block.Layer);
            this.Owner.Layers.References[block.Layer.Name].Add(block);

            //for new block definitions configure its entities
            foreach (EntityObject entity in block.Entities)
            {
                this.Owner.AddEntityToDocument(entity, assignHandle);
            }

            //for new block definitions configure its attributes
            foreach (AttributeDefinition attDef in block.AttributeDefinitions.Values)
            {
                this.Owner.AddAttributeDefinitionToDocument(attDef, assignHandle);
            }

            block.Record.Owner = this;

            block.NameChanged += this.Item_NameChanged;
            block.LayerChanged += this.Block_LayerChanged;
            block.EntityAdded += this.Block_EntityAdded;
            block.EntityRemoved += this.Block_EntityRemoved;
            block.AttributeDefinitionAdded += this.Block_AttributeDefinitionAdded; 
            block.AttributeDefinitionRemoved += this.Block_AttributeDefinitionRemoved;

            Debug.Assert(!string.IsNullOrEmpty(block.Handle), "The block handle cannot be null or empty.");
            this.Owner.AddedObjects.Add(block.Handle, block);
            this.Owner.AddedObjects.Add(block.Owner.Handle, block.Owner);

            return block;
        }

        /// <summary>
        /// Removes a block.
        /// </summary>
        /// <param name="name"><see cref="Block">Block</see> name to remove from the document.</param>
        /// <returns>True if the block has been successfully removed, or false otherwise.</returns>
        /// <remarks>Reserved blocks or any other referenced by objects cannot be removed.</remarks>
        public override bool Remove(string name)
        {
            return this.Remove(this[name]);
        }

        /// <summary>
        /// Removes a block.
        /// </summary>
        /// <param name="item"><see cref="Block">Block</see> to remove from the document.</param>
        /// <returns>True if the block has been successfully removed, or false otherwise.</returns>
        /// <remarks>Reserved blocks or any other referenced by objects cannot be removed.</remarks>
        public override bool Remove(Block item)
        {
            if (item == null)
            {
                return false;
            }

            if (!this.Contains(item))
            {
                return false;
            }

            if (item.IsReserved)
            {
                return false;
            }

            if (this.HasReferences(item))
            {
                return false;
            }

            // remove the block from the associated layer
            this.Owner.Layers.References[item.Layer.Name].Remove(item);

            // we will remove all entities from the block definition
            foreach (EntityObject entity in item.Entities)
            {
                this.Owner.RemoveEntityFromDocument(entity);
            }

            // remove all attribute definitions from the associated layers
            foreach (AttributeDefinition attDef in item.AttributeDefinitions.Values)
            {
                this.Owner.RemoveAttributeDefinitionFromDocument(attDef);
            }

            this.Owner.AddedObjects.Remove(item.Handle);
            this.References.Remove(item.Name);
            this.List.Remove(item.Name);

            item.Record.Handle = null;
            item.Record.Owner = null;

            item.Handle = null;
            item.Owner = null;

            item.NameChanged -= this.Item_NameChanged;
            item.LayerChanged -= this.Block_LayerChanged;
            item.EntityAdded -= this.Block_EntityAdded;
            item.EntityRemoved -= this.Block_EntityRemoved;
            item.AttributeDefinitionAdded -= this.Block_AttributeDefinitionAdded;
            item.AttributeDefinitionRemoved -= this.Block_AttributeDefinitionRemoved;

            return true;
        }

        #endregion

        #region Block events

        private void Item_NameChanged(TableObject sender, TableObjectChangedEventArgs<string> e)
        {
            if (this.Contains(e.NewValue))
            {
                throw new ArgumentException("There is already another block with the same name.");
            }

            this.List.Remove(sender.Name);
            this.List.Add(e.NewValue, (Block) sender);

            List<DxfObjectReference> refs = this.GetReferences(sender.Name);
            this.References.Remove(sender.Name);
            this.References.Add(e.NewValue, new DxfObjectReferences());
            this.References[e.NewValue].Add(refs);
        }

        private void Block_LayerChanged(Block sender, TableObjectChangedEventArgs<Layer> e)
        {
            this.Owner.Layers.References[e.OldValue.Name].Remove(sender);

            e.NewValue = this.Owner.Layers.Add(e.NewValue);
            this.Owner.Layers.References[e.NewValue.Name].Add(sender);
        }

        private void Block_EntityAdded(TableObject sender, BlockEntityChangeEventArgs e)
        {
            this.Owner.AddEntityToDocument(e.Item, string.IsNullOrEmpty(e.Item.Handle));
        }

        private void Block_EntityRemoved(TableObject sender, BlockEntityChangeEventArgs e)
        {
            this.Owner.RemoveEntityFromDocument(e.Item);
        }

        private void Block_AttributeDefinitionAdded(Block sender, BlockAttributeDefinitionChangeEventArgs e)
        {
            this.Owner.AddAttributeDefinitionToDocument(e.Item, string.IsNullOrEmpty(e.Item.Handle));
        }

        private void Block_AttributeDefinitionRemoved(Block sender, BlockAttributeDefinitionChangeEventArgs e)
        {
            this.Owner.RemoveAttributeDefinitionFromDocument(e.Item);
        }

        #endregion
    }
}