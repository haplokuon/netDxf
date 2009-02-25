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
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Threading;
using netDxf.Blocks;
using netDxf.Entities;
using netDxf.Header;
using netDxf.Tables;
using Attribute=netDxf.Entities.Attribute;

namespace netDxf
{
    public class DxfDocument
    {
        #region private fields

        #region header

        private string comments;

        private DxfVersion version;

        #endregion

        #region tables

        private List<string> appRegisterNames;
        private Dictionary<string, Layer> layers;
        private Dictionary<string, LineType> lineTypes;
        private Dictionary<string, TextStyle> textStyles;

        #endregion

        #region blocks

        private Dictionary<string, Block> blocks;

        #endregion

        #region entities

        private List<Arc> arcs;
        private List<Circle> circles;
        private readonly List<Ellipse> ellipses;
        private List<Solid> solids;
        private List<Face3D> faces3d;
        private List<Insert> inserts;
        private List<Line> lines;
        private List<Point> points;
        private List<IPolyline> polylines;
        private List<Text> texts;

        #endregion

        #endregion

        #region constructor

        public DxfDocument()
        {
           this.version = version;
            this.comments = "";
            this.layers = new Dictionary<string, Layer>();
            this.lineTypes = new Dictionary<string, LineType>();
            this.textStyles = new Dictionary<string, TextStyle>();
            this.blocks = new Dictionary<string, Block>();
            this.appRegisterNames = new List<string>();

            this.arcs = new List<Arc>();
            this.ellipses = new List<Ellipse>();
            this.faces3d = new List<Face3D>();
            this.solids = new List<Solid>();
            this.inserts = new List<Insert>();
            this.polylines = new List<IPolyline>();
            this.lines = new List<Line>();
            this.circles = new List<Circle>();
            this.points = new List<Point>();
            this.texts = new List<Text>();
        }

        #endregion

        #region public properties

        #region header

        /// <summary>
        /// Optional comments to be added at the dxf file beginning beginning.
        /// </summary>
        public string Comments
        {
            get { return this.comments; }
            set { this.comments = value; }
        }

        /// <summary>
        /// Dxf file version.
        /// </summary>
        /// <remarks>The only allowed option is AutoCad12.</remarks>
        public DxfVersion Version
        {
            get{ return this.version;}
        }

        #endregion

        #region table public properties

        public ReadOnlyCollection<string> AppRegisterNames
        {
            get { return this.appRegisterNames.AsReadOnly(); }
        }

        public ReadOnlyCollection<Layer> Layers
        {
            get
            {
                List<Layer> list = new List<Layer>();
                list.AddRange(this.layers.Values);
                return list.AsReadOnly();
            }
        }

        public ReadOnlyCollection<LineType> LineTypes
        {
            get
            {
                List<LineType> list = new List<LineType>();
                list.AddRange(this.lineTypes.Values);
                return list.AsReadOnly();
            }
        }

        public ReadOnlyCollection<TextStyle> TextStyle
        {
            get
            {
                List<TextStyle> list = new List<TextStyle>();
                list.AddRange(this.textStyles.Values);
                return list.AsReadOnly();
            }
        }

        public ReadOnlyCollection<Block> Blocks
        {
            get
            {
                List<Block> list = new List<Block>();
                list.AddRange(this.blocks.Values);
                return list.AsReadOnly();
            }
        }

        #endregion

        #region entities public properties

        public ReadOnlyCollection<Arc> Arcs
        {
            get { return this.arcs.AsReadOnly(); }
        }

        public ReadOnlyCollection<Ellipse> Ellipses
        {
            get { return this.ellipses.AsReadOnly(); }
        }

        public ReadOnlyCollection<Circle> Circles
        {
            get { return this.circles.AsReadOnly(); }
        }

        public ReadOnlyCollection<Face3D> Faces3d
        {
            get { return this.faces3d.AsReadOnly(); }
        }

        public ReadOnlyCollection<Solid> Solids
        {
            get { return this.solids.AsReadOnly(); }
        }

        public ReadOnlyCollection<Insert> Inserts
        {
            get { return this.inserts.AsReadOnly(); }
        }

