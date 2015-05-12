#region netDxf, Copyright(C) 2015 Daniel Carvajal, Licensed under LGPL.
// 
//                         netDxf library
//  Copyright (C) 2009-2015 Daniel Carvajal (haplokuon@gmail.com)
//  
//  This library is free software; you can redistribute it and/or
//  modify it under the terms of the GNU Lesser General Public
//  License as published by the Free Software Foundation; either
//  version 2.1 of the License, or (at your option) any later version.
//  
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
//  
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
//  FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
//  COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
//  IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
//  CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using Attribute = netDxf.Entities.Attribute;

namespace netDxf.Collections
{
    /// <summary>
    /// Represents a collection of <see cref="Entities.Attribute">Attributes</see>.
    /// </summary>
    public class AttributeDictionary :
        IDictionary<string, Attribute>
    {
        #region private fields

        private readonly Dictionary<string, Attribute> innerDictionary;

        #endregion

        #region constructor

        /// <summary>
        /// Initializes a new instance of <c>AttributeDictionary</c>.
        /// </summary>
        public AttributeDictionary()
        {
            this.innerDictionary = new Dictionary<string, Attribute>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Initializes a new instance of <c>AttributeDictionary</c> with the specified collection of attributes.
        /// </summary>
        /// <param name="attributes">The collection of attributes from which build the dictionary.</param>
        public AttributeDictionary(ICollection attributes)
        {
            if (attributes == null)
                throw new ArgumentNullException("attributes");

            this.innerDictionary = new Dictionary<string, Attribute>(attributes.Count, StringComparer.OrdinalIgnoreCase);
            foreach (Attribute item in attributes)
            {
                // netDxf does not support multiple attributes with the same tag
                if(this.innerDictionary.ContainsKey(item.Tag)) continue;
                this.innerDictionary.Add(item.Tag, item);
            }
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the attribute associated with the specified key.
        /// </summary>
        /// <param name="tag"> The tag of the attribute to get.</param>
        /// <returns>The attribute associated with the specified tag.</returns>
        public Attribute this[string tag]
        {
            get { return this.innerDictionary[tag]; }
        }

        /// <summary>
        /// Gets a collection containing the attributes in the dictionary.
        /// </summary>
        public ICollection<Attribute> Values
        {
            get { return this.innerDictionary.Values; }
        }

        /// <summary>
        /// Gets an ICollection containing the tags of the current dictionary.
        /// </summary>
        public ICollection<string> Tags
        {
            get { return this.innerDictionary.Keys; }
        }

        /// <summary>
        /// Gets the number of tag/attribute pairs contained in the dictionary.
        /// </summary>
        public int Count
        {
            get { return this.innerDictionary.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether dictionary is read-only. It always returns true.
        /// </summary>
        public bool IsReadOnly
        {
            get { return true; }
        }

        #endregion

        #region private properties

        Attribute IDictionary<string, Attribute>.this[string tag]
        {
            get { return this.innerDictionary[tag]; }
            set { throw new FieldAccessException("The attribute dictionary is for read only purposes."); }
        }

        ICollection<string> IDictionary<string, Attribute>.Keys
        {
            get { return this.innerDictionary.Keys; }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Determines whether the dictionary contains an attribute with the specified tag.
        /// </summary>
        /// <param name="tag">The tag to locate in the dictionary.</param>
        /// <returns>True if the dictionary contains an attribute with the tag; otherwise, false.</returns>
        public bool ContainsTag(string tag)
        {
            return this.innerDictionary.ContainsKey(tag);
        }

        /// <summary>
        /// Determines whether the dictionary contains a specific attribute.
        /// </summary>
        /// <param name="attribute">The attribute to locate in the dictionary.</param>
        /// <returns>True if the dictionary contains an attribute with the specified value; otherwise, false.</returns>
        public bool ContainsValue(Attribute attribute)
        {
            return this.innerDictionary.ContainsValue(attribute);
        }

        /// <summary>
        /// Gets the attribute associated with the specified tag.
        /// </summary>
        /// <param name="tag">The key of the value to tag.</param>
        /// <param name="attribute">When this method returns, contains the attribute associated with the specified tag, if the tag is found; otherwise, null. This parameter is passed uninitialized.</param>
        /// <returns>True if the dictionary contains an attribute with the specified tag; otherwise, false.</returns>
        public bool TryGetValue(string tag, out Attribute attribute)
        {
            return this.innerDictionary.TryGetValue(tag, out attribute);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the dictionary.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the dictionary.</returns>
        public IEnumerator<KeyValuePair<string, Attribute>> GetEnumerator()
        {
            return this.innerDictionary.GetEnumerator();
        }

        #endregion

        #region private methods

        bool IDictionary<string, Attribute>.ContainsKey(string tag)
        {
            return this.innerDictionary.ContainsKey(tag);
        }

        void IDictionary<string, Attribute>.Add(string key, Attribute value)
        {
            throw new FieldAccessException("The attribute dictionary is for read only purposes.");
        }

        void ICollection<KeyValuePair<string, Attribute>>.Add(KeyValuePair<string, Attribute> item)
        {
            throw new FieldAccessException("The attribute dictionary is for read only purposes.");
        }

        bool IDictionary<string, Attribute>.Remove(string key)
        {
            throw new FieldAccessException("The attribute dictionary is for read only purposes.");
        }

        bool ICollection<KeyValuePair<string, Attribute>>.Remove(KeyValuePair<string, Attribute> item)
        {
            throw new FieldAccessException("The attribute dictionary is for read only purposes.");
        }

        void ICollection<KeyValuePair<string, Attribute>>.Clear()
        {
            throw new FieldAccessException("The attribute dictionary is for read only purposes.");
        }

        bool ICollection<KeyValuePair<string, Attribute>>.Contains(KeyValuePair<string, Attribute> item)
        {
            return ((IDictionary<string, Attribute>)this.innerDictionary).Contains(item);
        }

        void ICollection<KeyValuePair<string, Attribute>>.CopyTo(KeyValuePair<string, Attribute>[] array, int arrayIndex)
        {
            ((IDictionary<string, Attribute>)this.innerDictionary).CopyTo(array, arrayIndex);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
}
