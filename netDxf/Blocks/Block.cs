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
using netDxf.Entities;
using netDxf.Tables;
using netDxf.Collections;
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
        private static readonly Block modelSpace;
        private static readonly Block paperSpace;

        #endregion

        #region constants

        /// <summary>
        /// Gets the default *Model_Space block.
        /// </summary>
        public static Block ModelSpace
        {
            get { return modelSpace; }
        }

        /// <summary>
        /// Gets the default *Paper_Space block.
        /// </summary>
        public static Block PaperSpace
        {
            get { return paperSpace; }
        }

        #endregion

        #region constructors

        static Block()
        {
            modelSpace = new Block("*Model_Space", false);
            paperSpace = new Block("*Paper_Space", false);
        }

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
            this.entities.BeforeAddItem += Entities_BeforeAddItem;
            this.entities.AddItem += Entities_AddItem;
            this.entities.BeforeRemoveItem += Entities_BeforeRemoveItem;
            this.entities.RemoveItem += Entities_RemoveItem;

            this.attributes = new AttributeDefinitionDictionary();
            this.attributes.BeforeAddItem += AttributeDefinitions_BeforeAddItem;
            this.attributes.AddItem += AttributeDefinitions_ItemAdd;
            this.attributes.BeforeRemoveItem += AttributeDefinitions_BeforeRemoveItem;
            this.attributes.RemoveItem += AttributeDefinitions_RemoveItem;

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

        #region overrides

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
            else if (this.attributes.ContainsKey(e.Item.Tag))
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