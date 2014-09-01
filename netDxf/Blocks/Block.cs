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
using netDxf.Collections;
using netDxf.Entities;
using netDxf.Header;
using netDxf.Tables;
using Attribute = netDxf.Entities.Attribute;

namespace netDxf.Blocks
{
    /// <summary>
    /// Represents a block definition.
    /// </summary>
    public class Block :
        TableObject
    {
        #region private fields

        private readonly EntityCollection entities;
        private readonly AttributeDefinitionDictionary attributes;
        private string description;
        private BlockEnd end;
        private BlockTypeFlags flags;
        private Layer layer;
        private Vector3 position;

        #endregion

        #region constants

        /// <summary>
        /// Gets the default *Model_Space block.
        /// </summary>
        public static Block ModelSpace
        {
            get { return new Block("*Model_Space", false); }
        }

        /// <summary>
        /// Gets the default *Paper_Space block.
        /// </summary>
        public static Block PaperSpace
        {
            get { return new Block("*Paper_Space", false); }
        }

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>Block</c> class.
        /// </summary>
        /// <param name="name">Block name.</param>
        public Block(string name)
            : this(name, true)
        {
        }

        internal Block(string name, bool checkName)
            : base(name, DxfObjectCode.Block, checkName)
        {
            this.reserved = name.Equals("*Model_Space", StringComparison.OrdinalIgnoreCase);
            this.description = string.Empty;
            this.position = Vector3.Zero;
            this.layer = Layer.Default;

            this.entities = new EntityCollection();
            this.entities.BeforeAddItem += this.Entities_BeforeAddItem;
            this.entities.AddItem += this.Entities_AddItem;
            this.entities.BeforeRemoveItem += this.Entities_BeforeRemoveItem;
            this.entities.RemoveItem += this.Entities_RemoveItem;

            this.attributes = new AttributeDefinitionDictionary();
            this.attributes.BeforeAddItem += this.AttributeDefinitions_BeforeAddItem;
            this.attributes.AddItem += this.AttributeDefinitions_ItemAdd;
            this.attributes.BeforeRemoveItem += this.AttributeDefinitions_BeforeRemoveItem;
            this.attributes.RemoveItem += this.AttributeDefinitions_RemoveItem;

            this.owner = new BlockRecord(name);
            this.flags = BlockTypeFlags.None;
            this.end = new BlockEnd
            {
                Layer = this.layer,
                Owner = this.owner
            };
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the block description.
        /// </summary>
        public string Description
        {
            get { return this.description; }
            set { this.description = string.IsNullOrEmpty(value) ? string.Empty : value; }
        }

        /// <summary>
        /// Gets or sets the block position in world coordinates.
        /// </summary>
        public Vector3 Position
        {
            get { return this.position; }
            set { this.position = value; }
        }

        /// <summary>
        /// Gets or sets the block <see cref="Layer">layer</see>.
        /// </summary>
        public Layer Layer
        {
            get { return this.layer; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                this.layer = value;
                this.end.Layer = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="EntityObject">entity</see> list of the block.
        /// </summary>
        /// <remarks>Null items, entities already owned by another Block, attribute definitions and attributes are not allowed in the entities list.</remarks>
        public EntityCollection Entities
        {
            get { return this.entities; }
        }

        /// <summary>
        /// Gets the <see cref="AttributeDefinition">entity</see> list of the block.
        /// </summary>
        /// <remarks>Null, and attribute definitions already owned by another Block are not allowed in the attribute definition list.</remarks>
        public AttributeDefinitionDictionary AttributeDefinitions
        {
            get { return this.attributes; }
        }

        /// <summary>
        /// Gets the block record associated with this block.
        /// </summary>
        /// <remarks>It returns the same object as the owner property.</remarks>
        public new BlockRecord Record
        {
            get { return (BlockRecord) this.owner; }
        }

        #endregion

        #region internal properties

        /// <summary>
        /// Gets or sets the block end object.
        /// </summary>
        internal BlockEnd End
        {
            get { return this.end; }
            set { this.end = value; }
        }

        /// <summary>
        /// Gets or sets the block-type flags (bit-coded values, may be combined).
        /// </summary>
        internal BlockTypeFlags Flags
        {
            get { return this.flags; }
            set { this.flags = value; }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Creates a block from an external dxf file.
        /// </summary>
        /// <param name="file">Dxf file name.</param>
        /// <param name="name">Name of the new block. By default the file name without extension will be used.</param>
        /// <returns>The block build from the dxf file content. It will return null if the file has not been able to load.</returns>
        /// <remarks>Only the entities contained in ModelSpace will make part of the block.</remarks>
        public static Block Load(string file, string name = null)
        {
#if DEBUG
            DxfDocument dwg = DxfDocument.Load(file);
#else
            DxfDocument dwg;
            try 
            {
                dwg = DxfDocument.Load(file);
            }
            catch
            {
                return null;
            }
#endif

            string blkName = string.IsNullOrEmpty(name) ? dwg.Name : name;

            Block block = new Block(blkName);
            block.position = dwg.DrawingVariables.InsBase;
            block.Record.Units = dwg.DrawingVariables.InsUnits;
            List<DxfObject> entities = dwg.Layouts.GetReferences("Model");
            foreach (DxfObject entity in entities)
            {
                Object clone = ((EntityObject) entity).Clone();
                if (clone is AttributeDefinition)
                {
                    AttributeDefinition attdef = (AttributeDefinition) clone;
                    block.AttributeDefinitions.Add(attdef);
                }
                else if (clone is EntityObject)
                    block.Entities.Add((EntityObject) clone);
            }

            return block;
        }

        /// <summary>
        /// Saves a block to a dxf file.
        /// </summary>
        /// <param name="file">Dxf file name.</param>
        /// <param name="version">Version of the dxf database version.</param>
        /// <param name="isBinary">Defines if the file will be saved as binary, by default it will be saved as text</param>
        /// <returns>Return true if the file has been succesfully save, false otherwise.</returns>
        public bool Save(string file, DxfVersion version, bool isBinary = false)
        {
            DxfDocument dwg = new DxfDocument(version);
            dwg.DrawingVariables.InsBase = this.position;
            dwg.DrawingVariables.InsUnits = this.Record.Units;
            foreach (EntityObject entity in this.entities)
            {
                EntityObject clone = (EntityObject) entity.Clone();
                dwg.AddEntity(clone);
            }
            foreach (AttributeDefinition attdef in this.attributes.Values)
            {
                AttributeDefinition clone = (AttributeDefinition) attdef.Clone();
                dwg.AddEntity(clone);
            }

            return dwg.Save(file, isBinary);
        }

        #endregion

        #region overrides

        /// <summary>
        /// Creates a new Block that is a copy of the current instance.
        /// </summary>
        /// <param name="newName">GBlockroup name of the copy.</param>
        /// <returns>A new Block that is a copy of this instance.</returns>
        public override TableObject Clone(string newName)
        {
            Block copy = new Block(newName)
            {
                Description = this.description,
                Flags = this.flags,
                Layer = (Layer)this.layer.Clone(),
                Position = this.position
            };

            foreach (EntityObject e in this.entities)
            {
                copy.entities.Add((EntityObject)e.Clone());
            }
            foreach (AttributeDefinition a in this.attributes.Values)
            {
                copy.attributes.Add(a);
            }

            return copy;

        }

        /// <summary>
        /// Creates a new Block that is a copy of the current instance.
        /// </summary>
        /// <returns>A new Block that is a copy of this instance.</returns>
        public override object Clone()
        {
            return Clone(this.name);
        }

        /// <summary>
        /// Asigns a handle to the object based in a integer counter.
        /// </summary>
        /// <param name="entityNumber">Number to asign.</param>
        /// <returns>Next avaliable entity number.</returns>
        /// <remarks>
        /// Some objects might consume more than one, is, for example, the case of polylines that will asign
        /// automatically a handle to its vertexes. The entity number will be converted to an hexadecimal number.
        /// </remarks>
        internal override long AsignHandle(long entityNumber)
        {
            entityNumber = this.owner.AsignHandle(entityNumber);
            entityNumber = this.end.AsignHandle(entityNumber);
            foreach (AttributeDefinition attdef in this.attributes.Values)
            {
                entityNumber = attdef.AsignHandle(entityNumber);
            }
            return base.AsignHandle(entityNumber);
        }

        #endregion

        #region Entities collection events

        private void Entities_BeforeAddItem(EntityCollection sender, EntityCollectionEventArgs e)
        {
            // null items, entities already owned by another Block, attribute definitions and attributes are not allowed in the entities list.
            if (e.Item == null)
                e.Cancel = true;
            else if (e.Item is AttributeDefinition)
                e.Cancel = true;
            else if (e.Item is Attribute)
                e.Cancel = true;
            else if (e.Item.Owner != null)
                e.Cancel = true;
            else
                e.Cancel = false;
        }

        private void Entities_AddItem(EntityCollection sender, EntityCollectionEventArgs e)
        {
            e.Item.Owner = this;
        }

        private void Entities_BeforeRemoveItem(EntityCollection sender, EntityCollectionEventArgs e)
        {
            // only items owned by tha actual block can be removed
            e.Cancel = !ReferenceEquals(e.Item.Owner, this);
        }

        private void Entities_RemoveItem(EntityCollection sender, EntityCollectionEventArgs e)
        {
            e.Item.Owner = null;
        }

        #endregion

        #region Attributes dictionary events

        private void AttributeDefinitions_BeforeAddItem(AttributeDefinitionDictionary sender, AttributeDefinitionDictionaryEventArgs e)
        {
            // null, attributes with the same tag, and attribute definitions already owned by another Block are not allowed in the attributes list.
            if (e.Item == null)
                e.Cancel = true;
            else if (this.attributes.ContainsTag(e.Item.Tag))
                e.Cancel = true;
            else if (e.Item.Owner != null)
                e.Cancel = true;
            else
                e.Cancel = false;
        }

        private void AttributeDefinitions_ItemAdd(AttributeDefinitionDictionary sender, AttributeDefinitionDictionaryEventArgs e)
        {
            e.Item.Owner = this;
        }

        private void AttributeDefinitions_BeforeRemoveItem(AttributeDefinitionDictionary sender, AttributeDefinitionDictionaryEventArgs e)
        {
            // only attribute definitions owned by tha actual block can be removed
            e.Cancel = !ReferenceEquals(e.Item.Owner, this);
        }

        private void AttributeDefinitions_RemoveItem(AttributeDefinitionDictionary sender, AttributeDefinitionDictionaryEventArgs e)
        {
            e.Item.Owner = null;
        }

        #endregion
    }
}