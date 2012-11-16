#region netDxf, Copyright(C) 2012 Daniel Carvajal, Licensed under LGPL.

//                        netDxf library
// Copyright (C) 2012 Daniel Carvajal (haplokuon@gmail.com)
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Threading;
using netDxf.Blocks;
using netDxf.Entities;
using netDxf.Header;
using netDxf.Objects;
using netDxf.Tables;
using Attribute=netDxf.Entities.Attribute;

namespace netDxf
{
    /// <summary>
    /// Represents a document to read and write dxf ASCII files.
    /// </summary>
    public class DxfDocument
    {
        #region private fields

        #region header

        private string fileName;
        private string version;
        private int handleCount;

        #endregion

        #region tables

        private Dictionary<string, ApplicationRegistry> appRegisterNames;
        private readonly Dictionary<string, ViewPort> viewports;
        private Dictionary<string, Layer> layers;
        private Dictionary<string, LineType> lineTypes;
        private Dictionary<string, TextStyle> textStyles;
        private readonly Dictionary<string, DimensionStyle> dimStyles;

        #endregion

        #region blocks

        private Dictionary<string, Block> blocks;

        #endregion

        #region entities

        private readonly Hashtable addedObjects;
        private List<Arc> arcs;
        private List<Circle> circles;
        private List<Ellipse> ellipses;
        private List<NurbsCurve> nurbsCurves;
        private List<Solid> solids;
        private List<Face3d> faces3d;
        private List<Insert> inserts;
        private List<Line> lines;
        private List<Point> points;
        private List<PolyfaceMesh> polyfaceMeshes;
        private List<LwPolyline> lightWeightPolylines;
        private List<Polyline> polylines;
        private List<Text> texts;
        private List<MText> mTexts;
        private List<Hatch> hatches;

        #endregion

        #region objects

        #endregion

        #endregion

        #region constructor

        /// <summary>
        /// Initalizes a new instance of the <c>DxfDocument</c> class.
        /// </summary>
        public DxfDocument()
        {
            this.addedObjects = new Hashtable(); // keeps track of the added object to avoid duplicates
            this.viewports = new Dictionary<string, ViewPort>();
            this.layers = new Dictionary<string, Layer>();
            this.lineTypes = new Dictionary<string, LineType>();
            this.textStyles = new Dictionary<string, TextStyle>();
            this.blocks = new Dictionary<string, Block>();
            this.appRegisterNames = new Dictionary<string, ApplicationRegistry>();
            this.dimStyles = new Dictionary<string, DimensionStyle>();

            AddDefaultObjects();

            this.arcs = new List<Arc>();
            this.ellipses = new List<Ellipse>();
            this.nurbsCurves = new List<NurbsCurve>();
            this.faces3d = new List<Face3d>();
            this.solids = new List<Solid>();
            this.inserts = new List<Insert>();
            this.lightWeightPolylines = new List<LwPolyline>(); 
            this.polylines = new List<Polyline>();
            this.polyfaceMeshes=new List<PolyfaceMesh>();
            this.lines = new List<Line>();
            this.circles = new List<Circle>();
            this.points = new List<Point>();
            this.texts = new List<Text>();
            this.mTexts = new List<MText>();
            this.hatches = new List<Hatch>();
        }

        #endregion

        #region public properties

        #region header

        /// <summary>
        /// Gets the dxf file <see cref="DxfVersion">version</see>.
        /// </summary>
        public string Version
        {
            get { return this.version; }
        }

        /// <summary>
        /// Gets the name of the dxf document, once a file is saved or loaded this field is equals the file name without extension.
        /// </summary>
        public string FileName
        {
            get { return this.fileName; }
        }

        #endregion

        #region table public properties

