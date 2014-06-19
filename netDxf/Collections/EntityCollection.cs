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
using System.Collections;
using System.Collections.Generic;
using netDxf.Entities;

namespace netDxf.Collections
{
    /// <summary>
    /// Represent a collection of entities that fire events when it is modified. 
    /// </summary>
    public class EntityCollection :
        IList<EntityObject>
    {

        #region delegates and events

        public delegate void AddItemEventHandler(EntityCollection sender, EntityCollectionEventArgs e);
        public delegate void BeforeAddItemEventHandler(EntityCollection sender, EntityCollectionEventArgs e);
        public delegate void RemoveItemEventHandler(EntityCollection sender, EntityCollectionEventArgs e);
        public delegate void BeforeRemoveItemEventHandler(EntityCollection sender, EntityCollectionEventArgs e);

        public event BeforeAddItemEventHandler BeforeAddItem;
        public event AddItemEventHandler AddItem;
        public event BeforeRemoveItemEventHandler BeforeRemoveItem;
        public event RemoveItemEventHandler RemoveItem;

        protected virtual void OnAddItemEvent(EntityObject item)
        {
            AddItemEventHandler ae = AddItem;
            if (ae != null)
                ae(this, new EntityCollectionEventArgs(item));
        }

        protected virtual bool OnBeforeAddItemEvent(EntityObject item)
        {
            BeforeAddItemEventHandler ae = BeforeAddItem;
            if (ae != null)
            {
                EntityCollectionEventArgs e = new EntityCollectionEventArgs(item);
                ae(this, e);
                return e.Cancel;
            }
            return false;
        }

        protected virtual bool OnBeforeRemoveItemEvent(EntityObject item)
        {
            BeforeRemoveItemEventHandler ae = BeforeRemoveItem;
            if (ae != null)
            {
                EntityCollectionEventArgs e = new EntityCollectionEventArgs(item);
                ae(this, e);
                return e.Cancel;
            }
            return false;
        }

        protected virtual void OnRemoveItemEvent(EntityObject item)
        {
            RemoveItemEventHandler ae = RemoveItem;
            if (ae != null)
                ae(this, new EntityCollectionEventArgs(item));
        }

        #endregion

        #region private fields

        protected List<EntityObject> innerArray;

        #endregion

        #region constructor

        /// <summary>
        /// Initializes a new instance of <c>EntityCollection</c>.
        /// </summary>
        public EntityCollection()
        {
            innerArray = new List<EntityObject>();
        }

        /// <summary>
        /// Initializes a new instance of <c>EntityCollection</c> and has the specified initial capacity.
        /// </summary>
        /// <param name="capacity">The number of items the collection can initially store.</param>
        public EntityCollection(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException();
            innerArray = new List<EntityObject>(capacity);
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index"> The zero-based index of the element to get or set.</param>
        /// <returns>The element at the specified index.</returns>
        public EntityObject this[int index]
        {
            get { return this.innerArray[index]; }
            set
            {
                EntityObject remove = this.innerArray[index];

                if(OnBeforeRemoveItemEvent(remove)) return;
                if (OnBeforeAddItemEvent(value)) return;
                if (value == null) throw new ArgumentNullException("value");
                this.innerArray[index] = value;
                OnAddItemEvent(value);
                OnRemoveItemEvent(remove);
            }
        }

        /// <summary>
        /// Gets the number of elements contained in the collection.
        /// </summary>
        public int Count
        {
            get { return this.innerArray.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the collection is read-only.
        /// </summary>
        public virtual bool IsReadOnly
        {
            get { return false; }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Adds an item to the collection.
        /// </summary>
        /// <param name="item"> The entity to add to the collection.</param>
        /// <returns>True if the entity has been added to the collection, or false otherwise.</returns>
        public bool Add(EntityObject item)
        {
            if (OnBeforeAddItemEvent(item)) return false;
            if (item == null) throw new ArgumentNullException("item");
            this.innerArray.Add(item);
            OnAddItemEvent(item);
            return true;
        }

        void ICollection<EntityObject>.Add(EntityObject item)
        {
            this.Add(item);
        }

        /// <summary>
        /// Adds the elements to the end of the collection.
        /// </summary>
        /// <param name="collection">The collection whose elements should be added.</param>
        public void AddRange(IList<EntityObject> collection)
        {
            if (collection == null) throw new ArgumentNullException();
            // we will make room for so the collection will fit without having to resize the internal array during the Add method
            this.innerArray.Capacity = this.innerArray.Count + collection.Count;
            foreach (EntityObject item in collection)
                this.Add(item);
        }

        /// <summary>
        /// Adds the elements to the end of the collection.
        /// </summary>
        /// <param name="collection">The collection whose elements should be added.</param>
        public void AddRange(EntityObject[] collection)
        {
            if (collection == null) throw new ArgumentNullException();
            // we will make room for so the collection will fit without having to resize the internal array during the Add method
            this.innerArray.Capacity = this.innerArray.Count + collection.Length;
            foreach (EntityObject item in collection)
                this.Add(item);
        }

        /// <summary>
        /// Inserts an entity into the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The entity to insert. The value can not be null.</param>
        /// <returns>True if the entity has been inserted to the collection; otherwise, false.</returns>
        public bool Insert(int index, EntityObject item)
        {
            if (OnBeforeRemoveItemEvent(this.innerArray[index])) return false;
            if (OnBeforeAddItemEvent(item)) return false;
            if (item == null) throw new ArgumentNullException("item");
            OnRemoveItemEvent(this.innerArray[index]);
            this.innerArray.Insert(index, item);
            OnAddItemEvent(item);
            return true;
        }

        void IList<EntityObject>.Insert(int index, EntityObject item)
        {
            this.Insert(index, item);
        }

        /// <summary>
        /// Removes the first occurrence of a specific entity from the colleciton
        /// </summary>
        /// <param name="item">The object to remove from the collection.</param>
        /// <returns>True if item is successfully removed; otherwise, false.</returns>
        public bool Remove(EntityObject item)
        {
            if (!this.innerArray.Contains(item)) return false;
            if (OnBeforeRemoveItemEvent(item)) return false;
            this.innerArray.Remove(item);
            OnRemoveItemEvent(item);
            return true;
        }

        /// <summary>
        /// Removes the element at the specified index of the collection.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= this.innerArray.Count) throw new IndexOutOfRangeException();
            EntityObject remove = this.innerArray[index];
            if (OnBeforeRemoveItemEvent(remove)) return;
            this.innerArray.RemoveAt(index);
            OnRemoveItemEvent(remove);
        }

        /// <summary>
        /// Removes all elements from the collection.
        /// </summary>
        public void Clear()
        {
            foreach (EntityObject item in innerArray)
                this.Remove(item);
        }

        /// <summary>
        /// Searches for the specified entity and returns the zero-based index of the first occurrence within the entire collection.
        /// </summary>
        /// <param name="item">The entity to locate in the collection.</param>
        /// <returns>The zero-based index of the first occurrence of item within the entire collection, if found; otherwise, –1.</returns>
        public int IndexOf(EntityObject item)
        {
            return this.innerArray.IndexOf(item);
        }

        /// <summary>
        /// Determines whether an element is in the collection.
        /// </summary>
        /// <param name="item">The object to locate in the collection.</param>
        /// <returns>True if item is found in the collection; otherwise, false.</returns>
        public bool Contains(EntityObject item)
        {
            return this.innerArray.Contains(item);
        }

        /// <summary>
        /// Copies the entire collection to a compatible one-dimensional array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array"> The one-dimensional System.Array that is the destination of the elements copied from the collection. The System.Array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(EntityObject[] array, int arrayIndex)
        {
            this.innerArray.CopyTo(array, arrayIndex);
        }

        public IEnumerator<EntityObject> GetEnumerator()
        {
            return this.innerArray.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

    }
}
