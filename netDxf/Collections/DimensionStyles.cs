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
using System.Collections.Generic;
using netDxf.Tables;

namespace netDxf.Collections
{
    /// <summary>
    /// Represents a collection of dimension styles.
    /// </summary>
    public sealed class DimensionStyles :
        TableObjects<DimensionStyle>
    {

        #region constructor

        internal DimensionStyles(DxfDocument document, string handle = null)
            : base(document,
            new Dictionary<string, DimensionStyle>(StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, List<DxfObject>>(StringComparer.OrdinalIgnoreCase),
            StringCode.DimensionStyleTable,
            handle)
        {
        }

        #endregion

        #region override methods

        /// <summary>
        /// Adds a dimension style to the list.
        /// </summary>
        /// <param name="style"><see cref="DimensionStyle">DimensionStyle</see> to add to the list.</param>
        /// <returns>
        /// If a dimension style already exists with the same name as the instance that is being added the method returns the existing dimension style,
        /// if not it will return the new dimension style.
        /// </returns>
        internal override DimensionStyle Add(DimensionStyle style, bool assignHandle)
        {
            DimensionStyle add;
            if (this.list.TryGetValue(style.Name, out add))
                return add;

            if(assignHandle)
                this.document.NumHandles = style.AsignHandle(this.document.NumHandles);

            this.document.AddedObjects.Add(style.Handle, style);
            this.list.Add(style.Name, style);
            this.references.Add(style.Name, new List<DxfObject>());
            style.TextStyle = this.document.TextStyles.Add(style.TextStyle);
            this.document.TextStyles.References[style.TextStyle.Name].Add(style);
            style.Owner = this;
            return style;
        }

        /// <summary>
        /// Removes a dimension style.
        /// </summary>
        /// <param name="name"><see cref="DimensionStyle">DimensionStyle</see> name to remove from the document.</param>
        /// <returns>True is the dimension style has been successfully removed, or false otherwise.</returns>
        /// <remarks>Reserved dimension styles or any other referenced by objects cannot be removed.</remarks>
        public override bool Remove(string name)
        {
            return Remove(this[name]);
        }

        /// <summary>
        /// Removes a dimension style.
        /// </summary>
        /// <param name="style"><see cref="DimensionStyle">DimensionStyle</see> to remove from the document.</param>
        /// <returns>True is the dimension style has been successfully removed, or false otherwise.</returns>
        /// <remarks>Reserved dimension styles or any other referenced by objects cannot be removed.</remarks>
        public override bool Remove(DimensionStyle style)
        {
            if (style == null)
                return false;

            if (!this.Contains(style))
                return false;

            if (style.IsReserved)
                return false;

            if (this.references[style.Name].Count != 0)
                return false;

            style.Owner = null;
            this.document.TextStyles.References[style.TextStyle.Name].Remove(style);
            this.document.AddedObjects.Remove(style.Handle);
            this.references.Remove(style.Name);
            this.list.Remove(style.Name);

            return true;

        }

        #endregion

    }
}