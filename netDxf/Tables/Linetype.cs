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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using netDxf.Collections;

namespace netDxf.Tables
{
    /// <summary>
    /// Represents a line type. Simple and complex line types are supported.
    /// </summary>
    public class Linetype :
        TableObject
    {
        #region delegates and events

        public delegate void LinetypeSegmentAddedEventHandler(Linetype sender, LinetypeSegmentChangeEventArgs e);
        public event LinetypeSegmentAddedEventHandler LinetypeSegmentAdded;
        protected virtual void OnLinetypeSegmentAddedEvent(LinetypeSegment item)
        {
            LinetypeSegmentAddedEventHandler ae = this.LinetypeSegmentAdded;
            if (ae != null)
            {
                ae(this, new LinetypeSegmentChangeEventArgs(item));
            }
        }

        public delegate void LinetypeSegmentRemovedEventHandler(Linetype sender, LinetypeSegmentChangeEventArgs e);
        public event LinetypeSegmentRemovedEventHandler LinetypeSegmentRemoved;
        protected virtual void OnLinetypeSegmentRemovedEvent(LinetypeSegment item)
        {
            LinetypeSegmentRemovedEventHandler ae = this.LinetypeSegmentRemoved;
            if (ae != null)
            {
                ae(this, new LinetypeSegmentChangeEventArgs(item));
            }
        }

        public delegate void LinetypeTextSegmentStyleChangedEventHandler(Linetype sender, TableObjectChangedEventArgs<TextStyle> e);
        public event LinetypeTextSegmentStyleChangedEventHandler LinetypeTextSegmentStyleChanged;
        protected virtual TextStyle OnLinetypeTextSegmentStyleChangedEvent(TextStyle oldTextStyle, TextStyle newTextStyle)
        {
            LinetypeTextSegmentStyleChangedEventHandler ae = this.LinetypeTextSegmentStyleChanged;
            if (ae != null)
            {
                TableObjectChangedEventArgs<TextStyle> eventArgs = new TableObjectChangedEventArgs<TextStyle>(oldTextStyle, newTextStyle);
                ae(this, eventArgs);
                return eventArgs.NewValue;
            }
            return newTextStyle;
        }

        public delegate void LinetypeShapeSegmentStyleChangedEventHandler(Linetype sender, TableObjectChangedEventArgs<ShapeStyle> e);
        public event LinetypeShapeSegmentStyleChangedEventHandler LinetypeShapeSegmentStyleChanged;
        protected virtual ShapeStyle OnLinetypeShapeSegmentStyleChangedEvent(ShapeStyle oldShapeStyle, ShapeStyle newShapeStyle)
        {
            LinetypeShapeSegmentStyleChangedEventHandler ae = this.LinetypeShapeSegmentStyleChanged;
            if (ae != null)
            {
                TableObjectChangedEventArgs<ShapeStyle> eventArgs = new TableObjectChangedEventArgs<ShapeStyle>(oldShapeStyle, newShapeStyle);
                ae(this, eventArgs);
                return eventArgs.NewValue;
            }
            return newShapeStyle;
        }

        #endregion

        #region private fields

        private string description;
        private readonly ObservableCollection<LinetypeSegment> segments;
        private double segmentsPatternLength;

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
                List<LinetypeSegment> segments = new List<LinetypeSegment>
                {
                    new LinetypeSimpleSegment(1.25),
                    new LinetypeSimpleSegment(-0.25),
                    new LinetypeSimpleSegment(0.25),
                    new LinetypeSimpleSegment(-0.25)
                };

                return new Linetype("Center", segments, "Center, ____ _ ____ _ ____ _ ____ _ ____ _ ____");
            }
        }

        /// <summary>
        /// Gets a predefined dash dot line.
        /// </summary>
        public static Linetype DashDot
        {
            get
            {
                List<LinetypeSegment> segments = new List<LinetypeSegment>
                {
                    new LinetypeSimpleSegment(0.5),
                    new LinetypeSimpleSegment(-0.25),
                    new LinetypeSimpleSegment(0.0),
                    new LinetypeSimpleSegment(-0.25)
                };

                return new Linetype("Dashdot", segments, "Dash dot, __ . __ . __ . __ . __ . __ . __ . __");
            }
        }

        /// <summary>
        /// Gets a predefined dashed line
        /// </summary>
        public static Linetype Dashed
        {
            get
            {
                List<LinetypeSegment> segments = new List<LinetypeSegment>
                {
                    new LinetypeSimpleSegment(0.5),
                    new LinetypeSimpleSegment(-0.25)
                };

                return new Linetype("Dashed", segments, "Dashed, __ __ __ __ __ __ __ __ __ __ __ __ __ _");
            }
        }

        /// <summary>
        /// Gets a predefined dot line
        /// </summary>
        public static Linetype Dot
        {
            get
            {
                List<LinetypeSegment> segments = new List<LinetypeSegment>
                {
                    new LinetypeSimpleSegment(0.0),
                    new LinetypeSimpleSegment(-0.25)
                };

                return new Linetype("Dot", segments, "Dot, . . . . . . . . . . . . . . . . . . . . . . . .");
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
        /// <param name="description">Line type description.</param>
        public Linetype(string name, string description)
            : this(name, null, description, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Linetype</c> class.
        /// </summary>
        /// <param name="name">Line type name.</param>
        /// <param name="segments">List of linetype segments.</param>
        public Linetype(string name, IEnumerable<LinetypeSegment> segments)
            : this(name, segments, string.Empty, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Linetype</c> class.
        /// </summary>
        /// <param name="name">Line type name.</param>
        /// <param name="segments">List of linetype segments.</param>
        /// <param name="description">Line type description.</param>
        public Linetype(string name, IEnumerable<LinetypeSegment> segments, string description)
            : this(name, segments, description, true)
        {
        }

        internal Linetype(string name, IEnumerable<LinetypeSegment> segments, string description, bool checkName)
            : base(name, DxfObjectCode.Linetype, checkName)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name), "The line type name should be at least one character long.");
            }

            this.IsReserved = name.Equals(ByLayerName, StringComparison.OrdinalIgnoreCase) ||
                              name.Equals(ByBlockName, StringComparison.OrdinalIgnoreCase) ||
                              name.Equals(DefaultName, StringComparison.OrdinalIgnoreCase);
            this.description = string.IsNullOrEmpty(description) ? string.Empty : description;

            this.segments = new ObservableCollection<LinetypeSegment>();
            this.segments.BeforeAddItem += this.Segments_BeforeAddItem;
            this.segments.AddItem += this.Segments_AddItem;
            this.segments.BeforeRemoveItem += this.Segments_BeforeRemoveItem;
            this.segments.RemoveItem += this.Segments_RemoveItem;
            if (segments != null)
            {
                this.segments.AddRange(segments);
            }
        }

        #endregion

        #region public properties

        /// <summary>
        /// Defines if the line type is defined by layer.
        /// </summary>
        public bool IsByLayer
        {
            get { return this.Name.Equals(ByLayerName, StringComparison.InvariantCultureIgnoreCase); }
        }

        /// <summary>
        /// Defines if the line type is defined by block.
        /// </summary>
        public bool IsByBlock
        {
            get { return this.Name.Equals(ByBlockName, StringComparison.InvariantCultureIgnoreCase); }
        }

        /// <summary>
        /// Gets or sets the line type description.
        /// </summary>
        /// <remarks>
        /// New line characters are not allowed.
        /// </remarks>
        public string Description
        {
            get { return this.description; }
            set { this.description = string.IsNullOrEmpty(value) ? string.Empty : value; }
        }

        /// <summary>
        /// Gets the list of line type segments.
        /// </summary>
        public ObservableCollection<LinetypeSegment> Segments
        {
            get { return this.segments; }
        }

        /// <summary>
        /// Gets the owner of the actual DXF object.
        /// </summary>
        public new Linetypes Owner
        {
            get { return (Linetypes) base.Owner; }
            internal set { base.Owner = value; }
        }

        /// <summary>
        /// Gets the or sets segments pattern total length of the line type.
        /// </summary>
        public double SegmentsPatternLength
        {
            get { return this.segmentsPatternLength; }
            set { this.segmentsPatternLength = value; }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Gets the total length of the line type.
        /// </summary>
        public double Length()
        {
            double result = 0.0;
            foreach (LinetypeSegment s in this.segments)
            {
                result += Math.Abs(s.Length);
            }
            return result;
        }

        /// <summary>
        /// Gets the list of linetype names defined in a LIN file.
        /// </summary>
        /// <param name="file">Linetype definitions file.</param>
        /// <returns>List of linetype names contained in the specified LIN file.</returns>
        public static List<string> NamesFromFile(string file)
        {
            if (string.IsNullOrEmpty(file))
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (!string.Equals(Path.GetExtension(file), ".LIN", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ArgumentException("The linetype definitions file must have the extension LIN.", nameof(file));
            }

            List<string> names = new List<string>();
            using (StreamReader reader = new StreamReader(File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), true))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                    {
                        throw new FileLoadException("Unknown error reading LIN file.", file);
                    }

                    // every line type definition starts with '*'
                    if (!line.StartsWith("*"))
                    {
                        continue;
                    }

                    // reading line type name and description
                    int endName = line.IndexOf(',');
                    // the first semicolon divides the name from the description that might contain more semicolons
                    names.Add(line.Substring(1, endName - 1));
                }
            }

            return names;
        }

        /// <summary>
        /// Creates a new line type from the definition in a LIN file.
        /// </summary>
        /// <param name="file">Lin file where the definition is located.</param>
        /// <param name="linetypeName">Name of the line type definition to read (ignore case).</param>
        /// <returns>The linetype defined in the LIN file with the specified name, null if the linetype has not been found in the linetype definitions file.</returns>
        public static Linetype Load(string file, string linetypeName)
        {

            if (string.IsNullOrEmpty(file))
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (!string.Equals(Path.GetExtension(file), ".LIN", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ArgumentException("The linetype definitions file must have the extension LIN.", nameof(file));
            }

            if (string.IsNullOrEmpty(linetypeName))
            {
                return null;
            }

            Linetype linetype = null;            
            List<LinetypeSegment> segments = new List<LinetypeSegment>();
            using (StreamReader reader = new StreamReader(File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), true))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                    {
                        throw new FileLoadException("Unknown error reading LIN file.", file);
                    }

                    // every line type definition starts with '*'
                    if (!line.StartsWith("*"))
                    {
                        continue;
                    }

                    // reading line type name and description
                    int endName = line.IndexOf(','); // the first comma divides the name from the description that might contain more commas
                    string name = line.Substring(1, endName - 1);
                    string description = line.Substring(endName + 1, line.Length - endName - 1);

                    // remove start and end spaces
                    description = description.Trim();

                    if (name.Equals(linetypeName, StringComparison.OrdinalIgnoreCase))
                    {
                        // we have found the line type name, the next line of the file contains the line type definition
                        line = reader.ReadLine();
                        if (line == null)
                        {
                            throw new FileLoadException("Unknown error reading LIN file.", file);
                        }

                        string[] tokens = Regex.Split(line, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

                        // the index 0 is always A (alignment field)
                        for (int i = 1; i < tokens.Length; i++)
                        {
                            if (double.TryParse(tokens[i], NumberStyles.Float, CultureInfo.InvariantCulture, out double length))
                            {
                                // is the length followed by a shape or text segment
                                if (i + 1 < tokens.Length)
                                {
                                    if (tokens[i + 1].StartsWith("["))
                                    {
                                        List<string> data = new List<string>();

                                        // there are two kinds of complex linetype Text and Shape, the data is enclosed in brackets
                                        for (++i; i < tokens.Length; i++)
                                        {
                                            data.Add(tokens[i]);

                                            // text and shape data must be enclosed by brackets
                                            if (i >= tokens.Length)
                                            {
                                                throw new FormatException("The linetype definition is not well formatted.");
                                            }

                                            if (tokens[i].EndsWith("]"))
                                            {
                                                break;
                                            }
                                        }

                                        LinetypeSegment segment = ReadLineTypeComplexSegment(data.ToArray(), length);
                                        if (segment != null)
                                        {
                                            segments.Add(segment);
                                        }
                                    }
                                    else
                                    {
                                        segments.Add(new LinetypeSimpleSegment(length));
                                    }
                                }
                                else
                                {
                                    segments.Add(new LinetypeSimpleSegment(length));
                                }
                            }
                            else
                            {
                                throw new FormatException("The linetype definition is not well formatted.");
                            }
                        }

                        linetype = new Linetype(name, segments, description);
                        break;
                    }
                }
            }

            return linetype;
        }

        /// <summary>
        /// Saves the current linetype to the specified file, if the file does not exist it creates a new one.
        /// </summary>
        /// <param name="file">File where the current linetype will be saved.</param>
        public void Save(string file)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format("*{0},{1}", this.Name, this.description));
            sb.Append("A"); // A (alignment field)
            foreach (LinetypeSegment s in this.segments)
            {
                switch (s.Type)
                {
                    case LinetypeSegmentType.Simple:
                        sb.Append(string.Format(",{0}", s.Length.ToString(CultureInfo.InvariantCulture)));
                        break;
                    case LinetypeSegmentType.Text:
                        LinetypeTextSegment ts = (LinetypeTextSegment) s;
                        string trt = "R=";
                        switch (ts.RotationType)
                        {
                            case LinetypeSegmentRotationType.Absolute:
                                trt = "A=";
                                break;
                            case LinetypeSegmentRotationType.Relative:
                                trt = "R=";
                                break;
                            case LinetypeSegmentRotationType.Upright:
                                trt = "U=";
                                break;
                        }

                        sb.Append(string.Format(",{0},[\"{1}\",{2},S={3},{4}{5},X={6},Y={7}]",
                            ts.Length.ToString(CultureInfo.InvariantCulture),
                            ts.Text, 
                            ts.Style.Name,
                            ts.Scale.ToString(CultureInfo.InvariantCulture),
                            trt,
                            ts.Rotation.ToString(CultureInfo.InvariantCulture),
                            ts.Offset.X.ToString(CultureInfo.InvariantCulture), 
                            ts.Offset.Y.ToString(CultureInfo.InvariantCulture)));
                        break;
                    case LinetypeSegmentType.Shape:
                        LinetypeShapeSegment ss = (LinetypeShapeSegment) s;
                        string srt = "R=";
                        switch (ss.RotationType)
                        {
                            case LinetypeSegmentRotationType.Absolute:
                                srt = "A=";
                                break;
                            case LinetypeSegmentRotationType.Relative:
                                srt = "R=";
                                break;
                            case LinetypeSegmentRotationType.Upright:
                                srt = "U=";
                                break;
                        }

                        sb.Append(string.Format(",{0},[{1},{2},S={3},{4}{5},X={6},Y={7}]",
                            ss.Length.ToString(CultureInfo.InvariantCulture),
                            ss.Name,
                            ss.Style.File,
                            ss.Scale.ToString(CultureInfo.InvariantCulture),
                            srt,
                            ss.Rotation.ToString(CultureInfo.InvariantCulture),
                            ss.Offset.X.ToString(CultureInfo.InvariantCulture), 
                            ss.Offset.Y.ToString(CultureInfo.InvariantCulture)));
                        break;
                }
            }
            sb.Append(Environment.NewLine);

            File.AppendAllText(file, sb.ToString());
        }

        #endregion

        #region private methods

        private static LinetypeSegment ReadLineTypeComplexSegment(string[] data, double length)
        {
            // the data is enclosed in brackets
            Debug.Assert(data[0][0] == '[' || data[data.Length - 1][data[data.Length - 1].Length-1] == ']', "The data is enclosed in brackets.");

            // the first data item in a complex linetype definition segment
            // can be a shape name or a text string, the last always is enclosed in ""
            LinetypeSegmentType type = data[0][1] == '"' ? LinetypeSegmentType.Text : LinetypeSegmentType.Shape;

            // remove the start and end brackets
            data[0] = data[0].Remove(0, 1);
            data[data.Length - 1] = data[data.Length - 1].Remove(data[data.Length - 1].Length - 1, 1);

            Vector2 position = Vector2.Zero;
            LinetypeSegmentRotationType rotationType = LinetypeSegmentRotationType.Relative;
            double rotation = 0.0;
            double scale = 0.1;

            // at least two items must be present the shape name and file
            if (data.Length < 2)
            {
                return null;
            }

            string name = data[0].Trim('"');
            string style = data[1];
            for (int i = 2; i < data.Length; i++)
            {
                string value = data[i].Remove(0, 2);
                if (data[i].StartsWith("X=", StringComparison.InvariantCultureIgnoreCase))
                {
                    position.X = double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
                }
                else if (data[i].StartsWith("Y=", StringComparison.InvariantCultureIgnoreCase))
                {
                    position.Y = double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
                }
                else if (data[i].StartsWith("S=", StringComparison.InvariantCultureIgnoreCase))
                {
                    scale = double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
                    if (scale <= 0.0)
                    {
                        scale = 0.1;
                    }
                }
                else if (data[i].StartsWith("A=", StringComparison.InvariantCultureIgnoreCase))
                {
                    rotationType = LinetypeSegmentRotationType.Absolute;
                    rotation = ReadRotation(value);
                }
                else if (data[i].StartsWith("R=", StringComparison.InvariantCultureIgnoreCase))
                {
                    rotationType = LinetypeSegmentRotationType.Relative;
                    rotation = ReadRotation(value);
                }
                else if (data[i].StartsWith("U=", StringComparison.InvariantCultureIgnoreCase))
                {
                    rotationType = LinetypeSegmentRotationType.Upright;
                    rotation = ReadRotation(value);
                }
            }

            LinetypeSegment segment = null;
            switch (type)
            {
                case LinetypeSegmentType.Text:
                {
                    // complex text linetype segments only holds the name of the style
                    TextStyle textStyle = new TextStyle(style, TextStyle.DefaultFont);
                    segment = new LinetypeTextSegment(name, textStyle, length, position, rotationType, rotation, scale);
                    break;
                }
                case LinetypeSegmentType.Shape:
                {
                    ShapeStyle shapeStyle = new ShapeStyle(Path.GetFileNameWithoutExtension(style), style);
                    segment = new LinetypeShapeSegment(name, shapeStyle, length, position, rotationType, rotation, scale);
                    break;
                }
            }

            return segment;
        }

        private static double ReadRotation(string data)
        {
            if (data.EndsWith("D", StringComparison.InvariantCultureIgnoreCase))
            {
                // the angle is in degrees
                return double.Parse(data.Remove(data.Length - 1, 1), NumberStyles.Float, CultureInfo.InvariantCulture);
            }
            if (data.EndsWith("F", StringComparison.InvariantCultureIgnoreCase))
            {
                // the angle is in radians
                return double.Parse(data.Remove(data.Length - 1, 1), NumberStyles.Float, CultureInfo.InvariantCulture) * MathHelper.RadToDeg;
            }
            if (data.EndsWith("G", StringComparison.InvariantCultureIgnoreCase))
            {
                // the angle is in gradians
                return double.Parse(data.Remove(data.Length - 1, 1), NumberStyles.Float, CultureInfo.InvariantCulture) * MathHelper.GradToDeg;
            }
                    
            return double.Parse(data, NumberStyles.Float, CultureInfo.InvariantCulture);
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
        /// Creates a new Linetype that is a copy of the current instance.
        /// </summary>
        /// <param name="newName">Linetype name of the copy.</param>
        /// <returns>A new Linetype that is a copy of this instance.</returns>
        public override TableObject Clone(string newName)
        {
            List<LinetypeSegment> items = new List<LinetypeSegment>(this.segments.Count);
            foreach (LinetypeSegment segment in this.segments)
            {
                items.Add((LinetypeSegment)segment.Clone());
            }

            Linetype copy = new Linetype(newName, items, this.description);

            foreach (XData data in this.XData.Values)
            {
                copy.XData.Add((XData)data.Clone());
            }

            return copy;
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

        #region Elements collection events

        private void Segments_BeforeAddItem(ObservableCollection<LinetypeSegment> sender, ObservableCollectionEventArgs<LinetypeSegment> e)
        {
            // null items are not allowed
            e.Cancel = e.Item == null;
        }

        private void Segments_AddItem(ObservableCollection<LinetypeSegment> sender, ObservableCollectionEventArgs<LinetypeSegment> e)
        {
            this.OnLinetypeSegmentAddedEvent(e.Item);

            if (e.Item.Type == LinetypeSegmentType.Text)
            {
                ((LinetypeTextSegment)e.Item).TextStyleChanged += this.LinetypeTextSegment_StyleChanged;
            }
            if (e.Item.Type == LinetypeSegmentType.Shape)
            {
                ((LinetypeShapeSegment)e.Item).ShapeStyleChanged += this.LinetypeShapeSegment_StyleChanged;
            }
        }

        private void Segments_BeforeRemoveItem(ObservableCollection<LinetypeSegment> sender, ObservableCollectionEventArgs<LinetypeSegment> e)
        {
        }

        private void Segments_RemoveItem(ObservableCollection<LinetypeSegment> sender, ObservableCollectionEventArgs<LinetypeSegment> e)
        {
            this.OnLinetypeSegmentRemovedEvent(e.Item);

            if (e.Item.Type == LinetypeSegmentType.Text)
            {
                ((LinetypeTextSegment)e.Item).TextStyleChanged -= this.LinetypeTextSegment_StyleChanged;
            }
            if (e.Item.Type == LinetypeSegmentType.Shape)
            {
                ((LinetypeShapeSegment)e.Item).ShapeStyleChanged -= this.LinetypeShapeSegment_StyleChanged;
            }
        }

        #endregion

        #region LinetypeSegment events

        private void LinetypeTextSegment_StyleChanged(LinetypeTextSegment sender, TableObjectChangedEventArgs<TextStyle> e)
        {
            e.NewValue = this.OnLinetypeTextSegmentStyleChangedEvent(e.OldValue, e.NewValue);
        }

        private void LinetypeShapeSegment_StyleChanged(LinetypeShapeSegment sender, TableObjectChangedEventArgs<ShapeStyle> e)
        {
            e.NewValue = this.OnLinetypeShapeSegmentStyleChangedEvent(e.OldValue, e.NewValue);
        }

        #endregion
    }
}