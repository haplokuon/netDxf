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
using System.Collections.Generic;
using netDxf.Objects;
using netDxf.Tables;

namespace netDxf.Collections
{
    /// <summary>
    /// Represents a collection of image definitions.
    /// </summary>
    public sealed class ImageDefinitions :
        TableObjects<ImageDef>
    {
        #region constructor

        internal ImageDefinitions(DxfDocument document, string handle = null)
            : this(document,0,handle)
        {
        }

        internal ImageDefinitions(DxfDocument document, int capacity, string handle = null)
            : base(document,
            new Dictionary<string, ImageDef>(capacity, StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, List<DxfObject>>(capacity, StringComparer.OrdinalIgnoreCase),
            DxfObjectCode.ImageDefDictionary,
            handle)
        {
            this.maxCapacity = int.MaxValue;
        }

        #endregion

        #region override methods

        /// <summary>
        /// Adds an image definition to the list.
        /// </summary>
        /// <param name="imageDef"><see cref="ImageDef">ImageDef</see> to add to the list.</param>
        /// <param name="assignHandle">Specifies if a handle needs to be generated for the image definition parameter.</param>
        /// <returns>
        /// If an image definition already exists with the same name as the instance that is being added the method returns the existing image definition,
        /// if not it will return the new image definition.
        /// </returns>
        internal override ImageDef Add(ImageDef imageDef, bool assignHandle)
        {
            if (this.list.Count >= this.maxCapacity)
                throw new OverflowException(string.Format("Table overflow. The maximum number of elements the table {0} can have is {1}", this.codeName, this.maxCapacity));

            ImageDef add;
            if (this.list.TryGetValue(imageDef.Name, out add))
                return add;

            if (assignHandle || string.IsNullOrEmpty(imageDef.Handle))
                this.document.NumHandles = imageDef.AsignHandle(this.document.NumHandles);

            this.document.AddedObjects.Add(imageDef.Handle, imageDef);
            this.list.Add(imageDef.Name, imageDef);
            this.references.Add(imageDef.Name, new List<DxfObject>());

            imageDef.Owner = this;

            imageDef.NameChange += this.Item_NameChange;

            return imageDef;
        }

        /// <summary>
        /// Removes an image definition.
        /// </summary>
        /// <param name="name"><see cref="ImageDef">ImageDef</see> name to remove from the document.</param>
        /// <returns>True if the image definition has been successfully removed, or false otherwise.</returns>
        /// <remarks>Any image definition referenced by objects cannot be removed.</remarks>
        public override bool Remove(string name)
        {
            return this.Remove(this[name]);
        }

        /// <summary>
        /// Removes an image definition.
        /// </summary>
        /// <param name="imageDef"><see cref="ImageDef">ImageDef</see> to remove from the document.</param>
        /// <returns>True if the image definition has been successfully removed, or false otherwise.</returns>
        /// <remarks>Any image definition referenced by objects cannot be removed.</remarks>
        public override bool Remove(ImageDef imageDef)
        {
            if (imageDef == null)
                return false;

            if (!this.Contains(imageDef))
                return false;

            if (imageDef.IsReserved)
                return false;

            if (this.references[imageDef.Name].Count != 0)
                return false;

            this.document.AddedObjects.Remove(imageDef.Handle);
            this.references.Remove(imageDef.Name);
            this.list.Remove(imageDef.Name);

            imageDef.Handle = null;
            imageDef.Owner = null;

            imageDef.NameChange -= this.Item_NameChange;

            return true;
        }

        #endregion

        #region TableObject events

        private void Item_NameChange(TableObject sender, TableObjectChangeEventArgs<string> e)
        {
            if (this.Contains(e.NewValue))
                throw new ArgumentException("There is already another image definition with the same name.");

            this.list.Remove(sender.Name);
            this.list.Add(e.NewValue, (ImageDef)sender);

            List<DxfObject> refs = this.references[sender.Name];
            this.references.Remove(sender.Name);
            this.references.Add(e.NewValue, refs);
        }

        #endregion
    }
}