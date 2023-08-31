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
using System.Collections;
using System.Collections.Generic;
using netDxf.Tables;

namespace netDxf.Collections
{
    /// <summary>
    /// Represents a list of table objects
    /// </summary>
    /// <typeparam name="T"><see cref="TableObject">TableObject</see>.</typeparam>
    public abstract class TableObjects<T> :
        DxfObject,
        IEnumerable<T> where T : TableObject
    {
        #region private fields

        private readonly Dictionary<string, T> list;
        private readonly Dictionary<string, DxfObjectReferences> references;

        #endregion

        #region constructor

        /// <summary>
        /// Initializes a new instance of the <c>TableObjects</c> class.
        /// </summary>
        /// <param name="document">DxfDocument associated with this instance.</param>
        /// <param name="codeName">DxfCode assigned to this instance.</param>
        /// <param name="handle">Handle assigned to this instance.</param>
        protected TableObjects(DxfDocument document, string codeName, string handle)
            : base(codeName)
        {
            this.list = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
            this.references = new Dictionary<string, DxfObjectReferences>(StringComparer.OrdinalIgnoreCase);
            this.Owner = document;

            if (string.IsNullOrEmpty(handle))
            {
                this.Owner.NumHandles = base.AssignHandle(this.Owner.NumHandles);
            }
            else
            {
                this.Handle = handle;
            }

            this.Owner.AddedObjects.Add(this.Handle, this);
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets a table object from the list by name.
        /// </summary>
        /// <param name="name">Table object name.</param>
        /// <returns>The table object with the specified name.</returns>
        /// <remarks>Table object names are case insensitive.</remarks>
        public T this[string name]
        {
            get { return this.list.TryGetValue(name, out T item) ? item : null; }
        }

        /// <summary>
        /// Gets the table object list.
        /// </summary>
        public ICollection<T> Items
        {
            get { return this.list.Values; }
        }

        /// <summary>
        /// Gets the ObjectTable names.
        /// </summary>
        public ICollection<string> Names
        {
            get { return this.list.Keys; }
        }

        /// <summary>
        /// Gets the number of table objects.
        /// </summary>
        public int Count
        {
            get { return this.list.Count; }
        }

        /// <summary>
        /// Gets the owner of the actual DXF object.
        /// </summary>
        public new DxfDocument Owner
        {
            get { return (DxfDocument) base.Owner; }
            internal set { base.Owner = value; }
        }

        #endregion

        #region internal properties

        internal Dictionary<string, T> List
        {
            get { return this.list; }
        }

        internal Dictionary<string, DxfObjectReferences> References
        {
            get { return this.references; }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Checks if the specified TableObject has been referenced by other DxfObjects. 
        /// </summary>
        /// <param name="name">Table object name.</param>
        /// <returns>
        /// Returns true if the specified TableObject has been referenced by other DxfObjects, false otherwise.
        /// </returns>
        public bool HasReferences(string name)
        {
            return !this.references[name].IsEmpty();
        }

        /// <summary>
        /// Checks if the specified TableObject has been referenced by other DxfObjects. 
        /// </summary>
        /// <param name="item">Table object.</param>
        /// <returns>
        /// Returns true if the specified TableObject has been referenced by other DxfObjects, false otherwise.
        /// </returns>
        public bool HasReferences(T item)
        {
            return !this.references[item.Name].IsEmpty();
        }

        /// <summary>
        /// Gets the <see cref="DxfObject">dxf objects</see> referenced by a T.
        /// </summary>
        /// <param name="name">Table object name.</param>
        /// <returns>The list of DxfObjects that reference the specified table object.</returns>
        /// <remarks>
        /// If there is no table object with the specified name in the list the method an empty list.<br />
        /// The Groups collection method GetReferences will always return an empty list since there are no DxfObjects that references them.
        /// </remarks>
        public List<DxfObjectReference> GetReferences(string name)
        {
            return this.references[name].ToList();
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
        public List<DxfObjectReference> GetReferences(T item)
        {
            return this.references[item.Name].ToList();
        }

        /// <summary>
        /// Checks if a table object already exists in the list. 
        /// </summary>
        /// <param name="name">Table object name.</param>
        /// <returns>True is a table object exists with the specified name, false otherwise.</returns>
        public bool Contains(string name)
        {
            return this.list.ContainsKey(name);
        }

        /// <summary>
        /// Checks if a table object already exists in the list. 
        /// </summary>
        /// <param name="item">Table object.</param>
        /// <returns>True is a table object exists, false otherwise.</returns>
        public bool Contains(T item)
        {
            return this.list.ContainsValue(item);
        }

        /// <summary>
        /// Gets the table object associated with the specified name.
        /// </summary>
        /// <param name="name"> The name of the table object to get.</param>
        /// <param name="item">When this method returns, contains the table object associated with the specified name, if the key is found;
        /// otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
        /// <returns>True if the table contains an element with the specified name; otherwise, false.</returns>
        public bool TryGetValue(string name, out T item)
        {
            return this.list.TryGetValue(name, out item);
        }

        /// <summary>
        /// Adds a table object to the list.
        /// </summary>
        /// <param name="item"><see cref="TableObject">Table object</see> to add to the list.</param>
        /// <returns>
        /// If a table object already exists with the same name as the instance that is being added the method returns the existing table object,
        /// if not it will return the new table object.
        /// </returns>
        public T Add(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            return this.Add(item, true);
        }

        internal abstract T Add(T item, bool assignHandle);

        /// <summary>
        /// Removes a table object.
        /// </summary>
        /// <param name="name">Table object name to remove from the document.</param>
        /// <returns>True is the table object has been successfully removed, or false otherwise.</returns>
        /// <remarks>Reserved table objects or any other referenced by objects cannot be removed.</remarks>
        public abstract bool Remove(string name);

        /// <summary>
        /// Removes a table object.
        /// </summary>
        /// <param name="item"><see cref="TableObject">Table object</see> to remove from the document.</param>
        /// <returns>True is the table object has been successfully removed, or false otherwise.</returns>
        /// <remarks>Reserved table objects or any other referenced by objects cannot be removed.</remarks>
        public abstract bool Remove(T item);

        /// <summary>
        /// Removes all table objects that are not reserved and have no references.
        /// </summary>
        public void Clear()
        {
            string[] names = new string[this.list.Count];
            this.list.Keys.CopyTo(names, 0);
            foreach (string o in names)
            {
                this.Remove(o);
            }
        }

        #endregion

        #region implements IEnumerator<T>

        /// <summary>
        /// Returns an enumerator that iterates through the table object collection.
        /// </summary>
        /// <returns>An enumerator for the table object collection.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return this.list.Values.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the table object collection.
        /// </summary>
        /// <returns>An enumerator for the table object collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.list.Values.GetEnumerator();
        }

        #endregion
    }
}