        public ReadOnlyCollection<Line> Lines
        {
            get { return this.lines.AsReadOnly(); }
        }

        public ReadOnlyCollection<IPolyline> Polylines
        {
            get { return this.polylines.AsReadOnly(); }
        }

        public ReadOnlyCollection<Point> Points
        {
            get { return this.points.AsReadOnly(); }
        }

        public ReadOnlyCollection<Text> Texts
        {
            get { return this.texts.AsReadOnly(); }
        }

        #endregion

        #endregion

        #region public table methods

        /// <summary>
        /// Gets a text style from the the table.
        /// </summary>
        /// <param name="name">TextStyle name</param>
        /// <returns>TextStyle.</returns>
        public TextStyle GetTextStyle(string name)
        {
            return this.textStyles[name];
        }

        /// <summary>
        /// Determines if a specified text style exists in the table.
        /// </summary>
        /// <param name="textStyle">Text style to locate.</param>
        /// <returns>True if the specified text style exists or false in any other case.</returns>
        public bool ContainsTextStyle(TextStyle textStyle)
        {
            return this.textStyles.ContainsKey(textStyle.Name);
        }

        /// <summary>
        /// Gets a block from the the table.
        /// </summary>
        /// <param name="name">Block name</param>
        /// <returns>Block.</returns>
        public Block GetBlock(string name)
        {
            return this.blocks[name];
        }

        /// <summary>
        /// Determines if a specified block exists in the table.
        /// </summary>
        /// <param name="block">Block to locate.</param>
        /// <returns>True if the specified block exists or false in any other case.</returns>
        public bool ContainsBlock(Block block)
        {
            return this.blocks.ContainsKey(block.Name);
        }

        /// <summary>
        /// Gets a line type from the the table.
        /// </summary>
        /// <param name="name">LineType name</param>
        /// <returns>LineType.</returns>
        public LineType GetLineType(string name)
        {
            return this.lineTypes[name];
        }

        /// <summary>
        /// Determines if a specified line type exists in the table.
        /// </summary>
        /// <param name="lineType">Line type to locate.</param>
        /// <returns>True if the specified line type exists or false in any other case.</returns>
        public bool ContainsLineType(LineType lineType)
        {
            return this.lineTypes.ContainsKey(lineType.Name);
        }

        /// <summary>
        /// Gets a layer from the the table.
        /// </summary>
        /// <param name="name">Layer name</param>
        /// <returns>Layer.</returns>
        public Layer GetLayer(string name)
        {
            return this.layers[name];
        }

        /// <summary>
        /// Determines if a specified layer exists in the table.
        /// </summary>
        /// <param name="layer">Layer to locate.</param>
        /// <returns>True if the specified layer exists or false in any other case.</returns>
        public bool ContainsLayer(Layer layer)
        {
            return this.layers.ContainsKey(layer.Name);
        }

        #endregion

        #region public methods

