#region netDxf, Copyright(C) 2013 Daniel Carvajal, Licensed under LGPL.

//                        netDxf library
// Copyright (C) 2013 Daniel Carvajal (haplokuon@gmail.com)
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

using System.Collections.Generic;
using netDxf.Objects;

namespace netDxf.Collections
{
    /// <summary>
    /// Represents a collection of image definitions.
    /// </summary>
    public sealed class ImageDefinitions :
        TableObjects<ImageDef>
    {

        #region constructor

        internal ImageDefinitions(DxfDocument document)
            : base(document)
        {
        }

        internal ImageDefinitions(DxfDocument document, Dictionary<string, ImageDef> list, Dictionary<string, List<DxfObject>> references)
            : base(document, list, references)
        {
        }

        #endregion

        #region override methods

        /// <summary>
        /// Adds an image definition to the list.
        /// </summary>
        /// <param name="imageDef"><see cref="ImageDef">ImageDef</see> to add to the list.</param>
        /// <returns>
        /// If an image definition already exists with the same name as the instance that is being added the method returns the existing image definition,
        /// if not it will return the new image definition.
        /// </returns>
        public override ImageDef Add(ImageDef imageDef)
        {
            ImageDef add;
            if (this.list.TryGetValue(imageDef.Name, out add))
                return add;

            this.document.NumHandles = imageDef.AsignHandle(this.document.NumHandles);
            this.list.Add(imageDef.Name, imageDef);
            this.references.Add(imageDef.Name, new List<DxfObject>());
            return imageDef;
        }

        /// <summary>
        /// Removes an image definition.
        /// </summary>
        /// <param name="name"><see cref="ImageDef">ImageDef</see> name to remove from the document.</param>
        /// <returns>True is the image definition has been successfully removed, or false otherwise.</returns>
        /// <remarks>Any image definition referenced by objects cannot be removed.</remarks>
        public override bool Remove(string name)
        {
            ImageDef imageDef = this[name];

            if (imageDef == null)
                return false;

            if (this.references[imageDef.Name].Count != 0)
                return false;

            this.references.Remove(imageDef.Name);
            return this.list.Remove(imageDef.Name);

        }

        /// <summary>
        /// Removes an image definition.
        /// </summary>
        /// <param name="imageDef"><see cref="ImageDef">ImageDef</see> to remove from the document.</param>
        /// <returns>True is the image definition has been successfully removed, or false otherwise.</returns>
        /// <remarks>Any image definition referenced by objects cannot be removed.</remarks>
        public override bool Remove(ImageDef imageDef)
        {
            return Remove(imageDef.Name);
        }

        #endregion

    }
}