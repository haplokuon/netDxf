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
}