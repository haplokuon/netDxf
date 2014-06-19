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
using netDxf.Collections;
using netDxf.Tables;

namespace netDxf.Objects
{
    /// <summary>
    /// Represents a group of entities.
    /// </summary>
    public class Group :
        TableObject
    {

        #region private fields

        private string description;
        private bool isUnnamed;
        private bool isSelectable;
        private EntityCollection entities;

        #endregion

        #region constructor

        /// <summary>
        /// Initialized a new empty group.
        /// </summary>
        /// <param name="name">Group name (optional).</param>
        /// <remarks>
        /// If the name is set to null or empty, a unique name will be generated when it is added to the document.
        /// </remarks>
        public Group(string name = "")
            : base(name, DxfObjectCode.Group, !string.IsNullOrEmpty(name))
        {
            this.isUnnamed = string.IsNullOrEmpty(name);
            this.description = string.Empty;
            this.isSelectable = true;
            this.entities = new EntityCollection();
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the description of the group.
        /// </summary>
        public string Description
        {
            get { return description; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value"); 
                description = value;
            }
        }

        /// <summary>
        /// Get if the group has an automatic generated name.
        /// </summary>
        public bool IsUnnamed
        {
            get { return isUnnamed; }
            internal set { isUnnamed = value; }
        }

        /// <summary>
        /// Gets or sets if the group is selectable.
        /// </summary>
        public bool IsSelectable
        {
            get { return isSelectable; }
            set { isSelectable = value; }
        }

        /// <summary>
        /// Gets or sets the list of entities contained in the group.
        /// </summary>
        /// <remarks>
        /// When the group is added to the document the entities in it will be automatically added too.<br/>
        /// An entity may be contained in different groups.<br/>
        /// If the entities list is modified after it has been added to the document the entities will have to be added manually.
        /// </remarks>
        public EntityCollection Entities
        {
            get { return entities; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                entities = value;
            }
        }

        /// <summary>
        /// Gets the owner of the actual dxf object.
        /// </summary>
        public new Groups Owner
        {
            get { return (Groups)this.owner; }
            internal set { this.owner = value; }
        }

        #endregion

    }
}
