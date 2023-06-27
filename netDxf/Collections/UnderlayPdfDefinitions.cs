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
using netDxf.Objects;
using netDxf.Tables;

namespace netDxf.Collections
{
    /// <summary>
    /// Represents a collection of PDF underlay definitions.
    /// </summary>
    public sealed class UnderlayPdfDefinitions :
        TableObjects<UnderlayPdfDefinition>
    {
        #region constructor

        internal UnderlayPdfDefinitions(DxfDocument document)
            : this(document, null)
        {
        }

        internal UnderlayPdfDefinitions(DxfDocument document, string handle)
            : base(document, DxfObjectCode.UnderlayPdfDefinitionDictionary, handle)
        {
        }

        #endregion

        #region override methods

        /// <summary>
        /// Adds a PDF underlay definition to the list.
        /// </summary>
        /// <param name="underlayPdfDefinition"><see cref="UnderlayPdfDefinition">UnderlayPdfDefinition</see> to add to the list.</param>
        /// <param name="assignHandle">Specifies if a handle needs to be generated for the underlay definition parameter.</param>
        /// <returns>
        /// If an underlay definition already exists with the same name as the instance that is being added the method returns the existing underlay definition,
        /// if not it will return the new underlay definition.
        /// </returns>
        internal override UnderlayPdfDefinition Add(UnderlayPdfDefinition underlayPdfDefinition, bool assignHandle)
        {
            if (underlayPdfDefinition == null)
            {
                throw new ArgumentNullException(nameof(underlayPdfDefinition));
            }

            if (this.List.TryGetValue(underlayPdfDefinition.Name, out UnderlayPdfDefinition add))
            {
                return add;
            }

            if (assignHandle || string.IsNullOrEmpty(underlayPdfDefinition.Handle))
            {
                this.Owner.NumHandles = underlayPdfDefinition.AssignHandle(this.Owner.NumHandles);
            }

            this.List.Add(underlayPdfDefinition.Name, underlayPdfDefinition);
            this.References.Add(underlayPdfDefinition.Name, new DxfObjectReferences());

            underlayPdfDefinition.Owner = this;

            underlayPdfDefinition.NameChanged += this.Item_NameChanged;

            this.Owner.AddedObjects.Add(underlayPdfDefinition.Handle, underlayPdfDefinition);

            return underlayPdfDefinition;
        }

        /// <summary>
        /// Removes a PDF underlay definition.
        /// </summary>
        /// <param name="name"><see cref="UnderlayPdfDefinition">UnderlayPdfDefinition</see> name to remove from the document.</param>
        /// <returns>True if the underlay definition has been successfully removed, or false otherwise.</returns>
        /// <remarks>Any underlay definition referenced by objects cannot be removed.</remarks>
        public override bool Remove(string name)
        {
            return this.Remove(this[name]);
        }

        /// <summary>
        /// Removes a PDF underlay definition.
        /// </summary>
        /// <param name="item"><see cref="UnderlayPdfDefinition">UnderlayPdfDefinition</see> to remove from the document.</param>
        /// <returns>True if the underlay definition has been successfully removed, or false otherwise.</returns>
        /// <remarks>Any underlay definition referenced by objects cannot be removed.</remarks>
        public override bool Remove(UnderlayPdfDefinition item)
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

        #region TableObject events

        private void Item_NameChanged(TableObject sender, TableObjectChangedEventArgs<string> e)
        {
            if (this.Contains(e.NewValue))
            {
                throw new ArgumentException("There is already another PDF underlay definition with the same name.");
            }

            this.List.Remove(sender.Name);
            this.List.Add(e.NewValue, (UnderlayPdfDefinition) sender);

            List<DxfObjectReference> refs = this.GetReferences(sender.Name);
            this.References.Remove(sender.Name);
            this.References.Add(e.NewValue, new DxfObjectReferences());
            this.References[e.NewValue].Add(refs);
        }

        #endregion
    }
}