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
    /// Represents the arguments thrown by the <c>ObservableDictionaryEventArgs</c> events.
    /// </summary>
    /// <typeparam name="TKey">Type of items.</typeparam>
    /// <typeparam name="TValue">Type of items.</typeparam>
    public class ObservableDictionaryBaseEventArgs<TKey, TValue> :
        EventArgs
    {
        #region private fields

        private readonly KeyValuePair<TKey, TValue> item;
        private bool cancel;

        #endregion

        #region constructor

        /// <summary>
        /// Initializes a new instance of <c>ObservableDictionaryEventArgs</c>.
        /// </summary>
        /// <param name="item">Item that is being added or removed from the dictionary.</param>
        public ObservableDictionaryBaseEventArgs(KeyValuePair<TKey, TValue> item)
        {
            this.item = item;
            this.cancel = false;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Get the item that is being added to or removed from the dictionary.
        /// </summary>
        public KeyValuePair<TKey, TValue> Item
        {
            get { return this.item; }
        }

        /// <summary>
        /// Gets or sets if the operation must be canceled.
        /// </summary>
        /// <remarks>This property is used by the OnBeforeAdd and OnBeforeRemove events to cancel the add or remove operations.</remarks>
        public bool Cancel
        {
            get { return this.cancel; }
            set { this.cancel = value; }
        }

        #endregion
    }

    public class ObservableDictionaryBase<TKey, TValue> :
        IDictionary<TKey, TValue>
    {
        #region delegates and events

        public delegate void AddItemEventHandler(ObservableDictionaryBase<TKey, TValue> sender, ObservableDictionaryBaseEventArgs<TKey, TValue> e);
        public delegate void BeforeAddItemEventHandler(ObservableDictionaryBase<TKey, TValue> sender, ObservableDictionaryBaseEventArgs<TKey, TValue> e);
        public delegate void RemoveItemEventHandler(ObservableDictionaryBase<TKey, TValue> sender, ObservableDictionaryBaseEventArgs<TKey, TValue> e);
        public delegate void BeforeRemoveItemEventHandler(ObservableDictionaryBase<TKey, TValue> sender, ObservableDictionaryBaseEventArgs<TKey, TValue> e);

        public event BeforeAddItemEventHandler BeforeAddItem;
        public event AddItemEventHandler AddItem;
        public event BeforeRemoveItemEventHandler BeforeRemoveItem;
        public event RemoveItemEventHandler RemoveItem;

        protected virtual bool BeforeAddItemEvent(KeyValuePair<TKey, TValue> item)
        {
            BeforeAddItemEventHandler ae = BeforeAddItem;
            if (ae != null)
            {
                ObservableDictionaryBaseEventArgs<TKey, TValue> e = new ObservableDictionaryBaseEventArgs<TKey, TValue>(item);
                ae(this, e);
                return e.Cancel;
            }
            return false;
        }

        protected virtual void AddItemEvent(KeyValuePair<TKey, TValue> item)
        {
            AddItemEventHandler ae = AddItem;
            if (ae != null)
                ae(this, new ObservableDictionaryBaseEventArgs<TKey, TValue>(item));
        }

        protected virtual bool BeforeRemoveItemEvent(KeyValuePair<TKey, TValue> item)
        {
            BeforeRemoveItemEventHandler ae = BeforeRemoveItem;
            if (ae != null)
            {
                ObservableDictionaryBaseEventArgs<TKey, TValue> e = new ObservableDictionaryBaseEventArgs<TKey, TValue>(item);
                ae(this, e);
                return e.Cancel;
            }
            return false;
        }

        protected virtual void RemoveItemEvent(KeyValuePair<TKey, TValue> item)
        {
            RemoveItemEventHandler ae = RemoveItem;
            if (ae != null)
                ae(this, new ObservableDictionaryBaseEventArgs<TKey, TValue>(item));
        }

        #endregion

        #region private fields

        private readonly Dictionary<TKey, TValue> innerDictionary;

        #endregion

        #region constructor

        public ObservableDictionaryBase()
        {
            this.innerDictionary = new Dictionary<TKey, TValue>();
        }

        public ObservableDictionaryBase(int capacity)
        {
            this.innerDictionary = new Dictionary<TKey, TValue>(capacity);
        }

        public ObservableDictionaryBase(IEqualityComparer<TKey> comparer)
        {
            this.innerDictionary = new Dictionary<TKey, TValue>(comparer);
        }

        public ObservableDictionaryBase(int capacity, IEqualityComparer<TKey> comparer)
        {
            this.innerDictionary = new Dictionary<TKey, TValue>(capacity, comparer);
        }

        #endregion

        #region public properties

        public TValue this[TKey key]
        {
            get { return this.innerDictionary[key]; }
            set
            {
                KeyValuePair<TKey, TValue> remove = new KeyValuePair<TKey, TValue>(key, this.innerDictionary[key]);
                KeyValuePair<TKey, TValue> add = new KeyValuePair<TKey, TValue>(key, value);

                if (BeforeRemoveItemEvent(remove)) return;
                if (BeforeAddItemEvent(add)) return;
                this.innerDictionary[key] = value;
                AddItemEvent(add);
                RemoveItemEvent(remove);
            }
        }

        public ICollection<TKey> Keys
        {
            get { return this.innerDictionary.Keys; }
        }

        public ICollection<TValue> Values
        {
            get { return this.innerDictionary.Values; }
        }

        public int Count
        {
            get { return this.innerDictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        #endregion

        #region public methods

        public void Add(TKey key, TValue value)
        {
            KeyValuePair<TKey, TValue> add = new KeyValuePair<TKey, TValue>(key, value);
            if (BeforeAddItemEvent(add)) return;
            this.innerDictionary.Add(key, value);
            AddItemEvent(add);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            this.Add(item.Key, item.Value);
        }

        public bool Remove(TKey key)
        {
            if (!this.innerDictionary.ContainsKey(key)) return false;

            KeyValuePair<TKey, TValue> remove = new KeyValuePair<TKey, TValue>(key, this.innerDictionary[key]);
            if (BeforeRemoveItemEvent(remove)) return false;
            this.innerDictionary.Remove(key);
            RemoveItemEvent(remove);

            return true;
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            if(!ReferenceEquals(item.Value, this.innerDictionary[item.Key])) return false;
            return this.Remove(item.Key);
        }

        public void Clear()
        {
            foreach (KeyValuePair<TKey, TValue> item in this.innerDictionary)
            {
                this.Remove(item.Key);
            }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
           return ((IDictionary<TKey, TValue>)this.innerDictionary).Contains(item);
        }

        public bool ContainsKey(TKey key)
        {
            return this.innerDictionary.ContainsKey(key);
        }

        public bool ContainsValue(TValue value)
        {
            return this.innerDictionary.ContainsValue(value);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return this.innerDictionary.TryGetValue(key, out value);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((IDictionary<TKey, TValue>)this.innerDictionary).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return this.innerDictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }


        #endregion

    }
}
