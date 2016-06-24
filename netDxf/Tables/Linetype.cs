#region netDxf library, Copyright (C) 2009-2016 Daniel Carvajal (haplokuon@gmail.com)

//                        netDxf library
// Copyright (C) 2009-2016 Daniel Carvajal (haplokuon@gmail.com)
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
using System.IO;
using netDxf.Collections;

namespace netDxf.Tables
{
    /// <summary>
    /// Represents a line type.
    /// </summary>
    /// <remarks>
    /// Only simple line types are supported.
    /// </remarks>
    public class Linetype :
        TableObject
    {
        #region private fields

        private string description;
        private readonly List<double> segments;

        #endregion

        #region constants

        /// <summary>
        /// ByLayer line type name.
        /// </summary>
        public const string ByLayerName = "ByLayer";

        /// <summary>
        /// ByBlock line type name.
        /// </summary>
        public const string ByBlockName = "ByBlock";

        /// <summary>
        /// Default line type name.
        /// </summary>
        public const string DefaultName = "Continuous";

        /// <summary>
        /// Gets the ByLayer line type.
        /// </summary>
        public static Linetype ByLayer
        {
            get { return new Linetype(ByLayerName); }
        }

        /// <summary>
        /// Gets the ByBlock line type.
        /// </summary>
        public static Linetype ByBlock
        {
            get { return new Linetype(ByBlockName); }
        }

        /// <summary>
        /// Gets the predefined continuous line.
        /// </summary>
        public static Linetype Continuous
        {
            get { return new Linetype(DefaultName, "Solid line"); }
        }

        /// <summary>
        /// Gets a predefined center line.
        /// </summary>
        public static Linetype Center
        {
            get
            {
                Linetype result = new Linetype("Center")
                {
                    Description = "Center, ____ _ ____ _ ____ _ ____ _ ____ _ ____"
                };
                result.Segments.AddRange(new[] {1.25, -0.25, 0.25, -0.25});
                return result;
            }
        }

        /// <summary>
        /// Gets a predefined dash dot line.
        /// </summary>
        public static Linetype DashDot
        {
            get
            {
                Linetype result = new Linetype("Dashdot")
                {
                    Description = "Dash dot, __ . __ . __ . __ . __ . __ . __ . __"
                };
                result.Segments.AddRange(new[] {0.5, -0.25, 0.0, -0.25});
                return result;
            }
        }

        /// <summary>
        /// Gets a predefined dashed line
        /// </summary>
        public static Linetype Dashed
        {
            get
            {
                Linetype result = new Linetype("Dashed")
                {
                    Description = "Dashed, __ __ __ __ __ __ __ __ __ __ __ __ __ _"
                };
                result.Segments.AddRange(new[] {0.5, -0.25});
                return result;
            }
        }

        /// <summary>
        /// Gets a predefined dot line
        /// </summary>
        public static Linetype Dot
        {
            get
            {
                Linetype result = new Linetype("Dot")
                {
                    Description = "Dot, . . . . . . . . . . . . . . . . . . . . . . . ."
                };
                result.Segments.AddRange(new[] {0.0, -0.25});
                return result;
            }
        }

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>Linetype</c> class.
        /// </summary>
        /// <param name="name">Line type name.</param>
        public Linetype(string name)
            : this(name, null, string.Empty, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Linetype</c> class.
        /// </summary>
        /// <param name="name">Line type name.</param>
        /// <param name="description">Line type description (optional).</param>
        public Linetype(string name, string description)
            : this(name, null, description, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Linetype</c> class.
        /// </summary>
        /// <param name="name">Line type name.</param>
        public Linetype(string name, IEnumerable<double> segments)
            : this(name, segments, string.Empty, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Linetype</c> class.
        /// </summary>
        /// <param name="name">Line type name.</param>
        /// <param name="description">Line type description (optional).</param>
        public Linetype(string name, IEnumerable<double> segments, string description)
            : this(name, segments, description, true)
        {
        }

        internal Linetype(string name, IEnumerable<double> segments, string description, bool checkName)
            : base(name, DxfObjectCode.Linetype, checkName)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name), "The line type name should be at least one character long.");

            this.IsReserved = name.Equals(ByLayerName, StringComparison.OrdinalIgnoreCase) ||
                              name.Equals(ByBlockName, StringComparison.OrdinalIgnoreCase) ||
                              name.Equals(DefaultName, StringComparison.OrdinalIgnoreCase);
            this.description = string.IsNullOrEmpty(description) ? string.Empty : description;
            this.segments = segments == null ? new List<double>() : new List<double>(segments);
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the line type description (optional).
        /// </summary>
        public string Description
        {
            get { return this.description; }
            set { this.description = value; }
        }

        /// <summary>
        /// Gets or sets the list of line type segments.
        /// </summary>
        /// <remarks>
        /// A positive decimal number denotes a pen-down (dash) segment of that length. 
        /// A negative decimal number denotes a pen-up (space) segment of that length. 
        /// A dash length of 0 draws a dot. 
        /// </remarks>
        public List<double> Segments
        {
            get { return this.segments; }
        }

        /// <summary>
        /// Gets the owner of the actual dxf object.
        /// </summary>
        public new Linetypes Owner
        {
            get { return (Linetypes) base.Owner; }
            internal set { base.Owner = value; }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Gets the total length of the line type.
        /// </summary>
        /// <returns>The total length of the line type.</returns>
        public double Length()
        {
            double result = 0.0;
            foreach (double s in this.segments)
            {
                result += Math.Abs(s);
            }
            return result;
        }

        /// <summary>
        /// Creates a new line type from the definition in a .lin file.
        /// </summary>
        /// <remarks>Only simple line types are supported.</remarks>
        /// <param name="file">Lin file where the definition is located.</param>
        /// <param name="linetypeName">Name of the line type definition that wants to be read (ignore case).</param>
        /// <returns>A line type defined by the .lin file.</returns>
        public static Linetype FromFile(string file, string linetypeName)
        {
            Linetype linetype = null;

            using (StreamReader reader = new StreamReader(File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), true))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                        throw new FileLoadException("Unknown error reading .lin file.", file);
                    // lines starting with semicolons are comments
                    if (line.StartsWith(";"))
                        continue;
                    // every line type definition starts with '*'
                    if (!line.StartsWith("*"))
                        continue;

                    // reading line type name and description
                    int endName = line.IndexOf(','); // the first semicolon divides the name from the description that might contain more semicolons
                    string name = line.Substring(1, endName - 1);
                    string description = line.Substring(endName + 1, line.Length - endName - 1);

                    // remove start and end spaces
                    description = description.Trim();

                    if (!name.Equals(linetypeName, StringComparison.OrdinalIgnoreCase))
                        continue;

                    // we have found the line type name, the next line of the file contains the line type definition
                    line = reader.ReadLine();
                    if (line == null)
                        throw new FileLoadException("Unknown error reading .lin file.", file);
                    linetype = new Linetype(name, description);

                    string[] tokens = line.Split(',');

                    // the index 0 is always A (alignment field)
                    for (int i = 1; i < tokens.Length; i++)
                    {
                        double segment;
                        if (double.TryParse(tokens[i], out segment))
                            linetype.Segments.Add(segment);
                        else
                        {
                            // only simple line types are supported.
                            linetype = null;
                            break;
                        }
                    }
                    break;
                }
            }
            return linetype;
        }

        #endregion

        #region overrides

        /// <summary>
        /// Creates a new Linetype that is a copy of the current instance.
        /// </summary>
        /// <param name="newName">Linetype name of the copy.</param>
        /// <returns>A new Linetype that is a copy of this instance.</returns>
        public override TableObject Clone(string newName)
        {
            return new Linetype(newName, this.segments, this.description);
        }

        /// <summary>
        /// Creates a new Linetype that is a copy of the current instance.
        /// </summary>
        /// <returns>A new Linetype that is a copy of this instance.</returns>
        public override object Clone()
        {
            return this.Clone(this.Name);
        }

        #endregion
    }
}