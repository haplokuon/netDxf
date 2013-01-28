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
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using netDxf.Blocks;
using netDxf.Entities;
using netDxf.Header;
using netDxf.Objects;
using netDxf.Tables;
using Attribute = netDxf.Entities.Attribute;

namespace netDxf
{
    /// <summary>
    /// Low level dxf reader
    /// </summary>
    internal sealed class DxfReader
    {
        #region private fields

        private CodeValuePair dxfPairInfo;
        private readonly string file;
        private int fileLine;
        private bool isFileOpen;
        private Stream input;
        private StreamReader reader;

        //header
        private List<string> comments;
        private string version;
        private string handleSeed;
        private DefaultDrawingUnits drawingUnits;

        //entities
        Hashtable addedObjects;
        private List<Arc> arcs;
        private List<Circle> circles;
        private List<Point> points;
        private List<Ellipse> ellipses;
        private List<Face3d> faces3d;
        private List<Solid> solids;
        private List<Insert> inserts;
        private List<Line> lines;
        private List<PolyfaceMesh> polyfaceMeshes;
        private List<LwPolyline> lightWeightPolylines;
        private List<Polyline> polylines;
        private List<Text> texts;
        private List<MText> mTexts;
        private List<Hatch> hatches;
        private List<Dimension> dimensions;
        private List<Spline> splines;
        private List<Image> images;

        //tables
        private Dictionary<string, ApplicationRegistry> appIds;
        private Dictionary<string, BlockRecord> blockRecords;
        private Dictionary<string, Layer> layers;
        private Dictionary<string, LineType> lineTypes;
        private Dictionary<string, TextStyle> textStyles;
        private Dictionary<string, DimensionStyle> dimStyles;

        //blocks
        private Dictionary<string, Block> blocks;
        private int dimCount; // keeps track of the dimension blocks generated

        // in nested blocks (blocks that contains Insert entities) the block definition might be defined AFTER the block that references them
        // this variables will store temporary information to post process the nested block list
        private Dictionary<Insert, string> nestedBlocks;
        private Dictionary<string, Dimension> nestedDimBlocks;
        private Dictionary<Attribute, string> nestedBlocksAttributes;

        //objects
        private RasterVariables rasterVariables;
        private Dictionary<string, ImageDef> imageDefs;
        private Dictionary<string, DictionaryObject> dictionaries;
        private Dictionary<string, ImageDefReactor> imageDefReactors;

        // the image definitions are defined AFTER the image that references them,
        // temporary this variables will store information to post process the image list
        private Dictionary<Image, string> imgToImgDefHandles;
        private Dictionary<string, ImageDef> imgDefHandles;
        
        #endregion

        #region constructors

        public DxfReader(string file)
        {
            this.file = file;
        }

        #endregion

        #region public properties

        public bool IsFileOpen
        {
            get { return this.isFileOpen; }
        }

        public List<string> Comments
        {
            get { return this.comments; }
        }

        public int DimBlockCount
        {
            get { return this.dimCount; }
        }

        #endregion

        #region public header properties

        public string HandleSeed
        {
            get { return this.handleSeed; }
        }

        public string Version
        {
            get { return this.version; }
        }

        public DefaultDrawingUnits DrawingUnits
        {
            get { return drawingUnits; }
        }

        #endregion

        #region public entity properties

        public List<Arc> Arcs
        {
            get { return this.arcs; }
        }

        public List<Circle> Circles
        {
            get { return this.circles; }
        }

        public List<Dimension> Dimensions
        {
            get { return this.dimensions; }
        }

        public List<Ellipse> Ellipses
        {
            get { return this.ellipses; }
        }

        public List<Point> Points
        {
            get { return this.points; }
        }

        public List<Face3d> Faces3d
        {
            get { return this.faces3d; }
        }

        public List<Solid> Solids
        {
            get { return this.solids; }
        }

        public List<Spline> Splines
        {
            get { return this.splines; }
        }

        public List<Line> Lines
        {
            get { return this.lines; }
        }

        public List<LwPolyline> LightWeightPolyline
        {
            get { return this.lightWeightPolylines; }
        }

        public List<Polyline> Polylines
        {
            get { return this.polylines; }
        }

        public List<PolyfaceMesh> PolyfaceMeshes
        {
            get { return this.polyfaceMeshes; }
        }

        public List<Insert> Inserts
        {
            get { return this.inserts; }
        }

        public List<Text> Texts
        {
            get { return this.texts; }
        }

        public List<MText> MTexts
        {
            get { return this.mTexts; }
        }

        public List<Hatch> Hatches
        {
            get { return this.hatches; }
        }

        public List<Image> Images
        {
            get { return images; }
        }

        #endregion

        #region public table properties

        public Dictionary<string, ApplicationRegistry> ApplicationRegistrationIds
        {
            get { return this.appIds; }
        }

        public Dictionary<string, Layer> Layers
        {
            get { return this.layers; }
        }

        public Dictionary<string, LineType> LineTypes
        {
            get { return this.lineTypes; }
        }

        public Dictionary<string, TextStyle> TextStyles
        {
            get { return this.textStyles; }
        }

        public Dictionary<string, DimensionStyle> DimensionStyles
        {
            get { return this.dimStyles; }
        }

        public Dictionary<string, Block> Blocks
        {
            get { return this.blocks; }
        }


        #endregion

        #region public object properties

        public Dictionary<string, ImageDef> ImageDefs
        {
            get { return imageDefs; }
        }

        public RasterVariables RasterVariables
        {
            get { return rasterVariables; }
        }

        #endregion

        #region public methods

        public void Open()
        {
            try
            {
                this.input = File.OpenRead(this.file);
                this.reader = new StreamReader(this.input, Encoding.ASCII);
                this.isFileOpen = true;
            }
            catch (Exception ex)
            {
                throw (new DxfException(this.file, "Error al intentar abrir el archivo.", ex));
            }
        }

        public void Close()
        {
            if (this.isFileOpen)
            {
                this.reader.Close();
                this.input.Close();
            }
            this.isFileOpen = false;
        }

        public void Read()
        {
            this.addedObjects = new Hashtable();

            this.comments = new List<string>();

            // tables
            this.appIds = new Dictionary<string, ApplicationRegistry>();
            this.blockRecords = new Dictionary<string, BlockRecord>();
            this.layers = new Dictionary<string, Layer>();
            this.lineTypes = new Dictionary<string, LineType>();
            this.textStyles = new Dictionary<string, TextStyle>();
            this.dimStyles = new Dictionary<string, DimensionStyle>();

            // blocks
            this.nestedBlocks = new Dictionary<Insert, string>();
            this.nestedDimBlocks = new Dictionary<string, Dimension>();
            this.nestedBlocksAttributes = new Dictionary<Attribute, string>();
            this.blocks = new Dictionary<string, Block>();

            // entities
            this.arcs = new List<Arc>();
            this.circles = new List<Circle>();
            this.faces3d = new List<Face3d>();
            this.ellipses = new List<Ellipse>();
            this.solids = new List<Solid>();
            this.inserts = new List<Insert>();
            this.lines = new List<Line>();
            this.polyfaceMeshes = new List<PolyfaceMesh>();
            this.lightWeightPolylines = new List<LwPolyline>();
            this.polylines = new List<Polyline>();
            this.points = new List<Point>();
            this.texts = new List<Text>();
            this.mTexts = new List<MText>();
            this.hatches = new List<Hatch>();
            this.dimensions = new List<Dimension>();
            this.splines = new List<Spline>();
            this.images = new List<Image>();

            // objects
            this.dictionaries = new Dictionary<string, DictionaryObject>();
            this.imageDefs = new Dictionary<string, ImageDef>();
            this.imageDefReactors = new Dictionary<string, ImageDefReactor>();
            this.imgDefHandles = new Dictionary<string, ImageDef>();
            this.imgToImgDefHandles = new Dictionary<Image, string>();

            this.fileLine = -1;

            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Value != StringCode.EndOfFile)
            {
                if (dxfPairInfo.Value == StringCode.BeginSection)
                {
                    dxfPairInfo = this.ReadCodePair();
                    switch (dxfPairInfo.Value)
                    {
                        case StringCode.HeaderSection:
                            this.ReadHeader();
                            break;
                        case StringCode.ClassesSection:
                            this.ReadClasses();
                            break;
                        case StringCode.TablesSection:
                            this.ReadTables();
                            break;
                        case StringCode.BlocksSection:
                            this.ReadBlocks();
                            break;
                        case StringCode.EntitiesSection:
                            this.ReadEntities();
                            break;
                        case StringCode.ObjectsSection:
                            this.ReadObjects();
                            break;
                        default:
                            throw new InvalidDxfSectionException(dxfPairInfo.Value, this.file, "Unknown section " + dxfPairInfo.Value + " line " + this.fileLine);
                    }
                }
                dxfPairInfo = this.ReadCodePair();
            }

            // postprocess the image list to assign their image definitions.
            foreach (KeyValuePair<Image, string> pair in this.imgToImgDefHandles)
            {
                Image image = pair.Key;
                image.Definition = this.imgDefHandles[pair.Value];
                image.Definition.Reactors.Add(image.Handle, this.imageDefReactors[image.Handle]);
            }
        }

        #endregion

        #region sections methods

