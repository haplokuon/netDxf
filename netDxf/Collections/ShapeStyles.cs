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
using System.Collections.Generic;
using netDxf.Tables;

namespace netDxf.Collections
{
    /// <summary>
    /// Represents a collection of shape styles.
    /// </summary>
    public class ShapeStyles :
        TableObjects<ShapeStyle>
    {
        #region constructor

        internal ShapeStyles(DxfDocument document)
            : this(document, null)
        {
        }

        internal ShapeStyles(DxfDocument document, string handle)
            : base(document, DxfObjectCode.TextStyleTable, handle)
        {
        }

        #endregion

        #region public methods

        /// <summary>
        /// Looks for a shape style that contains a shape with the specified name.
        /// </summary>
        /// <param name="name">Shape name.</param>
        /// <returns>The shape style that contains a shape with the specified name, null otherwise.</returns>
        public ShapeStyle ContainsShapeName(string name)
        {
            foreach (ShapeStyle style in this.Items)
            {
                if (style.ContainsShapeName(name)) return style;
            }
            // there are no shape styles in the list that contain a shape with the specified name
            return null;
        }

        #endregion

        #region override methods

        /// <summary>
        /// Adds a shape style to the list.
        /// </summary>
        /// <param name="style"><see cref="ShapeStyle">ShapeStyle</see> to add to the list.</param>
        /// <param name="assignHandle">Specifies if a handle needs to be generated for the shape style parameter.</param>
        /// <returns>
        /// If a shape style already exists with the same name as the instance that is being added the method returns the existing shape style,
        /// if not it will return the new text style.
        /// </returns>
        internal override ShapeStyle Add(ShapeStyle style, bool assignHandle)
        {
            if (style == null)
            {
                throw new ArgumentNullException(nameof(style));
            }

            if (this.List.TryGetValue(style.Name, out ShapeStyle add))
            {
                return add;
            }

            if (assignHandle || string.IsNullOrEmpty(style.Handle))
            {
                this.Owner.NumHandles = style.AssignHandle(this.Owner.NumHandles);
            }

            this.List.Add(style.Name, style);
            this.References.Add(style.Name, new DxfObjectReferences());

            style.Owner = this;

            style.NameChanged += this.Item_NameChanged;

            this.Owner.AddedObjects.Add(style.Handle, style);

            return style;
        }

        /// <summary>
        /// Removes a shape style.
        /// </summary>
        /// <param name="name"><see cref="ShapeStyle">ShapeStyle</see> name to remove from the document.</param>
        /// <returns>True if the shape style has been successfully removed, or false otherwise.</returns>
        /// <remarks>Reserved shape styles or any other referenced by objects cannot be removed.</remarks>
        public override bool Remove(string name)
        {
            return this.Remove(this[name]);
        }

        /// <summary>
        /// Removes a shape style.
        /// </summary>
        /// <param name="item"><see cref="ShapeStyle">ShapeStyle</see> to remove from the document.</param>
        /// <returns>True if the shape style has been successfully removed, or false otherwise.</returns>
        /// <remarks>Reserved shape styles or any other referenced by objects cannot be removed.</remarks>
        public override bool Remove(ShapeStyle item)
        {
            if (item == null)
            {
                return false;
            }

            if (!this.Contains(item))
            {
                return false;
            }

            if (item.IsReserved)
            {
                return false;
            }

            if (this.HasReferences(item))
            {
                return false;
            }

            this.Owner.AddedObjects.Remove(item.Handle);
            this.References.Remove(item.Name);
            this.List.Remove(item.Name);

            item.Handle = null;
            item.Owner = null;

            item.NameChanged -= this.Item_NameChanged;

            return true;
        }

        #endregion

        #region ShapeStyle events

        private void Item_NameChanged(TableObject sender, TableObjectChangedEventArgs<string> e)
        {
            if (this.Contains(e.NewValue))
            {
                throw new ArgumentException("There is already another shape style with the same name.");
            }

            this.List.Remove(sender.Name);
            this.List.Add(e.NewValue, (ShapeStyle)sender);

            List<DxfObjectReference> refs = this.GetReferences(sender.Name);
            this.References.Remove(sender.Name);
            this.References.Add(e.NewValue, new DxfObjectReferences());
            this.References[e.NewValue].Add(refs);
        }

        #endregion
    }
}