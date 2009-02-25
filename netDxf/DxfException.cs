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

namespace netDxf
{
    public class DxfException : Exception
    {
        private readonly string file;

        public DxfException(string file)
        {
            this.file = file;
        }

        public DxfException(string file, string message)
            : base(message)
        {
            this.file = file;
        }

        public DxfException(string file, string message, Exception innerException)
            : base(message, innerException)
        {
            this.file = file;
        }

        public string File
        {
            get { return this.file; }
        }
    }

    #region section exceptions

    public class DxfHeaderVariableException : DxfException
        {
        private readonly string name;

        public DxfHeaderVariableException(string name, string file)
            : base(file)
        {
            this.name = name;
        }

        public DxfHeaderVariableException(string name, string file, string message)
            : base(file, message)
        {
            this.name = name;
        }

        public string Name
        {
            get { return this.name; }
        }
    }

    public class DxfSectionException : DxfException
    {
        private readonly string section;

        public DxfSectionException(string section, string file)
            : base(file)
        {
            this.section = section;
        }

        public DxfSectionException(string section, string file, string message)
            : base(file, message)
        {
            this.section = section;
        }

        public string Section
        {
            get { return this.section; }
        }
    }

    public class InvalidDxfSectionException : DxfSectionException
    {
        public InvalidDxfSectionException(string section, string file)
            : base(section, file)
        {
        }

        public InvalidDxfSectionException(string section, string file, string message)
            : base(section, file, message)
        {
        }
    }

    public class OpenDxfSectionException : DxfSectionException
    {
        public OpenDxfSectionException(string section, string file)
            : base(section, file)
        {
        }

        public OpenDxfSectionException(string section, string file, string message)
            : base(section, file, message)
        {
        }
    }

    public class ClosedDxfSectionException : DxfSectionException
    {
        public ClosedDxfSectionException(string section, string file)
            : base(section, file)
        {
        }

        public ClosedDxfSectionException(string section, string file, string message)
            : base(section, file, message)
        {
        }
    }

    #endregion

    #region table exceptions

    public class DxfTableException : DxfException
    {
        private readonly string table;

        public DxfTableException(string table, string file)
            : base(file)
        {
            this.table = table;
        }

        public DxfTableException(string table, string file, string message)
            : base(file, message)
        {
            this.table = table;
        }

        public string Table
        {
            get { return this.table; }
        }
    }

    public class InvalidDxfTableException : DxfTableException
    {
        public InvalidDxfTableException(string table, string file)
            : base(table, file)
        {
        }

        public InvalidDxfTableException(string table, string file, string message)
            : base(table, file, message)
        {
        }
    }

    public class OpenDxfTableException : DxfTableException
    {
        public OpenDxfTableException(string table, string file)
            : base(table, file)
        {
        }

        public OpenDxfTableException(string table, string file, string message)
            : base(table, file, message)
        {
        }
    }

    public class ClosedDxfTableException : DxfTableException
    {
        public ClosedDxfTableException(string table, string file)
            : base(table, file)
        {
        }

        public ClosedDxfTableException(string table, string file, string message)
            : base(table, file, message)
        {
        }
    }

    #endregion

    #region entity exceptions

    public class EntityDxfException : DxfException
    {
        private readonly string name;

        public EntityDxfException(string name, string file, string message)
            : base(file, message)
        {
            this.name = name;
        }

        public string Name
        {
            get { return this.name; }
        }
    }

    public class InvalidCodeValueEntityDxfException : DxfException
    {
        private readonly CodeValuePair codePair;

        public InvalidCodeValueEntityDxfException(CodeValuePair codePair, string file, string message)
            : base(file, message)
        {
            this.codePair = codePair;
        }

        public CodeValuePair CodePair
        {
            get { return this.codePair; }
        }
    }

    #endregion
}