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
using netDxf.Collections;

namespace netDxf.Tables
{
    /// <summary>
    /// Represents a registered application name to which the <see cref="XData">extended data</see> is associated.
    /// </summary>
    /// <remarks>
    /// Do not use the default "ACAD" application registry name for your own extended data, it is sometimes used by AutoCad to store internal data.
    /// Instead, create your own application registry name and store your extended data there.
    /// </remarks>
    public class ApplicationRegistry :
        TableObject
    {
        #region constants

        /// <summary>
        /// Default application registry name.
        /// </summary>
        public const string DefaultName = "ACAD";

        /// <summary>
        /// Gets the default application registry.
        /// </summary>
        public static ApplicationRegistry Default
        {
            get { return new ApplicationRegistry(DefaultName); }
        }

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>ApplicationRegistry</c> class.
        /// </summary>
        /// <param name="name">Application registry name.</param>
        public ApplicationRegistry(string name)
            : this(name, true)
        {
        }

        internal ApplicationRegistry(string name, bool checkName)
            : base(name, DxfObjectCode.AppId, checkName)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name), "The application registry name should be at least one character long.");
            }

            // the DXF official documentation says that the application registry name cannot have more than 31 characters
            // but doesn't seem to hold true anymore. When this limitation was lifted I ignore it, if it was ever true.
            //if (name.Length > 31)
            //{
            //    throw new ArgumentOutOfRangeException(nameof(name), "The application registry name cannot have more than 31 characters.");
            //}

            this.IsReserved = name.Equals(DefaultName, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the owner of the actual DXF object.
        /// </summary>
        public new ApplicationRegistries Owner
        {
            get { return (ApplicationRegistries) base.Owner; }
            internal set { base.Owner = value; }
        }

        #endregion

        #region overrides

        /// <summary>
        /// Checks if this instance has been referenced by other DxfObjects. 
        /// </summary>
        /// <returns>
        /// Returns true if this instance has been referenced by other DxfObjects, false otherwise.
        /// It will always return false if this instance does not belong to a document.
        /// </returns>
        /// <remarks>
        /// This method returns the same value as the HasReferences method that can be found in the TableObjects class.
        /// </remarks>
        public override bool HasReferences()
        {
            return this.Owner != null && this.Owner.HasReferences(this.Name);
        }

        /// <summary>
        /// Gets the list of DxfObjects referenced by this instance.
        /// </summary>
        /// <returns>
        /// A list of DxfObjectReference that contains the DxfObject referenced by this instance and the number of times it does.
        /// It will return null if this instance does not belong to a document.
        /// </returns>
        /// <remarks>
        /// This method returns the same list as the GetReferences method that can be found in the TableObjects class.
        /// </remarks>
        public override List<DxfObjectReference> GetReferences()
        {
            return this.Owner?.GetReferences(this.Name);
        }

        /// <summary>
        /// Creates a new ApplicationRegistry that is a copy of the current instance.
        /// </summary>
        /// <param name="newName">ApplicationRegistry name of the copy.</param>
        /// <returns>A new ApplicationRegistry that is a copy of this instance.</returns>
        public override TableObject Clone(string newName)
        {
            // container to temporary store application registries that has already been cloned,
            // this will handle possible circular references inside de extended data structure
            Dictionary<string, ApplicationRegistry> cloned = new Dictionary<string, ApplicationRegistry>();
            return CloneApplicationRegistry(this, ref cloned) ;
        }

        private static ApplicationRegistry CloneApplicationRegistry(ApplicationRegistry appReg, ref Dictionary<string, ApplicationRegistry> cloned)
        {
            if (!cloned.TryGetValue(appReg.Name, out ApplicationRegistry copy))
            {
                copy = new ApplicationRegistry(appReg.Name);
                cloned.Add(copy.Name, copy);
                
                foreach (XData data in appReg.XData.Values)
                {
                    ApplicationRegistry xdataAppReg = CloneApplicationRegistry(data.ApplicationRegistry, ref cloned);
                    XData xdataCopy = new XData(xdataAppReg);
                    foreach (XDataRecord record in data.XDataRecord)
                    {
                        xdataCopy.XDataRecord.Add(new XDataRecord(record.Code, record.Value));
                    }
                    copy.XData.Add(xdataCopy);
                }
            }

            return copy;
        }
        
        /// <summary>
        /// Creates a new ApplicationRegistry that is a copy of the current instance.
        /// </summary>
        /// <returns>A new ApplicationRegistry that is a copy of this instance.</returns>
        public override object Clone()
        {
            return this.Clone(this.Name);
        }

        #endregion
    }
}