        private void ReadHeader()
        {
            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Value != StringCode.EndSection)
            {
                if (HeaderVariable.Allowed.ContainsKey(dxfPairInfo.Value))
                {
                    int codeGroup = HeaderVariable.Allowed[dxfPairInfo.Value];
                    string variableName = dxfPairInfo.Value;
                    dxfPairInfo = this.ReadCodePair();
                    if (dxfPairInfo.Code != codeGroup)
                        throw new DxfHeaderVariableException(variableName, this.file, "Invalid variable name and code group convination in line " + this.fileLine);
                    switch (variableName)
                    {
                        case SystemVariable.DatabaseVersion:
                            this.version = dxfPairInfo.Value;
                            break;
                        case SystemVariable.HandSeed:
                            this.handleSeed = dxfPairInfo.Value;
                            break;
                        case SystemVariable.Angbase:
                            break;
                        case SystemVariable.Angdir:
                            break;
                        case SystemVariable.Extnames:
                            break;
                        case SystemVariable.Insunits:
                            this.drawingUnits = (DefaultDrawingUnits) int.Parse(dxfPairInfo.Value);
                            break;
                    }
                }
                dxfPairInfo = this.ReadCodePair();
            }
        }

        private void ReadClasses()
        {
            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Value != StringCode.EndSection)
            {
                dxfPairInfo = this.ReadCodePair();
            }
        }

        private void ReadTables()
        {
            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Value != StringCode.EndSection)
            {
                dxfPairInfo = this.ReadCodePair();
                switch (dxfPairInfo.Value)
                {
                    case StringCode.ApplicationIDTable:
                        Debug.Assert(dxfPairInfo.Code == 2);
                        this.ReadApplicationsId();
                        break;
                    case StringCode.BlockRecordTable:
                        Debug.Assert(dxfPairInfo.Code == 2);
                        this.ReadBlockRecords();
                        break;
                    case StringCode.LayerTable:
                        Debug.Assert(dxfPairInfo.Code == 2);
                        this.ReadLayers();
                        break;
                    case StringCode.LineTypeTable:
                        Debug.Assert(dxfPairInfo.Code == 2);
                        this.ReadLineTypes();
                        break;
                    case StringCode.TextStyleTable:
                        Debug.Assert(dxfPairInfo.Code == 2);
                        this.ReadTextStyles();
                        break;
                    case StringCode.DimensionStyleTable:
                        Debug.Assert(dxfPairInfo.Code == 2);
                        this.ReadDimStyles();
                        break;
                }
            }
        }

        private void ReadApplicationsId()
        {
            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Value != StringCode.EndTable)
            {
                if (dxfPairInfo.Value == StringCode.ApplicationIDTable)
                {
                    Debug.Assert(dxfPairInfo.Code == 0);
                    ApplicationRegistry appId = this.ReadApplicationId();
                    this.appIds.Add(appId.Name, appId);
                }
                else
                {
                    dxfPairInfo = this.ReadCodePair();
                }
            }
        }

        private ApplicationRegistry ReadApplicationId()
        {
            string appId = string.Empty;
            string handle = string.Empty;
            dxfPairInfo = this.ReadCodePair();

            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 2:
                        if (string.IsNullOrEmpty(dxfPairInfo.Value))
                        {
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value, this.file,
                                                                         "Invalid value " + dxfPairInfo.Value + " in code " + dxfPairInfo.Code + " line " + this.fileLine);
                        }
                        appId = dxfPairInfo.Value;
                        break;
                    case 5:
                        handle = dxfPairInfo.Value;
                        break;
                }
                dxfPairInfo = this.ReadCodePair();
            }

            return new ApplicationRegistry(appId)
                       {
                           Handle = handle
                       };
        }

        private void ReadBlocks()
        {
            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Value != StringCode.EndSection)
            {
                switch (dxfPairInfo.Value)
                {
                    case StringCode.BeginBlock:
                        Block block = this.ReadBlock();
                        this.blocks.Add(block.Name, block);
                        break;
                    default:
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            // post process the possible nested blocks
            foreach (KeyValuePair<Insert,  string> pair in this.nestedBlocks)
            {
                Insert insert = pair.Key;
                insert.Block = this.blocks[pair.Value];
                foreach (Attribute att in insert.Attributes)
                {
                    string attDefId = this.nestedBlocksAttributes[att];
                    att.Definition = insert.Block.Attributes[attDefId];
                }
            }
            foreach (KeyValuePair<string, Dimension> pair in this.nestedDimBlocks)
            {
                pair.Value.Block = this.blocks[pair.Key];
            }
        }

        private Block ReadBlock()
        {
            BlockRecord blockRecord = null;
            Layer layer = null;
            string name = string.Empty;
            string handle = string.Empty;
            BlockTypeFlags type = BlockTypeFlags.None;
            Vector3 basePoint = Vector3.Zero;
            List<IEntityObject> entities = new List<IEntityObject>();
            Dictionary<string, AttributeDefinition> attdefs = new Dictionary<string, AttributeDefinition>();

            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Value != StringCode.EndBlock)
            {
                switch (dxfPairInfo.Code)
                {
                    case 5:
                        handle = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 8:
                        layer = this.GetLayer(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 2:
                        name = dxfPairInfo.Value;
                        if (!this.blockRecords.TryGetValue(name, out blockRecord))
                            throw new ClosedDxfTableException(StringCode.BlockRecordTable, "The block record " + name + " is not defined");
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 70:
                        type = (BlockTypeFlags) int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 10:
                        basePoint.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        basePoint.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        basePoint.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 3:
                        name = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 0: // entity
                        IEntityObject entity = this.ReadBlockEntity();
                        if (entity != null)
                            if (entity.Type == EntityType.AttributeDefinition)
                                attdefs.Add(((AttributeDefinition) entity).Id, (AttributeDefinition) entity);
                            else
                                entities.Add(entity);
                        break;
                    default:
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            // read the end bloc object until a new element is found
            dxfPairInfo = this.ReadCodePair();
            string endBlockHandle = string.Empty;
            Layer endBlockLayer = layer;
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 5:
                        endBlockHandle = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 8:
                        endBlockLayer = this.GetLayer(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    default:
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }
            Block block = new Block(name)
                              {
                                  Record = blockRecord,
                                  Position = basePoint,
                                  Layer = layer,
                                  Entities = entities,
                                  Attributes = attdefs,
                                  Handle = handle,
                                  TypeFlags = type
                              };
            block.End.Handle = endBlockHandle;
            block.End.Layer = endBlockLayer;

            return block;
        }

        private IEntityObject ReadBlockEntity()
        {
            IEntityObject entity = null;

            switch (dxfPairInfo.Value)
            {
                case DxfObjectCode.Arc:
                    entity = this.ReadArc();
                    break;
                case DxfObjectCode.Circle:
                    entity = this.ReadCircle();
                    break;
                case DxfObjectCode.Dimension:
                    entity = this.ReadDimension(true);
                    break;
                case DxfObjectCode.Ellipse:
                    entity = this.ReadEllipse();
                    break;
                case DxfObjectCode.Face3D:
                    entity = this.ReadFace3D();
                    break;
                case DxfObjectCode.Hatch:
                    entity = this.ReadHatch();
                    break;
                case DxfObjectCode.Insert:
                    entity = this.ReadInsert(true);
                    break;
                case DxfObjectCode.Line:
                    entity = this.ReadLine();
                    break;
                case DxfObjectCode.LightWeightPolyline:
                    entity = this.ReadLwPolyline();
                    break;
                case DxfObjectCode.MText:
                    entity = this.ReadMText();
                    break;
                case DxfObjectCode.Point:
                    entity = this.ReadPoint();
                    break;
                case DxfObjectCode.Polyline:
                    entity = this.ReadPolyline();
                    if (entity is LwPolyline)
                        this.lightWeightPolylines.Add((LwPolyline) entity);
                    if (entity is PolyfaceMesh)
                        this.polyfaceMeshes.Add((PolyfaceMesh) entity);
                    if (entity is Polyline)
                        this.polylines.Add((Polyline) entity);
                    break;
                case DxfObjectCode.Solid:
                    entity = this.ReadSolid();
                    break;
                case DxfObjectCode.Text:
                    entity = this.ReadText();
                    break;
                case DxfObjectCode.Spline:
                    entity = this.ReadSpline();
                    break;
                case DxfObjectCode.Image:
                    entity = this.ReadImage();
                    break;
                case DxfObjectCode.AttributeDefinition:
                    entity = this.ReadAttributeDefinition();
                    break;
                default:
                    ReadUnknowEntity();
                    break;
            }
            if (entity != null) this.addedObjects.Add(entity, entity);
            return entity;
        }

        private AttributeDefinition ReadAttributeDefinition()
        {
            string handle = string.Empty;
            string id = string.Empty;
            string text = string.Empty;
            object value = null;
            AttributeFlags flags = AttributeFlags.Visible;
            Vector3 firstAlignmentPoint = Vector3.Zero;
            Vector3 secondAlignmentPoint = Vector3.Zero;
            Layer layer = Layer.Default;
            AciColor color = AciColor.ByLayer;
            LineType lineType = LineType.ByLayer;
            TextStyle style = TextStyle.Default;
            double height = 0.0;
            double widthFactor = 0.0;
            int horizontalAlignment = 0;
            int verticalAlignment = 0;
            double rotation = 0.0;
            Vector3 normal = Vector3.UnitZ;

            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 5:
                        handle = dxfPairInfo.Value;
                        break;
                    case 2:
                        id = dxfPairInfo.Value;
                        break;
                    case 3:
                        text = dxfPairInfo.Value;
                        break;
                    case 1:
                        value = dxfPairInfo.Value;
                        break;
                    case 8: //layer code
                        layer = this.GetLayer(dxfPairInfo.Value);
                        break;
                    case 62: //aci color code
                        color = new AciColor(short.Parse(dxfPairInfo.Value));
                        break;
                    case 6: //type line code
                        lineType = this.GetLineType(dxfPairInfo.Value);
                        break;
                    case 70:
                        flags = (AttributeFlags) int.Parse(dxfPairInfo.Value);
                        break;
                    case 10:
                        firstAlignmentPoint.X = double.Parse(dxfPairInfo.Value);
                        break;
                    case 20:
                        firstAlignmentPoint.Y = double.Parse(dxfPairInfo.Value);
                        break;
                    case 30:
                        firstAlignmentPoint.Z = double.Parse(dxfPairInfo.Value);
                        break;
                    case 11:
                        secondAlignmentPoint.X = double.Parse(dxfPairInfo.Value);
                        break;
                    case 21:
                        secondAlignmentPoint.Y = double.Parse(dxfPairInfo.Value);
                        break;
                    case 31:
                        secondAlignmentPoint.Z = double.Parse(dxfPairInfo.Value);
                        break;
                    case 7:
                        style = this.GetTextStyle(dxfPairInfo.Value);
                        break;
                    case 40:
                        height = double.Parse(dxfPairInfo.Value);
                        break;
                    case 41:
                        widthFactor = double.Parse(dxfPairInfo.Value);
                        break;
                    case 50:
                        rotation = double.Parse(dxfPairInfo.Value);
                        break;
                    case 72:
                        horizontalAlignment = int.Parse(dxfPairInfo.Value);
                        break;
                    case 74:
                        verticalAlignment = int.Parse(dxfPairInfo.Value);
                        break;
                    case 210:
                        normal.X = double.Parse(dxfPairInfo.Value);
                        break;
                    case 220:
                        normal.Y = double.Parse(dxfPairInfo.Value);
                        break;
                    case 230:
                        normal.Z = double.Parse(dxfPairInfo.Value);
                        break;
                }

                dxfPairInfo = this.ReadCodePair();
            }

            TextAlignment alignment = ObtainAlignment(horizontalAlignment, verticalAlignment);
            Vector3 ocsBasePoint = alignment == TextAlignment.BaselineLeft ? firstAlignmentPoint : secondAlignmentPoint;

            return new AttributeDefinition(id)
                       {
                           Position = MathHelper.Transform(ocsBasePoint, normal, MathHelper.CoordinateSystem.Object, MathHelper.CoordinateSystem.World),
                           Normal = normal,
                           Alignment = alignment,
                           Text = text,
                           Value = value,
                           Flags = flags,
                           Layer = layer,
                           Color = color,
                           LineType = lineType,
                           Style = style,
                           Height = height,
                           WidthFactor = MathHelper.IsZero(widthFactor) ? style.WidthFactor : widthFactor,
                           Rotation = rotation,
                           Handle = handle
                       };
        }

        private Attribute ReadAttribute(Block block, bool isNestedBlock = false)
        {
            string handle = string.Empty;
            string attdefId = null;
            AttributeDefinition attdef = null;
            Layer layer = Layer.Default;
            AciColor color = AciColor.ByLayer;
            LineType lineType = LineType.ByLayer;
            Object value = null;
            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 5:
                        handle = dxfPairInfo.Value;
                        break;
                    case 2:
                        attdefId = dxfPairInfo.Value;
                        if(!isNestedBlock) attdef = block.Attributes[attdefId];
                        break;
                    case 1:
                        value = dxfPairInfo.Value;
                        break;
                    case 8: //layer code
                        layer = this.GetLayer(dxfPairInfo.Value);
                        break;
                    case 62: //aci color code
                        color = new AciColor(short.Parse(dxfPairInfo.Value));
                        break;
                    case 6: //type line code
                        lineType = this.GetLineType(dxfPairInfo.Value);
                        break;
                }
                dxfPairInfo = this.ReadCodePair();
            }
            
            Attribute att = new Attribute
                       {
                           Definition = attdef,
                           Color = color,
                           Layer = layer,
                           LineType = lineType,
                           Value = value,
                           Handle = handle
                       };

            if (isNestedBlock)
                if (attdefId != null) this.nestedBlocksAttributes.Add(att, attdefId);
                
            return att;
        }

        private void ReadEntities()
        {
            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Value != StringCode.EndSection)
            {
                IEntityObject entity = null;
                switch (dxfPairInfo.Value)
                {
                    case DxfObjectCode.Arc:
                        entity = this.ReadArc();
                        this.arcs.Add((Arc) entity);
                        break;
                    case DxfObjectCode.Circle:
                        entity = this.ReadCircle();
                        this.circles.Add((Circle) entity);
                        break;
                    case DxfObjectCode.Dimension:
                        entity = this.ReadDimension();
                        this.dimensions.Add((Dimension) entity);
                        break;
                    case DxfObjectCode.Point:
                        entity = this.ReadPoint();
                        this.points.Add((Point) entity);
                        break;
                    case DxfObjectCode.Ellipse:
                        entity = this.ReadEllipse();
                        this.ellipses.Add((Ellipse) entity);
                        break;
                    case DxfObjectCode.Face3D:
                        entity = this.ReadFace3D();
                        this.faces3d.Add((Face3d) entity);
                        break;
                    case DxfObjectCode.Solid:
                        entity = this.ReadSolid();
                        this.solids.Add((Solid) entity);
                        break;
                    case DxfObjectCode.Spline:
                        entity = this.ReadSpline();
                        this.splines.Add((Spline)entity);
                        break;
                    case DxfObjectCode.Insert:
                        entity = this.ReadInsert();
                        this.inserts.Add((Insert) entity);
                        break;
                    case DxfObjectCode.Line:
                        entity = this.ReadLine();
                        this.lines.Add((Line) entity);
                        break;
                    case DxfObjectCode.LightWeightPolyline:
                        entity = this.ReadLwPolyline();
                        this.lightWeightPolylines.Add((LwPolyline) entity);
                        break;
                    case DxfObjectCode.Polyline:
                        entity = this.ReadPolyline();
                        if (entity is LwPolyline)
                            this.lightWeightPolylines.Add((LwPolyline) entity);
                        if (entity is PolyfaceMesh)
                            this.polyfaceMeshes.Add((PolyfaceMesh) entity);
                        if (entity is Polyline)
                            this.polylines.Add((Polyline) entity);
                        break;
                    case DxfObjectCode.Text:
                        entity = this.ReadText();
                        this.texts.Add((Text) entity);
                        break;
                    case DxfObjectCode.MText:
                        entity = this.ReadMText();
                        this.mTexts.Add((MText) entity);
                        break;
                    case DxfObjectCode.Hatch:
                        entity = this.ReadHatch();
                        this.hatches.Add((Hatch) entity);
                        break;
                    case DxfObjectCode.Image:
                        entity = this.ReadImage();
                        this.images.Add((Image)entity);
                        break;
                    default:
                        ReadUnknowEntity();
                        break;
                }
                if (entity != null) this.addedObjects.Add(entity, entity);
            }
        }

        private void ReadObjects()
        {
            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Value != StringCode.EndSection)
            {
                switch (dxfPairInfo.Value)
                {
                    case (DxfObjectCode.Dictionary):
                        DictionaryObject dictionary = ReadDictionary();
                        this.dictionaries.Add(dictionary.Handle, dictionary);
                        break;
                    case(DxfObjectCode.RasterVariables):
                        this.rasterVariables = ReadRasterVariables();
                        break;
                    case(DxfObjectCode.ImageDef):
                        ImageDef imageDef = ReadImageDefinition();
                        this.imageDefs.Add(imageDef.FileName, imageDef);
                        break;
                    case(DxfObjectCode.ImageDefReactor):
                        ImageDefReactor reactor = ReadImageDefReactor();
                        this.imageDefReactors.Add(reactor.ImageHandle, reactor);
                        break;
                    default:
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
                
            }
        }

        #endregion

        #region blockrecord methods

        private void ReadBlockRecords()
        {
            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Value != StringCode.EndTable)
            {
                if (dxfPairInfo.Value == StringCode.BlockRecordTable)
                {
                    Debug.Assert(dxfPairInfo.Code == 0);
                    BlockRecord blockRecord = this.ReadBlockRecord();
                    this.blockRecords.Add(blockRecord.Name, blockRecord);
                }
                else
                {
                    dxfPairInfo = this.ReadCodePair();
                }
            }
        }

        private BlockRecord ReadBlockRecord()
        {
            string handle = string.Empty;
            string name = string.Empty;

            dxfPairInfo = this.ReadCodePair();

            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 5:
                        handle = dxfPairInfo.Value;
                        break;
                    case 2:
                        if (string.IsNullOrEmpty(dxfPairInfo.Value))
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value, this.file,
                                                                         "Invalid value " + dxfPairInfo.Value + " in code " + dxfPairInfo.Code + " line " + this.fileLine);
                        name = dxfPairInfo.Value;
                        break;
                }


                dxfPairInfo = this.ReadCodePair();
            }

            // we need to check for generated blocks by dimensions, even if the dimension was deleted the block might persist in the drawing.
            CheckDimBlockName(name);

            return new BlockRecord(name){Handle = handle};
        }

        #endregion

        #region layer methods

        private void ReadLayers()
        {
            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Value != StringCode.EndTable)
            {
                if (dxfPairInfo.Value == StringCode.LayerTable)
                {
                    Debug.Assert(dxfPairInfo.Code == 0);
                    Layer layer = this.ReadLayer();
                    this.layers.Add(layer.Name, layer);
                }
                else
                {
                    dxfPairInfo = this.ReadCodePair();
                }
            }
        }

        private Layer ReadLayer()
        {
            string handle = string.Empty;
            string name = string.Empty;
            bool isVisible = true;
            bool plot = true;
            AciColor color = null;
            LineType lineType = null;

            dxfPairInfo = this.ReadCodePair();

            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 5:
                        handle = dxfPairInfo.Value;
                        break;
                    case 2:
                        if (string.IsNullOrEmpty(dxfPairInfo.Value))
                        {
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value, this.file,
                                                                         "Invalid value " + dxfPairInfo.Value + " in code " + dxfPairInfo.Code + " line " + this.fileLine);
                        }
                        name = dxfPairInfo.Value;
                        break;
                    case 62:
                        short index;
                        if (short.TryParse(dxfPairInfo.Value, out index))
                        {
                            if (index < 0)
                            {
                                isVisible = false;
                                index = Math.Abs(index);
                            }
                            if (index > 256)
                            {
                                throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value, this.file,
                                                                             "Invalid value " + dxfPairInfo.Value + " in code " + dxfPairInfo.Code + " line " + this.fileLine);
                            }
                        }
                        else
                        {
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value, this.file,
                                                                         "Invalid value " + dxfPairInfo.Value + " in code " + dxfPairInfo.Code + " line " + this.fileLine);
                        }

                        color = new AciColor(index);
                        break;
                    case 6:
                        if (string.IsNullOrEmpty(dxfPairInfo.Value))
                        {
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value, this.file,
                                                                         "Invalid value " + dxfPairInfo.Value + " in code " + dxfPairInfo.Code + " line " + this.fileLine);
                        }
                        lineType = this.GetLineType(dxfPairInfo.Value);
                        break;
                    case 290:
                        if(int.Parse(dxfPairInfo.Value)==0)
                            plot = false;
                        break;
                }

                dxfPairInfo = this.ReadCodePair();
            }

            return new Layer(name)
                       {
                           Color = color,
                           LineType = lineType,
                           IsVisible = isVisible,
                           Plot = plot,
                           Handle = handle
                       };
        }

        #endregion

        #region line type methods

        private void ReadLineTypes()
        {
            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Value != StringCode.EndTable)
            {
                if (dxfPairInfo.Value == StringCode.LineTypeTable)
                {
                    Debug.Assert(dxfPairInfo.Code == 0);
                    LineType tl = this.ReadLineType();
                    this.lineTypes.Add(tl.Name, tl);
                }
                else
                {
                    dxfPairInfo = this.ReadCodePair();
                }
            }
        }

        private LineType ReadLineType()
        {
            string handle = string.Empty;
            string name = string.Empty;
            string description = string.Empty;
            List<double> segments = new List<double>();

            dxfPairInfo = this.ReadCodePair();

            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 5:
                        handle = dxfPairInfo.Value;
                        break;
                    case 2:
                        if (string.IsNullOrEmpty(dxfPairInfo.Value))
                        {
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value, this.file,
                                                                         "Invalid value " + dxfPairInfo.Value + " in code " + dxfPairInfo.Code + " line " + this.fileLine);
                        }
                        name = dxfPairInfo.Value;
                        break;
                    case 3: // linetype description
                        description = dxfPairInfo.Value;
                        break;
                    case 73:
                        //number of segments (not needed)
                        break;
                    case 40:
                        //length of the line type segments (not needed)
                        break;
                    case 49:
                        segments.Add(double.Parse(dxfPairInfo.Value));
                        break;
                }
                dxfPairInfo = this.ReadCodePair();
            }

            return new LineType(name)
                       {
                           Description = description,
                           Segments = segments,
                           Handle = handle
                       };
        }

        #endregion

        #region text style methods

        private void ReadTextStyles()
        {
            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Value != StringCode.EndTable)
            {
                if (dxfPairInfo.Value == StringCode.TextStyleTable)
                {
                    Debug.Assert(dxfPairInfo.Code == 0); //el código 0 indica el inicio de una nueva capa
                    TextStyle style = this.ReadTextStyle();
                    this.textStyles.Add(style.Name, style);
                }
                else
                {
                    dxfPairInfo = this.ReadCodePair();
                }
            }
        }

        private TextStyle ReadTextStyle()
        {
            string handle = string.Empty;
            string name = string.Empty;
            string font = string.Empty;
            bool isVertical = false;
            bool isBackward = false;
            bool isUpsideDown = false;
            double height = 0.0f;
            double widthFactor = 0.0f;
            double obliqueAngle = 0.0f;

            dxfPairInfo = this.ReadCodePair();

            //leer los datos mientras no encontramos el código 0 que indicaría el final de la capa
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 5:
                        handle = dxfPairInfo.Value;
                        break;
                    case 2:
                        if (string.IsNullOrEmpty(dxfPairInfo.Value))
                        {
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value, this.file,
                                                                         "Invalid value " + dxfPairInfo.Value + " in code " + dxfPairInfo.Code + " line " + this.fileLine);
                        }
                        name = dxfPairInfo.Value;
                        break;
                    case 3:
                        if (string.IsNullOrEmpty(dxfPairInfo.Value))
                        {
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value, this.file,
                                                                         "Invalid value " + dxfPairInfo.Value + " in code " + dxfPairInfo.Code + " line " + this.fileLine);
                        }
                        font = dxfPairInfo.Value;
                        break;

                    case 70:
                        if (int.Parse(dxfPairInfo.Value) == 4)
                        {
                            isVertical = true;
                        }
                        break;
                    case 71:
                        //orientación texto (normal)
                        if (int.Parse(dxfPairInfo.Value) == 6)
                        {
                            isBackward = true;
                            isUpsideDown = true;
                        }
                        else if (int.Parse(dxfPairInfo.Value) == 2)
                        {
                            isBackward = true;
                        }
                        else if (int.Parse(dxfPairInfo.Value) == 4)
                        {
                            isUpsideDown = true;
                        }
                        break;
                    case 40:
                        height = double.Parse(dxfPairInfo.Value);
                        break;
                    case 41:
                        widthFactor = double.Parse(dxfPairInfo.Value);
                        break;
                    case 42:
                        //last text height used (not aplicable)
                        break;
                    case 50:
                        obliqueAngle = double.Parse(dxfPairInfo.Value);
                        break;
                }
                dxfPairInfo = this.ReadCodePair();
            }

            return new TextStyle(name, font)
                       {
                           Height = height,
                           IsBackward = isBackward,
                           IsUpsideDown = isUpsideDown,
                           IsVertical = isVertical,
                           ObliqueAngle = obliqueAngle,
                           WidthFactor = widthFactor,
                           Handle = handle
                       };
        }

        #endregion

        #region dimension style methods

        private void ReadDimStyles()
        {
            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Value != StringCode.EndTable)
            {
                if (dxfPairInfo.Value == StringCode.DimensionStyleTable)
                {
                    Debug.Assert(dxfPairInfo.Code == 0); //el código 0 indica el inicio de una nueva capa
                    DimensionStyle ds = this.ReadDimensionStyle();
                    this.dimStyles.Add(ds.Name, ds);
                }
                else
                {
                    dxfPairInfo = this.ReadCodePair();
                }
            }
        }

        private DimensionStyle ReadDimensionStyle()
        {
            string handle = string.Empty;
            string name = string.Empty;
            string txtStyleHandle = string.Empty;

            // lines
            double dimexo = 0.0625;
            double dimexe = 0.18;

            // symbols and arrows
            double dimasz = 0.18;

            // text
            double dimtxt = 0.18;
            int dimjust = 0;
            int dimtad = 1;
            double dimgap = 0.09;
            int dimdec = 2;
            string dimpost = "<>";
            int dimtih = 0;
            int dimtoh = 0;
            int dimaunit = 0;

            dxfPairInfo = this.ReadCodePair();

            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 105:
                        handle = dxfPairInfo.Value;
                        break;
                    case 2:
                        if (string.IsNullOrEmpty(dxfPairInfo.Value))
                        {
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value, this.file,
                                                                         "Invalid value " + dxfPairInfo.Value + " in code " + dxfPairInfo.Code + " line " + this.fileLine);
                        }
                        name = dxfPairInfo.Value;
                        break;
                    case 3:
                        dimpost = dxfPairInfo.Value;
                        break;
                    case 41:
                        dimasz = double.Parse(dxfPairInfo.Value);
                        break;
                    case 42:
                        dimexo = double.Parse(dxfPairInfo.Value);
                        break;
                    case 44:
                        dimexe = double.Parse(dxfPairInfo.Value);
                        break;
                    case 140:
                        dimtxt = double.Parse(dxfPairInfo.Value);
                        break;
                    case 147:
                        dimgap = double.Parse(dxfPairInfo.Value);
                        break;
                    case 73:
                        dimtih = int.Parse(dxfPairInfo.Value);
                        break;
                    case 74:
                        dimtoh = int.Parse(dxfPairInfo.Value);
                        break;
                    case 77:
                        dimtad = int.Parse(dxfPairInfo.Value);
                        break;
                    case 271:
                        dimdec = int.Parse(dxfPairInfo.Value);
                        break;
                    case 275:
                        dimaunit = int.Parse(dxfPairInfo.Value);
                        break;
                    case 280:
                        dimjust = int.Parse(dxfPairInfo.Value);
                        break;
                    case 340:
                        txtStyleHandle = dxfPairInfo.Value;
                        break;
                }
                dxfPairInfo = this.ReadCodePair();
            }

            return new DimensionStyle(name)
                       {
                           Handle = handle,
                           DIMEXO = dimexo,
                           DIMEXE = dimexe,
                           DIMASZ = dimasz,
                           DIMTXT = dimtxt,
                           DIMJUST = dimjust,
                           DIMTAD = dimtad,
                           DIMGAP = dimgap,
                           DIMDEC = dimdec,
                           DIMPOST = dimpost,
                           DIMTIH = dimtih,
                           DIMTOH = dimtoh,
                           DIMAUNIT = dimaunit,
                           TextStyle = GetTextStyleByHandle(txtStyleHandle)
                       };
        }

        #endregion

        #region entity methods

        private Image ReadImage()
        {
            string handle = null;
            Layer layer = Layer.Default;
            AciColor color = AciColor.ByLayer;
            LineType lineType = LineType.ByLayer;
            double posX = 0.0, posY = 0.0, posZ = 0.0;
            double uX = 0.0, uY = 0.0, uZ = 0.0;
            double vX = 0.0, vY = 0.0, vZ = 0.0;
            double width = 0, height = 0;
            string imageDefHandle = null;
            ImageDisplayFlags displayOptions = ImageDisplayFlags.ShowImage | ImageDisplayFlags.ShowImageWhenNotAlignedWithScreen | ImageDisplayFlags.UseClippingBoundary;
            bool clipping = false;
            float brightness = 50.0f;
            float contrast = 50.0f;
            float fade = 0.0f;
            ImageClippingBoundaryType boundaryType = ImageClippingBoundaryType.Rectangular;
            ImageClippingBoundary clippingBoundary = null;

            Dictionary<ApplicationRegistry, XData> xData = new Dictionary<ApplicationRegistry, XData>();
            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 5:
                        handle = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 8: //layer code
                        layer = this.GetLayer(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 62: //aci color code
                        color = new AciColor(short.Parse(dxfPairInfo.Value));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 6: //type line code
                        lineType = this.GetLineType(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 10:
                        posX = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        posY = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        posZ = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 11:
                        uX = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 21:
                        uY = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 31:
                        uZ = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 12:
                        vX = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 22:
                        vY = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 32:
                        vZ = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 13:
                        width = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 23:
                        height = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 340:
                        imageDefHandle = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 70:
                        displayOptions = (ImageDisplayFlags) int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 280:
                        clipping = int.Parse(dxfPairInfo.Value) != 0;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 281:
                        brightness = float.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 282:
                        contrast = float.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 283:
                        fade = float.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 71:
                        boundaryType = (ImageClippingBoundaryType) int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 91:
                        int numVertexes = int.Parse(dxfPairInfo.Value);
                        List<Vector2> vertexes = new List<Vector2>();
                        dxfPairInfo = this.ReadCodePair();
                        for (int i = 0; i < numVertexes; i++)
                        {
                            double x = 0.0;
                            double y = 0.0;
                            if (dxfPairInfo.Code == 14) x = double.Parse(dxfPairInfo.Value);
                            dxfPairInfo = this.ReadCodePair();
                            if (dxfPairInfo.Code == 24) y = double.Parse(dxfPairInfo.Value);
                            dxfPairInfo = this.ReadCodePair();
                            vertexes.Add(new Vector2(x, y));
                        }
                        if(boundaryType == ImageClippingBoundaryType.Rectangular)
                            clippingBoundary = new ImageClippingBoundary(vertexes[0], vertexes[1]);
                        else if (boundaryType == ImageClippingBoundaryType.Polygonal)
                            clippingBoundary = new ImageClippingBoundary(vertexes);
                        else
                            clippingBoundary = null;
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        xData.Add(xDataItem.ApplicationRegistry, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value,
                                                                         this.file, "The extended data of an entity must start with the application registry code " + this.fileLine);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            Vector3 u = new Vector3(uX, uY, uZ);
            Vector3 v = new Vector3(vX, vY, vZ);
            Vector3 normal = Vector3.CrossProduct(u, v);
            Vector3 uOCS = MathHelper.Transform(u, normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            double rotation = Vector2.Angle(new Vector2(uOCS.X, uOCS.Y)) * MathHelper.RadToDeg;
            double uLength = u.Modulus();
            double vLength = v.Modulus();

            Image image = new Image(width * uLength, height * vLength)
                              {
                                  Handle = handle,
                                  Layer = layer,
                                  Color = color,
                                  LineType = lineType,
                                  Position = new Vector3(posX, posY, posZ),
                                  Normal = normal,
                                  Rotation = Math.Round(rotation, MathHelper.MaxAngleDecimals),
                                  DisplayOptions = displayOptions,
                                  Clipping = clipping,
                                  Brightness = brightness,
                                  Contrast = contrast,
                                  Fade = fade,
                                  ClippingBoundary = clippingBoundary,
                                  XData = xData
                              };

            this.imgToImgDefHandles.Add(image, imageDefHandle);

            return image;
        }

        private Arc ReadArc()
        {
            Arc arc = new Arc();
            Vector3 center = Vector3.Zero;
            Vector3 normal = Vector3.UnitZ;
            Dictionary<ApplicationRegistry, XData> xData = new Dictionary<ApplicationRegistry, XData>();

            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 5:
                        arc.Handle = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 8: //layer code
                        arc.Layer = this.GetLayer(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 62: //aci color code
                        arc.Color = new AciColor(short.Parse(dxfPairInfo.Value));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 6: //type line code
                        arc.LineType = this.GetLineType(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 10:
                        center.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        center.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        center.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 40:
                        arc.Radius = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 50:
                        arc.StartAngle = Math.Round(double.Parse(dxfPairInfo.Value), MathHelper.MaxAngleDecimals);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 51:
                        arc.EndAngle = Math.Round(double.Parse(dxfPairInfo.Value), MathHelper.MaxAngleDecimals);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 39:
                        arc.Thickness = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        xData.Add(xDataItem.ApplicationRegistry, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value,
                                                                         this.file, "The extended data of an entity must start with the application registry code " + this.fileLine);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            // this is just an example of the stupid autodesk dxf way of doing things, while an ellipse the center is given in world coordinates,
            // the center of an arc is given in object coordinates (different rules for the same concept).
            // It is a lot more intuitive to give the center in world coordinates and then define the orientation with the normal..
            Vector3 wcsCenter = MathHelper.Transform(center, normal, MathHelper.CoordinateSystem.Object, MathHelper.CoordinateSystem.World);

            arc.XData = xData;
            arc.Center = wcsCenter;
            arc.Normal = normal;
            return arc;
        }

        private Circle ReadCircle()
        {
            Circle circle = new Circle();
            Vector3 center = Vector3.Zero;
            Vector3 normal = Vector3.UnitZ;
            Dictionary<ApplicationRegistry, XData> xData = new Dictionary<ApplicationRegistry, XData>();

            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 5:
                        circle.Handle = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 8: //layer code
                        circle.Layer = this.GetLayer(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 62: //aci color code
                        circle.Color = new AciColor(short.Parse(dxfPairInfo.Value));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 6: //type line code
                        circle.LineType = this.GetLineType(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 10:
                        center.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        center.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        center.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 40:
                        circle.Radius = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 39:
                        circle.Thickness = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        xData.Add(xDataItem.ApplicationRegistry, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value, this.file,
                                                                         "The extended data of an entity must start with the application registry code " + this.fileLine);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            // this is just an example of the stupid autodesk dxf way of doing things, while an ellipse the center is given in world coordinates,
            // the center of a circle is given in object coordinates (different rules for the same concept).
            // It is a lot more intuitive to give the center in world coordinates and then define the orientation with the normal..
            Vector3 wcsCenter = MathHelper.Transform(center, normal, MathHelper.CoordinateSystem.Object, MathHelper.CoordinateSystem.World);

            circle.XData = xData;
            circle.Center = wcsCenter;
            circle.Normal = normal;
            return circle;
        }

        private Dimension ReadDimension(bool isNestedBlock = false)
        {
            string handle = null;
            Layer layer = Layer.Default;
            AciColor color = AciColor.ByLayer;
            LineType lineType = LineType.ByLayer;
            string drawingBlockName = null;
            Block drawingBlock = null;
            Vector3 defPoint = Vector3.Zero;
            Vector3 midtxtPoint = Vector3.Zero;
            Vector3 normal = Vector3.UnitZ;
            DimensionTypeFlag dimType = DimensionTypeFlag.Linear;
            MTextAttachmentPoint attachmentPoint = MTextAttachmentPoint.BottomCenter;
            MTextLineSpacingStyle lineSpacingStyle = MTextLineSpacingStyle.AtLeast;
            DimensionStyle style = DimensionStyle.Default;
            double dimRot = 0.0;
            double lineSpacingFactor = 1.0;
            bool dimInfo = false;

            dxfPairInfo = this.ReadCodePair();
            while (!dimInfo)
            {
                switch (dxfPairInfo.Code)
                {
                    case 3:
                        style = this.GetDimensionStyle(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 5:
                        handle = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 8: //layer code
                        layer = this.GetLayer(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 62: //aci color code
                        color = new AciColor(short.Parse(dxfPairInfo.Value));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 6: //type line code
                        lineType = this.GetLineType(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 2:
                        drawingBlockName = dxfPairInfo.Value;
                        drawingBlock = this.GetBlock(drawingBlockName, isNestedBlock);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 10:
                        defPoint.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        defPoint.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        defPoint.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 11:
                        midtxtPoint.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 21:
                        midtxtPoint.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 31:
                        midtxtPoint.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 70:
                        dimType = (DimensionTypeFlag)int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 71:
                        attachmentPoint = (MTextAttachmentPoint) int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 72:
                        lineSpacingStyle = (MTextLineSpacingStyle) int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 41:
                        lineSpacingFactor = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 51:
                        // even if the documentation says that code 51 is optional, rotated ordinate dimensions will not work correctly is this value is not provided
                        dimRot = 360-double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 100:
                        if (dxfPairInfo.Value == SubclassMarker.AlignedDimension ||
                            dxfPairInfo.Value == SubclassMarker.RadialDimension ||
                            dxfPairInfo.Value == SubclassMarker.DiametricDimension ||
                            dxfPairInfo.Value == SubclassMarker.Angular3PointDimension ||
                            dxfPairInfo.Value == SubclassMarker.Angular2LineDimension ||
                            dxfPairInfo.Value == SubclassMarker.OrdinateDimension)
                            dimInfo = true; // we have finished reading the basic dimension info
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    default:
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            Dimension dim;
            OrdinateDimensionAxis axis = OrdinateDimensionAxis.X;
            // this is the result of the way the dxf use the DimensionTypeFlag enum, it is a mixture of a regular enum with flags
            DimensionTypeFlag type = dimType;
            if ((type & DimensionTypeFlag.BlockReference) == DimensionTypeFlag.BlockReference)
                type -= DimensionTypeFlag.BlockReference;
            if ((type & DimensionTypeFlag.OrdinteType) == DimensionTypeFlag.OrdinteType)
            {
                axis = OrdinateDimensionAxis.X;
                type -= DimensionTypeFlag.OrdinteType;
            }
            if ((type & DimensionTypeFlag.UserTextPosition) == DimensionTypeFlag.UserTextPosition)
                type -= DimensionTypeFlag.UserTextPosition;

            switch (type)
            {
                case (DimensionTypeFlag.Aligned):
                    dim = ReadAlignedDimension(defPoint);
                    break;
                case (DimensionTypeFlag.Linear):
                    dim = ReadLinearDimension(defPoint);
                    break;
                case (DimensionTypeFlag.Radius):
                    dim = ReadRadialDimension(defPoint, normal);
                    break;
                case (DimensionTypeFlag.Diameter):
                    dim = ReadDiametricDimension(defPoint, normal);
                    break;
                case (DimensionTypeFlag.Angular3Point):
                    dim = ReadAngular3PointDimension(defPoint);
                    break;
                case (DimensionTypeFlag.Angular):
                    dim = ReadAngular2LineDimension(defPoint);
                    break;
                case (DimensionTypeFlag.Ordinate):
                    dim = ReadOrdinateDimension(defPoint, axis, normal, dimRot);
                    break;
                default:
                    throw new ArgumentException("The dimension type: " + type + " is not implemented or unknown.");
            }

            if (dim != null)
            {
                dim.Handle = handle;
                dim.Layer = layer;
                dim.Color = color;
                dim.LineType = lineType;
                dim.Style = style;
                dim.Block = drawingBlock;
                dim.DefinitionPoint = defPoint;
                dim.MidTextPoint = midtxtPoint;
                dim.AttachmentPoint = attachmentPoint;
                dim.LineSpacingStyle = lineSpacingStyle;
                dim.LineSpacingFactor = lineSpacingFactor;
                dim.Normal = normal;
            }

            if (isNestedBlock)
                if (drawingBlockName != null) this.nestedDimBlocks.Add(drawingBlockName, dim);

            return dim;
        }

        private AlignedDimension ReadAlignedDimension(Vector3 defPoint)
        {
            Vector3 firstRef = Vector3.Zero;
            Vector3 secondRef = Vector3.Zero;
            Dictionary<ApplicationRegistry, XData> xData = new Dictionary<ApplicationRegistry, XData>();

            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 13:
                        firstRef.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 23:
                        firstRef.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 33:
                        firstRef.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 14:
                        secondRef.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 24:
                        secondRef.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 34:
                        secondRef.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        xData.Add(xDataItem.ApplicationRegistry, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value, this.file,
                                                                         "The extended data of an entity must start with the application registry code " + this.fileLine);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }
            double offset = Vector3.Distance(secondRef, defPoint);
            return new AlignedDimension(firstRef, secondRef, offset) {XData = xData};
        }

        private LinearDimension ReadLinearDimension(Vector3 defPoint)
        {
            Vector3 firtRef = Vector3.Zero;
            Vector3 secondRef = Vector3.Zero;
            double rot = 0.0;
            Dictionary<ApplicationRegistry, XData> xData = new Dictionary<ApplicationRegistry, XData>();

            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 13:
                        firtRef.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 23:
                        firtRef.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 33:
                        firtRef.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 14:
                        secondRef.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 24:
                        secondRef.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 34:
                        secondRef.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 50:
                        rot = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 52:
                        // AutoCAD is unable to recognized code 52 for oblique dimension line even though it appears as valid in the dxf documentation
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        xData.Add(xDataItem.ApplicationRegistry, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value, this.file,
                                                                         "The extended data of an entity must start with the application registry code " + this.fileLine);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            Vector3 midPoint = Vector3.MidPoint(firtRef, secondRef);
            Vector3 origin = defPoint;
            Vector3 dir = new Vector3(Math.Cos(rot*MathHelper.DegToRad), Math.Sin(rot*MathHelper.DegToRad), 0.0);
            dir.Normalize();
            double offset = MathHelper.PointLineDistance(midPoint, origin, dir);

            return new LinearDimension(firtRef, secondRef, offset, rot) {XData = xData};
        }

        private RadialDimension ReadRadialDimension(Vector3 defPoint, Vector3 normal)
        {
            Vector3 circunferenceRef = Vector3.Zero;
            Dictionary<ApplicationRegistry, XData> xData = new Dictionary<ApplicationRegistry, XData>();

            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 15:
                        circunferenceRef.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 25:
                        circunferenceRef.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 35:
                        circunferenceRef.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 40:
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        xData.Add(xDataItem.ApplicationRegistry, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value, this.file,
                                                                         "The extended data of an entity must start with the application registry code " + this.fileLine);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            double radius = Vector3.Distance(defPoint, circunferenceRef);
            Vector3 refPoint = MathHelper.Transform(defPoint, normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            Vector2 firstPoint = new Vector2(refPoint.X, refPoint.Y);

            refPoint = MathHelper.Transform(circunferenceRef, normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            Vector2 seconPoint = new Vector2(refPoint.X, refPoint.Y);

            double rotation = Math.Round(Vector2.Angle(firstPoint, seconPoint) * MathHelper.RadToDeg, MathHelper.MaxAngleDecimals);
            return new RadialDimension(defPoint, radius, rotation ) { XData = xData };
        }

        private DiametricDimension ReadDiametricDimension(Vector3 defPoint, Vector3 normal)
        {
            Vector3 circunferenceRef = Vector3.Zero;
            Dictionary<ApplicationRegistry, XData> xData = new Dictionary<ApplicationRegistry, XData>();

            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 15:
                        circunferenceRef.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 25:
                        circunferenceRef.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 35:
                        circunferenceRef.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 40:
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        xData.Add(xDataItem.ApplicationRegistry, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value, this.file,
                                                                         "The extended data of an entity must start with the application registry code " + this.fileLine);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            double diameter = Vector3.Distance(defPoint, circunferenceRef);
            Vector3 refPoint = MathHelper.Transform(defPoint, normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            Vector2 firstPoint = new Vector2(refPoint.X, refPoint.Y);

            refPoint = MathHelper.Transform(circunferenceRef, normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            Vector2 seconPoint = new Vector2(refPoint.X, refPoint.Y);

            double rotation = Math.Round(Vector2.Angle(firstPoint, seconPoint) * MathHelper.RadToDeg, MathHelper.MaxAngleDecimals);
            return new DiametricDimension(Vector3.MidPoint(defPoint, circunferenceRef), diameter, rotation) { XData = xData };
        }

        private Angular3PointDimension ReadAngular3PointDimension(Vector3 defPoint)
        {
            Vector3 center = Vector3.Zero;
            Vector3 firstRef = Vector3.Zero;
            Vector3 secondRef = Vector3.Zero;

            Dictionary<ApplicationRegistry, XData> xData = new Dictionary<ApplicationRegistry, XData>();

            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 13:
                        firstRef.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 23:
                        firstRef.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 33:
                        firstRef.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 14:
                        secondRef.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 24:
                        secondRef.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 34:
                        secondRef.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 15:
                        center.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 25:
                        center.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 35:
                        center.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        xData.Add(xDataItem.ApplicationRegistry, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value, this.file,
                                                                         "The extended data of an entity must start with the application registry code " + this.fileLine);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            return new Angular3PointDimension(center, firstRef, secondRef, Vector3.Distance(center, defPoint));

        }

        private Angular2LineDimension ReadAngular2LineDimension(Vector3 defPoint)
        {      
            Vector3 startFirstLine = Vector3.Zero;
            Vector3 endFirstLine = Vector3.Zero;
            Vector3 startSecondLine = Vector3.Zero;
            Vector3 arcDefinitionPoint = Vector3.Zero;

            Dictionary<ApplicationRegistry, XData> xData = new Dictionary<ApplicationRegistry, XData>();

            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 13:
                        startFirstLine.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 23:
                        startFirstLine.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 33:
                        startFirstLine.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 14:
                        endFirstLine.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 24:
                        endFirstLine.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 34:
                        endFirstLine.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 15:
                        startSecondLine.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 25:
                        startSecondLine.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 35:
                        startSecondLine.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 16:
                        arcDefinitionPoint.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 26:
                        arcDefinitionPoint.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 36:
                        arcDefinitionPoint.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        xData.Add(xDataItem.ApplicationRegistry, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value, this.file,
                                                                         "The extended data of an entity must start with the application registry code " + this.fileLine);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }
            return new Angular2LineDimension(startFirstLine, endFirstLine, startSecondLine, defPoint, Vector3.Distance(startSecondLine, defPoint)) { ArcDefinitionPoint = arcDefinitionPoint };
        }

        private OrdinateDimension ReadOrdinateDimension(Vector3 defPoint, OrdinateDimensionAxis axis, Vector3 normal, double rotation)
        {
            Vector3 firstPoint = Vector3.Zero;
            Vector3 secondPoint = Vector3.Zero;

            Dictionary<ApplicationRegistry, XData> xData = new Dictionary<ApplicationRegistry, XData>();

            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 13:
                        firstPoint.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 23:
                        firstPoint.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 33:
                        firstPoint.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 14:
                        secondPoint.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 24:
                        secondPoint.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 34:
                        secondPoint.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        xData.Add(xDataItem.ApplicationRegistry, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value, this.file,
                                                                         "The extended data of an entity must start with the application registry code " + this.fileLine);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }
            Vector3 localPoint = MathHelper.Transform(defPoint, normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            Vector2 refCenter = new Vector2(localPoint.X, localPoint.Y);

            localPoint = MathHelper.Transform(firstPoint, normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            Vector2 firstRef = MathHelper.Transform(new Vector2(localPoint.X, localPoint.Y) - refCenter, rotation * MathHelper.DegToRad, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);

            localPoint = MathHelper.Transform(secondPoint, normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            Vector2 secondRef = MathHelper.Transform(new Vector2(localPoint.X, localPoint.Y) - refCenter, rotation * MathHelper.DegToRad, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);

            double length = axis == OrdinateDimensionAxis.X ? secondRef.Y - firstRef.Y : secondRef.X - firstRef.X;

            return new OrdinateDimension(defPoint, firstRef, length, rotation, axis);
        }

        private Ellipse ReadEllipse()
        {
            Ellipse ellipse = new Ellipse();
            Vector3 center = Vector3.Zero;
            Vector3 axisPoint = Vector3.Zero;
            Vector3 normal = Vector3.UnitZ;
            double[] param = new double[2];
            double ratio = 0.0;

            Dictionary<ApplicationRegistry, XData> xData = new Dictionary<ApplicationRegistry, XData>();

            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 5:
                        ellipse.Handle = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 8: //layer code
                        ellipse.Layer = this.GetLayer(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 62: //aci color code
                        ellipse.Color = new AciColor(short.Parse(dxfPairInfo.Value));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 6: //type line code
                        ellipse.LineType = this.GetLineType(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 10:
                        center.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        center.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        center.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 11:
                        axisPoint.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 21:
                        axisPoint.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 31:
                        axisPoint.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 40:
                        ratio = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 41:
                        param[0] = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 42:
                        param[1] = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        xData.Add(xDataItem.ApplicationRegistry, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value, this.file,
                                                                         "The extended data of an entity must start with the application registry code " + this.fileLine);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            Vector3 ocsAxisPoint = MathHelper.Transform(axisPoint,
                                                        normal,
                                                        MathHelper.CoordinateSystem.World,
                                                        MathHelper.CoordinateSystem.Object);
            double rotation = Vector2.Angle(new Vector2(ocsAxisPoint.X, ocsAxisPoint.Y));
            ellipse.MajorAxis = 2*axisPoint.Modulus();
            ellipse.MinorAxis = ellipse.MajorAxis*ratio;
            ellipse.Rotation = Math.Round(rotation*MathHelper.RadToDeg, MathHelper.MaxAngleDecimals);
            ellipse.Center = center;
            ellipse.Normal = normal;
            ellipse.XData = xData;

            ellipse.SetParameters(param);
            return ellipse;
        }

        private Point ReadPoint()
        {
            Point point = new Point();
            Vector3 location = Vector3.Zero;
            Vector3 normal = Vector3.UnitZ;
            Dictionary<ApplicationRegistry, XData> xData = new Dictionary<ApplicationRegistry, XData>();

            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 5:
                        point.Handle = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 8: //layer code
                        point.Layer = this.GetLayer(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 62: //aci color code
                        point.Color = new AciColor(short.Parse(dxfPairInfo.Value));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 6: //type line code
                        point.LineType = this.GetLineType(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 10:
                        location.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        location.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        location.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 39:
                        point.Thickness = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        xData.Add(xDataItem.ApplicationRegistry, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value, this.file,
                                                                         "The extended data of an entity must start with the application registry code " + this.fileLine);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            point.Location = location;
            point.Normal = normal;
            point.XData = xData;
            return point;
        }

        private Face3d ReadFace3D()
        {
            Face3d face = new Face3d();
            Vector3 v0 = Vector3.Zero;
            Vector3 v1 = Vector3.Zero;
            Vector3 v2 = Vector3.Zero;
            Vector3 v3 = Vector3.Zero;
            Dictionary<ApplicationRegistry, XData> xData = new Dictionary<ApplicationRegistry, XData>();

            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 5:
                        face.Handle = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 8: //layer code
                        face.Layer = this.GetLayer(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 62: //aci color code
                        face.Color = new AciColor(short.Parse(dxfPairInfo.Value));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 6: //type line code
                        face.LineType = this.GetLineType(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 10:
                        v0.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        v0.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        v0.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 11:
                        v1.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 21:
                        v1.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 31:
                        v1.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 12:
                        v2.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 22:
                        v2.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 32:
                        v2.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 13:
                        v3.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 23:
                        v3.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 33:
                        v3.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 70:
                        face.EdgeFlags = (EdgeFlags) (int.Parse(dxfPairInfo.Value));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        xData.Add(xDataItem.ApplicationRegistry, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value, this.file,
                                                                         "The extended data of an entity must start with the application registry code " + this.fileLine);

                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            face.FirstVertex = v0;
            face.SecondVertex = v1;
            face.ThirdVertex = v2;
            face.FourthVertex = v3;
            face.XData = xData;
            return face;
        }

        private Solid ReadSolid()
        {
            Solid solid = new Solid();
            Vector3 v0 = Vector3.Zero;
            Vector3 v1 = Vector3.Zero;
            Vector3 v2 = Vector3.Zero;
            Vector3 v3 = Vector3.Zero;
            Vector3 normal = Vector3.UnitZ;
            Dictionary<ApplicationRegistry, XData> xData = new Dictionary<ApplicationRegistry, XData>();

            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 5:
                        solid.Handle = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 8: //layer code
                        solid.Layer = this.GetLayer(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 62: //aci color code
                        solid.Color = new AciColor(short.Parse(dxfPairInfo.Value));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 6: //type line code
                        solid.LineType = this.GetLineType(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 10:
                        v0.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        v0.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        v0.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 11:
                        v1.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 21:
                        v1.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 31:
                        v1.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 12:
                        v2.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 22:
                        v2.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 32:
                        v2.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 13:
                        v3.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 23:
                        v3.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 33:
                        v3.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 70:
                        solid.Thickness = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        xData.Add(xDataItem.ApplicationRegistry, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value, this.file,
                                                                         "The extended data of an entity must start with the application registry code " + this.fileLine);

                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            solid.FirstVertex = v0;
            solid.SecondVertex = v1;
            solid.ThirdVertex = v2;
            solid.FourthVertex = v3;
            solid.Normal = normal;
            solid.XData = xData;
            return solid;
        }

        private Spline ReadSpline()
        {
            string handle = string.Empty;
            Layer layer = Layer.Default;
            AciColor color = AciColor.ByLayer;
            LineType lineType = LineType.ByLayer;
            SplineTypeFlags flags = SplineTypeFlags.None;
            Vector3 normal = Vector3.UnitZ;
            short degree = 3;
            int numKnots = 0;
            int numCtrlPoints = 0;
            int ctrlPointIndex = -1;
            
            double[] knots = null;
            List<SplineVertex> ctrlPoints = new List<SplineVertex>();
            double ctrlX = 0;
            double ctrlY = 0;
            double ctrlZ = 0;
            double ctrlWeigth = -1;

            // tolerances (not used)
            double knotTolerance = 0.0000001;
            double ctrlPointTolerance = 0.0000001;
            double fitTolerance = 0.0000000001;

            // start and end tangents (not used)
            double stX = 0;
            double stY = 0;
            double stZ = 0;
            Vector3? startTangent = null;
            double etX = 0;
            double etY = 0;
            double etZ = 0;
            Vector3? endTangent = null;

            // fit points variable (not used)
            int numFitPoints = 0;
            List<Vector3> fitPoints = new List<Vector3>();
            double fitX = 0;
            double fitY = 0;
            double fitZ = 0;

            Dictionary<ApplicationRegistry, XData> xData = new Dictionary<ApplicationRegistry, XData>();

            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 5:
                        handle = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 8:
                        layer = this.GetLayer(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 62:
                        color = new AciColor(short.Parse(dxfPairInfo.Value));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 6:
                        lineType = this.GetLineType(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 70:
                        flags = (SplineTypeFlags)(int.Parse(dxfPairInfo.Value));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 71:
                        degree = short.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 72:
                        numKnots = int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 73:
                        numCtrlPoints = int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 74:
                        numFitPoints = int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 42:
                        knotTolerance = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 43:
                        ctrlPointTolerance = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 44:
                        fitTolerance = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 12:
                        stX = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 22:
                        stY = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 32:
                        stZ = double.Parse(dxfPairInfo.Value);
                        startTangent = new Vector3(stX, stY, stZ);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 13:
                        etX = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 23:
                        etY = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 33:
                        etZ = double.Parse(dxfPairInfo.Value);
                        endTangent = new Vector3(stX, stY, stZ);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 40:
                        // multiple code 40 entries, one per knot value
                        knots = ReadSplineKnots(numKnots);
                        break;
                    case 10:
                        ctrlX = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        ctrlY = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        ctrlZ = double.Parse(dxfPairInfo.Value);
                        
                        if (ctrlWeigth.Equals(-1))
                        {
                            ctrlPoints.Add(new SplineVertex(ctrlX, ctrlY, ctrlZ));
                            ctrlPointIndex = ctrlPoints.Count - 1;
                        }
                        else
                        {
                            ctrlPoints.Add(new SplineVertex(ctrlX, ctrlY, ctrlZ, ctrlWeigth));
                            ctrlPointIndex = -1;
                        }
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 41:
                        // code 41 might appear before or after the control point coordiantes.
                        // I am open to better ways to handling this.
                        if (ctrlPointIndex == -1)
                        {
                            ctrlWeigth = double.Parse(dxfPairInfo.Value);
                        }
                        else
                        {
                            ctrlPoints[ctrlPointIndex].Weigth = double.Parse(dxfPairInfo.Value);
                            ctrlWeigth = -1;
                        }
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 11:
                        fitX = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 21:
                        fitY = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 31:
                        fitZ = double.Parse(dxfPairInfo.Value);
                        fitPoints.Add(new Vector3(fitX, fitY, fitZ));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        xData.Add(xDataItem.ApplicationRegistry, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value, this.file,
                                                                         "The extended data of an entity must start with the application registry code " + this.fileLine);

                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            Spline spline = new Spline(ctrlPoints, knots, degree)
                                {
                                    Handle = handle,
                                    Layer = layer,
                                    Color = color,
                                    LineType = lineType
                                };
            return spline;
        }

        private double[] ReadSplineKnots(int numKnots)
        {
            double[] knots = new double[numKnots];
            for (int i = 0; i < numKnots; i++)
            {
                if (dxfPairInfo.Code != 40)
                    throw new DxfException("The knot vector must have " + numKnots + " code 40 entries.");
                knots[i] = double.Parse(dxfPairInfo.Value);
                dxfPairInfo = this.ReadCodePair();
            }
            return knots;
        }

        private Insert ReadInsert(bool isNestedBlock = false)
        {
            string handle = string.Empty;
            Vector3 basePoint = Vector3.Zero;
            Vector3 normal = Vector3.UnitZ;
            Vector3 scale = new Vector3(1.0, 1.0, 1.0);
            double rotation = 0.0;
            string blockName = null;
            Block block = null;
            Layer layer = Layer.Default;
            AciColor color = AciColor.ByLayer;
            LineType lineType = LineType.ByLayer;
            List<Attribute> attributes = new List<Attribute>();
            Dictionary<ApplicationRegistry, XData> xData = new Dictionary<ApplicationRegistry, XData>();

            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 5:
                        handle = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 2:
                        blockName = dxfPairInfo.Value;
                        block = this.GetBlock(blockName, isNestedBlock);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 8: //layer code
                        layer = this.GetLayer(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 62: //aci color code
                        color = new AciColor(short.Parse(dxfPairInfo.Value));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 6: //type line code
                        lineType = this.GetLineType(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 10:
                        basePoint.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        basePoint.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        basePoint.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 41:
                        scale.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 42:
                        scale.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 43:
                        scale.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 50:
                        rotation = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        xData.Add(xDataItem.ApplicationRegistry, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value, this.file,
                                                                         "The extended data of an entity must start with the application registry code " + this.fileLine);

                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            // if there are attributes
            string endSequenceHandle = string.Empty;
            Layer endSequenceLayer = Layer.Default;
            if (dxfPairInfo.Value == DxfObjectCode.Attribute)
            {
                while (dxfPairInfo.Value != StringCode.EndSequence)
                {
                    if (dxfPairInfo.Value != DxfObjectCode.Attribute) continue;

                    // read the attribute
                    Attribute attribute = this.ReadAttribute(block, isNestedBlock);
                    attributes.Add(attribute);
                }

                // read the end end sequence object until a new element is found
                dxfPairInfo = this.ReadCodePair();
                while (dxfPairInfo.Code != 0)
                {
                    switch (dxfPairInfo.Code)
                    {
                        case 5:
                            endSequenceHandle = dxfPairInfo.Value;
                            dxfPairInfo = this.ReadCodePair();
                            break;
                        case 8:
                            endSequenceLayer = this.GetLayer(dxfPairInfo.Value);
                            dxfPairInfo = this.ReadCodePair();
                            break;
                        default:
                            dxfPairInfo = this.ReadCodePair();
                            break;
                    }
                }
            }

            // It is a lot more intuitive to give the center in world coordinates and then define the orientation with the normal.
            Vector3 wcsBasePoint = MathHelper.Transform(basePoint, normal, MathHelper.CoordinateSystem.Object, MathHelper.CoordinateSystem.World);

            Insert insert = new Insert
                                {
                                    Block = block,
                                    Color = color,
                                    Layer = layer,
                                    LineType = lineType,
                                    Position = wcsBasePoint,
                                    Rotation = rotation,
                                    Scale = scale,
                                    Normal = normal,
                                    Handle = handle
                                };

            insert.EndSequence.Handle = endSequenceHandle;
            insert.EndSequence.Layer = endSequenceLayer;
            insert.Attributes.Clear();
            insert.Attributes.AddRange(attributes);
            insert.XData = xData;

            if (isNestedBlock)
                if (blockName != null) this.nestedBlocks.Add(insert, blockName);
                
            return insert;
        }

        private Line ReadLine()
        {
            Line line = new Line();
            Vector3 start = Vector3.Zero;
            Vector3 end = Vector3.Zero;
            Vector3 normal = Vector3.UnitZ;
            Dictionary<ApplicationRegistry, XData> xData = new Dictionary<ApplicationRegistry, XData>();

            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 5:
                        line.Handle = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 8: //layer code
                        line.Layer = this.GetLayer(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 62: //aci color code
                        line.Color = new AciColor(short.Parse(dxfPairInfo.Value));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 6: //type line code
                        line.LineType = this.GetLineType(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 10:
                        start.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        start.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        start.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 11:
                        end.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 21:
                        end.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 31:
                        end.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 39:
                        line.Thickness = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        xData.Add(xDataItem.ApplicationRegistry, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value, this.file,
                                                                         "The extended data of an entity must start with the application registry code " + this.fileLine);

                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            line.StartPoint = start;
            line.EndPoint = end;
            line.Normal = normal;
            line.XData = xData;

            return line;
        }

        private LwPolyline ReadLwPolyline()
        {
            LwPolyline pol = new LwPolyline();
            double constantWidth = 0.0;
            LwPolylineVertex v = new LwPolylineVertex();
            double vX = 0.0;
            Vector3 normal = Vector3.UnitZ;

            Dictionary<ApplicationRegistry, XData> xData = new Dictionary<ApplicationRegistry, XData>();

            dxfPairInfo = this.ReadCodePair();

            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 5:
                        pol.Handle = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 8:
                        pol.Layer = this.GetLayer(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 62:
                        pol.Color = new AciColor(short.Parse(dxfPairInfo.Value));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 6:
                        pol.LineType = this.GetLineType(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 38:
                        pol.Elevation = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 39:
                        pol.Thickness = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 43:
                        // constant width (optional; default = 0). Not used if variable width (codes 40 and/or 41) is set
                        constantWidth = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 70:
                        PolylineTypeFlags flags = (PolylineTypeFlags) int.Parse(dxfPairInfo.Value);
                        pol.IsClosed = (flags & PolylineTypeFlags.ClosedPolylineOrClosedPolygonMeshInM) == PolylineTypeFlags.ClosedPolylineOrClosedPolygonMeshInM;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 90:
                        //numVertexes = int.Parse(code.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 10:
                        v = new LwPolylineVertex
                                {
                                    BeginWidth = constantWidth,
                                    EndWidth = constantWidth
                                };
                        vX = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        double vY = double.Parse(dxfPairInfo.Value);
                        v.Location = new Vector2(vX, vY);
                        pol.Vertexes.Add(v);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 40:
                        v.BeginWidth = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 41:
                        v.EndWidth = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 42:
                        v.Bulge = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        xData.Add(xDataItem.ApplicationRegistry, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value, this.file,
                                                                         "The extended data of an entity must start with the application registry code " + this.fileLine);

                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            pol.Normal = normal;
            pol.XData = xData;

            return pol;
        }

        private IEntityObject ReadPolyline()
        {
            // the entity Polyline in dxf can actually hold three kinds of entities
            // polyline 3d is the generic polyline
            // polyface mesh
            // polylines 2d is the old way of writing polylines the AutoCAD2000 and newer always use LightweightPolylines to define a polyline 2d
            // this way of reading polylines 2d is here for compatibility reasons with older dxf versions.
            string handle = string.Empty;
            Layer layer = Layer.Default;
            AciColor color = AciColor.ByLayer;
            LineType lineType = LineType.ByLayer;
            PolylineTypeFlags flags = PolylineTypeFlags.OpenPolyline;
            double elevation = 0.0;
            double thickness = 0.0;
            Vector3 normal = Vector3.UnitZ;

            List<Vertex> vertexes = new List<Vertex>();
            Dictionary<ApplicationRegistry, XData> xData = new Dictionary<ApplicationRegistry, XData>();

            dxfPairInfo = this.ReadCodePair();

            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 5:
                        handle = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 8:
                        layer = this.GetLayer(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 62:
                        color = new AciColor(short.Parse(dxfPairInfo.Value));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 6:
                        lineType = this.GetLineType(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        elevation = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 39:
                        thickness = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 70:
                        flags = (PolylineTypeFlags) (int.Parse(dxfPairInfo.Value));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 71:
                        //this field might not exist for polyface meshes, we cannot depend on it
                        //numVertexes = int.Parse(code.Value); code = this.ReadCodePair();
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 72:
                        //this field might not exist for polyface meshes, we cannot depend on it
                        //numFaces  = int.Parse(code.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        xData.Add(xDataItem.ApplicationRegistry, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value, this.file,
                                                                         "The extended data of an entity must start with the application registry code " + this.fileLine);

                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            //begin to read the vertex list
            if (dxfPairInfo.Value != DxfObjectCode.Vertex)
                throw new DxfEntityException(DxfObjectCode.Polyline, this.file, "Vertex not found in line " + this.fileLine);
            while (dxfPairInfo.Value != StringCode.EndSequence)
            {
                if (dxfPairInfo.Value == DxfObjectCode.Vertex)
                {
                    Debug.Assert(dxfPairInfo.Code == 0);
                    Vertex vertex = this.ReadVertex();
                    vertexes.Add(vertex);
                }
            }

            // read the end sequence object until a new element is found
            if (dxfPairInfo.Value != StringCode.EndSequence)
                throw new DxfEntityException(DxfObjectCode.Polyline, this.file, "End sequence entity not found in line " + this.fileLine);
            dxfPairInfo = this.ReadCodePair();
            string endSequenceHandle = string.Empty;
            Layer endSequenceLayer = layer;
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 5:
                        endSequenceHandle = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 8:
                        endSequenceLayer = this.GetLayer(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    default:
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            IEntityObject pol;
            bool isClosed = (flags & PolylineTypeFlags.ClosedPolylineOrClosedPolygonMeshInM) == PolylineTypeFlags.ClosedPolylineOrClosedPolygonMeshInM;

            //to avoid possible error between the vertex type and the polyline type
            //the polyline type will decide which information to use from the read vertex
            if ((flags & PolylineTypeFlags.Polyline3D) == PolylineTypeFlags.Polyline3D)
            {
                List<PolylineVertex> polyline3dVertexes = new List<PolylineVertex>();
                foreach (Vertex v in vertexes)
                {
                    PolylineVertex vertex = new PolylineVertex
                                                {
                                                    Location = v.Location,
                                                    Handle = v.Handle,
                                                    XData = v.XData
                                                };
                    polyline3dVertexes.Add(vertex);
                }

                pol = new Polyline(polyline3dVertexes, isClosed)
                          {
                              Handle = handle
                          };
                ((Polyline) pol).EndSequence.Handle = endSequenceHandle;
                ((Polyline) pol).EndSequence.Layer = endSequenceLayer;
            }
            else if ((flags & PolylineTypeFlags.PolyfaceMesh) == PolylineTypeFlags.PolyfaceMesh)
            {
                //the vertex list created contains vertex and face information
                List<PolyfaceMeshVertex> polyfaceVertexes = new List<PolyfaceMeshVertex>();
                List<PolyfaceMeshFace> polyfaceFaces = new List<PolyfaceMeshFace>();
                foreach (Vertex v in vertexes)
                {
                    if ((v.Flags & (VertexTypeFlags.PolyfaceMeshVertex | VertexTypeFlags.Polygon3dMesh)) == (VertexTypeFlags.PolyfaceMeshVertex | VertexTypeFlags.Polygon3dMesh))
                    {
                        PolyfaceMeshVertex vertex = new PolyfaceMeshVertex
                                                        {
                                                            Location = v.Location,
                                                            Handle = v.Handle,
                                                            XData = xData
                                                        };
                        polyfaceVertexes.Add(vertex);
                    }
                    else if ((v.Flags & (VertexTypeFlags.PolyfaceMeshVertex)) == (VertexTypeFlags.PolyfaceMeshVertex))
                    {
                        PolyfaceMeshFace vertex = new PolyfaceMeshFace
                                                      {
                                                          VertexIndexes = v.VertexIndexes,
                                                          Handle = v.Handle,
                                                          XData = xData
                                                      };
                        polyfaceFaces.Add(vertex);
                    }
                }
                pol = new PolyfaceMesh(polyfaceVertexes, polyfaceFaces)
                          {
                              Handle = handle
                          };
                ((PolyfaceMesh) pol).EndSequence.Handle = endSequenceHandle;
                ((PolyfaceMesh) pol).EndSequence.Layer = endSequenceLayer;
            }
            else
            {
                List<LwPolylineVertex> polylineVertexes = new List<LwPolylineVertex>();
                foreach (Vertex v in vertexes)
                {
                    LwPolylineVertex vertex = new LwPolylineVertex
                                                  {
                                                      Location = new Vector2(v.Location.X, v.Location.Y),
                                                      BeginWidth = v.BeginThickness,
                                                      Bulge = v.Bulge,
                                                      EndWidth = v.EndThickness,
                                                  };

                    polylineVertexes.Add(vertex);
                }

                pol = new LwPolyline(polylineVertexes, isClosed)
                          {
                              Thickness = thickness,
                              Elevation = elevation,
                              Normal = normal,
                              Handle = handle
                          };
            }

            pol.Color = color;
            pol.Layer = layer;
            pol.LineType = lineType;
            pol.XData = xData;

            return pol;
        }

        private Text ReadText()
        {
            Text text = new Text();

            Vector3 firstAlignmentPoint = Vector3.Zero;
            Vector3 secondAlignmentPoint = Vector3.Zero;
            Vector3 normal = Vector3.UnitZ;
            int horizontalAlignment = 0;
            int verticalAlignment = 0;
            Dictionary<ApplicationRegistry, XData> xData = new Dictionary<ApplicationRegistry, XData>();

            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 5:
                        text.Handle = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1:
                        text.Value = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 8: //layer code
                        text.Layer = this.GetLayer(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 62: //aci color code
                        text.Color = new AciColor(short.Parse(dxfPairInfo.Value));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 6: //type line code
                        text.LineType = this.GetLineType(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 10:
                        firstAlignmentPoint.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        firstAlignmentPoint.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        firstAlignmentPoint.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 11:
                        secondAlignmentPoint.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 21:
                        secondAlignmentPoint.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 31:
                        secondAlignmentPoint.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 40:
                        text.Height = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 41:
                        text.WidthFactor = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 50:
                        text.Rotation = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 51:
                        text.ObliqueAngle = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 7:
                        text.Style = this.GetTextStyle(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 72:
                        horizontalAlignment = int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 73:
                        verticalAlignment = int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        xData.Add(xDataItem.ApplicationRegistry, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value, this.file,
                                                                         "The extended data of an entity must start with the application registry code " + this.fileLine);

                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            TextAlignment alignment = ObtainAlignment(horizontalAlignment, verticalAlignment);
            Vector3 ocsBasePoint = alignment == TextAlignment.BaselineLeft ? firstAlignmentPoint : secondAlignmentPoint;

            // another example of this ocs vs wcs non sense.
            // while the MText position is written in WCS the position of the Text is written in OCS (different rules for the same concept).
            text.Position = MathHelper.Transform(ocsBasePoint, normal, MathHelper.CoordinateSystem.Object, MathHelper.CoordinateSystem.World);
            text.Normal = normal;
            text.Alignment = alignment;
            text.XData = xData;

            return text;
        }

        private MText ReadMText()
        {
            string handle = string.Empty;
            Vector3 insertionPoint = Vector3.Zero;
            Vector2 direction = Vector2.UnitX;
            Vector3 normal = Vector3.UnitZ;
            Layer layer = Layer.Default;
            AciColor color = AciColor.Default;
            LineType lineType = LineType.ByLayer;
            double height = 0.0;
            double rectangleWidth = 0.0;
            double lineSpacing = 1.0;
            double rotation = 0.0;
            bool isRotationDefined = false;
            MTextAttachmentPoint attachmentPoint = MTextAttachmentPoint.TopLeft;
            TextStyle style = TextStyle.Default;
            string text = string.Empty;
            Dictionary<ApplicationRegistry, XData> xData = new Dictionary<ApplicationRegistry, XData>();

            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 5:
                        handle = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1:
                        text += dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 3:
                        text += dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 8: //layer code
                        layer = this.GetLayer(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 62: //aci color code
                        color = new AciColor(short.Parse(dxfPairInfo.Value));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 6: //type line code
                        lineType = this.GetLineType(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 10:
                        insertionPoint.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        insertionPoint.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        insertionPoint.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 11:
                        direction.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 21:
                        direction.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 31:
                        // Z direction value (direction.Z = double.Parse(dxfPairInfo.Value);)
                        // we will alway defined the angle of the text on the plane where it is defined so Z value will be zero.
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 40:
                        height = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 41:
                        rectangleWidth = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 44:
                        lineSpacing = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 50:
                        isRotationDefined = true;
                        rotation = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 7:
                        style = this.GetTextStyle(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 71:
                        attachmentPoint = (MTextAttachmentPoint) int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        xData.Add(xDataItem.ApplicationRegistry, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value, this.file,
                                                                         "The extended data of an entity must start with the application registry code " + this.fileLine);

                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            return new MText(text, insertionPoint, height, rectangleWidth, style)
                              {
                                  Handle = handle,
                                  Layer = layer,
                                  Color = color,
                                  LineType = lineType,
                                  LineSpacingFactor = lineSpacing,
                                  AttachmentPoint = attachmentPoint,
                                  Rotation = isRotationDefined
                                                 ? rotation
                                                 : Math.Round(Vector2.Angle(direction)*MathHelper.RadToDeg, MathHelper.MaxAngleDecimals),
                                  Normal = normal,
                                  XData = xData
                              };


        }

        private Hatch ReadHatch()
        {
            string name = string.Empty;
            string handle = string.Empty;
            Layer layer = Layer.Default;
            AciColor color = AciColor.ByLayer;
            LineType lineType = LineType.ByLayer;
            FillType fill = FillType.SolidFill;
            double elevation = 0.0;
            Vector3 normal = Vector3.UnitZ;
            HatchPattern pattern = HatchPattern.Line;
            List<HatchBoundaryPath> paths = new List<HatchBoundaryPath>();
            Dictionary<ApplicationRegistry, XData> xData = new Dictionary<ApplicationRegistry, XData>();

            dxfPairInfo = this.ReadCodePair();

            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 2:
                        name = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 5:
                        handle = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 8:
                        layer = this.GetLayer(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 62:
                        color = new AciColor(short.Parse(dxfPairInfo.Value));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 6:
                        lineType = this.GetLineType(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        elevation = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 91:
                        // the next lines hold the information about the hatch boundary paths
                        int numPaths = int.Parse(dxfPairInfo.Value);
                        paths = ReadHatchBoundaryPaths(numPaths);
                        break;
                    case 70:
                        // Solid fill flag
                        fill = (FillType) int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 71:
                        // Associativity flag (associative = 1; non-associative = 0); for MPolygon, solid-fill flag (has solid fill = 1; lacks solid fill = 0)
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 75:
                        // the next lines hold the information about the hatch pattern
                        pattern = ReadHatchPattern(name);
                        pattern.Fill = fill; // just in case, as far as I know only the pattern name "SOLID" has pattern fill = 1, the rest of patterns have pattern fill = 0 
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        xData.Add(xDataItem.ApplicationRegistry, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value, this.file,
                                                                         "The extended data of an entity must start with the application registry code " + this.fileLine);

                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            Hatch hatch = new Hatch(pattern, paths)
                              {
                                  Handle = handle,
                                  Layer = layer,
                                  Color = color,
                                  LineType = lineType,
                                  Elevation = elevation,
                                  Normal = normal,
                                  XData = xData
                              };

            return hatch;
        }

        private List<HatchBoundaryPath> ReadHatchBoundaryPaths(int numPaths)
        {
            List<HatchBoundaryPath> paths = new List<HatchBoundaryPath>();

            dxfPairInfo = this.ReadCodePair();

            while (paths.Count < numPaths)
            {
                switch (dxfPairInfo.Code)
                {
                    case 92:
                        BoundaryPathTypeFlag pathTypeFlag = (BoundaryPathTypeFlag) int.Parse(dxfPairInfo.Value);
                        if ((pathTypeFlag & BoundaryPathTypeFlag.Polyline) == BoundaryPathTypeFlag.Polyline)
                            paths.Add(ReadPolylineBoundaryPath());
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 93:
                        int numEdges = int.Parse(dxfPairInfo.Value);
                        paths.Add(ReadEdgeBoundaryPath(numEdges));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    default:
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }
            return paths;
        }

        private HatchBoundaryPath ReadPolylineBoundaryPath()
        {
            bool read = false;
            bool hasBulge = false;
            bool isClosed = false;
            List<LwPolylineVertex> vertexes = new List<LwPolylineVertex>();
            dxfPairInfo = this.ReadCodePair();

            while (!read)
            {
                switch (dxfPairInfo.Code)
                {
                    case 72:
                        hasBulge = int.Parse(dxfPairInfo.Value) != 0;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 73:
                        // is polyline closed
                        isClosed = int.Parse(dxfPairInfo.Value) == 1;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 93:
                        int numVertexes = int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        for (int i = 0; i < numVertexes; i++)
                        {
                            double x = 0.0;
                            double y = 0.0;
                            double bulge = 0.0;
                            if (dxfPairInfo.Code == 10) x = double.Parse(dxfPairInfo.Value);
                            dxfPairInfo = this.ReadCodePair();
                            if (dxfPairInfo.Code == 20) y = double.Parse(dxfPairInfo.Value);
                            dxfPairInfo = this.ReadCodePair();
                            if (hasBulge)
                            {
                                if (dxfPairInfo.Code == 42) bulge = double.Parse(dxfPairInfo.Value);
                                dxfPairInfo = this.ReadCodePair();
                            }
                            vertexes.Add(new LwPolylineVertex(x, y, bulge));
                        }
                        break;
                    default:
                        // the way the information is written is quite strict, a none recognize code and the polyline reading is over.
                        read = true;
                        break;
                }
            }

            LwPolyline polyline = new LwPolyline(vertexes, isClosed);
            List<IEntityObject> entities = isClosed ? new List<IEntityObject> {polyline} : polyline.Explode();
            return new HatchBoundaryPath(entities);
        }

        private HatchBoundaryPath ReadEdgeBoundaryPath(int numEdges)
        {
            List<IEntityObject> entities = new List<IEntityObject>();
            dxfPairInfo = this.ReadCodePair();

            while (entities.Count < numEdges)
            {
                switch (int.Parse(dxfPairInfo.Value))
                {
                    case 1:
                        dxfPairInfo = this.ReadCodePair();
                        // line
                        double lX1 = 0.0, lX2 = 0.0, lY1 = 0.0, lY2 = 0.0;
                        if (dxfPairInfo.Code == 10) lX1 = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        if (dxfPairInfo.Code == 20) lY1 = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        if (dxfPairInfo.Code == 11) lX2 = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        if (dxfPairInfo.Code == 21) lY2 = double.Parse(dxfPairInfo.Value);

                        entities.Add(new Line(new Vector3(lX1, lY1, 0.0), new Vector3(lX2, lY2, 0.0)));

                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 2:
                        dxfPairInfo = this.ReadCodePair();
                        // circular arc
                        double aX = 0.0, aY = 0.0, aR = 0.0, aStart = 0.0, aEnd = 0.0;
                        bool aCCW = true;
                        if (dxfPairInfo.Code == 10) aX = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        if (dxfPairInfo.Code == 20) aY = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        if (dxfPairInfo.Code == 40) aR = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        if (dxfPairInfo.Code == 50) aStart = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        if (dxfPairInfo.Code == 51) aEnd = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        if (dxfPairInfo.Code == 73) aCCW = int.Parse(dxfPairInfo.Value) != 0;

                        // a full circle will never happen AutoCAD exports circle boundary paths as two vertex polylines with bulges of 1 and -1
                        entities.Add(aCCW
                                         ? new Arc(new Vector3(aX, aY, 0.0), aR, aStart, aEnd)
                                         : new Arc(new Vector3(aX, aY, 0.0), aR, 360 - aEnd, 360 - aStart));

                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 3:
                        dxfPairInfo = this.ReadCodePair();
                        // elliptic arc
                        double eX = 0.0, eY = 0.0, eAxisX = 0.0, eAxisY = 0.0, eAxisRatio = 0.0, eStart = 0.0, eEnd = 0.0;
                        bool eCCW = true;
                        if (dxfPairInfo.Code == 10) eX = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        if (dxfPairInfo.Code == 20) eY = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        if (dxfPairInfo.Code == 11) eAxisX = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        if (dxfPairInfo.Code == 21) eAxisY = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        if (dxfPairInfo.Code == 40) eAxisRatio = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        if (dxfPairInfo.Code == 50) eStart = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        if (dxfPairInfo.Code == 51) eEnd = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        if (dxfPairInfo.Code == 73) eCCW = int.Parse(dxfPairInfo.Value) != 0;

                        Vector3 center = new Vector3(eX, eY, 0.0);
                        Vector3 axisPoint = new Vector3(eAxisX, eAxisY, 0.0);
                        Vector3 ocsAxisPoint = MathHelper.Transform(axisPoint,
                                                                    Vector3.UnitZ,
                                                                    MathHelper.CoordinateSystem.World,
                                                                    MathHelper.CoordinateSystem.Object);
                        double rotation = Vector2.Angle(new Vector2(ocsAxisPoint.X, ocsAxisPoint.Y));
                        double majorAxis = 2*axisPoint.Modulus();
                        Ellipse ellipse = new Ellipse
                                              {
                                                  MajorAxis = majorAxis,
                                                  MinorAxis = majorAxis*eAxisRatio,
                                                  Rotation = rotation*MathHelper.RadToDeg,
                                                  Center = center,
                                                  StartAngle = eCCW ? eStart : 360 - eEnd,
                                                  EndAngle = eCCW ? eEnd : 360 - eStart,
                                                  Normal = Vector3.UnitZ
                                              };

                        dxfPairInfo = this.ReadCodePair();
                        entities.Add(ellipse);
                        break;
                    case 4:
                        // spline not implemented
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            return new HatchBoundaryPath(entities);
        }

        private HatchPattern ReadHatchPattern(string name)
        {
            HatchPattern hatch = new HatchPattern(name);
            double angle = 0.0;
            double scale = 1.0;
            bool read = false;
            HatchType type = HatchType.UserDefined;
            HatchStyle style = HatchStyle.Normal;

            while (!read)
            {
                switch (dxfPairInfo.Code)
                {
                    case 52:
                        angle = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 41:
                        scale = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 75:
                        style = (HatchStyle) int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 76:
                        type = (HatchType) int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 77:
                        // hatch pattern double flag (not used)
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 78:
                        // number of pattern definition lines
                        int numLines = int.Parse(dxfPairInfo.Value);
                        hatch.LineDefinitions = ReadHatchPatternDefinitionLine(scale, angle, numLines);
                        read = true;
                        break;
                    default:
                        // the way the information is written is quite strict, a none recognized code and the hatch reading is over
                        // there are more codes associated with a hatch pattern but only these ones are recognized
                        read = true;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            hatch.Style = style;
            hatch.Scale = scale;
            hatch.Angle = angle;
            hatch.Type = type;
            return hatch;
        }

        private List<HatchPatternLineDefinition> ReadHatchPatternDefinitionLine(double hatchScale, double hatchAngle, int numLines)
        {
            List<HatchPatternLineDefinition> lineDefinitions = new List<HatchPatternLineDefinition>();

            dxfPairInfo = this.ReadCodePair();
            for (int i = 0; i < numLines; i++)
            {
                double angle = 0.0;
                Vector2 origin = Vector2.Zero;
                Vector2 offset = Vector2.Zero;
                List<double> dashPattern = new List<double>();


                if (dxfPairInfo.Code == 53) angle = double.Parse(dxfPairInfo.Value);
                dxfPairInfo = this.ReadCodePair();

                if (dxfPairInfo.Code == 43) origin.X = double.Parse(dxfPairInfo.Value);
                dxfPairInfo = this.ReadCodePair();

                if (dxfPairInfo.Code == 44) origin.Y = double.Parse(dxfPairInfo.Value);
                dxfPairInfo = this.ReadCodePair();

                if (dxfPairInfo.Code == 45) offset.X = double.Parse(dxfPairInfo.Value);
                dxfPairInfo = this.ReadCodePair();

                if (dxfPairInfo.Code == 46) offset.Y = double.Parse(dxfPairInfo.Value);
                dxfPairInfo = this.ReadCodePair();

                if (dxfPairInfo.Code == 79)
                {
                    int numSegments = int.Parse(dxfPairInfo.Value);
                    dashPattern = ReadHatchLineSegments(hatchScale, numSegments);
                }

                // Pattern fill data. In theory this should hold the same information as the pat file but for unkown reason the dxf requires global data instead of local.
                // what it means is that we need to apply the scale and rotation of the hatch to the pattern definiton lines, it's a guess the documentation is kind of obscure.
                // What seems to work is to read the data in global coordinates and then convert it in local, this means we have to apply the pattern rotation and scale to the line definitions
                double lineAngle = angle - hatchAngle;
                double sin = Math.Sin(hatchAngle*MathHelper.DegToRad);
                double cos = Math.Cos(hatchAngle*MathHelper.DegToRad);
                Vector2 delta = new Vector2(cos*offset.X/hatchScale + sin*offset.Y/hatchScale, -sin*offset.X/hatchScale + cos*offset.Y/hatchScale);

                lineDefinitions.Add(new HatchPatternLineDefinition
                                        {
                                            Angle = lineAngle,
                                            Origin = origin,
                                            Delta = delta,
                                            DashPattern = dashPattern
                                        });
            }

            return lineDefinitions;
        }

        private List<double> ReadHatchLineSegments(double scale, int numSegments)
        {
            List<double> dashPattern = new List<double>();

            dxfPairInfo = this.ReadCodePair();
            for (int i = 0; i < numSegments; i++)
            {
                // Positive values means solid segments and negative values means spaces (one entry per element)
                if (dxfPairInfo.Code == 49)
                    dashPattern.Add(double.Parse(dxfPairInfo.Value)/scale);

                dxfPairInfo = this.ReadCodePair();
            }
            return dashPattern;
        }

        private Vertex ReadVertex()
        {
            string handle = string.Empty;
            Layer layer = Layer.Default;
            AciColor color = AciColor.ByLayer;
            LineType lineType = LineType.ByLayer;
            Vector3 location = new Vector3();
            Dictionary<ApplicationRegistry, XData> xData = new Dictionary<ApplicationRegistry, XData>();
            double endThickness = 0.0;
            double beginThickness = 0.0;
            double bulge = 0.0;
            List<int> vertexIndexes = new List<int>();
            VertexTypeFlags flags = VertexTypeFlags.PolylineVertex;

            dxfPairInfo = this.ReadCodePair();

            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 5:
                        handle = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 8:
                        layer = this.GetLayer(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 62:
                        color = new AciColor(short.Parse(dxfPairInfo.Value));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 6:
                        lineType = this.GetLineType(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 10:
                        location.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        location.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        location.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 40:
                        beginThickness = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 41:
                        endThickness = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 42:
                        bulge = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 70:
                        flags = (VertexTypeFlags) int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 71:
                        vertexIndexes.Add(int.Parse(dxfPairInfo.Value));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 72:
                        vertexIndexes.Add(int.Parse(dxfPairInfo.Value));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 73:
                        vertexIndexes.Add(int.Parse(dxfPairInfo.Value));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 74:
                        vertexIndexes.Add(int.Parse(dxfPairInfo.Value));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        xData.Add(xDataItem.ApplicationRegistry, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value, this.file,
                                                                         "The extended data of an entity must start with the application registry code "
                                                                         + this.fileLine);

                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            return new Vertex
                       {
                           Flags = flags,
                           Location = location,
                           BeginThickness = beginThickness,
                           Bulge = bulge,
                           Color = color,
                           EndThickness = endThickness,
                           Layer = layer,
                           LineType = lineType,
                           VertexIndexes = vertexIndexes.ToArray(),
                           XData = xData,
                           Handle = handle
                       };
        }

        private void ReadUnknowEntity()
        {
            // if the entity is unknown keep reading until an end of section or a new entity is found
            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 0)
            {
                dxfPairInfo = this.ReadCodePair();
            }
        }

        #endregion

        #region object methods

        private DictionaryObject ReadDictionary()
        {
            string handle = null;
            string handleOwner = null;
            ClonningFlag clonning = ClonningFlag.KeepExisting;
            bool isHardOwner = false;
            int numEntries = 0;
            List<string> names = new List<string>();
            List<string> handlesToOwner = new List<string>();

            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 5:
                        handle = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 330:
                        handleOwner = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 280:
                        isHardOwner = int.Parse(dxfPairInfo.Value) != 0;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 281:
                        clonning = (ClonningFlag) int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 3:
                        numEntries += 1;
                        names.Add(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 350:
                        handlesToOwner.Add(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    default:
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            DictionaryObject dictionary = new DictionaryObject(handleOwner)
                                              {
                                                  Handle = handle,
                                                  IsHardOwner = isHardOwner,
                                                  Clonning = clonning
                                              };
            for (int i = 0; i < numEntries; i++)
            {
                string id = null;
                if (i < handlesToOwner.Count)
                    id = handlesToOwner[i];
                dictionary.Entries.Add(new DictionaryObjectEntry(names[i], id));
            }

            return dictionary;
        }

        private RasterVariables ReadRasterVariables()
        {
            RasterVariables variables = new RasterVariables();
            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 5:
                        variables.Handle = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 70:
                        variables.DisplayFrame = int.Parse(dxfPairInfo.Value) != 0;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 71:
                        variables.DisplayQuality = (ImageDisplayQuality) int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 72:
                        variables.Units = (ImageUnits) int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    default:
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            return variables;
        }

        private ImageDef ReadImageDefinition()
        {
            string handle = null;
            string ownerHandle = null;
            string fileName = null;
            string name = null;
            double width = 0, height = 0;
            float wPixel = 0.0f;
            float hPixel = 0.0f;
            ResolutionUnits units = ResolutionUnits.NoUnits;

            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 5:
                        handle = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1:
                        fileName = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 10:
                        width = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        height = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 11:
                        wPixel = float.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 21:
                        hPixel = float.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 281:
                        units = (ResolutionUnits) int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 330:
                        ownerHandle = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    default:
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            if (ownerHandle != null)
            {
                DictionaryObject imageDefDict = this.dictionaries[ownerHandle];
                foreach (DictionaryObjectEntry entry in imageDefDict.Entries)
                {
                    if (entry.HandleToOwner == handle)
                        name = entry.Name;
                }
            }

            ImageDef imageDef = new ImageDef(fileName, (int)width, 25.4f/wPixel, (int)height, 25.4f/hPixel, name, units)
                                    {
                                        Handle = handle
                                    };

            this.imgDefHandles.Add(imageDef.Handle, imageDef);
            return imageDef;
        }

        private ImageDefReactor ReadImageDefReactor()
        {
            string handle = null;
            string imgOwner = null;

            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 5:
                        handle = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 330:
                        imgOwner = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                   default:
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            return new ImageDefReactor(imgOwner) {Handle = handle};
        }

        #endregion

        #region private methods

        private void CheckDimBlockName(string name)
        {
            // the autocad block names has the form *D#
            // we need to find which is the last available number, in case more dimensions are added
            int num;
            string token = name.Remove(0, 2);
            if (!int.TryParse(token, out num)) return;
            if (num > this.dimCount)
                this.dimCount = num;
        }

        private static TextAlignment ObtainAlignment(int horizontal, int vertical)
        {
            TextAlignment alignment = TextAlignment.BaselineLeft;

            if (horizontal == 0 && vertical == 3)
                alignment = TextAlignment.TopLeft;

            else if (horizontal == 1 && vertical == 3)
                alignment = TextAlignment.TopCenter;

            else if (horizontal == 2 && vertical == 3)
                alignment = TextAlignment.TopRight;

            else if (horizontal == 0 && vertical == 2)
                alignment = TextAlignment.MiddleLeft;

            else if (horizontal == 1 && vertical == 2)
                alignment = TextAlignment.MiddleCenter;

            else if (horizontal == 2 && vertical == 2)
                alignment = TextAlignment.MiddleRight;

            else if (horizontal == 0 && vertical == 1)
                alignment = TextAlignment.BottomLeft;

            else if (horizontal == 1 && vertical == 1)
                alignment = TextAlignment.BottomCenter;

            else if (horizontal == 2 && vertical == 1)
                alignment = TextAlignment.BottomRight;

            else if (horizontal == 0 && vertical == 0)
                alignment = TextAlignment.BaselineLeft;

            if (horizontal == 1 && vertical == 0)
                alignment = TextAlignment.BaselineCenter;

            else if (horizontal == 2 && vertical == 0)
                alignment = TextAlignment.BaselineRight;

            return alignment;
        }

        private Block GetBlock(string name, bool isNestedBlock)
        {
            if (this.blocks.ContainsKey(name)) 
                return this.blocks[name];
            if(isNestedBlock) return null;

            throw new DxfTableException(StringCode.BlockRecordTable, this.file, "The block with name " + name + " does not exist.");
        }

        private Layer GetLayer(string name)
        {
            if (this.layers.ContainsKey(name))
                return this.layers[name];
            throw new DxfTableException(StringCode.LayerTable, this.file, "The layer with name " + name + " does not exist.");
        }

        private LineType GetLineType(string name)
        {
            if (this.lineTypes.ContainsKey(name))
                return this.lineTypes[name];
            throw new DxfTableException(StringCode.LineTypeTable, this.file, "The line type with name " + name + " does not exist.");
        }

        private TextStyle GetTextStyle(string name)
        {
            if (this.textStyles.ContainsKey(name))
                return this.textStyles[name];
            throw new DxfTableException(StringCode.TextStyleTable, this.file, "The text style with name " + name + " does not exist.");
        }

        private TextStyle GetTextStyleByHandle(string handle)
        {
            foreach (TextStyle style in this.textStyles.Values)
            {
                if (style.Handle == handle)
                    return style;
            }
            throw new DxfTableException(StringCode.TextStyleTable, this.file, "The text style with handle " + handle + " does not exist.");
        }

        private DimensionStyle GetDimensionStyle(string name)
        {
            if (this.dimStyles.ContainsKey(name))
                return this.dimStyles[name];
            throw new DxfTableException(StringCode.DimensionStyleTable, this.file, "The dimension style with name " + name + " does not exist.");
        }

        private CodeValuePair ReadCodePair()
        {
            int intCode;
            string readCode = this.reader.ReadLine();
            this.fileLine += 1;
            if (!int.TryParse(readCode, out intCode))
            {
                throw (new DxfException("Invalid group code " + readCode + " in line " + this.fileLine));
            }
            string value = this.reader.ReadLine();
            this.fileLine += 1;
            return new CodeValuePair(intCode, value);
        }

        private XData ReadXDataRecord(string appId)
        {
            XData xData = new XData(this.appIds[appId]);
            dxfPairInfo = this.ReadCodePair();

            while (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
            {
                if (dxfPairInfo.Code == XDataCode.AppReg)
                    break;

                XDataRecord xDataRecord = new XDataRecord(dxfPairInfo.Code, dxfPairInfo.Value);
                xData.XDataRecord.Add(xDataRecord);
                dxfPairInfo = this.ReadCodePair();
            }

            return xData;
        }

        #endregion
    }
}