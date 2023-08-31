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
using System.Globalization;
using System.IO;
using System.Text;

namespace netDxf.Entities
{
    /// <summary>
    /// Represents a <see cref="Hatch">hatch</see> pattern style.
    /// </summary>
    public class HatchPattern :
        ICloneable
    {
        #region private fields

        private readonly string name;
        private readonly List<HatchPatternLineDefinition> lineDefinitions;
        private HatchStyle style;
        private HatchFillType fill;
        private HatchType type;
        private Vector2 origin;
        private double angle;
        private double scale;
        private string description;

        #endregion

        #region constructor

        /// <summary>
        /// Initializes a new instance of the <c>HatchPattern</c> class.
        /// </summary>
        /// <param name="name">Pattern name, always stored as uppercase.</param>
        public HatchPattern(string name)
            : this(name, null, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>HatchPattern</c> class.
        /// </summary>
        /// <param name="name">Pattern name, always stored as uppercase.</param>
        /// <param name="description">Description of the pattern (optional, this information is not saved in the DXF file). By default it will use the supplied name.</param>
        public HatchPattern(string name, string description)
            : this(name, null, description)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>HatchPattern</c> class.
        /// </summary>
        /// <param name="name">Pattern name, always stored as uppercase.</param>
        /// <param name="lineDefinitions">The definition of the lines that make up the pattern (not applicable in Solid fills).</param>
        public HatchPattern(string name, IEnumerable<HatchPatternLineDefinition> lineDefinitions)
            : this(name, lineDefinitions, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>HatchPattern</c> class.
        /// </summary>
        /// <param name="name">Pattern name, always stored as uppercase.</param>
        /// <param name="lineDefinitions">The definition of the lines that make up the pattern (not applicable in Solid fills).</param>
        /// <param name="description">Description of the pattern (optional, this information is not saved in the DXF file). By default it will use the supplied name.</param>
        public HatchPattern(string name, IEnumerable<HatchPatternLineDefinition> lineDefinitions, string description)
        {
            this.name = string.IsNullOrEmpty(name) ? string.Empty : name;
            this.description = string.IsNullOrEmpty(description) ? string.Empty : description;
            this.style = HatchStyle.Normal;
            this.fill = this.name == "SOLID" ? HatchFillType.SolidFill : HatchFillType.PatternFill;
            this.type = HatchType.UserDefined;
            this.origin = Vector2.Zero;
            this.angle = 0.0;
            this.scale = 1.0;
            this.lineDefinitions = lineDefinitions == null ? new List<HatchPatternLineDefinition>() : new List<HatchPatternLineDefinition>(lineDefinitions);
        }

        #endregion

        #region predefined patterns

        /// <summary>
        /// Solid hatch pattern.
        /// </summary>
        /// <remarks>The predefined pattern values are based on the acad.pat file of AutoCAD.</remarks>
        public static HatchPattern Solid
        {
            get
            {
                HatchPattern pattern = new HatchPattern("SOLID", "Solid fill") {type = HatchType.Predefined};
                // this is the pattern line definition for solid fills as defined in the acad.pat, but it is not needed
                //HatchPatternLineDefinition lineDefinition = new HatchPatternLineDefinition
                //                                                {
                //                                                    Angle = 45,
                //                                                    Origin = Vector2.Zero,
                //                                                    Delta = new Vector2(0.0, 0.125)
                //                                                };
                //pattern.LineDefinitions.Add(lineDefinition);
                return pattern;
            }
        }

        /// <summary>
        /// Lines hatch pattern.
        /// </summary>
        /// <remarks>The predefined pattern values are based on the acad.pat file of AutoCAD.</remarks>
        public static HatchPattern Line
        {
            get
            {
                HatchPattern pattern = new HatchPattern("LINE", "Parallel horizontal lines");
                HatchPatternLineDefinition lineDefinition = new HatchPatternLineDefinition
                {
                    Angle = 0,
                    Origin = Vector2.Zero,
                    Delta = new Vector2(0.0, 0.125)
                };
                pattern.LineDefinitions.Add(lineDefinition);
                pattern.type = HatchType.Predefined;
                return pattern;
            }
        }

        /// <summary>
        /// Net or squares hatch pattern.
        /// </summary>
        /// <remarks>The predefined pattern values are based on the acad.pat file of AutoCAD.</remarks>
        public static HatchPattern Net
        {
            get
            {
                HatchPattern pattern = new HatchPattern("NET", "Horizontal / vertical grid");

                HatchPatternLineDefinition lineDefinition = new HatchPatternLineDefinition
                {
                    Angle = 0,
                    Origin = Vector2.Zero,
                    Delta = new Vector2(0.0, 0.125)
                };
                pattern.LineDefinitions.Add(lineDefinition);

                lineDefinition = new HatchPatternLineDefinition
                {
                    Angle = 90,
                    Origin = Vector2.Zero,
                    Delta = new Vector2(0.0, 0.125)
                };
                pattern.LineDefinitions.Add(lineDefinition);
                pattern.type = HatchType.Predefined;
                return pattern;
            }
        }

        /// <summary>
        /// Dots hatch pattern.
        /// </summary>
        /// <remarks>The predefined pattern values are based on the acad.pat file of AutoCAD.</remarks>
        public static HatchPattern Dots
        {
            get
            {
                HatchPattern pattern = new HatchPattern("DOTS", "A series of dots");
                HatchPatternLineDefinition lineDefinition = new HatchPatternLineDefinition
                {
                    Angle = 0,
                    Origin = Vector2.Zero,
                    Delta = new Vector2(0.03125, 0.0625),
                };
                lineDefinition.DashPattern.AddRange(new[] {0, -0.0625});
                pattern.LineDefinitions.Add(lineDefinition);
                pattern.type = HatchType.Predefined;
                return pattern;
            }
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the hatch pattern name.
        /// </summary>
        public string Name
        {
            get { return this.name; }
        }

        /// <summary>
        /// Gets or sets the hatch description (optional, this information is not saved in the DXF file).
        /// </summary>
        public string Description
        {
            get { return this.description; }
            set { this.description = value; }
        }

        /// <summary>
        /// Gets the hatch style.
        /// </summary>
        /// <remarks>Only normal style is implemented.</remarks>
        public HatchStyle Style
        {
            get { return this.style; }
            internal set { this.style = value; }
        }

        /// <summary>
        /// Gets or sets the hatch pattern type.
        /// </summary>
        public HatchType Type
        {
            get { return this.type; }
            set { this.type = value; }
        }

        /// <summary>
        /// Gets the solid fill flag.
        /// </summary>
        public HatchFillType Fill
        {
            get { return this.fill; }
            internal set { this.fill = value; }
        }

        /// <summary>
        /// Gets or sets the pattern origin.
        /// </summary>
        public Vector2 Origin
        {
            get { return this.origin; }
            set { this.origin = value; }
        }

        /// <summary>
        /// Gets or sets the pattern angle in degrees.
        /// </summary>
        public double Angle
        {
            get { return this.angle; }
            set { this.angle = MathHelper.NormalizeAngle(value); }
        }

        /// <summary>
        /// Gets or sets the pattern scale (not applicable in Solid fills).
        /// </summary>
        public double Scale
        {
            get { return this.scale; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The scale can not be zero or less.");
                this.scale = value;
            }
        }

        /// <summary>
        /// Gets the definition of the lines that make up the pattern (not applicable in Solid fills).
        /// </summary>
        public List<HatchPatternLineDefinition> LineDefinitions
        {
            get { return this.lineDefinitions; }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Gets the list of hatch pattern names defined in a PAT file.
        /// </summary>
        /// <param name="file">Hatch pattern definitions file.</param>
        /// <returns>List of hatch pattern names contained in the specified PAT file.</returns>
        public static List<string> NamesFromFile(string file)
        {
            List<string> names = new List<string>();
            using (StreamReader reader = new StreamReader(File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), true))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                        throw new FileLoadException("Unknown error reading PAT file.", file);

                    // every line type definition starts with '*'
                    if (!line.StartsWith("*"))
                        continue;

                    // reading line type name and description
                    int endName = line.IndexOf(',');
                    // the first semicolon divides the name from the description that might contain more semicolons
                    names.Add(line.Substring(1, endName - 1));
                }
            }
            return names;
        }

        /// <summary>
        /// Creates a new hatch pattern from the definition in a PAT file.
        /// </summary>
        /// <param name="file">PAT file where the definition is located.</param>
        /// <param name="patternName">Name of the pattern definition that wants to be read (ignore case).</param>
        /// <returns>A Hatch pattern as defined in the PAT file.</returns>
        public static HatchPattern Load(string file, string patternName)
        {
            HatchPattern pattern = null;

            using (StreamReader reader = new StreamReader(File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), true))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                    {
                        throw new FileLoadException("Unknown error reading pat file.", file);
                    }
                    line = line.Trim();

                    // every pattern definition starts with '*'
                    if (!line.StartsWith("*"))
                    {
                        continue;
                    }

                    // reading pattern name and description
                    int endName = line.IndexOf(','); // the first semicolon divides the name from the description that might contain more semicolons
                    string name = line.Substring(1, endName - 1);
                    string description = line.Substring(endName + 1, line.Length - endName - 1);

                    // remove start and end spaces
                    description = description.Trim();
                    if (!name.Equals(patternName, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    // we have found the pattern name, the next lines of the file contains the pattern definition
                    line = reader.ReadLine();
                    if (line == null)
                    {
                        throw new FileLoadException("Unknown error reading PAT file.", file);
                    }

                    line = line.Trim();

                    pattern = new HatchPattern(name, description);

                    while (!string.IsNullOrEmpty(line) && !line.StartsWith("*") &&  !line.StartsWith(";"))
                    {
                        string[] tokens = line.Split(',');
                        if (tokens.Length < 5)
                        {
                            throw new FileLoadException("The hatch pattern definition lines must contain at least 5 values.", file);
                        }

                        double angle = double.Parse(tokens[0], NumberStyles.Float, CultureInfo.InvariantCulture);
                        Vector2 origin = new Vector2(
                            double.Parse(tokens[1], NumberStyles.Float, CultureInfo.InvariantCulture),
                            double.Parse(tokens[2], NumberStyles.Float, CultureInfo.InvariantCulture));
                        Vector2 delta = new Vector2(
                            double.Parse(tokens[3], NumberStyles.Float, CultureInfo.InvariantCulture),
                            double.Parse(tokens[4], NumberStyles.Float, CultureInfo.InvariantCulture));

                        HatchPatternLineDefinition lineDefinition = new HatchPatternLineDefinition
                        {
                            Angle = angle,
                            Origin = origin,
                            Delta = delta,
                        };

                        // the rest of the info is optional if it exists define the dash pattern definition
                        for (int i = 5; i < tokens.Length; i++)
                            lineDefinition.DashPattern.Add(double.Parse(tokens[i], NumberStyles.Float, CultureInfo.InvariantCulture));

                        pattern.LineDefinitions.Add(lineDefinition);
                        pattern.Type = HatchType.UserDefined;

                        if (reader.EndOfStream)
                        {
                            break;
                        }

                        line = reader.ReadLine();
                        if (line == null)
                        {
                            throw new FileLoadException("Unknown error reading PAT file.", file);
                        }
                        line = line.Trim();
                    } 

                    // there is no need to continue parsing the file, the info has been read
                    break;
                }
            }

            return pattern;
        }

        /// <summary>
        /// Saves the current linetype to the specified file, if the file does not exist it creates a new one.
        /// </summary>
        /// <param name="file">File where the current linetype will be saved.</param>
        public void Save(string file)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format("*{0},{1}", this.Name, this.description));
            