        public void AddEntity(IEntityObject entity)
        {
            foreach (XData xDataRecord in entity.XData)
            {
                if (!this.appRegisterNames.Contains(xDataRecord.ApplicationRegistry.Value.ToString()))
                {
                    this.appRegisterNames.Add(xDataRecord.ApplicationRegistry.Value.ToString());
                }
            }

            if (!this.layers.ContainsKey(entity.Layer.Name))
            {
                this.layers.Add(entity.Layer.Name, entity.Layer);
            }

            //if (entity.LineType.Name != LineType.ByLayer.Name && entity.LineType.Name != LineType.ByBlock.Name)
            //{
                if (!this.lineTypes.ContainsKey(entity.LineType.Name))
                {
                    this.lineTypes.Add(entity.LineType.Name, entity.LineType);
                }
            //}
            switch (entity.Type)
            {
                case EntityType.Arc:
                    this.arcs.Add((Arc) entity);
                    break;
                case EntityType.Circle:
                    this.circles.Add((Circle) entity);
                    break;
                case EntityType.Ellipse:
                    this.ellipses.Add((Ellipse)entity);
                    break;
                case EntityType.Point:
                    this.points.Add((Point) entity);
                    break;
                case EntityType.Face3D:
                    this.faces3d.Add((Face3D) entity);
                    break;
                case EntityType.Solid:
                    this.solids.Add((Solid)entity);
                    break;
                case EntityType.Insert:
                    if (!this.blocks.ContainsKey(((Insert) entity).Block.Name))
                    {
                        this.blocks.Add(((Insert) entity).Block.Name, ((Insert) entity).Block);
                    }
                    foreach (Attribute attribute in ((Insert) entity).Attributes)
                    {
                        if (!this.layers.ContainsKey(attribute.Layer.Name))
                        {
                            this.layers.Add(attribute.Layer.Name, attribute.Layer);
                        }

                        if (attribute.LineType.Name != LineType.ByLayer.Name && attribute.LineType.Name != LineType.ByBlock.Name)
                        {
                            if (!this.lineTypes.ContainsKey(attribute.LineType.Name))
                            {
                                this.lineTypes.Add(attribute.LineType.Name, attribute.LineType);
                            }
                        }

                        AttributeDefinition attDef = attribute.Definition;
                        if (!this.layers.ContainsKey(attDef.Layer.Name))
                        {
                            this.layers.Add(attDef.Layer.Name, attDef.Layer);
                        }
                        if (attDef.LineType.Name != LineType.ByLayer.Name && attDef.LineType.Name != LineType.ByBlock.Name)
                        {
                            if (!this.lineTypes.ContainsKey(attDef.LineType.Name))
                            {
                                this.lineTypes.Add(attDef.LineType.Name, attDef.LineType);
                            }
                        }
                        if (!this.textStyles.ContainsKey(attDef.Style.Name))
                        {
                            this.textStyles.Add(attDef.Style.Name, attDef.Style);
                        }
                    }
                    this.inserts.Add((Insert) entity);
                    break;
                case EntityType.Line:
                    this.lines.Add((Line) entity);
                    break;
                case EntityType.Polyline:
                    this.polylines.Add((IPolyline) entity);
                    break;
                case EntityType.Polyline3d:
                    this.polylines.Add((IPolyline) entity);
                    break;
                case EntityType.PolyfaceMesh:
                    this.polylines.Add((IPolyline) entity);
                    break;
                case EntityType.Text:
                    if (!this.textStyles.ContainsKey(((Text) entity).Style.Name))
                    {
                        this.textStyles.Add(((Text) entity).Style.Name, ((Text) entity).Style);
                    }
                    this.texts.Add((Text) entity);

                    break;
                case EntityType.Vertex:
                    throw new ArgumentException("The entity " + entity.Type + " is only allowed as part of another entity","entity");

                case EntityType.PolylineVertex:
                    throw new ArgumentException("The entity " + entity.Type + " is only allowed as part of another entity", "entity");

                case EntityType.Polyline3dVertex:
                    throw new ArgumentException("The entity " + entity.Type + " is only allowed as part of another entity", "entity");

                case EntityType.PolyfaceMeshVertex:
                    throw new ArgumentException("The entity " + entity.Type + " is only allowed as part of another entity", "entity");

                case EntityType.PolyfaceMeshFace:
                    throw new ArgumentException("The entity " + entity.Type + " is only allowed as part of another entity", "entity");

                case EntityType.AttributeDefinition:
                    throw new ArgumentException("The entity " + entity.Type + " is only allowed as part of another entity", "entity");

                case EntityType.Attribute:
                    throw new ArgumentException("The entity " + entity.Type + " is only allowed as part of another entity", "entity");

                default:
                    throw new NotImplementedException("The entity " + entity.Type + " is not implemented or unknown");
            }
        }

        public void Load(string file)
        {
            if (!File.Exists(file))
                throw new FileNotFoundException("File " + file + "not found", file);

            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            DxfReader dxfReader = new DxfReader(file);
            dxfReader.Open();
            dxfReader.Read();
            dxfReader.Close();

            //tqbles information
            this.appRegisterNames = dxfReader.ApplicationRegistrationIds;
            this.layers = dxfReader.Layers;
            this.lineTypes = dxfReader.LineTypes;
            this.textStyles = dxfReader.TextStyles;
            this.blocks = dxfReader.Blocks;

            //entities information
            this.arcs = dxfReader.Arcs;
            this.circles = dxfReader.Circles;
            this.points = dxfReader.Points;
            this.faces3d = dxfReader.Faces3d;
            this.solids = dxfReader.Solids;
            this.polylines = dxfReader.Polylines;
            this.lines = dxfReader.Lines;
            this.inserts = dxfReader.Inserts;
            this.texts = dxfReader.Texts;

            Thread.CurrentThread.CurrentCulture = cultureInfo;
        }

