#region netDxf library licensed under the MIT License, Copyright © 2009-2021 Daniel Carvajal (haplokuon@gmail.com)
// 
//                        netDxf library
// Copyright © 2021 Daniel Carvajal (haplokuon@gmail.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software
// and associated documentation files (the “Software”), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;

namespace netDxf.Tables
{
    /// <summary>
    /// Event data for changes or substitutions of table objects in entities or other tables.
    /// </summary>
    /// <typeparam name="T">A table object</typeparam>
    public class TableObjectChangedEventArgs<T> :
        EventArgs
    {
        #region private fields

        private readonly T oldValue;
        private T newValue;

        #endregion

        #region constructor

        /// <summary>
        /// Initializes a new instance of <c>TableObjectModifiedEventArgs</c>.
        /// </summary>
        /// <param name="oldTable">The previous table object.</param>
        /// <param name="newTable">The new table object.</param>
        public TableObjectChangedEventArgs(T oldTable, T newTable)
        {
            this.oldValue = oldTable;
            this.newValue = newTable;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the previous property value.
        /// </summary>
        public T OldValue
        {
            get { return this.oldValue; }
        }

        /// <summary>
        /// Gets or sets the new property value.
        /// </summary>
        public T NewValue
        {
            get { return this.newValue; }
            set { this.newValue = value; }
        }

        #endregion
    }
}