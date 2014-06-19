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

namespace netDxf.Header
{
    /// <summary>
    /// Defines a header variable.
    /// </summary>
    public class HeaderVariable
    {
        #region private fields

        private readonly string name;
        private readonly int codeGroup;
        private object value;

        #endregion

        #region constants

        public static readonly Dictionary<string, int> Allowed = HeaderVariablesCodeGroup();

        #endregion

        #region constructors

        public HeaderVariable(string name, object value)
        {
            if (!Allowed.ContainsKey(name))
                throw new ArgumentOutOfRangeException("name", name, string.Format("Variable name {0} not defined.", name));
            this.codeGroup = Allowed[name];
            this.name = name;
            this.value = value;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the header variable name.
        /// </summary>
        public string Name
        {
            get { return this.name; }
        }

        /// <summary>
        /// Gets the header variable code group.
        /// </summary>
        public int CodeGroup
        {
            get { return this.codeGroup; }
        }

        /// <summary>
        /// Gets the header variable stored value.
        /// </summary>
        public object Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        #endregion

        #region overrides

        public override string ToString()
        {
            return String.Format("{0}:{1}", this.name, this.value);
        }

        #endregion

        #region private methods

        private static Dictionary<string, int> HeaderVariablesCodeGroup()
        {
            return new Dictionary<string, int>
                       {
                           {HeaderVariableCode.AcadVer, 1},
                           {HeaderVariableCode.HandleSeed, 5},
                           {HeaderVariableCode.Angbase, 50},
                           {HeaderVariableCode.Angdir, 70},
                           {HeaderVariableCode.AttMode, 70},
                           {HeaderVariableCode.AUnits, 70},
                           {HeaderVariableCode.AUprec, 70},
                           {HeaderVariableCode.LUnits, 70},
                           {HeaderVariableCode.LUprec, 70},
                           {HeaderVariableCode.CeColor, 62},
                           {HeaderVariableCode.CeLtScale, 40},
                           {HeaderVariableCode.CeLtype, 6},
                           {HeaderVariableCode.CeLweight, 370},
                           {HeaderVariableCode.CLayer, 8},
                           {HeaderVariableCode.CMLJust, 70},
                           {HeaderVariableCode.CMLScale, 40},
                           {HeaderVariableCode.CMLStyle, 2},
                           {HeaderVariableCode.DimStyle, 2},
                           {HeaderVariableCode.TextSize, 40},
                           {HeaderVariableCode.TextStyle, 7},
                           {HeaderVariableCode.DwgCodePage, 3},
                           {HeaderVariableCode.Extnames, 290},
                           {HeaderVariableCode.InsUnits, 70},
                           {HeaderVariableCode.LastSavedBy, 1},
                           {HeaderVariableCode.LtScale, 40},
                           {HeaderVariableCode.LwDisplay, 290},
                           {HeaderVariableCode.PdMode, 70},
                           {HeaderVariableCode.PdSize, 40},
                           {HeaderVariableCode.PLineGen, 70},
                           {HeaderVariableCode.TdCreate, 40},
                           {HeaderVariableCode.TduCreate, 40},
                           {HeaderVariableCode.TdUpdate, 40},
                           {HeaderVariableCode.TduUpdate, 40},
                           {HeaderVariableCode.TdinDwg, 40},

                       };
        }

        #endregion

    }
}