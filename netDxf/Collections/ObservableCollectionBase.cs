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

namespace netDxf.Collections
{
    /// <summary>
    /// Represents the arguments thrown by the <c>ObservableCollection</c> events.
    /// </summary>
    /// <typeparam name="T">Type of items.</typeparam>
    public class ObservableCollectionBaseEventArgs<T> :
        EventArgs
    {
        #region private fields

        private readonly T item;
        private bool cancel;

        #endregion

        #region constructor

        /// <summary>
        /// Initializes a new instance of <c>ObservableCollectionEventArgs</c>.
        /// </summary>
        /// <param name="item">Item that is being added or removed from the collection.</param>
        public ObservableCollectionBaseEventArgs(T item)
        {
            this.item = item;
            this.cancel = false;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Get the item that is being added or removed from the collection.
        /// </summary>
        public T Item
        {
            get { return this.item; }
        }

        /// <summary>
        /// Gets or sets if the operation must be canceled.
        /// </summary>
        /// <remarks>This property is used by the OnBeforeAdd and OnBeforeRemove events to cancel the add or remove operation.</remarks>
        public bool Cancel
        {
            get { return this.cancel; }
            set { this.cancel = value; }
        }

        #endregion
    }

    /// <summary>
    /// Represent a collection of items that fire events when it is modified. 
    /// </summary>
    /// <typeparam name="T">Type of items.</typeparam>
    public class ObservableCollectionBase<T> :
        IList<T>
    {

        #region delegates and events

        public delegate void AddItemEventHandler(ObservableCollectionBase<T> sender, ObservableCollectionBaseEventArgs<T> e);
        public delegate void BeforeAddItemEventHandler(ObservableCollectionBase<T> sender, ObservableCollectionBaseEventArgs<T> e);
        public delegate void RemoveItemEventHandler(ObservableCollectionBase<T> sender, ObservableCollectionBaseEventArgs<T> e);
        public delegate void BeforeRemoveItemEventHandler(ObservableCollectionBase<T> sender, ObservableCollectionBaseEventArgs<T> e);

        public event BeforeAddItemEventHandler BeforeAddItem;
        public event AddItemEventHandler AddItem;
        public event BeforeRemoveItemEventHandler BeforeRemoveItem;
        public event RemoveItemEventHandler RemoveItem;

        protected virtual void OnAddItemEvent(T item)
        {
            AddItemEventHandler ae = AddItem;
            if (ae != null)
                ae(this, new ObservableCollectionBaseEventArgs<T>(item));
        }

        protected virtual bool OnBeforeAddItemEvent(T item)
        {
            BeforeAddItemEventHandler ae = BeforeAddItem;
            if (ae != null)
            {
                ObservableCollectionBaseEventArgs<T> e = new ObservableCollectionBaseEventArgs<T>(item);
                ae(this, e);
                return e.Cancel;
            }
            return false;
        }

        protected virtual bool OnBeforeRemoveItemEvent(T item)
        {
            BeforeRemoveItemEventHandler ae = BeforeRemoveItem;
            if (ae != null)
            {
                ObservableCollectionBaseEventArgs<T> e = new ObservableCollectionBaseEventArgs<T>(item);
                ae(this, e);
                return e.Cancel;
            }
            return false;
        }

        protected virtual void OnRemoveItemEvent(T item)
        {
            RemoveItemEventHandler ae = RemoveItem;
            if (ae != null)
                ae(this, new ObservableCollectionBaseEventArgs<T>(item));
        }

        #endregion

        #region private fields

        protected List<T> innerArray;

        #endregion

        #region constructor

        public ObservableCollectionBase()
        {
            innerArray = new List<T>();
        }

        public ObservableCollectionBase(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException();
            innerArray = new List<T>(capacity);
        }

        #endregion

        #region public properties

        public T this[int index]
        {
            get { return this.innerArray[index]; }
            set
            {
                T remove = this.innerArray[index];
                T add = value;

                if(OnBeforeRemoveItemEvent(remove)) return;
                if (OnBeforeAddItemEvent(add)) return;
                this.innerArray[index] = value;
                OnAddItemEvent(add);
                OnRemoveItemEvent(remove);
            }
        }

        public int Count
        {
            get { return this.innerArray.Count; }
        }

        public virtual bool IsReadOnly
        {
            get { return false; }
        }

        #endregion

        #region public methods

        public void Add(T item)
        {
            if (OnBeforeAddItemEvent(item)) return;
            this.innerArray.Add(item);
            OnAddItemEvent(item);
        }

        public void AddRange(IList<T> collection)
        {
            if (collection == null) throw new ArgumentNullException();
            // we will make room for so the collection will fit without having to resize the internal array during the Add method
            this.innerArray.Capacity = this.innerArray.Count + collection.Count;
            foreach (T item in collection)
            {
                if (OnBeforeAddItemEvent(item)) continue;
                this.innerArray.Add(item);
                OnAddItemEvent(item);
            }
        }

        public void AddRange(T[] collection)
        {
            if (collection == null) throw new ArgumentNullException();
            // we will make room for so the collection will fit without having to resize the internal array during the Add method
            this.innerArray.Capacity = this.innerArray.Count + collection.Length;
            foreach (T item in collection)
            {
                if (OnBeforeAddItemEvent(item)) continue;
                this.innerArray.Add(item);
                OnAddItemEvent(item);
            }
        }

        public void Insert(int index, T item)
        {
            if (OnBeforeRemoveItemEvent(this.innerArray[index])) return;
            if (OnBeforeAddItemEvent(item)) return;
            OnRemoveItemEvent(this.innerArray[index]);
            this.innerArray.Insert(index, item);
            OnAddItemEvent(item);
        }

        public bool Remove(T item)
        {
            if (!this.innerArray.Contains(item)) return false;
            if (OnBeforeRemoveItemEvent(item)) return false;
            this.innerArray.Remove(item);
            OnRemoveItemEvent(item);
            return true;
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= this.innerArray.Count) return;
            T remove = this.innerArray[index];
            if (OnBeforeRemoveItemEvent(remove)) return;
            this.innerArray.RemoveAt(index);
            OnRemoveItemEvent(remove);
        }

        public void Clear()
        {
            foreach (T item in innerArray)
            {
                this.Remove(item);
            }
        }

        public int IndexOf(T item)
        {
            return this.innerArray.IndexOf(item);
        }

        public bool Contains(T item)
        {
            return this.innerArray.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this.innerArray.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
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
