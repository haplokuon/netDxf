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
using netDxf.Tables;

namespace netDxf.Collections
{
    /// <summary>
    /// Represents a collection of application registries.
    /// </summary>
    public sealed class ApplicationRegistries :
        TableObjects<ApplicationRegistry>
    {

        #region constructor

        internal ApplicationRegistries(DxfDocument document)
            : base(document)
        {
        }

        internal ApplicationRegistries(DxfDocument document, Dictionary<string, ApplicationRegistry> list, Dictionary<string, List<DxfObject>> references)
            : base(document, list, references)
        {
        }

        #endregion

        #region override methods

        /// <summary>
        /// Adds an application registry to the list.
        /// </summary>
        /// <param name="appReg"><see cref="ApplicationRegistry">ApplicationRegistry</see> to add to the list.</param>
        /// <returns>
        /// If a layer already exists with the same name as the instance that is being added the method returns the existing layer,
        /// if not it will return the new layer.
        /// </returns>
        public override ApplicationRegistry Add(ApplicationRegistry appReg)
        {
            ApplicationRegistry add;
            if (this.list.TryGetValue(appReg.Name, out add))
                return add;

            this.document.NumHandles = appReg.AsignHandle(this.document.NumHandles);
            this.list.Add(appReg.Name, appReg);
            this.references.Add(appReg.Name, new List<DxfObject>());
            return appReg;
        }

        /// <summary>
        /// Removes an application registry.
        /// </summary>
        /// <param name="name"><see cref="ApplicationRegistry">ApplicationRegistry</see> name to remove from the document.</param>
        /// <returns>True is the application registry has been successfully removed, or false otherwise.</returns>
        /// <remarks>Reserved application registries or any other referenced by objects cannot be removed.</remarks>
        public override bool Remove(string name)
        {
            ApplicationRegistry appReg = this[name];

            if (appReg == null)
                return false;

            if (appReg.IsReserved)
                return false;

            if (this.references[appReg.Name].Count != 0)
                return false;

            this.references.Remove(appReg.Name);
            return this.list.Remove(appReg.Name);

        }

        /// <summary>
        /// Removes an application registry.
        /// </summary>
        /// <param name="appReg"><see cref="ApplicationRegistry">ApplicationRegistry</see> to remove from the document.</param>
        /// <returns>True is the application registry has been successfully removed, or false otherwise.</returns>
        /// <remarks>Reserved application registries or any other referenced by objects cannot be removed.</remarks>
        public override bool Remove(ApplicationRegistry appReg)
        {
            return Remove(appReg.Name);
        }

        #endregion

    }
}