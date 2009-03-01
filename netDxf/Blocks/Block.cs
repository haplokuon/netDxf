#region netDxf, Copyright(C) 2009 Daniel Carvajal, Licensed under LGPL.

// netDxf library
// Copyright (C) 2009  
// Daniel Carvajal
// haplokuon@gmail.com
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

#endregion

using System;
using System.Collections.Generic;
using netDxf.Entities;
using netDxf.Tables;

namespace netDxf.Blocks
{
    /// <summary>
    /// Represents a block definition.
    /// </summary>
    public class Block
    {
        #region private fields

        private readonly string name;
        private Layer layer;
        private Vector3 basePoint;
        private Dictionary<string, AttributeDefinition> attributes;
        private List<IEntityObject> entities;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>Block</c> class.
        /// </summary>
        /// <param name="name">Block name.</param>
        public Block(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw (new ArgumentNullException("name"));
            this.name = name;
            this.basePoint = Vector3.Zero;
            this.layer = Layer.Default;
            this.attributes = new Dictionary<string, AttributeDefinition>();
            this.entities = new List<IEntityObject>();
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the block name.
        /// </summary>
        public string Name
        {
            get { return this.name; }
        }

        /// <summary>
        /// Gets or sets the block base point.
        /// </summary>
        public Vector3 BasePoint
        {
            get { return this.basePoint; }
            set { this.basePoint = value; }
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
            }
        }

        /// <summary>
        /// Gets or sets the block <see cref="AttributeDefinition">attribute definition</see> list.
        /// </summary>
        public Dictionary<string, AttributeDefinition> Attributes
        {
            get { return this.attributes; }
            set
            {
                if (value == null)
                    throw new NullReferenceException("value");
                this.attributes = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IEntityObject">entity</see> list that makes the block.
        /// </summary>
        public List<IEntityObject> Entities
        {
            get { return this.entities; }
            set
            {
                if (value == null)
                    throw new NullReferenceException("value");
                this.entities = value;
            }
        }

        #endregion

        #region overrides

        /// <summary>
        /// Converts the value of this instance to its equivalent string representation.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            return this.name;
        }

        #endregion
    }
}