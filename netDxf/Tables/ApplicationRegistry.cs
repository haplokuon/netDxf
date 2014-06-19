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
using netDxf.Collections;

namespace netDxf.Tables
{

    /// <summary>
    /// Represents a registered application name to which the <see cref="netDxf.XData">extended data</see> is associated.
    /// </summary>
    public class ApplicationRegistry :
        TableObject
    {
        #region private fields

        private static readonly ApplicationRegistry defaultApp;

        #endregion

        #region constants

        /// <summary>
        /// Gets the default application registry.
        /// </summary>
        internal static ApplicationRegistry Default
        {
            get { return defaultApp; }
        }

        #endregion

        #region constructors

        static ApplicationRegistry()
        {
            defaultApp = new ApplicationRegistry("ACAD");
        }

        /// <summary>
        /// Initializes a new instance of the <c>ApplicationRegistry</c> class.
        /// </summary>
        /// <param name="name">Layer name.</param>
        public ApplicationRegistry(string name)
            : base(name, DxfObjectCode.AppId, true)
        {
            this.reserved = name.Equals("ACAD", StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the owner of the actual dxf object.
        /// </summary>
        public new ApplicationRegistries Owner
        {
            get { return (ApplicationRegistries)this.owner; }
            internal set { this.owner = value; }
        }

        #endregion

        #region overrides

        /// <summary>
        /// Determines whether the specified <see cref="netDxf.Tables.ApplicationRegistry" /> is equal to the current <see cref="netDxf.Tables.ApplicationRegistry" />.
        /// </summary>
        /// <returns>
        /// True if the specified <see cref="netDxf.Tables.ApplicationRegistry" /> is equal to the current <see cref="netDxf.Tables.ApplicationRegistry" />; otherwise, false.
        /// </returns>
        /// <remarks>Two <see cref="netDxf.Tables.ApplicationRegistry" /> instances are equal if their names are equal.</remarks>
        /// <exception cref="T:System.NullReferenceException">
        /// The <paramref name="obj" /> parameter is null.
        /// </exception>
        public bool Equals(ApplicationRegistry obj)
        {
            return obj.Name == this.Name;
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />.
        /// </summary>
        /// <returns>
        /// True if the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />; otherwise, false.
        /// </returns>
        /// <param name="obj"> The <see cref="T:System.Object" /> to compare with the current <see cref="T:System.Object" />.</param>
        /// <exception cref="T:System.NullReferenceException">
        /// The <paramref name="obj" /> parameter is null.
        /// </exception>
        public override bool Equals(object obj)
        {
            if (obj is ApplicationRegistry)
                return this.Equals((ApplicationRegistry) obj);
            return false;
        }

        ///<summary>
        /// Serves as a hash function for a particular type. 
        ///</summary>
        ///<returns>
        /// A hash code for the current <see cref="T:System.Object" />.
        ///</returns>
        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        #endregion
    }
}