        /// <summary>
        /// Gets the application registered names.
        /// </summary>
        public ReadOnlyCollection<ApplicationRegistry> AppRegisterNames
        {
            get
            {
                List<ApplicationRegistry> list = new List<ApplicationRegistry>();
                list.AddRange(this.appRegisterNames.Values);
                return list.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets the <see cref="Layer">layer</see> list.
        /// </summary>
        public ReadOnlyCollection<Layer> Layers
        {
            get
            {
                List<Layer> list = new List<Layer>();
                list.AddRange(this.layers.Values);
                return list.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets the <see cref="LineType">linetype</see> list.
        /// </summary>
        public ReadOnlyCollection<LineType> LineTypes
        {
            get
            {
                List<LineType> list = new List<LineType>();
                list.AddRange(this.lineTypes.Values);
                return list.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets the <see cref="TextStyle">text style</see> list.
        /// </summary>
        public ReadOnlyCollection<TextStyle> TextStyles
        {
            get
            {
                List<TextStyle> list = new List<TextStyle>();
                list.AddRange(this.textStyles.Values);
                return list.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets the <see cref="Block">block</see> list.
        /// </summary>
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

        /// <summary>
        /// Gets the <see cref="Arc">arc</see> list.
        /// </summary>
        public ReadOnlyCollection<Arc> Arcs
        {
            get { return this.arcs.AsReadOnly(); }
        }

        /// <summary>
        /// Gets the <see cref="Ellipse">ellipse</see> list.
        /// </summary>
        public ReadOnlyCollection<Ellipse> Ellipses
        {
            get { return this.ellipses.AsReadOnly(); }
        }

        /// <summary>
        /// Gets the <see cref="NurbsCurve">NURBS Curve</see> list.
        /// </summary>
        public ReadOnlyCollection<NurbsCurve> NurbsCurves
        {
            get { return this.nurbsCurves.AsReadOnly(); }
        }

        /// <summary>
        /// Gets the <see cref="Circle">circle</see> list.
        /// </summary>
        public ReadOnlyCollection<Circle> Circles
        {
            get { return this.circles.AsReadOnly(); }
        }

        /// <summary>
        /// Gets the <see cref="Face3d">3d face</see> list.
        /// </summary>
        public ReadOnlyCollection<Face3d> Faces3d
        {
            get { return this.faces3d.AsReadOnly(); }
        }

        /// <summary>
        /// Gets the <see cref="Solid">solid</see> list.
        /// </summary>
        public ReadOnlyCollection<Solid> Solids
        {
            get { return this.solids.AsReadOnly(); }
        }

        /// <summary>
        /// Gets the <see cref="Insert">insert</see> list.
        /// </summary>
        public ReadOnlyCollection<Insert> Inserts
        {
            get { return this.inserts.AsReadOnly(); }
        }

        /// <summary>
        /// Gets the <see cref="Line">line</see> list.
        /// </summary>
        public ReadOnlyCollection<Line> Lines
        {
            get { return this.lines.AsReadOnly(); }
        }

        /// <summary>
        /// Gets the <see cref="Polyline">polyline</see> list.
        /// </summary>
        /// <remarks>
        /// The polyline list contains all entities that are considered polylines in the dxf, they are:
        /// <see cref="Polyline">polylines</see>, <see cref="Polyline">3d polylines</see> and <see cref="PolyfaceMesh">polyface meshes</see>
        /// </remarks>
        public ReadOnlyCollection<Polyline> Polylines
        {
            get { return this.polylines.AsReadOnly(); }
        }

        /// <summary>
        /// Gets the <see cref="LightWeightPolyline">LightWeightPolyline</see> list.
        /// </summary>
        public ReadOnlyCollection<LwPolyline> LightWeightPolyline
        {
            get { return this.lightWeightPolylines.AsReadOnly(); }
        }

        /// <summary>
        /// Gets the <see cref="PolyfaceMesh">PolyfaceMesh</see> list.
        /// </summary>
        public ReadOnlyCollection<PolyfaceMesh> PolyfaceMesh
        {
            get { return this.polyfaceMeshes.AsReadOnly(); }
        }

        /// <summary>
        /// Gets the <see cref="netDxf.Entities.Point">point</see> list.
        /// </summary>
        public ReadOnlyCollection<Point> Points
        {
            get { return this.points.AsReadOnly(); }
        }

        /// <summary>
        /// Gets the <see cref="netDxf.Entities.Text">text</see> list.
        /// </summary>
        public ReadOnlyCollection<Text> Texts
        {
            get { return this.texts.AsReadOnly(); }
        }

        /// <summary>
        /// Gets the <see cref="netDxf.Entities.MText">multiline text</see> list.
        /// </summary>
        public ReadOnlyCollection<MText> MTexts
        {
            get { return this.mTexts.AsReadOnly(); }
        }

        /// <summary>
        /// Gets the <see cref="netDxf.Entities.Hatch">hatch</see> list.
        /// </summary>
        public ReadOnlyCollection<Hatch> Hatches
        {
            get { return this.hatches.AsReadOnly(); }
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

         /// <summary>
        /// Adds a new <see cref="IEntityObject">entity</see> to the document.
        /// </summary>
        /// <param name="entities">A list of <see cref="IEntityObject">entities</see></param>
        public void AddEntity(IEnumerable<IEntityObject> entities)
         {
             foreach (IEntityObject entity in entities)
             {
                 this.AddEntity(entity);
             }
         }

        /// <summary>
        /// Adds a new <see cref="IEntityObject">entity</see> to the document.
        /// </summary>
        /// <param name="entity">An <see cref="IEntityObject">entity</see></param>
        public void AddEntity(IEntityObject entity)
        {
            // check if the entity has not been added to the document
            if (this.addedObjects.ContainsKey(entity))
                throw new ArgumentException("The entity " + entity.Type + " object has already been added to the document.", "entity");

            this.addedObjects.Add(entity, entity);

            if (entity.XData != null)
            {
                foreach (ApplicationRegistry appReg in entity.XData.Keys)
                {
                    if (!this.appRegisterNames.ContainsKey(appReg.Name))
                    {
                        this.appRegisterNames.Add(appReg.Name, appReg);
                    }
                }
            }

            if (!this.layers.ContainsKey(entity.Layer.Name))
            {
                if (!this.lineTypes.ContainsKey(entity.Layer.LineType.Name))
                {
                    this.lineTypes.Add(entity.Layer.LineType.Name, entity.Layer.LineType);
                }
                this.layers.Add(entity.Layer.Name, entity.Layer);
            }

            if (!this.lineTypes.ContainsKey(entity.LineType.Name))
            {
                this.lineTypes.Add(entity.LineType.Name, entity.LineType);
            }

            switch (entity.Type)
            {
                case EntityType.Arc:
                    this.arcs.Add((Arc) entity);
                    break;
                case EntityType.Circle:
                    this.circles.Add((Circle) entity);
                    break;
                case EntityType.Ellipse:
                    this.ellipses.Add((Ellipse) entity);
                    break;
                case EntityType.NurbsCurve:
                    throw new NotImplementedException("Nurbs curves not avaliable at the moment.");
                    //this.nurbsCurves.Add((NurbsCurve) entity);
                    //break;
                case EntityType.Point:
                    this.points.Add((Point) entity);
                    break;
                case EntityType.Face3D:
                    this.faces3d.Add((Face3d) entity);
                    break;
                case EntityType.Solid:
                    this.solids.Add((Solid) entity);
                    break;
                case EntityType.Insert:
                    // if the block definition has already been added, we do not need to do anything else
                    if (!this.blocks.ContainsKey(((Insert) entity).Block.Name))
                    {
                        this.blocks.Add(((Insert) entity).Block.Name, ((Insert) entity).Block);

                        if (!this.layers.ContainsKey(((Insert)entity).Block.Layer.Name))
                        {
                            this.layers.Add(((Insert)entity).Block.Layer.Name, ((Insert)entity).Block.Layer);
                        }

                        //for new block definitions configure its entities
                        foreach (IEntityObject blockEntity in ((Insert) entity).Block.Entities)
                        {
                            // check if the entity has not been added to the document
                            if (this.addedObjects.ContainsKey(blockEntity))
                                throw new ArgumentException("The entity " + blockEntity.Type +
                                                            " object of the block " + ((Insert) entity).Block.Name +
                                                            " has already been added to the document.", "entity");
                            this.addedObjects.Add(blockEntity, blockEntity);

                            if (!this.layers.ContainsKey(blockEntity.Layer.Name))
                            {
                                this.layers.Add(blockEntity.Layer.Name, blockEntity.Layer);
                            }
                            if (!this.lineTypes.ContainsKey(blockEntity.LineType.Name))
                            {
                                this.lineTypes.Add(blockEntity.LineType.Name, blockEntity.LineType);
                            }
                        }
                        //for new block definitions configure its attributes
                        foreach (Attribute attribute in ((Insert) entity).Attributes)
                        {
                            if (!this.layers.ContainsKey(attribute.Layer.Name))
                            {
                                this.layers.Add(attribute.Layer.Name, attribute.Layer);
                            }
                            if (!this.lineTypes.ContainsKey(attribute.LineType.Name))
                            {
                                this.lineTypes.Add(attribute.LineType.Name, attribute.LineType);
                            }

                            AttributeDefinition attDef = attribute.Definition;
                            if (!this.layers.ContainsKey(attDef.Layer.Name))
                            {
                                this.layers.Add(attDef.Layer.Name, attDef.Layer);
                            }

                            if (!this.lineTypes.ContainsKey(attDef.LineType.Name))
                            {
                                this.lineTypes.Add(attDef.LineType.Name, attDef.LineType);
                            }

                            if (!this.textStyles.ContainsKey(attDef.Style.Name))
                            {
                                this.textStyles.Add(attDef.Style.Name, attDef.Style);
                            }
                        }
                    }

                    this.inserts.Add((Insert) entity);
                    break;
                case EntityType.Line:
                    this.lines.Add((Line) entity);
                    break;
                case EntityType.LightWeightPolyline:
                    this.lightWeightPolylines.Add((LwPolyline)entity);
                    break;
                case EntityType.Polyline3d:
                    this.polylines.Add((Polyline) entity);
                    break;
                case EntityType.PolyfaceMesh:
                    this.polyfaceMeshes.Add((PolyfaceMesh)entity);
                    break;
                case EntityType.Text:
                    if (!this.textStyles.ContainsKey(((Text) entity).Style.Name))
                    {
                        this.textStyles.Add(((Text) entity).Style.Name, ((Text) entity).Style);
                    }
                    this.texts.Add((Text) entity);
                    break;
                case EntityType.MText:
                    if (!this.textStyles.ContainsKey(((MText) entity).Style.Name))
                    {
                        this.textStyles.Add(((MText) entity).Style.Name, ((MText) entity).Style);
                    }
                    this.mTexts.Add((MText) entity);
                    break;
                case EntityType.Hatch:
                    this.hatches.Add((Hatch) entity);
                    break;
                case EntityType.Vertex:
                    throw new ArgumentException("The entity " + entity.Type + " is only allowed as part of another entity", "entity");

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

        /// <summary>
        /// Loads a dxf ASCII file.
        /// </summary>
        /// <param name="file">File name.</param>
        public void Load(string file)
        {
            if (!File.Exists(file))
                throw new FileNotFoundException("File " + file + " not found.", file);

            this.fileName = Path.GetFileNameWithoutExtension(file);

            // In dxf files the decimal point is always a dot. We have to make sure that this doesn't interfere with the system configuration.
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            DxfReader dxfReader = new DxfReader(file);
            dxfReader.Open();
            dxfReader.Read();
            dxfReader.Close();

            //header information
            this.version = dxfReader.Version;
            this.handleCount = Convert.ToInt32(dxfReader.HandleSeed,16);

            //tables information
            this.appRegisterNames = dxfReader.ApplicationRegistrationIds;
            this.layers = dxfReader.Layers;
            this.lineTypes = dxfReader.LineTypes;
            this.textStyles = dxfReader.TextStyles;
            this.blocks = dxfReader.Blocks;

            //entities information
            this.arcs = dxfReader.Arcs;
            this.circles = dxfReader.Circles;
            this.ellipses = dxfReader.Ellipses;
            this.points = dxfReader.Points;
            this.faces3d = dxfReader.Faces3d;
            this.solids = dxfReader.Solids;
            this.lightWeightPolylines = dxfReader.LightWeightPolyline;
            this.polylines = dxfReader.Polylines;
            this.polyfaceMeshes = dxfReader.PolyfaceMeshes;
            this.lines = dxfReader.Lines;
            this.inserts = dxfReader.Inserts;
            this.texts = dxfReader.Texts;
            this.mTexts = dxfReader.MTexts;
            this.hatches = dxfReader.Hatches;

            Thread.CurrentThread.CurrentCulture = cultureInfo;

        }

        /// <summary>
        /// Saves the database of the actual DxfDocument to a dxf ASCII file.
        /// </summary>
        /// <param name="file">File name.</param>
        /// <param name="dxfVersion">Dxf file <see cref="DxfVersion">version</see>.</param>
        public void Save(string file, DxfVersion dxfVersion)
        {
            // we will start counting the handles of the document elements after the few reserved by the writer for internal use
            this.handleCount = DxfWriter.ReservedHandles;

            AsignHandlers();
            this.fileName = Path.GetFileNameWithoutExtension(file);
            this.version = StringEnum.GetStringValue(dxfVersion);

            // create the list for the block record table
            Dictionary<string, List<IEntityObject>> blockEntities = new Dictionary<string, List<IEntityObject>>();
            foreach (Block block in this.blocks.Values)
            {
                blockEntities.Add(block.Name, new List<IEntityObject>());
                foreach (IEntityObject entity in block.Entities)
                {
                    blockEntities[block.Name].Add(entity);
                }
            }

            // In dxf files the decimal point is always a dot. We have to make sure that this doesn't interfere with the system configuration.
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            DxfWriter dxfWriter = new DxfWriter(file, dxfVersion);
            dxfWriter.Open();
            dxfWriter.WriteComment("Dxf file generated by netDxf, Copyright(C) 2012 Daniel Carvajal, Licensed under LGPL");

            //HEADER SECTION
            dxfWriter.BeginSection(StringCode.HeaderSection);
            dxfWriter.WriteSystemVariable(new HeaderVariable(SystemVariable.DatabaseVersion, this.version));
            dxfWriter.WriteSystemVariable(new HeaderVariable(SystemVariable.HandSeed, Convert.ToString(this.handleCount, 16)));
            dxfWriter.EndSection();

            ////CLASSES SECTION
            //dxfWriter.BeginSection(StringCode.ClassesSection);
            //dxfWriter.EndSection();

            //TABLES SECTION
            dxfWriter.BeginSection(StringCode.TablesSection);

            //viewport tables
            dxfWriter.BeginTable(StringCode.ViewPortTable);
            foreach (ViewPort vport in this.viewports.Values)
            {
                dxfWriter.WriteViewPort(vport);
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

            //view
            dxfWriter.BeginTable(StringCode.ViewTable);
            dxfWriter.EndTable();

            //ucs
            dxfWriter.BeginTable(StringCode.UcsTable);
            dxfWriter.EndTable();

            //registered application tables
            dxfWriter.BeginTable(StringCode.ApplicationIDTable);
            foreach (ApplicationRegistry id in this.appRegisterNames.Values)
            {
                dxfWriter.RegisterApplication(id);
            }
            dxfWriter.EndTable();

            //dimension style tables
            dxfWriter.BeginTable(StringCode.DimensionStyleTable);
            foreach (DimensionStyle style in this.dimStyles.Values)
            {
                dxfWriter.WriteDimensionStyle(style);
            }
            dxfWriter.EndTable();

            //block reacord table
            dxfWriter.BeginTable(StringCode.BlockRecordTable);
            foreach (Block block in this.blocks.Values)
            {
                dxfWriter.WriteBlockRecord(block.Record);
            }
            dxfWriter.EndTable();

            dxfWriter.EndSection(); //End section tables

            dxfWriter.BeginSection(StringCode.BlocksSection);
            foreach (Block block in this.blocks.Values)
            {
                dxfWriter.WriteBlock(block, blockEntities[block.Name]);
            }

            dxfWriter.EndSection(); //End section blocks

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
            foreach (Ellipse ellipse  in this.ellipses)
            {
                dxfWriter.WriteEntity(ellipse);
            }
            foreach (NurbsCurve nurbsCurve  in this.nurbsCurves)
            {
                dxfWriter.WriteEntity(nurbsCurve);
            }
            foreach (Point point in this.points)
            {
                dxfWriter.WriteEntity(point);
            }
            foreach (Face3d face in this.faces3d)
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
            foreach (LwPolyline pol in this.lightWeightPolylines)
            {
                dxfWriter.WriteEntity(pol);
            }
            foreach (PolyfaceMesh pol in this.polyfaceMeshes)
            {
                dxfWriter.WriteEntity(pol);
            }
            foreach (Polyline pol in this.polylines)
            {
               dxfWriter.WriteEntity(pol);
            }
            foreach (Text text in this.texts)
            {
                dxfWriter.WriteEntity(text);
            }
            foreach (MText mText in this.mTexts)
            {
                dxfWriter.WriteEntity(mText);
            }
            foreach (Hatch hatch in this.hatches)
            {
                dxfWriter.WriteEntity(hatch);
            }

            dxfWriter.EndSection(); //End section entities

            //OBJECTS SECTION
            dxfWriter.BeginSection(StringCode.ObjectsSection);
            dxfWriter.WriteDictionary(Dictionary.Default);
            dxfWriter.EndSection();

            dxfWriter.Close();

            Thread.CurrentThread.CurrentCulture = cultureInfo;
        }
         
        #endregion

        #region private methods
		 
        private void AddDefaultObjects()
        {
            //add default viewports
            ViewPort active = ViewPort.Active;
            this.viewports.Add(active.Name, active);

            //add default layer
            Layer byDefault = Layer.Default;
            this.layers.Add(byDefault.Name, byDefault);

            // add default line types
            LineType byLayer = LineType.ByLayer;
            LineType byBlock = LineType.ByBlock;
            this.lineTypes.Add(byLayer.Name, byLayer);
            this.lineTypes.Add(byBlock.Name, byBlock);

            // add default blocks
            Block modelSpace = Block.ModelSpace;
            Block paperSpace = Block.PaperSpace;
            this.blocks.Add(modelSpace.Name, modelSpace);
            this.blocks.Add(paperSpace.Name, paperSpace);

            // add default text style
            TextStyle defaultStyle = TextStyle.Default;
            this.textStyles.Add(defaultStyle.Name, defaultStyle);

            // add default application registry
            ApplicationRegistry defaultAppId = ApplicationRegistry.Default;
            this.appRegisterNames.Add(defaultAppId.Name, defaultAppId);

            // add default dimension style
            DimensionStyle defaultDimStyle = DimensionStyle.Default;
            this.dimStyles.Add(defaultDimStyle.Name, defaultDimStyle);
        }

        private void AsignHandlers()
        {
            // assign handles to the document tables
            foreach (ViewPort viewPort in this.viewports.Values)
            {
                this.handleCount = viewPort.AsignHandle(this.handleCount);
            }
            
            Layer.PlotStyleHandle = Convert.ToString(this.handleCount++, 16);
            
            foreach (Layer layer in this.layers.Values)
            {
                this.handleCount = layer.AsignHandle(this.handleCount);
            }
            foreach (LineType lineType in this.lineTypes.Values)
            {
                this.handleCount = lineType.AsignHandle(this.handleCount);
            }
            foreach (TextStyle textStyle in this.textStyles.Values)
            {
                this.handleCount = textStyle.AsignHandle(this.handleCount);
            }
            foreach (Block block in this.blocks.Values)
            {
                this.handleCount = block.AsignHandle(this.handleCount);
            }
            foreach (ApplicationRegistry appId in this.appRegisterNames.Values)
            {
                this.handleCount = appId.AsignHandle(this.handleCount);
            }
            foreach (DimensionStyle style in this.dimStyles.Values)
            {
                this.handleCount = style.AsignHandle(this.handleCount);
            }

            // assign handles to the document entities
            foreach (Arc entity in this.arcs)
            {
                this.handleCount = entity.AsignHandle(this.handleCount);
            }
            foreach (Ellipse entity in this.ellipses)
            {
                this.handleCount = entity.AsignHandle(this.handleCount);
            }
            foreach (Face3d entity in this.faces3d)
            {
                this.handleCount = entity.AsignHandle(this.handleCount);
            }
            foreach (Solid entity in this.solids )
            {
                this.handleCount = entity.AsignHandle(this.handleCount);
            }
            foreach (Insert entity in this.inserts )
            {
                this.handleCount = entity.AsignHandle(this.handleCount);
            }
            foreach (LwPolyline entity in this.lightWeightPolylines)
            {
                this.handleCount = (entity).AsignHandle(this.handleCount);
            }
            foreach (PolyfaceMesh entity in this.polyfaceMeshes)
            {
                this.handleCount = (entity).AsignHandle(this.handleCount);
            }
            foreach (Polyline entity in this.polylines)
            {
                this.handleCount = (entity).AsignHandle(this.handleCount);
            }
            foreach (Line entity in this.lines)
            {
                this.handleCount = entity.AsignHandle(this.handleCount);
            }
            foreach (Circle entity in this.circles)
            {
                this.handleCount = entity.AsignHandle(this.handleCount);
            }
            foreach (Point  entity in this.points)
            {
                this.handleCount = entity.AsignHandle(this.handleCount);
            }
            foreach (Text entity in this.texts)
            {
                this.handleCount = entity.AsignHandle(this.handleCount);
            }
            foreach (MText entity in this.mTexts)
            {
                this.handleCount = entity.AsignHandle(this.handleCount);
            }
            foreach (Hatch entity in this.hatches)
            {
                this.handleCount = entity.AsignHandle(this.handleCount);
            }
        }
        #endregion
    }
}