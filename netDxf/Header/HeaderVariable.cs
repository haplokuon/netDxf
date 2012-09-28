#region netDxf, Copyright(C) 2009 Daniel Carvajal, Licensed under LGPL.

//                        netDxf library
// Copyright (C) 2009 Daniel Carvajal (haplokuon@gmail.com)
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
    internal class HeaderVariable
    {
        public const int NAME_CODE_GROUP = 9;
        public static readonly Dictionary<string, int> Allowed = InitializeSystemVariables();
        private readonly string name;
        private readonly int codeGroup;
        private readonly object value;

        public HeaderVariable(string name, object value)
        {
            if (!Allowed.ContainsKey(name))
                throw new ArgumentOutOfRangeException("name", name,"Variable name " + name + " not defined.");
            this.codeGroup = Allowed[name];
            this.name = name;
            this.value = value;
        }

        public string Name
        {
            get { return this.name; }
        }

        public int CodeGroup
        {
            get { return this.codeGroup; }
        }

        public object Value
        {
            get { return this.value; }
        }

        public override string ToString()
        {
            return String.Format("{0} : {1}", this.name, this.value);
        }

        private static Dictionary<string, int> InitializeSystemVariables()
        {
            return new Dictionary<string, int>
                       {
                           {SystemVariable.DatabaseVersion, 1},
                           {SystemVariable.HandSeed, 5}
                       };
        }
    }
}