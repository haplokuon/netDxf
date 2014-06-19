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
using netDxf.Objects;

namespace netDxf.Collections
{
    /// <summary>
    /// Represents a collection of multiline styles.
    /// </summary>
    public sealed class MLineStyles :
        TableObjects<MLineStyle>
    {

        #region constructor

        internal MLineStyles(DxfDocument document, string handle = null)
            : base(document,
            new Dictionary<string, MLineStyle>(StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, List<DxfObject>>(StringComparer.OrdinalIgnoreCase),
            StringCode.MLineStyleDictionary,
            handle)
        {
        }

        #endregion

        #region override methods

        /// <summary>
        /// Adds a multiline style to the list.
        /// </summary>
        /// <param name="style"><see cref="MLineStyle">MLineStyle</see> to add to the list.</param>
        /// <returns>
        /// If a multiline style already exists with the same name as the instance that is being added the method returns the existing multiline style,
        /// if not it will return the new multiline style.
        /// </returns>
        internal override MLineStyle Add(MLineStyle style, bool assignHandle)
        {

            MLineStyle add;
            if (this.list.TryGetValue(style.Name, out add))
                return add;

            if (assignHandle)
                this.document.NumHandles = style.AsignHandle(this.document.NumHandles);

            this.document.AddedObjects.Add(style.Handle, style);

            this.list.Add(style.Name, style);
            this.references.Add(style.Name, new List<DxfObject>());
            foreach (MLineStyleElement element in style.Elements)
            {
                element.LineType = this.document.LineTypes.Add(element.LineType);
                this.document.LineTypes.References[element.LineType.Name].Add(style);
            }
            style.Owner = this;
            return style;
        }

        /// <summary>
        /// Removes a multiline style.
        /// </summary>
        /// <param name="name"><see cref="MLineStyle">MLineStyle</see> name to remove from the document.</param>
        /// <returns>True is the multiline style has been successfully removed, or false otherwise.</returns>
        /// <remarks>Reserved multiline styles or any other referenced by objects cannot be removed.</remarks>
        public override bool Remove(string name)
        {
            return Remove(this[name]);
        }

        /// <summary>
        /// Removes a multiline style.
        /// </summary>
        /// <param name="style"><see cref="MLineStyle">MLineStyle</see> to remove from the document.</param>
        /// <returns>True is the multiline style has been successfully removed, or false otherwise.</returns>
        /// <remarks>Reserved multiline styles or any other referenced by objects cannot be removed.</remarks>
        public override bool Remove(MLineStyle style)
        {
            if (style == null)
                return false;

            if (!this.Contains(style))
                return false;

            if (style.IsReserved)
                return false;

            if (this.references[style.Name].Count != 0)
                return false;

            foreach (MLineStyleElement element in style.Elements)
            {
                this.document.LineTypes.References[element.LineType.Name].Remove(style);
            }

            style.Owner = null;
            this.document.AddedObjects.Remove(style.Handle);
            this.references.Remove(style.Name);
            this.list.Remove(style.Name);

            return true;
        }

        #endregion

    }
}