            foreach (HatchPatternLineDefinition def in this.lineDefinitions)
            {
                sb.Append(string.Format("{0},{1},{2},{3},{4}",
                    def.Angle.ToString(CultureInfo.InvariantCulture),
                    def.Origin.X.ToString(CultureInfo.InvariantCulture),
                    def.Origin.Y.ToString(CultureInfo.InvariantCulture),
                    def.Delta.X.ToString(CultureInfo.InvariantCulture),
                    def.Delta.Y.ToString(CultureInfo.InvariantCulture)));
                foreach (double d in def.DashPattern)
                {
                    sb.Append(string.Format(",{0}", d.ToString(CultureInfo.InvariantCulture)));
                }
                sb.Append(Environment.NewLine);
            }

            File.AppendAllText(file, sb.ToString());
        }

        #endregion

        #region ICloneable

        public virtual object Clone()
        {
            HatchPattern copy = new HatchPattern(this.name, this.description)
            {
                Style = this.style,
                Fill = this.fill,
                Type = this.type,
                Origin = this.origin,
                Angle = this.angle,
                Scale = this.scale,
            };

            foreach (HatchPatternLineDefinition def in this.lineDefinitions)
                copy.LineDefinitions.Add((HatchPatternLineDefinition) def.Clone());

            return copy;
        }

        #endregion
    }
}