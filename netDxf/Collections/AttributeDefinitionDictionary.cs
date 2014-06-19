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
    /// Represents the arguments thrown by the <see cref="AttributeDefinitionDictionary">AttributeDefinitionDictionary</see> events.
    /// </summary>
    public class AttributeDefinitionDictionaryEventArgs :
        EventArgs
    {
        #region private fields

        private readonly AttributeDefinition item;
        private bool cancel;

        #endregion

        #region constructor

        /// <summary>
        /// Initializes a new instance of <c>AttributeDefinitionDictionaryEventArgs</c>.
        /// </summary>
        /// <param name="item">Item that is being added or removed from the dictionary.</param>
        public AttributeDefinitionDictionaryEventArgs(AttributeDefinition item)
        {
            this.item = item;
            this.cancel = false;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Get the item that is being added to or removed from the dictionary.
        /// </summary>
        public AttributeDefinition Item
        {
            get { return this.item; }
        }

        /// <summary>
        /// Gets or sets if the operation must be canceled.
        /// </summary>
        /// <remarks>This property is used by the BeforeAddItem and BeforeRemoveItem events to cancel the add or remove operations.</remarks>
        public bool Cancel
        {
            get { return this.cancel; }
            set { this.cancel = value; }
        }

        #endregion
    }

    public class AttributeDefinitionDictionary :
        IDictionary<string, AttributeDefinition>
    {
        #region delegates and events

        public delegate void BeforeAddItemEventHandler(AttributeDefinitionDictionary sender, AttributeDefinitionDictionaryEventArgs e);
        public delegate void AddItemEventHandler(AttributeDefinitionDictionary sender, AttributeDefinitionDictionaryEventArgs e);
        public delegate void RemoveItemEventHandler(AttributeDefinitionDictionary sender, AttributeDefinitionDictionaryEventArgs e);
        public delegate void BeforeRemoveItemEventHandler(AttributeDefinitionDictionary sender, AttributeDefinitionDictionaryEventArgs e);

        public event BeforeAddItemEventHandler BeforeAddItem;
        public event AddItemEventHandler AddItem;
        public event BeforeRemoveItemEventHandler BeforeRemoveItem;
        public event RemoveItemEventHandler RemoveItem;

        protected virtual bool OnBeforeAddItemEvent(AttributeDefinition item)
        {
            BeforeAddItemEventHandler ae = BeforeAddItem;
            if (ae != null)
            {
                AttributeDefinitionDictionaryEventArgs e = new AttributeDefinitionDictionaryEventArgs(item);
                ae(this, e);
                return e.Cancel;
            }
            return false;
        }

        protected virtual void OnAddItemEvent(AttributeDefinition item)
        {
            AddItemEventHandler ae = AddItem;
            if (ae != null)
                ae(this, new AttributeDefinitionDictionaryEventArgs(item));
        }

        protected virtual bool OnBeforeRemoveItemEvent(AttributeDefinition item)
        {
            BeforeRemoveItemEventHandler ae = BeforeRemoveItem;
            if (ae != null)
            {
                AttributeDefinitionDictionaryEventArgs e = new AttributeDefinitionDictionaryEventArgs(item);
                ae(this, e);
                return e.Cancel;
            }
            return false;
        }

        protected virtual void OnRemoveItemEvent(AttributeDefinition item)
        {
            RemoveItemEventHandler ae = RemoveItem;
            if (ae != null)
                ae(this, new AttributeDefinitionDictionaryEventArgs(item));
        }

        #endregion

        #region private fields

        private readonly Dictionary<string, AttributeDefinition> innerDictionary;

        #endregion

        #region constructor

        public AttributeDefinitionDictionary()
        {
            this.innerDictionary = new Dictionary<string, AttributeDefinition>(StringComparer.OrdinalIgnoreCase);
        }

        public AttributeDefinitionDictionary(int capacity)
        {
            this.innerDictionary = new Dictionary<string, AttributeDefinition>(capacity, StringComparer.OrdinalIgnoreCase);
        }

        #endregion

        #region public properties

        public AttributeDefinition this[string key]
        {
            get { return this.innerDictionary[key]; }
            set
            {
                AttributeDefinition remove = this.innerDictionary[key];

                if (OnBeforeRemoveItemEvent(remove)) return;
                if (OnBeforeAddItemEvent(value)) return;
                if (value == null) throw new ArgumentNullException("value");
                this.innerDictionary[key] = value;
                OnAddItemEvent(value);
                OnRemoveItemEvent(remove);
            }
        }

        public ICollection<string> Keys
        {
            get { return this.innerDictionary.Keys; }
        }

        public ICollection<AttributeDefinition> Values
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

        public bool Add(AttributeDefinition item)
        {
            if (OnBeforeAddItemEvent(item)) return false;
            if (item == null) throw new ArgumentNullException("item");
            this.innerDictionary.Add(item.Tag, item);
            OnAddItemEvent(item);
            return true;
        }

        void IDictionary<string, AttributeDefinition>.Add(string key, AttributeDefinition value)
        {
            this.Add(value);
        }

        void ICollection<KeyValuePair<string, AttributeDefinition>>.Add(KeyValuePair<string, AttributeDefinition> item)
        {
            this.Add(item.Value);
        }

        public bool Remove(string key)
        {
            AttributeDefinition remove;
            if (!this.innerDictionary.TryGetValue(key, out remove)) return false;
            if (OnBeforeRemoveItemEvent(remove)) return false;
            this.innerDictionary.Remove(key);
            OnRemoveItemEvent(remove);
            return true;
        }

        bool ICollection<KeyValuePair<string, AttributeDefinition>>.Remove(KeyValuePair<string, AttributeDefinition> item)
        {
            if(!ReferenceEquals(item.Value, this.innerDictionary[item.Key])) return false;
            return this.Remove(item.Key);
        }

        public void Clear()
        {
            foreach (KeyValuePair<string, AttributeDefinition> item in this.innerDictionary)
            {
                this.Remove(item.Key);
            }
        }

        bool ICollection<KeyValuePair<string, AttributeDefinition>>.Contains(KeyValuePair<string, AttributeDefinition> item)
        {
            return ((IDictionary<string, AttributeDefinition>)this.innerDictionary).Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return this.innerDictionary.ContainsKey(key);
        }

        public bool ContainsValue(AttributeDefinition value)
        {
            return this.innerDictionary.ContainsValue(value);
        }

        public bool TryGetValue(string key, out AttributeDefinition value)
        {
            return this.innerDictionary.TryGetValue(key, out value);
        }

        void ICollection<KeyValuePair<string, AttributeDefinition>>.CopyTo(KeyValuePair<string, AttributeDefinition>[] array, int arrayIndex)
        {
            ((IDictionary<string, AttributeDefinition>)this.innerDictionary).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, AttributeDefinition>> GetEnumerator()
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
