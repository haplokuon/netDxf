#region netDxf, Copyright(C) 2013 Daniel Carvajal, Licensed under LGPL.

//                        netDxf library
// Copyright (C) 2013 Daniel Carvajal (haplokuon@gmail.com)
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

namespace netDxf.Objects
{
    /// <summary>
    /// Represents a group of entities.
    /// </summary>
    public class Group :
        DxfObject
    {

        #region private fields

        private string name;
        private string description;
        private bool isUnnamed;
        private bool isSelectable;
        private List<EntityObject> entities;

        #endregion

        #region constructor

        /// <summary>
        /// Initialized a new empty group.
        /// </summary>
        /// <param name="name">Group name (optional).</param>
        /// <remarks>
        /// If the name is set to null or empty, a unique name will be generated when it is added to the document.
        /// </remarks>
        public Group(string name = null)
            : base(DxfObjectCode.Group)
        {
            if (string.IsNullOrEmpty(name))
            {
                this.isUnnamed = true;
                this.name = null;
            }
            else
            {
                if (name.StartsWith("*"))
                    throw new ArgumentException("Names starting with * are reserved.", name);
                this.isUnnamed = false;
                this.name = name;
            }
            this.description = null;
            this.isSelectable = true;
            
            this.entities = new List<EntityObject>();
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the name of the group.
        /// </summary>
        public string Name
        {
            get { return name; }
            internal set { name = value; }
        }

        /// <summary>
        /// Gets or sets the description of the group.
        /// </summary>
        public string Description
        {
            get { return description; }
            set { description = value; }
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
        public List<EntityObject> Entities
        {
            get { return entities; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                entities = value;
            }
        }

        #endregion

    }
}