        public void Save(string file, DxfVersion dxfVersion)
        {
            if (dxfVersion != DxfVersion.AutoCad12)
                throw new NotImplementedException("The only valid dxf version is AutoCad12");

            this.version = dxfVersion;
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            DxfWriter dxfWriter = new DxfWriter(file);
            dxfWriter.Open();
            dxfWriter.WriteComments("Dxf file generated by netDxf, Copyright(C) 2009 Daniel Carvajal, Licensed under LGPL");
            dxfWriter.WriteComments(this.comments);

            //HEADER SECTION
            dxfWriter.BeginSection(StringCode.HeaderSection);
            dxfWriter.WriteSystemVariable(new HeaderVariable(SystemVariable.DabaseVersion,StringEnum.GetStringValue(this.version)));
            dxfWriter.EndSection();

            //CLASSES SECTION
            dxfWriter.BeginSection(StringCode.ClassesSection);
            dxfWriter.EndSection();

            //TABLES SECTION
            dxfWriter.BeginSection(StringCode.TablesSection);

            //registered application tables
            dxfWriter.BeginTable(StringCode.ApplicationIDTable);
            foreach (string id in this.appRegisterNames)
            {
                dxfWriter.RegisterApplication(id);
            }
            dxfWriter.EndTable();

            //line type tables
            dxfWriter.BeginTable(StringCode.LineTypeTable);
            foreach (LineType lineType in this.lineTypes.Values)
            {
                dxfWriter.WriteLineType(lineType);
            }
            dxfWriter.EndTable();

            //layer tables
            dxfWriter.BeginTable(StringCode.LayerTable);
            foreach (Layer layer in this.layers.Values)
            {
                dxfWriter.WriteLayer(layer);
            }
            dxfWriter.EndTable();

            //text style tables
            dxfWriter.BeginTable(StringCode.TextStyleTable);
            foreach (TextStyle style in this.textStyles.Values)
            {
                dxfWriter.WriteTextStyle(style);
            }
            dxfWriter.EndTable();

            dxfWriter.EndSection();

            dxfWriter.BeginSection(StringCode.BlocksSection);
            foreach (Block block in this.blocks.Values)
            {
                dxfWriter.WriteBlock(block);
            }

            dxfWriter.EndSection();

            //ENTITIES SECTION
            dxfWriter.BeginSection(StringCode.EntitiesSection);
            foreach (Arc arc in this.arcs)
            {
                dxfWriter.WriteEntity(arc);
            }
            foreach (Circle circle in this.circles)
            {
                dxfWriter.WriteEntity(circle);
            }
            foreach (Ellipse ellipse  in this.ellipses )
            {
                dxfWriter.WriteEntity(ellipse);
            }
            foreach (Point point in this.points)
            {
                dxfWriter.WriteEntity(point);
            }
            foreach (Face3D face in this.faces3d)
            {
                dxfWriter.WriteEntity(face);
            }
            foreach (Solid solid in this.solids)
            {
                dxfWriter.WriteEntity(solid);
            }
            foreach (Insert insert in this.inserts)
            {
                dxfWriter.WriteEntity(insert);
            }
            foreach (Line line in this.lines)
            {
                dxfWriter.WriteEntity(line);
            }
            foreach (IPolyline polyline in this.polylines)
            {
                dxfWriter.WriteEntity(polyline);
            }
            foreach (Text text in this.texts)
            {
                dxfWriter.WriteEntity(text);
            }

            dxfWriter.EndSection();

            //OBJECTS SECTION
            dxfWriter.BeginSection(StringCode.ObjectsSection);
            dxfWriter.EndSection();

            dxfWriter.Close();

            Thread.CurrentThread.CurrentCulture = cultureInfo;
        }

        #endregion
    }
}